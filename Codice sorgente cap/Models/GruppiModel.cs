using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;

namespace IZSLER_CAP.Models
{
   
    public class PagedData<MyGrurep>: B16ModelMgr
    {
        public IEnumerable<MyGrurep> Data { get; set; }
        public int NumberOfPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class GruppiModel:B16ModelMgr 
    {
        private int m_UtenteGruppoProfilo_ID = 0;
        public int UtenteGruppoProfilo_ID { get { return m_UtenteGruppoProfilo_ID; } }
        
        public IEnumerable<MyGrurep> Data { get; set; }
        public int NumberOfPages { get; set; }
        private MyGrurep m_currentGruppo { set; get; }
        public MyGrurep CurrentGruppo { get { return m_currentGruppo; } }
        public GruppiModel() 
        {
            m_listaGruppi = m_le.GetGruppi();//.Take (200);
            m_listaGruppiReparti = m_le.GetRepartiGruppi();
            
            
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
        public GruppiModel(int? ut)
        {
            if (ut.HasValue) m_UtenteGruppoProfilo_ID = ut.Value;
            m_listaGruppi = m_le.GetGruppi();//.Take (200);
            m_listaGruppiReparti = m_le.GetRepartiGruppi();


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
        private List<MyUtente_Profilo> m_elencoUtenti_profilo = new List<MyUtente_Profilo> ();
        public List<MyUtente_Profilo> ElencoUtenti_Profilo { get { return m_elencoUtenti_profilo; } }
        public GruppiModel(int Grurep_ID)
        {
            m_currentGruppo = m_le.GetGruppo(Grurep_ID);
            m_elencoUtenti_profilo = m_le.GetUtentiProfili(Grurep_ID);

        }
        
        private IEnumerable<MyGrurep> m_listaGruppi= null;
        private IEnumerable<MyGrurep> m_listaGruppiReparti = null;
        
        public IEnumerable<MyGrurep> ElencoGruppi 
        { 
            get 
            {
                if (SearchDescription!=null && SearchDescription.Trim() != "")
                {
                    return m_listaGruppi.Where(z => testStringNull(z.Grurep_Codice,SearchDescription)
                                                || testStringNull(z.Grurep_Desc,SearchDescription));
                }
                else
                    return m_listaGruppi; 
            } 
        
        }

        public IEnumerable<MyGrurep> ElencoGruppiReparti
        {
            get
            {
                if (SearchDescription != null && SearchDescription.Trim() != "")
                {
                    return m_listaGruppiReparti.Where(z => testStringNull(z.Grurep_Codice,SearchDescription) 
                                                      || testStringNull(z.Grurep_Desc,SearchDescription)
                                                      || ConvertFlagReparto(z.Grurep_Flg_Reparto,SearchDescription)
                                                      );
                }
                else
                    return m_listaGruppiReparti;
            }

        }
  
        private bool ConvertFlagReparto(bool flagReparto,string search)
        { 
            string info = "Gruppo";
            if(flagReparto)
                info = "Gruppo Prodotto";
            return info.Contains(search); 

        }

        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; } 

    }
}