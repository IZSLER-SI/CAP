using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using MvcContrib.UI.Grid;
using IZSLER_CAP.Models;
using IZSLER_CAP.Helpers;
using System.IO;

namespace IZSLER_CAP.Controllers
{
    public static partial class ControllerExtensions
    {
        public static string RenderPartialViewToString(this ControllerBase controller, string partialPath, object model)
        {
            if (string.IsNullOrEmpty(partialPath))
                partialPath = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialPath);
                ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                // copy model state items to the html helper
                foreach (var item in viewContext.Controller.ViewData.ModelState)
                    if (!viewContext.ViewData.ModelState.Keys.Contains(item.Key))
                    {
                        viewContext.ViewData.ModelState.Add(item);
                    }


                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }

    public class SettingsController : B16Controller
    {
        //
        // GET: /Settings/
        public SettingsController()
            : base()
        {
            this.SetMessage = "Settings";
            this.SetLiSettings = "current";

        }
        [HttpPost]
        public ActionResult SaveNewFase(MyFaseAjax fase)
        { 
            bool flagOK = true;
            string er="";
            try 
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                FASE f = new FASE();
              
                f.FASE_CODICE = fase.Fase_Codice;
                f.FASE_DESC= fase.Fase_Desc ;
                f.FASE_FASE_ID = fase.Fase_Fase_ID;
                f.FASE_GRUREP_ID = fase.Fase_Grurep_ID;
                en.FASE.AddObject(f);
                en.SaveChanges();
                flagOK = true;
            
                if (fase.Fase_Fase_ID != 0)
                {
                    List<M_FIGATT_FIGURAPROF_ATTIVITA> sm = en.M_FIGATT_FIGURAPROF_ATTIVITA.Where(x => x.M_FIGATT_FASE_ID == fase.Fase_Fase_ID).ToList<M_FIGATT_FIGURAPROF_ATTIVITA>();

                    foreach (M_FIGATT_FIGURAPROF_ATTIVITA m in sm)
                    {
                        M_FIGATT_FIGURAPROF_ATTIVITA nm = new M_FIGATT_FIGURAPROF_ATTIVITA();

                        nm.M_FIGATT_FASE_ID = f.FASE_ID;
                        nm.M_FIGATT_FIGPRO_ID = m.M_FIGATT_FIGPRO_ID;
                        en.M_FIGATT_FIGURAPROF_ATTIVITA.AddObject(nm);
                    }

                    en.SaveChanges();  
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString ();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        
        }

        public ActionResult SaveFaseEdit(MyFaseAjax fase)
        {
            bool flagOK = true;
            string er = "";
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                FASE f = new FASE();
                f = en.FASE.Where(x => x.FASE_ID == fase.Fase_ID).SingleOrDefault();
                f.FASE_ID = fase.Fase_ID;
                f.FASE_CODICE = fase.Fase_Codice;
                f.FASE_DESC = fase.Fase_Desc;
                if (fase.Fase_Grurep_ID != 0)
                    f.FASE_GRUREP_ID = fase.Fase_Grurep_ID;
                else
                    f.FASE_GRUREP_ID = null;
                //en.FASE.AddObject(f);
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
        public ActionResult deleteFigProfEdit(MyFigProfAjax fpa)
        {
            bool flagOK = true;
            string er = "";
            int idret = fpa.FigProf_ID;
            try 
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                FIGPRO_FIGURA_PROFESSIONALE fp = en.FIGPRO_FIGURA_PROFESSIONALE.Where(x => x.FIGPRO_ID == fpa.FigProf_ID).SingleOrDefault();
                en.FIGPRO_FIGURA_PROFESSIONALE.DeleteObject(fp);
                en.SaveChanges();
                flagOK = true; 
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = idret });

        }
        public ActionResult saveFigProfEdit(MyFigProfAjax fpa)
        {
            bool flagOK = true;
            string er = "";
            int idret = 0;
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                FIGPRO_FIGURA_PROFESSIONALE fp = new FIGPRO_FIGURA_PROFESSIONALE();

                decimal? prezzoUnit = null;
                try
                {
                    prezzoUnit = decimal.Parse(fpa.FigProf_Cost.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                }
                catch { }


                List<FIGPRO_FIGURA_PROFESSIONALE> lfp = en.FIGPRO_FIGURA_PROFESSIONALE.Where(x => x.FIGPRO_ID != fpa.FigProf_ID && x.FIGPRO_CODICE == fpa.FigProf_Codice ).ToList<FIGPRO_FIGURA_PROFESSIONALE>();
                if (lfp.Count() == 0)
                {
                    if (fpa.FigProf_ID > 0)
                    {

                        fp = en.FIGPRO_FIGURA_PROFESSIONALE.Where(x => x.FIGPRO_ID == fpa.FigProf_ID).SingleOrDefault();
                        fp.FIGPRO_CODICE = fpa.FigProf_Codice;
                        fp.FIGPRO_DESC = fpa.FigProf_Desc;
                        fp.FIGPRO_COSTO = prezzoUnit.Value;
                        

                    }
                    else
                    {
                        fp.FIGPRO_CODICE = fpa.FigProf_Codice;
                        fp.FIGPRO_DESC = fpa.FigProf_Desc ;
                        fp.FIGPRO_COSTO = prezzoUnit.Value;
                        

                        en.FIGPRO_FIGURA_PROFESSIONALE.AddObject(fp);
                    }
                    en.SaveChanges();
                    flagOK = true;
                    idret = fp.FIGPRO_ID;

                    TRKFIG_FIGPRO_TRACKING tk = new TRKFIG_FIGPRO_TRACKING();
                    tk.TRKFIG_UTENTE_ID = this.CurrentUserID;
                    tk.TRKFIG_FIGPRO_ID = fp.FIGPRO_ID;
                    tk.TRKFIG_FIGPRO_COSTO = prezzoUnit.Value;
                    tk.TRKFIG_DATA_INS = DateTime.Now;
                    en.TRKFIG_FIGPRO_TRACKING.AddObject(tk);
                    en.SaveChanges(); 

                }
                else
                {
                    er = "Impossibile procedere con la modifca.<br>Codice già presente";
                    flagOK = false;
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = idret });
        }
        public ActionResult saveGruppoEdit(MyGrurepAjax Gruppo)
        {
            bool flagOK = true;
            string er = "";
            int idret=0;
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                GRUREP_GRUPPI_REPARTI g = new GRUREP_GRUPPI_REPARTI();

                decimal? prezzoUnitAccettazione = null;
                try 
                {
                    prezzoUnitAccettazione = decimal.Parse(Gruppo.Grurep_PrezzoUnit_Accettazione.Replace (",","."), System.Globalization.CultureInfo.InvariantCulture);
                }
                catch { }
                

                List<GRUREP_GRUPPI_REPARTI> lg = en.GRUREP_GRUPPI_REPARTI.Where(x => x.GRUREP_ID != Gruppo.Grurep_ID && x.GRUREP_CODICE == Gruppo.Grurep_Codice && x.GRUREP_FLG_REPARTO == false).ToList<GRUREP_GRUPPI_REPARTI>();
                if (lg.Count() == 0)
                {
                    if (Gruppo.Grurep_ID > 0)
                    {

                        g = en.GRUREP_GRUPPI_REPARTI.Where(x => x.GRUREP_ID == Gruppo.Grurep_ID).SingleOrDefault();
                        g.GRUREP_CODICE = Gruppo.Grurep_Codice;
                        g.GRUREP_DESC = Gruppo.Grurep_Desc;
                        g.GRUREP_DESC_ESTESA = Gruppo.Grurep_DescEstesa;
                        g.GRUREP_FLG_REPARTO = false;
                        g.GRUREP_COST_IND = Gruppo.Grurep_Cost_Ind.Replace(",",".");
                        g.GRUREP_PREZZO_UNIT_ACCET = prezzoUnitAccettazione;
                        
                    }
                    else
                    {
                        g.GRUREP_CODICE = Gruppo.Grurep_Codice;
                        g.GRUREP_DESC = Gruppo.Grurep_Desc;
                        g.GRUREP_DESC_ESTESA = Gruppo.Grurep_DescEstesa;
                        g.GRUREP_FLG_REPARTO = false;
                        g.GRUREP_COST_IND = Gruppo.Grurep_Cost_Ind.Replace(",", "."); ;
                        g.GRUREP_PREZZO_UNIT_ACCET = prezzoUnitAccettazione;

                        en.GRUREP_GRUPPI_REPARTI.AddObject(g);
                    }
                    en.SaveChanges();
                    flagOK = true;
                    idret = g.GRUREP_ID;
                }
                else
                {
                    er = "Impossibile procedere con la modifca.<br>Codice già presente";
                    flagOK = false;
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = idret });
        }

