using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;

namespace IZSLER_CAP.Models
{

    public class FigProfModel:B16ModelMgr 
    {
        private int m_UtenteGruppoProfilo_ID = 0;
        public int UtenteGruppoProfilo_ID { get { return m_UtenteGruppoProfilo_ID; } }

        public IEnumerable<MyFigProf> Data { get; set; }
        public int NumberOfPages { get; set; }
        private MyFigProf m_currentFigProf { set; get; }
        public MyFigProf CurrentFigProf { get { return m_currentFigProf; } }
        public FigProfModel() 
        {
            m_listaFigProf = m_le.GetListFigProf();//.Take (200);
            
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
        //public FigProfModel(int? ut)
        //{
        //    if (ut.HasValue) m_UtenteGruppoProfilo_ID = ut.Value;
        //    m_listaGruppi = m_le.GetGruppi();//.Take (200);
        //    m_listaGruppiReparti = m_le.GetRepartiGruppi();


        //    NumEntities = 10;
        //    CurrentPage = 1;
        //    SearchDescription = "";
        //    List<SelectListItem> l = new List<SelectListItem>();
        //    l.Add(new SelectListItem { Value = "10", Text = "10", Selected = true });
        //    l.Add(new SelectListItem { Value = "25", Text = "25" });
        //    l.Add(new SelectListItem { Value = "50", Text = "50" });
        //    l.Add(new SelectListItem { Value = "100", Text = "100" });
        //    l.Add(new SelectListItem { Value = "200", Text = "200" });

        //    EntitiesN = l;
        //}

        public FigProfModel(int FigProf_id)
        {
            m_currentFigProf = m_le.GetFigProfDaFigProf_ID(FigProf_id);
        }

        private IEnumerable<MyFigProf> m_listaFigProf = null;

        public IEnumerable<MyFigProf> ElencoFigureProfessionali
        { 
            get 
            {
                if (SearchDescription!=null && SearchDescription.Trim() != "")
                {
                    return m_listaFigProf.Where(z => testStringNull(z.FigProf_Codice, SearchDescription)
                                                || testStringNull(z.FigProf_Desc,SearchDescription));
                }
                else
                    return m_listaFigProf; 
            } 
        
        }

       
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; } 

    }
}