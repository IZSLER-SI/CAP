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

    public class RepartiModel:B16ModelMgr 
    {

        public IEnumerable<MyGrurep> Data { get; set; }
        public int NumberOfPages { get; set; }
        private MyGrurep m_currentReparto { set; get; }
        public MyGrurep CurrentReparto { get { return m_currentReparto; } }
        public RepartiModel() 
        {
            m_listaReparti = m_le.GetReparti();//.Take (200);
            NumEntities = 10;
            CurrentPage = 1;
            SearchDescription = "";
            List<SelectListItem > l = new List<SelectListItem> ();
            l.Add ( new SelectListItem{Value = "10",Text = "10",Selected =true});
            l.Add ( new SelectListItem{Value = "25",Text = "25"});
            l.Add ( new SelectListItem{Value = "50",Text = "50"});
            l.Add(new SelectListItem { Value = "100", Text = "100" });
            l.Add(new SelectListItem { Value = "200", Text = "200" });

            EntitiesN = l; 
        }
        private List<MyUtente_Profilo> m_elencoUtenti_profilo = new List<MyUtente_Profilo>();
        public List<MyUtente_Profilo> ElencoUtenti_Profilo { get { return m_elencoUtenti_profilo; } }
        public RepartiModel(int Grurep_ID)
        {
            m_currentReparto = m_le.GetReparto(Grurep_ID);
            m_elencoUtenti_profilo = m_le.GetUtentiProfili(Grurep_ID);
        }
        
        private IEnumerable<MyGrurep> m_listaReparti= null;
        public IEnumerable<MyGrurep> ElencoReparti
        { 
            get 
            {
                if (SearchDescription!=null && SearchDescription.Trim() != "")
                {
                    return m_listaReparti.Where(z => testStringNull(z.Grurep_Codice, SearchDescription) 
                        || testStringNull(z.Grurep_Desc,SearchDescription));
                }
                else
                    return m_listaReparti; 
            } 
        
        }
     

        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; } 

    }
}