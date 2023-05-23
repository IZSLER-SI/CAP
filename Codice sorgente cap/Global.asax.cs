using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using IZSLER_CAP.Models;
using IZSLER_CAP.Helpers;

namespace IZSLER_CAP
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

             routes.MapRoute(
                  "ErrorPage",
                "{*anything}",
                new { controller = "Home", action = "FileNotFound" }
                );
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            // Log the exception.
           // Response.Redirect(Request.ApplicationPath + "/error/500");
            Response.Clear();

            HttpException httpException = exception as HttpException;
            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");

            if (httpException == null)
            {
                routeData.Values.Add("Home", "Index");
            }
            else //It's an Http Exception, Let's handle it.
            {
                switch (httpException.GetHttpCode())
                {
                    case 404:
                        // Page not found.
                        //routeData.Values.Add("Home", "FileNotFound");
                        Response.Redirect("~/Home/FileNotFound");
                        break;
                }
            }

            // Pass exception details to the target error View.
           // routeData.Values.Add("error", exception);

            // Clear the error on server.
            Server.ClearError();

            // Avoid IIS7 getting in the middle
            Response.TrySkipIisCustomErrors = true;

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
        protected void Session_End()
        {
            Session.Clear(); 
        }
        protected void Session_Start()
        {
            if (Context.Session != null)
            { 
                if (Context.Session.IsNewSession)
                {
                    Session[SessionVar.LOGINOK.ToString()] = false;
                    bool flagRedirect = false;
                    bool flagRedirectNoUser = false;
                    string nome = "";
                    List<M_UTPRGR_UTENTI_PROFILI_GRUPPI> lstProf=new List < M_UTPRGR_UTENTI_PROFILI_GRUPPI>();
                    IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
                    List<UTENTE> lstUtente = new List<UTENTE>();
                    try
                    {
                        nome = HttpContext.Current.User.Identity.Name;
                        nome = nome.Substring(nome.IndexOf("\\") + 1);
                        
                        lstUtente = context.UTENTE.Include("M_UTPRGR_UTENTI_PROFILI_GRUPPI").Where(ut => ut.UTENTE_USER == nome).ToList<UTENTE>() ;
                        if (lstUtente.Count == 1)
                        {
                            UTENTE currUT = lstUtente[0];
                            
                            lstProf = context.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(Z => Z.M_UTPRGR_UTENTE_ID ==currUT.UTENTE_ID  && Z.M_UTPRGR_GRUREP_ID != null && Z.M_UTPRGR_PROFIL_ID != null).ToList<M_UTPRGR_UTENTI_PROFILI_GRUPPI>();
                            int numProf = lstProf.Count ;
                            if (numProf == 0)
                            {
                                flagRedirect = true;
                                flagRedirectNoUser = true;
                                // redirect to RichiediUtenza
                            }
                            else
                            {
                                if (numProf > 1)
                                {
                                    flagRedirect = true;
                                }
                            }

                            if (lstUtente[0].UTENTE_LOCK)
                            { 
                                flagRedirect = true;
                                flagRedirectNoUser = true;
                                // redirect to RichiediUtenza
                            }
                        }
                        else 
                        {
                            flagRedirectNoUser = true;
                            // redirect to RichiediUtenza
                        }

                    }
                    catch {}
                    if (flagRedirectNoUser)
                    {
                        HttpContext.Current.Response.Redirect("~/Login/RichiediUtenza");
                        return;
                    }
                    if (flagRedirect)
                    {
                        string valProfile = "";
                        try
                        {
                            valProfile = Session[SessionVar.Profile.ToString()].ToString();
                        }
                        catch
                        {

                        }

                        if (valProfile == "")
                        {
                           // System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " " + nome + " Session[Profile] :" + nome + " 1 volta");
                            Session[SessionVar.UserID.ToString()] = (lstUtente[0]).UTENTE_ID.ToString();

                            HttpContext.Current.Response.Redirect("~/Login/Login");
                        }
                        else
                        {
                           // System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " " + nome + " Session[Profile] :" + nome + " Valorizzato");
                        }
                    }
                    else 
                    {
                        string valProfile = "";
                        try
                        {
                            valProfile = Session[SessionVar.Profile.ToString()].ToString();
                        }
                        catch
                        {
                        }
                        if( valProfile =="")
                        {
                            Session[SessionVar.UserID.ToString()] = (lstUtente[0]).UTENTE_ID.ToString();
                            int profID = ((M_UTPRGR_UTENTI_PROFILI_GRUPPI)lstProf[0]).M_UTPRGR_PROFIL_ID.Value ;
                            Session[SessionVar.Profile.ToString()] = profID.ToString();
                            Session[SessionVar.LOGINOK.ToString()] = true;
                            PROFIL_PROFILI profilo = context.PROFIL_PROFILI.Where(prof => prof.PROFIL_ID == profID).SingleOrDefault();

                            Session[SessionVar.ProfileDesc.ToString()] = profilo.PROFIL_DESC.ToString();
                            
                        }
                    }
                }
            }
        }
        protected void Application_AuthorizeRequest(object sender, EventArgs args)
        {
            if (HttpContext.Current != null)
            {
            }
        }
        protected void Application_AuthenticateRequest(object sender, EventArgs args)
        {
            if (HttpContext.Current != null)
            {
              
            }
        }
    }
}