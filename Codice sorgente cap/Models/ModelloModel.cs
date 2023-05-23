using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;

namespace IZSLER_CAP.Models
{

    public class PPModel : B16ModelMgr
    {
        public IEnumerable<MyAnalisi> Data { get; set; }
        public int NumberOfPages { get; set; }
        public bool FlagSec { get { return m_flagSec; } }
        private bool m_flagProdotto = true;
        private bool m_flagSec = true;
        private int m_ID = 0;
        public int MasterObject_ID { get { return m_ID; } }
        public bool IsProdotto { get { return m_flagProdotto; } }
        private MyAnalisi m_analisi;
        private MyProdotto m_prodotto;
        public PPModel(int id, int sec, int p)
        {
            if (sec == 0)
                m_flagSec = false;
            if (p == 0)
                m_flagProdotto = false;
            m_ID = id;

            if (m_flagProdotto)
            {
                m_prodotto = m_le.GetProdotti(m_ID);
                m_listaModelli = m_le.GetAnalisi().Where(z => z.Analisi_flgModello == true && z.Analisi_Reparto_id == m_prodotto.Prodot_Reparto_ID).ToList<MyAnalisi>();
            }
            else
            {
                m_analisi = m_le.GetAnalisi(m_ID);
                m_listaModelli = m_le.GetAnalisi().Where(z => z.Analisi_flgModello == true && z.Analisi_Gruppo_id == m_analisi.Analisi_Gruppo_id).ToList<MyAnalisi>();
            }
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });
            l.Add(new SelectListItem { Value = "200", Text = "200" });

            EntitiesN = l; 
        }

        public IEnumerable<MyAnalisi> ElencoModelli
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaModelli.Where(z => testStringNull(z.Analisi_CodiceGenerico,SearchDescription) 
                        || testStringNull(z.Analisi_Descrizione,SearchDescription));
                }
                else
                    return m_listaModelli;
            }

        }
        
        private IEnumerable<MyAnalisi> m_listaModelli = null;
      
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; } 
    }

    public class ListaModelliModel : B16ModelMgr
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

        private IEnumerable<MyAnalisi> m_listaModelli= null;
       // public IEnumerable<MyAnalisi> ElencoIntermedi { get { return m_listaModelli; } }
        public ListaModelliModel()
        {
            loadSearchSettings();
        }
        public IEnumerable<MyAnalisi> GetElencoModelli(int utente_id, int profilo_id)
        {
            IEnumerable<MyAnalisi> l_listaAnalisi = null;
            Profili profilo = m_le.GetProfilo(utente_id, profilo_id);
            if (profilo.ProfiloCodice == "VAL" || profilo.ProfiloCodice == "REFVAL")
            {
                l_listaAnalisi = m_le.GetAnalisiPerProfilo(profilo).Where(z => z.Analisi_flgModello == true).ToList<MyAnalisi>();
            }
            else
            {
                l_listaAnalisi = m_le.GetAnalisi().Where(z => z.Analisi_flgModello == true).ToList<MyAnalisi>();
            }
            m_listaModelli = l_listaAnalisi;
            if (SearchDescription != null && SearchDescription.Trim() != "")
            {
                return m_listaModelli
                           .Where(z => testStringNull(z.Analisi_CodiceGenerico, SearchDescription)
                               || testStringNull(z.Analisi_Descrizione,SearchDescription)
                               || testStringNull(z.Analisi_GruppoRepartoGenerico_Desc,SearchDescription))
                               ;
            }
            else
                return m_listaModelli;
        }
       
    }
    public class ModelloModel : B16ModelMgr
    {

        private IEnumerable<MyAnalisiPos> m_listaModelliPos = null;
        private IEnumerable<MyAnalisiPos> m_listaModelliPosSec = null;
        public IEnumerable<MyAnalisiPos> ElencoModelliPos { get { return m_listaModelliPos; } }
        public IEnumerable<MyAnalisiPos> ElencoModelliPosSec { get { return m_listaModelliPosSec; } }

        private MyAnalisi m_MyAnalisi = null;
        public MyAnalisi Analisi { get { return m_MyAnalisi; } }

        public ModelloModel(int analisi_id)
        {
            m_MyAnalisi = m_le.GetAnalisi(analisi_id);
            m_listaModelliPos = m_le.GetAnalisiPos(analisi_id);
            m_listaModelliPosSec = m_le.GetAnalisiPosSec(analisi_id);
        }

        public List<MyFigProf> GetFigProf(int Attiv_id)
        {
            return m_le.GetFigProf(Attiv_id);
        }

    }
}