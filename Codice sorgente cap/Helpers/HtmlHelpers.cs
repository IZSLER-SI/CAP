using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Models;
using System.Text;
using System.Configuration;

namespace IZSLER_CAP.Helpers
{

    public static class ViewPageExtensions
    {
        private const string SCRIPTBLOCK_BUILDER = "ScriptBlockBuilder";

        public static MvcHtmlString ScriptBlock(
            this WebViewPage webPage,
            Func<dynamic, System.Web.WebPages.HelperResult> template)
        {
            if (!webPage.IsAjax)
            {
                var scriptBuilder = webPage.Context.Items[SCRIPTBLOCK_BUILDER]
                                   as StringBuilder ?? new StringBuilder();

                scriptBuilder.Append(template(null).ToHtmlString());

                webPage.Context.Items[SCRIPTBLOCK_BUILDER] = scriptBuilder;

                return new MvcHtmlString(string.Empty);
            }
            return new MvcHtmlString(template(null).ToHtmlString());
        }

        public static MvcHtmlString WriteScriptBlocks(this WebViewPage webPage)
        {
            var scriptBuilder = webPage.Context.Items[SCRIPTBLOCK_BUILDER]
                               as StringBuilder ?? new StringBuilder();

            return new MvcHtmlString(scriptBuilder.ToString());
        }
    }

    public static class QueryExtention
    {
        public static SelectList ToSelectList<T>(this IQueryable<T> query, string dataValueField, string dataTextField, object selectedValue)
        {
            return new SelectList(query, dataValueField, dataTextField, selectedValue ?? -1);
        }

    }
    public static class HtmlHelpers
    {
         private const string SCRIPTBLOCK_BUILDER = "ScriptBlockBuilder";

         public static MvcHtmlString ScriptBlock1(this HtmlHelper htmlHelper, Func<dynamic, System.Web.WebPages.HelperResult> template)
        {
	        var context = htmlHelper.ViewContext.HttpContext;

	        if (!context.Request.IsAjaxRequest())
	        {
		        var scriptBuilder = context.Items[SCRIPTBLOCK_BUILDER] as StringBuilder ?? new StringBuilder();
		        scriptBuilder.Append(template(null).ToHtmlString());
		        context.Items[SCRIPTBLOCK_BUILDER] = scriptBuilder;
		        return new MvcHtmlString(string.Empty);
	        }
	        return new MvcHtmlString(template(null).ToHtmlString());
        }

        #region gestione layout Date /  truncate testi
        public static string GetDateTimeFormat(this HtmlHelper helper, DateTime dt)
        {
            string lret = "";
            lret = String.Format("{0:dd}", dt) + "-" + String.Format("{0:MM}", dt) + "-" + String.Format("{0:yy}", dt);
            return lret;
        }
        public static string GetDateTimeHourFormat(this HtmlHelper helper, DateTime dt)
        {
            string lret = "";
            lret = String.Format("{0:HH}", dt) + ":" + String.Format("{0:mm}", dt);
            return lret;
        }
        public static string GetDateTimeFullHourFormat(this HtmlHelper helper, DateTime dt)
        {
            string lret = "";
            lret = String.Format("{0:HH}", dt) + ":" + String.Format("{0:mm}", dt) + ":" + String.Format("{0:ss}", dt);
            return lret;
        }
        public static string GetDateDayFormat(this HtmlHelper helper, DateTime dt)
        {
            string lret = "";
            lret = String.Format("{0:dd}", dt);
            return lret;
        }
        public static string GetHeaderDate(this HtmlHelper helper, DateTime dt)
        {
            string lret = "";
            lret = String.Format("{0:y}", dt);
            return lret;

        }

        public static string RedirectToSettings(this HtmlHelper helper, string info)
        {
            string lret = "";
            lret = "<a href=\"/Settings\">" + info + "</a>";
            return lret;
        }
        public static string Truncate(this HtmlHelper helper, string input, int length)
        {
            if (input.Length <= length)
            {
                return input;
            }
            else
            {
                return input.Substring(0, length) + "...";
            }
        }
        #endregion

