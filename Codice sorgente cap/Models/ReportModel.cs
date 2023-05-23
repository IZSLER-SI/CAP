using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Helpers;
using System.Text;

namespace IZSLER_CAP.Models
{
    public class ReportModel : B16ModelMgr
    {
        public string Mode { get { return m_mode; } }
        public int ID { get { return m_id; } }
        private int m_id = 0;
        private string m_mode = "";
        private string m_dataDa = "";
        private string m_dataA = "";
        private MyAnalisi m_Analisi = null;
        private MyProdotto m_Prodotto = null;
        public ReportModel(int id,string mode)
        {
            m_id = id;
            m_mode = mode;
            if(mode == "A")
            {
                m_Analisi = m_le.GetAnalisi(id);   
            }
            if(mode == "P")
            {
                m_Prodotto= m_le.GetProdotti(id);   
            }
            loadData();        
        }
        public string TestataReport
        {
            get {
                string lret = "";
                if(m_Analisi !=null)
                    lret = "Analisi " + m_Analisi.Analisi_Codice_Descrizione + " " +  m_Analisi.Analisi_VN +"  " +  m_Analisi.Analisi_MP_Rev;
                if (m_Prodotto != null)
                    lret = "Prodotto " + m_Prodotto.Prodot_Codice_Desc + " " + m_Prodotto.Prodot_Codice; 
                return lret;
            }
        }
        public void SetIntervallo(string dataDa, string dataA)
        {
            this.m_dataDa = dataDa;
            this.m_dataA = dataA;
        }
        private void loadData()
        {
            if (m_Prodotto != null)
            { }
            if (m_Analisi != null)
            { }
        }
        protected MyGoogleChartDataAjax getChartData(string titolo, string val1, string val2)
        {
            MyGoogleChartDataAjax chrt = new MyGoogleChartDataAjax();
            chrt.Titolo = titolo;
            chrt.Val1 = val1;
            chrt.Val2 = val2;
            return chrt;
        }
        public List<MyGoogleChartDataAjax> GetDataChart(int numEl)
        {

            List<MyGoogleChartDataAjax> lst = new List<MyGoogleChartDataAjax>();
            lst.Add(getChartData("Prezzo", "Deliberato", "Attualizzato"));
            if (this.m_dataDa != "" && this.m_dataA != "")
            {
                lst.AddRange(elencoDatiValorizzati(numEl));
            }
            else 
            {
                lst.Add(getChartData(" ", "0", "0"));
            }

            //List<MyFaseValorImporto> lstACorrente = new MyAnalisiFase(m_MyAnalisi.Analisi_id).ElencoFasi;
            //MyGoogleFaseChart m = new MyGoogleFaseChart(m_ListaAnalisiSimili, lstACorrente);
            //if (m.ResultList.Count() == 0) { lst.Add(getChartData("", "0", "0")); }
            //foreach (MyGoogleFaseChartElement c in m.ResultList.OrderBy(z => z.Fase_ID))
            //{
            //    lst.Add(getChartData(c.Fase_Desc, c.Fase_Percentuale_1.ToString().Replace(",", "."), c.Fase_Percentuale_2.ToString().Replace(",", ".")));
            //}

            return lst;
        }
        private DateTime? convertString2DateTime(string info)
        {
            DateTime? lret = null;
            string[] part = info.Split("/".ToCharArray());

            if (part.Length != 3)
            {
                return lret;
            }
            else
            {
                try 
                {
                    int gg = int.Parse(part[0]);
                    int mm = int.Parse(part[1]);
                    int yy = int.Parse(part[2]);
                    lret = new DateTime(yy, mm, gg);
                }
                catch { }
            }
            return lret;
        }
        private List<MyGoogleChartDataAjax> getList (List<MyGoogleChartDataAjax> lstTmp,int numel)
        {
            if (lstTmp.Count() <= numel)
                return lstTmp;
            else
            {
                return lstTmp;
            }
        }
        private List<MyGoogleChartDataAjax> elencoDatiValorizzati(int numEl)
        {
            List<MyGoogleChartDataAjax> lst = new List<MyGoogleChartDataAjax>();
            List<MyGoogleChartDataAjax> lstTmp = new List<MyGoogleChartDataAjax>();
            DateTime? dtDa =convertString2DateTime(this.m_dataDa);
            DateTime? dtA = convertString2DateTime(this.m_dataA);

            if (dtDa.HasValue && dtA.HasValue)
            {
                
                if(m_Analisi!=null)
                {
                    lstTmp=m_le.GetTkvalPrezzi(this.m_id, dtDa.Value, dtA.Value);
                }
                if(m_Prodotto!=null)
                {
                    lstTmp = m_le.GetTkproPrezzi(this.m_id, dtDa.Value, dtA.Value);
                }
                if (lstTmp.Count() == 0)
                { 
                    MyGoogleChartDataAjax l = new MyGoogleChartDataAjax();
                    l.Titolo =" ";
                    l.Val1 ="0";
                    l.Val2 ="0";
                    lstTmp.Add(l);
                }
                lst = getList(lstTmp, numEl);
            }
            return lst;
        }

        public string GetDrawChart(string divName)
        {
            StringBuilder lret = new StringBuilder();
            lret.AppendLine("function drawAreaChart()");
            lret.AppendLine("{");
            lret.AppendLine("var data = google.visualization.arrayToDataTable(jsonDataChar);");
            lret.AppendLine("var div = $('#" + divName + "'),");
            lret.AppendLine("divWidth = div.width();");


            lret.AppendLine(" optionsChart =");
            lret.AppendLine(" {");
            lret.AppendLine("   title: 'Andamento dei costi rispetto al periodo di confronto',");
            //lret.AppendLine(" 		width: divWidth,");
            //lret.AppendLine(" 		height: $.template.mediaQuery.is('mobile') ? 180 : 265,");
            lret.AppendLine(" 		width: 850,");
            lret.AppendLine(" 		height: 400,");
			lret.AppendLine(" 		legend: 'right',");
			lret.AppendLine(" 		yAxis: {title: '(Euro)'},");
			lret.AppendLine(" 		backgroundColor: $.template.ie7 ? '#494C50' : 'transparent',");
			lret.AppendLine(" 		legendTextStyle: { color: 'white' },");
			lret.AppendLine(" 		titleTextStyle: { color: 'white' },");
			lret.AppendLine(" 		hAxis: {");
			lret.AppendLine(" 			textStyle: { color: 'white' }");
			lret.AppendLine(" 		},");
			lret.AppendLine(" 		vAxis: {");
			lret.AppendLine(" 			textStyle: { color: 'white' },");
			lret.AppendLine(" 			baselineColor: '#666666'");
			lret.AppendLine(" 		},");
			lret.AppendLine(" 		chartArea: {");
			lret.AppendLine(" 			top: 35,");
			lret.AppendLine(" 			left: 30,");
			//lret.AppendLine(" 			width: divWidth-40,");
            lret.AppendLine(" 			width: 850-40,");
            lret.AppendLine(" 			height: 320,");
			lret.AppendLine(" 		},");
			lret.AppendLine(" 		legend: 'bottom'");
            lret.AppendLine(" 	};");

            lret.AppendLine("var chart = new google.visualization.AreaChart(document.getElementById('" + divName + "'));");
            lret.AppendLine("chart.draw(data, optionsChart);");
            
            lret.AppendLine("}");
            return lret.ToString();
        }
    }
}