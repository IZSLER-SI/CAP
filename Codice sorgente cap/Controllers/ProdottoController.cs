using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Models;
using IZSLER_CAP.Helpers;
using System.Net.Mail;
using System.Text;
using System.Net;
using System.IO;

namespace IZSLER_CAP.Controllers
{
    

    public class ProdottoController : B16Controller
    {

        private bool isAccettazione(int? fase_id)
        {
            if (fase_id.HasValue)
                return isAccettazione(fase_id.Value);
            return false;
        }
        private bool isAccettazione(int fase_id)
        {
            LoadEntities le = new LoadEntities();
            MyFase fase = le.GetFase(fase_id);
            if (fase.Fase_Desc.ToUpper() == "ACCETTAZIONE")
                return true;
            return false;
        }

        private void chiudiRichiestaCorrente(int prodot_id, IZSLER_CAP_Entities en)
        {
            List<RICHIE_RICHIESTE> lstRic = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_PRODOT_ID == prodot_id && z.RICHIE_T_STARIC_ID == 2).ToList<RICHIE_RICHIESTE>();
            foreach (RICHIE_RICHIESTE r in lstRic)
            {
                r.RICHIE_T_STARIC_ID = 3; // Evasa  
            }
        }
        private string getErroreDestinatarioUserID(PRODOT_PRODOTTI pr, string ProfilCodice, IZSLER_CAP_Entities en)
        {
            PROFIL_PROFILI p = en.PROFIL_PROFILI.Where(z => z.PROFIL_CODICE == ProfilCodice).SingleOrDefault();
            string err = "Impossibile attuare la richiesta.<br/>";
            err += "Nessun responsabile di tipo " + p.PROFIL_DESC + " è stato trovato per il Gruppo ";
            if (pr.PRODOT_REPARTO_GRUREP_ID.HasValue)
            {

                GRUREP_GRUPPI_REPARTI gr = en.GRUREP_GRUPPI_REPARTI.Where(z => z.GRUREP_ID == pr.PRODOT_REPARTO_GRUREP_ID.Value).SingleOrDefault();
                err += gr.GRUREP_DESC;
            }
            err += ".";
            return err;
        }
        private int getDestinatarioUserID(PRODOT_PRODOTTI pr, string ProfilCodice, IZSLER_CAP_Entities en)
        {
            if (pr.PRODOT_REPARTO_GRUREP_ID.HasValue)
            {
                PROFIL_PROFILI p = en.PROFIL_PROFILI.Where(z => z.PROFIL_CODICE == ProfilCodice).SingleOrDefault();

                M_UTPRGR_UTENTI_PROFILI_GRUPPI g = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Include("PROFIL_PROFILI").
                    Where(x => x.M_UTPRGR_GRUREP_ID == pr.PRODOT_REPARTO_GRUREP_ID.Value
                        && x.M_UTPRGR_FLG_PRINCIPALE == true
                        && x.M_UTPRGR_PROFIL_ID == p.PROFIL_ID).Take(1).SingleOrDefault();
                if (g != null)
                {

                    return g.M_UTPRGR_UTENTE_ID;
                }
            }
            return 0;
        }
        private RICHIE_RICHIESTE creaRichiestaRespintaProdotto(PRODOT_PRODOTTI p, string motivo, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = p.PRODOT_UTENTE_ID ;
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 2; // attribuzione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = motivo;
            r.RICHIE_TITOLO = "Respingimento validazione prodotto [" + p.PRODOT_CODICE + "]";
            r.RICHIE_PRODOT_ID = p.PRODOT_ID;
            r.RICHIE_FLG_ASSEGN_AL_GRUPPO = p.PRODOT_FLG_ASSEGN_AL_GRUPPO; //Ric#3
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;

        }
        private RICHIE_RICHIESTE creaRichiestaValidazioneProdotto(PRODOT_PRODOTTI p, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = getDestinatarioUserID(p, "REFVAL", en);
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 2; // attribuzione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = "Richiesta automatica validazione prodotto";
            r.RICHIE_TITOLO = "Richiesta Validazione Prodotto [" + p.PRODOT_CODICE + "]";
            r.RICHIE_PRODOT_ID = p.PRODOT_ID;
            r.RICHIE_FLG_ASSEGN_AL_GRUPPO = p.PRODOT_FLG_ASSEGN_AL_GRUPPO;//Ric#3

            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
        }
        private RICHIE_RICHIESTE creaRichiestaSbloccoProdottoMagazzino(PRODOT_PRODOTTI p, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = getDestinatarioUserID(p, "RESMAG", en);
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 2; // attribuzione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = "Richiesta automatica sblocco prodotto interno a magazzino";
            r.RICHIE_TITOLO = "Richiesta sblocco prodotto interno [" + p.PRODOT_CODICE + "]";
            r.RICHIE_PRODOT_ID = p.PRODOT_ID;
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
        }
        private RICHIE_RICHIESTE creaRichiestaDeliberaProdotto(PRODOT_PRODOTTI p, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = getDestinatarioUserID(p, "CDG", en);
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 2; // attribuzione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = "Richiesta automatica Delibera prodotto";
            r.RICHIE_TITOLO = "Richiesta delibera prodotto [" + p.PRODOT_CODICE + "]";
            r.RICHIE_PRODOT_ID = p.PRODOT_ID;
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
        }
        private RICHIE_RICHIESTE creaRichiestaSbloccoProdotto(PRODOT_PRODOTTI p, string motivo, int priorita, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = getDestinatarioUserID(p, "CDG", en);
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 2; // attribuzione
            r.RICHIE_T_RICPRI_ID = priorita;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = motivo;
            r.RICHIE_TITOLO = "Richiesta sblocco prodotto[" + p.PRODOT_CODICE +"]";
            r.RICHIE_PRODOT_ID = p.PRODOT_ID;
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
        }
        private RICHIE_RICHIESTE creaRichiestaRispostaSblocco(PRODOT_PRODOTTI p, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = p.PRODOT_UTENTE_ID;
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 2; // attribuzione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = "Il prodotto [" + p.PRODOT_CODICE + "] risulta sbloccato.";
            r.RICHIE_TITOLO = "Sblocco prodotto [" + p.PRODOT_CODICE + "]";
            r.RICHIE_PRODOT_ID = p.PRODOT_ID ;
            r.RICHIE_FLG_ASSEGN_AL_GRUPPO = p.PRODOT_FLG_ASSEGN_AL_GRUPPO;
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
        }

        public ActionResult ProdottoWorkflow(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Workflow prodotto";
            ProdottoWorkflowModel an = new ProdottoWorkflowModel(id);
            return View(an);
        }
        public ProdottoController()
            : base()
        {
            this.SetMessage = "Prodotti";
            this.SetLiProdotto = "current";
            
        }

        public string SetLitagbg1(string color)
        {
            string s = "tag " + color + "-bg";

            return s;
        }


        public ActionResult Index(int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) 
            {
                return r;
            }

