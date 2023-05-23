using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;

//simone
//using System.IO;

namespace IZSLER_CAP.Models
{

    public class StampaIntermediAnalisiModel : B16ModelMgr
    {
        private MyAnalisi m_MyAnalisi = null;
        public MyAnalisi Analisi { get { return m_MyAnalisi; } }
        
        private IEnumerable<MyIntermediEsplosiAnalisi> m_listaIntermediEsplosi = null;

        public IEnumerable<MyIntermediEsplosiAnalisi> ElencoIntermediEsplosi { get { return m_listaIntermediEsplosi; } }

        public StampaIntermediAnalisiModel()
        {
            

        }

        public StampaIntermediAnalisiModel(int analisi_id)
        {
            m_MyAnalisi = m_le.GetAnalisi(analisi_id);
            m_listaIntermediEsplosi = m_le.GetIntermediEsplosiAnalisi_stampa(analisi_id);

        }

        public string getFatherId(string idParent, string id, bool? sec)
        {
            string sChild = getChildId(idParent, id, sec);
            string lret = "expandable_open_button" + sChild;
            return lret;
        }

        public string getChildId(string idParent, string id, bool? sec)
        {
            string sSec = "";
            string lret = string.Empty;
            if (sec == true)
            {
                sSec = "s";
            }
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

        public string getChildId2(string idParent, bool? sec)
        {
            string sSec = "";
            string lret = string.Empty;
            if (sec == true)
            {
                sSec = "s";
            }
            List<MyIntermediEsplosiAnalisi> a = m_listaIntermediEsplosi.Where(z => z.Intesp_id == idParent).ToList<MyIntermediEsplosiAnalisi>();
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

            List<MyIntermediEsplosiAnalisi> lstIntermediEsplosi = ElencoIntermediEsplosi.ToList<MyIntermediEsplosiAnalisi>();

            int? livello_old = 1;
            bool? secondaria_old = false;

            foreach (MyIntermediEsplosiAnalisi map in lstIntermediEsplosi)
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
                    s = s + "<tr style =\"width :100%;display:none;\" id=\"" + getChildId2(map.Intesp_id_padre, map.Intesp_secondaria) + "\">";
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

                if (map.Intesp_secondaria != secondaria_old)
                {
                    s = s + "</tbody>";
                    s = s + "</table>";
                    //s = s + "</td>";
                    //s = s + "</tr>";

                    s = s + "<h2>&nbsp;</h2>";
                    s = s + "<table class=\"table responsive-table\" id=\"idTable\">";
                    s = s + "<thead>";
                    s = s + "<tr>";
                    s = s + "<th scope=\"col\" width=\"10px\" class=\"align-center\">&nbsp;</th>";
                    s = s + "<th scope=\"col\" class=\"align-center\">Fase</th>";
                    s = s + "<th scope=\"col\" class=\"align-center\">Descrizione</th>";
                    s = s + "<th scope=\"col\" class=\"align-center \">Quantità</th>";
                    s = s + "<th scope=\"col\" class=\"align-center\">UDM</th>";
                    s = s + "</tr>";
                    s = s + "</thead>";
                    s = s + "<tbody>";
                }

                s = s + " <tr style =\"width :100%\" >";

                if (lstIntermediEsplosi.Where(z => z.Intesp_id_padre == map.Intesp_id).Count() > 0)
                {
                    s = s + "<td width=\"10px\">";
                    s = s + "<a href='#' class=\"expandable-open-button\" id=\"" + getFatherId(map.Intesp_id_padre, map.Intesp_id, map.Intesp_secondaria) + "\" rel=\"1\" data-id=\"" + map.Intesp_id + "\"";
                    s = s + "onclick=\"" + "javascript" + ":toggleEsplosi(this,'" + getChildId(map.Intesp_id_padre, map.Intesp_id, map.Intesp_secondaria) + "');\">+</a>";
                    s = s + "</td>";
                }
                else
                {
                    s = s + "<td width=\"10px\"></td>";
                }
                s = s + "<td>" + map.Intesp_fase_desc + "</td>";
                s = s + "<td>" + map.Intesp_descrizione + "</td>";
                s = s + "<td>" + map.Intesp_valpos_qta.ToString() + "</td>";
                s = s + "<td>" + map.Intesp_t_unimis_desc + "</td>";
                s = s + "</tr>";

                livello_old = map.Intesp_livello;
                secondaria_old = map.Intesp_secondaria;
            }



            return new HtmlString(s);

        }

    }

