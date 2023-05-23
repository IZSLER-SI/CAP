using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP.Models
{
    public class RichiestaModel : B16ModelMgr
    {
        private string m_tipoOggetto = "";
        public string TipoOggetto { get { return m_tipoOggetto; } set { m_tipoOggetto = value; } }
       // 
        private int m_chiave = 0;
        public int Chiave { get { return m_chiave; } set { m_chiave = value; } }
        public string chiave_desc;
        public int destinatatio_id;
        public string destinatatio_nome;
        private int m_Richiesta_ID = 0;
        public string Command { get; set; }
        
        private bool m_isRichiestaDiSblocco = false;
        public bool IsRichiestaDiSblocco
        { get { return m_isRichiestaDiSblocco; } set { m_isRichiestaDiSblocco = value; } }
        public bool IsDaLavorare
        {
            get
            {
                bool lret = false;
                try
                {
                    if (Paramiter != null)
                    {
                        //string[] l = Paramiter.Split("&".ToCharArray());
                        if (Paramiter.ToLower().Contains("lavorare"))
                        { return true; }
                    }
                }
                catch { }
                return lret;

            }
        }
        public bool IsInviate
        {
            get
            {
                bool lret = false;
                try
                {
                    if (Paramiter != null)
                    {
                        //string[] l = Paramiter.Split("&".ToCharArray());
                        if (Paramiter.ToLower().Contains("inviate"))
                        { return true; }
                    }
                }
                catch { }
                return lret;

            }
        }
        public bool IsReadOnly
        {
            get
            {
                bool lret = false;
                try{
                    if (Paramiter != null)
                    {
                        //string[] l = Paramiter.Split("&".ToCharArray());
                        if (Paramiter.ToLower().Contains("home_index"))
                        { return true; }
                    }
                }
                catch {}
                    return lret;
                
                }
        }

        public bool IsVisibleProdotto
        {
            get
            {
                bool lret = false;
                try
                {
                    if (Paramiter != null)
                    {
                        //string[] l = Paramiter.Split("&".ToCharArray());
                        if (Paramiter.ToLower().Contains("prodotto_index"))
                        { return true; }
                    }
                }
                catch { }
                return lret;

            }
        }

        public bool IsInsert (int id)
        {
            bool lret = false;
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            RICHIE_RICHIESTE nr = new RICHIE_RICHIESTE();
                
            nr = en.RICHIE_RICHIESTE.Include("T_STARIC_STATO_RICHIESTA").Where(x => x.RICHIE_ID == id).SingleOrDefault();
            if (nr.T_STARIC_STATO_RICHIESTA.T_STARIC_CODICE == "INS")
            {
                lret = true;
            }
            return lret;
        }

        public bool IsElimina(int id)
        {
            bool lret = false;
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            RICHIE_RICHIESTE nr = new RICHIE_RICHIESTE();

            nr = en.RICHIE_RICHIESTE.Include("T_STARIC_STATO_RICHIESTA").Where(x => x.RICHIE_ID == id).SingleOrDefault();
            if (nr.T_STARIC_STATO_RICHIESTA.T_STARIC_CODICE == "ELI")
            {
                lret = true;
            }
            return lret;
        }

        private IEnumerable<Indirizzi> m_listaIndirizzi = null;
        private IEnumerable<MyRichiesta> m_listaRichiesta = null;
        private IEnumerable<PrioritaRichiesta> m_listaPrioritaRichiesta = null;
         
        public RichiestaModel() 
        {
            m_listaRichiesta = m_le.GetRichieste().Where(z=>z.Richie_t_staric_id ==1 || z.Richie_t_staric_id ==2) ;
            m_listaIndirizzi = m_le.GetIndirizzi();
            m_listaPrioritaRichiesta = m_le.GetPrioritaRichiesta();
        }


        public RichiestaModel(int richiesta_id)
        {
            m_Richiesta_ID = richiesta_id;
            //m_listaRichiesta = m_le.GetRichieste().Where(x => x.Richie_id == m_Richiesta_ID);
            m_RichiestaCorrente = m_le.GetRichiesta(m_Richiesta_ID);


            if (m_RichiestaCorrente.Richie_t_richie_id == 4)
            {
                m_tipoOggetto = TipoOggettoRichiesta.Intermedio.ToString();
            }
            if (m_RichiestaCorrente.Richie_t_richie_id == 1)
            {
                if(m_RichiestaCorrente.Richie_valori_id != null)
                    m_tipoOggetto = TipoOggettoRichiesta.Analisi.ToString();
                if(m_RichiestaCorrente.Richie_prodotto_id != null)
                    m_tipoOggetto = TipoOggettoRichiesta.Prodotto.ToString();
            }
            if (m_RichiestaCorrente.Richie_t_richie_id == 2)
            {
                if (m_RichiestaCorrente.Richie_prodotto_id != null)
                    m_tipoOggetto = TipoOggettoRichiesta.Prodotto.ToString();
            }

            m_listaPrioritaRichiesta = m_le.GetPrioritaRichiesta();
        }

        public MyRichiesta m_RichiestaCorrente=null;
        public MyRichiesta RichiestaCorrente { get { return m_RichiestaCorrente ; } }
        public IEnumerable<MyRichiesta> ElencoRichieste { get { return m_listaRichiesta; } set { m_listaRichiesta = value; } }
        public IEnumerable<Indirizzi> ElencoIndirizzi { get { return m_listaIndirizzi; } }
        public IEnumerable<PrioritaRichiesta> ElencoPrioritaRichiesta { get { return m_listaPrioritaRichiesta; } }
    }
}