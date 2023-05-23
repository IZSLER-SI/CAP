using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IZSLER_CAP.Helpers;
using IZSLER_CAP.Models;

namespace IZSLER_CAP.Controllers
{
    public class IntermediController : B16Controller 
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

        private VALPOS_POSIZIONI clonaPosizione(VALPOS_POSIZIONI objSrc)
        {
            VALPOS_POSIZIONI newObj = new VALPOS_POSIZIONI();
            
            newObj.VALPOS_COEFF_CONVERSIONE = objSrc.VALPOS_COEFF_CONVERSIONE;
            newObj.VALPOS_COSTO_QTA = objSrc.VALPOS_COSTO_QTA;
            newObj.VALPOS_DESC = objSrc.VALPOS_DESC;
            newObj.VALPOS_FASE_ID = objSrc.VALPOS_FASE_ID;
            newObj.VALPOS_FIGPRO_ID = objSrc.VALPOS_FIGPRO_ID;
            newObj.VALPOS_INTERM_ID = objSrc.VALPOS_INTERM_ID;
            newObj.VALPOS_PRODOT_ID = objSrc.VALPOS_PRODOT_ID;
            newObj.VALPOS_QTA = objSrc.VALPOS_QTA;
            newObj.VALPOS_TOT = objSrc.VALPOS_TOT;
            newObj.VALPOS_VALORI_ID = objSrc.VALPOS_VALORI_ID;
            newObj.VALPOS_T_UNIMIS_ID = objSrc.VALPOS_T_UNIMIS_ID;
            newObj.VALPOS_SECONDARIE = objSrc.VALPOS_SECONDARIE;
            newObj.VALPOS_COEFF_CONVERSIONE = objSrc.VALPOS_COEFF_CONVERSIONE;
            newObj.VALPOS_COD_SETTORE = objSrc.VALPOS_COD_SETTORE;
            
            return newObj;
        }
        //
        // GET: //
        public IntermediController()
            : base()
        {
            this.SetMessage= "Intermedi";
            this.SetLiIntermedi  = "current";
        }
       
        public ActionResult Index(int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ListaIntermediModel an = new ListaIntermediModel();
            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                an.NumEntities = numEntities;
            }

