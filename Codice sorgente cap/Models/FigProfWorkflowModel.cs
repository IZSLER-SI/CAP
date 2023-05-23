using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP.Models
{
    public class FigProfWorkflowModel : B16ModelMgr
    {
        //
        // GET: /FigProfWorkflowModel/
        private int m_FigProf_id;
        MyFigProf m_figProf = null;
        public FigProfWorkflowModel(int figProf_id)
        {
            m_FigProf_id = figProf_id;
            m_listaTrackingFigProf = m_le.GetTrackingFigProf(m_FigProf_id);
            MyFigProf m_figProf = m_le.GetFigProfDaFigProf_ID(m_FigProf_id);
        }
        public string Codice { get { return m_figProf.FigProf_Codice; } }
        private IEnumerable<TrackingFigProf> m_listaTrackingFigProf= null;
        public IEnumerable<TrackingFigProf> ElencoTrkFigProf { get { return m_listaTrackingFigProf; } }

    }
}
