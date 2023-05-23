using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;

namespace IZSLER_CAP.Models
{
    #region UdMConversioneModel
    //public class UdMConversioneModel : B16ModelMgr
    //{
    //    private int m_Analisi_pos_ID { set; get; }
    //    private bool m_flagSec { set; get; }
    //    public bool FlagSec { get { return m_flagSec; } }
    //    private MyAnalisiPos m_AnalisiPosCorrente { set; get; }
    //    public MyAnalisiPos AnalisiPosCorrente { get { return m_AnalisiPosCorrente; } }
    //    public UdMConversioneModel(int analisi_pos_ID,int sec)
    //    {
    //        m_flagSec = false;
    //        if(sec==1)
    //            m_flagSec=true;
    //        m_Analisi_pos_ID = analisi_pos_ID;
    //        m_valorizzato = false;
    //        m_AnalisiPosCorrente = m_le.GetGenericAnalisiPos(m_Analisi_pos_ID);
    //        if (m_AnalisiPosCorrente.AnalisiPos_Prodotto_id.HasValue)
    //        {
    //            m_valorizzato = true;
    //            m_ProdottoCorrente = m_le.GetProdotti(m_AnalisiPosCorrente.AnalisiPos_Prodotto_id.Value);
    //            m_udmProdotto = m_le.GetElencoUDM().Where(z => z.Unimis_Id == m_ProdottoCorrente.Prodot_UnitaMisura_ID).SingleOrDefault();
    //        }
    //        if (m_AnalisiPosCorrente.AnalisiPos_UdM_id.HasValue)
    //        {
    //            m_udmSelect = m_le.GetElencoUDM().Where(z => z.Unimis_Id == m_AnalisiPosCorrente.AnalisiPos_UdM_id.Value).SingleOrDefault();
    //        }
    //    }
    //    private bool m_valorizzato { set; get; }
    //    public bool Valorizzato { get { return m_valorizzato; } }
    //    private MyProdotto m_ProdottoCorrente { set; get; }
    //    public MyProdotto ProdottoCorrente { get { return m_ProdottoCorrente; } }
    //    private MyUdM m_udmProdotto { set; get; }
    //    public MyUdM UDMCorrenteProdotto { get { return m_udmProdotto; } }
    //    private MyUdM m_udmSelect { set; get; }
    //    public MyUdM UDMCorrenteSelect { get { return m_udmSelect; } }
    //    public string GetConversionRatio()
    //    {
    //        string ret = "";
    //        if (m_udmProdotto.Unimis_Grudmi_id == m_udmSelect.Unimis_Grudmi_id)
    //        { 
    //            MyUdM defMyUdm = m_le.GetElencoUDM().Where(z => z.Unidmi_Default == true && z.Unimis_Grudmi_id == m_udmProdotto.Unimis_Grudmi_id).SingleOrDefault();
    //            decimal d = m_udmProdotto.Unimis_Conversione / m_udmSelect.Unimis_Conversione;
    //            ret = d.ToString().Replace (".",",");
    //        }
    //        return ret;
    //    }

    //}
    #endregion

    #region ex ListaIntermediAnalisiModel
    public class ListaGruppiRepartiModel : B16ModelMgr
    {

        public IEnumerable<MyGrurep> Data { get; set; }
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
            List<SelectListItem > l = new List<SelectListItem> ();
            l.Add ( new SelectListItem{Value = "10",Text = "10",Selected =true});
            l.Add ( new SelectListItem{Value = "25",Text = "25"});
            l.Add ( new SelectListItem{Value = "50",Text = "50"});
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l; 
        }


        private IEnumerable<MyGrurep> m_listaGR = null;