        #region variabili utente messi in sessione
        public static string GetUserName(this HtmlHelper helper)
        {

            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            string val = "";
            try
            {
                val = HttpContext.Current.User.Identity.Name;
                if (val.IndexOf("\\") > 0)
                {
                    val = val.Substring(val.IndexOf("\\") + 1);
                    UTENTE ut = en.UTENTE.Where(z => z.UTENTE_USER == val).SingleOrDefault();
                    val = ut.UTENTE_NOME + " " + ut.UTENTE_COGNOME;
                }
            }
            catch { }
            return val;
        }
        public static int GetUserID(this HtmlHelper helper)
        {
            int val = 0;
            try
            {
                string v = HttpContext.Current.Session[SessionVar.UserID.ToString()].ToString();
                val = int.Parse(v);
            }
            catch { }
            return val;
        }
        public static int GetUserProfileID(this HtmlHelper helper)
        {
            int val = 0;
            try
            {
                string v = HttpContext.Current.Session[SessionVar.Profile.ToString()].ToString();
                val = int.Parse(v);
            }
            catch { }
            return val;
        }
        public static string GetUserProfile(this HtmlHelper helper)
        {
            string val = "";
            try
            {
                val = HttpContext.Current.Session[SessionVar.ProfileDesc.ToString()].ToString();
            }
            catch { }
            return val;
        }
        #endregion

        #region Gestione controllo Accessi

        private static bool checkGruppoEntita(this HtmlHelper helper, CASlides param)
        {
            bool lret = true;
            if (param == CASlides.SL_ANALISI || param == CASlides.SL_PRODOTTI)
            {
                int profile_id = GetUserProfileID(helper);
                int user_id = GetUserID(helper);
                LoadEntities le = new LoadEntities();

                Profili p = le.GetProfilo(user_id, profile_id);
                
                if (param == CASlides.SL_ANALISI)
                {
                    if(p.ElencoGruppiProva.Count == 0)
                        lret = false;
                }
                if (param == CASlides.SL_PRODOTTI)
                {
                    if (p.ElencoGruppiProdotto.Count == 0)
                        lret = false;
                }
            }
            return lret;
        }


        private static bool checkElencoEntita(this HtmlHelper helper, CASlides param)
        {
            bool lret = true;
            if (param == CASlides.SL_ANALISI  || param == CASlides.SL_PRODOTTI)
            {
                int profile_id = GetUserProfileID(helper);
                int user_id = GetUserID(helper);
                if (param == CASlides.SL_ANALISI)
                {
                    ListaAnalisiModel an = new ListaAnalisiModel();
                    if(an.GetElencoAnalisi(user_id, profile_id).Count()==0)
                        lret = false;
                }
                if (param == CASlides.SL_PRODOTTI)
                {
                    ListaIndexProdottoModel p = new ListaIndexProdottoModel();
                    if (p.GetElencoProdotti(user_id, profile_id).Count() == 0)
                        lret = false;
                }
            }
            return lret;
        }
        public static bool isSuperVisorOrSimilar(this HtmlHelper helper)
        {
            bool lret = false;
            string profile_Desc = GetUserProfile(helper).ToUpper();
            if (profile_Desc == "SUPERVISORE" || profile_Desc == "GESTORE NOMENCLATORE" || profile_Desc == "GESTORE MAGAZZINO")
                lret = true;
            return lret;
        }
        public static bool IsVisible(this HtmlHelper helper, CASlides param)
        {
            bool lret = isVisible(helper, param.ToString());
            if (lret)
            { 
                // controllo la presenza di dati sugli elenchi
                // param = CASlides.SL_ANALISI 
                // param = CASlides.SL_PRODOTTI
                
                //Ora ogni profilo abilitato può vedere le enalisi e i prodotti anche se non ha prodotti o analisi associati
                //lret = checkElencoEntita(helper, param);

                if (param == CASlides.SL_ANALISI || param == CASlides.SL_PRODOTTI)
                {
                    lret = checkGruppoEntita(helper, param);
                }
                
                //Anche se sono supervisore non ha senso vedere un pulsante se non ho neanche un gruppo associato!
                //if (!lret && isSuperVisorOrSimilar(helper))
                //    lret = true;
            }
            return lret;
        }
        public static bool IsVisible(this HtmlHelper helper, CAProdotti param)
        {
            return isVisible(helper, param.ToString());
        }
        public static bool IsVisible(this HtmlHelper helper, CAModelli param)
        {
            return isVisible(helper, param.ToString());
        }
        public static bool IsVisible(this HtmlHelper helper, CAValorizzazioni param)
        {
            return isVisible(helper, param.ToString());
        }
        public static bool IsVisible(this HtmlHelper helper, CAAnalisi param)
        {
            return isVisible(helper, param.ToString());
        }
        public static bool IsVisible(this HtmlHelper helper, CAIntermedi param)
        {
            return isVisible(helper, param.ToString());
        }
        public static bool IsVisible(this HtmlHelper helper, CARichieste param)
        {
            return isVisible(helper, param.ToString());
        }
        private static bool isVisible(this HtmlHelper helper, string param)
        {
            int profile_id = GetUserProfileID(helper);
            if (profile_id > 0)
                return CA.IsVisible(profile_id, param);
            return false;
        }

