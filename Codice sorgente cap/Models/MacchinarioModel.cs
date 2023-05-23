using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;


namespace IZSLER_CAP.Models
{
    public class ListaMacchinari : B16ModelMgr
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

        private IEnumerable<MyMacchinario> m_listaMacchinari = null;

        public IEnumerable<MyMacchinario> ElencoMacchinari
        {
            get
            {
                IEnumerable<MyMacchinario> lst = m_le.GetElencoMacchinari().ToList<MyMacchinario>(); 
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    lst = lst.Where(z => testStringNull(z.Macchi_Codice, SearchDescription)
                        || testStringNull(z.Macchi_Desc, SearchDescription));
                }
                m_listaMacchinari = lst;
                return m_listaMacchinari;
            }
        }
        
        public ListaMacchinari()
        {
            loadSearchSettings();
        }
        private MyMacchinario m_macchinario;
        public MyMacchinario Macchinario { get { return m_macchinario; } }
        public ListaMacchinari(int macchi_ID)
        {
            m_macchinario = m_le.GetMacchinario(macchi_ID);
            //loadSearchSettings();
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

    }
   
}