using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;

namespace IZSLER_CAP.Models
{

    //public class PagedData<MyGrurep>: B16ModelMgr
    //{
    //    public IEnumerable<MyGrurep> Data { get; set; }
    //    public int NumberOfPages { get; set; }
    //    public int CurrentPage { get; set; }
    //}

    public class UtentiModel:B16ModelMgr 
    { 
        private IEnumerable<MyUtenti_Profili_Gruppi> m_listaUtenti_profili_gruppi = null;
        public IEnumerable<MyUtenti_Profili_Gruppi> ElencoUtenti_profili_gruppi 
        { 
            get { loadUtenti_profili_gruppi(); return m_listaUtenti_profili_gruppi; } 
        }
        public int SelectUtente_ID { set; get; }
        public IEnumerable<MyUtente> Data { get; set; }
        
        private MyUtente m_currentUtente { set; get; }
        public MyUtente CurrentUtente { get { return m_currentUtente; } }
        public UtentiModel() 
        {
            m_listaUtenti = m_le.GetUtenti().OrderBy(z=>z.Utente_User);//.Take (200); 
            loadSearchSettings();
            loadSearchSettingsDown();

            //SelectUtente_ID = 0;

            if (m_listaUtenti.Count() > 0)
            {
                if (SelectUtente_ID == null || SelectUtente_ID == 0)
                {
                    MyUtente f = m_listaUtenti.ElementAt(0);
                    SelectUtente_ID = f.Utente_ID;
                }
            }

        }

        private void loadSearchSettings()
        {
            NumEntities_UP = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN = l;
        }
        private void loadSearchSettingsDown()
        {
            NumEntities_Down = 10;
            CurrentPage_Down = 1;
            SearchDescription_Down = "";
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
            l.Add(new SelectListItem { Value = "25", Text = "25" });
            l.Add(new SelectListItem { Value = "50", Text = "50" });
            l.Add(new SelectListItem { Value = "100", Text = "100" });

            EntitiesN_Down = l;
        }
        public UtentiModel(int Utenti_ID)
        {
            m_currentUtente = m_le.GetUtente(Utenti_ID);
            SelectUtente_ID = Utenti_ID;
            
        }

        private IEnumerable<MyUtente> m_listaUtenti = null;
       
        
        public IEnumerable<MyUtente> ElencoUtenti
        { 
            get 
            {
                if (SearchDescription!=null && SearchDescription.Trim() != "")
                {
                    return m_listaUtenti.Where(z =>  testStringNull(z.Utente_Nome,SearchDescription) 
                        || testStringNull(z.Utente_Cognome,SearchDescription) 
                        || testStringNull(z.Utente_User,SearchDescription) 
                        || testStringNull(z.Utente_Email,SearchDescription));
                }
                else
                    return m_listaUtenti; 
            } 
        
        }
        private void loadUtenti_profili_gruppi()
        {
            if (SearchDescription_Down != null && SearchDescription_Down.Trim() != "")
            {

                m_listaUtenti_profili_gruppi = m_le.GetUtenti_Profili_Gruppi()
                    .Where(z => z.M_Utprgr_Utente_Id == this.SelectUtente_ID && (testStringNull(z.Profilo_desc, SearchDescription_Down) || testStringNull(z.Gruppo_desc, SearchDescription_Down)));
 
            }
            else
                m_listaUtenti_profili_gruppi = m_le.GetUtenti_Profili_Gruppi().Where(z => z.M_Utprgr_Utente_Id == this.SelectUtente_ID); 
        }

        public int NumberOfPages { get; set; }
        public int NumEntities_UP { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }
        public IEnumerable<SelectListItem> EntitiesN { get; set; }

        public IEnumerable<MyUtenti_Profili_Gruppi> Data_Down { get; set; }
        public int NumberOfPages_Down { get; set; }
        public int NumEntities_Down { set; get; }
        public int CurrentPage_Down { set; get; }
        public string SearchDescription_Down { set; get; }
        public IEnumerable<SelectListItem> EntitiesN_Down { get; set; }


        public List<MyProfilo> ListaUdM { set; get; }
        

    }
}