using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ICSI_UDIN.Models;

namespace ICSI_UDIN.Controllers
{
    public class UDINController : Controller
    {
        // GET: UDIN

        public ActionResult Index()
        {

            return View();
        }
        
        [HttpPost]
        public ActionResult GenerateAlphaNumericOTP()
        {
            string numbers = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random objrandom = new Random();
            string passwordString = "";
            string strrandom = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                int temp = objrandom.Next(0, numbers.Length);
                passwordString = numbers.ToCharArray()[temp].ToString();
                strrandom += passwordString;
            }
            ViewBag.anotp = strrandom;
            return View("Index");
        }

        public ViewResult Login()
        {

            return View();

        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        // GET: UDIN

        public ActionResult UDINVerification()
        {

            return View();
        }

    }

    }
