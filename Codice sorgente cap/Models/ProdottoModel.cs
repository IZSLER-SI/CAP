using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;
using System.Text;

namespace IZSLER_CAP.Models
{
    
    public class StampaIntermediModel : B16ModelMgr
    { 
        private MyProdotto m_MyProdotto = null;
        public MyProdotto Prodotto { get { return m_MyProdotto; } }
        private IEnumerable<MyProdotto> m_listaProdotto = null;
        private IEnumerable<MyIntermediEsplosiProdotto> m_listaIntermediEsplosi = null;

        public IEnumerable<MyIntermediEsplosiProdotto> ElencoIntermediEsplosi { get { return m_listaIntermediEsplosi; } }

        public StampaIntermediModel()
        {
            m_listaProdotto = m_le.GetProdotti();
            
        }

        public StampaIntermediModel(int prodotto_id)
        {
            this.m_MyProdotto = m_le.GetProdotti(prodotto_id);
            m_listaIntermediEsplosi = m_le.GetIntermediEsplosiProdotto_stampa(prodotto_id);

        }

        public string getFatherId(string idParent, string id, bool sec)
        {
            string sChild = getChildId(idParent, id, sec);
            string lret = "expandable_open_button" + sChild;
            return lret;
        }

    public string getChildId(string idParent, string id, bool sec)
    {
        string lret = string.Empty;
        string sSec = sec ? "s" : "";
        if (string.IsNullOrEmpty(idParent))
        {
            lret = "_" + sSec + id;
        }
        else
        {
            lret = idParent + "_" + sSec + id;
        }
        return lret;

    }

    public string getChildId2(string idParent, bool sec)
    {
        string lret = string.Empty;
        string sSec = sec ? "s" : "";
        List<MyIntermediEsplosiProdotto> a = m_listaIntermediEsplosi.Where(z => z.Intesp_id == idParent).ToList<MyIntermediEsplosiProdotto>();
        string GranParent = m_listaIntermediEsplosi.Where(z => z.Intesp_id == idParent).SingleOrDefault().Intesp_id_padre;

        if (string.IsNullOrEmpty(GranParent))
        {
            lret = "_" + sSec + idParent;
        }
        else
        {
            lret = GranParent + "_" + sSec + idParent;
        }
        return lret;

    }

        public HtmlString StampaIntermediHtml()
        {
            string s = "";

            List<MyIntermediEsplosiProdotto> lstIntermediEsplosi = ElencoIntermediEsplosi.ToList<MyIntermediEsplosiProdotto>();

            int? livello_old = 1;

            foreach (MyIntermediEsplosiProdotto map in lstIntermediEsplosi)
            {
                if (livello_old > map.Intesp_livello)
                {
                    int? i = @livello_old;

                    while (i > map.Intesp_livello)
                    {
                        s = s + "</tbody>";
                        s = s + "</table>";
                        s = s + "</td>";
                        s = s + "</tr>";
                        i = i - 1;
                    }
                }
                
                if (@livello_old < map.Intesp_livello)
                {
                    s = s + "<tr style =\"width :100%;display:none;\" id=\"" + getChildId2(map.Intesp_id_padre, false) + "\">";
                    s = s + "<td width=\"10px\"></td>";
                    s = s + "<td colspan=\"4\">";
                    s = s + "<table class=\"table responsive-table\" id=\"idTable\">";
                    s = s + "<thead>";
                    s = s + "<tr>";
                    s = s + "<th scope=\"col\" width=\"10px\" class=\"align-center\">&nbsp;</th>";
                    s = s + "<th scope=\"col\" class=\"align-center\">Fase</th>";
                    s = s + "<th scope=\"col\" class=\"align-center\">Descrizione</th>";
                    s = s + "<th scope=\"col\" class=\"align-center\">Quantità</th>";
                    s = s + "<th scope=\"col\" class=\"align-center\">UDM</th>";
                    s = s + "</tr>";
                    s = s + "</thead>";
                    s = s + "<tbody>";
                    
               }
               
                s = s + " <tr style =\"width :100%\" >";
                
                if (lstIntermediEsplosi.Where(z => z.Intesp_id_padre == map.Intesp_id).Count() > 0)
                {
                    s = s + "<td width=\"10px\">";
                    s = s + "<a href='#' class=\"expandable-open-button\" id=\"" + getFatherId(map.Intesp_id_padre, map.Intesp_id, false)+ "\" rel=\"1\" data-id=\"" + map.Intesp_id + "\"";
                    s = s + "onclick=\"" + "javascript" +":toggleEsplosi(this,'" + getChildId(map.Intesp_id_padre, map.Intesp_id, false) + "');\">+</a>";
                    s = s + "</td>";
                }
                else
                {
                    s = s + "<td width=\"10px\"></td>";
                }
                s = s + "<td>" + map.Intesp_fase_desc + "</td>";
                s = s + "<td>" + map.Intesp_descrizione + "</td>";
                s = s + "<td>" + map.Intesp_propos_qta.ToString() + "</td>";
                s = s + "<td>" + map.Intesp_t_unimis_desc + "</td>";
                s = s + "</tr>";

                livello_old = map.Intesp_livello;
            }

            

            return new HtmlString(s);

        }
  
    }

   

    public class ListaCopiaDaProdottoModel : B16ModelMgr
    {
        public int DataTot { get; set; }
        public IEnumerable<MyProdotto> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }
        public IEnumerable<SelectListItem> EntitiesN { get; set; }
        private int m_prodot_id { set; get; }
        private MyProdotto m_ProdottoMaster { set; get; }
        public int ProdotIdMaster { get { return this.m_prodot_id; } }
        private string m_costoInd = "";
        public string CostoInd { set; get; }

