using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP.Models
{
    public class IndirizziModel : B16ModelMgr
    {
        public IndirizziModel() 
        {
            m_listaIndirizzi = m_le.GetIndirizzi();
        }
        public IndirizziModel(int Profilo_id)
        {
              //PopUp degli utenti viene caricata a seconda di chi è che la apre.Man mano che si implementano i vari profili bisogna gestirla
            //problema x supervisore che vede i nomi moltiplicati per ogni profilo
            //inizio
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            PROFIL_PROFILI u = new PROFIL_PROFILI();

            u = en.PROFIL_PROFILI.Where(z => z.PROFIL_ID == Profilo_id).SingleOrDefault();

            if (u != null && u.PROFIL_CODICE == "RESNK")
            {
                //m_listaIndirizzi = m_le.GetIndirizzi().Where(x => x.Profilo_cod == "VAL" || x.Profilo_cod == "REFVAL");

                // RIC_01336
                m_listaIndirizzi = m_le.GetIndirizzi().Where(x => x.Profilo_cod == "VAL");
            }
            else
            {
                m_listaIndirizzi = m_le.GetIndirizzi();
            }

        }

        private IEnumerable<Indirizzi> m_listaIndirizzi = null;
        public IEnumerable<Indirizzi> ElencoIndirizzi { get { return m_listaIndirizzi; } }
    }
}