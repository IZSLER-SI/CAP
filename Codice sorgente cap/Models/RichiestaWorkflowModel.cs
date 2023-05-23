using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP.Models
{
    public class RichiestaWorkflowModel : B16ModelMgr
    {
        private int m_richie_id;
        MyRichiesta m_ric=null;
        public RichiestaWorkflowModel(int richie_id)
        {
            m_richie_id = richie_id;
            m_listaTrackingRichiesta = m_le.GetRichiestaWorkflow(m_richie_id);
            MyRichiesta m_ric = m_le.GetRichiesta(m_richie_id);
        }
        public string Codice { get { return m_ric.Richie_codice; } }
        private IEnumerable<TrackingRichiesta> m_listaTrackingRichiesta = null;
        public IEnumerable<TrackingRichiesta> ElencoRichieste { get { return m_listaTrackingRichiesta; } }
    } 
}