        public IEnumerable<MyGrurep> ElencoEntita
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaGR.Where(z => testStringNull(z.Grurep_Codice, SearchDescription)
                        || testStringNull(z.Grurep_Desc,SearchDescription) 
                        );
                }
                else
                    return m_listaGR;
            }

        }
       
        private int m_Analisi_ID { set; get; }
        public int Analisi_ID { get { return m_Analisi_ID; } }

        private IEnumerable<MyGrurep> setListData(int user_id, int profile_id)
        {
            //m_User_id,m_Profile_id

            IEnumerable<MyGrurep> l_listaGR = null;
            Profili profilo = m_le.GetProfilo(user_id, profile_id);
            if (profilo.ProfiloCodice == "REFVAL")
            {
                l_listaGR = m_le.GetRepartiGruppiPerProfilo(profilo);
            }
            else
            {
                l_listaGR = m_le.GetRepartiGruppi().ToList<MyGrurep>();
            }
            return l_listaGR;
            
        }

        public ListaGruppiRepartiModel(int analisi_id,int user_id ,int profile_id)
        {
            // da filtrare per profilo
            m_Analisi_ID = analisi_id;
            m_listaGR = setListData(user_id ,profile_id);
            loadSearchSettings();
        }
        public ListaGruppiRepartiModel()
        {
            m_listaGR = m_le.GetRepartiGruppi().ToList<MyGrurep>();
            loadSearchSettings();
        }
    }
    #endregion

    #region ListaIntermediModel ex ListaAnalisiModel
    public class ListaIntermediModel  : B16ModelMgr
    {


        public IEnumerable<MyAnalisi> Data { get; set; }
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



        private IEnumerable<MyAnalisi> m_listaIntermedi= null;
       // public IEnumerable<MyAnalisi> ElencoIntermedi { get { return m_listaIntermedi; } }
        public ListaIntermediModel()
        {
             loadSearchSettings();
          //  m_listaIntermedi = m_le.GetAnalisi().Where(z=>z.Analisi_flgIntermedio==true);
        }
        public IEnumerable<MyAnalisi> GetElencoIntermedi(int utente_id, int profilo_id)
        {
            IEnumerable<MyAnalisi> l_listaAnalisi = null;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
            if (profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL")
            {
                l_listaAnalisi = m_le.GetAnalisiPerProfilo(profilo).Where(z =>z.Analisi_flgIntermedio==true ).ToList<MyAnalisi>();
            }
            else
            {
                l_listaAnalisi = m_le.GetAnalisi().Where(z =>z.Analisi_flgIntermedio==true ).ToList<MyAnalisi>();
            }
            m_listaIntermedi = l_listaAnalisi;

            if (SearchDescription != null && SearchDescription.Trim() != "")
            {
                return m_listaIntermedi
                           .Where(z =>  testStringNull(z.Analisi_CodiceGenerico,SearchDescription)
                               || testStringNull(z.Analisi_Descrizione,SearchDescription)
                               || testStringNull(z.Analisi_GruppoRepartoGenerico_Desc,SearchDescription))
                               ;
            }
            else
                return m_listaIntermedi;
        }
     }
    #endregion

    #region AnalisiModel + sblocco
    //public class AnalisiModelSblocco : AnalisiModel 
    //{
    //    public AnalisiModelSblocco(int analisi_id)
    //        : base(analisi_id)
    //    { m_listaPrioritaRichiesta = m_le.GetPrioritaRichiesta(); }
    //    private IEnumerable<PrioritaRichiesta> m_listaPrioritaRichiesta = null;
    //    public IEnumerable<PrioritaRichiesta> ElencoPrioritaRichiesta { get { return m_listaPrioritaRichiesta; } }
    //}

    public class IntermediModel : B16ModelMgr
    {

        private IEnumerable<MyAnalisiPos> m_listaIntermediPos = null;
        private IEnumerable<MyAnalisiPos> m_listaIntermediPosSec = null;
        public IEnumerable<MyAnalisiPos> ElencoIntermediPos { get { return m_listaIntermediPos; } }
        public IEnumerable<MyAnalisiPos> ElencoIntermediPosSec { get { return m_listaIntermediPosSec; } }
        
        private MyAnalisi m_MyAnalisi = null;
        public MyAnalisi Analisi { get { return m_MyAnalisi; } }

        public IntermediModel(int analisi_id)
        {
            m_MyAnalisi = m_le.GetAnalisi(analisi_id);
            m_listaIntermediPos = m_le.GetAnalisiPos(analisi_id);
            m_listaIntermediPosSec = m_le.GetAnalisiPosSec(analisi_id);
        }

        public List<MyFigProf> GetFigProf(int Attiv_id)
        {
            return m_le.GetFigProf(Attiv_id);
        }
   
    }
    #endregion
}