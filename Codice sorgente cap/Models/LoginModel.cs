using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP.Models
{
    public class LoginModel:B16ModelMgr 
    {

        private List<Profili> m_listaProfili = new List<Profili>();
        public LoginModel() { }
        public LoginModel(int id)
        {
            m_listaProfili = m_le.GetProfili(id);
        }

        public IEnumerable<Profili> ListProfili()
        {
            return this.m_listaProfili;
        }
    }
}