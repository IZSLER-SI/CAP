using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Web.Mvc;

namespace IZSLER_CAP.Models
{
   
    public class SettingsVarModel:B16ModelMgr 
    {
     
        public IEnumerable<MySettings> Data { get; set; }
        public int NumberOfPages { get; set; }
        private MySettings m_currentSetting { set; get; }
        public MySettings CurrentSetting { get { return m_currentSetting; } }
        public SettingsVarModel() 
        {
            m_listaSettings = m_le.GetSettingsList();//.Take (200);
            
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
      
        public SettingsVarModel(int Setting_ID)
        {
            m_currentSetting = m_le.GetSettingsElement(Setting_ID);
        }
        
        private IEnumerable<MySettings> m_listaSettings= null;

        public IEnumerable<MySettings> ElencoSettings
        { 
            get 
            {
                if (SearchDescription!=null && SearchDescription.Trim() != "")
                {
                    return m_listaSettings.Where(z => testStringNull(z.Settings_Codice , SearchDescription)
                                                || testStringNull(z.Settings_Value, SearchDescription));
                }
                else
                    return m_listaSettings; 
            } 
        
        }

      
        public int NumEntities { set; get; }
        public int CurrentPage { set; get; }
        public string SearchDescription { set; get; }

        public IEnumerable<SelectListItem> EntitiesN { get; set; } 

    }
}