        public static bool IsEditableStato(this HtmlHelper helper, MyProdotto prodotto)
        {
            int profile_id = GetUserProfileID(helper);
            if (profile_id > 0)
                return CAStatoProfilo.IsEditableStato(profile_id, prodotto);
            return false;
        }
        public static bool IsEditableStato(this HtmlHelper helper, MyAnalisi analisi)
        {
            int profile_id = GetUserProfileID(helper);
            if (profile_id > 0)
                return CAStatoProfilo.IsEditableStato(profile_id, analisi);
            return false;
        }

        public static bool IsVisibleStato(this HtmlHelper helper, CAValorizzazioni param, MyAnalisi analisi)
        {
            return isVisibleStato(helper, param.ToString(), analisi);
        }
        public static bool IsVisibleStato(this HtmlHelper helper, CAAnalisi param, MyAnalisi analisi)
        {
            return isVisibleStato(helper, param.ToString(), analisi);
        }
        public static bool IsVisibleStato(this HtmlHelper helper, CAProdotti param, MyProdotto prodotto)
        {
            return isVisibleStato(helper, param.ToString(), prodotto);
        }
        public static bool IsVisibleStato(this HtmlHelper helper, CAIntermedi param, MyAnalisi analisi)
        {
            return isVisibleStato(helper, param.ToString(), analisi);
        }

        private static bool isVisibleStato(this HtmlHelper helper, string param, MyAnalisi analisi)
        {
            return CAStato.IsVisibleStato(analisi, param);
        }
        private static bool isVisibleStato(this HtmlHelper helper, string param, MyProdotto prodotto)
        {
            return CAStato.IsVisibleStato(prodotto, param);
        }

        public static string GetReportPath(this HtmlHelper helper)
        {
            return ConfigurationManager.AppSettings["ReportServer"];
        }
        public static string GetReportPath(this HtmlHelper helper, string relativeUrl)
        {
            return ConfigurationManager.AppSettings["ReportServer"] + relativeUrl;
        }
        #endregion


    }
    #region Enumeratori Controllo Accessi
    public enum TipoOggettoRichiesta
    {
        Analisi,
        Prodotto,
        Intermedio,
    }
    public enum SessionVar
    {
        UserID,
        UserName,
        Profile,
        ProfileDesc,
        LOGINOK,
    }
    public enum CASlides
    {
        SL_DASHBOARD,
        SL_SETTINGS,
        SL_PRODOTTI,
        SL_ANALISI,
        SL_INTERMEDI,
        SL_MODELLI,
        SL_REPORT,
        SL_STATISTICHE,
    }
    public enum CAProdotti
    {
        BT_PROD_NRIC,
        BT_PROD_SALVA,
        BT_PROD_RIC_SBLOCCO,//BT_ANAL_RIC_SBLOCCO
        BT_PROD_INV_VAL,//BT_ANAL_INV_VAL
        BT_PROD_VALIDA,//BT_ANAL_VALIDA
        BT_PROD_RESPINGI,//BT_ANAL_RESPINGI
        BT_PROD_REGDEL,//BT_ANAL_REGDEL