    public class ListaCopiaDaValorizzazioneModel : B16ModelMgr
    {
        public int DataTot { get; set; }
        public IEnumerable<MyAnalisi> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }
        public IEnumerable<SelectListItem> EntitiesN { get; set; }
        private int m_valori_id { set; get; }
        private MyAnalisi m_AnalisiMaster { set; get; }
        public int ValoriIdMaster { get { return this.m_valori_id; } }
        private string m_costoInd = "";
        public string CostoInd { set; get; }

        private void loadSearchSettings(int valori_id)
        {
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });
            m_valori_id = valori_id;
            m_AnalisiMaster = m_le.GetAnalisi(m_valori_id);
            m_costoInd = m_le.GetCostoInd(m_AnalisiMaster);
            EntitiesN = l;
        }

        private IEnumerable<MyAnalisi> m_listaValorizzazioni= null;

        public ListaCopiaDaValorizzazioneModel(int valori_id)
        {
            loadSearchSettings(valori_id);
        }
        public IEnumerable<MyAnalisi> GetElencoValorizzazioni(int utente_id, int profilo_id)
        {
            IEnumerable<MyAnalisi> l_listaAnalisi = null;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
            if (profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL")
            {
                l_listaAnalisi = m_le.GetAnalisiPerProfilo(profilo).Where(z => z.Analisi_flgModello == false && z.Analisi_flgIntermedio == false && z.Analisi_Gruppo_id == m_AnalisiMaster.Analisi_Gruppo_id ).ToList<MyAnalisi>();
            }
            else
            {
                l_listaAnalisi = m_le.GetAnalisi().Where(z => z.Analisi_flgModello == false && z.Analisi_flgIntermedio == false && z.Analisi_Gruppo_id == m_AnalisiMaster.Analisi_Gruppo_id).ToList<MyAnalisi>();
            }
            m_listaValorizzazioni = l_listaAnalisi.Where(z=> z.Analisi_id != m_valori_id  );
            
            if (SearchDescription != null && SearchDescription.Trim() != "")
            {
                return m_listaValorizzazioni
                           .Where(z => testStringNull(z.Analisi_CodiceGenerico, SearchDescription)
                               || testStringNull(z.Analisi_Descrizione, SearchDescription)
                               || testStringNull(z.Analisi_Tecnica , SearchDescription)
                               || testStringNull(z.Analisi_GruppoRepartoGenerico_Desc, SearchDescription))
                               ;
            }
            else
                return m_listaValorizzazioni;
        }
        
        public decimal? GetCostoTotDeliberato(MyAnalisi analisi)
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
                if (analisi.Analisi_Gruppo_id.HasValue)
                {
                    if (analisi.Analisi_Gruppo_id.Value == masterGruppo_id)
                    {
                        flgMatch = true;
                    }
                }
            }
            if (masterReparto_id > 0) //l'intermedio Master e' un prodotto controllo il reparto dell' "analisi" parametro
            {
                if (analisi.Analisi_Reparto_id.HasValue)
                {
                    if (analisi.Analisi_Reparto_id.Value == masterReparto_id)
                    {
                        flgMatch = true;
                    }
                }
            }
            
            // Non e' stata gestita la modifica degli importi //m_le.GetSettings("PRICE_POS") == "1"
            // in quanto si visualizza l'analisi da utilizzare come master.
            // Le posizioni del Master serviranno da copia per la analisi corrente

            //if (flgMatch)    // se il Gruppo Reparto dell'analisiIntermedio Master coincide con quello dell'analisiCorrente 
            if (true)
            {                // metto il solo costo diretto unitario
                ret = analisi.Analisi_CostoTotDelib;
            }
            else
            {
                decimal costoInd = 0;
                decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                ret = analisi.Analisi_CostoTotDelib * (1 + costoInd);
            }
            
            if (ret.HasValue)
            {
                ret = decimal.Round(ret.Value, 5, MidpointRounding.AwayFromZero);
            }
            else 
                ret = 0;
            return ret;

        }
        
    }

    public class UdMConversioneModel : B16ModelMgr
    {
        private int m_Analisi_pos_ID { set; get; }
        private bool m_flagSec { set; get; }
        public bool FlagSec { get { return m_flagSec; } }
        private MyAnalisiPos m_AnalisiPosCorrente { set; get; }
        public MyAnalisiPos AnalisiPosCorrente { get { return m_AnalisiPosCorrente; } }
        public UdMConversioneModel(int analisi_pos_ID,int sec)
        {
            m_flagSec = false;
            if(sec==1)
                m_flagSec=true;
            m_Analisi_pos_ID = analisi_pos_ID;
            m_valorizzato = false;
            m_AnalisiPosCorrente = m_le.GetGenericAnalisiPos(m_Analisi_pos_ID);
            if (m_AnalisiPosCorrente.AnalisiPos_Prodotto_id.HasValue)
            {
                m_valorizzato = true;
                m_ProdottoCorrente = m_le.GetProdotti(m_AnalisiPosCorrente.AnalisiPos_Prodotto_id.Value);
                m_udmProdotto = m_le.GetElencoUDM().Where(z => z.Unimis_Id == m_ProdottoCorrente.Prodot_UnitaMisura_ID).SingleOrDefault();
            }
            if (m_AnalisiPosCorrente.AnalisiPos_UdM_id.HasValue)
            {
                m_udmSelect = m_le.GetElencoUDM().Where(z => z.Unimis_Id == m_AnalisiPosCorrente.AnalisiPos_UdM_id.Value).SingleOrDefault();
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

    public class ListaIntermediAnalisiModel : B16ModelMgr
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
                            || testStringNull(z.Analisi_CodiceGenerico,SearchDescription)
                            || testStringNull(z.Analisi_Tecnica, SearchDescription)

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
            List<SelectListItem > l = new List<SelectListItem> ();
            l.Add ( new SelectListItem{Value = "10",Text = "10",Selected =true});
            l.Add ( new SelectListItem{Value = "25",Text = "25"});
            l.Add ( new SelectListItem{Value = "50",Text = "50"});
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l; 
        }



        private bool m_flgSec = false;
        private int m_valopos_id = 0;
        public bool FlagSecondario { get { return m_flgSec; } }
        public int ValPos_ID{get{return m_valopos_id ;}}
        private IEnumerable<MyAnalisi> m_listaAnalisi = null;
    

        /*filtra per altro Ente*/
        public IEnumerable<MyAnalisi> filterElencoAnalisi(int utente_id, int profilo_id)
        {
            IEnumerable<MyAnalisi> l_listaAnalisi = null;
            l_listaAnalisi = m_listaAnalisi;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
 
            if (!(profilo.ProfiloCodice == "CDG" || profilo.ProfiloCodice == "SUP"))
            {
                l_listaAnalisi = l_listaAnalisi.Where(z =>!testStringNull(z.Analisi_MP_Rev, "altro Ente")).ToList<MyAnalisi>();
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
                    return m_listaAnalisi.Where(z => 
                           testStringNull(z.Analisi_Codice_Descrizione, SearchDescription)
                        || testStringNull(z.Analisi_Descrizione, SearchDescription) 
                        || testStringNull(z.Analisi_MP_Rev, SearchDescription)
                        || testStringNull(z.Analisi_VN,SearchDescription) 
                        || testStringNull(z.Analisi_Tecnica,SearchDescription)
                        || testStringNull(z.Analisi_Gruppo_desc,SearchDescription)
                        || testStringNull(z.Analisi_CodiceGenerico, SearchDescription)
                        || testStringNull(z.Analisi_VN, SearchDescription)
                        || testStringNull(z.Analisi_MP_Rev, SearchDescription)
                        || testStringNullIntermedio(z,SearchDescription)


                        );
                }
                else
                    return m_listaAnalisi;
            

        }
        private string m_costoInd="";
        public string CostoInd { set; get; }
        private MyAnalisi m_analisiPosIntermezzo=null;
        public ListaIntermediAnalisiModel(int valpos_id)
        {
            m_flgSec = false;
            m_valopos_id = valpos_id;
            m_analisiPosIntermezzo = m_le.GetAnalisiPosIntermezzo(valpos_id);
            DateTime today = DateTime.Now.Date; 
            List<MyAnalisi>lst = m_le.GetAnalisi().Where(z => 
                    !isDaValorizzare(z) 
                    && z.Analisi_flgIntermedio == false 
                    && z.Analisi_flgModello == false
                    && z.Analisi_VN_Data_Da <= today
                    && z.Analisi_VN_Data_A >= today
                    && z.Analisi_MP_Rev_Data_Scadenza >= today
                    ).ToList<MyAnalisi>();
            lst.AddRange(m_le.GetIntermediDaAnalisiPosizione(valpos_id));
            m_listaAnalisi = lst;
            loadSearchSettings();
            
            m_AnalisiMaster = m_le.GetAnalisiGenericaDaValPos_ID(m_valopos_id);
            //m_costoInd = m_le.GetSettings("COST_IND");
            m_costoInd = m_le.GetCostoInd(m_AnalisiMaster);
        }
    
        public ListaIntermediAnalisiModel(int valpos_id,int sec)
        {
            string sPricePos = m_le.GetSettings("PRICE_POS");
            bool bPricePos = sPricePos == "1";

            string sTarNomenc = m_le.GetSettings("TAR_NOMENC");
            bool bTarNomenc = sTarNomenc == "1";

            m_flgSec = (sec==1);
            m_valopos_id = valpos_id;
            m_analisiPosIntermezzo = m_le.GetAnalisiPosIntermezzo( valpos_id );
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

            lst.AddRange(m_le.GetIntermediDaAnalisiPosizione(valpos_id));
            m_listaAnalisi = lst;
            loadSearchSettings();
            
            m_AnalisiMaster = m_le.GetAnalisiGenericaDaValPos_ID(m_valopos_id);
            //m_costoInd = m_le.GetSettings("COST_IND");
            m_costoInd = m_le.GetCostoInd(m_AnalisiMaster);
        }
        private MyAnalisi m_AnalisiMaster;

        public string GetPricePos()
        {
            string pricePos_Value = m_le.GetSettings("PRICE_POS");
            return pricePos_Value;
        }

        public decimal? GetCostoTotDeliberato(MyAnalisi analisi)
        {
            decimal? ret = null;
            // da ValPos_ID recupero la analisi "Testata" e capisco che Gruppo / reparto (solo per intermedio) mi trovo
            bool flgMatch = false;
            int masterGruppo_id =0;
            int masterReparto_id =0;
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
                if (analisi.Analisi_Gruppo_id.HasValue)
                {
                    if (analisi.Analisi_Gruppo_id.Value == masterGruppo_id)
                    {
                        flgMatch = true;
                    }
                }
            }
            if (masterReparto_id > 0) //l'intermedio Master e' un prodotto controllo il reparto dell' "analisi" parametro
            {
                if (analisi.Analisi_Reparto_id.HasValue)
                {
                    if (analisi.Analisi_Reparto_id.Value == masterReparto_id)
                    {
                        flgMatch = true;
                    }
                }
            }

            string pricePos_Value = m_le.GetSettings("PRICE_POS");// +
            switch (pricePos_Value)
            {
                case "2":// Costo Industriale
                    ret = getImportoCostoIndustriale(analisi);
                    break;
                case "1":
                    //ret = analisi.Analisi_CostoTariffaDelib; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro all'analisi

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
                    break;
            }
            /* sostituito da switch sopra
            if (m_le.GetSettings("PRICE_POS") == "1") // se abilitata da Settings OK
            {
                ret = analisi.Analisi_CostoTariffaDelib; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro all'analisi
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

        public MyAnalisi IntermedioAnalisi { get { return m_analisiPosIntermezzo; } }

        public List<MyAnalisiPos> IntermedioAnalisiPos { get { return m_le.GetAnalisiPos(m_analisiPosIntermezzo.Analisi_id); } }
        public List<MyAnalisiPos> IntermedioAnalisiPosSec { get { return m_le.GetAnalisiPosSec(m_analisiPosIntermezzo.Analisi_id); } }

    }
    public class ListaAnalisiModel : B16ModelMgr
    {
        private int m_FiltroStato { get; set; }
        public int FiltroStato { get; set; }
        public int FiltroStatoObsoleta { get; set; }
        private int m_FiltroStatoObsoleta { get; set; }
        public string PathAttach { get { return m_le.GetSettings("PATH_ATTAC"); } }
        public IEnumerable<MyAnalisi> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }
        public IEnumerable<SelectListItem> EntitiesN { get; set; }
        private void loadSearchSettings()
        {
            m_FiltroStatoObsoleta = 0;
            m_FiltroStato = 0; 
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


        private IEnumerable<MyAnalisi> m_listaAnalisi = null;
       // public IEnumerable<MyAnalisi> ElencoAnalisi { get { return m_listaAnalisi; } }
        public ListaAnalisiModel()
        {
            loadSearchSettings();
          //  m_listaAnalisi = m_le.GetAnalisi().Where(z => z.Analisi_flgIntermedio == false && z.Analisi_flgModello == false ); 
        }
        public List<MyAnalisi> GetAnalisiDaValorizzare(int utente_id, int profilo_id)
        {

            IEnumerable<MyAnalisi> l_listaAnalisi = null;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
            //if(profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL" 
           // DateTime today = DateTime.Now.Date; 
            if (profilo.ProfiloCodice == "RESNK")
            {
                l_listaAnalisi = m_le.GetAnalisiPerProfilo(profilo).Where(z => 
                    z.Analisi_flgIntermedio == false 
                    && z.Analisi_flgModello == false 
                    && isDaValorizzare(z)
                    && z.Analisi_flgObsoleta == false
                    //&& z.Analisi_VN_Data_Da <= today
                    //&& z.Analisi_VN_Data_A >= today
                    //&& z.Analisi_MP_Rev_Data_Scadenza >= today
                    ).ToList<MyAnalisi>();
            }
            else
            {
                l_listaAnalisi = m_le.GetAnalisi().Where(z => 
                    z.Analisi_flgIntermedio == false 
                    && z.Analisi_flgModello == false 
                    && isDaValorizzare(z)
                    && z.Analisi_flgObsoleta == false
                    //&& z.Analisi_VN_Data_Da <= today
                    //&& z.Analisi_VN_Data_A >= today
                    //&& z.Analisi_MP_Rev_Data_Scadenza >= today
                    ).ToList<MyAnalisi>();
            }
            
            if (SearchDescription != null && SearchDescription.Trim() != "")
            {
               l_listaAnalisi= l_listaAnalisi
                        .Where(z => testStringNull(z.Analisi_Codice_Descrizione, SearchDescription) 
                        || testStringNull(z.Analisi_Descrizione,SearchDescription) 
                        || testStringNull(z.Analisi_utente_des_cognome,SearchDescription)
                        || testStringNull(z.Analisi_Tecnica ,SearchDescription )
                        || testStringNull(z.Analisi_GruppoRepartoGenerico_Desc ,SearchDescription )
                        || testStringNull(z.Analisi_Gruppo_desc , SearchDescription)
                        || testStringNull(z.Analisi_VN , SearchDescription)
                        || testStringNull(z.Analisi_MP_Rev , SearchDescription)
                               );
            }
           // else                return l_listaAnalisi;

            List<MyAnalisi> lstOut = new List<MyAnalisi>();
            lstOut.AddRange(l_listaAnalisi);
            return lstOut;
        }


        public IEnumerable<MyAnalisi> GetElencoAnalisi(int utente_id, int profilo_id)
        { 
            IEnumerable<MyAnalisi> l_listaAnalisi = null;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
          //  DateTime today = DateTime.Now.Date; 
            if (profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL")
            {
                l_listaAnalisi = m_le.GetAnalisiPerProfilo(profilo).Where(z =>
                    z.Analisi_flgIntermedio==false 
                    &&  z.Analisi_flgModello == false 
                    && !isDaValorizzare(z)
                    && !testStringNull(z.Analisi_MP_Rev,"altro Ente")// AltroEnte
                    //&& z.Analisi_VN_Data_Da <= today
                    //&& z.Analisi_VN_Data_A >= today
                    //&& z.Analisi_MP_Rev_Data_Scadenza >= today
                    ).ToList<MyAnalisi>();
            }
            else
            {
                if (profilo.ProfiloCodice == "RESNK" || profilo.ProfiloCodice == "RESMAG" || profilo.ProfiloCodice == "MON")
                {
                    l_listaAnalisi = m_le.GetAnalisi().Where(z =>
                                    z.Analisi_flgIntermedio == false
                                    && z.Analisi_flgModello == false
                                    && !isDaValorizzare(z)
                                    && !testStringNull(z.Analisi_MP_Rev, "altro Ente")// AltroEnte
                        //&& z.Analisi_VN_Data_Da <= today
                        //&& z.Analisi_VN_Data_A >= today
                        //&& z.Analisi_MP_Rev_Data_Scadenza >= today
                                    ).ToList<MyAnalisi>();
                }
                else 
                {
                    l_listaAnalisi = m_le.GetAnalisi().Where(z =>
                    z.Analisi_flgIntermedio == false
                    && z.Analisi_flgModello == false
                    && !isDaValorizzare(z)
                                //&& z.Analisi_VN_Data_Da <= today
                                //&& z.Analisi_VN_Data_A >= today
                                //&& z.Analisi_MP_Rev_Data_Scadenza >= today
                    ).ToList<MyAnalisi>();

                }
            }

            if (this.FiltroStatoObsoleta != 0)
            {
                if(this.FiltroStatoObsoleta ==1)
                    l_listaAnalisi = l_listaAnalisi.Where(z => z.Analisi_flgObsoleta == false);
                if(this.FiltroStatoObsoleta ==2)
                    l_listaAnalisi = l_listaAnalisi.Where(z => z.Analisi_flgObsoleta == true);
            }
            
            if (this.FiltroStato != 0)
            {
                l_listaAnalisi = l_listaAnalisi.Where(z => z.Analisi_T_Staval_id == this.FiltroStato);
            }


            m_listaAnalisi = l_listaAnalisi;
            

            if (SearchDescription != null && SearchDescription.Trim() != "")
            {
                return m_listaAnalisi
                    .Where(z => testStringNull(z.Analisi_Codice_Descrizione, SearchDescription) 
                        || testStringNull(z.Analisi_Descrizione,SearchDescription) 
                        || testStringNull(z.Analisi_utente_des_cognome,SearchDescription)
                        || testStringNull(z.Analisi_Tecnica ,SearchDescription )
                        || testStringNull(z.Analisi_GruppoRepartoGenerico_Desc ,SearchDescription )
                        || testStringNull(z.Analisi_Gruppo_desc , SearchDescription)
                        || testStringNull(z.Analisi_VN , SearchDescription)
                        || testStringNull(z.Analisi_MP_Rev , SearchDescription)
                        );
            }
            else
                return m_listaAnalisi;

        }
        private bool isDaValorizzare(MyAnalisi z)
        {
            bool lret = false;
            if (z.Analisi_T_Staval_id == 1) lret = true;
            return lret;
        }
        //public bool IsDaValorizzare(int id) { return isDaValorizzare(id); }
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
    }



    public class AnalisiModelSblocco : AnalisiModel 
    {
        public AnalisiModelSblocco(int analisi_id)
            : base(analisi_id)
        { m_listaPrioritaRichiesta = m_le.GetPrioritaRichiesta(); }
        private IEnumerable<PrioritaRichiesta> m_listaPrioritaRichiesta = null;
        public IEnumerable<PrioritaRichiesta> ElencoPrioritaRichiesta { get { return m_listaPrioritaRichiesta; } }
    }

    //sim
    public class AnalisiModel_IntermediEsplosiDetail : B16ModelMgr
    {
        //sim
        private IEnumerable<MyAnalisiPos_IntermediEsplosiDetail> m_listaAnalisiPos_IntermediEsplosiDetail = null;
        //sim
        private IEnumerable<MyAnalisiPos_IntermediEsplosiDetail> m_listaAnalisiPosSec_IntermediEsplosiDetail = null;
        //sim
        public IEnumerable<MyAnalisiPos_IntermediEsplosiDetail> ElencoAnalisiPos_IntermediEsplosiDetail { get { return m_listaAnalisiPos_IntermediEsplosiDetail; } }
        //sim
        public IEnumerable<MyAnalisiPos_IntermediEsplosiDetail> ElencoAnalisiPosSec_IntermediEsplosiDetail { get { return m_listaAnalisiPosSec_IntermediEsplosiDetail; } }

        private MyAnalisi m_MyAnalisi = null;
        public MyAnalisi Analisi { get { return m_MyAnalisi; } }

        //sim
        public AnalisiModel_IntermediEsplosiDetail(int analisi_id)
        {
            /*
            StreamWriter sw = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga = "pre_query_Model" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            riga = riga + " ID: " + analisi_id.ToString();
            sw.WriteLine(riga);
            sw.Close();
            */
            m_MyAnalisi = m_le.GetAnalisi(analisi_id);
            m_listaAnalisiPos_IntermediEsplosiDetail = m_le.GetAnalisiPos_IntermediEsplosiDetail(analisi_id);
            /*
            StreamWriter sw_post = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga_post = "post_query_Model" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            riga_post = riga_post + " ID: " + analisi_id.ToString();
            sw_post.WriteLine(riga_post);
            sw_post.Close();
            */
            //return m_listaAnalisiPos_IntermediEsplosiDetail;

            
            //sim
            /*
            StreamWriter sw_sec = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga_sec = "pre_query_Model_Sec" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            sw_sec.WriteLine(riga_sec);
            sw_sec.Close();
            */
            m_listaAnalisiPosSec_IntermediEsplosiDetail = m_le.GetAnalisiPosSec_IntermediEsplosiDetail(analisi_id);
            /*
            StreamWriter sw_post_Sec = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga_post_Sec = "post_query_Model_Sec" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            sw_post_Sec.WriteLine(riga_post_Sec);
            sw_post_Sec.Close();
            */
            //return m_listaAnalisiPosSec_IntermediEsplosiDetail;
        }

    }

    public class AnalisiModel : B16ModelMgrChart, IGetGoogleChart
    {
        public string PathAttach { get { return m_le.GetSettings("PATH_ATTAC"); } }
      
        public decimal CostoMedioProdSimili = 0;
        private List<MyFaseValorImporto> m_ListaAnalisiSimili = new List<MyFaseValorImporto>();
        private List<MyFaseValorImporto> getAnalisiSimili()
        {
            List<MyFaseValorImporto> lstFPI = new List<MyFaseValorImporto>();
            if (m_MyAnalisi.Analisi_Gruppo_id.HasValue)
            {
                decimal costoProdSimili = 0;
                List<MyFaseValorImporto> lstAnalisiFase = new List<MyFaseValorImporto>();
                List<MyAnalisi> lstAnalisi = m_le.GetAnalisi().
                                            Where(z => z.Analisi_Gruppo_id == m_MyAnalisi.Analisi_Gruppo_id.Value
                                                    && z.Analisi_Tecnica == m_MyAnalisi.Analisi_Tecnica     
                                                    && z.Analisi_id != m_MyAnalisi.Analisi_id 
                                                    && z.Analisi_T_Staval_id == 6
                                                 ).ToList<MyAnalisi>();
                foreach (MyAnalisi analisi in lstAnalisi) // Elenco Fasi-Analisi appartenenti allo stesso gruppo e tecnica del prodotto selezionato
                {
                    if (analisi.Analisi_CostoTot.HasValue)
                    {
                        costoProdSimili += analisi.Analisi_CostoTot.Value;
                    }
                    MyAnalisiFase l = new MyAnalisiFase(analisi.Analisi_id);
                    lstAnalisiFase.AddRange(l.ElencoFasi);
                }
                if (lstAnalisi.Count() > 0)
                {
                    CostoMedioProdSimili = decimal.Round((costoProdSimili / lstAnalisi.Count()), 2, MidpointRounding.AwayFromZero);
                }

                // Elenco Fasi-prodotti appartenenti allo stesso reparto del prodotto selezionato ordinati per padre Fase
                Dictionary<int, MyFaseValorImporto> dic = new Dictionary<int, MyFaseValorImporto>();
                foreach (MyFaseValorImporto fase in lstAnalisiFase.OrderBy(z => z.Fase_ID))
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
                    dic[k].Fase_Percentuale = decimal.Round((dic[k].Fase_Percentuale / lstAnalisi.Count()), 2, MidpointRounding.AwayFromZero);
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
            this.m_ListaAnalisiSimili = getAnalisiSimili(); // Agnar QUi da Intervenire
            List<MyGoogleChartDataAjax> lst = new List<MyGoogleChartDataAjax>();
            lst.Add(getChartData("Fasi", "Peso % medio della fase per gruppo e tecnica", "Peso % della fase per la valorizzazione corrente"));

            List<MyFaseValorImporto> lstACorrente = new MyAnalisiFase(m_MyAnalisi.Analisi_id).ElencoFasi;
            MyGoogleFaseChart m = new MyGoogleFaseChart(m_ListaAnalisiSimili, lstACorrente);
            if (m.ResultList.Count() == 0){ lst.Add(getChartData("", "0", "0"));}
            foreach (MyGoogleFaseChartElement c in m.ResultList.OrderBy(z => z.Fase_ID))
            {
                lst.Add(getChartData(c.Fase_Desc, c.Fase_Percentuale_1.ToString().Replace(",", "."), c.Fase_Percentuale_2.ToString().Replace(",", ".")));
            }

            return lst;

            /*
            lst.Add(getChartData("Accettazione", "0.2", "0.7"));
            lst.Add(getChartData("Esecuzione", "0.5", "0.6"));
            lst.Add(getChartData("Fase 3", "0.53", "0.12"));
            lst.Add(getChartData("Fase 4", "0.05", "0." + dtsec));
            return lst;
             */
        }
     

        private IEnumerable<MyAnalisiPos> m_listaAnalisiPos = null;
        private IEnumerable<MyAnalisiPos> m_listaAnalisiPosSec = null;
        public IEnumerable<MyAnalisiPos> ElencoAnalisiPos { get { return m_listaAnalisiPos; } }
        public IEnumerable<MyAnalisiPos> ElencoAnalisiPosSec { get { return m_listaAnalisiPosSec; } }




        public string CoeffTariffa { get { return m_le.GetSettings("TARIFFA"); } }
        public string CoeffTariffa_D { get { return m_le.GetSettings("TARIFFA_D"); } }
        public string Coeff_Generale { get { return m_le.GetSettings("COST_GEN"); } }
        
        private MyAnalisi m_MyAnalisi = null;
        public MyAnalisi Analisi { get { return m_MyAnalisi; } }

        public string m_CoeffTariffa;
        public string m_CoeffTariffa_D;
        public string m_Coeff_Generale;
        
        
        public AnalisiModel(int analisi_id)
        {
            m_MyAnalisi = m_le.GetAnalisi(analisi_id);
            m_listaAnalisiPos = m_le.GetAnalisiPos(analisi_id);
            m_listaAnalisiPosSec = m_le.GetAnalisiPosSec(analisi_id);

           // this.m_ListaAnalisiSimili = getAnalisiSimili(); // Agnar QUi da Intervenire
        }

        public List<MyFigProf> GetFigProf(int Attiv_id)
        {
            return m_le.GetFigProf(Attiv_id);
        }
        public string CostoIndiretto { get { return m_le.GetCostoInd(m_MyAnalisi);/* return m_le.GetSettings("COST_IND");*/ } }
       
    }



    public class ListaMacchinariModel : B16ModelMgr
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



        private bool m_flgSec = false;
        private int m_valopos_id = 0;
        public bool FlagSecondario { get { return m_flgSec; } }
        public int ValPos_ID { get { return m_valopos_id; } }
        private IEnumerable<MyMacchinario> m_listaMacchinari= null;

        public IEnumerable<MyMacchinario> ElencoMacchinari
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaMacchinari.Where(z => testStringNull(z.Macchi_Codice,SearchDescription)
                     
                        );
                }
                else
                    return m_listaMacchinari;
            }

        }
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
    
        private string m_costoInd = "";
        public string CostoInd { set; get; }
        private MyMacchinario m_analisiPosMacchinario= null;

        public ListaMacchinariModel(int valpos_id)
        {
            m_flgSec = false;
            m_valopos_id = valpos_id;
            m_analisiPosMacchinario = m_le.GetAnalisiPosMacchinario(valpos_id);
            List<MyMacchinario> lst = m_le.GetElencoMacchinari()
                //.Where(z => !isDaValorizzare(z.Analisi_id) && z.Analisi_flgIntermedio == false)
                .ToList<MyMacchinario>();
            //lst.AddRange(m_le.GetIntermediDaAnalisiPosizione(valpos_id));
            m_listaMacchinari = lst;
            loadSearchSettings();

            m_AnalisiMaster = m_le.GetAnalisiGenericaDaValPos_ID(m_valopos_id);
            //m_costoInd = m_le.GetSettings("COST_IND");
            m_costoInd = m_le.GetCostoInd(m_AnalisiMaster);
        }
        public ListaMacchinariModel(int valpos_id, int sec)
        {
            m_flgSec = (sec == 1);
            m_valopos_id = valpos_id;
            m_analisiPosMacchinario = m_le.GetAnalisiPosMacchinario(valpos_id);
            List<MyMacchinario> lst = m_le.GetElencoMacchinari()
                //.Where(z => !isDaValorizzare(z.Analisi_id) && z.Analisi_flgIntermedio == false)
                .ToList<MyMacchinario>();
            //lst.AddRange(m_le.GetIntermediDaAnalisiPosizione(valpos_id));
            m_listaMacchinari = lst;
            loadSearchSettings();

            m_AnalisiMaster = m_le.GetAnalisiGenericaDaValPos_ID(m_valopos_id);
            //m_costoInd = m_le.GetSettings("COST_IND");
            m_costoInd = m_le.GetCostoInd(m_AnalisiMaster);
        }
        private MyAnalisi m_AnalisiMaster;
     
        public MyMacchinario  Macchinario { get { return m_analisiPosMacchinario; } }

       
    }
}