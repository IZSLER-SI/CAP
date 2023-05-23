using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Models;
using IZSLER_CAP.Helpers;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Text;

namespace IZSLER_CAP.Controllers
{
    public class AnalisiController : B16Controller 
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
            MyFase fase =  le.GetFase(fase_id);
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
            newObj.VALPOS_MACCHI_ID = objSrc.VALPOS_MACCHI_ID; 
            newObj.VALPOS_COEFF_CONVERSIONE = objSrc.VALPOS_COEFF_CONVERSIONE;
            newObj.VALPOS_COD_SETTORE = objSrc.VALPOS_COD_SETTORE;
            
            return newObj;
        }
        //
        // GET: /AnalisiController/
        public AnalisiController()
            : base()
        {
            this.SetMessage= "Analisi";
            this.SetLiAnalisi = "current";
        }
        //public ActionResult Index()
        //{
        //    RedirectToRouteResult r = CheckLogin();
        //    if (r != null) { return r; }

        //    ListaAnalisiModel an = new ListaAnalisiModel();
        //    return View(an);
            
        //}

        public ActionResult Index(int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ListaAnalisiModel an = new ListaAnalisiModel();
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
            an.FiltroStato = 0;
            an.FiltroStatoObsoleta = 0;
            List<MyAnalisi > listAnalisi = an.GetElencoAnalisi(this.CurrentUserID,this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).ToList<MyAnalisi>() ;

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
            ListaAnalisiModel an = new ListaAnalisiModel();
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

            an.FiltroStato = mpa.FiltroStato;
            an.FiltroStatoObsoleta = mpa.FiltroStatoObsoleta; 

            an.NumEntities = mpa.NumEntities;
            an.SearchDescription = mpa.SearchDescription;
            List<MyAnalisi> listAnalisi = an.GetElencoAnalisi(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).ToList<MyAnalisi>();
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

        private decimal? getCosto(VALPOS_POSIZIONI currPos, VALORI_VALORIZZAZIONI v)
        {
            decimal? costoDelib=null;
            
            LoadEntities le = new LoadEntities();
            
            if (currPos.VALPOS_INTERM_ID.HasValue)
            {
                /*Vecchia*/
                /*
                MyAnalisi lAnalisi = le.GetAnalisi(currPos.VALPOS_INTERM_ID.Value);
                if(lAnalisi.Analisi_Gruppo_id == lAnalisiMaster.Analisi_Gruppo_id )
                {costoInd = "0";}
                costoDelib=lAnalisi.Analisi_CostoTotDelib;
                 * */

                MyAnalisi analisi = le.GetAnalisi(currPos.VALPOS_INTERM_ID.Value);
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
                            if (currPos.VALPOS_COD_SETTORE == "D")
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
            if (currPos.VALPOS_PRODOT_ID.HasValue)
            {
                //MyProdotto  lprod= le.GetProdotti(currPos.VALPOS_PRODOT_ID.Value);
                //costoDelib = lprod.Prodot_CostoUnitario_Deliberato;

                MyAnalisi m_AnalisiMaster = le.GetAnalisi(v.VALORI_ID);
                MyProdotto prodotto = le.GetProdotti(currPos.VALPOS_PRODOT_ID.Value);

                bool flgMatch = false;
                int masterGruppo_id = 0;
                int masterReparto_id = 0;
                if (m_AnalisiMaster.Analisi_flgIntermedio)
                {
                    if (m_AnalisiMaster.Analisi_Gruppo_id.HasValue) // interemedio Analisi
                    {
                        masterGruppo_id = m_AnalisiMaster.Analisi_Gruppo_id.Value;
                    }
                    if (m_AnalisiMaster.Analisi_Reparto_id.HasValue) // interemedio Prodotto
                    {
                        masterReparto_id = m_AnalisiMaster.Analisi_Reparto_id.Value;
                    }
                }
                else
                {
                    if (m_AnalisiMaster.Analisi_Gruppo_id.HasValue) // Analisi
                    {
                        masterGruppo_id = m_AnalisiMaster.Analisi_Gruppo_id.Value;
                    }
                }

                if (masterGruppo_id > 0) //l'analisi o intermedio Master NON e' un prodotto controllo il gruppo dell' "analisi" parametro
                {
                    if (prodotto.Prodot_Reparto_ID.HasValue)
                    {
                        if (prodotto.Prodot_Reparto_ID.Value == masterGruppo_id)
                        {
                            flgMatch = true;
                        }
                    }
                }
                if (masterReparto_id > 0) //l'intermedio Master e' un prodotto controllo il reparto dell' "analisi" parametro
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
                        // sono dentro un'analisi e sto scegliendo una posizione Prodotto
                        // quindi i reparto gruppo non combaciano mai , tuttavia per chiarezza lascio il flagMatch 
                        if (flgMatch)
                        {
                            costoDelib = prodotto.Prodot_CostoUnitario_Deliberato;
                        }
                        else
                        {
                            decimal costoInd = 0;
                            if (prodotto.Prodot_PercCostInd != null)
                            { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                            costoDelib = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);

                        }
                        break;
                    default:
                        costoDelib = prodotto.Prodot_CostoUnitario_Deliberato;
                        break;
                }
            }

            if (currPos.VALPOS_MACCHI_ID.HasValue)
            {
                //costoInd = "0";
                MyMacchinario lMacc = le.GetMacchinario(currPos.VALPOS_MACCHI_ID.Value);
                costoDelib = lMacc.Macchi_Prezzo;
            }
            if (currPos.VALPOS_FIGPRO_ID.HasValue )
            {
                MyFigProf f = le.GetFigProfDaFigProf_ID(currPos.VALPOS_FIGPRO_ID.Value);
                costoDelib=f.FigProf_Costo ;
            }

          

            decimal coeff = 1;
            decimal dcostoInd = 0;
            //decimal.TryParse(costoInd.Replace(".", ","), out dcostoInd);
            coeff = (1 + dcostoInd);
            
            // gestione caso "Accettazione"                      
            if (isAccettazione(currPos.VALPOS_FASE_ID))// == 1) 
            {
                if (v.VALORI_GRUPPO_GRUREP_ID.HasValue)
                {
                    MyGrurep mgr = le.GetGruppo(v.VALORI_GRUPPO_GRUREP_ID.Value);
                    decimal val = 0;
                    if (mgr.Grurep_PrezzoUnit_Accettazione.HasValue)
                    {
                        val = mgr.Grurep_PrezzoUnit_Accettazione.Value;
                        costoDelib = val;
                        //costoInd = "0";
                    }
                    
                }
            }

            decimal? ret=null;
            ret = costoDelib;
            
            if (ret.HasValue)
            {
                ret = decimal.Round(ret.Value, 5, MidpointRounding.AwayFromZero);
            }
            else
            { ret = 0; }

            return  ret;
        }

        public ActionResult NuovaValorizzazioneTest()
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Dettaglio analisi";
            int analisi_id = 0;

            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

                //int gruppo_id = 0;
                //PROFIL_PROFILI profilo = en.PROFIL_PROFILI.Where(p => p.PROFIL_CODICE == "VAL").SingleOrDefault();
                T_STAVAL_STATO_VALORIZZAZIONE stato = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(s => s.T_STAVAL_CODICE == "INVAL").First();
                
                //IEnumerable<M_UTPRGR_UTENTI_PROFILI_GRUPPI> gruppi = en.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(x => x.M_UTPRGR_UTENTE_ID == this.CurrentUserID &&
                //    x.M_UTPRGR_PROFIL_ID == profilo.PROFIL_ID);

                ////Abbina la nuova valorizzazione al gruppo del valorizzatore, se non lo trova, prende il primo gruppo dell'elenco
                //if (gruppi.Any() && gruppi.First().M_UTPRGR_GRUREP_ID.HasValue)
                //    gruppo_id = (int)gruppi.First().M_UTPRGR_GRUREP_ID;
                //else
                //    gruppo_id = en.GRUREP_GRUPPI_REPARTI.Where(gr => gr.GRUREP_FLG_REPARTO == false).First().GRUREP_ID;


                //Prendo il primo gruppo tra i gruppi prova
                int gruppo_id = 0;
                LoadEntities le = new LoadEntities();
                Profili p = le.GetProfilo(this.CurrentUserID,this.CurrentProfileID);
                if (p.ElencoGruppiProva.Count > 0)
                {
                    gruppo_id = p.ElencoGruppiProva[0].GruppoID;
                }

                VALORI_VALORIZZAZIONI val = new VALORI_VALORIZZAZIONI();
                val.VALORI_VN = "T";
                val.VALORI_MP_REV = string.Empty;
                val.VALORI_CODICE_DESC = "Prova di valorizzazione";
                val.VALORI_GRUPPO_GRUREP_ID = gruppo_id;
                val.VALORI_FLG_INTERM = false;
                val.VALORI_FLAG_MODELLO = false;
                val.VALORI_FLG_BLOCCATO = false;
                val.VALORI_FLG_PONDERAZIONE = false;
                val.VALORI_UTENTE_ID = this.CurrentUserID;
                val.VALORI_DATA_VN_A = new DateTime(2999, 12, 31);
                val.VALORI_DATA_VN_DA = DateTime.Now;
                val.VALORI_DATA_MP_REV_SCADENZA = new DateTime(2999, 12, 31);
                val.VALORI_T_STAVAL_ID = stato.T_STAVAL_ID;

                en.VALORI_VALORIZZAZIONI.AddObject(val);
                en.SaveChanges();

                val.VALORI_VN = val.VALORI_VN + "_" + val.VALORI_ID;
                en.SaveChanges();

                analisi_id = val.VALORI_ID;
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Analisi");
            }

            return RedirectToAction("AnalisiEdit", new { id = analisi_id });
        }

        [HttpPost]
        public ActionResult ApplicaValorizzazioneAdAnalisi(MyAnalisiAjax mpa)
        {
            bool flagOK = true;
            string er = "";
            try
            {

                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI vMaster = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == mpa.Analisi_id_Master).SingleOrDefault();
                VALORI_VALORIZZAZIONI vcopy = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == mpa.Analisi_id).SingleOrDefault();
                vMaster.VALORI_DIM_LOTTO = vcopy.VALORI_DIM_LOTTO;
                vMaster.VALORI_NR_CAMP_QUALITA = vcopy.VALORI_NR_CAMP_QUALITA;
                vMaster.VALORI_FLG_PONDERAZIONE = vcopy.VALORI_FLG_PONDERAZIONE;
                vMaster.VALORI_PESO_POSITIVO = vcopy.VALORI_PESO_POSITIVO;
                en.SaveChanges();
                List<VALPOS_POSIZIONI> posModello = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == mpa.Analisi_id).ToList<VALPOS_POSIZIONI>();
                List<VALPOS_POSIZIONI> newListPos = new List<VALPOS_POSIZIONI>();
                foreach (VALPOS_POSIZIONI pos in posModello)
                {
                    VALPOS_POSIZIONI currPos = clonaPosizione(pos);
                    currPos.VALPOS_COSTO_QTA = getCosto(currPos, vMaster);
                    // se Fase== Accettazione --> allora l'unità di misura e' Min
                    
                    if (isAccettazione(pos.VALPOS_FASE_ID))// == 1) 
                    {
                        // currPos.VALPOS_T_UNIMIS_ID = 13;  // UdM Minuti
                        currPos.VALPOS_T_UNIMIS_ID = 25; // UdM Numero
                    }
                    //currPos.VALPOS_TOT = decimal.Round((currPos.VALPOS_QTA * currPos.VALPOS_COSTO_QTA.Value), 2, MidpointRounding.AwayFromZero);
                    currPos.VALPOS_SECONDARIE = pos.VALPOS_SECONDARIE;
                    currPos.VALPOS_VALORI_ID = mpa.Analisi_id_Master;
                    newListPos.Add(currPos);
                }
                foreach (VALPOS_POSIZIONI newPos in newListPos)
                {
                    en.VALPOS_POSIZIONI.AddObject(newPos);
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
        public ActionResult ApplicaModelloAAnalisi(MyModelloAjax mpa)
        {
            bool flagOK = true;
            string er = "";
            try
            {
               
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == mpa.MasterID).SingleOrDefault();
                List<VALPOS_POSIZIONI> posModello = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == mpa.Modello_ID).ToList<VALPOS_POSIZIONI>();
                List<VALPOS_POSIZIONI> newListPos = new List<VALPOS_POSIZIONI>();
                foreach (VALPOS_POSIZIONI pos in posModello)
                {
                    VALPOS_POSIZIONI currPos = clonaPosizione(pos);
                    currPos.VALPOS_COSTO_QTA = getCosto(currPos, v);
                    // se Fase== Accettazione --> allora l'unità di misura e' Min
                    if (isAccettazione(pos.VALPOS_FASE_ID))// == 1) 
                    {
                        // currPos.VALPOS_T_UNIMIS_ID = 13;  // UdM Minuti
                        currPos.VALPOS_T_UNIMIS_ID = 25; // UdM Numero
                    }
                    currPos.VALPOS_TOT =  decimal.Round((currPos.VALPOS_QTA * currPos.VALPOS_COSTO_QTA.Value), 2, MidpointRounding.AwayFromZero);
                    currPos.VALPOS_SECONDARIE = mpa.FlagSec;
                    currPos.VALPOS_VALORI_ID = mpa.MasterID;
                    newListPos.Add(currPos);
                }
                foreach (VALPOS_POSIZIONI newPos in newListPos)
                {
                    en.VALPOS_POSIZIONI.AddObject(newPos);
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
        public ActionResult PPCopiaDaValorizzazioni(MyPaginAjax mpa)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ViewData["NumEntities"] = mpa.NumEntities.ToString();
            ViewData["table_search"] = mpa.SearchDescription;
            ViewData["CurrentPage"] = mpa.CurrentPage;
            TempData["NumEntities"] = mpa.NumEntities.ToString();
            TempData["table_search"] = mpa.SearchDescription;
            TempData["CurrentPage"] = mpa.CurrentPage;

            ListaCopiaDaValorizzazioneModel ppCopiaVal = new ListaCopiaDaValorizzazioneModel(mpa.id);

            if (mpa.CurrentPage.HasValue)
            {
                if (mpa.CurrentPage.Value != 0)
                    ppCopiaVal.CurrentPage = mpa.CurrentPage.Value;
                else
                    ppCopiaVal.CurrentPage = 1;
            }
            else
                ppCopiaVal.CurrentPage = 1;

            this.SetMessage = "Elenco valorizzazioni";
            ppCopiaVal.NumEntities = mpa.NumEntities;
            ppCopiaVal.SearchDescription = mpa.SearchDescription;

            List<MyAnalisi> lstAnalisi = ppCopiaVal.GetElencoValorizzazioni(this.CurrentUserID, this.CurrentProfileID).ToList<MyAnalisi>();
            ppCopiaVal.DataTot = lstAnalisi.Count;
            ppCopiaVal.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)lstAnalisi.Count() / ppCopiaVal.NumEntities));
            if (ppCopiaVal.CurrentPage > ppCopiaVal.NumberOfPages) ppCopiaVal.CurrentPage = ppCopiaVal.NumberOfPages;
            ppCopiaVal.Data = lstAnalisi.OrderBy(z => z.Analisi_Descrizione).Skip(ppCopiaVal.NumEntities * (ppCopiaVal.CurrentPage - 1)).Take(ppCopiaVal.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPCopiaDaValorizzazioni", ppCopiaVal) });

        }
        [HttpGet]
        public ActionResult PPCopiaDaValorizzazioni(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            string rangeID = ViewData["NumEntities"] as string;
            string table_search1 = ViewData["table_search"] as string;
            string CurrentPageStr = ViewData["CurrentPage"] as string;
            rangeID = ViewBag.NumEntities;
            table_search1 = ViewBag.table_search;

            this.SetMessage = "Elenco valorizzazioni";
            ListaCopiaDaValorizzazioneModel ppCopiaVal = new ListaCopiaDaValorizzazioneModel(id);
            int numEntities = 0;
            if (int.TryParse(rangeID, out numEntities))
            {
                ppCopiaVal.NumEntities = numEntities;
            }

            if (table_search1 != null)
                ppCopiaVal.SearchDescription = table_search1;

            int pageNum = 0;
            if (int.TryParse(CurrentPageStr, out pageNum))
            {
                ppCopiaVal.CurrentPage = pageNum;
            }
            else
                ppCopiaVal.CurrentPage = 1;

            List<MyAnalisi> lstAnalisi = ppCopiaVal.GetElencoValorizzazioni(this.CurrentUserID, this.CurrentProfileID).ToList<MyAnalisi>();
            ppCopiaVal.DataTot = lstAnalisi .Count ;
            ppCopiaVal.Data = lstAnalisi.OrderBy(z => z.Analisi_Descrizione).Take(ppCopiaVal.NumEntities).ToList();
            ppCopiaVal.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)lstAnalisi.Count() / ppCopiaVal.NumEntities));
            ppCopiaVal.CurrentPage = 1;

            return View(ppCopiaVal);
        }

        [HttpPost]
        public ActionResult PPCopiaModelloAnalisi(MyPaginAjax mpa)
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
        public ActionResult PPCopiaModelloAnalisi(int id, int sec,int p)
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

        public ActionResult PopUpIntermediEsplosi(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            AnalisiModel an = new AnalisiModel(id);

            return View(an);
        }

        public ActionResult PopUpIntermediEsplosi_new(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            StampaIntermediAnalisiModel an = new StampaIntermediAnalisiModel(id);

            return View(an);
        }
        public ActionResult PopUpAnalisi(int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            ListaAnalisiModel an = new ListaAnalisiModel();
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
            an.FiltroStato = 0;
            an.FiltroStatoObsoleta = 0;
            List<MyAnalisi> listAnalisi = an.GetAnalisiDaValorizzare(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).ToList<MyAnalisi>();

            an.Data = listAnalisi.Take(an.NumEntities).ToList();
            an.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listAnalisi.Count() / an.NumEntities));
            an.CurrentPage = 1;


            return View(an);

        }
        [HttpPost]
        public ActionResult PopUpAnalisi(MyPaginAjax mpa)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString() + " - Index ");
            ListaAnalisiModel an = new ListaAnalisiModel();
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

            an.FiltroStato = 0;
            an.FiltroStatoObsoleta = 0;
            an.NumEntities = mpa.NumEntities;
            an.SearchDescription = mpa.SearchDescription;
            List<MyAnalisi> listAnalisi = an.GetAnalisiDaValorizzare(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Analisi_MP_Rev).ToList<MyAnalisi>();
            an.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listAnalisi.Count() / an.NumEntities));
            if (an.CurrentPage > an.NumberOfPages) an.CurrentPage = an.NumberOfPages;
            an.Data = listAnalisi.Skip(an.NumEntities * (an.CurrentPage - 1)).Take(an.NumEntities).ToList();

            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PopUpAnalisi", an) });

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

            prod.Data = prod.ElencoProdotti.Where(pp => !pp.Prodot_Codice.StartsWith("T_")).OrderBy(z => z.Prodot_Codice).Take(prod.NumEntities).ToList();
            prod.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)prod.ElencoProdotti.Count() / prod.NumEntities));
            prod.CurrentPage = 1;

            return View(prod);
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
            prod.Data = prod.ElencoProdotti.Where(pp => !pp.Prodot_Codice.StartsWith("T_")).OrderBy(z => z.Prodot_Codice).Skip(prod.NumEntities * (prod.CurrentPage - 1)).Take(prod.NumEntities).ToList();
            
            return Json(new { status = "ok", partial = this.RenderPartialViewToString("_PPProdotti", prod) });
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


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PPMacchinari(int id, int sec, int? NumEntities, string table_search, int? CurrentPage)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaMacchinariModel IA = new ListaMacchinariModel(valpos_id, sec);

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

            List<MyMacchinario> listMacchinari= IA.GetElencoMacchinari(this.CurrentUserID, this.CurrentProfileID).OrderBy(z => z.Macchi_Codice).ToList<MyMacchinario>();
            
            IA.Data = listMacchinari.OrderBy(z => z.Macchi_Codice).Take(IA.NumEntities).ToList();
            IA.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)listMacchinari.Count() / IA.NumEntities));
            IA.CurrentPage = 1;


            return View(IA);
        }
        [HttpPost]
        public ActionResult PPMacchinari(MyPaginAjax mpa)
        {
            ListaMacchinariModel IA = new ListaMacchinariModel(mpa.valpos_id, mpa.sec);
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
        public ActionResult PPMacchinarioDettRO(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaMacchinariModel IA = new ListaMacchinariModel(valpos_id);
            return View(IA);
        }

        public ActionResult PPIntermAnalisiDettRO(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaIntermediAnalisiModel IA = new ListaIntermediAnalisiModel(valpos_id);
            return View(IA);
        }
        public ActionResult PPProdottiDettRO(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int valpos_id = id;
            ListaProdottoModel IA = new ListaProdottoModel(valpos_id);
            return View(IA);
        }
        public ActionResult AnalisiEdit(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }
            this.SetMessage = "Dettaglio analisi";


            //if stato = da valorizzare (idem su prodotto e intermedio)
            // {attualizzaPosizioniAnalisi(v);}
            
            AnalisiModel an = new AnalisiModel(id);

            //if (an.Analisi.Analisi_T_Staval_id == 2)
            //{
            //    if (IsEditable(an))
            //    {
            //        attualizzaPosizioniAnalisi(id);
            //        saveValAnalisiTot(an);
            //        an = new AnalisiModel(id);
            //    }
            //}

            return View(an);

        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetImportoFaseAccettazione(string analisi_id, string Attivi_id, string valpos_id)
        {
            if (Attivi_id == "") Attivi_id = "0";
            if (valpos_id == "") valpos_id = "0";
            if (analisi_id == "") analisi_id = "0";
            AnalisiModel f = new AnalisiModel(int.Parse(analisi_id));
            LoadEntities le = new LoadEntities ();
            MyGrurep mgr = le.GetGruppo(f.Analisi.Analisi_Gruppo_id.Value);
            
            decimal val = 0;
            if (mgr.Grurep_PrezzoUnit_Accettazione.HasValue)
            { val = mgr.Grurep_PrezzoUnit_Accettazione.Value; }
            string lret ="";
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
            if (analisi_id == "") analisi_id = "0";
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
        public ActionResult AnalisiWorkflow(int id)
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
                if (anaPosId != null && anaPosId.AnalisiPosIds!=null)
                {
                    foreach (int posId in anaPosId.AnalisiPosIds)
                    {
                        List<VALPOS_POSIZIONI> lstDelete = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_ID == posId).ToList<VALPOS_POSIZIONI>();
                        foreach (VALPOS_POSIZIONI vp in lstDelete)
                        {
                            en.VALPOS_POSIZIONI.DeleteObject(vp);
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
        [HttpPost]
        public ActionResult ClonaPosizione(MyAnalisiPosAjaxList anaPosId)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                if (anaPosId != null && anaPosId.AnalisiPosIds != null)
                {
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
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetElencoChart(string analisi_id)
        {
            int anal_ID = int.Parse(analisi_id);
            AnalisiModel an = new AnalisiModel(anal_ID);
            return Json(an.GetDataChart(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// /// Attenzione. Funzione doppia.. se si cambia questa cambiare anche l'altra con altra firma
        /// </summary>
        /// <param name="an"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool saveValAnalisiTot(AnalisiModel an)
        {
            bool flagOK = true;
            
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi.Analisi_id).SingleOrDefault();
                //v.VALORI_FLG_PONDERAZIONE = an.Analisi_flgPonderazione;

                v.VALORI_DIM_LOTTO = an.Analisi.Analisi_Dim_Lotto;
                v.VALORI_NR_CAMP_QUALITA = an.Analisi.Analisi_Nr_Camp_Qualita;
                v.VALORI_MATRICE = an.Analisi.Analisi_Matrice;
                v.VALORI_PESO_POSITIVO = an.Analisi.Analisi_Peso_Positivo;

                // Inizio Ric#3

                int? Utente_id_old = v.VALORI_UTENTE_ID;

                if (an.Analisi.Analisi_utente_id != null)
                {
                    v.VALORI_UTENTE_ID = an.Analisi.Analisi_utente_id;
                }

                v.VALORI_FLG_ASSEGN_AL_GRUPPO = an.Analisi.Analisi_flg_assegn_al_gruppo;

                List<RICHIE_RICHIESTE> ric_da_aggiornare = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_DESTINATARIO_UTENTE_ID == Utente_id_old 
                                                                                         && z.T_STARIC_STATO_RICHIESTA.T_STARIC_CODICE == "INV"
                                                                                         && z.RICHIE_VALORI_ID == v.VALORI_ID).ToList<RICHIE_RICHIESTE>();
                foreach (RICHIE_RICHIESTE r in ric_da_aggiornare)
                {
                    r.RICHIE_DESTINATARIO_UTENTE_ID = v.VALORI_UTENTE_ID;
                    r.RICHIE_FLG_ASSEGN_AL_GRUPPO = v.VALORI_FLG_ASSEGN_AL_GRUPPO;
                }

                //fine Ric#3

                //  v.VALORI_CODICE_DESC = an.Analisi_Codice_Descrizione; 
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
                    pesoNeg = 0; // 
                }

                //totCorrente = totCorrentePrim * (pesoPos / 100) + totCorrenteSec * (pesoNeg / 100);
                totCorrente = totCorrentePrim + totCorrenteSec * (pesoNeg / 100);


                if (totCorrente.HasValue)
                {
                    totCorrente = decimal.Round(totCorrente.Value, 2, MidpointRounding.AwayFromZero);
                }


                float coeffUnitario = 0;
                if (v.VALORI_DIM_LOTTO.HasValue)
                {
                    if (v.VALORI_NR_CAMP_QUALITA.HasValue)
                    {
                        coeffUnitario = ((float)((float)v.VALORI_NR_CAMP_QUALITA.Value / (float)v.VALORI_DIM_LOTTO.Value)) + 1;
                        coeffUnitario = ((float)coeffUnitario / (float)v.VALORI_DIM_LOTTO.Value);
                    }
                    else
                    { coeffUnitario = 1; }
                }

                totCorrente = decimal.Round(totCorrente.Value * (decimal)coeffUnitario, 2, MidpointRounding.AwayFromZero);

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

        /// <summary>
        /// Attenzione. Funzione doppia.. se si cambia questa cambiare anche l'altra con altra firma
        /// </summary>
        /// <param name="an"></param>
        /// <param name="er"></param>
        /// <returns></returns>

        //Ric#3
        [HttpPost]
        public ActionResult ChangeAnalisiUtenteAss(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();


                int? Utente_id_old = v.VALORI_UTENTE_ID;

                if (an.Analisi_utente_id != null)
                {
                    v.VALORI_UTENTE_ID = an.Analisi_utente_id;
                }
                v.VALORI_FLG_ASSEGN_AL_GRUPPO = an.Analisi_flg_assegn_al_gruppo;


                List<RICHIE_RICHIESTE> ric_da_aggiornare = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_DESTINATARIO_UTENTE_ID == Utente_id_old
                                                                                         && z.T_STARIC_STATO_RICHIESTA.T_STARIC_CODICE == "INV"
                                                                                         && z.RICHIE_VALORI_ID == v.VALORI_ID).ToList<RICHIE_RICHIESTE>();
                foreach (RICHIE_RICHIESTE r in ric_da_aggiornare)
                {
                    r.RICHIE_DESTINATARIO_UTENTE_ID = v.VALORI_UTENTE_ID;
                    r.RICHIE_FLG_ASSEGN_AL_GRUPPO = v.VALORI_FLG_ASSEGN_AL_GRUPPO;    
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

                v.VALORI_DIM_LOTTO = an.Analisi_Dim_Lotto;
                v.VALORI_NR_CAMP_QUALITA = an.Analisi_nr_Campioni;
                v.VALORI_MATRICE = an.Analisi_Matrice;
                v.VALORI_PESO_POSITIVO = an.Analisi_Peso_Positivo;

                
                // Inizio Ric#3
                int? Utente_id_old = v.VALORI_UTENTE_ID;

                if (an.Analisi_utente_id != null)
                {
                    v.VALORI_UTENTE_ID = an.Analisi_utente_id;
                }
                v.VALORI_FLG_ASSEGN_AL_GRUPPO = an.Analisi_flg_assegn_al_gruppo;

                List<RICHIE_RICHIESTE> ric_da_aggiornare = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_DESTINATARIO_UTENTE_ID == Utente_id_old
                                                                                         && z.T_STARIC_STATO_RICHIESTA.T_STARIC_CODICE == "INV"
                                                                                         && z.RICHIE_VALORI_ID == v.VALORI_ID).ToList<RICHIE_RICHIESTE>();
                foreach (RICHIE_RICHIESTE r in ric_da_aggiornare)
                {
                    r.RICHIE_DESTINATARIO_UTENTE_ID = v.VALORI_UTENTE_ID;
                    r.RICHIE_FLG_ASSEGN_AL_GRUPPO = v.VALORI_FLG_ASSEGN_AL_GRUPPO;
                }
                
                //Fine Ric#3

                //  v.VALORI_CODICE_DESC = an.Analisi_Codice_Descrizione; 
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
                    pesoNeg = 0; // 
                }

                //totCorrente = totCorrentePrim * (pesoPos / 100) + totCorrenteSec * (pesoNeg / 100);
                totCorrente = totCorrentePrim + totCorrenteSec * (pesoNeg / 100);


                if (totCorrente.HasValue)
                {
                    totCorrente = decimal.Round(totCorrente.Value, 2, MidpointRounding.AwayFromZero);
                }


                float coeffUnitario = 0;
                if (v.VALORI_DIM_LOTTO.HasValue)
                {
                    if (v.VALORI_NR_CAMP_QUALITA.HasValue)
                    {
                        coeffUnitario = ((float)((float)v.VALORI_NR_CAMP_QUALITA.Value / (float)v.VALORI_DIM_LOTTO.Value)) + 1;
                        coeffUnitario = ((float)coeffUnitario / (float)v.VALORI_DIM_LOTTO.Value);
                    }
                    else
                    { coeffUnitario = 1; }
                }

                totCorrente = decimal.Round(totCorrente.Value * (decimal)coeffUnitario, 2, MidpointRounding.AwayFromZero);

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
                //string path = Request.Url.AbsoluteUri.Replace(Request.Url.Query ,"").Replace (Request.Url.AbsolutePath, "");
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
                        do {
                            byteCount = responseStream.Read(buffer, 0, buffer.Length);
                            memStream.Write(buffer, 0, byteCount);
                        } while (byteCount > 0);
                    }
                    memStream.Seek(0, SeekOrigin.Begin);

                    Response.Clear();
                    Response.AddHeader("Accept-Header", memStream.Length.ToString());
                    Response.ContentType = "application/pdf";
                    Response.OutputStream.Write( memStream.ToArray(), 0, Convert.ToInt32(memStream.Length));
                    Response.Flush();
                    try { Response.End(); }
                    catch { }
                }
            }
            catch(Exception ex) 
            {
                err += ex.Message;
            }
            return null;
        }
       

        [HttpPost]
        public ActionResult RevisioneFormale(MyAnalisiAjax an)
        {
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
            string err = string.Empty;
            bool ret = true;

           try
            {
                //Recupera la revisione corrente
                string[] s = v.VALORI_MP_REV.Split('-');
                string srev =s[1];
                int rev = int.Parse(srev.Trim());

                //Recupera la versione precedente della valorizzazione
                string prevMP = s[0] + "- " + (rev - 1).ToString();
                VALORI_VALORIZZAZIONI vp = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_MP_REV == prevMP && z.VALORI_VN == v.VALORI_VN).SingleOrDefault();

                if (vp == null) err = "La revisione precedente non è stata trovata";
                else
                {
                    //Elimina le eventuali posizioni presenti
                    foreach (VALPOS_POSIZIONI pos in en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == an.Analisi_id))
                    {
                        en.DeleteObject(pos);
                    }

                    //copia la valorizzazione
                    v.VALORI_DIM_LOTTO = vp.VALORI_DIM_LOTTO;
                    v.VALORI_NR_CAMP_QUALITA = vp.VALORI_NR_CAMP_QUALITA;
                    v.VALORI_FLG_PONDERAZIONE = vp.VALORI_FLG_PONDERAZIONE;
                    v.VALORI_PESO_POSITIVO = vp.VALORI_PESO_POSITIVO;
                    v.VALORI_COSTO_TOT = vp.VALORI_COSTO_TOT;
                    en.SaveChanges();

                    if (an.AnalisiPosIds == null) an.AnalisiPosIds = new List<int>();
                    if (an.AnalisiPosSIds == null) an.AnalisiPosSIds = new List<int>();

                    List<VALPOS_POSIZIONI> posModello = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == vp.VALORI_ID).ToList<VALPOS_POSIZIONI>();
                    List<VALPOS_POSIZIONI> newListPos = new List<VALPOS_POSIZIONI>();
                    foreach (VALPOS_POSIZIONI pos in posModello)
                    {
                        VALPOS_POSIZIONI currPos = clonaPosizione(pos);
                        currPos.VALPOS_COSTO_QTA = getCosto(currPos, vp);

                        // se Fase== Accettazione --> allora l'unità di misura e' Numero
                        if (isAccettazione(pos.VALPOS_FASE_ID)) currPos.VALPOS_T_UNIMIS_ID = 25; // UdM Numero

                        decimal? coef = 1;
                        if (currPos.VALPOS_COEFF_CONVERSIONE.HasValue)
                            //07/03/2019 andava in errore nel caso in cui il coefficente era scritto in modo esponenziole nel db es. 1E-06
                            //ho corretto aggiungendo System.Globalization.NumberStyles.Float
                            coef = decimal.Parse(currPos.VALPOS_COEFF_CONVERSIONE.ToString(), System.Globalization.NumberStyles.Float);

                        //sim non teneva conto del coef di conversione
                        currPos.VALPOS_TOT = decimal.Round((currPos.VALPOS_QTA * currPos.VALPOS_COSTO_QTA.Value * coef.Value), 5, MidpointRounding.AwayFromZero);
                        currPos.VALPOS_SECONDARIE = pos.VALPOS_SECONDARIE;
                        currPos.VALPOS_VALORI_ID = v.VALORI_ID;
                        newListPos.Add(currPos);
                    }
                    foreach (VALPOS_POSIZIONI newPos in newListPos)
                    {
                        en.VALPOS_POSIZIONI.AddObject(newPos);
                        en.SaveChanges();

                        if (!newPos.VALPOS_SECONDARIE)
                            an.AnalisiPosIds.Add(newPos.VALPOS_ID);
                        else
                            an.AnalisiPosSIds.Add(newPos.VALPOS_ID);
                    }

                    //valida le posizioni
                    string listaErrori = string.Empty;
                    checkAnalisi(v, en, true, out listaErrori,an.AnalisiPosIds, an.AnalisiPosSIds);

                    //Se le posizioni sono valide la valorizzazione viene inviata in automatico
                    if (listaErrori.Length == 0)
                    {
                        T_STAVAL_STATO_VALORIZZAZIONE tStaval = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z => z.T_STAVAL_CODICE == "INVALI").SingleOrDefault();

                        chiudiRichiestaCorrente(v.VALORI_ID, en);

                        RICHIE_RICHIESTE r = creaRichiestaValidazioneAnalisi(v, en, DateTime.Now);
                        if (r.RICHIE_DESTINATARIO_UTENTE_ID > 0)
                        {
                            v.VALORI_T_STAVAL_ID = tStaval.T_STAVAL_ID;
                            v.VALORI_MATRICE = "Revisione formale";
                            //Ric#5
                            en.USPT_STORICIZZA_COSTI_ANALISI(v.VALORI_ID);
                            salvaTrackingAnalisi(v, en, DateTime.Now);

                            //sim 2020-10-13: correzione, non veniva aggiornato il totale della valorizzazione ma solo i totali delle singole posizioni
                            an.Analisi_Dim_Lotto = int.Parse(v.VALORI_DIM_LOTTO.ToString());
                            an.Analisi_nr_Campioni = int.Parse(v.VALORI_NR_CAMP_QUALITA.ToString());
                            an.Analisi_flgPonderazione = v.VALORI_FLG_PONDERAZIONE;
                            an.Analisi_Peso_Positivo = int.Parse(v.VALORI_PESO_POSITIVO.ToString());                            
                            SaveValAnalisiTot(an);
                            //sim 2020-10-13: fine correzione

                            en.SaveChanges();

                            
                            inviaEmail(r);
                        }
                    }
                    //else
                    //    err = "La revisione formale è stata creata, ma la valorizzazione contiene delle posizioni non valide.";

                }

             }
             catch (Exception e)
             {
                 err = e.Message;
             }

            if (err.Length > 0) ret = false;
            return Json(new { ok = ret, infopersonali = err });
        }

        [HttpPost]
        public ActionResult SaveValAnalisiTot(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er ="";
            flagOK = saveValAnalisiTot( an,out er);
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
            
            return "[pos. positive n. " + pos.ToString() + "] " + errore;
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
        
        private bool checkAnalisi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en, bool flagControllo, out string listaErrori, List<int> analisiPosIDs, List<int> analisiPosSIDs)
        {
            listaErrori = "";
            string IntermedioNonValidato = "";
            List<int> idStatoCorrettoAvanzamentoIntermedio = new List<int> {10};
            //List<int> idStatoCorrettoAvanzamentoAnalisi = new List<int> {4,5,6};
            List<int> idStatoCorrettoAvanzamentoProdotto = new List<int> {4,5,6,8};

            List<string> elencoErrori = new List<string>();
            int analisi_id = v.VALORI_ID ;

            LoadEntities  le = new LoadEntities ();
            List<MyAnalisiPos> lstPrimarie = le.GetAnalisiPos(v.VALORI_ID );
            List<MyAnalisiPos> lstSecondarie = le.GetAnalisiPosSec(v.VALORI_ID );

            
            //List<VALPOS_POSIZIONI> lstPosPrimarie = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == analisi_id && z.VALPOS_SECONDARIE == false).OrderBy(z=>z.VALPOS_ID) .ToList<VALPOS_POSIZIONI>();
            List<VALPOS_POSIZIONI> lstPosPrimarie = new List<VALPOS_POSIZIONI>();

            foreach (MyAnalisiPos ap in lstPrimarie)
            {
                lstPosPrimarie.Add(en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == analisi_id && z.VALPOS_SECONDARIE == false && z.VALPOS_ID == ap.AnalisiPos_id).SingleOrDefault() );
            }

            //List<VALPOS_POSIZIONI> lstPosSecodarie = en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == analisi_id && z.VALPOS_SECONDARIE == true).OrderBy(z=>z.VALPOS_ID) .ToList<VALPOS_POSIZIONI>();
            List<VALPOS_POSIZIONI> lstPosSecodarie = new List<VALPOS_POSIZIONI>();

            foreach (MyAnalisiPos aps in lstSecondarie)
            {
                lstPosSecodarie.Add(en.VALPOS_POSIZIONI.Where(z => z.VALPOS_VALORI_ID == analisi_id && z.VALPOS_SECONDARIE == true && z.VALPOS_ID == aps.AnalisiPos_id).SingleOrDefault());
            }


            int dimLotto =0;
            if(v.VALORI_DIM_LOTTO.HasValue )
            {dimLotto = v.VALORI_DIM_LOTTO.Value ;}
            if (dimLotto == 0)
            {
                elencoErrori.Add("Dimensione lotto non valorizzata.");
            }

            int count=1;
            foreach (VALPOS_POSIZIONI pos in lstPosPrimarie)
            {
                count = analisiPosIDs.IndexOf(pos.VALPOS_ID);count++;

                if (pos.VALPOS_INTERM_ID == v.VALORI_ID)
                {
                    elencoErrori.Add(insertErrore(count, "Non è possibile usare la stessa analisi che si sta valorizzando."));
                    continue;
                }

                if (!pos.VALPOS_FASE_ID.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Fase non inserita"));
                }
                if (pos.VALPOS_FASE_ID.HasValue && isAccettazione(pos.VALPOS_FASE_ID)) // pos.VALPOS_FASE_ID == 1)
                {
                    if (pos.VALPOS_QTA != dimLotto)
                    {
                        elencoErrori.Add(insertErrore(count, "Quantità Fase Accettazione diversa dalla Dimensione Lotto."));
                    }
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

                if (!pos.VALPOS_T_UNIMIS_ID.HasValue && pos.VALPOS_PRODOT_ID.HasValue )
                {
                    elencoErrori.Add(insertErrore(count, "Unità di misura non inserita."));
                }
                if (!pos.VALPOS_COEFF_CONVERSIONE.HasValue)
                {
                    elencoErrori.Add(insertErrore(count, "Coef. di conversione non inserito."));
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
                    count = analisiPosSIDs.IndexOf(pos.VALPOS_ID); count++;

                    if (pos.VALPOS_INTERM_ID == v.VALORI_ID)
                    {
                        elencoErrori.Add(insertErrore(count, "Non è possibile usare la stessa analisi che si sta valorizzando.", true));
                        continue;
                    }

                    if (!pos.VALPOS_FASE_ID.HasValue)
                    {
                        elencoErrori.Add(insertErrore(count, "Fase non inserita", true));
                    }
                    if (pos.VALPOS_FASE_ID.HasValue && isAccettazione(pos.VALPOS_FASE_ID))//pos.VALPOS_FASE_ID == 1)
                    {
                        if (pos.VALPOS_QTA != dimLotto)
                        {
                            elencoErrori.Add(insertErrore(count, "Quantità Fase Accettazione diversa dalla Dimensione Lotto.",true));
                        }
                    }
                    if (pos.VALPOS_QTA == 0)
                    {
                        elencoErrori.Add(insertErrore(count, "Quantità non inserita",true));
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
            if(elencoErrori.Count ==0)
                return true;
            if (elencoErrori.Count != 0)
            {
                listaErrori = "";
                if (!flagControllo)
                {
                    listaErrori = "Impossibile inviare la valorizzazione al validatore.<br/>";
                }
                 
                listaErrori += "Errori riscontrati:<br/>";
                foreach (string s in elencoErrori)
                {
                    listaErrori += s + "<br/>";
                }
            }
            return false ;
        }
        private void chiudiRichiestaCorrente(int analisi_id,IZSLER_CAP_Entities en)
        {
            List<RICHIE_RICHIESTE> lstRic = en.RICHIE_RICHIESTE.Where(z => z.RICHIE_VALORI_ID == analisi_id && z.RICHIE_T_STARIC_ID == 2).ToList<RICHIE_RICHIESTE>();
            foreach (RICHIE_RICHIESTE r in lstRic )
            {
                r.RICHIE_T_STARIC_ID = 3; // Evasa  
            }
        }
        
        private RICHIE_RICHIESTE creaRichiestaSbloccoAnalisi(VALORI_VALORIZZAZIONI v, string motivo, int priorita, IZSLER_CAP_Entities en, DateTime dt)
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
            return r;
        }
        private RICHIE_RICHIESTE creaRichiestaRespintaAnalisi(VALORI_VALORIZZAZIONI v, string motivo, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = v.VALORI_UTENTE_ID;
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 1; // valorizzazione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = motivo;
            r.RICHIE_TITOLO = "Respingimento validazione analisi [" + v.VALORI_VN + " - " + v.VALORI_MP_REV + "]";
            r.RICHIE_VALORI_ID = v.VALORI_ID;
            r.RICHIE_FLG_ASSEGN_AL_GRUPPO = v.VALORI_FLG_ASSEGN_AL_GRUPPO;
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
    
        }
        private RICHIE_RICHIESTE creaRichiestaDeliberaAnalisi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en, DateTime dt)
        {
            RICHIE_RICHIESTE r = new RICHIE_RICHIESTE();
            //r.RICHIE_CODICE;
            r.RICHIE_DATA_RICHIESTA = dt;
            r.RICHIE_DESTINATARIO_UTENTE_ID = getDestinatarioUserID(v, "CDG", en);
            //r.RICHIE_ID ;
            //r.RICHIE_PRODOT_ID ;
            r.RICHIE_RICHIEDENTE_UTENTE_ID = this.CurrentUserID;
            r.RICHIE_T_RICHIE_ID = 1; // valorizzazione
            r.RICHIE_T_RICPRI_ID = 3;// priorita normale
            r.RICHIE_T_STARIC_ID = 2; // inviata
            r.RICHIE_TESTO = "Richiesta automatica Delibera analisi";
            r.RICHIE_TITOLO = "Richiesta delibera analisi [" + v.VALORI_VN + " - " + v.VALORI_MP_REV + "]";
            r.RICHIE_VALORI_ID = v.VALORI_ID;
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
        }
        private RICHIE_RICHIESTE creaRichiestaValidazioneAnalisi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en,DateTime dt)
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
            r.RICHIE_FLG_ASSEGN_AL_GRUPPO = v.VALORI_FLG_ASSEGN_AL_GRUPPO; //Ric#3
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
        }
        private RICHIE_RICHIESTE creaRichiestaRispostaSblocco(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en, DateTime dt)
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
            r.RICHIE_FLG_ASSEGN_AL_GRUPPO = v.VALORI_FLG_ASSEGN_AL_GRUPPO; //Ric#3
            en.RICHIE_RICHIESTE.AddObject(r);
            return r;
        }
        private string getErroreDestinatarioUserID(VALORI_VALORIZZAZIONI v, string ProfilCodice, IZSLER_CAP_Entities en)
        {
            PROFIL_PROFILI p = en.PROFIL_PROFILI.Where(z => z.PROFIL_CODICE == ProfilCodice).SingleOrDefault();
            string err = "Impossibile attuare la richiesta.<br/>";
            err += "Nessun responsabile di tipo "+p.PROFIL_DESC +" è stato trovato per il Gruppo ";
            if (v.VALORI_GRUPPO_GRUREP_ID.HasValue)
            {
                
                GRUREP_GRUPPI_REPARTI gr = en.GRUREP_GRUPPI_REPARTI.Where(z => z.GRUREP_ID == v.VALORI_GRUPPO_GRUREP_ID.Value).SingleOrDefault();
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
            return 0;
        }
        [HttpPost]
        public ActionResult RichiediSbloccoAnalisi(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                T_STAVAL_STATO_VALORIZZAZIONE tStaval = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z => z.T_STAVAL_CODICE == "INSBLO").SingleOrDefault();
                chiudiRichiestaCorrente(v.VALORI_ID, en);
                v.VALORI_T_STAVAL_ID = tStaval.T_STAVAL_ID;
                RICHIE_RICHIESTE r= creaRichiestaSbloccoAnalisi(v, an.Analisi_Motivo, an.Analisi_T_RICPRI_ID, en, dt);
                if (r.RICHIE_DESTINATARIO_UTENTE_ID > 0)
                {
                    salvaTrackingAnalisi(v, en, dt);
                    en.SaveChanges();
                    inviaEmail(r);
                    flagOK = true;
                }
                else
                {
                    er = getErroreDestinatarioUserID(v, "CDG", en);
                    flagOK = false; 
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
        public ActionResult RespingiAnalisi(MyAnalisiAjax an)
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
                RICHIE_RICHIESTE r= creaRichiestaRespintaAnalisi(v, an.Analisi_Motivo, en, dt);
                salvaTrackingAnalisi(v, en, dt);
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
        public ActionResult PopUpRichiediSbloccoAnalisi(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int analisi_id = id;
            AnalisiModelSblocco am = new AnalisiModelSblocco(analisi_id);
            return View(am);
        }
        public ActionResult PopUpRespingiAnalisi(int id)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null) { return r; }

            int analisi_id = id;
            AnalisiModel am = new AnalisiModel(analisi_id);
            return View(am);
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

                if (r.RICHIE_DESTINATARIO_UTENTE_ID.HasValue )
                {
                    MailMessage mess = new MailMessage();
                    mess.To.Add(getAddressByUserID(r.RICHIE_DESTINATARIO_UTENTE_ID.Value));
                    if (forceEmailServiceAccount())
                        mess.From = getAddressServiceAccount();
                    else
                        mess.From = getAddressByUserID(r.RICHIE_RICHIEDENTE_UTENTE_ID);
                    mess.Body = r.RICHIE_TESTO ;
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
        public ActionResult DeliberaAnalisi(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                T_STAVAL_STATO_VALORIZZAZIONE tStaval = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z => z.T_STAVAL_CODICE == "DELIBE").SingleOrDefault();
                chiudiRichiestaCorrente(v.VALORI_ID, en);
                v.VALORI_T_STAVAL_ID = tStaval.T_STAVAL_ID;
                v.VALORI_FLG_BLOCCATO = true;
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
        public ActionResult ApprovaEdInviaCdGAnalisi(MyAnalisiAjax an)
        {
            bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt =DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                T_STAVAL_STATO_VALORIZZAZIONE tStaval = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z => z.T_STAVAL_CODICE == "INDEL").SingleOrDefault();
                chiudiRichiestaCorrente(v.VALORI_ID, en);
                v.VALORI_T_STAVAL_ID = tStaval.T_STAVAL_ID;
                v.VALORI_FLG_BLOCCATO = true;
                RICHIE_RICHIESTE r= creaRichiestaDeliberaAnalisi(v, en, dt);
                if(r.RICHIE_DESTINATARIO_UTENTE_ID >0)
                {
                    salvaTrackingAnalisi(v, en, dt);
                    en.SaveChanges();
                    inviaEmail(r);
                    flagOK = true;
                }
                else
                {
                    er = getErroreDestinatarioUserID(v, "CDG", en);
                    flagOK = false; 
                }
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        private void salvaTrackingAnalisi(VALORI_VALORIZZAZIONI v, IZSLER_CAP_Entities en ,DateTime dt)
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
            trk.TRKVAL_COSTO_TARIFFA_DELIBERATO = v.VALORI_COSTO_TARIFFA_DELIBERATO ;
            trk.TRKVAL_COSTO_TARIFFA_D_DELIBERATO = v.VALORI_COSTO_TARIFFA_D_DELIBERATO ;

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

        //sim verifico che l'analisi sia stata aggiornata da non più di un ora prima di effettuare l'invio al validatore
        public bool IsAggiornata(VALORI_VALORIZZAZIONI v, out string listaErrori)
        {
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            
            listaErrori = "";
            if (!v.VALORI_TS_AGGIORNAMENTO_POSIZIONI.HasValue || DateTime.Now.Subtract(v.VALORI_TS_AGGIORNAMENTO_POSIZIONI.Value).TotalHours > 1)
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
        public ActionResult InviaValidatoreAnalisi(MyAnalisiAjax an)
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
                    T_STAVAL_STATO_VALORIZZAZIONE tStaval = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z => z.T_STAVAL_CODICE == "INVALI").SingleOrDefault();
                    string listaErrori = "";
                    string msg_da_aggiornare =""; //sim
                    bool flgOKAnalisi = checkAnalisi(v, en, false, out listaErrori, an.AnalisiPosIds, an.AnalisiPosSIds);
                    bool flgOKAggiornata = IsAggiornata(v, out msg_da_aggiornare);//sim
                    if (flgOKAnalisi && flgOKAggiornata) //sim
                    {
                        chiudiRichiestaCorrente(v.VALORI_ID, en);
                        v.VALORI_T_STAVAL_ID = tStaval.T_STAVAL_ID;
                        RICHIE_RICHIESTE r = creaRichiestaValidazioneAnalisi(v, en, dt);
                        if (r.RICHIE_DESTINATARIO_UTENTE_ID > 0)
                        {
                            salvaTrackingAnalisi(v, en, dt);
                            en.SaveChanges();
                            //Ric#5
                            en.USPT_STORICIZZA_COSTI_ANALISI(v.VALORI_ID);
                            inviaEmail(r);
                            flagOK = true;
                        }
                        else
                        {
                            er = getErroreDestinatarioUserID(v, "REFVAL", en);
                            flagOK = false;
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
        public ActionResult CheckPosizioniAnalisi(MyAnalisiAjax an)
        { 
             bool flagOK = true;
            string er = "";
            try
            {
                DateTime dt = DateTime.Now;
                IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
                //sempre update
                VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == an.Analisi_id).SingleOrDefault();
                T_STAVAL_STATO_VALORIZZAZIONE tStaval = en.T_STAVAL_STATO_VALORIZZAZIONE.Where(z => z.T_STAVAL_CODICE == "INVALI").SingleOrDefault();
                string listaErrori = "";
                bool flgOKAnalisi = checkAnalisi(v, en, true, out listaErrori, an.AnalisiPosIds, an.AnalisiPosSIds);
                if (!flgOKAnalisi)
                {
                    flagOK = false;
                    er = "Elenco Errori: <br/>" + listaErrori ;
                }    
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
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

                if (v.T_STAVAL_STATO_VALORIZZAZIONE.T_STAVAL_ID == 2) //Controllo lo stato
                {
                    attualizzaPosizioniAnalisi(v);
                    TrackingAggiornaPosizioni(an.Analisi_id); //sim
                    SaveValAnalisiTot(an);
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
        public ActionResult SbloccaAnalisi(MyAnalisiAjax an)
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
                RICHIE_RICHIESTE r = creaRichiestaRispostaSblocco(v, en, dt);
                salvaTrackingAnalisi(v, en, dt);
                en.SaveChanges();
                inviaEmail(r);
                flagOK = true;
                attualizzaPosizioniAnalisi(v);
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
        private void attualizzaPosizioniAnalisi(VALORI_VALORIZZAZIONI v)
        {
            AttualizzatorePosizioni a = new AttualizzatorePosizioni(v);
            a.Attualizza();
        }
        //private void attualizzaPosizioniAnalisi(int valori_id)
        //{
        //    IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
        //    VALORI_VALORIZZAZIONI v = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == valori_id).SingleOrDefault();
        //    AttualizzatorePosizioni a = new AttualizzatorePosizioni(v);
        //    a.Attualizza();
        //}
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
                            // UdM Numero
                            vp.VALPOS_T_UNIMIS_ID = 25; // UdM Numero

                            vp.VALPOS_COD_SETTORE = anapos.AnalisiPos_CodSettore;
                            break;
                    case TipoSave.FaseAccettazione:
                            if (anapos.AnalisiPos_Fase_id == 0)
                                vp.VALPOS_FASE_ID  = null;
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

                            // vp.VALPOS_T_UNIMIS_ID = 13; // minuti
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

                int valori_id = vp.VALPOS_VALORI_ID;
                VALORI_VALORIZZAZIONI vl = en.VALORI_VALORIZZAZIONI.Where(z => z.VALORI_ID == vp.VALPOS_VALORI_ID).SingleOrDefault();

                MyAnalisiAjax an = new MyAnalisiAjax();
                an.Analisi_id = vl.VALORI_ID;

                if (String.IsNullOrEmpty(vl.VALORI_DIM_LOTTO.ToString()))
                {
                    an.Analisi_Dim_Lotto = 1;
                }
                else
                {
                    an.Analisi_Dim_Lotto = int.Parse(vl.VALORI_DIM_LOTTO.ToString());
                }

                if (String.IsNullOrEmpty(vl.VALORI_NR_CAMP_QUALITA.ToString()))
                {
                    an.Analisi_nr_Campioni = 0;
                }
                else
                {
                    an.Analisi_nr_Campioni = int.Parse(vl.VALORI_NR_CAMP_QUALITA.ToString());
                }

                an.Analisi_Matrice = vl.VALORI_MATRICE;

                if (String.IsNullOrEmpty(vl.VALORI_PESO_POSITIVO.ToString()))
                {
                    an.Analisi_Peso_Positivo = 0;
                }
                else
                {
                    an.Analisi_Peso_Positivo = int.Parse(vl.VALORI_PESO_POSITIVO.ToString());
                }

                //Ric#3
                an.Analisi_flg_assegn_al_gruppo = vl.VALORI_FLG_ASSEGN_AL_GRUPPO;
                //Ric#3
                an.Analisi_utente_id = vl.VALORI_UTENTE_ID;

                SaveValAnalisiTot(an);

                flagOK = true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                flagOK = false;
            }

            return Json(new { ok = flagOK, infopersonali = er });
        }
       
    }
}