        BT_PROD_SBLOCCA,//BT_ANAL_SBLOCCA
        BT_PROD_WORKFLOW,//BT_ANAL_WORKFLOW
    }
    public enum CARichieste
    {
        BT_RICH_INV,
        BT_RICH_SALVA,
        BT_RICH_CANCELLA,
        BT_RICH_INV_VAL,
        BT_RICH_RIMANDA_VAL,
        BT_RICH_SBLOCCA_VAL,
        //BT_RICH_INV_CDG,
        //BT_RICH_REGDEL,
        BT_RICH_WORKFLOW,
        BT_RICH_RISP_INTERMEDIO,
        BT_RICH_RISP_INTERMEDIO_EVADI,

    }
    public enum CAModelli
    {
        BT_MOD_SALVA,
        BT_MOD_WORKFLOW,
        BT_MOD_SBLOCCA,
        BT_MOD_BLOCCA,
    }
    public enum CAIntermedi
    {
        BT_INTE_VALORIZZ_INTER,
        BT_INTE_INS,
        BT_INTE_SALVA,
        BT_INTE_WORKFLOW,
        BT_INTE_SBLOCCA,
        BT_INTE_BLOCCA,
    }
    public enum CAValorizzazioni
    {

    }
    public enum CAAnalisi
    {
        BT_ANAL_VALORIZZ_INTER,
        BT_ANAL_VALORIZZ,
        BT_ANAL_SALVA,
        BT_ANAL_INV_VAL,
        BT_ANAL_INV_CDG,
        BT_ANAL_REGDEL,
        BT_ANAL_VALIDA,
        BT_ANAL_WORKFLOW,
        BT_ANAL_RESPINGI,
        BT_ANAL_RIC_SBLOCCO,
        BT_ANAL_SBLOCCA,
    }
    #endregion
    public class CAStatoProfilo
    {
        private static CAStatoProfilo instance;

        private Dictionary<int, int> m_dicAccessIntermedi = new Dictionary<int, int>();
        private Dictionary<int, int> m_dicAccessModelli = new Dictionary<int, int>();
        private Dictionary<int, int> m_dicAccessAnalisi = new Dictionary<int, int>();
        private Dictionary<int, int> m_dicAccessProdotto = new Dictionary<int, int>();
        private CAStatoProfilo()
        {
            IZSLER_CAP_Entities c = new IZSLER_CAP_Entities();
            List<CTRSPR_CONTROLLO_STATI_PROFILI> lstVal = c.CTRSPR_CONTROLLO_STATI_PROFILI.DefaultIfEmpty().ToList<CTRSPR_CONTROLLO_STATI_PROFILI>();
            m_dicAccessAnalisi.Clear();
            m_dicAccessProdotto.Clear();
            m_dicAccessIntermedi.Clear();
            m_dicAccessModelli.Clear();
            foreach (CTRSPR_CONTROLLO_STATI_PROFILI ca in lstVal.Where(z => z.CTRSPR_CODICE == "ANA"))
            {
                m_dicAccessAnalisi.Add(ca.CTRSPR_PROFILO_ID, ca.CTRSPR_T_STATO_ID);
            }
            foreach (CTRSPR_CONTROLLO_STATI_PROFILI ca in lstVal.Where(z => z.CTRSPR_CODICE == "PRO"))
            {
                m_dicAccessProdotto.Add(ca.CTRSPR_PROFILO_ID, ca.CTRSPR_T_STATO_ID);
            }
            foreach (CTRSPR_CONTROLLO_STATI_PROFILI ca in lstVal.Where(z => z.CTRSPR_CODICE == "MOD"))
            {
                m_dicAccessModelli.Add(ca.CTRSPR_PROFILO_ID, ca.CTRSPR_T_STATO_ID);
            }
            foreach (CTRSPR_CONTROLLO_STATI_PROFILI ca in lstVal.Where(z => z.CTRSPR_CODICE == "INT"))
            {
                m_dicAccessIntermedi.Add(ca.CTRSPR_PROFILO_ID, ca.CTRSPR_T_STATO_ID);
            }
        }