            if (table_search1 != null)
                an.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                an.CurrentPage = pageNum;
            }
            else
                an.CurrentPage = 1;

            List<MyAnalisi> listAnalisi = an.GetElencoIntermedi(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).ToList<MyAnalisi>();

            an.Data = listAnalisi.Take(an.NumEntities).ToList();
            an.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listAnalisi.Count() / an.NumEntities));
            an.CurrentPage = 1;


            return View(an);

        }
        [HttpPost]
        public ActionResult Index(MyPaginAjax mpa)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString() + " - Index ");
            ListaIntermediModel an = new ListaIntermediModel();
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
                    an.CurrentPage = mpa.CurrentPage.Value;
                else
                    an.CurrentPage = 1;
            }
            else
                an.CurrentPage = 1;


            an.NumEntities = mpa.NumEntities;
            an.SearchDescription = mpa.SearchDescription;
            List<MyAnalisi> listAnalisi = an.GetElencoIntermedi(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).ToList<MyAnalisi>();
            an.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listAnalisi.Count() / an.NumEntities));
            if (an.CurrentPage > an.NumberOfPages) an.CurrentPage = an.NumberOfPages;
            an.Data = listAnalisi.Skip(an.NumEntities * (an.CurrentPage - 1)).Take(an.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_Index", an) });

        }
        [HttpGet]
        public ActionResult PPUdMConversione(int id, int sec)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            UdMConversioneModel udm = new UdMConversioneModel(id, sec);

            return View(udm);
        }
    
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPProdotti(int id, int sec, int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaProdottoModel prod = new ListaProdottoModel(valpos_id, sec);

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

            prod.Data = prod.ElencoProdotti.OrderBy(z => z.Prodot_Codice).Take(prod.NumEntities).ToList();
            prod.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)prod.ElencoProdotti.Count() / prod.NumEntities));
            prod.CurrentPage = 1;

            return View(prod);
        }

        public ActionResult IntermedioInsert()
        {
            B16ModelMgr b = new B16ModelMgr();
            return View(b);
        }

        [HttpPost]
        public ActionResult PPProdotti(MyPaginAjax mpa)
        {
            ListaProdottoModel prod = new ListaProdottoModel(mpa.valpos_id, mpa.sec);

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
            prod.Data = prod.ElencoProdotti.OrderBy(z => z.Prodot_Codice).Skip(prod.NumEntities * (prod.CurrentPage - 1)).Take(prod.NumEntities).ToList();
            
            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPProdotti", prod) });
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPGruppiReparti(int id, int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int analisi_ID = id;

            ListaGruppiRepartiModel IA = new ListaGruppiRepartiModel(analisi_ID, this.CurrentUserID,this.CurrentProfileID );
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

            IA.Data = IA.ElencoEntita.OrderBy(z => z.Grurep_Codice).Take(IA.NumEntities).ToList();
            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)IA.ElencoEntita.Count() / IA.NumEntities));
            IA.CurrentPage = 1;


            return View(IA);
        }
        [HttpPost]
        public ActionResult PPGruppiReparti(MyPaginAjax mpa)
        {
            ListaGruppiRepartiModel IA = new ListaGruppiRepartiModel(mpa.id, this.CurrentUserID, this.CurrentProfileID);
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

            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)IA.ElencoEntita.Count() / IA.NumEntities));
            if (IA.CurrentPage > IA.NumberOfPages) IA.CurrentPage = IA.NumberOfPages;
            IA.Data = IA.ElencoEntita.OrderBy(z => z.Grurep_Codice).Skip(IA.NumEntities * (IA.CurrentPage - 1)).Take(IA.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPGruppiReparti", IA) });
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPIntermAnalisi(int id,int sec,int? NumEntities, string table_search, int? CurrentPage)
        { 
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaIntermediAnalisiModel IA = new ListaIntermediAnalisiModel(valpos_id,sec);

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
            ListaIntermediAnalisiModel IA = new ListaIntermediAnalisiModel(mpa.valpos_id, mpa.sec);
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



        public ActionResult PPIntermAnalisiDettRO(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaIntermediAnalisiModel IA = new ListaIntermediAnalisiModel(valpos_id);
            return View(IA);
        }

        //sim 2022-02-17 Nuova funzione di duplicazione degli intermedi
        public ActionResult IntermedioDuplicate(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Duplica intermedio";

            IntermediModel an = new IntermediModel(id);

            return View(an);

        }


        public ActionResult IntermediEdit(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Dettaglio intermedio";
            
            IntermediModel an = new IntermediModel(id);

            //if (an.Analisi.Analisi_T_Staval_id == 11)
            //{
            //    if (IsEditable(an))
            //    {
            //        attualizzaPosizioniIntermedio(id);

            //        saveValAnalisiTot(an);
            //        an = new IntermediModel(id);
            //    }
            //}
            
            return View(an);

        }

        //sim: tengo traccia di quando è stato effettuato l'ultimo aggiorna posizioni
        public void TrackingAggiornaPosizioni(int Analisi_id)
        {
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == Analisi_id).SingleOrDefault();
            v.VALORI_TS_AGGIORNAMENTO_POSIZIONI = DateTime.Now;

            en.SaveChanges();
        }

        [HttpPost]
        public ActionResult AttualizzaPosizioni(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();

                if (v.T_STAVAL_STATO_VALORIZZAZIONE.T_STAVAL_ID == 11) //Controllo lo stato
                {
                    attualizzaPosizioniIntermedio(v);
                    TrackingAggiornaPosizioni(an.Analisi_id); //sim
                    saveValAnalisiTot(an, out er);
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetImportoFaseAccettazione(string analisi_id, string Attivi_id, string valpos_id)
        {
            if (Attivi_id == "") Attivi_id = "0";
            if (valpos_id == "") valpos_id = "0";
            if (analisi_id == "") analisi_id = "0";
            AnalisiModel f = new AnalisiModel(int.Parse(analisi_id));
            LoadEntities le = new LoadEntities();
            MyGrurep mgr = le.GetGruppo(f.Analisi.Analisi_Gruppo_id.Value);

            decimal val = 0;
            if (mgr.Grurep_PrezzoUnit_Accettazione.HasValue)
            { val = mgr.Grurep_PrezzoUnit_Accettazione.Value; }
            string lret = "";
            lret = val.ToString("#.##");
            lret = lret.Replace(".", ",");
            if (lret == "") lret = "0,00";

            List<SelectListItem> lst = new List<SelectListItem>();

            SelectListItem sliVoid = new SelectListItem();

            sliVoid.Text = lret;
            sliVoid.Value = mgr.Grurep_ID.ToString();

            lst.Add(sliVoid);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetElencoFigProfessionali(string analisi_id, string Attivi_id, string valpos_id)
        {
            if (Attivi_id == "") Attivi_id = "0";
            if (valpos_id == "") valpos_id = "0";
            if (analisi_id == "") valpos_id = "0";
            AnalisiModel f = new AnalisiModel(int.Parse(analisi_id));
          //  f.SetListaFigProf(int.Parse(Attivi_id));
            MyAnalisiPos pos= f.ElencoAnalisiPos.Where(z => z.AnalisiPos_id == int.Parse(valpos_id)).SingleOrDefault();
            int fase_id = 0;
            try
            {
                fase_id = int.Parse(Attivi_id);
            }
            catch { }
            if (fase_id > 0)
            {
                pos.AnalisiPos_ListaFigProf = f.GetFigProf(fase_id);
            }
            
            return Json(pos.AnalisiPos_ListaFigProfSL, JsonRequestBehavior.AllowGet);   
          //  return Json(f.ListaFigProf , JsonRequestBehavior.AllowGet);
        }
    [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetElencoFigProfessionaliSec(string analisi_id, string Attivi_id, string valpos_id)
        {
            if (Attivi_id == "") Attivi_id = "0";
            if (valpos_id == "") valpos_id = "0";
            if (analisi_id == "") valpos_id = "0";
            AnalisiModel f = new AnalisiModel(int.Parse(analisi_id));
          //  f.SetListaFigProf(int.Parse(Attivi_id));
            MyAnalisiPos pos= f.ElencoAnalisiPosSec.Where(z => z.AnalisiPos_id == int.Parse(valpos_id)).SingleOrDefault();
            int fase_id = 0;
            try
            {
                fase_id = int.Parse(Attivi_id);
            }
            catch { }
            if (fase_id > 0)
            {
                pos.AnalisiPos_ListaFigProf = f.GetFigProf(fase_id);
            }
            
            return Json(pos.AnalisiPos_ListaFigProfSL, JsonRequestBehavior.AllowGet);   
          //  return Json(f.ListaFigProf , JsonRequestBehavior.AllowGet);
        } 
        public ActionResult IntermediWorkflow(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Workflow analisi";
            AnalisiWorkflowMdodel an = new AnalisiWorkflowMdodel(id);
            return View(an);
        }
        [HttpPost]
        public ActionResult DeleteAllPos(MyAnalisiPosAjax anapos)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                bool flagSecondaria = false;
                if (anapos.AnalisiPos_Secondaria.HasValue)
                {
                    flagSecondaria = anapos.AnalisiPos_Secondaria.Value;
                }

                List<VALPOS_POSIZIONI> lstDelete = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == anapos.AnalisiPos_MasterAnalisi_id && z.VALPOS_SECONDARIE == flagSecondaria).ToList<VALPOS_POSIZIONI>();
                foreach(VALPOS_POSIZIONI vp in lstDelete )
                {
                    en.VALPOS_POSIZIONI.DeleteObject(vp);   
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
        public ActionResult DeleteSinglePos(MyAnalisiPosAjaxList anaPosId)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                foreach (int posId in anaPosId.AnalisiPosIds)
                {
                    List<VALPOS_POSIZIONI> lstDelete = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_ID == posId).ToList<VALPOS_POSIZIONI>();
                    foreach (VALPOS_POSIZIONI vp in lstDelete)
                    {
                        en.VALPOS_POSIZIONI.DeleteObject(vp);
                    }
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
        public ActionResult ClonaPosizione(MyAnalisiPosAjaxList anaPosId)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                foreach (int posId in anaPosId.AnalisiPosIds)
                {
                    
                    List<VALPOS_POSIZIONI> lstSrcObjects = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_ID == posId).ToList<VALPOS_POSIZIONI>();
                    foreach (VALPOS_POSIZIONI vp in lstSrcObjects)
                    {
                        VALPOS_POSIZIONI lvp = clonaPosizione(vp);
                        en.VALPOS_POSIZIONI.AddObject(lvp);
                    }
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
        public ActionResult AddNewValPos(MyAnalisiPosAjax anapos)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALPOS_POSIZIONI vp=new VALPOS_POSIZIONI ();
                vp.VALPOS_VALORI_ID = anapos.AnalisiPos_MasterAnalisi_id;
                vp.VALPOS_COEFF_CONVERSIONE = 1;

                if (anapos.AnalisiPos_Secondaria.HasValue)
                {
                    vp.VALPOS_SECONDARIE = anapos.AnalisiPos_Secondaria.Value;
                }
                en.VALPOS_POSIZIONI.AddObject (vp);
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
        public ActionResult SaveInsertIntermedio(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            int new_id = 0;
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                string nuovocodice = "";
                ObjectParameter param = new ObjectParameter("CODICE",nuovocodice);
                en.USPT_GET_CODICE_INTERMEDIO(an.Analisi_Gruppo_id, param);
              
                if (string.IsNullOrEmpty((string)param.Value))
                {
                    throw new Exception("Impossibile generare un nuovo codice intermedio");
                }
                er = (string)param.Value;

                VALORI_VALORIZZAZIONI v = new VALORI_VALORIZZAZIONI ();
                v.VALORI_UTENTE_ID = this.CurrentUserID;
                v.VALORI_CODICE_INTERMEDIO = (string)param.Value;// an.Analisi_CodiceIntermedio;
                v.VALORI_DESC = an.Analisi_Descrizione;
                v.VALORI_PESO_POSITIVO = 100;
                v.VALORI_FLG_PONDERAZIONE = false;
                v.VALORI_COSTO_TOT = 0;
                v.VALORI_FLG_INTERNO = true;
                v.VALORI_FLG_BLOCCATO = false;
                v.VALORI_COSTO_TOT_DELIB = 0;
                v.VALORI_FLG_INTERM =true;
                v.VALORI_VN="";
                v.VALORI_MP_REV="";
                v.VALORI_T_STAVAL_ID = 11;
                if (an.Analisi_Flag_Reparto == 0)
                {
                    v.VALORI_GRUPPO_GRUREP_ID = an.Analisi_Gruppo_id;
                }
                else
                {
                    v.VALORI_REPARTO_GRUREP_ID = an.Analisi_Gruppo_id;
                    
                }
                en.VALORI_VALORIZZAZIONI.AddObject(v);
                en.SaveChanges();

                flagOK = true;
                new_id = v.VALORI_ID;
                if (new_id == 0)
                {
                    flagOK = false;
                }
                else 
                {
                    try
                    {
                        salvaTrackingAnalisi(v, en, DateTime.Now);
                        en.SaveChanges();
                    }catch {}
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er,id = new_id });
        }

        //sim 2022-02-17 Nuova funzione di duplicazione degli intermedi        
        [HttpPost]
        public ActionResult SaveDuplicateIntermedio(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            int new_id = 0;
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                string nuovocodice = "";
                ObjectParameter param = new ObjectParameter("CODICE",nuovocodice);
                en.USPT_GET_CODICE_INTERMEDIO(an.Analisi_Gruppo_id, param);
              
                if (string.IsNullOrEmpty((string)param.Value))
                {
                    throw new Exception("Impossibile generare un nuovo codice intermedio");
                }
                er = (string)param.Value;

                VALORI_VALORIZZAZIONI v_old = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();

                VALORI_VALORIZZAZIONI v = new VALORI_VALORIZZAZIONI ();
                v.VALORI_UTENTE_ID = this.CurrentUserID;
                v.VALORI_CODICE_INTERMEDIO = (string)param.Value;// an.Analisi_CodiceIntermedio;
                v.VALORI_DESC = an.Analisi_Descrizione;
                v.VALORI_PESO_POSITIVO = v_old.VALORI_PESO_POSITIVO;
                v.VALORI_FLG_PONDERAZIONE = v_old.VALORI_FLG_PONDERAZIONE;
                v.VALORI_COSTO_TOT = 0;
                v.VALORI_FLG_INTERNO = true;
                v.VALORI_FLG_BLOCCATO = false;
                v.VALORI_COSTO_TOT_DELIB = 0;
                v.VALORI_FLG_INTERM =true;
                v.VALORI_VN="";
                v.VALORI_MP_REV="";
                v.VALORI_T_STAVAL_ID = 11;
                if (an.Analisi_Flag_Reparto == 0)
                {
                    v.VALORI_GRUPPO_GRUREP_ID = an.Analisi_Gruppo_id;
                }
                else
                {
                    v.VALORI_REPARTO_GRUREP_ID = an.Analisi_Gruppo_id;
                    
                }
                en.VALORI_VALORIZZAZIONI.AddObject(v);
                en.SaveChanges();

                new_id = v.VALORI_ID;

                //clono le posizioni 
                List<VALPOS_POSIZIONI> posModello = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == an.Analisi_id).ToList<VALPOS_POSIZIONI>();
                List<VALPOS_POSIZIONI> newListPos = new List<VALPOS_POSIZIONI>();
                foreach (VALPOS_POSIZIONI pos in posModello)
                {
                    VALPOS_POSIZIONI currPos = clonaPosizione(pos);
                    // se Fase== Accettazione --> allora l'unità di misura e' Min

                    if (isAccettazione(pos.VALPOS_FASE_ID))// == 1) 
                    {
                        // currPos.VALPOS_T_UNIMIS_ID = 13;  // UdM Minuti
                        currPos.VALPOS_T_UNIMIS_ID = 25; // UdM Numero
                    }
                    //currPos.VALPOS_TOT = decimal.Round((currPos.VALPOS_QTA * currPos.VALPOS_COSTO_QTA.Value), 2, MidpointRounding.AwayFromZero);

                    string new_fase_desc = en.FASE.Where(z => z.FASE_ID == currPos.VALPOS_FASE_ID).SingleOrDefault().FASE_DESC;
                    //ricavo la medesima fase associata al nuovo gruppo se presente.
                    int num_fasi = en.FASE.Where(z => z.FASE_DESC == new_fase_desc && z.FASE_GRUREP_ID == an.Analisi_Gruppo_id).Count();

                    int? new_fase_id = null;
                    if (num_fasi == 1)  
                    {
                        new_fase_id = en.FASE.Where(z => z.FASE_DESC == new_fase_desc && z.FASE_GRUREP_ID == an.Analisi_Gruppo_id).SingleOrDefault().FASE_ID;
                    }
                    
                    currPos.VALPOS_FASE_ID = new_fase_id;
                    currPos.VALPOS_SECONDARIE = pos.VALPOS_SECONDARIE;
                    currPos.VALPOS_VALORI_ID = new_id;
                    newListPos.Add(currPos);
                }
                foreach (VALPOS_POSIZIONI newPos in newListPos)
                {
                    en.VALPOS_POSIZIONI.AddObject(newPos);
                    en.SaveChanges();
                }

                MyAnalisiAjax an_new = an;
                an_new.Analisi_id = new_id;
                an_new.Analisi_CodiceIntermedio = (string)param.Value;
                an_new.Analisi_Peso_Positivo = v_old.VALORI_PESO_POSITIVO.Value;

                saveValAnalisiTot(an_new, out er);

                flagOK = true;
               
                if (new_id == 0)
                {
                    flagOK = false;
                }
                else 
                {
                    try
                    {
                        salvaTrackingAnalisi(v, en, DateTime.Now);
                        en.SaveChanges();
                    }catch {}
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er,id = new_id });
        }
        

        private bool saveValAnalisiTot(IntermediModel an)
        {
            bool flagOK = true;
            
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi.Analisi_id).SingleOrDefault();
                //v.VALORI_FLG_PONDERAZIONE = an.Analisi_flgPonderazione;

                //v.VALORI_DIM_LOTTO = an.Analisi_Dim_Lotto ;
                //v.VALORI_MATRICE = an.Analisi_Matrice ;
                v.VALORI_CODICE_INTERMEDIO = an.Analisi.Analisi_CodiceGenerico;
                v.VALORI_DESC = an.Analisi.Analisi_Descrizione;
                v.VALORI_PESO_POSITIVO = an.Analisi.Analisi_Peso_Positivo;
                List<VALPOS_POSIZIONI> lstPos = new List<VALPOS_POSIZIONI>();
                if (v.VALORI_FLG_PONDERAZIONE)
                {
                    lstPos = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == an.Analisi.Analisi_id).ToList<VALPOS_POSIZIONI>();
                }
                else
                {
                    lstPos = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == an.Analisi.Analisi_id && z.VALPOS_SECONDARIE == false).ToList<VALPOS_POSIZIONI>();
                }
                decimal? totCorrente = 0;
                decimal? totCorrentePrim = 0;
                decimal? totCorrenteSec = 0;

                foreach (VALPOS_POSIZIONI pos in lstPos)
                {

                    if (pos.VALPOS_TOT.HasValue)
                    {
                        if (!pos.VALPOS_SECONDARIE)
                            totCorrentePrim += pos.VALPOS_TOT.Value;
                        else
                            totCorrenteSec += pos.VALPOS_TOT.Value;
                    }
                }
                decimal pesoPos = 0;
                decimal pesoNeg = 0;
                // N.B.
                // la vecchia formula  =>(SumPrimaria*(pesoPositivo)% + SumSecondaria*(100-pesoPositivo)%)
                // la formula attuale  =>(SumPrimaria*(100)% + SumSecondaria*(pesoPositivo)%) --> se non c'e' ponderazione => SumPrimaria*(100)%

                if (v.VALORI_FLG_PONDERAZIONE)
                {
                    pesoPos = an.Analisi.Analisi_Peso_Positivo.Value;
                    //pesoNeg = 100 - pesoPos;
                    pesoNeg = pesoPos;
                }
                else
                {
                    pesoPos = 100;
                    pesoNeg = 0;
                }
                //totCorrente = totCorrentePrim * (pesoPos / 100) + totCorrenteSec * (pesoNeg / 100);
                totCorrente = totCorrentePrim + totCorrenteSec * (pesoNeg / 100);

                if (totCorrente.HasValue)
                {
                    totCorrente = decimal.Round(totCorrente.Value, 2, MidpointRounding.AwayFromZero);
                }
                v.VALORI_COSTO_TOT = totCorrente;
                en.SaveChanges();
                flagOK = true;
            }
            catch (Exception ex)
            {
                flagOK = false;
            }

            return flagOK;
        }

        private bool saveValAnalisiTot(MyAnalisiAjax an, out string er)
        {
            bool flagOK = true;
            er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                //v.VALORI_FLG_PONDERAZIONE = an.Analisi_flgPonderazione;

                //v.VALORI_DIM_LOTTO = an.Analisi_Dim_Lotto ;
                //v.VALORI_MATRICE = an.Analisi_Matrice ;
                v.VALORI_CODICE_INTERMEDIO = an.Analisi_CodiceIntermedio;
                v.VALORI_DESC = an.Analisi_Descrizione;
                v.VALORI_PESO_POSITIVO = an.Analisi_Peso_Positivo;
                List<VALPOS_POSIZIONI> lstPos = new List<VALPOS_POSIZIONI>();
                if (v.VALORI_FLG_PONDERAZIONE)
                {
                    lstPos = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == an.Analisi_id).ToList<VALPOS_POSIZIONI>();
                }
                else
                {
                    lstPos = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == an.Analisi_id && z.VALPOS_SECONDARIE == false).ToList<VALPOS_POSIZIONI>();
                }
                decimal? totCorrente = 0;
                decimal? totCorrentePrim = 0;
                decimal? totCorrenteSec = 0;

                foreach (VALPOS_POSIZIONI pos in lstPos)
                {

                    if (pos.VALPOS_TOT.HasValue)
                    {
                        if (!pos.VALPOS_SECONDARIE)
                            totCorrentePrim += pos.VALPOS_TOT.Value;
                        else
                            totCorrenteSec += pos.VALPOS_TOT.Value;
                    }
                }
                decimal pesoPos = 0;
                decimal pesoNeg = 0;
                // N.B.
                // la vecchia formula  =>(SumPrimaria*(pesoPositivo)% + SumSecondaria*(100-pesoPositivo)%)
                // la formula attuale  =>(SumPrimaria*(100)% + SumSecondaria*(pesoPositivo)%) --> se non c'e' ponderazione => SumPrimaria*(100)%

                if (v.VALORI_FLG_PONDERAZIONE)
                {
                    pesoPos = an.Analisi_Peso_Positivo;
                    //pesoNeg = 100 - pesoPos;
                    pesoNeg = pesoPos;
                }
                else
                {
                    pesoPos = 100;
                    pesoNeg = 0;
                }
                //totCorrente = totCorrentePrim * (pesoPos / 100) + totCorrenteSec * (pesoNeg / 100);
                totCorrente = totCorrentePrim + totCorrenteSec * (pesoNeg / 100);

                if (totCorrente.HasValue)
                {
                    totCorrente = decimal.Round(totCorrente.Value, 2, MidpointRounding.AwayFromZero);
                }
                v.VALORI_COSTO_TOT = totCorrente;
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
        [HttpPost]
        public ActionResult SaveValAnalisiTot(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            flagOK = saveValAnalisiTot(an, out er);
            return Json(new { ok = flagOK, infopersonali = er });
        }
        private string insertErrore(int pos, string errore)
        {
            return insertErrore(pos, errore, false);
        }
        private string insertErrore(int pos, string errore ,bool flgSec)
        {
            if(!flgSec)
            return "[pos. n. " + pos.ToString() + "] " + errore;
            
            return "[pos. neg. n. " + pos.ToString() + "] " + errore;
        }

        //Mi restituisce il numero di intermedi annidati presenti nei padri!
        private int countLivelliIntermediPadre(int VALORI_ID, IZSLER_CAP_Entities en, int livelloAttuale, int maxLivello, List<string> elencoCodici)
        {
            if (livelloAttuale > maxLivello)
                return 0;

            //Carico tutte le posizioni che hanno "me stesso" come intermedio!
            List<VALPOS_POSIZIONI> lstPos = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_INTERM_ID == VALORI_ID).OrderBy(z => z.VALPOS_ID).ToList<VALPOS_POSIZIONI>();

            int maxCount = 0;
            int count = 0;
            //Devo controllare se vengo richiamato da un intermedio...
            foreach (VALPOS_POSIZIONI pos in lstPos)
            {
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == pos.VALPOS_VALORI_ID).Single<VALORI_VALORIZZAZIONI>();
                
                //Mi interessano solo gli itnermedi...
                if (!v.VALORI_FLG_INTERM) continue;

                count = countLivelliIntermediPadre(v.VALORI_ID, en, livelloAttuale + 1, maxLivello, elencoCodici) + 1 /* questa posizione */;
                if (count > maxCount)
                    maxCount = count;

                if (livelloAttuale + count > maxLivello)
                {
                    elencoCodici.Add(v.VALORI_CODICE_INTERMEDIO);
                }
            }

            return maxCount;
        }

        //Mi restituisce il numero di intermedi annidati presenti in questo intermedio. L'intermedio di partenza vale 1!!!
        private int countLivelliIntermediFigli(int VALORI_ID , IZSLER_CAP_Entities en, int livelloAttuale, int maxLivello)
        {
            if (livelloAttuale > maxLivello)
                return livelloAttuale;

            //Carico tutte le posizioni primarie dell'intermedio che riferiscono ad un altro intermedio
            List<VALPOS_POSIZIONI> lstPos = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == VALORI_ID && z.VALPOS_INTERM_ID != null).OrderBy(z => z.VALPOS_ID).ToList<VALPOS_POSIZIONI>();

            int maxCount = 0;
            //Ci sono posizioni che potrebbero essere intermedi!
            if (lstPos.Count > 0)
            {
                foreach (VALPOS_POSIZIONI pos in lstPos)
                {
                    bool isIntermedio = false;
                    //Controllo che la posizione non abbia al suo interno annidamenti superiori di intermedi rispetto al consentito!
                    if (pos.VALPOS_INTERM_ID.HasValue)
                    {
                        //Controllo se è un intermedio
                        VALORI_VALORIZZAZIONI vInt = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == pos.VALPOS_INTERM_ID).Single<VALORI_VALORIZZAZIONI>();

                        if (vInt.VALORI_FLG_INTERM)
                            isIntermedio = true;
                    }

                    if (isIntermedio) //La posizione è un intermedio..
                    {
                        int count = countLivelliIntermediFigli(pos.VALPOS_INTERM_ID.Value, en, livelloAttuale + 1, maxLivello);
                        if (count > maxCount)
                            maxCount = count;
                    }
                }
            }

            return maxCount + 1;
        }

        //Sim: Controllo in modo ricorsivo che un intermedio non contenga a sua volta un intermedio non validato. Se ce ne sono restituisco il codice corrispondente
        private bool IsIntermedioNonValidato(int VALORI_ID, IZSLER_CAP_Entities en, out string IntermedioNonValidato)
        {
            List<int> idStatoCorrettoAvanzamentoIntermedio = new List<int> { 10 };
            IntermedioNonValidato = "";
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

                    IsIntermedioNonValidato(pos.VALPOS_INTERM_ID.Value, en, out IntermedioNonValidato);

                }

            }
            return false;

        }

        private bool checkIntermedi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en, bool flagControllo, out string listaErrori)
        {
            LoadEntities le = new LoadEntities();

            listaErrori = "";
            string IntermedioNonValidato = ""; //sim
            List<int> idStatoCorrettoAvanzamentoIntermedio = new List<int> { 10 };
            //List<int> idStatoCorrettoAvanzamentoAnalisi = new List<int> { 4, 5, 6 };
            List<int> idStatoCorrettoAvanzamentoProdotto = new List<int> { 4, 5, 6, 8 };

            List<string> elencoErrori = new List<string>();
            int analisi_id = v.VALORI_ID;
            List<VALPOS_POSIZIONI> lstPosPrimarie = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == analisi_id && z.VALPOS_SECONDARIE == false).OrderBy(z => z.VALPOS_ID).ToList<VALPOS_POSIZIONI>();
            List<VALPOS_POSIZIONI> lstPosSecodarie = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == analisi_id && z.VALPOS_SECONDARIE == true).OrderBy(z => z.VALPOS_ID).ToList<VALPOS_POSIZIONI>();

            bool bAnalisi = false;
            bool erroreAnnidamento = false;
            //Controllo se l'intermedio fa parte di un gruppo prodotto o analisi
            if (v.VALORI_GRUPPO_GRUREP_ID.HasValue && v.VALORI_GRUPPO_GRUREP_ID.Value != 0)
                bAnalisi = true;

            int maxCiclo = 3;
            
            int livelloMassimoAnnidamentoFigli = 0;
            int livelloAttuale = 1;
            string value = string.Empty;
            if (bAnalisi)
            {
                value = le.GetSettings("INTERM_ANA");
            }
            else
            {
                value = le.GetSettings("INTERM_PRO");
            }

            int.TryParse(value, out maxCiclo);
            
            int count = 1;
            foreach (VALPOS_POSIZIONI pos in lstPosPrimarie)
            {
                if (pos.VALPOS_INTERM_ID == v.VALORI_ID)
                {
                    elencoErrori.Add(insertErrore(count, "Non è possibile usare lo stesso intermedio che si sta valorizzando."));
                    continue;
                }

                if (!pos.VALPOS_FASE_ID.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Fase non inserita"));
                }
                if (pos.VALPOS_QTA == 0)
                {
                    elencoErrori.Add(insertErrore(count, "Quantità non inserita"));
                }

                if (!pos.VALPOS_COSTO_QTA.HasValue || pos.VALPOS_COSTO_QTA == 0)
                {
                    elencoErrori.Add(insertErrore(count, "Importo unitario mancante"));
                }
                else if (pos.VALPOS_TOT == 0 || !pos.VALPOS_TOT.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Importo posizione mancante"));
                }

                if (!pos.VALPOS_T_UNIMIS_ID.HasValue && pos.VALPOS_PRODOT_ID.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Unità di misura non inserita."));
                }
                if (!pos.VALPOS_COEFF_CONVERSIONE.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Coef. di conversione non inserito."));
                }

                bool isIntermedio = false;
                //Controllo che la posizione non abbia al suo interno annidamenti superiori di intermedi rispetto al consentito!
                if (pos.VALPOS_INTERM_ID.HasValue)
                {
                    //Controllo se è un intermedio
                    VALORI_VALORIZZAZIONI vInt = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == pos.VALPOS_INTERM_ID).Single<VALORI_VALORIZZAZIONI>();

                    if (vInt.VALORI_FLG_INTERM)
                        isIntermedio = true;
                }

                if (isIntermedio) //La posizione è un intermedio..
                {
                    erroreAnnidamento = false;
                    if(maxCiclo <= 1) //Non è permesso
                    {
                        erroreAnnidamento = true;
                    }
                    else
                    {
                        int countLivello = countLivelliIntermediFigli(pos.VALPOS_INTERM_ID.Value, en, 1, maxCiclo);
                        if (countLivello > livelloMassimoAnnidamentoFigli)
                            livelloMassimoAnnidamentoFigli = countLivello;

                        if (livelloMassimoAnnidamentoFigli + livelloAttuale > maxCiclo)
                        {
                            erroreAnnidamento = true;
                        }
                    }

                    if (erroreAnnidamento)
                    {
                        if(bAnalisi)
                            elencoErrori.Add(insertErrore(count, "Superato il numero massimo di annidamenti di intermedi consentito per le analisi: " + maxCiclo));
                        else
                            elencoErrori.Add(insertErrore(count, "Superato il numero massimo di annidamenti di intermedi consentito per i prodotti: " + maxCiclo));
                    }
                }

                if (pos.VALPOS_INTERM_ID.HasValue)
                {
                    //Sono una posizione di intermendio o analisi
                    AnalisiModel an = new AnalisiModel(pos.VALPOS_INTERM_ID.Value);

                    if (an.Analisi.Analisi_flgIntermedio) //Intermedio
                    {
                        if (!idStatoCorrettoAvanzamentoIntermedio.Contains(an.Analisi.Analisi_T_Staval_id))
                        {
                            elencoErrori.Add(insertErrore(count, "Intermedio non bloccato."));
                            continue;
                        }

                        //verifico che l'intermedio non contenga a sua volta intermedi non validati pur essendo lui validato
                        if (IsIntermedioNonValidato(pos.VALPOS_INTERM_ID.Value, en, out IntermedioNonValidato) == true) //sim
                        {                                                                                         //sim
                            elencoErrori.Add(insertErrore(count, "La posizione contiene a sua volta il seguente intermedio non bloccato: " + IntermedioNonValidato)); //sim
                            continue;                                                                             //sim
                        }   
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

                if (pos.VALPOS_PRODOT_ID.HasValue)
                {
                    //La mia posizione è un prodotto
                    ProdottoModel pm = new ProdottoModel(pos.VALPOS_PRODOT_ID.Value);

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
            if (v.VALORI_FLG_PONDERAZIONE)
            {
                count = 1;
                foreach (VALPOS_POSIZIONI pos in lstPosSecodarie)
                {
                    if (pos.VALPOS_INTERM_ID == v.VALORI_ID)
                    {
                        elencoErrori.Add(insertErrore(count, "Non è possibile usare lo stesso intermedio che si sta valorizzando.", true));
                        continue;
                    }

                    if (!pos.VALPOS_FASE_ID.HasValue)
                    {
                        elencoErrori.Add(insertErrore(count, "Fase non inserita", true));
                    }
                    if (pos.VALPOS_QTA == 0)
                    {
                        elencoErrori.Add(insertErrore(count, "Quantità non inserita", true));
                    }

                    if (!pos.VALPOS_COSTO_QTA.HasValue || pos.VALPOS_COSTO_QTA == 0)
                    {
                        elencoErrori.Add(insertErrore(count, "Importo unitario mancante", true));
                    }
                    else if (pos.VALPOS_TOT == 0 || !pos.VALPOS_TOT.HasValue)
                    {
                        elencoErrori.Add(insertErrore(count, "Importo posizione mancante", true));
                    }

                    if (!pos.VALPOS_T_UNIMIS_ID.HasValue && pos.VALPOS_PRODOT_ID.HasValue)
                    {
                        elencoErrori.Add(insertErrore(count, "Unità di misura non inserita.", true));
                    }
                    if (!pos.VALPOS_COEFF_CONVERSIONE.HasValue)
                    {
                        elencoErrori.Add(insertErrore(count, "Coef. di conversione non inserito.", true));
                    }

                    bool isIntermedio = false;
                    //Controllo che la posizione non abbia al suo interno annidamenti superiori di intermedi rispetto al consentito!
                    if (pos.VALPOS_INTERM_ID.HasValue)
                    {
                        //Controllo se è un intermedio
                        VALORI_VALORIZZAZIONI vInt = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == pos.VALPOS_INTERM_ID).Single<VALORI_VALORIZZAZIONI>();

                        if (vInt.VALORI_FLG_INTERM)
                            isIntermedio = true;
                    }

                    if (isIntermedio) //La posizione è un intermedio..
                    {
                        erroreAnnidamento = false;
                        if (maxCiclo <= 1) //Non è permesso
                        {
                            erroreAnnidamento = true;
                        }
                        else
                        {

                            int countLivello = countLivelliIntermediFigli(pos.VALPOS_INTERM_ID.Value, en, 1, maxCiclo);
                            if (countLivello > livelloMassimoAnnidamentoFigli)
                                livelloMassimoAnnidamentoFigli = countLivello;

                            if (livelloMassimoAnnidamentoFigli + livelloAttuale > maxCiclo)
                            {
                                erroreAnnidamento = true;
                            }
                        }

                        if (erroreAnnidamento)
                        {
                            if (bAnalisi)
                                elencoErrori.Add(insertErrore(count, "Superato il numero massimo di annidamenti di intermedi consentito per le analisi: " + maxCiclo, true));
                            else
                                elencoErrori.Add(insertErrore(count, "Superato il numero massimo di annidamenti di intermedi consentito per i prodotti: " + maxCiclo, true));
                        }
                    }

                    if (pos.VALPOS_INTERM_ID.HasValue)
                    {
                        //Sono una posizione di intermendio o analisi
                        AnalisiModel an = new AnalisiModel(pos.VALPOS_INTERM_ID.Value);

                        if (an.Analisi.Analisi_flgIntermedio) //Intermedio
                        {
                            if (!idStatoCorrettoAvanzamentoIntermedio.Contains(an.Analisi.Analisi_T_Staval_id))
                            {
                                elencoErrori.Add(insertErrore(count, "Intermedio non bloccato.", true));
                                continue;
                            }
                            //verifico che l'intermedio non contenga a sua volta intermedi non validati pur essendo lui validato
                            if (IsIntermedioNonValidato(pos.VALPOS_INTERM_ID.Value, en, out IntermedioNonValidato) == true) //sim
                            {                                                                                         //sim
                                elencoErrori.Add(insertErrore(count, "La posizione contiene a sua volta il seguente intermedio non bloccato: " + IntermedioNonValidato)); //sim
                                continue;                                                                             //sim
                            }                                                                                         //sim
                        }
                        //else //Analisi
                        //{
                        //    if (!idStatoCorrettoAvanzamentoAnalisi.Contains(an.Analisi.Analisi_T_Staval_id))
                        //    {
                        //        elencoErrori.Add(insertErrore(count, "Analisi non validata.", true));
                        //        continue;
                        //    }
                        //}
                    }

                    if (pos.VALPOS_PRODOT_ID.HasValue)
                    {
                        //La mia posizione è un prodotto
                        ProdottoModel pm = new ProdottoModel(pos.VALPOS_PRODOT_ID.Value);

                        if (pm.Prodotto.Prodot_Flg_Interno) //Prodotto Interno
                        {
                            if (!idStatoCorrettoAvanzamentoProdotto.Contains(pm.Prodotto.Prodot_T_Stapro_Id.Value))
                            {
                                elencoErrori.Add(insertErrore(count, "Prodotto non validato.", true));
                                continue;
                            }
                        }
                    }
                    count++;
                }
            }

            //Visto che non ho l'errore di annidamento nei figli devo controllare dove questo intermedio viene usato e che non ci siano errori a salire..
            if (!erroreAnnidamento)
            {
                List<string> elencoCodici = new List<string>();
                int livelloPadri = countLivelliIntermediPadre(v.VALORI_ID, en, livelloMassimoAnnidamentoFigli + 1, maxCiclo, elencoCodici);
                if (livelloPadri + livelloMassimoAnnidamentoFigli + 1 > maxCiclo)
                {
                    if (bAnalisi)
                        elencoErrori.Add("Questo intermedio è già usato in altri intermedi: Superato il numero massimo di annidamenti consentito per le analisi: " + maxCiclo);
                    else
                        elencoErrori.Add("Questo intermedio è già usato in altri intermedi: Superato il numero massimo di annidamenti consentito per i prodotti: " + maxCiclo);

                    elencoErrori.Add("Intermedi che usano questo intermedio:");

                    foreach (string s in elencoCodici)
                    {
                        elencoErrori.Add(s);
                    }
                }
            }

            if (elencoErrori.Count == 0)
                return true;
            if (elencoErrori.Count != 0)
            {
                listaErrori = "";
                if (!flagControllo)
                {
                    listaErrori = "Impossibile bloccare l'intermedio.<br/>";
                }

                listaErrori += "Errori riscontrati:<br/>";
                foreach (string s in elencoErrori)
                {
                    listaErrori += s + "<br/>";
                }
            }
            return false;
        }
        private void chiudiRichiestaCorrente(int analisi_id,IZSLER_CAP_Entities en)
        {
            List<RICHIE_RICHIESTE> lstRic = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_VALORI_ID == analisi_id && z.RICHIE_T_STARIC_ID == 2).ToList<RICHIE_RICHIESTE>();
            foreach (RICHIE_RICHIESTE r in lstRic )
            {
                r.RICHIE_T_STARIC_ID = 3; // Evasa  
            }
        }
        private void creaRichiestaSbloccoAnalisi(VALORI_VALORIZZAZIONI v, string motivo, int priorita,IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = getDestinatarioUserID(v, "CDG", en);
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 1; // valorizzazione
            r.RICHIE_T_RICPRI_ID = priorita;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = motivo;
            r.RICHIE_TITOLO = "Richiesta sblocco analisi [" + v.VALORI_VN + " - " + v.VALORI_MP_REV + "]";
            r.RICHIE_VALORI_ID = v.VALORI_ID;
            en.RICHIE_RICHIESTE.AddObject(r);

        }
     
        private void creaRichiestaValidazioneAnalisi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en,DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = getDestinatarioUserID(v, "REFVAL",en);
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 1; // valorizzazione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = "Richiesta automatica validazione analisi";
            r.RICHIE_TITOLO = "Richiesta Validazione Analisi [" + v.VALORI_VN + " - " + v.VALORI_MP_REV + "]";
            r.RICHIE_VALORI_ID = v.VALORI_ID;
            en.RICHIE_RICHIESTE.AddObject(r);
        }
        private void creaRichiestaRispostaSblocco(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = v.VALORI_UTENTE_ID ;
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 1; // valorizzazione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = "L'analisi [" + v.VALORI_VN + " - " + v.VALORI_MP_REV + "] risulta sbloccata.";
            r.RICHIE_TITOLO = "Sblocco Analisi [" + v.VALORI_VN + " - " + v.VALORI_MP_REV + "]";
            r.RICHIE_VALORI_ID = v.VALORI_ID;
            en.RICHIE_RICHIESTE.AddObject(r);
        }
        private string getErroreDestinatarioUserID(VALORI_VALORIZZAZIONI v, string ProfilCodice, IZSLER_CAP_Entities en)
        {
            PROFIL_PROFILI p = en.PROFIL_PROFILI.Where(z => z.PROFIL_CODICE == ProfilCodice).SingleOrDefault();
            string err = "Impossibile attuare la richiesta.<br/>";
            err += "Nessun responsabile di tipo " + p.PROFIL_DESC + " è stato trovato per il Gruppo";
            if (v.VALORI_GRUPPO_GRUREP_ID.HasValue)
            {
                GRUREP_GRUPPI_REPARTI gr = en.GRUREP_GRUPPI_REPARTI.Where(z => z.GRUREP_ID == v.VALORI_GRUPPO_GRUREP_ID.Value).SingleOrDefault();
                err += gr.GRUREP_DESC;
            }
            if (v.VALORI_REPARTO_GRUREP_ID.HasValue)
            {
                GRUREP_GRUPPI_REPARTI gr = en.GRUREP_GRUPPI_REPARTI.Where(z => z.GRUREP_ID == v.VALORI_REPARTO_GRUREP_ID.Value).SingleOrDefault();
                err += gr.GRUREP_DESC;
            }
            err += ".";
            return err;
        }

        private int getDestinatarioUserID(VALORI_VALORIZZAZIONI v, string ProfilCodice, IZSLER_CAP_Entities en)
        {
            if (v.VALORI_GRUPPO_GRUREP_ID.HasValue)
            {
                PROFIL_PROFILI p  = en.PROFIL_PROFILI.Where(z => z.PROFIL_CODICE == ProfilCodice).SingleOrDefault();

                M_UTPRGR_UTENTI_PROFILI_GRUPPI g = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Include("PROFIL_PROFILI").
                    Where(x => x.M_UTPRGR_GRUREP_ID ==  v.VALORI_GRUPPO_GRUREP_ID.Value 
                        && x.M_UTPRGR_FLG_PRINCIPALE == true
                        && x.M_UTPRGR_PROFIL_ID == p.PROFIL_ID).Take(1).SingleOrDefault();
                if (g != null)
                {

                    return g.M_UTPRGR_UTENTE_ID;
                }
            }
            if (v.VALORI_REPARTO_GRUREP_ID.HasValue)
            {
                PROFIL_PROFILI p = en.PROFIL_PROFILI.Where(z => z.PROFIL_CODICE == ProfilCodice).SingleOrDefault();

                M_UTPRGR_UTENTI_PROFILI_GRUPPI g = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Include("PROFIL_PROFILI").
                    Where(x => x.M_UTPRGR_GRUREP_ID == v.VALORI_REPARTO_GRUREP_ID.Value
                        && x.M_UTPRGR_FLG_PRINCIPALE == true
                        && x.M_UTPRGR_PROFIL_ID == p.PROFIL_ID).Take(1).SingleOrDefault();
                if (g != null)
                {
                    return g.M_UTPRGR_UTENTE_ID;
                }
            }
            return 0;
        }
       
        private void salvaTrackingAnalisi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en ,DateTime dt)
        {
            TRKVAL_VALORIZZAZIONI_TRACKING trk = new TRKVAL_VALORIZZAZIONI_TRACKING();
            trk.TRKVAL_COSTO_TOT = v.VALORI_COSTO_TOT;
            trk.TRKVAL_DATA_INS = dt;
            trk.TRKVAL_DIM_LOTTO = v.VALORI_DIM_LOTTO; // non usato da intermedio
            trk.TRKVAL_FLG_PONDERAZIONE = v.VALORI_FLG_PONDERAZIONE;
            //trk.TRKVAL_ID 
            trk.TRKVAL_PESO_POSITIVO = v.VALORI_PESO_POSITIVO;
            trk.TRKVAL_T_STAVAL_ID = v.VALORI_T_STAVAL_ID;
            trk.TRKVAL_UTENTE_ID = this.CurrentUserID;
            trk.TRKVAL_VALORI_ID = v.VALORI_ID;
            trk.TRKVAL_VALORI_UTENTE_ID = v.VALORI_UTENTE_ID.Value;
            
            trk.TRKVAL_DESC_INTERMEDIO = v.VALORI_DESC;
            trk.TRKVAL_GRUPPO_GRUREP_ID = v.VALORI_GRUPPO_GRUREP_ID;
            trk.TRKVAL_REPARTO_GRUREP_ID = v.VALORI_REPARTO_GRUREP_ID;
            trk.TRKVAL_COSTO_TOT_DELIB = v.VALORI_COSTO_TOT_DELIB;
            trk.TRKVAL_CODICE_INTERMEDIO = v.VALORI_CODICE_INTERMEDIO;

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
            salvaTrackingAnalisiPos(v.VALORI_ID, dt, en);
        }
        private void salvaTrackingAnalisiPos(int valori_id, DateTime dt, IZSLER_CAP_Entities en)
        {
            List<VALPOS_POSIZIONI> lstPos= en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == valori_id).ToList<VALPOS_POSIZIONI>();
            foreach (VALPOS_POSIZIONI pos in lstPos)
            {
                trakingAnalisiPos(pos, dt, en);
            }
        }
        private void trakingAnalisiPos(VALPOS_POSIZIONI pos, DateTime dt, IZSLER_CAP_Entities en)
        {
            TRKVPS_VALORIPOS_TRACKING trkp=new TRKVPS_VALORIPOS_TRACKING ();
            trkp.TRKVAL_DATA_INS = dt;
            trkp.TRKVAL_UTENTE_ID = this.CurrentUserID;
            trkp.TRKVPS_VALPOS_COEFF_CONVERSIONE = pos.VALPOS_COEFF_CONVERSIONE;

            trkp.TRKVPS_VALPOS_COSTO_QTA = pos.VALPOS_COSTO_QTA;
            trkp.TRKVPS_VALPOS_DESC = pos.VALPOS_DESC ;
            trkp.TRKVPS_VALPOS_FASE_ID = pos.VALPOS_FASE_ID;

            trkp.TRKVPS_VALPOS_FIGPRO_ID = pos.VALPOS_FIGPRO_ID;
            trkp.TRKVPS_VALPOS_ID  = pos.VALPOS_ID;
            trkp.TRKVPS_VALPOS_INTERM_ID = pos.VALPOS_INTERM_ID;
            trkp.TRKVPS_VALPOS_PRODOT_ID = pos.VALPOS_PRODOT_ID;
            trkp.TRKVPS_VALPOS_QTA = pos.VALPOS_QTA;
            trkp.TRKVPS_VALPOS_SECONDARIE = pos.VALPOS_SECONDARIE;
            trkp.TRKVPS_VALPOS_T_UNIMIS_ID = pos.VALPOS_T_UNIMIS_ID;
            trkp.TRKVPS_VALPOS_TOT = pos.VALPOS_TOT;
            trkp.TRKVPS_VALPOS_VALORI_ID = pos.VALPOS_VALORI_ID;
            trkp.TRKVPS_VALPOS_COD_SETTORE = pos.VALPOS_COD_SETTORE;
            trkp.TRKVPS_VALPOS_MACCHI_ID = pos.VALPOS_MACCHI_ID;
            en.TRKVPS_VALORIPOS_TRACKING .AddObject (trkp);
        }

        [HttpPost]
        public ActionResult CheckPosizioniIntermedi(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                string listaErrori = "";
                bool flgOKIntermedio= checkIntermedi(v, en, true, out listaErrori);
                if (!flgOKIntermedio)
                {
                    flagOK = false;
                    er = listaErrori;
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
        public ActionResult SbloccaIntermedio(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                T_STAVAL_STATO_VALORIZZAZIONE tStaval = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z => z.T_STAVAL_CODICE == "INVAL").SingleOrDefault();
                chiudiRichiestaCorrente(v.VALORI_ID, en);
                v.VALORI_T_STAVAL_ID = tStaval.T_STAVAL_ID;
                v.VALORI_FLG_BLOCCATO = false;
                v.VALORI_T_STAVAL_ID = 11;
               // creaRichiestaRispostaSblocco(v, en, dt);
                salvaTrackingAnalisi(v, en, dt);
                en.SaveChanges();
                flagOK = true;
                attualizzaPosizioniIntermedio(v);
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        private void attualizzaPosizioniIntermedio(VALORI_VALORIZZAZIONI v)
        {
            AttualizzatorePosizioni a = new AttualizzatorePosizioni(v);
            a.Attualizza();
        }

        //private void attualizzaPosizioniIntermedio(int valori_id)
        //{
        //    IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
        //    VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == valori_id).SingleOrDefault();
        //    AttualizzatorePosizioni a = new AttualizzatorePosizioni(v);
        //    a.Attualizza();
        //}

        //sim verifico che l'intermedio sia stato aggiornato da non più di un ora prima di effettuare il blocco.
        public bool IsAggiornata(VALORI_VALORIZZAZIONI v, out string listaErrori)
        {
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

            listaErrori = "";
            if (!v.VALORI_TS_AGGIORNAMENTO_POSIZIONI.HasValue || DateTime.Now.Subtract(v.VALORI_TS_AGGIORNAMENTO_POSIZIONI.Value).TotalHours > 1)
            {
                listaErrori = "Per bloccare l'intermedio è necessario aver premuto prima sul bottone \"Aggiorna posizioni\" <br/>";
                return false;
            }
            else
            {
                return true;
            }
        }

        [HttpPost]
        public ActionResult BloccaIntermedio(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            flagOK = saveValAnalisiTot(an, out er);
            if (flagOK)
            {
                try
                {
                    DateTime dt = DateTime.Now;
                    IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                    //sempre update
                    VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();

                    string listaErrori = "";
                    string msg_da_aggiornare = ""; //sim
                    bool flgOKIntermedio = checkIntermedi(v, en, false, out listaErrori);
                    bool flgOKAggiornata = IsAggiornata(v, out msg_da_aggiornare);//sim
                    if (flgOKIntermedio && flgOKAggiornata)
                    {

                        chiudiRichiestaCorrente(v.VALORI_ID, en);
                        v.VALORI_FLG_BLOCCATO = true;
                        v.VALORI_COSTO_TOT_DELIB = v.VALORI_COSTO_TOT;
                        v.VALORI_T_STAVAL_ID = 10;
                        //creaRichiestaRispostaSblocco(v, en, dt);
                        salvaTrackingAnalisi(v, en, dt);
                        en.SaveChanges();
                        flagOK = true;
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
        public ActionResult SaveValAnalisi(MyAnalisiAjax an)
        { 
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                v.VALORI_FLG_PONDERAZIONE = an.Analisi_flgPonderazione;
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
        public ActionResult SaveValPos(MyAnalisiPosAjax anapos)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALPOS_POSIZIONI vp =  en.VALPOS_POSIZIONI.Where(z => z.VALPOS_ID == anapos.AnalisiPos_id).SingleOrDefault();
                TipoSave l = (TipoSave)Enum.Parse(typeof(TipoSave), anapos.TipoSalvataggio);
                switch (l)
                {

                    case TipoSave.UdMRatio :
                        string convFlg= anapos.AnalisiPos_CoeffConversioneString;
                        try 
                        {
                            double valConversione = double.Parse(convFlg);
                            vp.VALPOS_COEFF_CONVERSIONE = valConversione;
                        }
                        catch 
                        { }
                        
                        break;
                    case TipoSave.PulisciCosto:
                        vp.VALPOS_COSTO_QTA  = null;
                        break;
                    case TipoSave.Macchinario:
                        if (anapos.AnalisiPos_Macchinario_id == 0)
                            vp.VALPOS_MACCHI_ID = null;
                        else
                            vp.VALPOS_MACCHI_ID = anapos.AnalisiPos_Macchinario_id;

                        if (anapos.AnalisiPos_QuantitaCosto == 0)
                            vp.VALPOS_COSTO_QTA  = null;
                        else
                            vp.VALPOS_COSTO_QTA = anapos.AnalisiPos_QuantitaCosto;

                        vp.VALPOS_COEFF_CONVERSIONE = 1;
                        // pulizia livello fig prof
                        vp.VALPOS_FIGPRO_ID = null;
                        // pulizia Prodotto
                        vp.VALPOS_PRODOT_ID = null;
                        // pulizia IntermedioAnalisi
                        vp.VALPOS_INTERM_ID = null;
                        // UdM minuto
                        vp.VALPOS_T_UNIMIS_ID = 13; // UdM minuto
                            
                        break;
                    case TipoSave.AnalisiIntermedio:
                            if (anapos.AnalisiPos_Analisi_id == 0)
                                vp.VALPOS_INTERM_ID = null;
                            else
                                vp.VALPOS_INTERM_ID = anapos.AnalisiPos_Analisi_id;

                            if (anapos.AnalisiPos_QuantitaCosto == 0)
                                vp.VALPOS_COSTO_QTA  = null;
                            else
                                vp.VALPOS_COSTO_QTA = anapos.AnalisiPos_QuantitaCosto;

                            vp.VALPOS_COEFF_CONVERSIONE = 1;
                            // pulizia livello fig prof
                            vp.VALPOS_FIGPRO_ID = null;
                            // pulizia Prodotto
                            vp.VALPOS_PRODOT_ID = null;
                            // pulizia Macchinario
                            vp.VALPOS_MACCHI_ID = null;
                            
                            break;
                    case TipoSave.FaseAccettazione:
                            if (anapos.AnalisiPos_Fase_id == 0)
                                vp.VALPOS_FASE_ID = null;
                            else
                                vp.VALPOS_FASE_ID = anapos.AnalisiPos_Fase_id;

                            if (anapos.AnalisiPos_QuantitaCosto == 0)
                                vp.VALPOS_COSTO_QTA = null;
                            else
                                vp.VALPOS_COSTO_QTA = anapos.AnalisiPos_QuantitaCosto;

                            vp.VALPOS_COEFF_CONVERSIONE = 1;
                            // pulizia Intermedio
                            vp.VALPOS_INTERM_ID = null;
                            // pulizia livello fig prof
                            vp.VALPOS_FIGPRO_ID = null;
                            // pulizia Prodotto
                            vp.VALPOS_PRODOT_ID = null;
                            // pulizia Macchinario
                            vp.VALPOS_MACCHI_ID = null;

                            //vp.VALPOS_T_UNIMIS_ID = 13; // minuti
                            vp.VALPOS_T_UNIMIS_ID = 25; // UdM Numero
                            break;
                    case TipoSave.Fase:
                            if (anapos.AnalisiPos_Fase_id == 0)
                                vp.VALPOS_FASE_ID  = null;
                            else
                                vp.VALPOS_FASE_ID = anapos.AnalisiPos_Fase_id;
                            vp.VALPOS_FIGPRO_ID = null;

                            if (anapos.AnalisiPos_UdM_id == 0)
                                vp.VALPOS_T_UNIMIS_ID = null;
                            else
                                vp.VALPOS_T_UNIMIS_ID = anapos.AnalisiPos_UdM_id;
                        //    vp.VALPOS_T_UNIMIS_ID = 13; // minuti
                            break;

                    case TipoSave.Descrizione:
                            //if (anapos.AnalisiPos_desc == "")
                            //    vp.VALPOS_DESC = null;
                            //else
                                vp.VALPOS_DESC = anapos.AnalisiPos_desc;
                            break;

                    case TipoSave.Quantita:
                                vp.VALPOS_QTA = anapos.AnalisiPos_Quantita ;
                            break;

                    case TipoSave.Livello:
                            if (anapos.AnalisiPos_FigProf_id == 0)
                                vp.VALPOS_FIGPRO_ID = null;
                            else
                                vp.VALPOS_FIGPRO_ID = anapos.AnalisiPos_FigProf_id;
                            if (anapos.AnalisiPos_QuantitaCosto == 0)
                                vp.VALPOS_COSTO_QTA = null;
                            else
                                vp.VALPOS_COSTO_QTA = anapos.AnalisiPos_QuantitaCosto;

                            vp.VALPOS_COEFF_CONVERSIONE = 1;
                            // pulizia IntermedioAnalisi
                            vp.VALPOS_INTERM_ID = null;
                            // pulizia Prodotto
                            vp.VALPOS_PRODOT_ID = null;
                            // pulizia Macchinario
                            vp.VALPOS_MACCHI_ID = null;
                            if (anapos.AnalisiPos_UdM_id == 0)
                                vp.VALPOS_T_UNIMIS_ID = null;
                            else
                                vp.VALPOS_T_UNIMIS_ID = anapos.AnalisiPos_UdM_id;
                            break;

                    case TipoSave.Prodotto:
                            if (anapos.AnalisiPos_Prodotto_id == 0)
                                vp.VALPOS_PRODOT_ID = null;
                            else
                                vp.VALPOS_PRODOT_ID = anapos.AnalisiPos_Prodotto_id;
                            if (anapos.AnalisiPos_QuantitaCosto == 0)
                                vp.VALPOS_COSTO_QTA = null;
                            else
                                vp.VALPOS_COSTO_QTA = anapos.AnalisiPos_QuantitaCosto;


                            string convFlgProd= anapos.AnalisiPos_CoeffConversioneString;
                            if (convFlgProd !=null && convFlgProd != "")
                            {
                                try
                                {
                                    double valConversione = double.Parse(convFlgProd);
                                    vp.VALPOS_COEFF_CONVERSIONE = valConversione;
                                }
                                catch
                                { }
                            }
                            else
                                vp.VALPOS_COEFF_CONVERSIONE = null;
                            
                            // pulizia IntermedioAnalisi
                            vp.VALPOS_INTERM_ID = null;
                            // pulizia Figura professionale Livello
                            vp.VALPOS_FIGPRO_ID = null;
                            // pulizia Macchinario
                            vp.VALPOS_MACCHI_ID = null;
                            break;

                    case TipoSave.PrezzoPosizione:
                            vp.VALPOS_TOT = anapos.AnalisiPos_QuantitaCosto;
                            break;

                    case TipoSave.UdM:
                            if (anapos.AnalisiPos_UdM_id == 0)
                                vp.VALPOS_T_UNIMIS_ID = null;
                            else
                                vp.VALPOS_T_UNIMIS_ID = anapos.AnalisiPos_UdM_id;

                            if (anapos.AnalisiPos_CoeffConversioneString == "1")
                                vp.VALPOS_COEFF_CONVERSIONE = 1;
                            if (anapos.AnalisiPos_CoeffConversioneString == "clear")
                                vp.VALPOS_COEFF_CONVERSIONE = null;
                            if (anapos.AnalisiPos_CoeffConversioneString != "1" && anapos.AnalisiPos_CoeffConversioneString != "clear")
                            {
                                string convFlgProd2 = anapos.AnalisiPos_CoeffConversioneString;
                                if (convFlgProd2 != null && convFlgProd2 != "")
                                {
                                    try
                                    {
                                        double valConversione2 = double.Parse(convFlgProd2);
                                        vp.VALPOS_COEFF_CONVERSIONE = valConversione2;
                                    }
                                    catch
                                    { }
                                }
                            }
                            break;
                }
                double res= 0;
                if (vp.VALPOS_COSTO_QTA.HasValue && vp.VALPOS_COEFF_CONVERSIONE.HasValue)
                {
                    res = ((double)(vp.VALPOS_COSTO_QTA.Value * vp.VALPOS_QTA)) * vp.VALPOS_COEFF_CONVERSIONE.Value;
                    vp.VALPOS_TOT = (decimal)res;
                }
                else
                    vp.VALPOS_TOT = null;
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

        public ActionResult PopUpIntermediEsplosi(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            IntermediModel an = new IntermediModel(id);

            return View(an);
        }
    }
}
