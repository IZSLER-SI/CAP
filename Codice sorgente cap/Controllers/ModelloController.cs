using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Helpers;
using IZSLER_CAP.Models;

namespace IZSLER_CAP.Controllers
{
    public class ModelloController : B16Controller
    {
        public ModelloController()
            : base()
        {
            this.SetMessage= "Modelli";
            this.SetLiModelli = "current";
        }
        //
        // GET: /Modello/

        //public ActionResult Index()
        //{
        //    RedirectToRouteResult r = CheckLogin();
        //    if (r != null) { return r; }
        //    ListaModelliModel lm = new ListaModelliModel();
        //    return View(lm);
        //}

        public ActionResult Index(int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ListaModelliModel an = new ListaModelliModel();
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

            List<MyAnalisi> listAnalisi = an.GetElencoModelli(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).ToList<MyAnalisi>();

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
            ListaModelliModel an = new ListaModelliModel();
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
            List<MyAnalisi> listAnalisi = an.GetElencoModelli(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).ToList<MyAnalisi>();
            an.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listAnalisi.Count() / an.NumEntities));
            if (an.CurrentPage > an.NumberOfPages) an.CurrentPage = an.NumberOfPages;
            an.Data = listAnalisi.Skip(an.NumEntities * (an.CurrentPage - 1)).Take(an.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_Index", an) });

        }
        public ActionResult ModelloInsert()
        {
            B16ModelMgr b = new B16ModelMgr();
            return View(b);
        }
        public ActionResult ModelloEdit(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Dettaglio modello";
            ModelloModel an = new ModelloModel(id);
            return View(an);

        }
        #region gestione salvataggi ajax
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetImportoFaseAccettazione(string analisi_id, string Attivi_id, string valpos_id)
        {
            if (Attivi_id == "") Attivi_id = "0";
            if (valpos_id == "") valpos_id = "0";
            if (analisi_id == "") analisi_id = "0";
            ModelloModel f = new ModelloModel(int.Parse(analisi_id));
            LoadEntities le = new LoadEntities();
            int grurep_ID = 0;
            if (f.Analisi.Analisi_Gruppo_id.HasValue)
                grurep_ID = f.Analisi.Analisi_Gruppo_id.Value;
            if (f.Analisi.Analisi_Reparto_id.HasValue)
                grurep_ID = f.Analisi.Analisi_Reparto_id.Value;
            MyGrurep mgr = le.GetRepartoGruppo(grurep_ID);

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
        public JsonResult GetElencoFigProfessionali(string analisi_id, string Attivi_id, string valpos_id)
        {
            if (Attivi_id == "") Attivi_id = "0";
            if (valpos_id == "") valpos_id = "0";
            if (analisi_id == "") valpos_id = "0";
            AnalisiModel f = new AnalisiModel(int.Parse(analisi_id));
            //  f.SetListaFigProf(int.Parse(Attivi_id));
            MyAnalisiPos pos = f.ElencoAnalisiPos.Where(z => z.AnalisiPos_id == int.Parse(valpos_id)).SingleOrDefault();
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
            MyAnalisiPos pos = f.ElencoAnalisiPosSec.Where(z => z.AnalisiPos_id == int.Parse(valpos_id)).SingleOrDefault();
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
        public ActionResult SaveInsertModello(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            int new_id = 0;
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

                VALORI_VALORIZZAZIONI v = new VALORI_VALORIZZAZIONI();
                v.VALORI_UTENTE_ID = this.CurrentUserID;
                v.VALORI_CODICE_INTERMEDIO = an.Analisi_CodiceIntermedio;
                v.VALORI_DESC = an.Analisi_Descrizione;
                v.VALORI_PESO_POSITIVO = 100;
                v.VALORI_FLG_PONDERAZIONE = false;
                v.VALORI_COSTO_TOT = 0;
                v.VALORI_FLG_INTERNO = true;
                v.VALORI_FLG_BLOCCATO = false;
                v.VALORI_COSTO_TOT_DELIB = 0;
                v.VALORI_FLG_INTERM = false;
                v.VALORI_VN = "";
                v.VALORI_MP_REV = "";
                v.VALORI_T_STAVAL_ID = 12;
                v.VALORI_FLAG_MODELLO = true;
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
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = new_id });
        }
        [HttpPost]
        public ActionResult SbloccaModello(MyAnalisiAjax an)
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
                v.VALORI_T_STAVAL_ID = 12;
                // creaRichiestaRispostaSblocco(v, en, dt);
                salvaTrackingAnalisi(v, en, dt);
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
        public ActionResult BloccaModello(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                chiudiRichiestaCorrente(v.VALORI_ID, en);
                v.VALORI_FLG_BLOCCATO = true;
                v.VALORI_COSTO_TOT_DELIB = v.VALORI_COSTO_TOT;
                v.VALORI_T_STAVAL_ID = 13;
                //creaRichiestaRispostaSblocco(v, en, dt);
                salvaTrackingAnalisi(v, en, dt);
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
        public ActionResult EliminaModello(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                chiudiRichiestaCorrente(v.VALORI_ID, en);
                List<VALPOS_POSIZIONI> lstVP = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == v.VALORI_ID).ToList<VALPOS_POSIZIONI>();
                foreach (VALPOS_POSIZIONI vp in lstVP)
                {
                    en.VALPOS_POSIZIONI.DeleteObject(vp); 
                }
                en.SaveChanges();
                
                en.VALORI_VALORIZZAZIONI.DeleteObject(v); 
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
                VALPOS_POSIZIONI vp = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_ID == anapos.AnalisiPos_id).SingleOrDefault();
                TipoSave l = (TipoSave)Enum.Parse(typeof(TipoSave), anapos.TipoSalvataggio);
                switch (l)
                {

                    case TipoSave.UdMRatio:
                        string convFlg = anapos.AnalisiPos_CoeffConversioneString;
                        try
                        {
                            double valConversione = double.Parse(convFlg);
                            vp.VALPOS_COEFF_CONVERSIONE = valConversione;
                        }
                        catch
                        { }

                        break;
                    case TipoSave.PulisciCosto:
                        vp.VALPOS_COSTO_QTA = null;
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
                            vp.VALPOS_FASE_ID = null;
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
                        vp.VALPOS_QTA = anapos.AnalisiPos_Quantita;
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


                        string convFlgProd = anapos.AnalisiPos_CoeffConversioneString;
                        if (convFlgProd != null && convFlgProd != "")
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
                double res = 0;
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
        #endregion
        #region pressione bottoni 

        [HttpPost]
        public ActionResult SaveValAnalisiTot(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
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

                if (v.VALORI_FLG_PONDERAZIONE)
                {
                    pesoPos = an.Analisi_Peso_Positivo;
                    pesoNeg = 100 - pesoPos;
                }
                else
                {
                    pesoPos = 100;
                }
                totCorrente = totCorrentePrim * (pesoPos / 100) + totCorrenteSec * (pesoNeg / 100);
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

            return Json(new { ok = flagOK, infopersonali = er });
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
        [HttpPost]
        public ActionResult AddNewValPos(MyAnalisiPosAjax anapos)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALPOS_POSIZIONI vp = new VALPOS_POSIZIONI();
                vp.VALPOS_VALORI_ID = anapos.AnalisiPos_MasterAnalisi_id;
                vp.VALPOS_COEFF_CONVERSIONE = 1;

                if (anapos.AnalisiPos_Secondaria.HasValue)
                {
                    vp.VALPOS_SECONDARIE = anapos.AnalisiPos_Secondaria.Value;
                }
                en.VALPOS_POSIZIONI.AddObject(vp);
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
                foreach (VALPOS_POSIZIONI vp in lstDelete)
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
        #endregion
        #region tracking
        private void chiudiRichiestaCorrente(int analisi_id, IZSLER_CAP_Entities en)
        {
            List<RICHIE_RICHIESTE> lstRic = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_VALORI_ID == analisi_id && z.RICHIE_T_STARIC_ID == 2).ToList<RICHIE_RICHIESTE>();
            foreach (RICHIE_RICHIESTE r in lstRic)
            {
                r.RICHIE_T_STARIC_ID = 3; // Evasa  
            }
        }
        private void salvaTrackingAnalisi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en, DateTime dt)
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
            List<VALPOS_POSIZIONI> lstPos = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == valori_id).ToList<VALPOS_POSIZIONI>();
            foreach (VALPOS_POSIZIONI pos in lstPos)
            {
                trakingAnalisiPos(pos, dt, en);
            }
        }
        private void trakingAnalisiPos(VALPOS_POSIZIONI pos, DateTime dt, IZSLER_CAP_Entities en)
        {
            TRKVPS_VALORIPOS_TRACKING trkp = new TRKVPS_VALORIPOS_TRACKING();
            trkp.TRKVAL_DATA_INS = dt;
            trkp.TRKVAL_UTENTE_ID = this.CurrentUserID;
            trkp.TRKVPS_VALPOS_COEFF_CONVERSIONE = pos.VALPOS_COEFF_CONVERSIONE;

            trkp.TRKVPS_VALPOS_COSTO_QTA = pos.VALPOS_COSTO_QTA;
            trkp.TRKVPS_VALPOS_DESC = pos.VALPOS_DESC;
            trkp.TRKVPS_VALPOS_FASE_ID = pos.VALPOS_FASE_ID;

            trkp.TRKVPS_VALPOS_FIGPRO_ID = pos.VALPOS_FIGPRO_ID;
            trkp.TRKVPS_VALPOS_ID = pos.VALPOS_ID;
            trkp.TRKVPS_VALPOS_INTERM_ID = pos.VALPOS_INTERM_ID;
            trkp.TRKVPS_VALPOS_PRODOT_ID = pos.VALPOS_PRODOT_ID;
            trkp.TRKVPS_VALPOS_QTA = pos.VALPOS_QTA;
            trkp.TRKVPS_VALPOS_SECONDARIE = pos.VALPOS_SECONDARIE;
            trkp.TRKVPS_VALPOS_T_UNIMIS_ID = pos.VALPOS_T_UNIMIS_ID;
            trkp.TRKVPS_VALPOS_TOT = pos.VALPOS_TOT;
            trkp.TRKVPS_VALPOS_VALORI_ID = pos.VALPOS_VALORI_ID;
            trkp.TRKVPS_VALPOS_COD_SETTORE = pos.VALPOS_COD_SETTORE;
            trkp.TRKVPS_VALPOS_MACCHI_ID = pos.VALPOS_MACCHI_ID;
            en.TRKVPS_VALORIPOS_TRACKING.AddObject(trkp);
        }
        #endregion
        #region POP UP  Modelli
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPGruppiReparti(int id, int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int analisi_ID = id;

            ListaGruppiRepartiModel IA = new ListaGruppiRepartiModel(analisi_ID, this.CurrentUserID, this.CurrentProfileID);
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
        #endregion

    }
}
