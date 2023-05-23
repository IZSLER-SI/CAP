using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Models;
using IZSLER_CAP.Helpers;
using System.Net.Mail;
using System.Net;

namespace IZSLER_CAP.Controllers
{
    public class B16Controller : Controller
    {
        public B16Controller()
        {
            m_Session = Session;
            this.initViewBag();
        }
        public HttpSessionStateBase m_Session;

        public ActionResult SetSessionVariable(string key, string value)
        {
            Session[key] = value;
            m_Session = Session;
            return this.Json(new { success = true });
        }
        private void initViewBag()
        {
            ViewBag.Message = "";
            ViewBag.LiDashboard = "";
            ViewBag.LiAnalisi = "";
            ViewBag.LiProdotto = "";
            ViewBag.OpenCloseMenuDX = "";
            ViewBag.Litagbg = "";
            ViewBag.LiStatistiche = "";
            ViewBag.LiSettings = "";
            ViewBag.LiReports = "";
            ViewBag.LiIntermedi = "";
            ViewBag.LiModelli = "";
        }

        public string SetLiDashBoard { set { ViewBag.LiDashboard = value; } }
        public string SetLiProdotto { set { ViewBag.LiProdotto = value; } }
        public string SetLiAnalisi { set { ViewBag.LiAnalisi = value; } }
        public string SetLiReports { set { ViewBag.LiReports = value; } }
        public string SetLiStatistiche { set { ViewBag.LiStatistiche = value; } }
        public string SetLiSettings { set { ViewBag.LiSettings = value; } }
        public string SetLiIntermedi { set { ViewBag.LiIntermedi = value; } }
        public string SetLiModelli{ set { ViewBag.LiModelli = value; } }
        public string SetMessage { set { ViewBag.Message = value; } }
        public string SetOpenCloseMenuDX { set { ViewBag.OpenCloseMenuDX = value; } }
        public string SetLitagbg { set { ViewBag.Litagbg = value; } }


        protected int CurrentUserID { get { int id = 0; try { id = int.Parse(Session[SessionVar.UserID.ToString()].ToString()); } catch { } return id; } }
        protected int CurrentProfileID { get { int id = 0; try { id = int.Parse(Session[SessionVar.Profile.ToString()].ToString()); } catch { } return id; } }


        public EMAPAR_EMAIL_PARAMETRI GetParamEmail()
        {
            EMAPAR_EMAIL_PARAMETRI emailPar = null;
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            IQueryable<EMAPAR_EMAIL_PARAMETRI> lstEmail = context.EMAPAR_EMAIL_PARAMETRI.Where(ema => ema.EMAPAR_DEFAULT == true);
            if (lstEmail.Any()) emailPar = lstEmail.First();
            return emailPar;
        }

        public RedirectToRouteResult CheckLogin()
        {
            RedirectToRouteResult r =null;
            if (Session[SessionVar.LOGINOK.ToString()] != null)
            {
                if (bool.Parse(Session[SessionVar.LOGINOK.ToString()].ToString()) == false)
                {
                    r = RedirectToAction("RichiediUtenza", "Login");
                }
            }
            return r;
        }
        protected bool enableSendEmail()
        {
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            string value = en.T_SETTIN_SETTINGS.Where(z => z.T_SETTIN_CODICE == "ENABL_SEND").SingleOrDefault().T_SETTIN_VALUE;
            if (value == "1") return true;
            return false;
        }
        protected bool forceEmailServiceAccount()
        {
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                string value = en.T_SETTIN_SETTINGS.Where(z => z.T_SETTIN_CODICE == "FORCE_SEND").SingleOrDefault().T_SETTIN_VALUE;
                if (value == "1") return true;
            }
            catch{}
            return false;
        }
        protected MailAddress getAddressServiceAccount()
        {
            EMAPAR_EMAIL_PARAMETRI ep = GetParamEmail();
            MailAddress ma = new MailAddress(ep.EMAPAR_MAIL_FROM,ep.EMAPAR_MAIL_ALIAS);
            return ma;
        }
        private string getCurrentDate()
        {
            string ret = "";
            DateTime dt = DateTime.Now;
            ret = dt.Day.ToString().PadLeft(2, '0') + "/" +
                dt.Month.ToString().PadLeft(2, '0') + "/" +
                dt.Year.ToString().PadLeft(4, '0') + " " +
                dt.Hour.ToString().PadLeft(2, '0') + ":" +
                dt.Minute.ToString().PadLeft(2, '0') + ":" +
                dt.Second.ToString().PadLeft(2, '0') + "." +
                dt.Millisecond.ToString().PadLeft(3, '0');
            return ret;
        }
        protected void logEmail(Exception ex)
        {
            string a = getCurrentDate ()+ " - Inizio Eccezzione" + "\r\n"+ "[Eccezione] " + ex.Message;
            try { a += "\r\n" + "[Eccezione interna] " + ex.InnerException.Message; }
            catch { }
            try { a += "\r\n" + "[Utente] " + User.Identity.Name; }
            catch { }
            try { a += " [Autenticato] " + User.Identity.IsAuthenticated.ToString(); }
            catch { }
            a += getCurrentDate() + " - Fine Eccezzione";
            if (enableLogEmail())
            {
                string path = getPathLogEmail();
                try
                {
                    System.IO.File.AppendAllText(path, a); 
                }
                catch { }
            }
        }
        private bool enableLogEmail()
        {
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                string value = en.T_SETTIN_SETTINGS.Where(z => z.T_SETTIN_CODICE == "ENLOGEMAIL").SingleOrDefault().T_SETTIN_VALUE;
                if (value == "1") return true;
            }
            catch{}
            return false;
        }
        private string getPathLogEmail()
        {
            string value = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                value = en.T_SETTIN_SETTINGS.Where(z => z.T_SETTIN_CODICE == "PATHLOGEMA").SingleOrDefault().T_SETTIN_VALUE;
            }
            catch { }
            return value;
        }
        
        protected MailAddress getAddressByUserID(int userID)
        {
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            UTENTE ut = en.UTENTE.Where(z => z.UTENTE_ID == userID).SingleOrDefault();
            MailAddress ma = new MailAddress(ut.UTENTE_EMAIL, ut.UTENTE_NOME + " " + ut.UTENTE_COGNOME);
            return ma;
        }
        protected SmtpClient getSmtpClient()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                EMAPAR_EMAIL_PARAMETRI emapar = en.EMAPAR_EMAIL_PARAMETRI.Where(z => z.EMAPAR_DEFAULT == true).SingleOrDefault();
                string server = emapar.EMAPAR_SERVER;
                string user = emapar.EMAPAR_USER;
                string psw = emapar.EMAPAR_PASSWORD;
                bool flgSSL = emapar.EMAPAR_SSL;
                int? port = emapar.EMAPAR_PORT;

                SmtpClient smtp = new SmtpClient(server);
                smtp.Credentials = new System.Net.NetworkCredential(user, psw);
                smtp.EnableSsl = flgSSL;
                if (port.HasValue)
                {
                    smtp.Port = port.Value;
                }
                else { smtp.Port = 25; }
                return smtp;
            }
            catch { }
            return null;
        }
    }
}

