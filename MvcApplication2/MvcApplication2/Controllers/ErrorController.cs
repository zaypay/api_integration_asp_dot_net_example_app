using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace MvcApplication2.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult NotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            System.Diagnostics.Debug.WriteLine("exception is  oooppp : " + Server.GetLastError());
            return View();
        }
        
        public ActionResult ServerError()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }

        public ActionResult ThrowError()
        {
            throw new NotImplementedException("Pew ^ Pew");
        }

    }
}