        public static bool IsEditableStato(int profilo_id, MyAnalisi analisi)
        {
            if (instance == null)
            {
                instance = new CAStatoProfilo();
            }
            if (!analisi.Analisi_flgInterno)
            {
                if (instance.m_dicAccessAnalisi.ContainsKey(profilo_id))
                {
                    int res = instance.m_dicAccessAnalisi[profilo_id];
                    if (res == analisi.Analisi_T_Staval_id)
                        return true;
                }
                return false;
            }
            else
            {
                if (!analisi.Analisi_flgModello)
                {
                    if (instance.m_dicAccessIntermedi.ContainsKey(profilo_id))
                    {
                        int res = instance.m_dicAccessIntermedi[profilo_id];
                        if (res == analisi.Analisi_T_Staval_id)
                            return true;
                    }
                    return false;
                }
                else
                {
                    if (instance.m_dicAccessModelli.ContainsKey(profilo_id))
                    {
                        int res = instance.m_dicAccessModelli[profilo_id];
                        if (res == analisi.Analisi_T_Staval_id)
                            return true;
                    }
                    return false;
                }
            }
            //return false;
        }
        public static bool IsEditableStato(int profilo_id, MyProdotto prodotto)
        {
            if (instance == null)
            {
                instance = new CAStatoProfilo();
            }
            if (instance.m_dicAccessProdotto.ContainsKey(profilo_id))
            {
                int res = instance.m_dicAccessProdotto[profilo_id];
                if (res == prodotto.Prodot_T_Stapro_Id)
                    return true;
            }
            return false;
        }
    }
    public class CAStato
    {
        private static CAStato instance;

        private Dictionary<int, string> m_dicAccessAnalisi = new Dictionary<int, string>();
        private Dictionary<int, string> m_dicAccessProdotto = new Dictionary<int, string>();
        private CAStato()
        {
            IZSLER_CAP_Entities c = new IZSLER_CAP_Entities();
            List<CTRSTA_CONTROLLO_STATI> lstVal = c.CTRSTA_CONTROLLO_STATI.DefaultIfEmpty().ToList<CTRSTA_CONTROLLO_STATI>();
            m_dicAccessAnalisi.Clear();
            m_dicAccessProdotto.Clear();
            foreach (CTRSTA_CONTROLLO_STATI ca in lstVal.Where(z => z.CTRSTA_CODICE == "ANA"))
            {
                m_dicAccessAnalisi.Add(ca.CTRSTA_T_STATO_ID, ca.CTRSTA_VISIBILE);
            }
            foreach (CTRSTA_CONTROLLO_STATI ca in lstVal.Where(z => z.CTRSTA_CODICE == "PRO"))
            {
                m_dicAccessProdotto.Add(ca.CTRSTA_T_STATO_ID, ca.CTRSTA_VISIBILE);
            }
        }

        public static bool IsVisibleStato(MyAnalisi analisi, string val)
        {
            if (instance == null)
            {
                instance = new CAStato();
            }
            string res = instance.m_dicAccessAnalisi[analisi.Analisi_T_Staval_id];
            if (res.Contains("*"))
                return true;
            List<string> lstVal = res.Split(";".ToArray()).ToList<string>();
            if (lstVal.Contains(val))
                return true;
            return false;
        }
        public static bool IsVisibleStato(MyProdotto prodotto, string val)
        {
            if (instance == null)
            {
                instance = new CAStato();
            }
            string res = instance.m_dicAccessProdotto[prodotto.Prodot_T_Stapro_Id.Value];
            if (res.Contains("*"))
                return true;
            List<string> lstVal = res.Split(";".ToArray()).ToList<string>();
            if (lstVal.Contains(val))
                return true;
            return false;
        }
    }

    public class CA
    {
        private static CA instance;

        private Dictionary<int, string> m_dicAccess = new Dictionary<int, string>();
        private CA()
        {
            IZSLER_CAP_Entities c = new IZSLER_CAP_Entities();
            List<CTRACC_CONTROLLO_ACCESSI> lstVal = c.CTRACC_CONTROLLO_ACCESSI.DefaultIfEmpty().ToList<CTRACC_CONTROLLO_ACCESSI>();
            m_dicAccess.Clear();
            foreach (CTRACC_CONTROLLO_ACCESSI ca in lstVal)
            {
                m_dicAccess.Add(ca.CTRACC_PROFIL_ID, ca.CTRACC_VISIBLE);
            }
        }

        public static bool IsVisible(int profile_id, string val)
        {
            if (instance == null)
            {
                instance = new CA();
            }
            string res = instance.m_dicAccess[profile_id];
            if (res.Contains("*"))
                return true;
            List<string> lstVal = res.Split(";".ToArray()).ToList<string>();
            if (lstVal.Contains(val))
                return true;
            return false;
        }
    }
}