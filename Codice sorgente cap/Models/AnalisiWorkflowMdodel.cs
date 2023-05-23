using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP.Models
{
    public class AnalisiWorkflowMdodel : B16ModelMgr
    {
        //
        // GET: /AnalisiWorkflowMdodel/
        private int m_analisi_id;
        MyAnalisi m_anal=null;
        public AnalisiWorkflowMdodel(int analisi_id)
        {
            m_analisi_id = analisi_id ;
            m_listaTrackingAnalisi = m_le.GetTrackingAnalisi(m_analisi_id);
            MyAnalisi m_anal = m_le.GetAnalisi(m_analisi_id);
        }
        public string Codice { get { return m_anal.Analisi_VN + "-" + m_anal.Analisi_MP_Rev ; } }
        private IEnumerable<TrackingAnalisi> m_listaTrackingAnalisi = null;
        public IEnumerable<TrackingAnalisi> ElencoTrkAnalisi{ get { return m_listaTrackingAnalisi; } }

    }
}