            this.SetMessage = "Prodotti";
            ////string rangeID = TempData["NumEntities"] as string;
            ////string table_search1 = TempData["table_search"] as string;
            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            ListaIndexProdottoModel p = new ListaIndexProdottoModel();
            //if (NumEntities.HasValue)
            //    g.NumEntities = NumEntities.Value;
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                p.NumEntities = numEntities;
            }

            if (table_search1 != null)
                p.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                p.CurrentPage = pageNum;
            }
            else
                p.CurrentPage = 1;

            p.FiltroStato = 0;
            List<MyProdotto> listProdotti = p.GetElencoProdotti(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Prodot_Codice).ToList<MyProdotto>();

            p.Data = listProdotti.Take(p.NumEntities).ToList();
            p.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listProdotti.Count() / p.NumEntities));
            p.CurrentPage = 1;

            return View(p);
            
        }

        [HttpPost]
        public ActionResult Index(MyPaginAjax mpa)
        {
            this.SetMessage = "Prodotti";
            ListaIndexProdottoModel p = new ListaIndexProdottoModel();
            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
            //         ViewBag.NumEntities = mpa.NumEntities.ToString();
            //         ViewBag.table_search = mpa.SearchDescription.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;

            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    p.CurrentPage = mpa.CurrentPage.Value;
                else
                    p.CurrentPage = 1;
            }
            else
                p.CurrentPage = 1;

            p.FiltroStato = mpa.FiltroStato;
            p.NumEntities = mpa.NumEntities;
            p.SearchDescription = mpa.SearchDescription;
            List<MyProdotto> listProdotti = p.GetElencoProdotti(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Prodot_Codice).ToList<MyProdotto>();

            p.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listProdotti.Count() / p.NumEntities));
            if (p.CurrentPage > p.NumberOfPages) p.CurrentPage = p.NumberOfPages;
            p.Data = listProdotti.OrderBy(z => z.Prodot_Codice).Skip(p.NumEntities * (p.CurrentPage - 1)).Take(p.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_Index", p) });
        }


        public ActionResult ProdottoEdit(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Dettaglio prodotto";
            ProdottoModel an = new ProdottoModel(id);

            //if (an.Prodotto.Prodot_T_Stapro_Id == 2) //In valorizzazione
            //{
            //    if (IsEditable(an))
            //    {
            //        attualizzaPosizioniProdotto(id);
            //        saveValProdottoTot(an);

            //        an = new ProdottoModel(id);
            //    }
            //}

            return View(an);
        }

        //sim: tengo traccia di quando è stato effettuato l'ultimo aggiorna posizioni
        public void TrackingAggiornaPosizioni(int prodot_id)
        {
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == prodot_id).SingleOrDefault();
            p.PRODOT_TS_AGGIORNAMENTO_POSIZIONI = DateTime.Now;
            en.SaveChanges();
        }

        [HttpPost]
        public ActionResult AttualizzaPosizioni(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();

                if (p.T_STAPRO_STATO_PRODOTTO.T_STAPRO_ID == 2) //Controllo lo stato
                {
                    attualizzaPosizioniProdotto(p);
                    TrackingAggiornaPosizioni(p.PRODOT_ID);
                    saveValProdottoTot(pr, out er);
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }

        public ActionResult PopUpRichiediSbloccoProdotto(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int prodotto_id = id;
            ProdottoModelSblocco am = new ProdottoModelSblocco(prodotto_id);
            return View(am);
        }
        private void salvaTrackingProdotto(PRODOT_PRODOTTI pr, IZSLER_CAP_Entities en, DateTime dt)
        {
            TRKPRO_PRODOTTO_TRACKING trk = new TRKPRO_PRODOTTO_TRACKING();
            trk.TRKPRO_CODICE  = pr.PRODOT_CODICE ;
            trk.TRKPRO_COSTOUNITARIO = pr.PRODOT_COSTOUNITARIO ;
            trk.TRKPRO_COSTOUNITARIO_DELIBE = pr.PRODOT_COSTOUNITARIO_DELIBE;
            trk.TRKPRO_DATA_INS = dt;
            trk.TRKPRO_DESC = pr.PRODOT_DESC;
            trk.TRKPRO_DIM_LOTTO = pr.PRODOT_DIM_LOTTO.Value;
            trk.TRKPRO_FLG_INTERNO = pr.PRODOT_FLG_INTERNO;
            trk.TRKPRO_PRODOT_ID = pr.PRODOT_ID;
            trk.TRKPRO_NR_CAMP_QUALITA = pr.PRODOT_NR_CAMP_QUALITA;
            trk.TRKPRO_PRODOT_UTENTE_ID = pr.PRODOT_UTENTE_ID.Value;
            trk.TRKPRO_REPARTO_GRUREP_ID = pr.PRODOT_REPARTO_GRUREP_ID;
            trk.TRKPRO_T_STAPRO_ID = pr.PRODOT_T_STAPRO_ID.Value ;
            trk.TRKPRO_T_UNIMIS_ID = pr.PRODOT_T_UNIMIS_ID;
            trk.TRKPRO_UTENTE_ID = this.CurrentUserID;
            trk.TRKPRO_FLG_BLOCCATO_MAGAZZINO = pr.PRODOT_FLG_BLOCCATO_MAGAZZINO;
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
            salvaTrackingProdottoPos(pr.PRODOT_ID, dt, en);
        }
        private void salvaTrackingProdottoPos(int prodot_id, DateTime dt, IZSLER_CAP_Entities en)
        {
            List<PROPOS_POSIZIONI> lstPos = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_PRODOT_ID == prodot_id).ToList<PROPOS_POSIZIONI>();
            foreach (PROPOS_POSIZIONI pos in lstPos)
            {
                trakingProdottoPos(pos, dt, en);
            }
        }
        private void trakingProdottoPos(PROPOS_POSIZIONI pos, DateTime dt, IZSLER_CAP_Entities en)
        {
            TRKPPS_PRODOTPOS_TRACKING trkp = new TRKPPS_PRODOTPOS_TRACKING();

            trkp.TRKPPS_PROPOS_COEFF_CONVERSIONE = pos.PROPOS_COEFF_CONVERSIONE;
            trkp.TRKPPS_PROPOS_COSTO_QTA = pos.PROPOS_QTA ;
            trkp.TRKPPS_PROPOS_DESC = pos.PROPOS_DESC;
            trkp.TRKPPS_PROPOS_FASE_ID = pos.PROPOS_FASE_ID;
            trkp.TRKPPS_PROPOS_FIGPRO_ID = pos.PROPOS_FIGPRO_ID;
            trkp.TRKPPS_PROPOS_ID = pos.PROPOS_ID;
            trkp.TRKPPS_PROPOS_INTERM_ID = pos.PROPOS_INTERM_ID;
            trkp.TRKPPS_PROPOS_PRODOT_ID = pos.PROPOS_PRODOT_ID;
            trkp.TRKPPS_PROPOS_PRODOT_ID_SEC = pos.PROPOS_PRODOT_ID_SEC;
            trkp.TRKPPS_PROPOS_QTA = pos.PROPOS_QTA;
            trkp.TRKPPS_PROPOS_T_UNIMIS_ID = pos.PROPOS_T_UNIMIS_ID;
            trkp.TRKPPS_PROPOS_TOT = pos.PROPOS_T_UNIMIS_ID;
            trkp.TRKVAL_DATA_INS = dt;
            trkp.TRKVAL_UTENTE_ID = this.CurrentUserID;
            trkp.TRKPPS_PROPOS_COD_SETTORE = pos.PROPOS_COD_SETTORE;
            trkp.TRKVPS_PROPOS_MACCHI_ID = pos.PROPOS_MACCHI_ID;

            en.TRKPPS_PRODOTPOS_TRACKING.AddObject(trkp);
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
        [HttpPost]
        public ActionResult RichiediSbloccoProdotto(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();

                T_STAPRO_STATO_PRODOTTO tStapro = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "INSBLO").SingleOrDefault();
                chiudiRichiestaCorrente(p.PRODOT_ID , en);
                p.PRODOT_T_STAPRO_ID = tStapro.T_STAPRO_ID;
                RICHIE_RICHIESTE r = creaRichiestaSbloccoProdotto(p, pr.Prodotto_Motivo, pr.Prodotto_T_RICPRI_ID, en, dt);
                if(r.RICHIE_DESTINATARIO_UTENTE_ID >0)
                {
                    salvaTrackingProdotto(p, en, dt);
                    en.SaveChanges();
                    inviaEmail(r);
                    flagOK = true;
                }
                else 
                {
                    flagOK = false;
                    er = getErroreDestinatarioUserID(p, "CDG", en);
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        [HttpPost]
        public ActionResult SbloccaProdotto(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();
                T_STAPRO_STATO_PRODOTTO tStapro = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "INVAL").SingleOrDefault();
                chiudiRichiestaCorrente(p.PRODOT_ID, en);
                p.PRODOT_T_STAPRO_ID = tStapro.T_STAPRO_ID;
                p.PRODOT_FLG_BLOCCATO = false;
                RICHIE_RICHIESTE r = creaRichiestaRispostaSblocco(p, en, dt);
                salvaTrackingProdotto(p, en, dt);
                en.SaveChanges();
                inviaEmail(r);
                flagOK = true;
                attualizzaPosizioniProdotto(p);
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }

        //private void attualizzaPosizioniProdotto(int prodot_id)
        //{
        //    IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
        //    PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == prodot_id).SingleOrDefault();
        //    AttualizzatorePosizioni a = new AttualizzatorePosizioni(p);
        //    a.Attualizza();
        //}

        private void attualizzaPosizioniProdotto(PRODOT_PRODOTTI p)
        {
            AttualizzatorePosizioni a = new AttualizzatorePosizioni(p);
            a.Attualizza();
        }

        [HttpPost]
        public ActionResult AddNewValPos(MyProdottoPosAjax prodpos)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PROPOS_POSIZIONI pp = new PROPOS_POSIZIONI();
                
                pp.PROPOS_PRODOT_ID = prodpos.ProdottoPos_MasterProdotto_id ;
                pp.PROPOS_COEFF_CONVERSIONE = 1;

                en.PROPOS_POSIZIONI.AddObject(pp);
                en.SaveChanges();
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        [HttpPost]
        public ActionResult DeleteAllPos(MyProdottoPosAjax prodpos)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

                List<PROPOS_POSIZIONI> lstDelete = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_PRODOT_ID == prodpos.ProdottoPos_MasterProdotto_id).ToList<PROPOS_POSIZIONI>();
                foreach (PROPOS_POSIZIONI vp in lstDelete)
                {
                    en.PROPOS_POSIZIONI.DeleteObject(vp);
                }
                en.SaveChanges();
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        [HttpPost]
        public ActionResult DeleteSinglePos(MyProdottoPosAjaxList prodPosId)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                if (prodPosId != null && prodPosId.ProdottoPosIds!=null)
                {
                    foreach (int posId in prodPosId.ProdottoPosIds)
                    {
                        List<PROPOS_POSIZIONI> lstDelete = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_ID == posId).ToList<PROPOS_POSIZIONI>();
                        foreach (PROPOS_POSIZIONI vp in lstDelete)
                        {
                            en.PROPOS_POSIZIONI.DeleteObject(vp);
                        }
                        en.SaveChanges();
                    }
                }
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        private string getCostoInd(MyAnalisi analisi)
        {
            try
            {
                LoadEntities l_le = new LoadEntities();
                string ret = l_le.GetGruppo(analisi.Analisi_Gruppo_id.Value).Grurep_Cost_Ind;
                if (ret == null) return "0";
                return ret;
            }
            catch { }
            return "0";
        }

        private decimal getImportoCostoIndustriale(MyAnalisi analisi)
        {
            try
            {
                decimal cind = decimal.Parse(getCostoInd(analisi), System.Globalization.CultureInfo.InvariantCulture);
                if (analisi.Analisi_CostoTot.HasValue)
                    return analisi.Analisi_CostoTot.Value * (1 + cind);
            }
            catch { }
            return 0;
        }

        private string getCostoInd(MyProdotto prodotto)
        {
            try
            {
                LoadEntities l_le = new LoadEntities();
                string ret = l_le.GetReparto(prodotto.Prodot_Reparto_ID.Value).Grurep_Cost_Ind;
                if (ret == null) return "0";
                return ret;
            }
            catch { }
            return "0";
        }
        private decimal getImportoCostoIndustriale(MyProdotto prodotto)
        {
            try
            {
                decimal cind = decimal.Parse(getCostoInd(prodotto), System.Globalization.CultureInfo.InvariantCulture);
                if (prodotto.Prodot_CostoUnitario.HasValue)
                    return prodotto.Prodot_CostoUnitario.Value * (1 + cind);
            }
            catch { }
            return 0;
        }

        private decimal? getCosto(PROPOS_POSIZIONI currPos, PRODOT_PRODOTTI p)
        {
            decimal? costoDelib = null;
//            string costoInd = "0";
            LoadEntities le = new LoadEntities();
            MyProdotto lProdottoMaster = le.GetProdotti(p.PRODOT_ID);
            //costoInd = le.GetCostoInd(lProdottoMaster);

            if (currPos.PROPOS_INTERM_ID.HasValue)
            {
                //MyAnalisi lAnalisi = le.GetAnalisi(currPos.PROPOS_INTERM_ID.Value);
                //if (lAnalisi.Analisi_Reparto_id .HasValue && lProdottoMaster.Prodot_Reparto_ID.Value == lAnalisi.Analisi_Reparto_id.Value  ) // Caso intermedio Prodotto
                //{ costoInd = "0"; }

                //costoDelib = lAnalisi.Analisi_CostoTotDelib;

                MyAnalisi analisi = le.GetAnalisi(currPos.PROPOS_INTERM_ID.Value);
                string pricePos_Value = le.GetSettings("PRICE_POS");

                switch (pricePos_Value)
                {
                    case "2":// Costo Industriale
                        costoDelib = getImportoCostoIndustriale(analisi);
                        break;
                    case "1":
                        //ret = analisi.Analisi_CostoTariffaDelib; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro all'analisi

                        if (analisi.Analisi_flgIntermedio == true)
                        {
                            costoDelib = analisi.Analisi_CostoTotDelib;
                        }
                        else
                        {
                            if (currPos.PROPOS_COD_SETTORE == "D")
                                costoDelib = analisi.Analisi_CostoTariffa_D_Delib;
                            else
                                costoDelib = analisi.Analisi_CostoTariffaDelib;
                        }
                        break;

                    default:
                        //if (flgMatch)    // se il Gruppo Reparto dell'analisiIntermedio Master coincide con quello dell'analisiCorrente 
                        if (true)
                        {                // metto il solo costo diretto unitario
                            costoDelib = analisi.Analisi_CostoTotDelib;
                        }
                        else
                        {
                            decimal costoInd = 0;
                            if (analisi.Analisi_PercCostInd != null)
                            {
                                decimal.TryParse(analisi.Analisi_PercCostInd.Replace(".", ","), out costoInd);
                            }
                            //decimal.TryParse(m_costoInd.Replace(".",",") , out costoInd);
                            costoDelib = analisi.Analisi_CostoTotDelib * (1 + costoInd);
                        }
                        break;
                }
            }
            if (currPos.PROPOS_PRODOT_ID_SEC.HasValue)
            {
                MyProdotto prodotto = le.GetProdotti(currPos.PROPOS_PRODOT_ID_SEC.Value);
                
                //costoDelib = lprod.Prodot_CostoUnitario_Deliberato;

                bool flgMatch = false;
                int masterReparto_id = 0;
                if (lProdottoMaster.Prodot_Reparto_ID.HasValue)
                {
                    masterReparto_id = lProdottoMaster.Prodot_Reparto_ID.Value;
                }

                if (masterReparto_id > 0) //controllo il reparto del "prodotto" parametro
                {
                    if (prodotto.Prodot_Reparto_ID.HasValue)
                    {
                        if (prodotto.Prodot_Reparto_ID.Value == masterReparto_id)
                        {
                            flgMatch = true;
                        }
                    }
                }

                string pricePos_Value = le.GetSettings("PRICE_POS");// +
                switch (pricePos_Value)
                {
                    case "2":// Costo Industriale
                        costoDelib = getImportoCostoIndustriale(prodotto);
                        break;
                    case "1":
                        if (flgMatch)
                        {
                            costoDelib = prodotto.Prodot_CostoUnitario_Deliberato;
                        }
                        else
                        {
                            decimal costoInd = 0;
                            // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                            if (prodotto.Prodot_PercCostInd != null)
                            { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                            costoDelib = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                        }
                        break;
                    default:
                        {

                            //if (flgMatch)    // se il Gruppo Reparto del prodotto Master coincide con quello del prodotto Corrente 
                            if (true)
                            {                // metto il solo costo diretto unitario
                                costoDelib = prodotto.Prodot_CostoUnitario_Deliberato;
                            }
                            else
                            {
                                decimal costoInd = 0;
                                // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                                if (prodotto.Prodot_PercCostInd != null)
                                { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                                costoDelib = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                            }
                        }
                        break;
                }
            }
            if (currPos.PROPOS_MACCHI_ID.HasValue)
            {
                
                MyMacchinario lMacc = le.GetMacchinario(currPos.PROPOS_MACCHI_ID.Value);
                costoDelib = lMacc.Macchi_Prezzo;
            }
            if (currPos.PROPOS_FIGPRO_ID.HasValue)
            {
                MyFigProf f = le.GetFigProfDaFigProf_ID(currPos.PROPOS_FIGPRO_ID.Value);
                costoDelib = f.FigProf_Costo;
            }



            decimal coeff = 1;
            decimal dcostoInd = 0;
            //decimal.TryParse(costoInd.Replace(".", ","), out dcostoInd);
            coeff = (1 + dcostoInd);


            // gestione caso "Accettazione"                      
            //if (currPos.PROPOS_FASE_ID == 1) 
            if (isAccettazione(currPos.PROPOS_FASE_ID))
            {
                if (p.PRODOT_REPARTO_GRUREP_ID.HasValue)
                {
                    MyGrurep mgr = le.GetGruppo(p.PRODOT_REPARTO_GRUREP_ID.Value);
                    decimal val = 0;
                    if (mgr.Grurep_PrezzoUnit_Accettazione.HasValue)
                    {
                        val = mgr.Grurep_PrezzoUnit_Accettazione.Value;
                        costoDelib = val;
                    }

                }
            }

            // se il Gruppo Reparto dell'analisiIntermedio Master coincide con quello dell'analisiCorrente 
            // metto il solo costo diretto unitario
            //ret = costoDelib * coeff;

            decimal? ret = null;
            //Non più: si è deciso di usare sempre il costo diretto
            ret = costoDelib;

            if (ret.HasValue)
            {
                ret = decimal.Round(ret.Value, 2, MidpointRounding.AwayFromZero);
            }
            else
            { ret = 0; }

            return ret;
        }
        [HttpPost]
        public ActionResult ApplicaModelloAProdotto(MyModelloAjax mpa)
        {
            bool flagOK = true;
            string er = "";
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == mpa.MasterID).SingleOrDefault();
                List<VALPOS_POSIZIONI> posModello = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == mpa.Modello_ID).ToList<VALPOS_POSIZIONI>();
                List<PROPOS_POSIZIONI> newListPos = new List<PROPOS_POSIZIONI>();
                foreach (VALPOS_POSIZIONI pos in posModello)
                {
                    PROPOS_POSIZIONI currPos = clonaPosizione(pos);
                    currPos.PROPOS_COSTO_QTA = getCosto(currPos, p);
                    // se Fase== Accettazione --> allora l'unità di misura e' Min
                    //if (pos.VALPOS_FASE_ID == 1)
                    if (isAccettazione(pos.VALPOS_FASE_ID))
                    {
                        //currPos.PROPOS_T_UNIMIS_ID = 13; // UdM min
                        currPos.PROPOS_T_UNIMIS_ID = 25; // UdM Numero
                    }
                    currPos.PROPOS_TOT = decimal.Round((currPos.PROPOS_QTA * currPos.PROPOS_COSTO_QTA.Value), 2, MidpointRounding.AwayFromZero);
                    currPos.PROPOS_PRODOT_ID = mpa.MasterID;
                    newListPos.Add(currPos);
                }
                foreach (PROPOS_POSIZIONI newPos in newListPos)
                {
                    en.PROPOS_POSIZIONI.AddObject(newPos);
                    en.SaveChanges();
                }

                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
       
        [HttpPost]
        public ActionResult PPCopiaModelloProdotto(MyPaginAjax mpa)
        {
            PPModel ppModel = new PPModel(mpa.id, mpa.sec, mpa.p);

            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;

            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    ppModel.CurrentPage = mpa.CurrentPage.Value;
                else
                    ppModel.CurrentPage = 1;
            }
            else
                ppModel.CurrentPage = 1;

            ppModel.NumEntities = mpa.NumEntities;
            ppModel.SearchDescription = mpa.SearchDescription;

            ppModel.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)ppModel.ElencoModelli.Count() / ppModel.NumEntities));
            if (ppModel.CurrentPage > ppModel.NumberOfPages) ppModel.CurrentPage = ppModel.NumberOfPages;
            ppModel.Data = ppModel.ElencoModelli.OrderBy(z => z.Analisi_Descrizione).Skip(ppModel.NumEntities * (ppModel.CurrentPage - 1)).Take(ppModel.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPCopiaModelloAnalisi", ppModel) });
        }
        [HttpGet]
        public ActionResult PPCopiaModelloProdotto(int id, int sec, int p)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }



            this.SetMessage = "Settings - Gruppi";
            ////string rangeID = TempData["NumEntities"] as string;
            ////string table_search1 = TempData["table_search"] as string;
            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            PPModel ppModel = new PPModel(id, sec, p);
            //if (NumEntities.HasValue)
            //    g.NumEntities = NumEntities.Value;
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                ppModel.NumEntities = numEntities;
            }

            if (table_search1 != null)
                ppModel.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                ppModel.CurrentPage = pageNum;
            }
            else
                ppModel.CurrentPage = 1;

            ppModel.Data = ppModel.ElencoModelli.OrderBy(z => z.Analisi_Descrizione).Take(ppModel.NumEntities).ToList();
            ppModel.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)ppModel.ElencoModelli.Count() / ppModel.NumEntities));
            ppModel.CurrentPage = 1;

            return View(ppModel);
        }
        private PROPOS_POSIZIONI clonaPosizione(VALPOS_POSIZIONI objSrc)
        {
            PROPOS_POSIZIONI newObj = new PROPOS_POSIZIONI();
            newObj.PROPOS_COEFF_CONVERSIONE = objSrc.VALPOS_COEFF_CONVERSIONE;
            newObj.PROPOS_COSTO_QTA = objSrc.VALPOS_COSTO_QTA;
            newObj.PROPOS_DESC = objSrc.VALPOS_DESC;
            newObj.PROPOS_FASE_ID = objSrc.VALPOS_FASE_ID;
            newObj.PROPOS_FIGPRO_ID = objSrc.VALPOS_FIGPRO_ID;
            newObj.PROPOS_INTERM_ID = objSrc.VALPOS_INTERM_ID;
            newObj.PROPOS_PRODOT_ID_SEC = objSrc.VALPOS_PRODOT_ID;
            newObj.PROPOS_QTA = objSrc.VALPOS_QTA;
            newObj.PROPOS_TOT = objSrc.VALPOS_TOT;
            newObj.PROPOS_T_UNIMIS_ID = objSrc.VALPOS_T_UNIMIS_ID;
            newObj.PROPOS_MACCHI_ID = objSrc.VALPOS_MACCHI_ID;
            newObj.PROPOS_COD_SETTORE = objSrc.VALPOS_COD_SETTORE;
            return newObj;
        }
        private PROPOS_POSIZIONI clonaPosizione(PROPOS_POSIZIONI objSrc)
        {
            PROPOS_POSIZIONI newObj = new PROPOS_POSIZIONI();

            newObj.PROPOS_COEFF_CONVERSIONE = objSrc.PROPOS_COEFF_CONVERSIONE;
            newObj.PROPOS_COSTO_QTA = objSrc.PROPOS_COSTO_QTA;
            newObj.PROPOS_DESC = objSrc.PROPOS_DESC;
            newObj.PROPOS_FASE_ID = objSrc.PROPOS_FASE_ID;
            newObj.PROPOS_FIGPRO_ID = objSrc.PROPOS_FIGPRO_ID;
            newObj.PROPOS_INTERM_ID = objSrc.PROPOS_INTERM_ID;
            newObj.PROPOS_PRODOT_ID_SEC = objSrc.PROPOS_PRODOT_ID_SEC;
            newObj.PROPOS_QTA = objSrc.PROPOS_QTA;
            newObj.PROPOS_TOT = objSrc.PROPOS_TOT;
            newObj.PROPOS_T_UNIMIS_ID = objSrc.PROPOS_T_UNIMIS_ID;
            newObj.PROPOS_PRODOT_ID = objSrc.PROPOS_PRODOT_ID;
            newObj.PROPOS_MACCHI_ID = objSrc.PROPOS_MACCHI_ID;
            newObj.PROPOS_COD_SETTORE = objSrc.PROPOS_COD_SETTORE;
            return newObj;
        }
        [HttpPost]
        public ActionResult ClonaPosizione(MyProdottoPosAjaxList prodPosId)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                if (prodPosId != null && prodPosId.ProdottoPosIds!=null)
                {
                    foreach (int posId in prodPosId.ProdottoPosIds)
                    {

                        List<PROPOS_POSIZIONI> lstSrcObjects = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_ID == posId).ToList<PROPOS_POSIZIONI>();
                        foreach (PROPOS_POSIZIONI pp in lstSrcObjects)
                        {
                            PROPOS_POSIZIONI lpp = clonaPosizione(pp);
                            en.PROPOS_POSIZIONI.AddObject(lpp);
                        }
                        en.SaveChanges();
                    }
                }
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetImportoFaseAccettazione(string prodotto_id, string Attivi_id, string valpos_id)
        {
            if (Attivi_id == "") Attivi_id = "0";
            if (valpos_id == "") valpos_id = "0";
            if (prodotto_id == "") prodotto_id = "0";
            ProdottoModel f = new ProdottoModel(int.Parse(prodotto_id));
            LoadEntities le = new LoadEntities();
            MyGrurep mgr = le.GetReparto(f.Prodotto.Prodot_Reparto_ID.Value);

            decimal val = 0;
            if (mgr.Grurep_PrezzoUnit_Accettazione.HasValue)
            { val = mgr.Grurep_PrezzoUnit_Accettazione.Value; }
            string lret = "";
            lret = val.ToString("0.00000");
            lret = lret.Replace(".", ",");
            if (lret == "") lret = "0,00000";
            List<SelectListItem> lst = new List<SelectListItem>();

            SelectListItem sliVoid = new SelectListItem();

            sliVoid.Text = lret;
            sliVoid.Value = mgr.Grurep_ID.ToString();

            lst.Add(sliVoid);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetElencoFigProfessionali(string prodotto_id, string Attivi_id, string valpos_id)
        {
            if (Attivi_id == "") Attivi_id = "0";
            if (valpos_id == "") valpos_id = "0";
            if (prodotto_id == "") prodotto_id = "0";
            ProdottoModel f = new ProdottoModel(int.Parse(prodotto_id));
            MyProdottoPos pos = f.ElencoProdottoPos.Where(z => z.ProdottoPos_id == int.Parse(valpos_id)).SingleOrDefault();
            int fase_id = 0;
            try
            {
                fase_id = int.Parse(Attivi_id);
            }
            catch { }
            if (fase_id > 0)
            {
                pos.ProdottoPos_ListaFigProf = f.GetFigProf(fase_id);
            }

            return Json(pos.ProdottoPos_ListaFigProfSL, JsonRequestBehavior.AllowGet);
            //  return Json(f.ListaFigProf , JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveValPos(MyProdottoPosAjax propos)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PROPOS_POSIZIONI pp = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_ID == propos.ProdottoPos_id).SingleOrDefault();
                TipoSave l = (TipoSave)Enum.Parse(typeof(TipoSave), propos.TipoSalvataggio);
                switch (l)
                {

                    case TipoSave.UdMRatio:
                        string convFlg = propos.ProdottoPos_CoeffConversioneString;
                        try
                        {
                            double valConversione = double.Parse(convFlg);
                            pp.PROPOS_COEFF_CONVERSIONE = valConversione;
                        }
                        catch
                        { }

                        break;
                    case TipoSave.PulisciCosto:
                        pp.PROPOS_COSTO_QTA = null;
                        break;
                    case TipoSave.Macchinario:
                        if (propos.ProdottoPos_Macchinario_id == 0)
                            pp.PROPOS_MACCHI_ID = null;
                        else
                            pp.PROPOS_MACCHI_ID = propos.ProdottoPos_Macchinario_id;

                        if (propos.ProdottoPos_QuantitaCosto == 0)
                            pp.PROPOS_COSTO_QTA = null;
                        else
                            pp.PROPOS_COSTO_QTA = propos.ProdottoPos_QuantitaCosto;

                        pp.PROPOS_COEFF_CONVERSIONE = 1;
                        // pulizia livello fig prof
                        pp.PROPOS_FIGPRO_ID = null;
                        //pulizia Prodotto
                        pp.PROPOS_PRODOT_ID_SEC = null;
                        // pulizia IntermedioAnalisi
                        pp.PROPOS_INTERM_ID = null;
                        // UdM minuto
                        pp.PROPOS_T_UNIMIS_ID = 13; // UdM minuto    

                        break;
                    case TipoSave.AnalisiIntermedio:
                        if (propos.ProdottoPos_Analisi_id == 0)
                            pp.PROPOS_INTERM_ID = null;
                        else
                            pp.PROPOS_INTERM_ID = propos.ProdottoPos_Analisi_id;

                        if (propos.ProdottoPos_QuantitaCosto == 0)
                            pp.PROPOS_COSTO_QTA = null;
                        else
                            pp.PROPOS_COSTO_QTA = propos.ProdottoPos_QuantitaCosto;

                        pp.PROPOS_COEFF_CONVERSIONE = 1;
                        // pulizia livello fig prof
                        pp.PROPOS_FIGPRO_ID = null;
                        // pulizia Prodotto
                        pp.PROPOS_PRODOT_ID_SEC = null;
                        // pulizia Macchinario
                        pp.PROPOS_MACCHI_ID = null;
                        // UdM Numero
                        pp.PROPOS_T_UNIMIS_ID = 25; // UdM Numero    

                        pp.PROPOS_COD_SETTORE = propos.ProdottoPos_CodSettore;
                        break;
                    case TipoSave.FaseAccettazione:
                        if (propos.ProdottoPos_Fase_id == 0)
                            pp.PROPOS_FASE_ID = null;
                        else
                            pp.PROPOS_FASE_ID = propos.ProdottoPos_Fase_id;

                        if (propos.ProdottoPos_QuantitaCosto == 0)
                            pp.PROPOS_COSTO_QTA = null;
                        else
                            pp.PROPOS_COSTO_QTA = propos.ProdottoPos_QuantitaCosto;

                        pp.PROPOS_COEFF_CONVERSIONE = 1;
                        // pulizia Intermedio
                        pp.PROPOS_INTERM_ID = null;
                        // pulizia livello fig prof
                        pp.PROPOS_FIGPRO_ID = null;
                        // pulizia Prodotto
                        pp.PROPOS_PRODOT_ID_SEC = null;
                        // pulizia Macchinario
                        pp.PROPOS_MACCHI_ID = null;

                        //pp.PROPOS_T_UNIMIS_ID = 13; // minuti
                        pp.PROPOS_T_UNIMIS_ID = 25; // UdM Numero
                        break;
                    case TipoSave.Fase:
                        if (propos.ProdottoPos_Fase_id == 0)
                            pp.PROPOS_FASE_ID = null;
                        else
                            pp.PROPOS_FASE_ID = propos.ProdottoPos_Fase_id;
                        pp.PROPOS_FIGPRO_ID = null;

                        if (propos.ProdottoPos_UdM_id == 0)
                            pp.PROPOS_T_UNIMIS_ID = null;
                        else
                            pp.PROPOS_T_UNIMIS_ID = propos.ProdottoPos_UdM_id;
                        //    vp.VALPOS_T_UNIMIS_ID = 13; // minuti
                        break;

                    case TipoSave.Descrizione:
                        //if (anapos.AnalisiPos_desc == "")
                        //    vp.VALPOS_DESC = null;
                        //else
                        pp.PROPOS_DESC = propos.ProdottoPos_desc;
                        break;

                    case TipoSave.Quantita:
                        pp.PROPOS_QTA = propos.ProdottoPos_Quantita;
                        break;

                    case TipoSave.Livello:
                        if (propos.ProdottoPos_FigProf_id == 0)
                            pp.PROPOS_FIGPRO_ID = null;
                        else
                            pp.PROPOS_FIGPRO_ID = propos.ProdottoPos_FigProf_id;
                        if (propos.ProdottoPos_QuantitaCosto == 0)
                            pp.PROPOS_COSTO_QTA = null;
                        else
                            pp.PROPOS_COSTO_QTA = propos.ProdottoPos_QuantitaCosto;

                        pp.PROPOS_COEFF_CONVERSIONE = 1;
                        // pulizia IntermedioAnalisi
                        pp.PROPOS_INTERM_ID = null;
                        // pulizia Prodotto
                        pp.PROPOS_PRODOT_ID_SEC = null;
                        // pulizia Macchinario
                        pp.PROPOS_MACCHI_ID = null;
                        if (propos.ProdottoPos_UdM_id == 0)
                            pp.PROPOS_T_UNIMIS_ID = null;
                        else
                            pp.PROPOS_T_UNIMIS_ID = propos.ProdottoPos_UdM_id;
                        break;

                    case TipoSave.Prodotto:
                        if (propos.ProdottoPos_Prodotto_id == 0)
                            pp.PROPOS_PRODOT_ID_SEC = null;
                        else
                            pp.PROPOS_PRODOT_ID_SEC = propos.ProdottoPos_Prodotto_id;
                        if (propos.ProdottoPos_QuantitaCosto == 0)
                            pp.PROPOS_COSTO_QTA = null;
                        else
                            pp.PROPOS_COSTO_QTA = propos.ProdottoPos_QuantitaCosto;


                        string convFlgProd = propos.ProdottoPos_CoeffConversioneString;
                        if (convFlgProd != null && convFlgProd != "")
                        {
                            try
                            {
                                double valConversione = double.Parse(convFlgProd);
                                pp.PROPOS_COEFF_CONVERSIONE = valConversione;
                            }
                            catch
                            { }
                        }
                        else
                            pp.PROPOS_COEFF_CONVERSIONE = null;

                        // pulizia IntermedioAnalisi
                        pp.PROPOS_INTERM_ID = null;
                        // pulizia Figura professionale Livello
                        pp.PROPOS_FIGPRO_ID = null;
                        // pulizia Macchinario
                        pp.PROPOS_MACCHI_ID = null;
                        break;

                    case TipoSave.PrezzoPosizione:
                        pp.PROPOS_TOT = propos.ProdottoPos_QuantitaCosto;
                        break;

                    case TipoSave.UdM:
                        if (propos.ProdottoPos_UdM_id == 0)
                            pp.PROPOS_T_UNIMIS_ID = null;
                        else
                            pp.PROPOS_T_UNIMIS_ID = propos.ProdottoPos_UdM_id;

                        if (propos.ProdottoPos_CoeffConversioneString == "1")
                            pp.PROPOS_COEFF_CONVERSIONE = 1;
                        if (propos.ProdottoPos_CoeffConversioneString == "clear")
                            pp.PROPOS_COEFF_CONVERSIONE = null;
                        if (propos.ProdottoPos_CoeffConversioneString != "1" && propos.ProdottoPos_CoeffConversioneString != "clear")
                        {
                            string convFlgProd2 = propos.ProdottoPos_CoeffConversioneString;
                            if (convFlgProd2 != null && convFlgProd2 != "")
                            {
                                try
                                {
                                    double valConversione2 = double.Parse(convFlgProd2);
                                    pp.PROPOS_COEFF_CONVERSIONE = valConversione2;
                                }
                                catch
                                { }
                            }
                        }

                        break;
                }
                double res = 0;
                if (pp.PROPOS_COSTO_QTA.HasValue && pp.PROPOS_COEFF_CONVERSIONE.HasValue)
                {
                    res = ((double)(pp.PROPOS_COSTO_QTA.Value * pp.PROPOS_QTA)) * pp.PROPOS_COEFF_CONVERSIONE.Value;
                    pp.PROPOS_TOT = (decimal)res;
                }
                else
                    pp.PROPOS_TOT = null;
                en.SaveChanges();
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetElencoChart(string prodotto_id)
        {
           int prod_ID = int.Parse(prodotto_id);
           ProdottoModel p = new ProdottoModel(prod_ID);
           return Json( p.GetDataChart(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public FileStreamResult Report(string info)
        {

            string err = string.Empty;
            string url = info;

            try
            {
                //string path = Request.Url.AbsoluteUri.Replace(Request.Url.Query, "").Replace(Request.Url.AbsolutePath, "");
                //url = path + url;

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.PreAuthenticate = true;
                NetworkCredential cred = CredentialCache.DefaultNetworkCredentials;
                req.Credentials = cred;

                using (HttpWebResponse response = req.GetResponse() as HttpWebResponse)
                {

                    MemoryStream memStream;
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        memStream = new MemoryStream();

                        byte[] buffer = new byte[1024];
                        int byteCount;
                        do
                        {
                            byteCount = responseStream.Read(buffer, 0, buffer.Length);
                            memStream.Write(buffer, 0, byteCount);
                        } while (byteCount > 0);
                    }
                    memStream.Seek(0, SeekOrigin.Begin);

                    Response.Clear();
                    Response.AddHeader("Accept-Header", memStream.Length.ToString());
                    Response.ContentType = "application/pdf";
                    Response.OutputStream.Write(memStream.ToArray(), 0, Convert.ToInt32(memStream.Length));
                    Response.Flush();
                    try { Response.End(); }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                err += ex.Message;
            }
            return null;
        }

        public bool saveValProdottoTot(ProdottoModel pr)
        {
            bool flagOK = true;
            
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto.Prodot_ID).SingleOrDefault();
                //v.VALORI_FLG_PONDERAZIONE = an.Analisi_flgPonderazione;
                p.PRODOT_DIM_LOTTO = pr.Prodotto.Prodot_Dim_Lotto;
                p.PRODOT_NR_CAMP_QUALITA = pr.Prodotto.Prodot_Nr_Camp_Qualita;
                p.PRODOT_FLG_INTERNO = pr.Prodotto.Prodot_Flg_Interno;
                p.PRODOT_CODICE_DESC = pr.Prodotto.Prodot_Codice_Desc;
                p.PRODOT_STIMA_PROD_ANNO = pr.Prodotto.Prodot_Stima_Prod_Anno;
                p.PRODOT_PERC_SCARTO = pr.Prodotto.Prodot_Perc_Scarto;

                // Inizio Ric#3

                int? Utente_id_old = p.PRODOT_UTENTE_ID;

                if (pr.Prodotto.Prodot_utente_id != null)
                {
                    p.PRODOT_UTENTE_ID = pr.Prodotto.Prodot_utente_id;
                }

                p.PRODOT_FLG_ASSEGN_AL_GRUPPO = pr.Prodotto.Prodot_flg_assegn_al_gruppo;

                List<RICHIE_RICHIESTE> ric_da_aggiornare = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_DESTINATARIO_UTENTE_ID == Utente_id_old
                                                                                         && z.T_STARIC_STATO_RICHIESTA.T_STARIC_CODICE == "INV"
                                                                                         && z.RICHIE_PRODOT_ID == p.PRODOT_ID).ToList<RICHIE_RICHIESTE>();
                foreach (RICHIE_RICHIESTE r in ric_da_aggiornare)
                {
                    r.RICHIE_DESTINATARIO_UTENTE_ID = p.PRODOT_UTENTE_ID;
                    r.RICHIE_FLG_ASSEGN_AL_GRUPPO = p.PRODOT_FLG_ASSEGN_AL_GRUPPO;
                }


                //fine Ric#3

                string convFlg = pr.Prodotto.Prodotto_CoeffConversione.ToString();
                try
                {
                    double valConversione = double.Parse(convFlg);
                    p.PRODOT_COEFF_CONVERSIONE = valConversione;
                }
                catch
                { }

                string convTariffaProposta = pr.Prodotto.Prodot_Tariffa_Proposta.ToString();
                try
                {
                    decimal valConversioneTP = decimal.Parse(convTariffaProposta);
                    p.PRODOT_TARIFFA_PROPOSTA = valConversioneTP;
                }
                catch
                { }

                p.PRODOT_T_UNIMIS_ID = pr.Prodotto.Prodot_UnitaMisura_ID;
                p.PRODOT_T_UNIMIS_ID_SEC = pr.Prodotto.Prodot_UnitaMisura_ID_Sec;
                List<PROPOS_POSIZIONI> lstPos = new List<PROPOS_POSIZIONI>();
                lstPos = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_PRODOT_ID == pr.Prodotto.Prodot_ID).ToList<PROPOS_POSIZIONI>();

                decimal? totCorrente = 0;
                decimal? totCorrentePrim = 0;

                foreach (PROPOS_POSIZIONI pos in lstPos)
                {

                    if (pos.PROPOS_TOT.HasValue)
                    {
                        totCorrentePrim += pos.PROPOS_TOT.Value;
                    }
                }
                totCorrente = totCorrentePrim;
                if (totCorrente.HasValue)
                {
                    totCorrente = decimal.Round(totCorrente.Value, 5, MidpointRounding.AwayFromZero);
                }


                decimal coeffUnitario = 0;

                /* VECCHIA MODALITà */
                /*
                if (p.PRODOT_DIM_LOTTO.HasValue)
                {
                    if (p.PRODOT_NR_CAMP_QUALITA.HasValue)
                    {
                        coeffUnitario = 1 + p.PRODOT_NR_CAMP_QUALITA.Value / p.PRODOT_DIM_LOTTO.Value;
                    }
                    else
                    { coeffUnitario = 1; }
                }
                 */
                /* Nuova Modalità*/
                if (p.PRODOT_DIM_LOTTO.HasValue && p.PRODOT_PERC_SCARTO.HasValue)
                {
                    if (p.PRODOT_DIM_LOTTO.Value > 0)
                    {
                        decimal dimLotto = p.PRODOT_DIM_LOTTO.Value;
                        decimal percScarto = (decimal)p.PRODOT_PERC_SCARTO.Value;
                        decimal cento = 100;
                        coeffUnitario = 1 / (((cento - percScarto) / cento) * dimLotto);
                    }
                }

                totCorrente = decimal.Round(totCorrente.Value * coeffUnitario, 5, MidpointRounding.AwayFromZero);

                p.PRODOT_COSTOUNITARIO = totCorrente;
                en.SaveChanges();
                flagOK = true;
            }
            catch (Exception ex)
            {
                flagOK = false;
            }
            return flagOK;
        }


        //Ric#3
        [HttpPost]
        public ActionResult ChangeProdottoUtenteAss(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();


                int? Utente_id_old = p.PRODOT_UTENTE_ID;

                if (pr.Prodotto_utente_id != null)
                {
                    p.PRODOT_UTENTE_ID = pr.Prodotto_utente_id;
                }
                p.PRODOT_FLG_ASSEGN_AL_GRUPPO = pr.Prodotto_flg_assegn_al_gruppo;

                List<RICHIE_RICHIESTE> ric_da_aggiornare = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_DESTINATARIO_UTENTE_ID == Utente_id_old
                                                                                         && z.T_STARIC_STATO_RICHIESTA.T_STARIC_CODICE == "INV"
                                                                                         && z.RICHIE_PRODOT_ID == p.PRODOT_ID).ToList<RICHIE_RICHIESTE>();
                foreach (RICHIE_RICHIESTE r in ric_da_aggiornare)
                {
                    r.RICHIE_DESTINATARIO_UTENTE_ID = p.PRODOT_UTENTE_ID;
                    r.RICHIE_FLG_ASSEGN_AL_GRUPPO = p.PRODOT_FLG_ASSEGN_AL_GRUPPO;
                }
                en.SaveChanges();
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }
            return Json(new { ok = flagOK, infopersonali = er });

        }
        

        public bool saveValProdottoTot(MyProdottoAjax pr,out string er)
        {
            bool flagOK = true;
            er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();
                //v.VALORI_FLG_PONDERAZIONE = an.Analisi_flgPonderazione;
                p.PRODOT_DIM_LOTTO = pr.Prodotto_Dim_Lotto;
                p.PRODOT_NR_CAMP_QUALITA = pr.Prodotto_nr_Campioni;
                p.PRODOT_FLG_INTERNO = pr.Prodotto_Flag_Interno;
                p.PRODOT_CODICE_DESC = pr.Prodotto_Codice_Desc;
                p.PRODOT_STIMA_PROD_ANNO = pr.Prodotto_Stima_Prod_Anno;
                p.PRODOT_PERC_SCARTO = pr.Prodotto_perc_Scarto;

                // Inizio Ric#3
                int? Utente_id_old = p.PRODOT_UTENTE_ID;

                if (pr.Prodotto_utente_id != null)
                {
                    p.PRODOT_UTENTE_ID = pr.Prodotto_utente_id;
                }
                p.PRODOT_FLG_ASSEGN_AL_GRUPPO = pr.Prodotto_flg_assegn_al_gruppo;

                List<RICHIE_RICHIESTE> ric_da_aggiornare = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_DESTINATARIO_UTENTE_ID == Utente_id_old
                                                                                         && z.T_STARIC_STATO_RICHIESTA.T_STARIC_CODICE == "INV"
                                                                                         && z.RICHIE_PRODOT_ID == p.PRODOT_ID).ToList<RICHIE_RICHIESTE>();
                foreach (RICHIE_RICHIESTE r in ric_da_aggiornare)
                {
                    r.RICHIE_DESTINATARIO_UTENTE_ID = p.PRODOT_UTENTE_ID;
                    r.RICHIE_FLG_ASSEGN_AL_GRUPPO = p.PRODOT_FLG_ASSEGN_AL_GRUPPO;
                }
                //Fine Ric#3
                
                string convFlg = pr.Prodotto_Coeff_Conversione;
                try
                {
                    if (convFlg != null) // se si cercava di modificarlo a null il parse andava in errore e il valore di PRODOT_COEFF_CONVERSIONE non veniva settato
                    {
                        double valConversione = double.Parse(convFlg);
                        p.PRODOT_COEFF_CONVERSIONE = valConversione;
                    }
                    else
                    {
                        p.PRODOT_COEFF_CONVERSIONE = null;
                    }
                }
                catch
                { }

                string convTariffaProposta = pr.Prodotto_Tariffa_Proposta;
                try
                {
                    decimal valConversioneTP = decimal.Parse(convTariffaProposta);
                    p.PRODOT_TARIFFA_PROPOSTA = valConversioneTP;
                }
                catch
                { }

                p.PRODOT_T_UNIMIS_ID = pr.Prodotto_UdM_ID;
                p.PRODOT_T_UNIMIS_ID_SEC = pr.Prodotto_UdM_ID_Sec;
                List<PROPOS_POSIZIONI> lstPos = new List<PROPOS_POSIZIONI>();
                lstPos = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_PRODOT_ID == pr.Prodotto_id).ToList<PROPOS_POSIZIONI>();

                decimal? totCorrente = 0;
                decimal? totCorrentePrim = 0;

                foreach (PROPOS_POSIZIONI pos in lstPos)
                {

                    if (pos.PROPOS_TOT.HasValue)
                    {
                        totCorrentePrim += pos.PROPOS_TOT.Value;
                    }
                }
                totCorrente = totCorrentePrim;
                if (totCorrente.HasValue)
                {
                    totCorrente = decimal.Round(totCorrente.Value, 5, MidpointRounding.AwayFromZero);
                }


                decimal coeffUnitario = 0;

                /* VECCHIA MODALITà */
                /*
                if (p.PRODOT_DIM_LOTTO.HasValue)
                {
                    if (p.PRODOT_NR_CAMP_QUALITA.HasValue)
                    {
                        coeffUnitario = 1 + p.PRODOT_NR_CAMP_QUALITA.Value / p.PRODOT_DIM_LOTTO.Value;
                    }
                    else
                    { coeffUnitario = 1; }
                }
                 */
                /* Nuova Modalità*/
                if (p.PRODOT_DIM_LOTTO.HasValue && p.PRODOT_PERC_SCARTO.HasValue)
                {
                    if (p.PRODOT_DIM_LOTTO.Value > 0)
                    {
                        decimal dimLotto = p.PRODOT_DIM_LOTTO.Value;
                        decimal percScarto = (decimal)p.PRODOT_PERC_SCARTO.Value;
                        decimal cento = 100;
                        coeffUnitario = 1 / (((cento - percScarto) / cento) * dimLotto);
                    }
                }

                totCorrente = decimal.Round(totCorrente.Value * coeffUnitario, 5, MidpointRounding.AwayFromZero);

                p.PRODOT_COSTOUNITARIO = totCorrente;
                en.SaveChanges();
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }
            return flagOK;
        }
        [HttpPost]
        public ActionResult SaveValProdottoTot(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            flagOK = saveValProdottoTot(pr, out er);
            return Json(new { ok = flagOK, infopersonali = er });
        }
        private string insertErrore(int pos, string errore)
        {
            return "[pos. n. " + pos.ToString() + "] " + errore;
        }
        private bool checkProdottoDistinto(PRODOT_PRODOTTI p, IZSLER_CAP_Entities en, out string listaErrori)
        { 
            listaErrori = "";
            List<string> elencoErrori = new List<string>();

            LoadEntities le = new LoadEntities();
            List<MyProdotto> lstProdSimili = le.GetProdotti().Where(z => z.Prodot_ID != p.PRODOT_ID && z.Prodot_HASHKEY == p.PRODOT_HASHKEY).ToList<MyProdotto>();
            /*--RIC_01497
            if (lstProdSimili.Count > 0)
            {

                foreach (MyProdotto lpd in lstProdSimili)
                {
                    listaErrori = "Prodotto simile al prodotto : [" + lpd.Prodot_Codice + "]";
                    return false; // ha senso perche' non dovrebbe mai andrare oltre ad 1
                }
            }
             * /

            /*parte vecchia*/ 
            /*
            if (p.PRODOT_FLG_INTERNO)
            {
                // controllo solo se e' un prodotto Interno
                LoadEntities le = new LoadEntities();
                MyProdottoDistinto pd = le.GetCheckProdDistinto(p.PRODOT_ID);
                List<MyProdottoDistinto> lstPD = le.GetistaProdDistinto(p.PRODOT_ID, pd.NumTotPosizioni);
                foreach (MyProdottoDistinto lpd in lstPD)
                {
                    bool lret = pd.IsSimilar(lpd);
                    if (lret)
                    {
                        MyProdotto pds = le.GetProdotti(lpd.Prodot_id);
                        listaErrori = "Prodotto simile al prodotto : [" + pds.Prodot_Codice + "]";
                        return false;
                    }
                    
                }
            
            }
            */
            return true;
        }


        //Sim: Controllo in modo ricorsivo che un intermedio non contenga a sua volta un intermedio non validato. Se ce ne sono restituisco il codice corrispondente
         private bool IsIntermedioNonValidato(int VALORI_ID, IZSLER_CAP_Entities en, out string IntermedioNonValidato)
        {
            List<int> idStatoCorrettoAvanzamentoIntermedio = new List<int> { 10 };
            IntermedioNonValidato="";
            List<VALPOS_POSIZIONI> lstPos = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == VALORI_ID).ToList<VALPOS_POSIZIONI>();
            foreach (VALPOS_POSIZIONI pos in lstPos)
            {
                if (pos.VALPOS_INTERM_ID.HasValue)
                {
                    //Sono una posizione di intermendio o analisi
                    AnalisiModel an = new AnalisiModel(pos.VALPOS_INTERM_ID.Value);

                    if (an.Analisi.Analisi_flgIntermedio) //Intermedio
                    {
                        if (!idStatoCorrettoAvanzamentoIntermedio.Contains(an.Analisi.Analisi_T_Staval_id))
                        {
                            IntermedioNonValidato = an.Analisi.Analisi_CodiceGenerico.ToString();
                            return true;
                        }
                    }

                    IsIntermedioNonValidato(pos.VALPOS_INTERM_ID.Value, en,out IntermedioNonValidato);
                
                }

            }
            return false;

        }

        

        private bool checkProdotto(PRODOT_PRODOTTI p, IZSLER_CAP_Entities en, bool flagControllo, out string listaErrori, List<int> ProdottoPosIds)
        {
            listaErrori = "";
            string IntermedioNonValidato = ""; //sim
            List<int> idStatoCorrettoAvanzamentoIntermedio = new List<int> { 10 };
            //List<int> idStatoCorrettoAvanzamentoAnalisi = new List<int> { 4, 5, 6 };
            List<int> idStatoCorrettoAvanzamentoProdotto = new List<int> { 4, 5, 6, 8 };

            List<string> elencoErrori = new List<string>();
            int prodot_id = p.PRODOT_ID ;
            LoadEntities le = new LoadEntities();
            List<MyProdottoPos> lstPrimario= le.GetProdottiPos(p.PRODOT_ID);
            List<PROPOS_POSIZIONI> lstPos = new List<PROPOS_POSIZIONI>();
            //List<PROPOS_POSIZIONI> lstPos = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_PRODOT_ID == prodot_id) .OrderBy(z => z.PROPOS_ID).ToList<PROPOS_POSIZIONI>();
            foreach (MyProdottoPos pp in lstPrimario)
            {
                lstPos.Add(en.PROPOS_POSIZIONI.Where(z => z.PROPOS_PRODOT_ID == prodot_id && z.PROPOS_ID == pp.ProdottoPos_id).SingleOrDefault());  
            }
            int dimLotto = 0;
            if (p.PRODOT_DIM_LOTTO.HasValue)
            { dimLotto = p.PRODOT_DIM_LOTTO.Value; }
            if (dimLotto == 0)
            {
                elencoErrori.Add("Dimensione lotto non valorizzata.");
            }

            int count = 1;
            foreach (PROPOS_POSIZIONI pos in lstPos)
            {
                count = ProdottoPosIds.IndexOf(pos.PROPOS_ID); count++;

                if(pos.PROPOS_PRODOT_ID_SEC == p.PRODOT_ID)
                {
                    elencoErrori.Add(insertErrore(count, "Non è possibile usare lo stesso prodotto che si sta valorizzando."));
                    continue;
                }

                if (!pos.PROPOS_FASE_ID.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Fase non inserita"));
                }
                if (pos.PROPOS_FASE_ID.HasValue && isAccettazione(pos.PROPOS_FASE_ID)) //pos.PROPOS_FASE_ID == 1)
                {
                    if (pos.PROPOS_QTA != dimLotto)
                    {
                        elencoErrori.Add(insertErrore(count, "Quantità Fase Accettazione diversa dalla Dimensione Lotto."));
                    }
                }
                if (pos.PROPOS_QTA == 0)
                {
                    elencoErrori.Add(insertErrore(count, "Quantità non inserita"));
                }

                if (!pos.PROPOS_COSTO_QTA.HasValue || pos.PROPOS_COSTO_QTA == 0)
                {
                    elencoErrori.Add(insertErrore(count, "Importo unitario mancante"));
                }
                else if (pos.PROPOS_TOT == 0 || !pos.PROPOS_TOT.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Importo posizione mancante"));
                }

                if (!pos.PROPOS_T_UNIMIS_ID.HasValue && pos.PROPOS_PRODOT_ID_SEC.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Unità di misura non inserita."));
                }
                if (!pos.PROPOS_COEFF_CONVERSIONE.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Coef. di conversione non inserito."));
                }

                if (pos.PROPOS_INTERM_ID.HasValue)
                {
                    //Sono una posizione di intermendio o analisi
                    AnalisiModel an = new AnalisiModel(pos.PROPOS_INTERM_ID.Value);

                    if (an.Analisi.Analisi_flgIntermedio) //Intermedio
                    {
                        if (!idStatoCorrettoAvanzamentoIntermedio.Contains(an.Analisi.Analisi_T_Staval_id))
                        {
                            elencoErrori.Add(insertErrore(count, "Intermedio non bloccato."));
                            continue;
                        }

                        //verifico che l'intermedio non contenga a sua volta intermedi non validati pur essendo lui validato
                        if (IsIntermedioNonValidato(pos.PROPOS_INTERM_ID.Value, en, out IntermedioNonValidato) == true) //sim
                        {                                                                                         //sim
                            elencoErrori.Add(insertErrore(count, "La posizione contiene a sua volta il seguente intermedio non bloccato: " + IntermedioNonValidato)); //sim
                            continue;                                                                             //sim
                        }                                                                                         //sim
                    }
                    //else //Analisi
                    //{
                    //    if (!idStatoCorrettoAvanzamentoAnalisi.Contains(an.Analisi.Analisi_T_Staval_id))
                    //    {
                    //        elencoErrori.Add(insertErrore(count, "Analisi non validata."));
                    //        continue;
                    //    }
                    //}
                }

                if (pos.PROPOS_PRODOT_ID_SEC.HasValue)
                {
                    //La mia posizione è un prodotto
                    ProdottoModel pm = new ProdottoModel(pos.PROPOS_PRODOT_ID_SEC.Value);

                    if (pm.Prodotto.Prodot_Flg_Interno) //Prodotto Interno
                    {
                        if (!idStatoCorrettoAvanzamentoProdotto.Contains(pm.Prodotto.Prodot_T_Stapro_Id.Value))
                        {
                            elencoErrori.Add(insertErrore(count, "Prodotto non validato."));
                            continue;
                        }
                    }
                }
                count++;
            }

            if (elencoErrori.Count == 0)
                return true;
            if (elencoErrori.Count != 0)
            {
                listaErrori = "";
                if(!flagControllo)
                    listaErrori = "Impossibile inviare il prodotto al validatore.<br/>";
                listaErrori += "Errori riscontrati:<br/>";
                foreach (string s in elencoErrori)
                {
                    listaErrori += s + "<br/>";
                }
            }
            return false;
        }

        //sim verifico che il prodotto sia stato aggiornato da non più di un ora prima di effettuare l'invio al validatore
        public bool IsAggiornata(PRODOT_PRODOTTI p, out string listaErrori)
        {
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            listaErrori = "";
            if (!p.PRODOT_TS_AGGIORNAMENTO_POSIZIONI.HasValue || DateTime.Now.Subtract(p.PRODOT_TS_AGGIORNAMENTO_POSIZIONI.Value).TotalHours > 1)
            {
                listaErrori = "Per fare l'invio al validatore è necessario aver premuto prima sul bottone \"Aggiorna posizioni\" <br/>";
                return false;
            }
            else
            {
                return true;
            }
        }
    
        [HttpPost]
        public ActionResult InviaValidatoreProdotto(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            flagOK = saveValProdottoTot(pr, out er);

            if (flagOK)
            {

                try
                {
                    DateTime dt = DateTime.Now;
                    IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                    //sempre update
                    PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();
                    T_STAPRO_STATO_PRODOTTO tStapro = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "INVALI").SingleOrDefault();
                    string listaErrori = "";
                    string msg_da_aggiornare = ""; //sim
                    bool flgOKProdotto = checkProdotto(p, en, false, out listaErrori, pr.ProdottoPosIds);
                    bool flgOKAggiornata = IsAggiornata(p, out msg_da_aggiornare);//sim

                    //Ric#10
                    if (p.PRODOT_T_UNIMIS_ID == p.PRODOT_T_UNIMIS_ID_SEC)
                    {
                        p.PRODOT_COEFF_CONVERSIONE = 1;
                    }

                    if (p.PRODOT_CODICE.StartsWith("T_"))
                    {
                        flagOK = false;
                        flgOKProdotto = false;
                        listaErrori = "Le valorizzazioni di prova non possono essere inviate al validatore.";
                    }

                    if (flgOKProdotto && flgOKAggiornata) //sim
                    {

                        bool flgOKProdDistinti = checkProdottoDistinto(p, en, out listaErrori);
                        if (flgOKProdDistinti)
                        {
                            chiudiRichiestaCorrente(p.PRODOT_ID, en);
                            p.PRODOT_T_STAPRO_ID = tStapro.T_STAPRO_ID;
                            RICHIE_RICHIESTE r = creaRichiestaValidazioneProdotto(p, en, dt);
                            if (r.RICHIE_DESTINATARIO_UTENTE_ID > 0)
                            {
                                salvaTrackingProdotto(p, en, dt);
                                en.SaveChanges();
                                en.USPT_STORICIZZA_COSTI_PRODOTTI(p.PRODOT_ID);
                                inviaEmail(r);
                                flagOK = true;
                            }
                            else
                            {
                                flagOK = false;
                                er = getErroreDestinatarioUserID(p, "REFVAL", en);
                            }
                        }
                        else
                        {
                            flagOK = false;
                            er = "Elenco Errori: <br/>" + listaErrori;
                            
                        }
                    }
                    else
                    {
                        flagOK = false;
                        //er = "Elenco Errori: <br/>" + listaErrori;
                        er = "Elenco Errori: <br/>" + msg_da_aggiornare + listaErrori; //sim
                    }
                }
                catch (Exception ex)
                {
                    er = ex.ToString();
                    flagOK = false;
                }
            }
            return Json(new { ok = flagOK, infopersonali = er });
        }
        [HttpPost]
        public ActionResult CheckPosizioniProdotto(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();
                T_STAPRO_STATO_PRODOTTO tStapro = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "INVALI").SingleOrDefault();
                string listaErrori = "";
                bool flgOKProdotto = checkProdotto(p, en, true, out listaErrori, pr.ProdottoPosIds);
                if (!flgOKProdotto)
                {
                    flagOK = false;
                    er = "Elenco Errori: <br/>" + listaErrori;
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }

        [HttpPost]
        public ActionResult ApprovaEdInviaMagazzinoProdotto(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();
                T_STAPRO_STATO_PRODOTTO tStapro = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "INMAG").SingleOrDefault();
                chiudiRichiestaCorrente(p.PRODOT_ID, en);
                p.PRODOT_T_STAPRO_ID = tStapro.T_STAPRO_ID;
                p.PRODOT_FLG_BLOCCATO = true;
                p.PRODOT_FLG_BLOCCATO_MAGAZZINO = true;

                //Ric#10
                if (p.PRODOT_T_UNIMIS_ID == p.PRODOT_T_UNIMIS_ID_SEC)
                {
                    p.PRODOT_COEFF_CONVERSIONE = 1;
                }

                RICHIE_RICHIESTE r = creaRichiestaSbloccoProdottoMagazzino(p, en, dt);
                //come richiesto e confermato da Possenti con email del 14/10/2014, al momento dell'invio al CDG valorizzo il Costo unitario deliberato col valore del costo unitario
                p.PRODOT_COSTOUNITARIO_DELIBE = p.PRODOT_COSTOUNITARIO; //sim
                if (r.RICHIE_DESTINATARIO_UTENTE_ID > 0)
                {
                    salvaTrackingProdotto(p, en, dt);
                    en.SaveChanges();
                    inviaEmail(r);
                    flagOK = true;
                }
                else
                {
                    flagOK = false;
                    er = getErroreDestinatarioUserID(p, "RESMAG", en);
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        [HttpPost]
        public ActionResult ApprovaEdInviaCdGProdotto(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt =DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();
                T_STAPRO_STATO_PRODOTTO tStapro = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "INDEL").SingleOrDefault();
                chiudiRichiestaCorrente(p.PRODOT_ID, en);
                p.PRODOT_T_STAPRO_ID = tStapro.T_STAPRO_ID;
                p.PRODOT_FLG_BLOCCATO = true;

                //Ric#10
                if (p.PRODOT_T_UNIMIS_ID == p.PRODOT_T_UNIMIS_ID_SEC)
                {
                    p.PRODOT_COEFF_CONVERSIONE = 1;
                }

                RICHIE_RICHIESTE r= creaRichiestaDeliberaProdotto(p, en, dt);
                //come richiesto e confermato da Possenti con email del 14/10/2014, al momento dell'invio al CDG valorizzo il Costo unitario deliberato col valore del costo unitario
                p.PRODOT_COSTOUNITARIO_DELIBE = p.PRODOT_COSTOUNITARIO; //sim
                if(r.RICHIE_DESTINATARIO_UTENTE_ID >0)
                {
                    salvaTrackingProdotto(p, en, dt);
                    en.SaveChanges();
                    inviaEmail(r);
                    flagOK = true;
                }
                else 
                {
                    flagOK = false;
                    er = getErroreDestinatarioUserID(p, "CDG", en);
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }

        public ActionResult NuovaValorizzazioneTest()
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Dettaglio prodotto";
            int prodot_id = 0;

            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

                LoadEntities le = new LoadEntities();
                
                Profili p = le.GetProfilo(this.CurrentUserID,this.CurrentProfileID);

                //Prendo il primo gruppo tra i gruppi prodotto
                int gruppo_id = 0;

                if (p.ElencoGruppiProdotto.Count > 0)
                {
                    gruppo_id = p.ElencoGruppiProdotto[0].GruppoID;
                }

                //Recupera il gruppo dell'utente
                //int gruppo_id = 0;
                //PROFIL_PROFILI profilo = en.PROFIL_PROFILI.Where(p => p.PROFIL_CODICE == "VAL").SingleOrDefault();
                
                T_STAPRO_STATO_PRODOTTO stato = en.T_STAPRO_STATO_PRODOTTO.Where(s => s.T_STAPRO_CODICE == "INVAL").First();
                
                //IEnumerable<M_UTPRGR_UTENTI_PROFILI_GRUPPI> gruppi = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(x => x.M_UTPRGR_UTENTE_ID == this.CurrentUserID &&
                //    x.M_UTPRGR_PROFIL_ID == profilo.PROFIL_ID);
                
                T_UNIMIS_UNITA_MISURA udm = en.T_UNIMIS_UNITA_MISURA.First();

                ////Abbina al nuovo prodotto il gruppo del valorizzatore, se non lo trova, prende il primo gruppo dell'elenco
                //if (gruppi.Any() && gruppi.First().M_UTPRGR_GRUREP_ID.HasValue)
                //    gruppo_id = (int)gruppi.First().M_UTPRGR_GRUREP_ID;
                //else
                //    gruppo_id = en.GRUREP_GRUPPI_REPARTI.Where(gr => gr.GRUREP_FLG_REPARTO == true).First().GRUREP_ID;

                PRODOT_PRODOTTI prodotto = new PRODOT_PRODOTTI();
                prodotto.PRODOT_CODICE = "T";
                prodotto.PRODOT_DESC = "Prova di valorizzazione";
                prodotto.PRODOT_DESC_ESTESA = "Prova di valorizzazione";
                prodotto.PRODOT_REPARTO_GRUREP_ID = gruppo_id;
                prodotto.PRODOT_FLG_BLOCCATO = false;
                prodotto.PRODOT_FLG_BLOCCATO_MAGAZZINO = false;
                prodotto.PRODOT_FLG_INTERNO = true;
                prodotto.PRODOT_T_STAPRO_ID = stato.T_STAPRO_ID;
                prodotto.PRODOT_UTENTE_ID = this.CurrentUserID;
                prodotto.PRODOT_T_UNIMIS_ID = udm.T_UNIMIS_ID;


                en.PRODOT_PRODOTTI.AddObject(prodotto);
                en.SaveChanges();

                prodotto.PRODOT_CODICE = prodotto.PRODOT_CODICE + "_" + prodotto.PRODOT_ID;
                en.SaveChanges();

                prodot_id = prodotto.PRODOT_ID;
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Prodotto");
            }

            return RedirectToAction("ProdottoEdit", new { id = prodot_id });
        }


        #region Copia_da_prodotto

        [HttpPost]
        public ActionResult ApplicaValorizzazioneAProdotti(MyProdottoAjax mpa)
        {
            bool flagOK = true;
            string er = "";
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI vMaster = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == mpa.Prodotto_id_Master).SingleOrDefault();
                PRODOT_PRODOTTI vcopy = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == mpa.Prodotto_id).SingleOrDefault();
                vMaster.PRODOT_DIM_LOTTO = vcopy.PRODOT_DIM_LOTTO;
                vMaster.PRODOT_NR_CAMP_QUALITA = vcopy.PRODOT_NR_CAMP_QUALITA;
                vMaster.PRODOT_PERC_SCARTO = vcopy.PRODOT_PERC_SCARTO;
                vMaster.PRODOT_STIMA_PROD_ANNO = vcopy.PRODOT_STIMA_PROD_ANNO;
                vMaster.PRODOT_T_UNIMIS_ID = vcopy.PRODOT_T_UNIMIS_ID ;
                vMaster.PRODOT_FLG_INTERNO = vcopy.PRODOT_FLG_INTERNO;
                en.SaveChanges();
                List<PROPOS_POSIZIONI> posModello = en.PROPOS_POSIZIONI.Where(z => z.PROPOS_PRODOT_ID == mpa.Prodotto_id).ToList<PROPOS_POSIZIONI>();
                List<PROPOS_POSIZIONI> newListPos = new List<PROPOS_POSIZIONI>();
                foreach (PROPOS_POSIZIONI pos in posModello)
                {
                    PROPOS_POSIZIONI currPos = clonaPosizione(pos);
                    currPos.PROPOS_COSTO_QTA = getCosto(currPos, vMaster);
                    // se Fase== Accettazione --> allora l'unità di misura e' Min

                    if (isAccettazione(pos.PROPOS_FASE_ID))// == 1) 
                    {
                        // currPos.VALPOS_T_UNIMIS_ID = 13;  // UdM Minuti
                        currPos.PROPOS_T_UNIMIS_ID = 25; // UdM Numero
                    }
                    //currPos.PROPOS_TOT = decimal.Round((currPos.PROPOS_QTA * currPos.PROPOS_COSTO_QTA.Value), 2, MidpointRounding.AwayFromZero);
                    currPos.PROPOS_PRODOT_ID = mpa.Prodotto_id_Master;
                    newListPos.Add(currPos);
                }
                foreach (PROPOS_POSIZIONI newPos in newListPos)
                {
                    en.PROPOS_POSIZIONI.AddObject(newPos);
                    en.SaveChanges();
                }
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }


        #endregion

        #region Gestione POP UP
        public ActionResult PopUpRespingiProdotto(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int prodotto_id = id;
            ProdottoModel pr = new ProdottoModel(prodotto_id);
            return View(pr);
        }
        [HttpPost]
        public ActionResult RespingiProdotto(MyProdottoAjax pr)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                PRODOT_PRODOTTI p = en.PRODOT_PRODOTTI.Where(z => z.PRODOT_ID == pr.Prodotto_id).SingleOrDefault();
                T_STAPRO_STATO_PRODOTTO tStapro = en.T_STAPRO_STATO_PRODOTTO.Where(z => z.T_STAPRO_CODICE == "INVAL").SingleOrDefault();
                chiudiRichiestaCorrente(p.PRODOT_ID , en);
                p.PRODOT_T_STAPRO_ID = tStapro.T_STAPRO_ID;
                RICHIE_RICHIESTE r = creaRichiestaRespintaProdotto(p, pr.Prodotto_Motivo , en, dt);
                salvaTrackingProdotto(p, en, dt);
                en.SaveChanges();

                inviaEmail(r);
                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPIntermAnalisi(int id, int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int propos_id = id;
            ListaIntermediAnalisiPModel IA = new ListaIntermediAnalisiPModel(propos_id);

            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                IA.NumEntities = numEntities;
            }

            if (table_search1 != null)
                IA.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                IA.CurrentPage = pageNum;
            }
            else
                IA.CurrentPage = 1;

            IA.Data = IA.ElencoAnalisi(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).Take(IA.NumEntities).ToList();
            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)IA.ElencoAnalisi(this.CurrentUserID, this.CurrentProfileID).Count() / IA.NumEntities));
            IA.CurrentPage = 1;


            return View(IA);
        }
        [HttpPost]
        public ActionResult PPIntermAnalisi(MyPaginAjax mpa)
        {
            ListaIntermediAnalisiPModel IA = new ListaIntermediAnalisiPModel(mpa.valpos_id);
            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
            //         ViewBag.NumEntities = mpa.NumEntities.ToString();
            //         ViewBag.table_search = mpa.SearchDescription.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;

            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    IA.CurrentPage = mpa.CurrentPage.Value;
                else
                    IA.CurrentPage = 1;
            }
            else
                IA.CurrentPage = 1;

            IA.NumEntities = mpa.NumEntities;
            IA.SearchDescription = mpa.SearchDescription;

            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)IA.ElencoAnalisi(this.CurrentUserID, this.CurrentProfileID).Count() / IA.NumEntities));
            if (IA.CurrentPage > IA.NumberOfPages) IA.CurrentPage = IA.NumberOfPages;
            IA.Data = IA.ElencoAnalisi(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).Skip(IA.NumEntities * (IA.CurrentPage - 1)).Take(IA.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPIntermAnalisi", IA) });
        }

        public ActionResult PPMacchinarioDettRO(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaMacchinariPModel IA = new ListaMacchinariPModel(valpos_id);
            return View(IA);
        }


        public ActionResult PPIntermAnalisiDettRO(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int prodpos_id = id;
            ListaIntermediAnalisiPModel IA = new ListaIntermediAnalisiPModel(prodpos_id);
            return View(IA);
        }
        public ActionResult PPProdottiDettRO(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaProdottoPModel IA = new ListaProdottoPModel(valpos_id);
            return View(IA);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPProdotti(int id,int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaProdottoPModel prod = new ListaProdottoPModel(valpos_id);

            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                prod.NumEntities = numEntities;
            }

            if (table_search1 != null)
                prod.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                prod.CurrentPage = pageNum;
            }
            else
                prod.CurrentPage = 1;

            prod.Data = prod.ElencoProdotti.Where(pp => !pp.Prodot_Codice.StartsWith("T_")).OrderBy(z => z.Prodot_Codice).Take(prod.NumEntities).ToList();
            prod.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)prod.ElencoProdotti.Count() / prod.NumEntities));
            prod.CurrentPage = 1;

            return View(prod);
        }
        [HttpPost]
        public ActionResult PPProdotti(MyPaginAjax mpa)
        {
            ListaProdottoPModel prod = new ListaProdottoPModel(mpa.valpos_id);

            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;

            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    prod.CurrentPage = mpa.CurrentPage.Value;
                else
                    prod.CurrentPage = 1;
            }
            else
                prod.CurrentPage = 1;

            prod.NumEntities = mpa.NumEntities;
            prod.SearchDescription = mpa.SearchDescription;

            prod.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)prod.ElencoProdotti.Count() / prod.NumEntities));
            if (prod.CurrentPage > prod.NumberOfPages) prod.CurrentPage = prod.NumberOfPages;
            prod.Data = prod.ElencoProdotti.Where(pp => !pp.Prodot_Codice.StartsWith("T_")).OrderBy(z => z.Prodot_Codice).Skip(prod.NumEntities * (prod.CurrentPage - 1)).Take(prod.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPProdotti", prod) });
        }
        [HttpGet]
        public ActionResult PPUdMConversione(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            UdMConversionePModel udm = new UdMConversionePModel(id);

            return View(udm);
        }


        
        public ActionResult PopUpProdotto(int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ListaIndexProdottoModel pr = new ListaIndexProdottoModel();
            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                pr.NumEntities = numEntities;
            }

            if (table_search1 != null)
                pr.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                pr.CurrentPage = pageNum;
            }
            else
                pr.CurrentPage = 1;
            pr.FiltroStato = 0;
            List<MyProdotto> listProdotto = pr.GetProdottiDaValorizzare(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Prodot_Codice).ToList<MyProdotto>();

            pr.Data = listProdotto.Take(pr.NumEntities).ToList();
            pr.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listProdotto.Count() / pr.NumEntities));
            pr.CurrentPage = 1;


            return View(pr);

        }
        [HttpPost]
        public ActionResult PopUpProdotto(MyPaginAjax mpa)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }


            ListaIndexProdottoModel pr = new ListaIndexProdottoModel();
            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
            //         ViewBag.NumEntities = mpa.NumEntities.ToString();
            //         ViewBag.table_search = mpa.SearchDescription.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;

            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    pr.CurrentPage = mpa.CurrentPage.Value;
                else
                    pr.CurrentPage = 1;
            }
            else
                pr.CurrentPage = 1;

            pr.FiltroStato = 0;

            pr.NumEntities = mpa.NumEntities;
            pr.SearchDescription = mpa.SearchDescription;
            List<MyProdotto> listProdotti = pr.GetProdottiDaValorizzare(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Prodot_Codice).ToList<MyProdotto>();
            pr.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listProdotti.Count() / pr.NumEntities));
            if (pr.CurrentPage > pr.NumberOfPages) pr.CurrentPage = pr.NumberOfPages;
            pr.Data = listProdotti.Skip(pr.NumEntities * (pr.CurrentPage - 1)).Take(pr.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PopUpProdotto", pr) });

        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPMacchinari(int id, int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int prodpos_id = id;
            ListaMacchinariPModel IA = new ListaMacchinariPModel(prodpos_id);
            

            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                IA.NumEntities = numEntities;
            }

            if (table_search1 != null)
                IA.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                IA.CurrentPage = pageNum;
            }
            else
                IA.CurrentPage = 1;

            List<MyMacchinario> listMacchinari = IA.GetElencoMacchinari(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Macchi_Codice).ToList<MyMacchinario>();
            IA.Data = listMacchinari.OrderBy(z => z.Macchi_Codice).Take(IA.NumEntities).ToList();
            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listMacchinari.Count() / IA.NumEntities));
            IA.CurrentPage = 1;


            return View(IA);
        }
        [HttpPost]
        public ActionResult PPMacchinari(MyPaginAjax mpa)
        {
            ListaMacchinariPModel IA = new ListaMacchinariPModel(mpa.valpos_id);
            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
            //         ViewBag.NumEntities = mpa.NumEntities.ToString();
            //         ViewBag.table_search = mpa.SearchDescription.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;

            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    IA.CurrentPage = mpa.CurrentPage.Value;
                else
                    IA.CurrentPage = 1;
            }
            else
                IA.CurrentPage = 1;

            IA.NumEntities = mpa.NumEntities;
            IA.SearchDescription = mpa.SearchDescription;

            List<MyMacchinario> listMacchinari = IA.GetElencoMacchinari(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Macchi_Codice).ToList<MyMacchinario>();

            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listMacchinari.Count() / IA.NumEntities));
            if (IA.CurrentPage > IA.NumberOfPages) IA.CurrentPage = IA.NumberOfPages;
            IA.Data = listMacchinari.OrderBy(z => z.Macchi_Codice).Skip(IA.NumEntities * (IA.CurrentPage - 1)).Take(IA.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPMacchinari", IA) });
        }

        [HttpPost]
        public ActionResult PPCopiaDaProdotti(MyPaginAjax mpa)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;

            ListaCopiaDaProdottoModel ppCopiaPro = new ListaCopiaDaProdottoModel(mpa.id);

            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    ppCopiaPro.CurrentPage = mpa.CurrentPage.Value;
                else
                    ppCopiaPro.CurrentPage = 1;
            }
            else
                ppCopiaPro.CurrentPage = 1;

            this.SetMessage = "Elenco prodotti";
            ppCopiaPro.NumEntities = mpa.NumEntities;
            ppCopiaPro.SearchDescription = mpa.SearchDescription;

            List<MyProdotto> lstProdotti = ppCopiaPro.GetElencoProdotti(this.CurrentUserID, this.CurrentProfileID).ToList<MyProdotto>();
            ppCopiaPro.DataTot = lstProdotti.Count;
            ppCopiaPro.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)lstProdotti.Count() / ppCopiaPro.NumEntities));
            if (ppCopiaPro.CurrentPage > ppCopiaPro.NumberOfPages) ppCopiaPro.CurrentPage = ppCopiaPro.NumberOfPages;
            ppCopiaPro.Data = lstProdotti.OrderBy(z => z.Prodot_Desc).Skip(ppCopiaPro.NumEntities * (ppCopiaPro.CurrentPage - 1)).Take(ppCopiaPro.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPCopiaDaProdotti", ppCopiaPro) });

        }
        [HttpGet]
        public ActionResult PPCopiaDaProdotti(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            this.SetMessage = "Elenco prodotti";
            ListaCopiaDaProdottoModel ppCopiaPro = new ListaCopiaDaProdottoModel(id);
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                ppCopiaPro.NumEntities = numEntities;
            }

            if (table_search1 != null)
                ppCopiaPro.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                ppCopiaPro.CurrentPage = pageNum;
            }
            else
                ppCopiaPro.CurrentPage = 1;

            List<MyProdotto> lstProdotti = ppCopiaPro.GetElencoProdotti(this.CurrentUserID, this.CurrentProfileID).ToList<MyProdotto>();
            ppCopiaPro.DataTot = lstProdotti.Count;
            ppCopiaPro.Data = lstProdotti.OrderBy(z => z.Prodot_Desc).Take(ppCopiaPro.NumEntities).ToList();
            ppCopiaPro.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)lstProdotti.Count() / ppCopiaPro.NumEntities));
            ppCopiaPro.CurrentPage = 1;

            return View(ppCopiaPro);
        }

        #endregion

        public ActionResult PopUpIntermediEsplosi(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ProdottoModel an = new ProdottoModel(id);

            return View(an);
        }
        //SimStampaIntermedi
        public ActionResult PopUpIntermediEsplosi_new(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            StampaIntermediModel an = new StampaIntermediModel(id);

            return View(an);
        }

    }
}