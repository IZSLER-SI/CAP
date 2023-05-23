using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Helpers;
using IZSLER_CAP.Models;
using System.Net.Mail;

namespace IZSLER_CAP.Controllers
{
    public class SharedController :  B16Controller
    {
        //
        // GET: /Settings/
        public SharedController()
            : base()
        {
            this.SetMessage = "Settings";
            this.SetLiSettings = "current";
            
        }

        public ActionResult _SollecitiEdit(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            this.SetMessage = "Sollecito";
            B16ModelMgr m = new B16ModelMgr(id);
            return View(m);
        }

        [HttpPost]
        public ActionResult ChiudiSollecito(MySollecitoAjax soll)
        {
            bool flagOK = true;
            string er = "";
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            SOLLEC_SOLLECITI s = new SOLLEC_SOLLECITI();

            try
            {
                s = en.SOLLEC_SOLLECITI.Where(x => x.SOLLEC_ID == soll.Sollec_id).SingleOrDefault();
                s.SOLLEC_ARCHIVIATO = true;

                en.SaveChanges();

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }
            return Json(new { ok = flagOK, infopersonali = er });
        }
        [HttpPost]
        
        public ActionResult SalvaSollecito(MySollecitoAjax soll)
        {
            bool flagOK = true;
            string er = "";
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            SOLLEC_SOLLECITI s = new SOLLEC_SOLLECITI();

            try
            {
                s.SOLLEC_DATA_INS = DateTime.Now;
                s.SOLLEC_DATA_SCADENZA = soll.Sollec_Datascadenza.Year == 1 ? DateTime.Now.AddDays(3) : soll.Sollec_Datascadenza;
                s.SOLLEC_MESSAGGIO = soll.Sollec_Messaggio;
                s.SOLLEC_RICHIE_ID = soll.Sollec_Richie_id;
                s.SOLLEC_SOLLECITATO_UTENTE_ID = soll.Sollec_Utente_id;
                s.SOLLEC_SOLLECITANTE_UTENTE_ID = this.CurrentUserID;
                s.SOLLEC_ARCHIVIATO = false;

                en.SOLLEC_SOLLECITI.AddObject(s);

                en.SaveChanges();
                inviaEmail(s);

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }
            return Json(new { ok = flagOK, infopersonali = er });
        }
        private void inviaEmail(SOLLEC_SOLLECITI s)
        {

            if (enableSendEmail())
            {
                SmtpClient smtp = getSmtpClient();
                if (smtp != null)
                {
                    MailMessage mess = getMessage(s);
                    if (mess != null)
                    {
                        try
                        {
                            smtp.Send(mess);
                        }
                        catch (Exception ex)
                        {
                            logEmail(ex);
                        }
                    }
                }
            }
        }

        private MailMessage getMessage(SOLLEC_SOLLECITI s)
        {
            try
            {
                MailMessage mess = new MailMessage();
                mess.To.Add(getAddressByUserID(s.SOLLEC_SOLLECITATO_UTENTE_ID));
                if (forceEmailServiceAccount())
                    mess.From = getAddressServiceAccount();
                else
                    mess.From = getAddressByUserID(s.SOLLEC_SOLLECITANTE_UTENTE_ID);
                mess.Body = s.SOLLEC_MESSAGGIO ;
                mess.Subject = "Sollecito";
                mess.IsBodyHtml = true;

                return mess;
            }
            catch
            {
            }
            return null;
        }

        public ActionResult _SollecitiInsert(int ric_id, string origine)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }    

            this.SetMessage = "Sollecito";
            B16ModelMgr m = new B16ModelMgr();
            m.Paramiter = origine;
            m.Sollec_ric_id = ric_id;
            return View(m);
        }

    }
}
