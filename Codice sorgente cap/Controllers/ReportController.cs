using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IZSLER_CAP.Models;

namespace IZSLER_CAP.Controllers
{
    public class ReportController : B16Controller
    {

        public ReportController()
            : base()
        {
            this.SetMessage = "Report";
            this.SetLiReports = "current";
        }

        //
        // GET: /Report/

        public ActionResult Show(int id, string T)
        {
            RedirectToRouteResult r = CheckLogin();
            if (r != null)
            {
                return r;
            }

            ReportModel rep = new ReportModel(id, T);

            return View(rep);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetElencoChart(string id, string mode, string dataDa, string dataA)
        {
            int ID = int.Parse(id);

            ReportModel rep = new ReportModel(ID, mode);
            rep.SetIntervallo(dataDa, dataA);
            return Json(rep.GetDataChart(12), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            B16ModelMgr p = new B16ModelMgr();
            return View(p);
        }
    }
}
