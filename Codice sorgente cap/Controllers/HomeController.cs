using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Models;
using IZSLER_CAP.Helpers;
using System.Net.Mail;

//da togliere
using System.IO;

namespace IZSLER_CAP.Controllers
{

    public class HomeController : B16Controller
    {
        public HomeController ():base()
        {
            this.SetMessage= "Dashboard";
            this.SetLiDashBoard = "current";
        }
        
        public ActionResult FileNotFound()
        {
            this.SetMessage = "";
            RedirectToRouteResult r= CheckLogin();
            if(r!=null){return r;}
            
            //return View();

            RichiestaModel p = new RichiestaModel();
            return View(p);

        }
        public ActionResult Index()
        {
            RedirectToRouteResult r= CheckLogin();
            if(r!=null){return r;}
            
            //return View();

            RichiestaModel p = new RichiestaModel();
            return View(p);

        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult PopUpRispondiIntermedio(int? id)
        {

            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            RichiestaModel ric = new RichiestaModel(id.Value);
            
            return View(ric);
        }

        public ActionResult RichiesteInsert(int? chiave,string origine)
        {

           
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            RichiestaModel ric = new RichiestaModel();
            ric.Paramiter = origine;
            string info = "";

            if (chiave == null && origine == "prodotto_index")
            {
                ric.TipoOggetto = TipoOggettoRichiesta.Prodotto.ToString();
                info = "di valorizzazione";//"di censimento";
            }


            if (chiave == null && origine == "analisi_index")
            {
                ric.TipoOggetto = TipoOggettoRichiesta.Analisi.ToString();
                info = "di valorizzazione";
            }

            if (chiave == null && origine == "analisi_index_inter")
            {
                ric.TipoOggetto = TipoOggettoRichiesta.Intermedio.ToString();
                info = "di Intermedio";
                

            }

            if (chiave != null && origine == "analisi_edit")
            {
                ric.IsRichiestaDiSblocco = true;
                ric.TipoOggetto = TipoOggettoRichiesta.Analisi.ToString();
                info = "di sblocco";
            }

            if (chiave != null && ric.TipoOggetto == TipoOggettoRichiesta.Analisi.ToString())
            {
                ric.Chiave = (int)chiave;
                
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
               
                VALORI_VALORIZZAZIONI v  = en.VALORI_VALORIZZAZIONI.Include("UTENTE").
                                            Where(x => x.VALORI_ID == chiave).SingleOrDefault();

                ric.chiave_desc = v.VALORI_VN + "-" + v.VALORI_MP_REV;
                
                if (v.VALORI_GRUPPO_GRUREP_ID.HasValue)
                { // CERCO L'UNICO RESPONSABILE DEL GRUPPO PER IL PROFILO "CDG"
                    PROFIL_PROFILI p = en.PROFIL_PROFILI.Where(z => z.PROFIL_CODICE == "CDG").SingleOrDefault();
                    M_UTPRGR_UTENTI_PROFILI_GRUPPI g = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Include("PROFIL_PROFILI")
                                .Where(x => x.M_UTPRGR_GRUREP_ID == v.VALORI_GRUPPO_GRUREP_ID.Value
                                    && x.M_UTPRGR_FLG_PRINCIPALE == true && x.M_UTPRGR_PROFIL_ID == p.PROFIL_ID).SingleOrDefault();

                    if (g != null)
                    {
                        ric.destinatatio_id = g.M_UTPRGR_UTENTE_ID;
                        ric.destinatatio_nome = g.UTENTE.UTENTE_NOME + " " + g.UTENTE.UTENTE_COGNOME;
                    }
                }

                
            }            

           

            this.SetMessage = "Nuova Richiesta " + info;

            return View(ric);


            

        }

        public ActionResult RichiesteElencoDaFare( string origine)
        {
            this.SetMessage = "Elenco completo"; 
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            RichiestaModel ric = new RichiestaModel();
            ric.Paramiter = origine;
            return View(ric);

        }
        public ActionResult RichiesteEdit(int id,string origine)
        {
            
            

            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            RichiestaModel ric = new RichiestaModel(id);
            ric.Paramiter = origine;

            string info = "";

            if (origine == "analisi_edit")
            {
                ric.IsRichiestaDiSblocco = true;
                info = "sblocco";
            }
            
            info = ric.m_RichiestaCorrente.T_Richie_desc;

            this.SetMessage = "Richiesta di " + info;

            
            return View(ric);

        }
        [HttpPost]
        public ActionResult SavePopUpUtenti (PopUpClass pc)
        {
            return Json(new { ok = true, description = pc.Description, id = pc.Id });
        }

        
        public ActionResult PopUpIndirizziUtenti()
        {

            int profilo_id = this.CurrentProfileID;

            IndirizziModel ind = new IndirizziModel(profilo_id);
            //RichiestaModel ric = new RichiestaModel(p);
            return View(ind);
        }

        [HttpPost]
        public ActionResult RispostaRichiestaIntermedio(MyRichiestaAjax ric)
        {
            bool flagOK = true;
            string er = "";
            int nuovoID = 0;
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                RICHIE_RICHIESTE rOrig = en.RICHIE_RICHIESTE.Where(x => x.RICHIE_ID == ric.Richie_id).SingleOrDefault();
                T_STARIC_STATO_RICHIESTA ts = en.T_STARIC_STATO_RICHIESTA.Where(z => z.T_STARIC_CODICE == "EVA").SingleOrDefault();
                rOrig.RICHIE_T_STARIC_ID = ts.T_STARIC_ID;
                // storica
                TRKRIC_RICHIESTE_TRACKING tr = new TRKRIC_RICHIESTE_TRACKING();
                tr.TRKRIC_RICHIE_ID = rOrig.RICHIE_ID ;
                tr.TRKRIC_DATA_INS = dt;
                tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                tr.TRKRIC_T_STARIC_ID = ts.T_STARIC_ID;
                tr.TRKRIC_TITOLO = rOrig.RICHIE_TITOLO;
                tr.TRKRIC_T_RICPRI_ID = rOrig.RICHIE_T_RICPRI_ID;
                tr.TRKRIC_TESTO = rOrig.RICHIE_TESTO;
                en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);
                en.SaveChanges();
                RICHIE_RICHIESTE nr = new RICHIE_RICHIESTE();
                T_RICHIE_TIPO_RICHIESTA trictipo = en.T_RICHIE_TIPO_RICHIESTA.Where(z => z.T_RICHIE_CODICE == "INT").SingleOrDefault();
                nr.RICHIE_T_RICHIE_ID = trictipo.T_RICHIE_ID;
                ts = en.T_STARIC_STATO_RICHIESTA.Where(z => z.T_STARIC_CODICE == "INV").SingleOrDefault();
                nr.RICHIE_T_STARIC_ID = ts.T_STARIC_ID;
                nr.RICHIE_TITOLO = "Risposta Richiesta Intermedio ";
                nr.RICHIE_RICHIEDENTE_UTENTE_ID = rOrig.RICHIE_DESTINATARIO_UTENTE_ID.Value;
                nr.RICHIE_DESTINATARIO_UTENTE_ID = rOrig.RICHIE_RICHIEDENTE_UTENTE_ID;
                
                nr.RICHIE_TESTO = ric.Richie_testo;
                nr.RICHIE_DATA_RICHIESTA = dt;
                nr.RICHIE_ID = 0;
                nr.RICHIE_T_STARIC_ID = ts.T_STARIC_ID;
                nr.RICHIE_VALORI_ID = null;
                nr.RICHIE_T_RICPRI_ID = ric.Richie_t_ricpri_id;

                en.RICHIE_RICHIESTE.AddObject(nr);
                en.SaveChanges();

                nuovoID = nr.RICHIE_ID;
                // storica
                tr = new TRKRIC_RICHIESTE_TRACKING();
                tr.TRKRIC_RICHIE_ID = nuovoID;
                tr.TRKRIC_DATA_INS = dt;
                tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                tr.TRKRIC_T_STARIC_ID = ts.T_STARIC_ID;
                tr.TRKRIC_TITOLO = nr.RICHIE_TITOLO;
                tr.TRKRIC_T_RICPRI_ID = nr.RICHIE_T_RICPRI_ID ;
                tr.TRKRIC_TESTO = nr.RICHIE_TESTO;

                en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);

