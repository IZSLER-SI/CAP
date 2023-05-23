using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;

namespace IZSLER_CAP.Models
{
    public class FaseModel : B16ModelMgr
    {
        private IEnumerable<MyFase> m_listaAttivita = null;
        private IEnumerable<MyFiguraProfessionale> m_listaFiguraProfessionale = null;
        public IEnumerable<MyFiguraProfessionale> ElencoFigureprof { get { return m_listaFiguraProfessionale; } }
        private IEnumerable<MyFiguraProfessionale_attivita> m_listaFiguraProfessionale_attivita = null;
        public IEnumerable<MyFiguraProfessionale_attivita> ElencoFigureprof_attivita { get { return m_listaFiguraProfessionale_attivita; } }
        public IEnumerable<MyFase> ElencoAttivita
        { get { loadAttivita(); return m_listaAttivita; } }
        private IEnumerable<MyFase> m_listaFasi = null;
        public int SelectFase_ID { set; get; }
        private  MyFase m_currentFase { set; get; }
        public MyFase CurrentFase { get { return m_currentFase; } }
        public int? IdPadre { set; get; }

        public IEnumerable<MyFase> ElencoFasi { get { return m_listaFasi; } }
        public FaseModel()
        {
            
            m_listaFiguraProfessionale = m_le.GetFiguraProf();
            m_listaFiguraProfessionale_attivita = m_le.GetFiguraProf_attivita();
            SelectFase_ID = 0;
            m_listaFasi = m_le.GetFasi().Where (z=>z.Fase_Fase_ID==null);
            if (m_listaFasi.Count() > 0)
            {
                MyFase f = m_listaFasi.ElementAt(0);
                SelectFase_ID = f.Fase_ID;
            //    m_currentFase = m_le.GetFase(SelectFase_ID);
            }
            
        }
        public FaseModel(int fase_id)
        {
            m_listaFiguraProfessionale = m_le.GetFiguraProf();
            m_listaFiguraProfessionale_attivita = m_le.GetFiguraProf_attivita();
            m_currentFase = m_le.GetFase(fase_id);
            SelectFase_ID = fase_id;
        }
        private void loadAttivita()
        {
            m_listaAttivita = m_le.GetFasi().Where(z => z.Fase_Fase_ID == this.SelectFase_ID); 
        }
        
    }




    public class FasiModelcoccop:B16ModelMgr
    {
        private IEnumerable<MyFiguraProfessionale> m_listaFiguraProfessionale = null;
        private IEnumerable<MyFiguraProfessionale_attivita> m_listaFiguraProfessionale_attivita = null;
        public IEnumerable<SelectListItem> ListaFasi { get; set; }
     //   public IEnumerable<SelectListItem> ListaAttivita { get; set; }
        private IEnumerable<MyFase> m_listaFasi = null;
      //  private IEnumerable<MyAttivita> m_listaAttivita = null;
        
        public FasiModelcoccop()
        {
            m_listaFiguraProfessionale = m_le.GetFiguraProf();
            m_listaFasi = m_le.GetFasi();
            ListaFasi = m_le.ListMyFaseToSLI(m_le.GetFasi());
          //  m_listaAttivita = m_le.GetAttivita();
        //    ListaAttivita = m_le.ListMyAttivitaToSLI(m_le.GetAttivita().Where (z=>z.Attivita_Fase_ID ==0).ToList<MyAttivita>());
        }
        public FasiModelcoccop(int fasi_id)
        {
            m_listaFiguraProfessionale = m_le.GetFiguraProf();
            m_listaFasi = m_le.GetFasi().Where(z => z.Fase_ID == fasi_id);
            ListaFasi = m_le.ListMyFaseToSLI(m_le.GetFasi().Where(z => z.Fase_ID == fasi_id).ToList<MyFase>());
       //     m_listaAttivita = m_le.GetAttivita(fasi_id);
         //   ListaAttivita = m_le.ListMyAttivitaToSLI(m_le.GetAttivita(fasi_id));
        }
        
        public IEnumerable<MyFase> ElencoFasi { get { return m_listaFasi; } }
        public IEnumerable<MyFiguraProfessionale> ElencoFigureprof { get { return m_listaFiguraProfessionale; } }
        public IEnumerable<MyFiguraProfessionale_attivita> ElencoFigureprof_attivita { get { return m_listaFiguraProfessionale_attivita; } }
     //   public IEnumerable<MyAttivita> ElencoAttivita { get { return m_listaAttivita; } }
    }
}