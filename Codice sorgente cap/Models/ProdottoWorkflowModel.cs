using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP.Models
{
    public class ProdottoWorkflowModel : B16ModelMgr
    {
        //
        // GET: /ProdottoWorkflowMode/
        private int m_Prodotto_id;
        MyProdotto m_prodotto = null;
        public ProdottoWorkflowModel(int Prodotto_id)
        {
            m_Prodotto_id = Prodotto_id;
            m_listaTrackingProdotto  = m_le.GetTrackingProdotto(m_Prodotto_id);
            MyProdotto m_prodotto = m_le.GetProdotti(m_Prodotto_id);
        }
        public string Codice { get { return m_prodotto.Prodot_Codice; } }
        private IEnumerable<TrackingProdotti> m_listaTrackingProdotto= null;
        public IEnumerable<TrackingProdotti> ElencoTrkProdotto { get { return m_listaTrackingProdotto; } }

    }
}
