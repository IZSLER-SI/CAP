using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Text;

namespace IZSLER_CAP.Models
{
    interface IGetGoogleChart 
    {
        List<MyGoogleChartDataAjax> GetDataChart();
    }
    public class B16ModelMgrChart : B16ModelMgr
    {
        protected MyGoogleChartDataAjax getChartData(string titolo, string val1, string val2)
        {
            MyGoogleChartDataAjax chrt = new MyGoogleChartDataAjax();
            chrt.Titolo = titolo;
            chrt.Val1 = val1;
            chrt.Val2 = val2;
            return chrt;
        }
        public string GetDrawChart(string divName,bool flgProd)
        {

            StringBuilder lret = new StringBuilder();
            lret.AppendLine("function drawChart() {");
            lret.AppendLine("var data = google.visualization.arrayToDataTable(jsonDataChar);");
            lret.AppendLine("var div = $('#" + divName + "'),");
            lret.AppendLine("divWidth = div.width(), divheight = div.height();");
            lret.AppendLine(" optionsChart =");
            lret.AppendLine("{");
            if(flgProd)
                lret.AppendLine(" title: 'Peso percentuale delle fasi rispetto alla media per Gruppo',");
            else
                lret.AppendLine(" title: 'Peso percentuale delle fasi rispetto alla media per tecnica e gruppo',");
            lret.AppendLine(" titleTextStyle: { color: 'white', fontName: 'Arial', fontSize: '12px' },");
            lret.AppendLine(" backgroundColor: $.template.ie7 ? '#494C50' : 'transparent', ");
            lret.AppendLine(" vAxis: { title: 'Fasi',");
            lret.AppendLine(" textStyle: { color: '#FFFFFF' },");
            lret.AppendLine(" baselineColor: '#666666'");
            lret.AppendLine(" },");
            lret.AppendLine(" hAxis: {");
            lret.AppendLine(" textStyle: { color: 'white' },");
            lret.AppendLine(" baselineColor: '#666666'");
            lret.AppendLine(" },");
            lret.AppendLine(" legend: {");
            lret.AppendLine(" textStyle: { color: 'white' },");
            lret.AppendLine(" baselineColor: '#666666',");
            lret.AppendLine(" position: 'bottom'");
            lret.AppendLine(" },");
            lret.AppendLine(" height: '400',");
            lret.AppendLine(" width: divWidth + 100");
            lret.AppendLine("};");

            lret.AppendLine("var chart = new google.visualization.ColumnChart(document.getElementById('" + divName + "'));");
            lret.AppendLine("chart.draw(data, optionsChart);");
            lret.AppendLine("};");
            return lret.ToString();
        }

    }
    public class B16ModelMgr
    {
        public string Paramiter { get; set; }
        public int Sollec_ric_id { get; set; }

        private bool containsNotSensitive(string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
        
        public bool testStringNull(string info, string search)
        {
            if (info == null) return false;
            return containsNotSensitive(info, search, StringComparison.OrdinalIgnoreCase);  
           // return  info.Contains(search);
        }

        protected LoadEntities m_le = new LoadEntities();
        public B16ModelMgr()
        { mb_listaSolleciti = m_le.GetSollecito(); }
        public B16ModelMgr(int sollec_id)
        { mb_listaSolleciti = m_le.GetSollecito().Where(z=>z.Sollec_id == sollec_id ); }
        private IEnumerable<MySollecito> mb_listaSolleciti = null;
        public IEnumerable<MySollecito> ElencoSolleciti { get { return mb_listaSolleciti; } }

        public string Sollec_sollecitato_id
        {
            get
            {
                MyRichiesta ric = m_le.GetRichiesta(Sollec_ric_id);
                if (ric != null)
                {
                    int? ret = m_le.GetRichiesta(Sollec_ric_id).Richie_destinatario_utente_id;
                    if (ret != null)
                        return ret.ToString();
                    else return "";
                }
                else return "";
            }
        }
     
    }
}