        public ActionResult saveRepartoEdit(MyGrurepAjax Gruppo)
        {
            bool flagOK = true;
            string er = "";
            int idret = 0;
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                GRUREP_GRUPPI_REPARTI g = new GRUREP_GRUPPI_REPARTI();
                decimal? prezzoUnitAccettazione = null;
                try
                {
                    prezzoUnitAccettazione = decimal.Parse(Gruppo.Grurep_PrezzoUnit_Accettazione.Replace (",","."), System.Globalization.CultureInfo.InvariantCulture);
                }
                catch { }
                

                List<GRUREP_GRUPPI_REPARTI> lg = en.GRUREP_GRUPPI_REPARTI.Where(x => x.GRUREP_ID != Gruppo.Grurep_ID && x.GRUREP_CODICE == Gruppo.Grurep_Codice && x.GRUREP_FLG_REPARTO == true).ToList<GRUREP_GRUPPI_REPARTI>();
                if (lg.Count() == 0)
                {
                    if (Gruppo.Grurep_ID > 0)
                    {

                        g = en.GRUREP_GRUPPI_REPARTI.Where(x => x.GRUREP_ID == Gruppo.Grurep_ID).SingleOrDefault();
                        g.GRUREP_CODICE = Gruppo.Grurep_Codice;
                        g.GRUREP_DESC = Gruppo.Grurep_Desc;
                        g.GRUREP_DESC_ESTESA = Gruppo.Grurep_DescEstesa;
                        g.GRUREP_COST_IND = Gruppo.Grurep_Cost_Ind.Replace(",", ".");
                        g.GRUREP_FLG_REPARTO = true;
                        g.GRUREP_PREZZO_UNIT_ACCET = prezzoUnitAccettazione;

                    }
                    else
                    {
                        g.GRUREP_CODICE = Gruppo.Grurep_Codice;
                        g.GRUREP_DESC = Gruppo.Grurep_Desc;
                        g.GRUREP_DESC_ESTESA = Gruppo.Grurep_DescEstesa;
                        g.GRUREP_FLG_REPARTO = true;
                        g.GRUREP_COST_IND = Gruppo.Grurep_Cost_Ind.Replace(",", ".");
                        g.GRUREP_PREZZO_UNIT_ACCET = prezzoUnitAccettazione;
                        en.GRUREP_GRUPPI_REPARTI.AddObject(g);
                    }
                    en.SaveChanges();
                    flagOK = true;
                    idret = g.GRUREP_ID;
                }
                else
                {
                    er = "Impossibile procedere con la modifca.<br>Codice già presente";
                    flagOK = false;
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = idret });
        }



        public ActionResult saveUtenti(MyUtente Utenti)
        {
            bool flagOK = true;
            string er = "";
            int idret = 0;
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                UTENTE u = new UTENTE();

                    if (Utenti.Utente_ID > 0)
                    {

                        u = en.UTENTE.Where(x => x.UTENTE_ID == Utenti.Utente_ID).SingleOrDefault();
                        u.UTENTE_COGNOME = Utenti.Utente_Cognome;
                        u.UTENTE_EMAIL = Utenti.Utente_Email;
                        u.UTENTE_LOCK = Utenti.Utente_Lock;
                        u.UTENTE_NOME = Utenti.Utente_Nome;
                        u.UTENTE_USER = Utenti.Utente_User;
                    }
                    else
                    {
                        u.UTENTE_COGNOME = Utenti.Utente_Cognome;
                        u.UTENTE_EMAIL = Utenti.Utente_Email;
                        u.UTENTE_LOCK = Utenti.Utente_Lock;
                        u.UTENTE_NOME = Utenti.Utente_Nome;
                        u.UTENTE_USER = Utenti.Utente_User;
                        en.UTENTE.AddObject(u);
                    }
                    en.SaveChanges();
                    flagOK = true;
                    idret = u.UTENTE_ID;
                
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = idret });
        }

        public ActionResult saveNuovoUtenti_profili_gruppi(MyUtenti_Profili_GruppiAjax upg)
        {
            bool flagOK = true;
            string er = "";
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            M_UTPRGR_UTENTI_PROFILI_GRUPPI m = new M_UTPRGR_UTENTI_PROFILI_GRUPPI();

            try
            {
                m.M_UTPRGR_UTENTE_ID = upg.M_Utprgr_Utente_Id;
                en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.AddObject(m);
                en.SaveChanges();
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }
            
            return Json(new { ok = flagOK, infopersonali = er });
        }
        public ActionResult EliminaProfiliUtente(MyProdottoPosAjaxList prodPosId)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                if (prodPosId != null && prodPosId.ProdottoPosIds != null)
                {
                    foreach (int posId in prodPosId.ProdottoPosIds)
                    {

                        List<M_UTPRGR_UTENTI_PROFILI_GRUPPI> lstSrcObjects = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(z => z.M_UTPRGR_ID == posId).ToList<M_UTPRGR_UTENTI_PROFILI_GRUPPI>();
                        foreach (M_UTPRGR_UTENTI_PROFILI_GRUPPI pp in lstSrcObjects)
                        {
                            en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.DeleteObject(pp);
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

        public ActionResult saveUtenti_Profili_Gruppi(MyUtenti_Profili_GruppiAjax upg)
        {
            bool flagOK = true;
            string er = "";
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            

            try
            {

                if (upg.M_Utprgr_Flg_Principale)
                {
                    List<M_UTPRGR_UTENTI_PROFILI_GRUPPI> er1 = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(z => z.M_UTPRGR_UTENTE_ID != upg.M_Utprgr_Utente_Id
                                                                                                    && z.M_UTPRGR_PROFIL_ID == upg.M_Utprgr_Profil_Id 
                                                                                                    && z.M_UTPRGR_GRUREP_ID==upg.M_Utprgr_Grurep_Id
                                                                                                    && z.M_UTPRGR_FLG_PRINCIPALE == true).ToList<M_UTPRGR_UTENTI_PROFILI_GRUPPI>();
                    if (er1.Count > 0)
                    { 
                        flagOK = false;
                        int utente_ID =  er1[0].M_UTPRGR_UTENTE_ID;
                        UTENTE l_ut = en.UTENTE.Where(z => z.UTENTE_ID == utente_ID).SingleOrDefault();
                        string utente = "Utente di dominio: " + l_ut.UTENTE_USER +" [" +  l_ut.UTENTE_NOME + " " + l_ut.UTENTE_COGNOME +"]";
                        er = "Esiste già un utente principale con questi accessi.<br>"+utente;
                    }
                }

                if (upg.M_Utprgr_Grurep_Id !=null && upg.M_Utprgr_Profil_Id != null)
                {
                    List<M_UTPRGR_UTENTI_PROFILI_GRUPPI> er2 = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(z => z.M_UTPRGR_ID != upg.M_Utprgr_Id
                                                                                                    && z.M_UTPRGR_UTENTE_ID == upg.M_Utprgr_Utente_Id
                                                                                                    && z.M_UTPRGR_PROFIL_ID == upg.M_Utprgr_Profil_Id
                                                                                                    && z.M_UTPRGR_GRUREP_ID == upg.M_Utprgr_Grurep_Id).ToList<M_UTPRGR_UTENTI_PROFILI_GRUPPI>();
                    if (er2.Count>0)
                    {
                        flagOK = false;
                        er = "L'utente ha già questi accessi associati";
                    }

                }


                if (flagOK)
                {
                    M_UTPRGR_UTENTI_PROFILI_GRUPPI m = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(z => z.M_UTPRGR_ID == upg.M_Utprgr_Id).SingleOrDefault();

                    if (upg.M_Utprgr_Grurep_Id > 0)
                        m.M_UTPRGR_GRUREP_ID = upg.M_Utprgr_Grurep_Id;
                    else
                        m.M_UTPRGR_GRUREP_ID = null;
                    if (upg.M_Utprgr_Profil_Id > 0)
                        m.M_UTPRGR_PROFIL_ID = upg.M_Utprgr_Profil_Id;
                    else
                        m.M_UTPRGR_PROFIL_ID = null;
                    m.M_UTPRGR_FLG_PRINCIPALE = upg.M_Utprgr_Flg_Principale;
                    m.M_UTPRGR_UTENTE_ID = upg.M_Utprgr_Utente_Id;
                    en.SaveChanges();
                }
            
            
            
            
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }
            
            return Json(new { ok = flagOK, infopersonali = er });
        }
        

        public ActionResult SaveM_Figatt(MyFiguraProfessionale_attivitaAjax figatt)
        {
            bool flagOK = true;
            string er = "";
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            M_FIGATT_FIGURAPROF_ATTIVITA m = new M_FIGATT_FIGURAPROF_ATTIVITA();

            try
                {
                if (figatt.M_Figatt_checked ) 
                {
                    
                    m.M_FIGATT_FASE_ID=figatt.M_Figatt_Fase_Id;
                    m.M_FIGATT_FIGPRO_ID=figatt.M_Figatt_Figpro_Id;
                    en.M_FIGATT_FIGURAPROF_ATTIVITA.AddObject(m);
                    en.SaveChanges();
                    flagOK = true;
 
                }
                else
                {
                    m = en.M_FIGATT_FIGURAPROF_ATTIVITA.Where(z=> z.M_FIGATT_FASE_ID == figatt.M_Figatt_Fase_Id && z.M_FIGATT_FIGPRO_ID == figatt.M_Figatt_Figpro_Id).SingleOrDefault();
                    en.M_FIGATT_FIGURAPROF_ATTIVITA.DeleteObject(m);
                    en.SaveChanges();
                    flagOK = true;
 
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
        public ActionResult Fasi(MyFaseAjax fase)
        {
            this.SetMessage = "Settings - Fasi";
            FaseModel f = new FaseModel();
            f.SelectFase_ID = fase.Fase_ID;

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_Fasi", f) });
        }
        [HttpPost]
        public ActionResult FaseInsert(string txtCodice,string txtDescrizione)
        {
            this.SetMessage = "Settings - Fasi";
            FaseModel f = new FaseModel();
            return View(f);
        }
        public ActionResult FaseInsert(int? currentfase)
        {
            this.SetMessage = "Settings - Fasi";
            FaseModel f = new FaseModel();
            f.IdPadre = currentfase;
            return View(f);
        }
        public ActionResult Fasi()
        {
            this.SetMessage = "Settings - Fasi";
            FaseModel f = new FaseModel();
            return View(f);
        }

        public ActionResult FaseEdit(int id)
        {
            this.SetMessage = "Settings - Fasi";
            FaseModel f = new FaseModel(id);
            return View(f);
        }

        //  [HttpPost]
        //public ActionResult Utenti(MyUtenteAjax Utente)
        //{
        //    this.SetMessage = "Settings - Utenti";
        //    UtentiModel u = new UtentiModel();
        //    u.SelectUtente_ID = Utente.Utente_ID;

        //    return Json(new { status = "ok", partial = this.RenderPartialViewToString("_Utenti", u) });
        //}
        [HttpPost]
        public ActionResult Utenti(MyPaginAjax mpa)
        {
            this.SetMessage = "Settings - Utenti";
            UtentiModel u = new UtentiModel();
            if (mpa.id != 0)
            {
                u.SelectUtente_ID = mpa.id;
            }

            ViewData["NumEntities_UP"] = mpa.NumEntities.ToString();
            ViewData["table_search_UP"] = mpa.SearchDescription;
            ViewData["CurrentPage_UP"] = mpa.CurrentPage;
            TempData["NumEntities_UP"] = mpa.NumEntities.ToString();
            //         ViewBag.NumEntities = mpa.NumEntities.ToString();
            //         ViewBag.table_search = mpa.SearchDescription.ToString();
            TempData["table_search_UP"] = mpa.SearchDescription;
            TempData["CurrentPage_UP"] = mpa.CurrentPage;
            
            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    u.CurrentPage = mpa.CurrentPage.Value;
                else
                    u.CurrentPage = 1;
            }
            else
                u.CurrentPage = 1;

            u.NumEntities_UP  = mpa.NumEntities;
            u.SearchDescription = mpa.SearchDescription;

            u.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)u.ElencoUtenti.Count() / u.NumEntities_UP));
            if (u.CurrentPage > u.NumberOfPages) u.CurrentPage = u.NumberOfPages;
            u.Data = u.ElencoUtenti.OrderBy(z => z.Utente_User).Skip(u.NumEntities_UP * (u.CurrentPage - 1)).Take(u.NumEntities_UP).ToList();
            if (u.Data.Count() >=1)
            {
                List<int> elencoID = getListUtenteID(u.Data);
                if(!elencoID.Contains(u.SelectUtente_ID))
                    u.SelectUtente_ID = u.Data.ElementAt(0).Utente_ID;
            }
            /*gestione sotto*/
            
            ViewData["NumEntities_Down"] = mpa.NumEntities_Down.ToString();
            ViewData["table_search_Down"] = mpa.SearchDescription_Down;
            ViewData["CurrentPage_Down"] = mpa.CurrentPage_Down;
            TempData["NumEntities_Down"] = mpa.NumEntities_Down.ToString();
            TempData["table_search_Down"] = mpa.SearchDescription_Down;
            TempData["CurrentPage_Down"] = mpa.CurrentPage_Down;

            if (mpa.CurrentPage_Down.HasValue)
            {
                if (mpa.CurrentPage_Down.Value != 0)
                    u.CurrentPage_Down = mpa.CurrentPage_Down.Value;
                else
                    u.CurrentPage_Down = 1;
            }
            else
                u.CurrentPage_Down = 1;

            u.NumEntities_Down = mpa.NumEntities_Down;
            u.SearchDescription_Down = mpa.SearchDescription_Down;

            u.NumberOfPages_Down = Convert.ToInt32(Math.Ceiling((double)u.ElencoUtenti_profili_gruppi.Count() / u.NumEntities_Down));
            if (u.CurrentPage_Down > u.NumberOfPages_Down) u.CurrentPage_Down = u.NumberOfPages_Down;
            u.Data_Down = u.ElencoUtenti_profili_gruppi.OrderBy(z => z.M_Utprgr_Id).Skip(u.NumEntities_Down * (u.CurrentPage_Down - 1)).Take(u.NumEntities_Down).ToList();
            
            /*Fine sotto*/

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_Utenti", u) });
        }
        private List<int> getListUtenteID(IEnumerable<MyUtente> data)
        {
            List<int> lst = new List<int>();
            int k = data.Count();
            for (int j = 0; j < k; j++)
            {
                lst.Add(data.ElementAt(j).Utente_ID);
            }
            return lst;
        }
        //public ActionResult Utenti()
        //{
        //    this.SetMessage = "Settings - Utenti";
        //    UtentiModel u = new UtentiModel();
        //    return View(u);
        //}

        public ActionResult Utenti(int? NumEntities, string table_search, int? CurrentPage,
            int? NumEntities_Down, string table_search_Down, int? CurrentPage_Down)
        {
            this.SetMessage = "Settings - Utenti";
            ////string rangeID = TempData["NumEntities"] as string;
            ////string table_search1 = TempData["table_search"] as string;
            string rangeID = ViewData["NumEntities_UP"] as string;
            string table_search1 = ViewData["table_search_UP"] as string;
            string CurrentPageStr = ViewData["CurrentPage_UP"] as string;
            rangeID = ViewBag.NumEntities_UP;
            table_search1 = ViewBag.table_search_UP;

            UtentiModel u = new UtentiModel();
            //if (NumEntities.HasValue)
            //    g.NumEntities = NumEntities.Value;
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                u.NumEntities_UP = numEntities;
            }

            if (table_search1 != null)
                u.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                u.CurrentPage = pageNum;
            }
            else
                u.CurrentPage = 1;

            u.Data = u.ElencoUtenti.OrderBy(z => z.Utente_User).Take(u.NumEntities_UP).ToList();
            u.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)u.ElencoUtenti.Count() / u.NumEntities_UP));
            u.CurrentPage = 1;
            

            string rangeID_Down = ViewData["NumEntities_Down"] as string;
            string table_search1_Down = ViewData["table_search_Down"] as string;
            string CurrentPageStr_Down = ViewData["CurrentPage_Down"] as string;
            rangeID_Down = ViewBag.NumEntities_Down;
            table_search1_Down = ViewBag.table_search_Down;



            int numEntities_Down = 0;
            if (int.TryParse(rangeID_Down, out numEntities_Down))
            {
                u.NumEntities_Down = numEntities_Down;
            }

            if (table_search1_Down != null)
                u.SearchDescription_Down = table_search1_Down;

            int pageNum_Down = 0;
            if (int.TryParse(CurrentPageStr_Down, out pageNum_Down))
            {
                u.CurrentPage_Down = pageNum_Down;
            }
            else
                u.CurrentPage_Down = 1;

            u.Data_Down = u.ElencoUtenti_profili_gruppi.OrderBy(z => z.M_Utprgr_Id).Take(u.NumEntities_Down).ToList();
            u.NumberOfPages_Down = Convert.ToInt32(Math.Ceiling((double)u.ElencoUtenti_profili_gruppi.Count() / u.NumEntities_Down));
            u.CurrentPage_Down = 1;

            
            return View(u);
        }

        public ActionResult UtentiInsert()
        {
            this.SetMessage = "Settings - Utenti";
            UtentiModel u = new UtentiModel();
            return View(u);
        }
        public ActionResult UtentiEdit(int id)
        {
            this.SetMessage = "Settings - Utenti";
            UtentiModel u = new UtentiModel(id);
            return View(u);
        }

        [HttpPost]
        public ActionResult PagingPos(MyPaginAjax mpa)
        {
            this.SetMessage = "Settings - Gruppi";
            GruppiModel g = new GruppiModel();
            TempData["NumEntities"] = mpa.NumEntities;
            TempData["table_search"] = mpa.SearchDescription;
            g.CurrentPage = mpa.CurrentPage.Value;
            g.NumEntities = mpa.NumEntities;
            g.SearchDescription = mpa.SearchDescription;
            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_GruppiElenco", g) });
        }

        public ActionResult RepartiEdit(int id)
        {
            this.SetMessage = "Settings - Gruppi Prodotto";
            RepartiModel g = new RepartiModel(id);
            return View(g);
        }

        public ActionResult RepartiInsert()
        {
            this.SetMessage = "Settings - Gruppi Prodotto";
            RepartiModel g = new RepartiModel();
            return View(g);
        }

        public ActionResult RepartiTable(int? NumEntities, string table_search, int? CurrentPage)
        {
            this.SetMessage = "Settings - Gruppi Prodotto";
            ////string rangeID = TempData["NumEntities"] as string;
            ////string table_search1 = TempData["table_search"] as string;
            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            RepartiModel g = new RepartiModel();
            //if (NumEntities.HasValue)
            //    g.NumEntities = NumEntities.Value;
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                g.NumEntities = numEntities;
            }

            if (table_search1 != null)
                g.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                g.CurrentPage = pageNum;
            }
            else
                g.CurrentPage = 1;

            g.Data = g.ElencoReparti.OrderBy(z => z.Grurep_Codice).Take(g.NumEntities).ToList();
            g.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)g.ElencoReparti.Count() / g.NumEntities));
            g.CurrentPage = 1;

            return View(g);
        }
        [HttpPost]
        public ActionResult RepartiTable(MyPaginAjax mpa)
        {
            this.SetMessage = "Settings - Gruppi Prodotto";
            RepartiModel g = new RepartiModel();
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
                    g.CurrentPage = mpa.CurrentPage.Value;
                else
                    g.CurrentPage = 1;
            }
            else
                g.CurrentPage = 1;

            g.NumEntities = mpa.NumEntities;
            g.SearchDescription = mpa.SearchDescription;

            g.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)g.ElencoReparti.Count() / g.NumEntities));
            if (g.CurrentPage > g.NumberOfPages) g.CurrentPage = g.NumberOfPages;
            g.Data = g.ElencoReparti.OrderBy(z => z.Grurep_Codice).Skip(g.NumEntities * (g.CurrentPage - 1)).Take(g.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_RepartiElencoTable", g) });
        }


        public ActionResult GruppiTable(int? NumEntities, string table_search, int? CurrentPage)
        {
            this.SetMessage = "Settings - Gruppi";
            ////string rangeID = TempData["NumEntities"] as string;
            ////string table_search1 = TempData["table_search"] as string;
            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;
            
            GruppiModel g = new GruppiModel();
            //if (NumEntities.HasValue)
            //    g.NumEntities = NumEntities.Value;
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                g.NumEntities = numEntities;
            }

            if (table_search1 != null)
                g.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                g.CurrentPage = pageNum;
            }
            else
                g.CurrentPage = 1;

            g.Data = g.ElencoGruppi.OrderBy(z => z.Grurep_Codice).Take(g.NumEntities).ToList();
            g.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)g.ElencoGruppi.Count() / g.NumEntities));
            g.CurrentPage = 1;

            return View(g);
        }
        public ActionResult FigProfInsert()
        {
            this.SetMessage = "Settings - Insert";
            FigProfModel fp = new FigProfModel();
            return View(fp);
        }
        public ActionResult FigProfEdit(int id)
        {
            this.SetMessage = "Settings - Figura Professionale";
            FigProfModel fp = new FigProfModel(id);
            return View(fp);
        }
        public ActionResult FigProfWorkflow(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Workflow Figura Professionale";
            FigProfWorkflowModel fpw = new FigProfWorkflowModel(id);
            return View(fpw);
        }
        public ActionResult FigProf(int? NumEntities, string table_search, int? CurrentPage)
        {
            this.SetMessage = "Settings - Figura Professionale";
            ////string rangeID = TempData["NumEntities"] as string;
            ////string table_search1 = TempData["table_search"] as string;
            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            FigProfModel fp = new FigProfModel();
          
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                fp.NumEntities = numEntities;
            }

            if (table_search1 != null)
                fp.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                fp.CurrentPage = pageNum;
            }
            else
                fp.CurrentPage = 1;

            fp.Data = fp.ElencoFigureProfessionali.OrderBy(z => z.FigProf_Desc).Take(fp.NumEntities).ToList();
            fp.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)fp.ElencoFigureProfessionali.Count() / fp.NumEntities));
            fp.CurrentPage = 1;

            return View(fp);
        }
        [HttpPost]
        public ActionResult FigProf(MyPaginAjax mpa)
        {
            this.SetMessage = "Settings - Figura Professionale";
            FigProfModel fp = new FigProfModel();
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
                    fp.CurrentPage = mpa.CurrentPage.Value;
                else
                    fp.CurrentPage = 1;
            }
            else
                fp.CurrentPage = 1;

            fp.NumEntities = mpa.NumEntities;
            fp.SearchDescription = mpa.SearchDescription;

            fp.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)fp.ElencoFigureProfessionali.Count() / fp.NumEntities));
            if (fp.CurrentPage > fp.NumberOfPages) fp.CurrentPage = fp.NumberOfPages;
            fp.Data = fp.ElencoFigureProfessionali.OrderBy(z => z.FigProf_Desc).Skip(fp.NumEntities * (fp.CurrentPage - 1)).Take(fp.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_FigProf", fp) });
        }

        [HttpPost]
        public ActionResult SaveInsertMacchinario(MyMacchinarioAjax an)
        {
            bool flagOK = true;
            string er = "";
            int new_id = 0;
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

                MACCHI_MACCHINARIO m = new MACCHI_MACCHINARIO();
                m.MACCHI_CODICE = an.Macchi_Codice;
                m.MACCHI_DESC = an.Macchi_Desc;
                if (an.Macchi_GruppoID != null && an.Macchi_GruppoID != 0)
                    m.MACCHI_GRUREP_ID = an.Macchi_GruppoID;
                else 
                    m.MACCHI_GRUREP_ID = 0;

                m.MACCHI_VALORE_STRUMENTAZIONE = an.Macchi_Valore_Strumentazione;
                m.MACCHI_COSTO_MANUTENZIONE_ANNUALE = an.Macchi_Costo_Manutenzione_Annuo;
                m.MACCHI_VITA_UTILE_ANNI = an.Macchi_Vita_Utile_Anni;
                m.MACCHI_MINUTI_ANNO = an.Macchi_Minuti_Anno;

                m.MACCHI_PREZZO_UNITARIO = ((m.MACCHI_VALORE_STRUMENTAZIONE / m.MACCHI_VITA_UTILE_ANNI) + m.MACCHI_COSTO_MANUTENZIONE_ANNUALE) / m.MACCHI_MINUTI_ANNO;
           
                en.MACCHI_MACCHINARIO.AddObject(m);
                en.SaveChanges();

                flagOK = true;
                new_id = m.MACCHI_ID;
                if (new_id == 0)
                {
                    flagOK = false;
                }
                else
                {
                    try
                    {
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
        public ActionResult SaveMacchinario(MyMacchinarioAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

                MACCHI_MACCHINARIO m = en.MACCHI_MACCHINARIO.Where(z=>z.MACCHI_ID == an.Macchi_ID).SingleOrDefault();
                m.MACCHI_CODICE = an.Macchi_Codice;
                m.MACCHI_DESC = an.Macchi_Desc;
                if (an.Macchi_GruppoID != null && an.Macchi_GruppoID != 0)
                    m.MACCHI_GRUREP_ID = an.Macchi_GruppoID;
                else
                    m.MACCHI_GRUREP_ID = 0;

                m.MACCHI_VALORE_STRUMENTAZIONE = an.Macchi_Valore_Strumentazione;
                m.MACCHI_COSTO_MANUTENZIONE_ANNUALE = an.Macchi_Costo_Manutenzione_Annuo;
                m.MACCHI_VITA_UTILE_ANNI =an.Macchi_Vita_Utile_Anni;
                m.MACCHI_MINUTI_ANNO = an.Macchi_Minuti_Anno;
                try
                {
                    m.MACCHI_PREZZO_UNITARIO = ((m.MACCHI_VALORE_STRUMENTAZIONE / m.MACCHI_VITA_UTILE_ANNI) + m.MACCHI_COSTO_MANUTENZIONE_ANNUALE) / m.MACCHI_MINUTI_ANNO;
                }
                catch { }
                en.SaveChanges();

                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = an.Macchi_ID });
        }
        [HttpPost]
        public ActionResult EliminaMacchinario(MyMacchinarioAjax an)
        {
            bool flagOK = true;
            string er = "";
            int new_id = 0;
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

                MACCHI_MACCHINARIO m = en.MACCHI_MACCHINARIO.Where(z=>z.MACCHI_ID == an.Macchi_ID ).SingleOrDefault();
                en.MACCHI_MACCHINARIO.DeleteObject(m);
                en.SaveChanges();

                flagOK = true;
            }
            catch (Exception ex)
            {
                er = "Impossibile eliminare il macchinario:<br/>";
                er += ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = new_id });
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPGruppiReparti(int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            
            ListaGruppiRepartiModel IA = new ListaGruppiRepartiModel();
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
            ListaGruppiRepartiModel IA = new ListaGruppiRepartiModel();
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
        public ActionResult MacchinarioEdit(int id)
        {
            this.SetMessage = "Settings - Apparecchiature dedicate";
            ListaMacchinari IA = new ListaMacchinari(id);
            return View(IA);
        }
        public ActionResult MacchinarioInsert()
        {
            this.SetMessage = "Settings - Apparecchiature dedicate";
            B16ModelMgr b = new B16ModelMgr();
            return View(b);
        }

        public ActionResult SettingsEdit(int id)
        {
            this.SetMessage = "Settings - Variabili C.A.P.";
            SettingsVarModel IA = new SettingsVarModel(id);
            return View(IA);
        }
        [HttpPost]
        public ActionResult SaveSetting(MySettingAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

                T_SETTIN_SETTINGS ts = en.T_SETTIN_SETTINGS.Where(z => z.T_SETTIN_ID == an.Setting_ID).SingleOrDefault();
                ts.T_SETTIN_CODICE = an.Setting_Codice;
                ts.T_SETTIN_VALUE = an.Setting_Valore;
                
                en.SaveChanges();

                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er, id = an.Setting_ID });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SettingsVarTable(int? NumEntities, string table_search, int? CurrentPage)
        {
            this.SetMessage = "Settings - Variabili C.A.P.";
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            SettingsVarModel IA = new SettingsVarModel();

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

            IA.Data = IA.ElencoSettings.OrderBy(z => z.Settings_Codice).Take(IA.NumEntities).ToList();
            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)IA.ElencoSettings.Count() / IA.NumEntities));
            IA.CurrentPage = 1;


            return View(IA);
        }
        [HttpPost]
        public ActionResult SettingsVarTable(MyPaginAjax mpa)
        {
            this.SetMessage = "Settings - Variabili C.A.P.";
            SettingsVarModel IA = new SettingsVarModel();
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

            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)IA.ElencoSettings.Count() / IA.NumEntities));
            if (IA.CurrentPage > IA.NumberOfPages) IA.CurrentPage = IA.NumberOfPages;
            IA.Data = IA.ElencoSettings.OrderBy(z => z.Settings_Codice).Skip(IA.NumEntities * (IA.CurrentPage - 1)).Take(IA.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_SettingsVarTable", IA) });
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Macchinari( int? NumEntities, string table_search, int? CurrentPage)
        {
            this.SetMessage = "Settings - Apparecchiature dedicate";
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ListaMacchinari IA = new ListaMacchinari();

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

            IA.Data = IA.ElencoMacchinari.OrderBy(z => z.Macchi_Codice).Take(IA.NumEntities).ToList();
            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)IA.ElencoMacchinari.Count() / IA.NumEntities));
            IA.CurrentPage = 1;


            return View(IA);
        }
        [HttpPost]
        public ActionResult Macchinari(MyPaginAjax mpa)
        {
            this.SetMessage = "Settings - Apparecchiature dedicate";
            ListaMacchinari IA = new ListaMacchinari();
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

            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)IA.ElencoMacchinari.Count() / IA.NumEntities));
            if (IA.CurrentPage > IA.NumberOfPages) IA.CurrentPage = IA.NumberOfPages;
            IA.Data = IA.ElencoMacchinari.OrderBy(z => z.Macchi_Codice).Skip(IA.NumEntities * (IA.CurrentPage - 1)).Take(IA.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_Macchinari", IA) });
        }
        public ActionResult PopUpGruppoReparto(int? Ut,int? NumEntities, string table_search, int? CurrentPage)
        {
            this.SetMessage = "Settings - Gruppi";
            ////string rangeID = TempData["NumEntities"] as string;
            ////string table_search1 = TempData["table_search"] as string;
            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            GruppiModel g = new GruppiModel(Ut);
            //if (NumEntities.HasValue)
            //    g.NumEntities = NumEntities.Value;
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                g.NumEntities = numEntities;
            }

            if (table_search1 != null)
                g.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                g.CurrentPage = pageNum;
            }
            else
                g.CurrentPage = 1;

            g.Data = g.ElencoGruppiReparti.OrderBy(z => z.Grurep_Codice).Take(g.NumEntities).ToList();
            g.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)g.ElencoGruppiReparti.Count() / g.NumEntities));
            g.CurrentPage = 1;

            return View(g);
        }

        [HttpPost]
        public ActionResult PopUpGruppoReparto(MyPaginAjax mpa)
        {
            this.SetMessage = "Settings - Gruppi";
            int? Ut=null;
            if (mpa.Ut > 0)
                Ut = mpa.Ut;

            GruppiModel g = new GruppiModel(Ut);
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
                    g.CurrentPage = mpa.CurrentPage.Value;
                else
                    g.CurrentPage = 1;
            }
            else
                g.CurrentPage = 1;

            g.NumEntities = mpa.NumEntities;
            g.SearchDescription = mpa.SearchDescription;

            g.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)g.ElencoGruppiReparti.Count() / g.NumEntities));
            if (g.CurrentPage > g.NumberOfPages) g.CurrentPage = g.NumberOfPages;
            g.Data = g.ElencoGruppiReparti.OrderBy(z => z.Grurep_Codice).Skip(g.NumEntities * (g.CurrentPage - 1)).Take(g.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PopUpGruppoReparto", g) });
        }

        public ActionResult GruppiEdit(int id)
        {
            this.SetMessage = "Settings - Gruppi";
            GruppiModel g = new GruppiModel(id);
            return View(g);
        }

        public ActionResult GruppiInsert()
        {
            this.SetMessage = "Settings - Gruppi";
            GruppiModel g = new GruppiModel();
            return View(g);
        }

        [HttpPost]
        public ActionResult GruppiTable(MyPaginAjax mpa)
        {
            this.SetMessage = "Settings - Gruppi";
            GruppiModel g = new GruppiModel();
            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
   //         ViewBag.NumEntities = mpa.NumEntities.ToString();
   //         ViewBag.table_search = mpa.SearchDescription.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;
            
            if(mpa.CurrentPage.HasValue )
            {
                if(mpa.CurrentPage.Value!=0)
                    g.CurrentPage = mpa.CurrentPage.Value;
                else
                    g.CurrentPage = 1;
            }
            else 
                g.CurrentPage =1;
            
            g.NumEntities = mpa.NumEntities;
            g.SearchDescription = mpa.SearchDescription;

            g.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)g.ElencoGruppi.Count() / g.NumEntities));
            if (g.CurrentPage > g.NumberOfPages) g.CurrentPage = g.NumberOfPages;
            g.Data = g.ElencoGruppi.OrderBy(z => z.Grurep_Codice).Skip(g.NumEntities * (g.CurrentPage - 1)).Take(g.NumEntities).ToList();
            
            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_GruppiElencoTable", g) });
        }

        //[HttpPost]
        //public ActionResult Gruppi(int? id, int? NumEntities, string table_search)
        //{
        //    this.SetMessage = "Settings - Gruppi";
        //    GruppiModel g = new GruppiModel();
        //    if (NumEntities.HasValue)
        //        g.NumEntities = NumEntities.Value;
        //    if (table_search != null)
        //        g.SearchDescription = table_search;
        //    return View(g);
        //}
        //public ActionResult Gruppi(int? NumEntities, string table_search)
        //{
        //    this.SetMessage = "Settings - Gruppi";
        //    ////string rangeID = TempData["NumEntities"] as string;
        //    ////string table_search1 = TempData["table_search"] as string;
        //    string rangeID = ViewData["NumEntities"] as string;
        //    string table_search1 = ViewData["table_search"] as string;

        //    rangeID = ViewBag.NumEntities;
        //    table_search1 = ViewBag.table_search ;
        //    GruppiModel g = new GruppiModel();
        //    //if (NumEntities.HasValue)
        //    //    g.NumEntities = NumEntities.Value;
        //    int numEntities=0;
        //    if (int.TryParse(rangeID, out numEntities))
        //    {
        //        g.NumEntities = numEntities; 
        //    }

        //    if (table_search1 != null)
        //        g.SearchDescription = table_search1;
          
        //    return View(g);
        //}
        //[HttpPost]
        //public ActionResult Gruppi(MyPaginAjax mpa)
        //{
        //    this.SetMessage = "Settings - Gruppi";
        //    GruppiModel g = new GruppiModel();
        //    ViewData["NumEntities"] = mpa.NumEntities.ToString();
        //    ViewData["table_search"] = mpa.SearchDescription;
        //    TempData["NumEntities"] = mpa.NumEntities.ToString();
        //    ViewBag.NumEntities = mpa.NumEntities.ToString();
        //    ViewBag.table_search = mpa.SearchDescription.ToString();
        //    TempData["table_search"] = mpa.SearchDescription;
        //   // g.CurrentPage = mpa.CurrentPage.Value;
        //    g.NumEntities = mpa.NumEntities;
        //    g.SearchDescription = mpa.SearchDescription;
        //    return Json(new { status = "ok", partial = this.RenderPartialViewToString("_GruppiElenco", g) });
        //}

        //public ActionResult GruppiGrid(string searchWord, int? genreId, int? artistId, GridSortOptions gridSortOptions, int? page)
        //{
        //    IZSLER_CAP_Entities _service = new IZSLER_CAP_Entities();
        //    GruppiModel g = new GruppiModel();
        //    var pagedViewModel = new PagedViewModel<MyGrurep>
        //    {
        //        ViewData = ViewData,
        //        Query =g.ElencoGruppiQueryable ,// _service.GetAlbumsView(),
        //        GridSortOptions = gridSortOptions,
        //        DefaultSortColumn = "Grurep_ID",
        //        Page = page,
        //        PageSize = 10,
        //    }
        //    .AddFilter("searchWord", searchWord, a => a.Grurep_Codice.Contains(searchWord) || a.Grurep_Desc.Contains(searchWord) || a.Grurep_DescEstesa.Contains(searchWord))
        //    //.AddFilter("genreId", genreId, a => a.GenreId == genreId, _service.GetGenres(), "Name")
        //    //.AddFilter("artistId", artistId, a => a.ArtistId == artistId, _service.GetArtists(), "Name")
        //    .Setup();

        //    return View(pagedViewModel);
        //}
        //public const int PageSize = 2;

        //public ActionResult GruppiPaging()
        //{
        //    var gru = new PagedData<MyGrurep>();

        //    //  using (var ctx = new AjaxPagingContext())
        //    {
        //        GruppiModel g = new GruppiModel();
        //        gru.Data = g.ElencoGruppi.OrderBy(z => z.Grurep_Codice).Take(PageSize).ToList();
        //        gru.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)g.ElencoGruppi.Count() / PageSize));
        //        gru.CurrentPage = 1;
                
        //    }

        //    return View(gru);
        //}

        //public ActionResult _GruppiList(int page)
        //{
        //    var gru = new PagedData<MyGrurep>();

        //  //  using (var ctx = new AjaxPagingContext())
        //    {
        //        GruppiModel g = new GruppiModel();
        //        gru.Data = g.ElencoGruppi.OrderBy(z => z.Grurep_Codice).Skip(PageSize * (page - 1)).Take(PageSize).ToList();
        //        gru.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)g.ElencoGruppi.Count() / PageSize));
        //        gru.CurrentPage = page;
        //        ////people.Data = ctx.People.OrderBy(p => p.Surname).Skip(PageSize * (page - 1)).Take(PageSize).ToList();
        //        //people.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)ctx.People.Count() / PageSize));
        //        //people.CurrentPage = page;
        //    }

        //    return PartialView(gru);
        //}

        //[AcceptVerbs(HttpVerbs.Get)]
        //public JsonResult GetElencoFasi(string id)
        //{
        //    FasiModel f = new FasiModel(int.Parse(id));
        //    return Json(f.ListaAttivita, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Gridview()
        {
            IDictionary<string, string> searchConditions = new Dictionary<string, string>();

            if (this.Request.Form.AllKeys.Length > 0)
            {
                searchConditions.Add("Conditions1", Request["Conditions1"]);
                searchConditions.Add("Conditions2", Request["Conditions2"]);
            }
            else
            {
                object values = null;

                if (this.TempData.TryGetValue("SearchConditions", out values))
                {
                    searchConditions = values as Dictionary<string, string>;
                }
            }
            this.TempData["SearchConditions"] = searchConditions;
            string conditions1 = GetSearchConditionValue(searchConditions, "Conditions1");
            string conditions2 = GetSearchConditionValue(searchConditions, "Conditions2");
            // DataContext da = new DataContext();
            /*
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            var result = (from s in da.Conditions
                          where (string.IsNullOrEmpty(conditions1) || s.Conditions1.StartsWith(conditions1))
                          && (string.IsNullOrEmpty(conditions2) || s.Conditions2.StartsWith(conditions2))
                          select s).ToList();
            this.ViewData.Model = result;*/
            this.ViewData.Model = new GruppiModel();
            return View();
        }

        private static string GetSearchConditionValue(IDictionary<string, string> searchConditions, string key)
        {
            string tempValue = string.Empty;

            if (searchConditions != null && searchConditions.Keys.Contains("Conditions1"))
            {
                searchConditions.TryGetValue(key, out tempValue);
            }
            return tempValue;
        }

        public ActionResult Index()
        {
            B16ModelMgr p = new B16ModelMgr();
            return View(p);
        }

    }
}

 