                en.SaveChanges();

            }
           
             catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            if (nuovoID == 0)
                flagOK = false;
            return Json(new { ok = flagOK, description = ric.Richie_testo, id = nuovoID, infopersonali = er });
        }
        [HttpPost]
        public ActionResult ArchiviaRispostaIntermedio
        (MyRichiestaAjax ric)
        {
            bool flagOK = true;
            string er = "";
            
            try
            {


                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                RICHIE_RICHIESTE nr = en.RICHIE_RICHIESTE.Where(x => x.RICHIE_ID == ric.Richie_id).SingleOrDefault();
                T_STARIC_STATO_RICHIESTA ts = en.T_STARIC_STATO_RICHIESTA.Where(z => z.T_STARIC_CODICE == "EVA").SingleOrDefault();
                nr.RICHIE_T_STARIC_ID = ts.T_STARIC_ID;
                TRKRIC_RICHIESTE_TRACKING tr = new TRKRIC_RICHIESTE_TRACKING();
                // storica
                tr.TRKRIC_RICHIE_ID = nr.RICHIE_ID ;
                tr.TRKRIC_DATA_INS = DateTime.Now;
                tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                tr.TRKRIC_T_STARIC_ID = ts.T_STARIC_ID;
                tr.TRKRIC_TITOLO = nr.RICHIE_TITOLO ;
                tr.TRKRIC_T_RICPRI_ID =  nr.RICHIE_T_RICPRI_ID;
                tr.TRKRIC_TESTO = nr.RICHIE_TESTO ;

                en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);
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
        public ActionResult InvioRichiesta(MyRichiestaAjax ric)
        {
            bool flagOK = true;
            string er = "";
            int nuovoID = 0;
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                RICHIE_RICHIESTE nr = new RICHIE_RICHIESTE();
                TRKRIC_RICHIESTE_TRACKING tr = new TRKRIC_RICHIESTE_TRACKING();

                T_STARIC_STATO_RICHIESTA ts = en.T_STARIC_STATO_RICHIESTA.Where(z => z.T_STARIC_CODICE == "INV").SingleOrDefault();
                ////UPDATE  
                //if (ric.Richie_id != null && ric.Richie_id > 0)
                //{
                //    nuovoID = ric.Richie_id;
                //    nr = en.RICHIE_RICHIESTE.Where(x => x.RICHIE_ID == ric.Richie_id).SingleOrDefault();
                //    nr.RICHIE_TITOLO = ric.Richie_titolo;
                //    //A.RICHIE_RICHIEDENTE_UTENTE_ID = ric.Richie_richiedente_utente_id;
                //    if (ric.Richie_destinatario_utente_id == 0)
                //        nr.RICHIE_DESTINATARIO_UTENTE_ID = null;
                //    else
                //        nr.RICHIE_DESTINATARIO_UTENTE_ID = ric.Richie_destinatario_utente_id;
                //    nr.RICHIE_TESTO = ric.Richie_testo;
                //    nr.RICHIE_T_RICPRI_ID = ric.Richie_t_ricpri_id;

                //    if (ric.Richie_valori_id == 0)
                //        nr.RICHIE_VALORI_ID = null;
                //    else
                //        nr.RICHIE_VALORI_ID = ric.Richie_valori_id;

                //    // storica
                //    tr.TRKRIC_RICHIE_ID = nuovoID;
                //    tr.TRKRIC_DATA_INS = DateTime.Now;
                //    tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                //    tr.TRKRIC_T_STARIC_ID = ts.T_STARIC_ID;
                //    tr.TRKRIC_TITOLO = ric.Richie_titolo;
                //    tr.TRKRIC_T_RICPRI_ID = ric.Richie_t_ricpri_id;
                //    tr.TRKRIC_TESTO = ric.Richie_testo;

                //    en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);

                //    en.SaveChanges();


                //}
                //else
                {
                    int t_richie_id;

                    if (ric.PaginaOrigine == "prodotto_index")
                    {
                        T_RICHIE_TIPO_RICHIESTA trictipo = en.T_RICHIE_TIPO_RICHIESTA.Where(z => z.T_RICHIE_CODICE == "ATT").SingleOrDefault();
                        t_richie_id = trictipo.T_RICHIE_ID;
                    }
                    else //per ora default a valorizzazione
                    {
                        T_RICHIE_TIPO_RICHIESTA trictipo = null;
                        if (ric.T_Richie_desc == TipoOggettoRichiesta.Intermedio.ToString())
                        {
                            trictipo = en.T_RICHIE_TIPO_RICHIESTA.Where(z => z.T_RICHIE_CODICE == "INT").SingleOrDefault();
                            t_richie_id = trictipo.T_RICHIE_ID;
                        }
                        else
                        {
                            trictipo = en.T_RICHIE_TIPO_RICHIESTA.Where(z => z.T_RICHIE_CODICE == "VAL").SingleOrDefault();
                            t_richie_id = trictipo.T_RICHIE_ID;
                        }

                    }


                    nr.RICHIE_TITOLO = ric.Richie_titolo;
                    nr.RICHIE_RICHIEDENTE_UTENTE_ID = ric.Richie_richiedente_utente_id;
                    if (ric.Richie_destinatario_utente_id == 0)
                        nr.RICHIE_DESTINATARIO_UTENTE_ID = null;
                    else
                        nr.RICHIE_DESTINATARIO_UTENTE_ID = ric.Richie_destinatario_utente_id;
                    nr.RICHIE_TESTO = ric.Richie_testo;
                    nr.RICHIE_DATA_RICHIESTA = DateTime.Now;
                    nr.RICHIE_ID = 0;
                    nr.RICHIE_T_STARIC_ID = ts.T_STARIC_ID;

                    if (ric.Richie_valori_id == 0)
                        nr.RICHIE_VALORI_ID = null;
                    else
                        nr.RICHIE_VALORI_ID = ric.Richie_valori_id;

                    nr.RICHIE_T_RICHIE_ID = t_richie_id;
                    nr.RICHIE_T_RICPRI_ID = ric.Richie_t_ricpri_id;
                    nr.RICHIE_FLG_ASSEGN_AL_GRUPPO = ric.Richie_flg_assegn_al_gruppo; //Ric#3

                    en.RICHIE_RICHIESTE.AddObject(nr);
                    en.SaveChanges();

                    nuovoID = nr.RICHIE_ID;

                    // storica
                    tr.TRKRIC_RICHIE_ID = nuovoID;
                    tr.TRKRIC_DATA_INS = DateTime.Now;
                    tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                    tr.TRKRIC_T_STARIC_ID = ts.T_STARIC_ID;
                    tr.TRKRIC_TITOLO = ric.Richie_titolo;
                    tr.TRKRIC_T_RICPRI_ID = ric.Richie_t_ricpri_id;
                    tr.TRKRIC_TESTO = ric.Richie_testo;

                    en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);
                    en.SaveChanges();
                    inviaEmail(nr);
                }

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            if (nuovoID == 0)
                flagOK = false;
            return Json(new { ok = flagOK, description = ric.Richie_testo, id = nuovoID, infopersonali = er });
        }
        private MailMessage getMessage(RICHIE_RICHIESTE r)
        {
            try
            {

                if (r.RICHIE_DESTINATARIO_UTENTE_ID.HasValue)
                {
                    MailMessage mess = new MailMessage();
                    mess.To.Add(getAddressByUserID(r.RICHIE_DESTINATARIO_UTENTE_ID.Value));
                    if (forceEmailServiceAccount())
                        mess.From = getAddressServiceAccount();
                    else
                        mess.From = getAddressByUserID(r.RICHIE_RICHIEDENTE_UTENTE_ID);
                    mess.Body = r.RICHIE_TESTO;
                    mess.Subject = r.RICHIE_TITOLO;
                    mess.IsBodyHtml = true;

                    return mess;
                }
            }
            catch
            {
            }
            return null;
        }

        private void inviaEmail(RICHIE_RICHIESTE r)
        {

            if (enableSendEmail())
            {
                SmtpClient smtp = getSmtpClient();
                if (smtp != null)
                {
                    MailMessage mess = getMessage(r);
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
        [HttpPost]
        public ActionResult SaveRichiesta(MyRichiestaAjax ric)
        {
            bool flagOK = true;
            string er="";
            int nuovoID = 0;
            try 
            {

              IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
              RICHIE_RICHIESTE nr = new RICHIE_RICHIESTE();
              TRKRIC_RICHIESTE_TRACKING tr = new TRKRIC_RICHIESTE_TRACKING();

              T_STARIC_STATO_RICHIESTA ts = en.T_STARIC_STATO_RICHIESTA.Where(z => z.T_STARIC_CODICE == "INS").SingleOrDefault();
              //UPDATE  
              if (ric.Richie_id != null && ric.Richie_id > 0)
              {
                  nuovoID = ric.Richie_id;
                  nr = en.RICHIE_RICHIESTE.Where(x => x.RICHIE_ID == ric.Richie_id).SingleOrDefault();
                  nr.RICHIE_TITOLO = ric.Richie_titolo;
                  //A.RICHIE_RICHIEDENTE_UTENTE_ID = ric.Richie_richiedente_utente_id;
                  if (ric.Richie_destinatario_utente_id == 0)
                      nr.RICHIE_DESTINATARIO_UTENTE_ID = null;
                  else
                      nr.RICHIE_DESTINATARIO_UTENTE_ID = ric.Richie_destinatario_utente_id;
                  nr.RICHIE_TESTO = ric.Richie_testo;
                  nr.RICHIE_T_RICPRI_ID = ric.Richie_t_ricpri_id;

                  //Ric#3
                  nr.RICHIE_FLG_ASSEGN_AL_GRUPPO = ric.Richie_flg_assegn_al_gruppo;

                  if (ric.Richie_valori_id == 0)
                      nr.RICHIE_VALORI_ID = null;
                  else
                      nr.RICHIE_VALORI_ID = ric.Richie_valori_id;

                  if (ric.Richie_prodot_id == 0)
                      nr.RICHIE_PRODOT_ID = null;
                  else
                      nr.RICHIE_PRODOT_ID = ric.Richie_prodot_id;

                  
                  // storica
                  tr.TRKRIC_RICHIE_ID = nuovoID;
                  tr.TRKRIC_DATA_INS = DateTime.Now;
                  tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                  tr.TRKRIC_T_STARIC_ID = ts.T_STARIC_ID;
                  tr.TRKRIC_TITOLO = ric.Richie_titolo;
                  tr.TRKRIC_T_RICPRI_ID = ric.Richie_t_ricpri_id;
                  tr.TRKRIC_TESTO = ric.Richie_testo;

                  en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);
                  
                  en.SaveChanges();

                  
              }
              else
              {
                  int t_richie_id;

                  if (ric.PaginaOrigine == "prodotto_index")
                  {
                      T_RICHIE_TIPO_RICHIESTA trictipo= en.T_RICHIE_TIPO_RICHIESTA.Where(z => z.T_RICHIE_CODICE == "ATT").SingleOrDefault();
                      t_richie_id = trictipo.T_RICHIE_ID;
                  }
                  else //per ora default a valorizzazione
                  {
                      T_RICHIE_TIPO_RICHIESTA trictipo = null;
                      if (ric.T_Richie_desc == TipoOggettoRichiesta.Intermedio.ToString())
                      {
                          trictipo = en.T_RICHIE_TIPO_RICHIESTA.Where(z => z.T_RICHIE_CODICE == "INT").SingleOrDefault();
                          t_richie_id = trictipo.T_RICHIE_ID;
                      }
                      else
                      {
                          trictipo = en.T_RICHIE_TIPO_RICHIESTA.Where(z => z.T_RICHIE_CODICE == "VAL").SingleOrDefault();
                          t_richie_id = trictipo.T_RICHIE_ID;    
                      }
                      
                  }
                  
               
                  nr.RICHIE_TITOLO = ric.Richie_titolo;
                  nr.RICHIE_RICHIEDENTE_UTENTE_ID = ric.Richie_richiedente_utente_id;
                  if (ric.Richie_destinatario_utente_id == 0)
                      nr.RICHIE_DESTINATARIO_UTENTE_ID = null;
                  else
                     nr.RICHIE_DESTINATARIO_UTENTE_ID = ric.Richie_destinatario_utente_id ;
                  nr.RICHIE_TESTO = ric.Richie_testo;
                  nr.RICHIE_DATA_RICHIESTA = DateTime.Now;
                  nr.RICHIE_ID = 0;
                  nr.RICHIE_T_STARIC_ID = ts.T_STARIC_ID;

                  //Ric#3
                  nr.RICHIE_FLG_ASSEGN_AL_GRUPPO = ric.Richie_flg_assegn_al_gruppo;

                  if (ric.Richie_valori_id == 0)
                      nr.RICHIE_VALORI_ID = null;
                  else
                      nr.RICHIE_VALORI_ID = ric.Richie_valori_id;

                  if (ric.Richie_prodot_id == 0)
                      nr.RICHIE_PRODOT_ID = null;
                  else
                      nr.RICHIE_PRODOT_ID = ric.Richie_prodot_id;

                  nr.RICHIE_T_RICHIE_ID = t_richie_id;
                  nr.RICHIE_T_RICPRI_ID = ric.Richie_t_ricpri_id;

                  if (nr.RICHIE_TITOLO == null || nr.RICHIE_TITOLO.Length == 0)
                      nr.RICHIE_TITOLO = "Nuova richiesta";
                  
                  
                  en.RICHIE_RICHIESTE.AddObject(nr);
                  en.SaveChanges();

                  nuovoID=nr.RICHIE_ID;

                  // storica
                  tr.TRKRIC_RICHIE_ID = nuovoID;
                  tr.TRKRIC_DATA_INS = DateTime.Now;
                  tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                  tr.TRKRIC_T_STARIC_ID = ts.T_STARIC_ID;
                  tr.TRKRIC_TITOLO = ric.Richie_titolo;
                  tr.TRKRIC_T_RICPRI_ID = ric.Richie_t_ricpri_id;
                  tr.TRKRIC_TESTO = ric.Richie_testo;

                  en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);
                  en.SaveChanges();
                      
              }

            }
            catch (Exception ex)
            {
                er = ex.ToString ();
                flagOK = false;
            }

            if (nuovoID == 0)
                flagOK = false;
            return Json(new { ok = flagOK, description = ric.Richie_testo, id = nuovoID, infopersonali = er });
        }


        public ActionResult DeleteRichiesta(MyRichiestaAjax ric)
        {
            bool flagOK = true;
            string er = "";
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //RICHIE_RICHIESTE nr = new RICHIE_RICHIESTE();
                RICHIE_RICHIESTE nr = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_ID == ric.Richie_id).SingleOrDefault();

                //Ric#8
                if (nr.RICHIE_VALORI_ID.HasValue)
                {
                    VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == nr.RICHIE_VALORI_ID).SingleOrDefault();
                    string stato_val = v.T_STAVAL_STATO_VALORIZZAZIONE.T_STAVAL_CODICE.ToString();
                    if (stato_val == "INVAL")
                    {
                        T_STAVAL_STATO_VALORIZZAZIONE st_val = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z => z.T_STAVAL_CODICE == "DAVAL").SingleOrDefault();
                        v.VALORI_T_STAVAL_ID = st_val.T_STAVAL_ID;
                        en.SaveChanges();
                    }
                }
                //Ric#8
                if (nr.RICHIE_PRODOT_ID.HasValue)
                {
                    PRODOT_PRODOTTI pr = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == nr.RICHIE_PRODOT_ID).SingleOrDefault();
                    string stato_pro = pr.T_STAPRO_STATO_PRODOTTO.T_STAPRO_CODICE.ToString();
                    if (stato_pro == "INVAL")
                    {
                        T_STAPRO_STATO_PRODOTTO sp = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "DAVAL").SingleOrDefault();
                        pr.PRODOT_T_STAPRO_ID = sp.T_STAPRO_ID;
                        en.SaveChanges();
                    }
                }


                    T_STARIC_STATO_RICHIESTA st = new T_STARIC_STATO_RICHIESTA();
                    TRKRIC_RICHIESTE_TRACKING tr = new TRKRIC_RICHIESTE_TRACKING();

                    st = en.T_STARIC_STATO_RICHIESTA.Where(z => z.T_STARIC_CODICE == "ELI").SingleOrDefault();
                    nr.RICHIE_T_STARIC_ID = st.T_STARIC_ID;

                    en.SaveChanges();

                    //storico
                    tr.TRKRIC_RICHIE_ID = ric.Richie_id;
                    tr.TRKRIC_DATA_INS = DateTime.Now;
                    tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                    tr.TRKRIC_T_STARIC_ID = st.T_STARIC_ID;
                    tr.TRKRIC_TITOLO = ric.Richie_titolo;
                    tr.TRKRIC_T_RICPRI_ID = ric.Richie_t_ricpri_id;
                    tr.TRKRIC_TESTO = ric.Richie_testo;

                    en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);
                    en.SaveChanges();
               
               
                
           }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            //if (nuovoID == 0)
            //    flagOK = false;
            return Json(new { ok = flagOK, infopersonali = er });
        }


        
        [HttpPost]
        public ActionResult RichiesteEdit(RichiestaModel model)
        {
            if (model.Command == "command1")
            {
                // do something
            }
            else if (model.Command == "command2")
            {

                string a = Request["cleditor"].ToString();
                // do something else
            }
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            RichiestaModel ric = new RichiestaModel();
            return View(ric);
        }

        public ActionResult RichiestaWorkflow(int id)
        {
            RichiestaWorkflowModel ric = new RichiestaWorkflowModel(id);
            return View(ric);
        }

        private void salvaTrackingAnalisi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en, DateTime dt)
        {
            TRKVAL_VALORIZZAZIONI_TRACKING trk = new TRKVAL_VALORIZZAZIONI_TRACKING();
            trk.TRKVAL_COSTO_TOT = v.VALORI_COSTO_TOT;
            trk.TRKVAL_DATA_INS = dt;
            trk.TRKVAL_DIM_LOTTO = v.VALORI_DIM_LOTTO;
            trk.TRKVAL_FLG_PONDERAZIONE = v.VALORI_FLG_PONDERAZIONE;
            //trk.TRKVAL_ID 
            trk.TRKVAL_PESO_POSITIVO = v.VALORI_PESO_POSITIVO;
            trk.TRKVAL_T_STAVAL_ID = v.VALORI_T_STAVAL_ID;
            trk.TRKVAL_UTENTE_ID = this.CurrentUserID;
            trk.TRKVAL_VALORI_ID = v.VALORI_ID;
            trk.TRKVAL_VALORI_UTENTE_ID = v.VALORI_UTENTE_ID.Value;

            //            trk.TRKVAL_DESC_INTERMEDIO = v.VALORI_DESC; non usato da Analisi
            //            trk.TRKVAL_GRUPPO_GRUREP_ID = v.VALORI_GRUPPO_GRUREP_ID; non usato da Analisi
            //            trk.TRKVAL_REPARTO_GRUREP_ID = v.VALORI_REPARTO_GRUREP_ID; non usato da Analisi
            trk.TRKVAL_COSTO_TOT_DELIB = v.VALORI_COSTO_TOT_DELIB;
            //            trk.TRKVAL_CODICE_INTERMEDIO = v.VALORI_CODICE_INTERMEDIO; non usato da Analisi
            trk.TRKVAL_COSTO_TARIFFA = v.VALORI_COSTO_TARIFFA;
            trk.TRKVAL_COSTO_TARIFFA_DELIBERATO = v.VALORI_COSTO_TARIFFA_DELIBERATO;
            trk.TRKVAL_COSTO_TARIFFA_D_DELIBERATO = v.VALORI_COSTO_TARIFFA_D_DELIBERATO;

            trk.TRKVAL_COD_VN_MP_REV_SETTORE = v.VALORI_COD_VN_MP_REV_SETTORE;
            trk.TRKVAL_COD_VN_MP_REV_SETTORE_D = v.VALORI_COD_VN_MP_REV_SETTORE_D;
            trk.TRKVAL_COD_VN_MP_REV_SETTORE_V = v.VALORI_COD_VN_MP_REV_SETTORE_V;
            trk.TRKVAL_ALLEGATO1 = v.VALORI_ALLEGATO1;
            trk.TRKVAL_ALLEGATO2 = v.VALORI_ALLEGATO2;
            trk.TRKVAL_DOCUMENTO = v.VALORI_DOCUMENTO;

            trk.TRKVAL_FLG_INTERNO = v.VALORI_FLG_INTERNO;
            trk.TRKVAL_FLG_BLOCCATO = v.VALORI_FLG_BLOCCATO;
            trk.TRKVAL_VN = v.VALORI_VN;
            trk.TRKVAL_MP_REV = v.VALORI_MP_REV;
            trk.TRKVAL_TECNICA = v.VALORI_TECNICA;
            trk.TRKVAL_MATRICE = v.VALORI_MATRICE;
            trk.TRKVAL_FLG_INTERM = v.VALORI_FLG_INTERM;
            trk.TRKVAL_FLG_NON_PROGRAMMABILI = v.VALORI_FLG_NON_PROGRAMMABILI;
            trk.TRKVAL_FLAG_MODELLO = v.VALORI_FLAG_MODELLO;
            trk.TRKVAL_CODICE_DESC = v.VALORI_CODICE_DESC;
            trk.TRKVAL_COSTO_DIRETTO = v.VALORI_COSTO_DIRETTO;
            trk.TRKVAL_FLG_ASSEGN_AL_GRUPPO = v.VALORI_FLG_ASSEGN_AL_GRUPPO;
            trk.TRKVAL_COSSTO_DIRETTO = v.VALORI_COSSTO_DIRETTO;
            trk.TRKVAL_COSSTO_INDIRETTO = v.VALORI_COSSTO_INDIRETTO;
            trk.TRKVAL_COSSTO_PERSONALE = v.VALORI_COSSTO_PERSONALE;
            trk.TRKVAL_COSSTO_MATERIALI = v.VALORI_COSSTO_MATERIALI;
            trk.TRKVAL_COSSTO_ANALISI = v.VALORI_COSSTO_ANALISI;
            trk.TRKVAL_COSSTO_ACCETTAZIONE = v.VALORI_COSSTO_ACCETTAZIONE;
            trk.TRKVAL_COSSTO_APP_DED = v.VALORI_COSSTO_APP_DED;
            trk.TRKVAL_COSSTO_PRODUZ_INT = v.VALORI_COSSTO_PRODUZ_INT;
            trk.TRKVAL_TS_AGGIORNAMENTO_POSIZIONI = v.VALORI_TS_AGGIORNAMENTO_POSIZIONI;

            en.TRKVAL_VALORIZZAZIONI_TRACKING.AddObject(trk);

        }

        private void salvaTrackingProdotto(PRODOT_PRODOTTI pr, IZSLER_CAP_Entities en, DateTime dt)
        {
            TRKPRO_PRODOTTO_TRACKING trk = new TRKPRO_PRODOTTO_TRACKING();
            trk.TRKPRO_CODICE = pr.PRODOT_CODICE;
            trk.TRKPRO_COSTOUNITARIO = pr.PRODOT_COSTOUNITARIO;
            trk.TRKPRO_COSTOUNITARIO_DELIBE = pr.PRODOT_COSTOUNITARIO_DELIBE;
            trk.TRKPRO_DATA_INS = dt;
            trk.TRKPRO_DESC = pr.PRODOT_DESC;
            trk.TRKPRO_DIM_LOTTO = 0; //pr.PRODOT_DIM_LOTTO.Value;
            trk.TRKPRO_FLG_INTERNO = pr.PRODOT_FLG_INTERNO;
            trk.TRKPRO_PRODOT_ID = pr.PRODOT_ID;
            trk.TRKPRO_NR_CAMP_QUALITA = pr.PRODOT_NR_CAMP_QUALITA;
            trk.TRKPRO_PRODOT_UTENTE_ID = pr.PRODOT_UTENTE_ID.Value;
            trk.TRKPRO_REPARTO_GRUREP_ID = pr.PRODOT_REPARTO_GRUREP_ID;
            trk.TRKPRO_T_STAPRO_ID = pr.PRODOT_T_STAPRO_ID.Value;
            trk.TRKPRO_T_UNIMIS_ID = pr.PRODOT_T_UNIMIS_ID;
            trk.TRKPRO_UTENTE_ID = this.CurrentUserID;
            trk.TRKPRO_FLG_BLOCCATO_MAGAZZINO = false; // pr.PRODOT_FLG_BLOCCATO_MAGAZZINO;
            trk.TRKPRO_DOCUMENTO = pr.PRODOT_DOCUMENTO;

            trk.TRKPRO_DESC_ESTESA = pr.PRODOT_DESC_ESTESA;
            trk.TRKPRO_FLG_BLOCCATO = pr.PRODOT_FLG_BLOCCATO;
            trk.TRKPRO_CODICE_DESC = pr.PRODOT_CODICE_DESC;
            trk.TRKPRO_COEFF_CONVERSIONE = pr.PRODOT_COEFF_CONVERSIONE;
            trk.TRKPRO_STIMA_PROD_ANNO = pr.PRODOT_STIMA_PROD_ANNO;
            trk.TRKPRO_PERC_SCARTO = pr.PRODOT_PERC_SCARTO;
            trk.TRKPRO_TARIFFA = pr.PRODOT_TARIFFA;
            trk.TRKPRO_TARIFFA_PROPOSTA = pr.PRODOT_TARIFFA_PROPOSTA;
            trk.TRKPRO_HASHKEY = pr.PRODOT_HASHKEY;
            trk.TRKPRO_T_UNIMIS_ID_SEC = pr.PRODOT_T_UNIMIS_ID_SEC;
            trk.TRKPRO_FLG_ASSEGN_AL_GRUPPO = pr.PRODOT_FLG_ASSEGN_AL_GRUPPO;
            trk.TRKPRO_COSSTO_DIRETTO = pr.PRODOT_COSSTO_DIRETTO;
            trk.TRKPRO_COSSTO_INDIRETTO = pr.PRODOT_COSSTO_INDIRETTO;
            trk.TRKPRO_COSSTO_PERSONALE = pr.PRODOT_COSSTO_PERSONALE;
            trk.TRKPRO_COSSTO_MATERIALI = pr.PRODOT_COSSTO_MATERIALI;
            trk.TRKPRO_COSSTO_ANALISI = pr.PRODOT_COSSTO_ANALISI;
            trk.TRKPRO_COSSTO_APP_DED = pr.PRODOT_COSSTO_APP_DED;
            trk.TRKPRO_COSSTO_PRODUZ_INT = pr.PRODOT_COSSTO_PRODUZ_INT;
            trk.TRKPRO_TS_AGGIORNAMENTO_POSIZIONI = pr.PRODOT_TS_AGGIORNAMENTO_POSIZIONI;

            en.TRKPRO_PRODOTTO_TRACKING.AddObject(trk);
        }
        

        [HttpPost]
        public ActionResult InvValRichiesta(MyRichiestaAjax ric)
        {
            bool flagOK = true;
            string er = "";
            int nuovoID = 0;
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                RICHIE_RICHIESTE nr = new RICHIE_RICHIESTE();
                TRKRIC_RICHIESTE_TRACKING tr = new TRKRIC_RICHIESTE_TRACKING();

                T_STARIC_STATO_RICHIESTA ts = en.T_STARIC_STATO_RICHIESTA.Where(z => z.T_STARIC_CODICE == "INV").SingleOrDefault();
                
                nuovoID = ric.Richie_id;
                nr = en.RICHIE_RICHIESTE.Where(x => x.RICHIE_ID == ric.Richie_id).SingleOrDefault();
                
                nr.RICHIE_TITOLO = ric.Richie_titolo;
                if (ric.Richie_destinatario_utente_id == 0)
                   nr.RICHIE_DESTINATARIO_UTENTE_ID = null;
                else
                    nr.RICHIE_DESTINATARIO_UTENTE_ID = ric.Richie_destinatario_utente_id;
                nr.RICHIE_TESTO = ric.Richie_testo;
                nr.RICHIE_T_RICPRI_ID = ric.Richie_t_ricpri_id;
                nr.RICHIE_T_STARIC_ID = ts.T_STARIC_ID;
                //Ric#3
                nr.RICHIE_FLG_ASSEGN_AL_GRUPPO = ric.Richie_flg_assegn_al_gruppo;

                if (ric.Richie_valori_id == 0)
                    nr.RICHIE_VALORI_ID = null;
                else
                    nr.RICHIE_VALORI_ID = ric.Richie_valori_id;

                if(ric.Richie_prodot_id ==0 )
                    nr.RICHIE_PRODOT_ID = null;
                else
                    nr.RICHIE_PRODOT_ID = ric.Richie_prodot_id;

                
                // storica
                tr.TRKRIC_RICHIE_ID = nuovoID;
                tr.TRKRIC_DATA_INS = DateTime.Now;
                tr.TRKRIC_UTENTE_ID = this.CurrentUserID;
                tr.TRKRIC_T_STARIC_ID = ts.T_STARIC_ID;
                tr.TRKRIC_TITOLO = ric.Richie_titolo;
                tr.TRKRIC_T_RICPRI_ID = ric.Richie_t_ricpri_id;
                tr.TRKRIC_TESTO = ric.Richie_testo;

                en.TRKRIC_RICHIESTE_TRACKING.AddObject(tr);

                //en.SaveChanges();


                DateTime dt = DateTime.Now;
                if (nr.RICHIE_VALORI_ID.HasValue)
                {
                    T_STAVAL_STATO_VALORIZZAZIONE  sv  = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z=> z.T_STAVAL_CODICE=="INVAL").SingleOrDefault();
                    VALORI_VALORIZZAZIONI va = new VALORI_VALORIZZAZIONI();
                    va = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == nr.RICHIE_VALORI_ID).SingleOrDefault();
                    va.VALORI_T_STAVAL_ID = sv.T_STAVAL_ID;
                    va.VALORI_UTENTE_ID = nr.RICHIE_DESTINATARIO_UTENTE_ID.Value;
                    va.VALORI_FLG_ASSEGN_AL_GRUPPO = nr.RICHIE_FLG_ASSEGN_AL_GRUPPO; //Ric#3                    


                    //controllo che il destinatario sia un validatore associato allo stesso gruppo della valorizzazione
                    int group_val = va.VALORI_GRUPPO_GRUREP_ID.Value;
                    List<M_UTPRGR_UTENTI_PROFILI_GRUPPI> group_dest = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(z => z.M_UTPRGR_UTENTE_ID == va.VALORI_UTENTE_ID.Value && z.M_UTPRGR_GRUREP_ID == group_val).ToList<M_UTPRGR_UTENTI_PROFILI_GRUPPI>();

                    if (group_dest.Count() > 0)
                    {
                        salvaTrackingAnalisi(va, en, dt);
                        en.SaveChanges();
                    }
                    else
                    {
                        er = "Il destinatario della richiesta non è associato al gruppo della valorizzazione";
                        flagOK = false;
                        return Json(new { ok = flagOK, description = ric.Richie_testo, id = nuovoID, infopersonali = er });
                    }
                }
                if (nr.RICHIE_PRODOT_ID.HasValue)
                {
                    T_STAPRO_STATO_PRODOTTO sp = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "INVAL").SingleOrDefault();
                  
                    PRODOT_PRODOTTI pr = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == nr.RICHIE_PRODOT_ID).SingleOrDefault();
                    pr.PRODOT_T_STAPRO_ID = sp.T_STAPRO_ID;
                    pr.PRODOT_UTENTE_ID = nr.RICHIE_DESTINATARIO_UTENTE_ID;
                    pr.PRODOT_FLG_BLOCCATO = false;
                    pr.PRODOT_FLG_ASSEGN_AL_GRUPPO = nr.RICHIE_FLG_ASSEGN_AL_GRUPPO; //Ric#3

                    //controllo che il destinatario sia un validatore associato allo stesso gruppo del prodotto
                    int group_prod = pr.PRODOT_REPARTO_GRUREP_ID.Value;
                    List<M_UTPRGR_UTENTI_PROFILI_GRUPPI> group_dest = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(z => z.M_UTPRGR_UTENTE_ID == pr.PRODOT_UTENTE_ID.Value && z.M_UTPRGR_GRUREP_ID == group_prod).ToList<M_UTPRGR_UTENTI_PROFILI_GRUPPI>();
                    
                    if (group_dest.Count() > 0)
                    {

                        salvaTrackingProdotto(pr, en, dt);
                        en.SaveChanges();
                    }
                    else
                    {
                        er = "Il destinatario della richiesta non è associato al gruppo del prodotto";
                        flagOK = false;
                        return Json(new { ok = flagOK, description = ric.Richie_testo, id = nuovoID, infopersonali = er });
                    }
                }

                en.SaveChanges();
                inviaEmail(nr);
                

                //tracking

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            if (nuovoID == 0)
                flagOK = false;
            return Json(new { ok = flagOK, description = ric.Richie_testo, id = nuovoID, infopersonali = er });
        }


    }
}
