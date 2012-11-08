using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication2.Models;
using System.Collections;
using Zaypay.WebService;
using Zaypay;
using System.Collections.Specialized;
using System.Web.Routing;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace MvcApplication2.Controllers
{
    
    public class PurchasesController : Controller
    {
        private ZaypayDBContext db = new ZaypayDBContext();        
        LogEntry logEntry = new LogEntry();            
        
        public ActionResult SubmitVerificationCode()
        {           
            
            int id = 0;            
            Int32.TryParse(Request.Form["purchaseId"], out id);
            
            int code = 0;
            Int32.TryParse(Request.Form["code"], out code);            

            Purchase purchase = db.Purchases.Find(id);
            
            if (purchase != null && purchase.SessionId == HttpContext.Session.SessionID)
            {
                if (code != 0)
                {
                    try
                    {
                      
                        PriceSetting ps = new PriceSetting(purchase.Product.PriceSettingId);
                        PaymentResponse payment = ps.VerificationCode(purchase.ZaypayPaymentId, Request.Form["code"]);
                        
                        purchase.Status = payment.Status();

                        SetViewData(ref payment);                        
                        return PartialView("_PaymentScreen", purchase);

                    }
                    catch (Exception e)
                    {
                        LogEntry(e.Message);
                        return Json(new { success = false, redirect = false, message = "Oops!! Some error occured" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { success = false, redirect = false, message = "Code cannot be empty and it cannot contian string literals" }, JsonRequestBehavior.AllowGet);
                }
                
            }
            else
            {
                return Json(new { success = false, redirect = true, message = "You are not authorized. Redirecting ...." }, JsonRequestBehavior.AllowGet);
            }
            
        }
        
        public ViewResult Details(int id = 0)
        {
            
            //System.Diagnostics.Debug.WriteLine("IN DETAIS WITH ID------------------: " + id);
            
            Purchase purchase = db.Purchases.Find(id);

            if (purchase != null && purchase.SessionId == HttpContext.Session.SessionID)
            {
                
                PriceSetting ps = new PriceSetting(purchase.Product.PriceSettingId);
               
                PaymentResponse payment = ps.ShowPayment(purchase.ZaypayPaymentId);


                string status = payment.Status();
                int paymentMethodId = payment.PaymentMethodId();
                
                Hashtable instructions = payment.Instructions();
                
                ViewData.Add("instructions", instructions["long-instructions"]);

                ViewData.Add("status", status);
                ViewData.Add("payment_method_id", paymentMethodId);

                ViewData.Add("verification_needed", payment.VerificationNeeded());
                ViewData.Add("verification_tries_left", payment.VerificationTriesLeft());
                ViewData.Add("payment_id", purchase.ZaypayPaymentId);

                return View(purchase);
            }
            else
            {                
                throw new HttpException(404, "Sorry, Resource not available");
            }            
            
        }

        [HttpPost]
        public ActionResult GetStatus()
        {
            
            int id = 0;
            Int32.TryParse(Request.Form["purchaseId"], out id);

            Purchase purchase = db.Purchases.Find(id);

            if (PurchaseIsCorrect(ref purchase))
            {

                if (purchase.Status == "prepared" || (purchase.Status == "in_progress" && purchase.NeedPolling == 0))
                {                    
                    return Json(new { status = purchase.Status }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (purchase.NeedPolling == 1)
                    {                        
                        PriceSetting ps = new PriceSetting(purchase.Product.PriceSettingId);

                        PaymentResponse payment = ps.ShowPayment(purchase.ZaypayPaymentId);

                        ViewData.Add("instructions", ((Hashtable)payment.Instructions())["long-instructions"]);
                        ViewData.Add("verification_needed", payment.VerificationNeeded());
                        ViewData.Add("verification_tries_left", payment.VerificationTriesLeft());
                        return PartialView("_PaymentScreen", purchase);

                    }                    
                    return PartialView("_PaymentScreen", purchase);
                    
                }
                
            }
            else
            {                
                return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);                
            }
            
        }
        
        public ActionResult GetPaymentMethods()
        {
            int id = 0;
            Int32.TryParse(Request.Form["productId"], out id);

            Product product = db.Products.First(m => m.ID == (id));

            if (product != null)
            {
                try
                {
                    PriceSetting ps = new PriceSetting(product.PriceSettingId);
                    string locale = Request.Form["language"] + "-" + Request.Form["country"];
                    ps.LOCALE = locale;
                    PaymentMethodResponse paymentObject = ps.ListPaymentMethods();

                    List<Hashtable> paymentMethods = paymentObject.PaymentMethods();
                    
                    return PartialView("_PaymentMethodButtons", paymentMethods);
    
                }
                catch (Exception ex)
                {
                    LogEntry(ex.Message);
                    return Json(new { success = false, message = "Some error occured while retrieving the payment methods , try again !!" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, message = "You are not authorized to make this call" }, JsonRequestBehavior.AllowGet);
            }
            
        }

       
        // GET: /Purchases/Create
        // GET: /Purchase/Create?id=?
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Create(int productId = 0)
        {

            Product product = db.Products.Find(productId);

            if (product != null)
            {                
                try
                {

                    //Exception ex2 = new Exception("this is the first exception");
                    //throw ex2;
                   

                    PriceSetting ps = new PriceSetting(product.PriceSettingId);
                    
                    string userIp = GetUserIP();             
                    string locale = "";
                    string[] locales;
                    
                    if (!String.IsNullOrWhiteSpace(userIp))
                    {                        
                        LocalForIPResponse localeResponse = ps.LocaleForIP(userIp);
                        locale = localeResponse.Locale();
                        
                        ps.LOCALE = locale;
                        locales = locale.Split('-');
                        
                        try
                        {                            
                            List<Hashtable> paymentMethods = ps.ListPaymentMethods().PaymentMethods();                            
                            ViewData.Add("paymentMethods", paymentMethods);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);                            
                        }

                    }
                    else
                    {
                        ViewData["ipIsNull"] = true;
                        locales = "en-".Split('-');
                    }

                    ListLocalesResponse localesList = ps.ListLocales();

                    List<SelectListItem> countriesList = null;
                    List<SelectListItem> languagesList = null;

                    GetSelectList(ref localesList, ref countriesList, true);
                    GetSelectList(ref localesList, ref languagesList, false);

                    ViewData.Add("countries", new SelectList(countriesList, "Value", "Text", locales[1]));
                    ViewData.Add("languages", new SelectList(languagesList, "Value", "Text", locales[0]));                    
                    
                    if (TempData["error"] != null)                    
                        ViewData["error"] = TempData["error"];
                    
                    return View(product);
                }
                catch (Exception e)
                {

                    
                    string mesg = GetExceptionMessage(e);
                    
                    if (mesg == "")
                        throw e;
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("IN DETAIS WITH ID------------------: ------------------------------------------");
                        LogEntry(e.Message);
                        ViewData["error"] = mesg;
                        return View("../products/index", db.Products.ToList());
                    }
                }
            }
            else
            {
                throw new HttpException(404, "Sorry, Resource not available");
            }            
            
        }

        //
        // POST: /Purchases/Create

        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Create(FormCollection form)
        {

            int productId = 0;
            int paymentMethodId = 0;

            Int32.TryParse(form["productId"], out productId);

            Product product = db.Products.Find(productId);

            if (product != null)
            {
                var url = Url.Action("Create", "Purchases");
                
                if (Int32.TryParse(form["paymentMethod"], out paymentMethodId) && form["languagesList"] != null && form["countriesList"] != null)
                {
                    Purchase purchase = null;

                    try
                    {
                        
                        PriceSetting ps = new PriceSetting(product.PriceSettingId);

                        string locale = form["languagesList"] + "-" + form["countriesList"];
                        ps.LOCALE = locale;
                        ps.PAYMENT_METHOD_ID = paymentMethodId;

                        purchase = CreatePurchase(product, HttpContext.Session.SessionID);

                        NameValueCollection options = new NameValueCollection();
                        options.Add("purchase_id", purchase.ID.ToString());

                        PaymentResponse payment = ps.CreatePayment(options);

                        purchase.Update(payment);
                        db.SaveChanges();

                        SetCreateViewData(ref payment, ref purchase, paymentMethodId);
                        
                        return View("Details", purchase);
                    }
                    catch (Exception e)
                    {
                        string mesg = GetExceptionMessage(e);

                        if (purchase != null)
                            RemovePurchase(ref purchase);
                        
                        if (mesg == "")
                            throw e;
                        else
                        {
                            LogEntry(e.Message);
                            TempData["error"] = mesg;
                            return RedirectToAction("Create", new { productId = product.ID });                            
                        }
                    }
                    
                }
                else
                {                    
                    TempData["error"] = "Language , Country or Payment Method is not selected properly";
                    return RedirectToAction("Create", new { productId = product.ID });                 
                }
            }
            else
            {
                throw new HttpException(404, "Sorry, Resource Not Found");
            }
            
            
            
        }
        
        // ========================================================================================
        // PROTECTED METHODS
        // ========================================================================================

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        // ========================================================================================
        // PRIVATE METHODS
        // ========================================================================================

        private string GetExceptionMessage(Exception ex)
        {
            string mesg = "";

            if (ex is System.Net.WebException)
            {
                mesg = "Payment server error. Try again !";
            }
            else if (ex is XmlException)
            {
                mesg = "Payment server config error. Try again !";
            }
            else
            {
                if (ex.Data.Contains("type"))
                {
                    mesg = ex.Data["user_message"].ToString();
                }
            }

            return mesg;

        }

        private void RemovePurchase(ref Purchase purchase)
        {
            if (purchase.ZaypayPaymentId == 0)
            {                
                db.Purchases.Remove(purchase);
                db.SaveChanges();
            }
        }

        private void SetViewData(ref PaymentResponse payment)
        {
            Hashtable instructions = payment.Instructions();

            ViewData.Add("instructions", instructions["long-instructions"]);
            ViewData.Add("status", payment.Status());

            ViewData.Add("verification_needed", payment.VerificationNeeded());
            ViewData.Add("verification_tries_left", payment.VerificationTriesLeft());

        }        

        private Purchase CreatePurchase(Product product, string sessionId)
        {
            Purchase purchase = new Purchase(product, HttpContext.Session.SessionID);
            db.Purchases.Add(purchase);
            db.SaveChanges();
            return purchase;
        }

        private void SetCreateViewData(ref PaymentResponse payment, ref Purchase purchase, int paymentMethodId)
        {

            Hashtable instructions = payment.Instructions();
            ViewData.Add("instructions", instructions["long-instructions"]);

            //ViewData.Add("status", payment.Status());
            ViewData.Add("verification_needed", payment.VerificationNeeded());
            ViewData.Add("verification_tries_left", payment.VerificationTriesLeft());
            //ViewData.Add("payment_id", purchase.ZaypayPaymentId);
            //ViewData.Add("paymentMethodChoosen", paymentMethodId);

        }
        
        private bool PurchaseIsCorrect(ref Purchase purchase)
        {
            return (purchase != null && purchase.SessionId == HttpContext.Session.SessionID);
        }

        private string GetUserIP()
        {
            string pattern = @"^((([0-9]{1,2})|(1[0-9]{2,2})|(2[0-4][0-9])|(25[0-5])|\*)\.){3}(([0-9]{1,2})|(1[0-9]{2,2})|(2[0-4][0-9])|(25[0-5])|\*)$";
            Regex check = new Regex(pattern);
            string local = "127.0.0.1";

            string ipList = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipList) && check.IsMatch(ipList.Split(',')[0], 0))
            { 
                return ipList.Split(',')[0];
            }

            if (check.IsMatch(Request.ServerVariables["REMOTE_ADDR"], 0))
            {                
                return Request.ServerVariables["REMOTE_ADDR"];
            }

            return local;
            
        }

        private void GetSelectList(ref ListLocalesResponse localesList, ref List<SelectListItem> list, bool countries)
        {
            List<Hashtable> hash;
            string text = "";


            if (countries)
            {
                hash = (List<Hashtable>)localesList.Countries();
                text = "name";
            }
            else
            {
                hash = (List<Hashtable>)localesList.Languages();
                text = "english-name";
            }

            list = GetSelectList(ref hash, text, "code");
            list = list.OrderBy(x => x.Text).ToList();

        }
        
        private List<SelectListItem> GetSelectList(ref List<Hashtable> hash, string text, string value)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            foreach (Hashtable h in (List<Hashtable>)hash)
            {

                items.Add(new SelectListItem
                {
                    Text = (String)h[text],
                    Value = (String)h[value]

                });

            }
            return items;

        }

        private void LogEntry(string mesg)
        {
            logEntry.Message = mesg;
            Logger.Write(logEntry);
        }
        
    }
}