        private void loadSearchSettings(int prodot_id)
        {
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });
            m_prodot_id = prodot_id;
            m_ProdottoMaster = m_le.GetProdotti(m_prodot_id);
            m_costoInd = m_le.GetCostoInd(m_ProdottoMaster);
            EntitiesN = l;
        }

        private IEnumerable<MyProdotto> m_listaProdotti = null;

        public ListaCopiaDaProdottoModel(int prodot_id)
        {
            loadSearchSettings(prodot_id);
        }

        public IEnumerable<MyProdotto> GetElencoProdotti(int utente_id, int profilo_id)
        {
            IEnumerable<MyProdotto> l_listaProdotti = null;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
            if (profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL")
            {
                l_listaProdotti = m_le.GetProdottiPerProfilo(profilo).Where(z => z.Prodot_Reparto_ID == m_ProdottoMaster.Prodot_Reparto_ID).ToList<MyProdotto>();
            }
            else
            {
                l_listaProdotti = m_le.GetProdotti().Where(z => z.Prodot_Reparto_ID == m_ProdottoMaster.Prodot_Reparto_ID).ToList<MyProdotto>();
            }
            m_listaProdotti = l_listaProdotti.Where(z => z.Prodot_ID != m_prodot_id);

            if (SearchDescription != null && SearchDescription.Trim() != "")
            {
                return m_listaProdotti
                           .Where(z => testStringNull(z.Prodot_Desc, SearchDescription)
                               || testStringNull(z.Prodot_Codice, SearchDescription)
                               || testStringNull(z.Prodot_UnitaMisura_descrizione, SearchDescription)
                               || testStringNull(z.Prodot_Reparto_Desc, SearchDescription))
                               ;
            }
            else
                return m_listaProdotti;
        }

        public decimal? GetCostoTotDeliberato(MyProdotto prodotto)
        {
            decimal? ret = null;
            // da m_ProdPos_ID recupero il prodotto "Testata" e capisco in che reparto  mi trovo
            bool flgMatch = false;
            int masterReparto_id = 0;
            if (m_ProdottoMaster.Prodot_Reparto_ID.HasValue)
            {
                masterReparto_id = m_ProdottoMaster.Prodot_Reparto_ID.Value;
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

            // Non e' stata gestita la modifica degli importi //m_le.GetSettings("PRICE_POS") == "1"
            // in quanto si visualizza il prodotto da utilizzare come master.
            // Le posizioni del Master serviranno da copia per il prodotto corrente

            //if (flgMatch)    // se il Gruppo Reparto del prodotto Master coincide con quello del prodotto Corrente 
            if (true)
            {                // metto il solo costo diretto unitario
                ret = prodotto.Prodot_CostoUnitario_Deliberato;
            }
            else
            {
                decimal costoInd = 0;
                // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                if (prodotto.Prodot_PercCostInd != null)
                { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
            }
            if (ret.HasValue)
            {
                ret = decimal.Round(ret.Value, 5, MidpointRounding.AwayFromZero);
            }
            else
            { ret = 0; }
            return ret;
        }

    }

    public class UdMConversionePModel : B16ModelMgr
    {
        private int m_Prodotto_pos_ID { set; get; }
        private MyProdottoPos m_ProdottoPosCorrente { set; get; }
        public MyProdottoPos ProdottoPosCorrente { get { return m_ProdottoPosCorrente; } }
        public UdMConversionePModel(int prodotto_pos_ID)
        {
            m_Prodotto_pos_ID = prodotto_pos_ID;
            m_valorizzato = false;
            m_ProdottoPosCorrente = m_le.GetGenericProdottiPos(m_Prodotto_pos_ID);
            if (m_ProdottoPosCorrente.ProdottoPos_Prodotto_id.HasValue)
            {
                m_valorizzato = true;
                m_ProdottoCorrente = m_le.GetProdotti(m_ProdottoPosCorrente.ProdottoPos_Prodotto_id.Value);
                m_udmProdotto = m_le.GetElencoUDM().Where(z => z.Unimis_Id == m_ProdottoCorrente.Prodot_UnitaMisura_ID).SingleOrDefault();
            }
            if (m_ProdottoPosCorrente.ProdottoPos_UdM_id.HasValue)
            {
                m_udmSelect = m_le.GetElencoUDM().Where(z => z.Unimis_Id == m_ProdottoPosCorrente.ProdottoPos_UdM_id.Value).SingleOrDefault();
            }
        }
        private bool m_valorizzato { set; get; }
        public bool Valorizzato { get { return m_valorizzato; } }
        private MyProdotto m_ProdottoCorrente { set; get; }
        public MyProdotto ProdottoCorrente { get { return m_ProdottoCorrente; } }
        private MyUdM m_udmProdotto { set; get; }
        public MyUdM UDMCorrenteProdotto { get { return m_udmProdotto; } }
        private MyUdM m_udmSelect { set; get; }
        public MyUdM UDMCorrenteSelect { get { return m_udmSelect; } }
        public string GetConversionRatio()
        {
            string ret = "";
            if (m_udmSelect != null)
            {
                if (m_udmProdotto.Unimis_Grudmi_id == m_udmSelect.Unimis_Grudmi_id)
                {
                    MyUdM defMyUdm = m_le.GetElencoUDM().Where(z => z.Unidmi_Default == true && z.Unimis_Grudmi_id == m_udmProdotto.Unimis_Grudmi_id).SingleOrDefault();
                    decimal d = m_udmProdotto.Unimis_Conversione / m_udmSelect.Unimis_Conversione;
                    ret = d.ToString().Replace(".", ",");
                }
            }
            return ret;
        }

    }

    public class ListaProdottoModel : B16ModelMgr
    {

        public IEnumerable<MyProdotto> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; } 

        private bool m_flgSec = false;
        public bool FlagSecondario { get { return m_flgSec; } }

        private int m_valopos_id = 0;
        public int ValPos_ID { get { return m_valopos_id; } }
        
        private IEnumerable<MyProdotto> m_listaProdotto = null;
        public IEnumerable<MyProdotto> ElencoProdotti
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaProdotto.Where(z => testStringNull(z.Prodot_Codice,SearchDescription) 
                                        || testStringNull(z.Prodot_Desc,SearchDescription));
                }
                else
                    return m_listaProdotto;
            }

        }
        
        private void loadSearchSettings()
        {
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l;
        }

        private MyAnalisi m_AnalisiMaster;
        private string m_costoInd = "";
        public string CostoInd { set; get; }
        private MyProdotto m_analisiPosProdotto= null;
        public ListaProdottoModel(int valpos_id)
        {
            m_flgSec = false;
            m_valopos_id = valpos_id;
            m_analisiPosProdotto = m_le.GetAnalisiPosProdotto(valpos_id);
            
            m_listaProdotto = m_le.GetProdotti().Where(z => z.Prodot_T_Stapro_Id != 3 //INVALI : In Validazione
                                                    && z.Prodot_Flg_Bloccato_Magazzino == false //Sospesi
                                                    //Escludo i prodotti esterni con costo 0.
                                                    && ((z.Prodot_Reparto_ID.HasValue) || (!z.Prodot_Reparto_ID.HasValue && z.Prodot_CostoUnitario_Deliberato != 0)));
            loadSearchSettings();
            
            m_AnalisiMaster = m_le.GetAnalisiGenericaDaValPos_ID(m_valopos_id);
            m_costoInd = m_le.GetCostoInd(m_AnalisiMaster);
           //m_costoInd = m_le.GetSettings("COST_IND");
        }
        public ListaProdottoModel(int valpos_id,int sec)
        {
             m_flgSec = (sec==1);
             m_valopos_id = valpos_id;
             m_analisiPosProdotto = m_le.GetAnalisiPosProdotto(valpos_id);
             m_listaProdotto = m_le.GetProdotti().Where(z => z.Prodot_T_Stapro_Id != 3 //INVALI : In Validazione
                                                    && z.Prodot_Flg_Bloccato_Magazzino == false //Sospesi
                                                    //Escludo i prodotti esterni con costo 0.
                                                    && ((z.Prodot_Reparto_ID.HasValue) || (!z.Prodot_Reparto_ID.HasValue && z.Prodot_CostoUnitario_Deliberato != 0)));

                
             loadSearchSettings();
           
             m_AnalisiMaster = m_le.GetAnalisiGenericaDaValPos_ID(m_valopos_id);
             //m_costoInd = m_le.GetSettings("COST_IND");
             m_costoInd = m_le.GetCostoInd(m_AnalisiMaster); 
        }
       
        public decimal? GetCostoTotDeliberato(MyProdotto prodotto)
        {
            decimal? ret = null;
            // da ValPos_ID recupero la analisi "Testata" e capisco che Gruppo / reparto (solo per intermedio) mi trovo
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

            string pricePos_Value = m_le.GetSettings("PRICE_POS");// +
            switch (pricePos_Value)
            {
                case "2":// Costo Industriale
                    ret = getImportoCostoIndustriale(prodotto);
                    break;
                case "1":
                    // sono dentro un'analisi e sto scegliendo una posizione Prodotto
                    // quindi i reparto gruppo non combaciano mai , tuttavia per chiarezza lascio il flagMatch 
                    if (flgMatch)
                    {
                        ret = prodotto.Prodot_CostoUnitario_Deliberato;
                    }
                    else
                    {
                        decimal costoInd = 0;
                        if (prodotto.Prodot_PercCostInd != null)
                        { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                        ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);

                    }
                    break;
                default :
                    //if (flgMatch)    // se il Gruppo Reparto dell'analisiIntermedio Master coincide con quello dell'analisiCorrente 
                    if (true)
                    {                // metto il solo costo diretto unitario
                        ret = prodotto.Prodot_CostoUnitario_Deliberato;
                    }
                    else
                    {
                        decimal costoInd = 0;
                        // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                        if (prodotto.Prodot_PercCostInd != null)
                        { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }

                        ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                    }
                    break;
            }
            /* sostituito da switch sopra
            if (m_le.GetSettings("PRICE_POS") == "1") // OK
            {
                // sono dentro un'analisi e sto scegliendo una posizione Prodotto
                // quindi i reparto gruppo non combaciano mai , tuttavia per chiarezza lascio il flagMatch 
                if (flgMatch)
                {
                    ret = prodotto.Prodot_CostoUnitario_Deliberato;
                }
                else
                {
                    decimal costoInd = 0;
                    if (prodotto.Prodot_PercCostInd != null)
                    { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                    ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                }
            }
            else
            {
                //if (flgMatch)    // se il Gruppo Reparto dell'analisiIntermedio Master coincide con quello dell'analisiCorrente 
                if (true)
                {                // metto il solo costo diretto unitario
                    ret = prodotto.Prodot_CostoUnitario_Deliberato;
                }
                else
                {
                    decimal costoInd = 0;
                    // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                    if (prodotto.Prodot_PercCostInd != null)
                    { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }

                    ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                }
            }
            */
            if (ret.HasValue)
            {
                ret = decimal.Round(ret.Value, 5, MidpointRounding.AwayFromZero);
            }
            else
            { ret = 0; }
            return ret;

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
        public MyProdotto Prodotto { get { return m_analisiPosProdotto; } }
        public List<MyProdottoPos> ProdottoPos { get { return m_le.GetProdottiPos(m_analisiPosProdotto.Prodot_ID); } }
    }
    public class ListaProdottoPModel : B16ModelMgr
    {

        public IEnumerable<MyProdotto> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; }

        private bool m_flgSec = false;
        public bool FlagSecondario { get { return m_flgSec; } }

        private int m_ProdPos_ID = 0;
        public int ProdPos_ID { get { return m_ProdPos_ID; } }

        private IEnumerable<MyProdotto> m_listaProdotto = null;
        public IEnumerable<MyProdotto> ElencoProdotti
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaProdotto.Where(z => testStringNull(z.Prodot_Codice,SearchDescription)
                        || testStringNull(z.Prodot_Desc,SearchDescription));
                }
                else
                    return m_listaProdotto;
            }

        }
       
        private void loadSearchSettings()
        {
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l;
        }

        private MyProdotto m_ProdottoMaster;
        private string m_costoInd = "";
        public string CostoInd { set; get; }

        private MyProdotto m_analisiPosProdotto = null;
        public ListaProdottoPModel(int prodpos_id)
        {
            m_flgSec = false;
            m_ProdPos_ID = prodpos_id;
            m_analisiPosProdotto = m_le.GetAnalisiPosProdottoDaProdotto(m_ProdPos_ID);
            m_listaProdotto = m_le.GetProdotti().Where(z => z.Prodot_T_Stapro_Id != 3 //INVALI : In Validazione
                                                    && z.Prodot_Flg_Bloccato_Magazzino == false //Sospesi
                                                    //Escludo i prodotti esterni con costo 0.
                                                    && ((z.Prodot_Reparto_ID.HasValue) || (!z.Prodot_Reparto_ID.HasValue && z.Prodot_CostoUnitario_Deliberato != 0)));

            loadSearchSettings();
            
            m_ProdottoMaster = m_le.GetProdottoGenericaDaProdPos_ID(prodpos_id);
            //m_costoInd = m_le.GetSettings("COST_IND"); 
            m_costoInd = m_le.GetCostoInd(m_ProdottoMaster);
        }

        public decimal? GetCostoTotDeliberato(MyProdotto prodotto)
        {
            decimal? ret = null;
            // da m_ProdPos_ID recupero il prodotto "Testata" e capisco in che reparto  mi trovo
            bool flgMatch = false;
            int masterReparto_id = 0;
            if (m_ProdottoMaster.Prodot_Reparto_ID.HasValue )
            {
                masterReparto_id = m_ProdottoMaster.Prodot_Reparto_ID.Value;
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

            string pricePos_Value = m_le.GetSettings("PRICE_POS");// +
            switch (pricePos_Value)
            {
                case "2":// Costo Industriale
                    ret = getImportoCostoIndustriale(prodotto);
                    break;
                case "1":
                    if (flgMatch)
                    {
                        ret = prodotto.Prodot_CostoUnitario_Deliberato;
                    }
                    else
                    {
                        decimal costoInd = 0;
                        // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                        if (prodotto.Prodot_PercCostInd != null)
                        { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                        ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                    }
                    break;
                default:
                    {

                        //if (flgMatch)    // se il Gruppo Reparto del prodotto Master coincide con quello del prodotto Corrente 
                        if (true)
                        {                // metto il solo costo diretto unitario
                            ret = prodotto.Prodot_CostoUnitario_Deliberato;
                        }
                        else
                        {
                            decimal costoInd = 0;
                            // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                            if (prodotto.Prodot_PercCostInd != null)
                            { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                            ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                        }
                    }
                    break;
            }
            /* sostituito da switch sopra
            if (m_le.GetSettings("PRICE_POS") == "1") // se abilitata da Settings OK
            {
                if (flgMatch)
                {
                    ret = prodotto.Prodot_CostoUnitario_Deliberato;
                }
                else 
                {
                    decimal costoInd = 0;
                    // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                    if (prodotto.Prodot_PercCostInd != null)
                    { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                    ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                }
            }
            else
            {

                //if (flgMatch)    // se il Gruppo Reparto del prodotto Master coincide con quello del prodotto Corrente 
                if (true)
                {                // metto il solo costo diretto unitario
                    ret = prodotto.Prodot_CostoUnitario_Deliberato;
                }
                else
                {
                    decimal costoInd = 0;
                    // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                    if (prodotto.Prodot_PercCostInd != null)
                    { decimal.TryParse(prodotto.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                    ret = prodotto.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                }
            }
            */
            if (ret.HasValue)
            {
                ret = decimal.Round(ret.Value, 5, MidpointRounding.AwayFromZero);
            }
            else
            { ret = 0; }
            return ret;
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

        public MyProdotto Prodotto { get { return m_analisiPosProdotto; } }
        public List<MyProdottoPos> ProdottoPos { get { return m_le.GetProdottiPos(m_analisiPosProdotto.Prodot_ID); } }
    }
    public class ProdottoModelSblocco : ProdottoModel
    {
        public ProdottoModelSblocco(int prodotto_id)
            : base(prodotto_id)
        { m_listaPrioritaRichiesta = m_le.GetPrioritaRichiesta(); }
        private IEnumerable<PrioritaRichiesta> m_listaPrioritaRichiesta = null;
        public IEnumerable<PrioritaRichiesta> ElencoPrioritaRichiesta { get { return m_listaPrioritaRichiesta; } }
    }


    public class ListaIndexProdottoModel : B16ModelMgr
    {

        private int m_FiltroStato { get; set; }
        public int FiltroStato { get; set; }

        public IEnumerable<MyProdotto> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }
        public IEnumerable<SelectListItem> EntitiesN { get; set; }
        private void loadSearchSettings()
        {
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l;
        }
        private IEnumerable<MyProdotto> m_listaProdotto = null;

        public IEnumerable<MyProdotto> GetProdottiDaValorizzare(int utente_id, int profilo_id)
        {

            IEnumerable<MyProdotto> l_listaProdotti = null;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
            //if (profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL")
            if (profilo.ProfiloCodice == "RESMAG")                 
            {
                l_listaProdotti = m_le.GetProdottiPerProfilo(profilo).Where(z => z.Prodot_T_Stapro_Id == 1).ToList<MyProdotto>();
            }
            else
            {
                l_listaProdotti = m_le.GetProdotti().Where(z => z.Prodot_T_Stapro_Id == 1).ToList<MyProdotto>();
            }

            
            if (SearchDescription != null && SearchDescription.Trim() != "")
            {
                return l_listaProdotti
                           .Where(z => testStringNull(z.Prodot_Codice, SearchDescription)
                               || testStringNull(z.Prodot_Desc, SearchDescription)
                               || testStringNull(z.Prodot_Codice_Desc, SearchDescription)
                               || testStringNull(z.Prodot_Reparto_Desc, SearchDescription)
                               || testStringNull(z.Prodot_utente_denominazione, SearchDescription)
                               );
            }
            else
                return l_listaProdotti;
        }
        public IEnumerable<MyProdotto> GetElencoProdotti(int utente_id, int profilo_id)
        {
             IEnumerable<MyProdotto> l_listaProdotti = null;
             Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
             if (profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL")
             {
                 l_listaProdotti = m_le.GetProdottiPerProfilo(profilo).ToList<MyProdotto>();
             }
             else
             {
                 l_listaProdotti = m_le.GetProdotti().ToList<MyProdotto>();
             }
             l_listaProdotti = l_listaProdotti.Where(z => z.Prodot_T_Stapro_Id != 1 && z.Prodot_Reparto_ID != null);


             if (this.FiltroStato != 0)
             {
                 m_listaProdotto = l_listaProdotti.Where(z => z.Prodot_T_Stapro_Id == this.FiltroStato);
             }
             else
             {
                 m_listaProdotto = l_listaProdotti;
             }
             if (SearchDescription != null && SearchDescription.Trim() != "")
             {
                 return m_listaProdotto
                            .Where(z => testStringNull(z.Prodot_Codice,SearchDescription) 
                                ||testStringNull( z.Prodot_Desc,SearchDescription)
                                || testStringNull(z.Prodot_Codice_Desc, SearchDescription)
                                || testStringNull(z.Prodot_Reparto_Desc, SearchDescription)
                                || testStringNull(z.Prodot_utente_denominazione, SearchDescription)
                                );
             }
             else
                 return m_listaProdotto;

       }

        public ListaIndexProdottoModel()
        {
            //m_listaProdotto = m_le.GetProdotti();
            loadSearchSettings();
        }

    }
    public class ProdottoModel : B16ModelMgrChart, IGetGoogleChart
    {
        public string PathAttach { get { return m_le.GetSettings("PATH_ATTAC"); } }

        public decimal CostoMedioProdSimili = 0;
        private List<MyFaseValorImporto> m_ListaProdSimili = new List<MyFaseValorImporto>();
        private List<MyFaseValorImporto> getProdottiSimili()
        {
            List<MyFaseValorImporto> lstFPI = new List<MyFaseValorImporto>();
            if(m_MyProdotto.Prodot_Reparto_ID.HasValue)
            {
                decimal costoProdSimili = 0;
                List<MyFaseValorImporto> lstProdFase = new List<MyFaseValorImporto>();
                List<MyProdotto > lstProd =  m_le.GetProdotti().Where(z => z.Prodot_Reparto_ID == m_MyProdotto.Prodot_Reparto_ID.Value && z.Prodot_ID != m_MyProdotto.Prodot_ID && z.Prodot_T_Stapro_Id == 6).ToList<MyProdotto>();
                foreach (MyProdotto prod in lstProd) // Elenco Fasi-prodotti appartenenti allo stesso reparto del prodotto selezionato
                {
                    if (prod.Prodot_CostoUnitario.HasValue)
                    {
                        costoProdSimili += prod.Prodot_CostoUnitario.Value;
                    }
                    MyProdFase l= new MyProdFase(prod.Prodot_ID);
                    lstProdFase.AddRange(l.ElencoFasi);
                }
                if (lstProd.Count() > 0)
                {
                    CostoMedioProdSimili = decimal.Round((costoProdSimili / lstProd.Count()), 2, MidpointRounding.AwayFromZero);
                }
                
                // Elenco Fasi-prodotti appartenenti allo stesso reparto del prodotto selezionato ordinati per padre Fase
                Dictionary<int, MyFaseValorImporto> dic = new Dictionary<int, MyFaseValorImporto>();
                foreach (MyFaseValorImporto fase in lstProdFase.OrderBy(z => z.Fase_ID))
                {
                    if (dic.ContainsKey(fase.Fase_ID))
                    {
                        MyFaseValorImporto lfase = dic[fase.Fase_ID];
                        lfase.Fase_Percentuale = lfase.Fase_Percentuale + fase.Fase_Percentuale;
                        dic[fase.Fase_ID] = lfase;
                    }
                    else 
                    {
                        dic.Add(fase.Fase_ID, fase);
                    }
                }
                foreach (int k in dic.Keys)
                {
                    dic[k].Fase_Percentuale = decimal.Round((dic[k].Fase_Percentuale / lstProd.Count()), 2, MidpointRounding.AwayFromZero);   
                }
                foreach (int k in dic.Keys)
                {
                    lstFPI.Add(dic[k]); 
                }

            }
            return lstFPI;
        }
        
        public List<MyGoogleChartDataAjax> GetDataChart()
        {
            this.m_ListaProdSimili = getProdottiSimili();
            List<MyGoogleChartDataAjax> lst = new List<MyGoogleChartDataAjax>();
            string dtsec = String.Format("{0:00}", DateTime.Now.Second); 
            lst.Add(getChartData("Fasi", "Peso % medio della fase per gruppo prodotto", "Peso % della fase per la valorizzazione corrente"));

            List<MyFaseValorImporto> lstPCorrente = new MyProdFase(m_MyProdotto.Prodot_ID).ElencoFasi;
            MyGoogleFaseChart m = new MyGoogleFaseChart(m_ListaProdSimili, lstPCorrente);
            if (m.ResultList.Count() == 0){ lst.Add(getChartData("", "0", "0")); }
            foreach (MyGoogleFaseChartElement c in m.ResultList.OrderBy(z=>z.Fase_ID ))
            {
                lst.Add(getChartData(c.Fase_Desc, c.Fase_Percentuale_1.ToString().Replace(",", "."), c.Fase_Percentuale_2.ToString().Replace(",", ".")));    
            }
            
            return lst;
        }
     
     
        public IEnumerable<MyProdotto> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; }
        private void loadSearchSettings()
        {
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
        
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l;

            if (m_MyProdotto.Prodot_UnitaMisura_ID.HasValue)
            {
                m_udmSelect = m_le.GetElencoUDM().Where(z => z.Unimis_Id == m_MyProdotto.Prodot_UnitaMisura_ID.Value).SingleOrDefault();
                _UdM_id = m_MyProdotto.Prodot_UnitaMisura_ID.Value;
                
                if (m_MyProdotto.Prodot_UnitaMisura_ID_Sec.HasValue)
                {
                    _UdM_id_Sec = m_MyProdotto.Prodot_UnitaMisura_ID_Sec.Value;
                }
            }
            ListaUdMSec = m_le.GetElencoUDM();
            ListaUdM = m_le.GetElencoUDM();
        }
        private MyUdM m_udmSelect { set; get; }
        public MyUdM UDMCorrenteSelect { get { return m_udmSelect; } }
        private  int? _UdM_id { set; get; }
        private int? _UdM_id_Sec { set; get; }
        public List<MyUdM> ListaUdM { set; get; }
        public IEnumerable<SelectListItem> ListaUdMSL
        {
            get
            {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyUdMToSLI(this.ListaUdM, this._UdM_id, true);

            }
        }

        public List<MyUdM> ListaUdMSec { set; get; }
        public IEnumerable<SelectListItem> ListaUdMSLSec
        {
            get
            {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyUdMToSLI(this.ListaUdMSec, this._UdM_id_Sec, true);

            }
        }
        private IEnumerable<MyProdotto> m_listaProdotto = null;
        
        public IEnumerable<MyProdotto> ElencoProdotti
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaProdotto.Where(z => testStringNull(z.Prodot_Codice,SearchDescription) 
                        || testStringNull(z.Prodot_Desc,SearchDescription));
                }
                else
                    return m_listaProdotto;
            }

        }
        public ProdottoModel()
        {
            m_listaProdotto = m_le.GetProdotti();
            loadSearchSettings();
        }

        
        public ProdottoModel(int prodotto_id)
        {
            this.m_listaProdotto = m_le.GetProdotti();
            this.m_MyProdotto = m_le.GetProdotti(prodotto_id);
            this.m_listaProdottoPos = m_le.GetProdottiPos(prodotto_id);
            loadSearchSettings();
     
        }






        private IEnumerable<MyProdottoPos> m_listaProdottoPos = null;

        public IEnumerable<MyProdottoPos> ElencoProdottoPos { get { return m_listaProdottoPos; } }
        
        private MyProdotto m_MyProdotto = null;
        public MyProdotto Prodotto{ get { return m_MyProdotto; } }
        public string CostoIndiretto { get { return m_le.GetCostoInd(m_MyProdotto); /* return m_le.GetSettings("COST_IND"); */} }
        public string CoeffTariffa { get{ return m_le.GetSettings("TARIFFA");}  }
        public List<MyFigProf> GetFigProf(int Attiv_id)
        {
            return m_le.GetFigProf(Attiv_id);
        }
        

    }



    public class ListaIntermediAnalisiPModel : B16ModelMgr
    {

        public IEnumerable<MyAnalisi> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; }

        public IEnumerable<MyAnalisi> ElencoEntita
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaAnalisi.Where(z => testStringNull(z.Analisi_MP_Rev,SearchDescription)
                            || testStringNull(z.Analisi_Descrizione,SearchDescription)
                            || testStringNull(z.Analisi_CodiceGenerico, SearchDescription)
                            );
                }
                else
                    return m_listaAnalisi;
            }

        }
        private void loadSearchSettings()
        {
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l;
        }



        private int m_prodpos_id = 0;
        public int ProdPos_ID { get { return m_prodpos_id; } }
        private IEnumerable<MyAnalisi> m_listaAnalisi = null;

        public IEnumerable<MyAnalisi> filterElencoAnalisi(int utente_id, int profilo_id)
        {
            IEnumerable<MyAnalisi> l_listaAnalisi = null;
            l_listaAnalisi = m_listaAnalisi;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);

            if (!(profilo.ProfiloCodice == "CDG" || profilo.ProfiloCodice == "SUP"))
            {
                l_listaAnalisi = l_listaAnalisi.Where(z => !testStringNull(z.Analisi_MP_Rev, "altro Ente")).ToList<MyAnalisi>();
            }
            m_listaAnalisi = l_listaAnalisi;
            return m_listaAnalisi;

        }
        public bool testStringNullIntermedio(MyAnalisi info, string search)
        {
            if (info == null) return false;
            if (search.ToUpper() == "INTERMEDIO")
            {
                if (info.Analisi_flgIntermedio) return true;
                //return containsNotSensitive(info, search, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public IEnumerable<MyAnalisi> ElencoAnalisi(int utente_id, int profilo_id)
        {
                m_listaAnalisi = filterElencoAnalisi(utente_id, profilo_id);  

                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    //return m_listaAnalisi.Where(z =>
                    //        testStringNull(z.Analisi_MP_Rev,SearchDescription)
                    //    || testStringNull(z.Analisi_VN,SearchDescription)
                    //    || testStringNull(z.Analisi_Descrizione,SearchDescription)
                    //    || testStringNull(z.Analisi_Tecnica ,SearchDescription )
                    //    || testStringNull(z.Analisi_Gruppo_desc,SearchDescription)
                    //    || testStringNull(z.Analisi_CodiceGenerico, SearchDescription)
                    //    || testStringNullIntermedio(z, SearchDescription)
                    //    );

                    //Allineato alla ricerca delle analisi negli altri punti dell'applicativo.
                    //Matteo 26/04/2013
                    return m_listaAnalisi.Where(z =>
                           testStringNull(z.Analisi_Codice_Descrizione, SearchDescription)
                        || testStringNull(z.Analisi_Descrizione, SearchDescription)
                        || testStringNull(z.Analisi_MP_Rev, SearchDescription)
                        || testStringNull(z.Analisi_VN, SearchDescription)
                        || testStringNull(z.Analisi_Tecnica, SearchDescription)
                        || testStringNull(z.Analisi_Gruppo_desc, SearchDescription)
                        || testStringNull(z.Analisi_CodiceGenerico, SearchDescription)
                        || testStringNull(z.Analisi_VN, SearchDescription)
                        || testStringNull(z.Analisi_MP_Rev, SearchDescription)
                        || testStringNullIntermedio(z, SearchDescription));
                }
                else
                    return m_listaAnalisi;
            
        }
      

        private MyProdotto m_ProdottoMaster;
        private string m_costoInd = "";
        public string CostoInd { set; get; }

        private MyAnalisi m_AnalisiPosIntermedio = null;
        public ListaIntermediAnalisiPModel(int prodpos_id)
        {
            string sPricePos = m_le.GetSettings("PRICE_POS");
            bool bPricePos = sPricePos == "1";

            string sTarNomenc = m_le.GetSettings("TAR_NOMENC");
            bool bTarNomenc = sTarNomenc == "1";
 
            m_prodpos_id = prodpos_id;
            m_AnalisiPosIntermedio = m_le.GetAnalisiPosIntermedioDaProdotto(prodpos_id);
            DateTime today = DateTime.Now.Date;

            List<MyAnalisi> lst = new List<MyAnalisi>();
            
            /* Se il price pos vale 1 devo far vedere il settore nella griglia delle analisi. 
             * Per questo filtro le analisi in base al Analisi_CostoTariffaDelib e  Analisi_CostoTariffa_D_Delib
             * per caricare sia quelle del settore A/V e quelle del settore D.
             * 
             * 
             * Se non vale 1.. devo vedere quell'analisi una sola volta!
             */
            if (bPricePos) 
            {
                List<MyAnalisi> lstSettA = new List<MyAnalisi>();
                List<MyAnalisi> lstSettD = new List<MyAnalisi>();

                lstSettA = m_le.GetAnalisi().Where(z =>
                    z.Analisi_flgIntermedio == false
                    && z.Analisi_flgModello == false
                    && z.Analisi_VN_Data_Da <= today
                    && z.Analisi_VN_Data_A >= today
                    && z.Analisi_MP_Rev_Data_Scadenza >= today
                    && z.Analisi_CostoTariffaDelib > 0
                    ).ToList<MyAnalisi>();

                foreach (MyAnalisi an in lstSettA)
                {
                    an.Analisi_SettoreSelezionato = "A";
                }

                lstSettD = m_le.GetAnalisi().Where(z =>
                    z.Analisi_flgIntermedio == false
                    && z.Analisi_flgModello == false
                    && z.Analisi_VN_Data_Da <= today
                    && z.Analisi_VN_Data_A >= today
                    && z.Analisi_MP_Rev_Data_Scadenza >= today
                    && z.Analisi_CostoTariffa_D_Delib > 0
                    ).ToList<MyAnalisi>();

                foreach (MyAnalisi an in lstSettD)
                {
                    an.Analisi_SettoreSelezionato = "D";
                }

                lst.AddRange(lstSettA);
                lst.AddRange(lstSettD);
                
            }
            else
            {
                List<MyAnalisi> lstAll = m_le.GetAnalisi().Where(z =>
                    z.Analisi_flgIntermedio == false
                    && z.Analisi_flgModello == false
                    && z.Analisi_VN_Data_Da <= today
                    && z.Analisi_VN_Data_A >= today
                    && z.Analisi_MP_Rev_Data_Scadenza >= today
                    ).ToList<MyAnalisi>();

                lst.AddRange(lstAll);
            }

            /* Se il TarNomenc vale 1 non va applicato il filtro sullo stato!*/
            if (!bTarNomenc)
            {
                lst = lst.Where(z => !isDaValorizzare(z)).ToList<MyAnalisi>();
            }

            lst.AddRange ( m_le.GetIntermediDaProdottoPosizione(prodpos_id));
            m_listaAnalisi = lst;
            loadSearchSettings();
            
            m_ProdottoMaster = m_le.GetProdottoGenericaDaProdPos_ID(prodpos_id);
            //m_costoInd = m_le.GetSettings("COST_IND"); 
            m_costoInd = m_le.GetCostoInd(m_ProdottoMaster);
            
        }

        private bool isDaValorizzare(MyAnalisi z)
        {
            bool lret = false;
            if (z.Analisi_T_Staval_id == 1) lret = true;
            return lret;
        }
        private bool isDaValorizzare(int id)
        {
            bool lret = false;
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            VALORI_VALORIZZAZIONI val = new VALORI_VALORIZZAZIONI();

            val = en.VALORI_VALORIZZAZIONI.Include("T_STAVAL_STATO_VALORIZZAZIONE").Where(x => x.VALORI_ID == id).SingleOrDefault();
            if (val.T_STAVAL_STATO_VALORIZZAZIONE.T_STAVAL_CODICE == "DAVAL")
            {
                lret = true;
            }
            return lret;
        }

        public string GetPricePos()
        {
            string pricePos_Value = m_le.GetSettings("PRICE_POS");
            return pricePos_Value;
        }
        
        public decimal? GetCostoTotDeliberato(MyAnalisi analisi)
        {
            decimal? ret = null;
            // da m_prodpos_id recupero il Prodotto "Testata" e capisco reparto (solo per intermedio) mi trovo
            bool flgMatch = false;
            int masterReparto_id =0;
            if (m_ProdottoMaster.Prodot_Reparto_ID.HasValue)
            {
                if (m_ProdottoMaster.Prodot_Reparto_ID.HasValue) // 
                {
                    masterReparto_id = m_ProdottoMaster.Prodot_Reparto_ID.Value;
                }
            }
            if (masterReparto_id > 0) //controllo il reparto dell' "analisi" parametro
            {
                if (analisi.Analisi_Reparto_id.HasValue)
                {
                    if (analisi.Analisi_Reparto_id.Value == masterReparto_id)
                    {
                        flgMatch = true;
                    }
                }
            }

            string pricePos_Value = m_le.GetSettings("PRICE_POS"); // +
            switch (pricePos_Value)
            {
                case "2":// Costo Industriale
                    ret = getImportoCostoIndustriale(analisi);
                    break;
                case "1":
                    //ret = analisi.Analisi_CostoTariffaDelib ; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro al prodotto

                    if (analisi.Analisi_flgIntermedio == true)
                    {
                        ret = analisi.Analisi_CostoTotDelib;
                    }
                    else
                    {
                        if (analisi.Analisi_SettoreSelezionato == "D")
                            ret = analisi.Analisi_CostoTariffa_D_Delib;
                        else
                            ret = analisi.Analisi_CostoTariffaDelib;
                    }
                    break;
                default:
                    {
                        //if (flgMatch)    // se il Gruppo Reparto dell'analisiIntermedio Master coincide con quello dell'analisiCorrente 
                        if (true)
                        {                // metto il solo costo diretto unitario
                            ret = analisi.Analisi_CostoTotDelib;
                        }
                        else
                        {
                            decimal costoInd = 0;
                            if (analisi.Analisi_PercCostInd != null)
                            {
                                decimal.TryParse(analisi.Analisi_PercCostInd.Replace(".", ","), out costoInd);
                            }
                            //decimal.TryParse(m_costoInd.Replace(".",",") , out costoInd);
                            ret = analisi.Analisi_CostoTotDelib * (1 + costoInd);
                        }
                    }
                    break;
            }
            /* sostituito da switch sopra
            if (m_le.GetSettings("PRICE_POS") == "1") // se abilitata da Settings OK
            {
                ret = analisi.Analisi_CostoTariffaDelib ; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro al prodotto
            }
            else
            {

                //if (flgMatch)    // se il Gruppo Reparto dell'analisiIntermedio Master coincide con quello dell'analisiCorrente 
                if (true)
                {                // metto il solo costo diretto unitario
                    ret = analisi.Analisi_CostoTotDelib;
                }
                else
                {
                    decimal costoInd = 0;
                    if (analisi.Analisi_PercCostInd != null)
                    {
                        decimal.TryParse(analisi.Analisi_PercCostInd.Replace(".", ","), out costoInd);
                    }
                    //decimal.TryParse(m_costoInd.Replace(".",",") , out costoInd);
                    ret = analisi.Analisi_CostoTotDelib * (1 + costoInd);
                }
            }
            */
            if (ret.HasValue)
            {
                ret = decimal.Round(ret.Value, 5, MidpointRounding.AwayFromZero);
            }
            else
                ret = 0;
            
            return ret;

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

        public MyAnalisi IntermedioAnalisi { get { return m_AnalisiPosIntermedio; } }
        public List<MyAnalisiPos> IntermedioAnalisiPos { get { return m_le.GetAnalisiPos(m_AnalisiPosIntermedio.Analisi_id); } }
        public List<MyAnalisiPos> IntermedioAnalisiPosSec { get { return m_le.GetAnalisiPosSec(m_AnalisiPosIntermedio.Analisi_id); } }

    }
    public class ListaMacchinariPModel : B16ModelMgr
    {

        public IEnumerable<MyMacchinario> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; }


        private void loadSearchSettings()
        {
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l;
        }



        private int m_prodpos_id = 0;
        public int ProdPos_ID { get { return m_prodpos_id; } }
        private IEnumerable<MyMacchinario> m_listaMacchinari = null;

        public IEnumerable<MyMacchinario> ElencoMacchinari
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaMacchinari.Where(z =>  testStringNull(z.Macchi_Codice,SearchDescription));
                }
                else
                    return m_listaMacchinari;
            }

        }
        private string m_costoInd = "";
        public string CostoInd { set; get; }
        private MyMacchinario m_analisiPosMacchinario = null;

        public ListaMacchinariPModel(int prodpos_id)
        {
            m_prodpos_id = prodpos_id;
            m_analisiPosMacchinario = m_le.GetAnalisiPosMacchinarioDaProdotto(prodpos_id);
            List<MyMacchinario> lst = m_le.GetElencoMacchinari()
                //.Where(z => !isDaValorizzare(z.Analisi_id) && z.Analisi_flgIntermedio == false)
                .ToList<MyMacchinario>();
            //lst.AddRange(m_le.GetIntermediDaAnalisiPosizione(valpos_id));
            m_listaMacchinari = lst;
            loadSearchSettings();

            m_ProdottoMaster = m_le.GetProdottoGenericaDaProdPos_ID(prodpos_id);
            m_costoInd = m_le.GetCostoInd(m_ProdottoMaster);
        }
        private MyProdotto m_ProdottoMaster;

        public MyMacchinario Macchinario { get { return m_analisiPosMacchinario; } }

        public IEnumerable<MyMacchinario> GetElencoMacchinari(int utente_id, int profilo_id)
        {
            IEnumerable<MyMacchinario> l_listaMacchinari = null;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
            //  DateTime today = DateTime.Now.Date; 
            if (profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL")
            {
                l_listaMacchinari = m_le.GetElencoMacchinari(profilo).ToList<MyMacchinario>();
            }
            else
            {
                l_listaMacchinari = m_le.GetElencoMacchinari().ToList<MyMacchinario>();
            }

            m_listaMacchinari = l_listaMacchinari;


            if (SearchDescription != null && SearchDescription.Trim() != "")
            {
                return m_listaMacchinari.Where(z => testStringNull(z.Macchi_Codice, SearchDescription)
                        || testStringNull(z.Macchi_Desc, SearchDescription));
            }
            else
                return m_listaMacchinari;

        }

    }
}