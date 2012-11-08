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
using System.IO;
using System.Collections.Specialized;
using System.Xml;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace MvcApplication2.Controllers
{ 
    public class ProductsController : Controller
    {
        private ZaypayDBContext db = new ZaypayDBContext();
        LogEntry logEntry = new LogEntry();            
        
        //
        // GET: /Products/

        public ViewResult Index()
        {
            return View(db.Products.ToList());
        }
        
        public ActionResult Reporting()
        {
                        
            NameValueCollection parameters = Request.QueryString;
            
            //check if all required params are there
            if (AllValuesPresent(parameters))
            {
                
                int priceSettingId = Convert.ToInt32(parameters["price_setting_id"]);
                int paymentId = Convert.ToInt32(parameters["payment_id"]);
                int purchaseId = Convert.ToInt32(parameters["purchase_id"]);
                
                Purchase purchase  = db.Purchases.Find(purchaseId);
                if (purchase != null && (purchase.ZaypayPaymentId == paymentId))
                {
                
                    Product product = purchase.Product;

                    if (product.PriceSettingId == priceSettingId)
                    {
                
                        // get the key from the xml file 
                        PriceSetting ps = new PriceSetting(product.PriceSettingId);

                        PaymentResponse response = ps.ShowPayment(purchase.ZaypayPaymentId);

                        string status = response.Status();

                        if (status == parameters["status"])
                        {
                            purchase.NeedPolling = SetNeedPollingValue(ref response);
                            purchase.Status = status;
                            db.SaveChanges();
                        }
                    }

                }
                else
                {
                    logEntry.Message = "Reporting Method in Products Controller ----  Values are missing --- the request had following query string :: " + parameters;
                    Logger.Write(logEntry);                    
                    System.Diagnostics.Debug.WriteLine("zayapy payment id is MiSSING");
                }

            }
            
            return Content("*ok*");

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

        private bool PayPerMinute(ref PaymentResponse payment)
        {            
            return (payment.Platform().ToLower() == "phone" && payment.SubPlatform().ToLower() == "pay per minute");
        }

        private bool SmsWithVerification(ref PaymentResponse payment)
        {
            return (payment.Platform().ToLower() == "sms" && payment.VerificationNeeded() == true);
        }

        private bool UnknownPlatform(ref PaymentResponse payment)
        {
            string platform = payment.Platform().ToLower();
            return (platform != "sms" && platform != "phone");
                
        }

        private int SetNeedPollingValue(ref PaymentResponse payment)
        {

            string status = payment.Status();
            bool verNeeded = payment.VerificationNeeded();
            string platform = payment.Platform();


            if (status == "in_progress")
            {
                // if PayperMinute, or sms with verificatio, or unknown payment
                if (PayPerMinute(ref payment) || SmsWithVerification(ref payment) || UnknownPlatform(ref payment))
                {                  
                    return 1;
                }
                else
                {                    
                    return 0;
                }
                
            }
            else if (status == "paused")
                return 1;
            else
                return 0;
            

        }        

        private bool AllValuesPresent(NameValueCollection collection)
        {
            if ((collection["status"] != null) && (collection["price_setting_id"] != null) && (collection["payment_id"] != null) && (collection["purchase_id"] != null))
                return true;
            else
                return false;

        }
    }
}