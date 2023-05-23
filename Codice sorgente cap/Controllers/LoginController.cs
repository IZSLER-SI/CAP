using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Models;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/

        public ActionResult Login()
        {
            
            string userID = Session["UserID"].ToString();
            LoginModel lm = getLoginModel(int.Parse(userID));
            IEnumerable<SelectListItem> items = lm.ListProfili().Select(c => new SelectListItem
            {
                Text = c.ProfiloDesc,
                Value = c.ProfiloID.ToString()
            });

            ViewBag.ddlListaProfili = items;
            if (items.Count() == 1)
            {
                string val = items.ElementAt(0).Value;
                //string val = Request["ddlListaProfili"].ToString();
                Session[SessionVar.Profile.ToString()] = val;
                Profili i = lm.ListProfili().Where(l => l.ProfiloID == int.Parse(val)).Distinct().SingleOrDefault();
                Session[SessionVar.ProfileDesc.ToString()] = i.ProfiloDesc;
                Session[SessionVar.LOGINOK.ToString()] = true;
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        private LoginModel getLoginModel(int idUser)
        {
            return new LoginModel(idUser);
        }

        private void loadDDLListaProfili()
        {
            
        }
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            //model.ListProfili  = GetCardTypes("MS");

            if (ModelState.IsValid)
            {
                string val = Request["ddlListaProfili"].ToString();
                Session[SessionVar.Profile.ToString()] = val;
                string userID = Session[SessionVar.UserID.ToString()].ToString();
                LoginModel lm = getLoginModel(int.Parse(userID));
                Profili i= lm.ListProfili().Where(l => l.ProfiloID == int.Parse(val)).Distinct().SingleOrDefault();
                Session[SessionVar.ProfileDesc.ToString()] = i.ProfiloDesc;
                Session[SessionVar.LOGINOK.ToString()] = true;
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
        public ViewResult RichiediUtenza()
        { return View(); }
    }
}
