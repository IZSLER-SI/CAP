using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IZSLER_CAP.Models;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

//sim
//using System.IO;


namespace IZSLER_CAP.Helpers
{
    //SimStampaIntermedi
    public class MyIntermediEsplosiAnalisi
    {
        public int? Intesp_ordine { set; get; }
        public int? Intesp_livello { set; get; }
        public string Intesp_id { set; get; }
        public string Intesp_id_padre { set; get; }
        public string Intesp_posizione { set; get; }
        public int? Intesp_valori_id { set; get; }
        public string Intesp_fase_desc { set; get; }
        public decimal? Intesp_valpos_qta { set; get; }
        public decimal? Intesp_valpos_tot { set; get; }
        public string Intesp_codice { set; get; }
        public string Intesp_descrizione { set; get; }
        public string Intesp_t_unimis_desc { set; get; }
        public string Intesp_valpos_desc { set; get; }
        public bool? Intesp_secondaria { set; get; } 
    }

    //SimStampaIntermedi
    public class MyIntermediEsplosiProdotto
    {
        public int? Intesp_ordine { set; get; }
        public int? Intesp_livello { set; get; }
        public string Intesp_id { set; get; }
        public string Intesp_id_padre { set; get; }
        public string Intesp_posizione { set; get; }
        public int? Intesp_valori_id { set; get; }
        public string Intesp_fase_desc { set; get; }
        public decimal? Intesp_propos_qta { set; get; }
        public decimal? Intesp_propos_tot { set; get; }	
        public string Intesp_codice { set; get; }	
        public string Intesp_descrizione { set; get; }	
        public string Intesp_t_unimis_desc { set; get; }	
        public string Intesp_propos_desc { set; get; }
    }

    public class AttualizzatorePosizioni
    {
        private VALORI_VALORIZZAZIONI m_ObjAnalisi = null;
        private VALORI_VALORIZZAZIONI m_ObjIntermedio = null;
        private PRODOT_PRODOTTI m_ObjProdotto = null;
        private LoadEntities m_le =null;
        private IZSLER_CAP_Entities m_ctx;
        private void loadObjects() { m_le = new LoadEntities(); m_ctx = new IZSLER_CAP_Entities(); }
        public AttualizzatorePosizioni(VALORI_VALORIZZAZIONI v)
        {
            loadObjects();
            if (v.VALORI_FLG_INTERM)
            {
                m_ObjIntermedio = v;
            }
            else
            {
                m_ObjAnalisi = v;
            }
        }
        public AttualizzatorePosizioni(PRODOT_PRODOTTI p)
        {
            loadObjects();
            m_ObjProdotto = p;   
        }
        public void Attualizza() 
        {
            if (m_ObjAnalisi!=null)      attualizzaPosizioniAnalisi();
            if (m_ObjIntermedio != null) attualizzaPosizioniIntermedio();
            if (m_ObjProdotto != null)   attualizzaPosizioniProdotto();
        }
        
        private void attualizzaPosizioniProdotto() 
        {
            // si aggiornano solo le posizioni quindi 
            // i campi delle posizioni
            // - pos.ProdottoPos_QuantitaCosto ( con eventuale coeff dato dal Reparto )
            // - pos.ProdottoPos_TotCosto ( =  pos.ProdottoPos_QuantitaCosto * pos.ProdottoPos_Quantita [* (pos.ProdottoPos_CoeffConversione )])
            List< MyProdottoPos > lstProdPos =  m_le.GetProdottiPos(m_ObjProdotto.PRODOT_ID);
            int codiceReparto_ID = 0;
            decimal coeffCostoInd = 0 ;
            decimal lCoeff = 1;
            if (m_ObjProdotto.PRODOT_REPARTO_GRUREP_ID.HasValue)
            {
                codiceReparto_ID = m_ObjProdotto.PRODOT_REPARTO_GRUREP_ID.Value;
                MyGrurep gr= m_le.GetReparto(codiceReparto_ID);
                string ci = gr.Grurep_Cost_Ind!=null?gr.Grurep_Cost_Ind:"0";
                coeffCostoInd = decimal.Parse(ci, System.Globalization.CultureInfo.InvariantCulture);
            }

            foreach (MyProdottoPos pos in lstProdPos)
            {
                decimal totQCosto = 0;
                decimal tot = 0;

                if (pos.ProdottoPos_Fase_id.HasValue && pos.ProdottoPos_Fase_Desc.ToUpper().Contains("ACCETTAZIONE"))
                {
                    List<MyGrurep> lstGruRep = m_le.GetRepartiGruppi().Where(z => z.Grurep_ID == m_ObjProdotto.PRODOT_REPARTO_GRUREP_ID.Value).ToList<MyGrurep>();
                    if (lstGruRep.Count() == 1)
                    {
                        MyGrurep g = lstGruRep[0];
                        decimal costoInd = g.Grurep_PrezzoUnit_Accettazione.HasValue? g.Grurep_PrezzoUnit_Accettazione.Value :0;
                        lCoeff = coeffCostoInd;
                        //totQCosto = lCoeff * costoInd;
                        totQCosto = costoInd;
                        tot = (decimal)((double)totQCosto * (double)pos.ProdottoPos_Quantita * pos.ProdottoPos_CoeffConversione.Value);
                    }
                }

                string pricePos_Value = m_le.GetSettings("PRICE_POS");// +
                if(pos.ProdottoPos_Prodotto_id.HasValue ) // la posizione e' di tipo prodotto
                {
                    switch (pricePos_Value)// +
                    {
                        case "2":// Costo Industriale
                            {
                                totQCosto = 0;
                                string ret = "0";
                                MyProdotto p = m_le.GetProdotti(pos.ProdottoPos_Prodotto_id.Value);
                                try
                                {
                                    ret = m_le.GetReparto(p.Prodot_Reparto_ID.Value).Grurep_Cost_Ind;
                                }
                                catch { }
                                try
                                {
                                    decimal cind = decimal.Parse(ret, System.Globalization.CultureInfo.InvariantCulture);
                                    if (p.Prodot_CostoUnitario.HasValue)
                                        totQCosto = p.Prodot_CostoUnitario.Value * (1 + cind);
                                }
                                catch { }
                                tot = (decimal)((double)totQCosto * (double)pos.ProdottoPos_Quantita * pos.ProdottoPos_CoeffConversione.Value);
                            }
                            break;
                        case "1":
                            {
                                MyProdotto p = m_le.GetProdotti(pos.ProdottoPos_Prodotto_id.Value);
                                // controllo che il reparto del prodotto Master coincida o meno con quello del prodotto posizione

                                decimal? totQCostoNullable = null;
                                int cRep = 0;
                                if (p.Prodot_Reparto_ID.HasValue)
                                {
                                    cRep = p.Prodot_Reparto_ID.Value;
                                }
                                if (codiceReparto_ID == cRep)
                                //if (flgMatch)
                                {
                                    totQCostoNullable = p.Prodot_CostoUnitario_Deliberato;
                                }
                                else
                                {
                                    decimal costoInd = 0;
                                    // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                                    if (p.Prodot_PercCostInd != null)
                                    { decimal.TryParse(p.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                                    totQCostoNullable = p.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                                }

                                //Sim: se è nullo lo setto a 0
                                if (!totQCostoNullable.HasValue)
                                {
                                    totQCostoNullable = 0;
                                }
                                //totQCosto = p.Prodot_CostoUnitario.Value; --Sim: E' corretto il ragionamento fatto per il totQCostoNullable
                                totQCosto = totQCostoNullable.Value;

                                if(pos.ProdottoPos_CoeffConversione.HasValue)
                                    tot = (decimal)((double)totQCosto * (double)pos.ProdottoPos_Quantita * pos.ProdottoPos_CoeffConversione.Value);
                                else
                                    tot = (decimal)((double)totQCosto * (double)pos.ProdottoPos_Quantita);
                                    
                            }
                            break;
                        default:
                            {
                                MyProdotto p = m_le.GetProdotti(pos.ProdottoPos_Prodotto_id.Value);
                                int cRep = 0;
                                if (p.Prodot_Reparto_ID.HasValue)
                                {
                                    cRep = p.Prodot_Reparto_ID.Value;
                                }
                                else
                                {  // se non c'e' reparto non devo attualizzarlo, il prodotto e' esterno e non ci sono coeff da applicare.
                                    cRep = codiceReparto_ID;
                                }
                                if (cRep != codiceReparto_ID)
                                {
                                    lCoeff = 1 + coeffCostoInd;
                                }
                                //totQCosto = lCoeff * p.Prodot_CostoUnitario.Value;
                                totQCosto = p.Prodot_CostoUnitario.Value;
                                tot = (decimal)((double)totQCosto * (double)pos.ProdottoPos_Quantita * pos.ProdottoPos_CoeffConversione.Value);
                            }
                            break;
                    }
                    /* sostituito da switch sopra
                    if (m_le.GetSettings("PRICE_POS") == "1") // se abilitata da Settings OK
                    {
                        MyProdotto p = m_le.GetProdotti(pos.ProdottoPos_Prodotto_id.Value);
                        // controllo che il reparto del prodotto Master coincida o meno con quello del prodotto posizione

                        decimal? totQCostoNullable = null;
                        int cRep = 0;
                        if (p.Prodot_Reparto_ID.HasValue)
                        {
                            cRep = p.Prodot_Reparto_ID.Value;
                        }
                        if (codiceReparto_ID == cRep)
                        //if (flgMatch)
                        {
                            totQCostoNullable = p.Prodot_CostoUnitario_Deliberato;
                        }
                        else
                        {
                            decimal costoInd = 0;
                            // decimal.TryParse(m_costoInd.Replace(".", ","), out costoInd);
                            if (p.Prodot_PercCostInd != null)
                            { decimal.TryParse(p.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                            totQCostoNullable = p.Prodot_CostoUnitario_Deliberato * (1 + costoInd);
                        }

                        totQCosto = p.Prodot_CostoUnitario.Value;
                        tot = (decimal)((double)totQCosto * (double)pos.ProdottoPos_Quantita * pos.ProdottoPos_CoeffConversione.Value);
                    }
                    else
                    {
                        MyProdotto p = m_le.GetProdotti(pos.ProdottoPos_Prodotto_id.Value);
                        int cRep = 0;
                        if (p.Prodot_Reparto_ID.HasValue)
                        {
                            cRep = p.Prodot_Reparto_ID.Value;
                        }
                        else
                        {  // se non c'e' reparto non devo attualizzarlo, il prodotto e' esterno e non ci sono coeff da applicare.
                            cRep = codiceReparto_ID;
                        }
                        if (cRep != codiceReparto_ID)
                        {
                            lCoeff = 1 + coeffCostoInd;
                        }
                        //totQCosto = lCoeff * p.Prodot_CostoUnitario.Value;
                        totQCosto = p.Prodot_CostoUnitario.Value;
                        tot = (decimal)((double)totQCosto * (double)pos.ProdottoPos_Quantita * pos.ProdottoPos_CoeffConversione.Value);
                    }
                    */
                }

                if(pos.ProdottoPos_Analisi_id.HasValue ) //la posizione e' di tipo Analisi/Intermedio 
                {

                    switch (pricePos_Value) // +
                    {
                        case "2":// Costo Industriale
                            {

                                totQCosto = 0;
                                string ret = "0";
                                MyAnalisi a = m_le.GetAnalisi(pos.ProdottoPos_Analisi_id.Value);
                                try
                                {
                                    ret = m_le.GetGruppo(a.Analisi_Gruppo_id.Value).Grurep_Cost_Ind;
                                }
                                catch { }
                                try
                                {
                                    decimal cind = decimal.Parse(ret, System.Globalization.CultureInfo.InvariantCulture);
                                    if (a.Analisi_CostoTot.HasValue)
                                        totQCosto = a.Analisi_CostoTot.Value * (1 + cind);
                                }
                                catch { }

                                tot = totQCosto * pos.ProdottoPos_Quantita;
                            }
                            break;
                        case "1":
                            {
                                // le analisi vengono riportate con tariffa deliberata
                                MyAnalisi a = m_le.GetAnalisi(pos.ProdottoPos_Analisi_id.Value);
                                totQCosto = 0;

                                //Se è un intermedio prendo il costo totale deliberato.
                                if (a.Analisi_flgIntermedio == true)
                                {
                                    totQCosto = a.Analisi_CostoTotDelib.Value;
                                }
                                //Se è una analisi scelgo in base al settore selezionato
                                else
                                {
                                    string sSettore = pos.ProdottoPos_Cod_Settore;
                                    if (string.IsNullOrEmpty(sSettore))
                                        sSettore = "A/V";

                                    if (sSettore == "A/V")
                                        totQCosto = a.Analisi_CostoTariffaDelib.Value;
                                    else
                                        totQCosto = a.Analisi_CostoTariffa_D_Delib.Value;
                                }
                                //if (a.Analisi_CostoTariffaDelib.HasValue)
                                //    totQCosto = a.Analisi_CostoTariffaDelib.Value; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro all'analisi
                                
                                tot = totQCosto * pos.ProdottoPos_Quantita;
                            }
                            break;
                        default:
                            {
                                MyAnalisi a = m_le.GetAnalisi(pos.ProdottoPos_Analisi_id.Value);
                                int cRep = 0;
                                if (a.Analisi_Gruppo_id.HasValue)
                                    cRep = a.Analisi_Gruppo_id.Value;
                                if (a.Analisi_Reparto_id.HasValue)
                                    cRep = a.Analisi_Reparto_id.Value;

                                if (cRep != codiceReparto_ID)
                                {
                                    lCoeff = 1 + coeffCostoInd;
                                }
                                //totQCosto = lCoeff * a.Analisi_CostoTot.Value;
                                totQCosto = a.Analisi_CostoTot.Value;
                                tot = totQCosto * pos.ProdottoPos_Quantita;
                            }
                            break;
                    }
                    /* sostituito da switch sopra
                    if (m_le.GetSettings("PRICE_POS") == "1") // se abilitata da Settings OK
                    {
                        // le analisi vengono riportate con tariffa deliberata
                        MyAnalisi a = m_le.GetAnalisi(pos.ProdottoPos_Analisi_id.Value);
                        totQCosto = 0;
                        if (a.Analisi_CostoTariffaDelib.HasValue)
                            totQCosto = a.Analisi_CostoTariffaDelib.Value; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro all'analisi
                        tot = totQCosto * pos.ProdottoPos_Quantita;
                    }
                    else
                    {
                        MyAnalisi a = m_le.GetAnalisi(pos.ProdottoPos_Analisi_id.Value);
                        int cRep = 0;
                        if (a.Analisi_Gruppo_id.HasValue)
                            cRep = a.Analisi_Gruppo_id.Value;
                        if (a.Analisi_Reparto_id.HasValue)
                            cRep = a.Analisi_Reparto_id.Value;

                        if (cRep != codiceReparto_ID)
                        {
                            lCoeff = 1 + coeffCostoInd;
                        }
                        //totQCosto = lCoeff * a.Analisi_CostoTot.Value;
                        totQCosto = a.Analisi_CostoTot.Value;
                        tot = totQCosto * pos.ProdottoPos_Quantita;
                    }
                    */ 
                }
                if (pos.ProdottoPos_FigProf_id.HasValue) // la posizione e' un tipo Livello
                {
                    MyFigProf f = m_le.GetFigProfDaFigProf_ID(pos.ProdottoPos_FigProf_id.Value);
                    totQCosto = f.FigProf_Costo;
                    tot = totQCosto * pos.ProdottoPos_Quantita;
                }
                if (pos.ProdottoPos_Macchinario_id.HasValue) // la posizione e' un macchinario
                {
                    MyMacchinario m = m_le.GetMacchinario(pos.ProdottoPos_Macchinario_id.Value);
                    totQCosto = m.Macchi_Prezzo.Value;
                    tot = totQCosto * pos.ProdottoPos_Quantita;
                }
                /*Salvataggio fisico su db*/
                PROPOS_POSIZIONI pp = m_ctx.PROPOS_POSIZIONI.Where(z => z.PROPOS_ID == pos.ProdottoPos_id).SingleOrDefault();
                pp.PROPOS_COSTO_QTA = totQCosto;
                pp.PROPOS_TOT = tot;
                m_ctx.SaveChanges();
            }
        }
        
        private void aggiornaPosizioneGenericaAnalisi(MyAnalisiPos pos, int codiceGruppoReparto_ID,decimal coeffCostoInd )
        {
            decimal lCoeff = 1;
            decimal totQCosto = 0;
            decimal tot = 0;
            
            if (pos.AnalisiPos_Fase_id.HasValue && pos.AnalisiPos_Fase_desc.ToUpper().Contains("ACCETTAZIONE"))
            {
                List<MyGrurep> lstGruRep = m_le.GetRepartiGruppi().Where(z=>z.Grurep_ID  == codiceGruppoReparto_ID).ToList<MyGrurep> ();
                if(lstGruRep.Count() ==1)
                {
                    MyGrurep g = lstGruRep[0];
                    decimal costoInd = g.Grurep_PrezzoUnit_Accettazione.HasValue?g.Grurep_PrezzoUnit_Accettazione.Value:0 ;
                    ///lCoeff = coeffCostoInd;
                    totQCosto = 1 * costoInd;
                    tot = (decimal)((double)totQCosto * (double)pos.AnalisiPos_Quantita * pos.AnalisiPos_CoeffConversione.Value);
                }
            }

            string pricePos_Value = m_le.GetSettings("PRICE_POS");// +

            if (pos.AnalisiPos_Prodotto_id.HasValue) // la posizione e' di tipo prodotto
            {

                switch (pricePos_Value)// +
                {
                    case "2":// Costo Industriale
                        {
                            totQCosto = 0;
                            string ret = "0";
                            MyProdotto p = m_le.GetProdotti(pos.AnalisiPos_Prodotto_id.Value);
                            try
                            {
                                ret = m_le.GetReparto(p.Prodot_Reparto_ID.Value).Grurep_Cost_Ind;
                            }
                            catch { }
                            try
                            {
                                decimal cind = decimal.Parse(ret, System.Globalization.CultureInfo.InvariantCulture);
                                if (p.Prodot_CostoUnitario.HasValue)
                                    totQCosto = p.Prodot_CostoUnitario.Value * (1 + cind);
                            }
                            catch { }
                            tot = (decimal)((double)totQCosto * (double)pos.AnalisiPos_Quantita * pos.AnalisiPos_CoeffConversione.Value);
                        }
                        break;
                    case "1":
                        {
                            MyProdotto p = m_le.GetProdotti(pos.AnalisiPos_Prodotto_id.Value);

                            // vedi GetCostoTotDeliberato(MyProdotto prodotto) di ProdottoModel
                            // quindi i reparto gruppo non combaciano mai 
                            // utilizzo il calcolo del costo Industriale

                            //Sim: E' possibile che un intermedio di un prodotto abbia il reparto uguale al prodotto scelto nella posizione quindi gestisco il caso
                            int cRep = 0;
                            if (p.Prodot_Reparto_ID.HasValue)
                            {
                                cRep = p.Prodot_Reparto_ID.Value;
                            }
                            decimal? totQCostoNullable = null;
                            if (codiceGruppoReparto_ID == cRep)
                            //if (flgMatch)
                            {
                                totQCostoNullable = p.Prodot_CostoUnitario_Deliberato;
                            }
                            else
                            {
                                decimal costoInd = 0;
                                if (p.Prodot_PercCostInd != null)
                                { decimal.TryParse(p.Prodot_PercCostInd.Replace(".", ","), out costoInd); }


                                if (p.Prodot_CostoUnitario_Deliberato.HasValue)
                                    totQCostoNullable = (p.Prodot_CostoUnitario_Deliberato.Value * (1 + costoInd));
                            }
                            totQCosto = 0;
                            if (!totQCostoNullable.HasValue)
                            {
                                totQCostoNullable = 0;
                            }
                            totQCosto = totQCostoNullable.Value;
                            //totQCosto = p.Prodot_CostoUnitario.Value;
                            if(pos.AnalisiPos_CoeffConversione.HasValue)
                                tot = (decimal)((double)totQCosto * (double)pos.AnalisiPos_Quantita * pos.AnalisiPos_CoeffConversione.Value);
                            else
                                tot = (decimal)((double)totQCosto * (double)pos.AnalisiPos_Quantita);
                        }
                        break;
                    default :
                        {
                            MyProdotto p = m_le.GetProdotti(pos.AnalisiPos_Prodotto_id.Value);
                            int cRep = 0;
                            if (p.Prodot_Reparto_ID.HasValue)
                            {
                                cRep = p.Prodot_Reparto_ID.Value;
                            }
                            else
                            {  // se non c'e' reparto non devo attualizzarlo, il prodotto e' esterno e non ci sono coeff da applicare.
                                cRep = codiceGruppoReparto_ID;
                            }
                            if (cRep != codiceGruppoReparto_ID)
                            {
                                lCoeff = 1 + coeffCostoInd;
                            }
                            //totQCosto = lCoeff * p.Prodot_CostoUnitario.Value;
                            totQCosto = p.Prodot_CostoUnitario.Value;
                            tot = (decimal)((double)totQCosto * (double)pos.AnalisiPos_Quantita * pos.AnalisiPos_CoeffConversione.Value);
                        }
                        break;
                }


                /* sostituito da switch sopra
                if (m_le.GetSettings("PRICE_POS") == "1") // se abilitata da Settings OK
                {
                    MyProdotto p = m_le.GetProdotti(pos.AnalisiPos_Prodotto_id.Value);

                    // vedi GetCostoTotDeliberato(MyProdotto prodotto) di ProdottoModel
                    // quindi i reparto gruppo non combaciano mai 
                    // utilizzo il calcolo del costo Industriale
                    decimal costoInd = 0;
                    if (p.Prodot_PercCostInd != null)
                    { decimal.TryParse(p.Prodot_PercCostInd.Replace(".", ","), out costoInd); }
                    
                    totQCosto = 0;
                    if(p.Prodot_CostoUnitario_Deliberato.HasValue )
                        totQCosto = (p.Prodot_CostoUnitario_Deliberato.Value * (1 + costoInd));
                    //totQCosto = p.Prodot_CostoUnitario.Value;
                    tot = (decimal)((double)totQCosto * (double)pos.AnalisiPos_Quantita * pos.AnalisiPos_CoeffConversione.Value);
                }
                else
                {
                    MyProdotto p = m_le.GetProdotti(pos.AnalisiPos_Prodotto_id.Value);
                    int cRep = 0;
                    if (p.Prodot_Reparto_ID.HasValue)
                    {
                        cRep = p.Prodot_Reparto_ID.Value;
                    }
                    else
                    {  // se non c'e' reparto non devo attualizzarlo, il prodotto e' esterno e non ci sono coeff da applicare.
                        cRep = codiceGruppoReparto_ID;
                    }
                    if (cRep != codiceGruppoReparto_ID)
                    {
                        lCoeff = 1 + coeffCostoInd;
                    }
                    //totQCosto = lCoeff * p.Prodot_CostoUnitario.Value;
                    totQCosto = p.Prodot_CostoUnitario.Value;
                    tot = (decimal)((double)totQCosto * (double)pos.AnalisiPos_Quantita * pos.AnalisiPos_CoeffConversione.Value);
                }
                */
            }
            if (pos.AnalisiPos_Analisi_id.HasValue) //la posizione e' di tipo Analisi/Intermedio 
            {
                switch (pricePos_Value)// +
                {
                    case "2": // Costo Industriale
                        {
                            totQCosto = 0;
                            string ret = "0";
                            MyAnalisi a = m_le.GetAnalisi(pos.AnalisiPos_Analisi_id.Value);
                            try
                            {
                                ret = m_le.GetGruppo(a.Analisi_Gruppo_id.Value).Grurep_Cost_Ind;
                            }
                            catch { }
                            try
                            {
                                decimal cind = decimal.Parse(ret, System.Globalization.CultureInfo.InvariantCulture);
                                if (a.Analisi_CostoTot.HasValue)
                                    totQCosto = a.Analisi_CostoTot.Value * (1 + cind);
                            }
                            catch { }

                            tot = totQCosto * pos.AnalisiPos_Quantita;
                        }
                        break;
                    case "1":
                        {
                            MyAnalisi a = m_le.GetAnalisi(pos.AnalisiPos_Analisi_id.Value);
                            totQCosto = 0;
                            //Se è un intermedio prendo il costo totale deliberato.
                            if (a.Analisi_flgIntermedio == true)
                            {
                                totQCosto = a.Analisi_CostoTotDelib.Value;
                            }
                            //Se è una analisi scelgo in base al settore selezionato
                            else
                            {
                                string sSettore = pos.AnalisiPos_Cod_Settore;
                                if (string.IsNullOrEmpty(sSettore))
                                    sSettore = "A/V";

                                if (sSettore == "A/V")
                                    totQCosto = a.Analisi_CostoTariffaDelib.Value;
                                else
                                    totQCosto = a.Analisi_CostoTariffa_D_Delib.Value;
                            }
                            
                            //if (a.Analisi_CostoTariffaDelib.HasValue)
                            //    totQCosto = a.Analisi_CostoTariffaDelib.Value; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro all'analisi
                            
                            tot = totQCosto * pos.AnalisiPos_Quantita;
                        }
                        break;
                    default:
                        {
                            MyAnalisi a = m_le.GetAnalisi(pos.AnalisiPos_Analisi_id.Value);
                            int cRep = 0;
                            if (a.Analisi_Gruppo_id.HasValue)
                                cRep = a.Analisi_Gruppo_id.Value;
                            if (a.Analisi_Reparto_id.HasValue)
                                cRep = a.Analisi_Reparto_id.Value;

                            if (cRep != codiceGruppoReparto_ID)
                            {
                                lCoeff = 1 + coeffCostoInd;
                            }
                            //totQCosto = lCoeff * a.Analisi_CostoTot.Value;
                            totQCosto = a.Analisi_CostoTot.Value;
                            tot = totQCosto * pos.AnalisiPos_Quantita;
                        }
                        break;
                }
                /* sostituito da switch sopra
                if (m_le.GetSettings("PRICE_POS") == "1") // se abilitata da Settings OK
                {
                    // le analisi vengono riportate con tariffa deliberata
                    MyAnalisi a = m_le.GetAnalisi(pos.AnalisiPos_Analisi_id.Value);
                    totQCosto = 0;
                    if(a.Analisi_CostoTariffaDelib.HasValue)
                        totQCosto = a.Analisi_CostoTariffaDelib.Value ; // fissa la tariffa perche' sto scegliendo di inserire un'analisi/Intermedio dentro all'analisi
                    tot = totQCosto * pos.AnalisiPos_Quantita;
                }
                else
                {
                    MyAnalisi a = m_le.GetAnalisi(pos.AnalisiPos_Analisi_id.Value);
                    int cRep = 0;
                    if (a.Analisi_Gruppo_id.HasValue)
                        cRep = a.Analisi_Gruppo_id.Value;
                    if (a.Analisi_Reparto_id.HasValue)
                        cRep = a.Analisi_Reparto_id.Value;

                    if (cRep != codiceGruppoReparto_ID)
                    {
                        lCoeff = 1 + coeffCostoInd;
                    }
                    //totQCosto = lCoeff * a.Analisi_CostoTot.Value;
                    totQCosto = a.Analisi_CostoTot.Value;
                    tot = totQCosto * pos.AnalisiPos_Quantita;
                }
                 */ 
            }
            if (pos.AnalisiPos_FigProf_id.HasValue) // la posizione e' un tipo Livello
            {
                MyFigProf f = m_le.GetFigProfDaFigProf_ID(pos.AnalisiPos_FigProf_id.Value);
                totQCosto = f.FigProf_Costo;
                tot = totQCosto * pos.AnalisiPos_Quantita;

            }
            if (pos.AnalisiPos_Macchinario_id.HasValue) // la posizione e' un macchinario
            {
                MyMacchinario m = m_le.GetMacchinario(pos.AnalisiPos_Macchinario_id.Value);
                totQCosto = m.Macchi_Prezzo.Value;
                tot = totQCosto * pos.AnalisiPos_Quantita;
            }
            /*Salvataggio fisico su db*/
            VALPOS_POSIZIONI vp = m_ctx.VALPOS_POSIZIONI.Where(z => z.VALPOS_ID == pos.AnalisiPos_id).SingleOrDefault();
            vp.VALPOS_COSTO_QTA = totQCosto;
            vp.VALPOS_TOT = tot;
            m_ctx.SaveChanges();
        }
        private void attualizzaPosizioniAnalisi() 
        {
            List<MyAnalisiPos> lstAnalPos = m_le.GetAnalisiPos(m_ObjAnalisi.VALORI_ID);
            List<MyAnalisiPos> lstAnalPosSec = m_le.GetAnalisiPosSec(m_ObjAnalisi.VALORI_ID);
            int codiceGruppo_ID = 0;
            decimal coeffCostoInd = 0;
            
            if (m_ObjAnalisi.VALORI_GRUPPO_GRUREP_ID.HasValue)
            {
                codiceGruppo_ID = m_ObjAnalisi.VALORI_GRUPPO_GRUREP_ID.Value;
                MyGrurep gr = m_le.GetGruppo(codiceGruppo_ID);
                string ci = gr.Grurep_Cost_Ind != null ? gr.Grurep_Cost_Ind : "0";
                coeffCostoInd = decimal.Parse(ci, System.Globalization.CultureInfo.InvariantCulture);
            }
            foreach (MyAnalisiPos pos in lstAnalPos)
            {
                aggiornaPosizioneGenericaAnalisi(pos, codiceGruppo_ID, coeffCostoInd);
            }
            foreach (MyAnalisiPos pos in lstAnalPosSec)
            {
                aggiornaPosizioneGenericaAnalisi(pos, codiceGruppo_ID, coeffCostoInd);
            }

        }
        private void attualizzaPosizioniIntermedio()
        {
            List<MyAnalisiPos> lstAnalPos = m_le.GetAnalisiPos(m_ObjIntermedio.VALORI_ID);
            List<MyAnalisiPos> lstAnalPosSec = m_le.GetAnalisiPosSec(m_ObjIntermedio.VALORI_ID);
            int codiceGruppoReparto_ID = 0;
            decimal coeffCostoInd = 0;
            if (m_ObjIntermedio.VALORI_GRUPPO_GRUREP_ID.HasValue)
                codiceGruppoReparto_ID = m_ObjIntermedio.VALORI_GRUPPO_GRUREP_ID.Value;
            if (m_ObjIntermedio.VALORI_REPARTO_GRUREP_ID.HasValue)
                codiceGruppoReparto_ID = m_ObjIntermedio.VALORI_REPARTO_GRUREP_ID.Value;
            if(codiceGruppoReparto_ID !=0)
            {
                MyGrurep gr = m_le.GetRepartoGruppo(codiceGruppoReparto_ID);
                string ci = gr.Grurep_Cost_Ind != null ? gr.Grurep_Cost_Ind : "0";
                coeffCostoInd = decimal.Parse(ci, System.Globalization.CultureInfo.InvariantCulture);
            }

            foreach (MyAnalisiPos pos in lstAnalPos)
            {
                aggiornaPosizioneGenericaAnalisi(pos, codiceGruppoReparto_ID, coeffCostoInd);
            }
            foreach (MyAnalisiPos pos in lstAnalPosSec)
            {
                aggiornaPosizioneGenericaAnalisi(pos, codiceGruppoReparto_ID, coeffCostoInd);
            }
        }
    }

    public class MyMacchinario
    {
        public int Macchi_ID { set; get; }
        public string Macchi_Codice { set; get; }
        public string Macchi_Desc { set; get; }
        public decimal? Macchi_Prezzo { set; get; }
        //public decimal? Macchi_Prezzo_Deliberato { set; get; }
        public int? Macchi_Grurep_id { set; get; }
        public string Macchi_Grurep_Desc { set; get; }
        public decimal? Macchi_Valore_Strumentazione { set; get; }
        public decimal? Macchi_Costo_Manutenzione_Annuo { set; get; }
        public decimal? Macchi_Vita_Utile_Anni { set; get; }
        public decimal? Macchi_Minuti_Anno { set; get; }
    }
    public class MyProfilo
    {
        public int Profilo_ID { set; get; }
        public string Profilo_Codice { set; get; }
        public string Profilo_Descrizione { set; get; }
    }
    public class MyUtenti_Profili_Gruppi
    {
        public int M_Utprgr_Id { set; get; }
        public int M_Utprgr_Utente_Id  { set; get; }
        public int? M_Utprgr_Profil_Id { set; get; }
        public int? M_Utprgr_Grurep_Id { set; get; }
        public bool M_Utprgr_Flg_Principale { set; get; }
        public string Profilo_desc { set; get; }
        public string Gruppo_desc { set; get; }
        public IEnumerable<SelectListItem> ListaProfiliSL
        {
            get
            {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyProfiloToSLI(this.ListaProfili, this.M_Utprgr_Profil_Id, true);

            }
        }
        public List<MyProfilo> ListaProfili { set; get; }
    }
    public class MyUtente_Profilo : MyUtente  
    {
        public string Utente_Profilo_Descrizione { set; get; }
        public bool Utente_Flag_Principale { set; get; }
        public int? Utente_Profilo_ID { set; get; }
    
    }
    public class MyUtente
    {
        public int Utente_ID  { set; get; }
        public string Utente_User  { set; get; }
        public string Utente_Email { set; get; }
        public string Utente_Nome { set; get; }
        public string Utente_Cognome { set; get; }
        public bool Utente_Lock { set; get; }
    }

    public class MyFiguraProfessionale
    {
        public int Figpro_Id { set; get; }
        public string Figpro_Codice { set; get; }
        public string Figpro_Desc { set; get; }
        public decimal Figpro_Costo { set; get; }
    }

    public class MyFiguraProfessionale_attivita
    {
        public int M_Figatt_Id { set; get; }
        public int M_Figatt_Fase_Id { set; get; }
        public int M_Figatt_Figpro_Id { set; get; }
    }

    public class MySettings
    {
        public int Settings_Id { set; get; }
        public string Settings_Codice { set; get; }
        public string Settings_Value { set; get; }
    }
    public class MyGruppoUdm
    {
        public int Grudmi_Id { set; get; }
        public string Grudmi_Codice { set; get; }
        public string Grudmi_Desc { set; get; }
    }
    public class MyUdM
    {
        public int Unimis_Id { set; get; }
        public string Unimis_Codice { set; get; }
        public string Unimis_Desc { set; get; }
        public int Unimis_Grudmi_id { set; get; }
        public decimal Unimis_Conversione { set; get; }
        public bool Unidmi_Default { set; get; }
        public string Grudmi_Codice { set; get; }
        public string Grudmi_Desc { set; get; }
        public bool Unimis_Visible { set; get; }
    }
    public class MyFigProf
    {
        public int FigProf_ID { set; get; }
        public string FigProf_Codice { set; get; }
        public string FigProf_Desc { set; get; }
        [DisplayFormat(DataFormatString = "{0:N5}", ApplyFormatInEditMode = true)]
        public decimal FigProf_Costo { set; get; }
        public int FigProf_Attivi_ID { set; get; }
    }
    
    public class MyFase
    {
        public int Fase_ID { set; get; }
        public string Fase_Codice { set; get; }
        public string Fase_Desc { set; get; }
        public string Fase_Desc_Clear { set; get; }
        public int? Fase_Fase_ID { set; get; }
        public int? Fase_Grurep_ID { set; get; }
        public string Fase_Grurep_desc { set; get; }
    }

    public class MyAttivita
    {
        public int Attivita_ID { set; get; }
        public string Attivita_Codice { set; get; }
        public string Attivita_Desc { set; get; }
        public int Attivita_Fase_ID { set; get; }
        public string Attivita_Fase_Desc { set; get; }
        public bool Attivita_FlgObbligatoria { set; get; }
    }

    public class MyGrurep
    {
        public int Grurep_ID { set; get; }
        public string Grurep_Codice { set; get; }
        public string Grurep_Desc { set; get; }
        public string Grurep_DescEstesa { set; get; }
        public bool   Grurep_Flg_Reparto { set; get; }
        public string Grurep_Cost_Ind { set; get; }
        
        [DisplayFormat(DataFormatString = "{0:N5}", ApplyFormatInEditMode = true)]
        public decimal? Grurep_PrezzoUnit_Accettazione{ set; get; }
    }

    public class MyStatoValorizzazione
    {
        public int Staval_ID { set; get; }
        public string Staval_Codice { set; get; }
        public string Staval_Desc { set; get; }
    }

    public class MyT_Settings
    { 
        public int T_Settings_ID{set;get;}
        public string T_Settings_CODICE { set; get; }
        public string T_Settings_VALUE { set; get; }
    }
    public class MyAnalisi
    {
        public string Analisi_legge { set; get; }
        public string Analisi_CodiceGenerico { set; get; }
        public string Analisi_GruppoRepartoGenerico_Desc { set; get; }
        public int Analisi_id { set; get; }
        public int Analisi_utente_id { set; get; }
        public string Analisi_utente_des_cognome { set; get; }
        public string Analisi_utente_des_nome { set; get; }
        public bool Analisi_flgInterno { set; get; }
        public bool Analisi_flgBloccato { set; get; }
        public int? Analisi_Gruppo_id { set; get; }
        public string Analisi_Gruppo_desc { set; get; }
        public int? Analisi_Reparto_id { set; get; }
        public string Analisi_Reparto_desc { set; get; }
        public string Analisi_VN { set; get; }
        public string Analisi_MP_Rev { set; get; }
        public string Analisi_Descrizione { set; get; }
        public string Analisi_Codice_Descrizione { set; get; }
        public string Analisi_Tecnica { set; get; }
        public int? Analisi_Dim_Lotto { set; get; }
        public int? Analisi_Nr_Camp_Qualita { set; get; }
        public string Analisi_Matrice { set; get; }
        public decimal? Analisi_CostoTot { set; get; }
        public decimal? Analisi_CostoTotDelib { set; get; }
        public decimal? Analisi_CostoTariffaDelib { set; get; }
        public decimal? Analisi_CostoTariffa_D_Delib { set; get; }
        public bool Analisi_flgPonderazione{ set; get; }
        public int? Analisi_Peso_Positivo { set; get; }
        public int Analisi_T_Staval_id { set; get; }
        public string Analisi_T_Staval_desc { set; get; }
        public bool Analisi_flgIntermedio{ set; get; }
        public bool Analisi_flg_non_Programmabili{ set; get; }
        public DateTime? Analisi_VN_Data_Da { set; get; }
        public DateTime? Analisi_VN_Data_A { set; get; }
        public DateTime? Analisi_MP_Rev_Data_Scadenza { set; get; }
        public bool Analisi_flgModello { set; get; }
        public bool Analisi_flgObsoleta { set; get; }
        public string Analisi_PercCostInd { set; get; }
        public decimal? Analisi_CostoDiretto { set; get; }
        public int? Analisi_COD_VN_MP_REV_SETTORE { set; get; }
        public string Analisi_DescrizioneIntermedio = "(Intermedio) ";
        public int? Analisi_COD_VN_MP_REV_SETTORE_D { set; get; }
        public int? Analisi_COD_VN_MP_REV_SETTORE_V { set; get; }
        public string Analisi_Documento { set; get; }
        public string Analisi_Allegato1 { set; get; }
        public string Analisi_Allegato2 { set; get; }
        [DefaultValue("A")]
        public string Analisi_SettoreSelezionato { set; get; }
        public string Analisi_SettoreDescrizione {
            get {
                if (Analisi_SettoreSelezionato == "A"
                    || Analisi_SettoreSelezionato == "V")
                    return "A/V";
                else if (Analisi_SettoreSelezionato == "D")
                    return "D";
                else
                    return string.Empty;
            } 
        }
        public bool? Analisi_flg_assegn_al_gruppo { set; get; }
        public string Analisi_T_Staval_codice { set; get; }

    }
    public class MyProdottoPos
    {
        public int ProdottoPos_id { set; get; }
        public int ProdottoPos_MasterProdotto_id { set; get; }
        public int? ProdottoPos_Analisi_id { set; get; }
        public string ProdottoPos_Analisi_Desc { set; get; }
        public int? ProdottoPos_Prodotto_id { set; get; }
        public int? ProdottoPos_Prodotto_UDM_id { set; get; }
        public string ProdottoPos_Prodotto_UDM_Desc { set; get; }
        public string ProdottoPos_Prodotto_Desc { set; get; }
        public int? ProdottoPos_Fase_id { set; get; }
        public string ProdottoPos_Fase_Desc { set; get; }
        public int ProdottoPos_Fase_ORDINE { set; get; }
        public int? ProdottoPos_Fase_id_MASTER { set; get; }

        public int? ProdottoPos_FigProf_id { set; get; }
        public string ProdottoPos_FigProf_desc { set; get; }
        public string ProdottoPos_desc { set; get; }
        public decimal ProdottoPos_Quantita { set; get; }
        public decimal? ProdottoPos_QuantitaCosto { set; get; }
        public decimal? ProdottoPos_TotCosto { set; get; }
        public double? ProdottoPos_CoeffConversione { set; get; }

        public int? ProdottoPos_Macchinario_id { set; get; }
        public string ProdottoPos_Macchinario_Desc  { set; get; }

        public string ProdottoPos_Cod_Settore { set; get; }

        public bool? ProdottoPos_Flg_Intermedio { set; get; }

        public int? ProdottoPos_UdM_id { set; get; }
        public string ProdottoPos_UdM_desc { set; get; }

        public List<MyUdM> ProdottoPos_ListaUdM { set; get; }
        public IEnumerable<SelectListItem> ProdottoPos_ListaUdMSL
        {
            get
            {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyUdMToSLI(this.ProdottoPos_ListaUdM, this.ProdottoPos_UdM_id, true);

            }
        }

        public List<MyFase> ProdottoPos_ListaFasi { set; get; }
        public List<MyFigProf> ProdottoPos_ListaFigProf { set; get; }
        public IEnumerable<SelectListItem> ProdottoPos_ListaFasiSL
        {
            get
            {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyFaseToSLI(this.ProdottoPos_ListaFasi, this.ProdottoPos_Fase_id, true);
            }
        }
        public IEnumerable<SelectListItem> ProdottoPos_ListaFigProfSL
        {
            get
            {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyFigProfessionaleToSLI(this.ProdottoPos_ListaFigProf, this.ProdottoPos_FigProf_id, true);
            }
        }
    }
    public class MyAnalisiPos
    {
        public int AnalisiPos_id { set; get; }
        public int AnalisiPos_MasterAnalisi_id { set; get; }
        public int? AnalisiPos_Analisi_id { set; get; }
        public string AnalisiPos_Analisi_Desc { set; get; }
        public int? AnalisiPos_Prodotto_id { set; get; }
        public int? AnalisiPos_Prodotto_UDM_id { set; get; }
        public string AnalisiPos_Prodotto_UDM_Desc { set; get; }
        public string AnalisiPos_Prodotto_Desc { set; get; }
        public int? AnalisiPos_Fase_id { set; get; }
        public int AnalisiPos_Fase_ORDINE { set; get; }
        public int? AnalisiPos_Fase_id_MASTER { set; get; }
        public string AnalisiPos_Fase_desc { set; get; }

        public int? AnalisiPos_FigProf_id { set; get; }
        public string AnalisiPos_FigProf_desc { set; get; }
        public string AnalisiPos_desc{ set; get; }
        public decimal AnalisiPos_Quantita { set; get; }
        public decimal? AnalisiPos_QuantitaCosto { set; get; }
        public decimal? AnalisiPos_TotCosto { set; get; }
        public double? AnalisiPos_CoeffConversione { set; get; }
        public bool AnalisiPos_Secondaria { set; get; }
        public int? AnalisiPos_UdM_id { set; get; }
        public string AnalisiPos_UdM_desc { set; get; }
        public List<MyUdM> AnalisiPos_ListaUdM { set; get; }

        public int? AnalisiPos_Macchinario_id { set; get; }
        public string AnalisiPos_Macchinario_Desc { set; get; }

        public string AnalisiPos_Cod_Settore { set; get; }

        public bool? AnalisiPos_Flg_Intermedio { set; get; }

        public IEnumerable<SelectListItem> AnalisiPos_ListaUdMSL 
        {
               get {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyUdMToSLI(this.AnalisiPos_ListaUdM, this.AnalisiPos_UdM_id, true);
                    
            }
        }
     
        public List<MyFase> AnalisiPos_ListaFasi { set; get; }
        public List<MyFigProf> AnalisiPos_ListaFigProf { set; get; }
        public IEnumerable<SelectListItem> AnalisiPos_ListaFasiSL
        {
            get {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyFaseToSLI(this.AnalisiPos_ListaFasi, this.AnalisiPos_Fase_id, true);
            }
        }
        public IEnumerable<SelectListItem> AnalisiPos_ListaFigProfSL
        {
            get
            {
                LoadEntities l_le = new LoadEntities();
                return l_le.ListMyFigProfessionaleToSLI(this.AnalisiPos_ListaFigProf, this.AnalisiPos_FigProf_id,true);
            }
        }
    }
    //sim
    public class MyAnalisiPos_IntermediEsplosiDetail
    {
        public int AnalisiPos_id { set; get; }
        public int AnalisiPos_MasterAnalisi_id { set; get; }
        public int? AnalisiPos_Analisi_id { set; get; }
        public string AnalisiPos_Analisi_Desc { set; get; }
        public int? AnalisiPos_Prodotto_id { set; get; }
        public string AnalisiPos_Prodotto_UDM_Desc { set; get; }
        public string AnalisiPos_Prodotto_Desc { set; get; }
        public string AnalisiPos_Fase_desc { set; get; }
        public string AnalisiPos_FigProf_desc { set; get; }
        public decimal AnalisiPos_Quantita { set; get; }
        public string AnalisiPos_UdM_desc { set; get; }
        public bool? AnalisiPos_Flg_Intermedio { set; get; }
        public bool AnalisiPos_Secondaria { set; get; }
                
    }
    
    public class MyRichiesta
    {
        public int Richie_id { set; get; }
        public string Richie_codice { set; get; }
        public string Richie_titolo { set; get; }
        public DateTime Richie_data_richiesta { set; get; }
        public int Richie_t_richie_id { set; get; }
        public int Richie_richiedente_utente_id  { set; get; } 
        public int? Richie_destinatario_utente_id { set; get; }
        public string Richie_testo { set; get; }
        public int Richie_t_staric_id { set; get; }
        public int? Richie_valori_id  { set; get; }
        public int? Richie_prodotto_id { set; get; }
        public string Richie_prodotto_descrizione { set; get; } 
        public int Richie_t_ricpri_id { set; get; }
        public string T_Richie_desc { set; get; } 
        public string T_Richie_color { set; get; } 
        public string Richie_utente_ric_nome { set; get; }
        public string Richie_utente_ric_cognome { set; get; }
        public string Richie_utente_des_nome { set; get; }
        public string Richie_utente_des_cognome { set; get; } 
        public string T_staric_desc { set; get; } 
        public string Richie_valorizzazione { set; get; } 
        public string T_Ricpri_desc { set; get; }
        public string T_Ricpri_color { set; get; }
        public string T_Richie_codice { set; get; }
        public bool? Richie_flg_assegn_al_gruppo { set; get; }

    }    
    public class Indirizzi
    {
        public Indirizzi() { }
        public Indirizzi(int id, string nome,string cognome, string email)
        {
            this.Id = id;
            this.Nome = nome;
            this.Cognome = cognome;
            this.Email = email;
        }
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Valore { get { return this.Nome + " " + this.Cognome; } } 
        public string Email { get; set; }
        public string Profilo_cod { get; set; }
        public int Profilo_id { get; set; }
    }

    public class MyGoogleFaseChart
    {
        public MyGoogleFaseChart(List<MyFaseValorImporto> elenco, List<MyFaseValorImporto> corrente)
        {
            Dictionary<int, MyGoogleFaseChartElement> dic = new Dictionary<int, MyGoogleFaseChartElement>();
            foreach (MyFaseValorImporto e in elenco)
            { 
                MyGoogleFaseChartElement m = new MyGoogleFaseChartElement ();
                m.Fase_ID =e.Fase_ID ;
                m.Fase_Desc  = e.Fase_Desc ;
                m.Fase_Percentuale_1 = e.Fase_Percentuale ;
                m.Fase_Percentuale_2 = 0; 
                dic.Add (e.Fase_ID ,m);
            }
            foreach (MyFaseValorImporto c in corrente)
            {
                if (dic.ContainsKey(c.Fase_ID))
                {
                    MyGoogleFaseChartElement m = dic[c.Fase_ID];
                    m.Fase_Percentuale_2 = c.Fase_Percentuale;
                }
                else
                {
                    MyGoogleFaseChartElement m = new MyGoogleFaseChartElement();
                    m.Fase_ID = c.Fase_ID;
                    m.Fase_Desc = c.Fase_Desc;
                    m.Fase_Percentuale_1 = 0;
                    m.Fase_Percentuale_2= c.Fase_Percentuale;
                    dic.Add(c.Fase_ID, m);
                }
            }
            foreach (int k in dic.Keys)
            {
                m_res.Add(dic[k]);
            }
        }
        private List<MyGoogleFaseChartElement> m_res = new List<MyGoogleFaseChartElement>();
        public List<MyGoogleFaseChartElement> ResultList { get { return m_res; } }
    }
    public class MyGoogleFaseChartElement
    {
        public int Fase_ID { set; get; }
        public string Fase_Desc { set; get; }
        public decimal Fase_Percentuale_1 { set; get; }
        public decimal Fase_Percentuale_2 { set; get; }
    }
    public class MyFaseValorImporto
    {
        public string Fase_Desc { set; get; }
        public int Fase_ID { set; get; }
        public decimal? Fase_Importo{ set; get; }
        public decimal Fase_Percentuale { set; get; }

    }
    public class MyAnalisiFase
    {
        private decimal getSum()
        {
            decimal t = 0;
            decimal tSec = 0;
            foreach (MyFaseValorImporto f in m_lstFasi_Pri)
            {
                if (f.Fase_Importo.HasValue)
                {
                    t = t + f.Fase_Importo.Value;
                }
            }
            if (m_FlgPonderazione && m_Peso_Positivo.HasValue)
            {
                t = decimal.Round((t * m_Peso_Positivo.Value / 100), 2, MidpointRounding.AwayFromZero);
                foreach (MyFaseValorImporto f in m_lstFasi_Sec)
                {
                    if (f.Fase_Importo.HasValue)
                    {
                        tSec = tSec + f.Fase_Importo.Value;
                    }
                }
                tSec = decimal.Round((tSec * (100-m_Peso_Positivo.Value)/ 100), 2, MidpointRounding.AwayFromZero);
                
            }

            return t + tSec;
        }

        private bool m_FlgPonderazione;
        private int? m_Peso_Positivo;
        private int m_analisi_ID;
        private List<MyFaseValorImporto> m_lstFasi = new List<MyFaseValorImporto>();
        public List<MyFaseValorImporto> ElencoFasi { get { return m_lstFasi; } }

        private List<MyFaseValorImporto> m_lstFasi_Pri = new List<MyFaseValorImporto>();
        //public List<MyFaseValorImporto> ElencoFasi_Pri { get { return m_lstFasi_Pri; } }

        private List<MyFaseValorImporto> m_lstFasi_Sec = new List<MyFaseValorImporto>();
        //public List<MyFaseValorImporto> ElencoFasi_Sec { get { return m_lstFasi_Sec; } }


        public MyAnalisiFase(int analisi_id)
        {
            m_analisi_ID = analisi_id;
            loadData();
        }
        private void loadData()
        {
            LoadEntities le = new LoadEntities();
            MyAnalisi an = le.GetAnalisi(m_analisi_ID);
            m_FlgPonderazione = an.Analisi_flgPonderazione;
            m_Peso_Positivo = an.Analisi_Peso_Positivo;
            if (m_Peso_Positivo == null) m_Peso_Positivo = 0;
            if (!m_FlgPonderazione)
                m_Peso_Positivo = 100;
            m_lstFasi_Pri = le.GetElencoFasiAnalisi(m_analisi_ID);
            m_lstFasi_Sec = le.GetElencoFasiAnalisiSec(m_analisi_ID);
            decimal totale = getSum();
            loadPercent(totale);
            mergeFasi();

        }
        private void mergeFasi()
        {
            Dictionary <int,MyFaseValorImporto> dic = new Dictionary<int,MyFaseValorImporto> ();
            
            foreach (MyFaseValorImporto f in m_lstFasi_Pri)
            {
                dic.Add(f.Fase_ID, f);
            }

            foreach (MyFaseValorImporto f in m_lstFasi_Sec)
            {
                if(dic.ContainsKey(f.Fase_ID ))
                {
                    MyFaseValorImporto lf = dic[f.Fase_ID];
                    lf.Fase_Percentuale = lf.Fase_Percentuale + f.Fase_Percentuale;
                    dic[f.Fase_ID]= lf;
                }
                else
                {
                    dic.Add(f.Fase_ID, f);
                }
            }
            foreach (int k in dic.Keys)
            {
                m_lstFasi.Add(dic[k]);
            }

        }
        private void loadPercent(decimal tot)
        {
            foreach (MyFaseValorImporto f in m_lstFasi_Pri)
            {
                f.Fase_Percentuale = calcolaPercentual(tot, f.Fase_Importo, m_Peso_Positivo.Value);
            }
            foreach (MyFaseValorImporto f in m_lstFasi_Sec)
            {
                decimal impFase = 0;
                if (f.Fase_Importo.HasValue)
                    impFase = f.Fase_Importo.Value;
                f.Fase_Percentuale = calcolaPercentual(tot, impFase, (100 - m_Peso_Positivo.Value));
            }
        }
        private decimal calcolaPercentual(decimal tot, decimal? current, int peso)
        {
            decimal lret = 0;
            if (tot > 0)
            {
                if (current.HasValue)
                {
                    lret = (current.Value / tot);
                    lret = lret * ((decimal)peso /(decimal)100);
                    lret = decimal.Round(lret, 2, MidpointRounding.AwayFromZero);
                }
            }
            return lret;
        }
    }
    public class MyProdFase
    { 
        public MyProdFase(int prodot_id)
        {
            m_prodot_ID  =prodot_id ;
            loadData();
        }
        private void loadData()
        {
            LoadEntities le = new LoadEntities();
            m_lstFasi = le.GetElencoFasiProdotto(m_prodot_ID);
            decimal totale = getSum();
            loadPercent(totale);

        }
        private decimal calcolaPercentual(decimal tot, decimal? current)
        {
            decimal lret = 0;
            if (tot > 0)
            {
                if (current.HasValue)
                {
                    lret = decimal.Round((current.Value/tot), 2, MidpointRounding.AwayFromZero);
                }
            }
            return lret;
        }
        private void loadPercent(decimal tot)
        {
            foreach (MyFaseValorImporto f in m_lstFasi)
            {
                f.Fase_Percentuale = calcolaPercentual(tot, f.Fase_Importo);
            }
        }
        private decimal getSum()
        { 
            decimal t = 0;
            foreach (MyFaseValorImporto f in m_lstFasi)
            {
                if (f.Fase_Importo.HasValue)
                {
                    t = t + f.Fase_Importo.Value ;
                }
            }
            return t;
        }
        private int m_prodot_ID;
        private List<MyFaseValorImporto> m_lstFasi = new List<MyFaseValorImporto>();
        public List<MyFaseValorImporto> ElencoFasi { get { return m_lstFasi; } }

    }
    
    public class MyProdotto
    {
        public int Prodot_ID { set; get; }
        public string Prodot_Codice { set; get; }
        public string Prodot_CodiceGenerico { set; get; }

        public string Prodot_Desc { set; get; }
        public string Prodot_Desc_Estesa { set; get; }
        public decimal? Prodot_CostoUnitario { set; get; }
        public decimal? Prodot_CostoUnitario_Deliberato { set; get; }
        public string Prodot_UnitaMisura_descrizione { set; get; }
        public int? Prodot_UnitaMisura_ID{ set; get; }
        public bool Prodot_Flg_Bloccato { set; get; }
        public bool Prodot_Flg_Bloccato_Magazzino { set; get; }
        public bool Prodot_Flg_Interno { set; get; }
        public int? Prodot_Reparto_ID { set; get; }

        public string Prodot_Codice_Desc { set; get; }// >
        public double? Prodotto_CoeffConversione { set; get; } // >
        public string Prodot_Reparto_Desc{ set; get; }

        public int? Prodot_Dim_Lotto { set; get; }
        public int? Prodot_Nr_Camp_Qualita { set; get; }

        public string Prodot_Stato_Descrizione { set; get; }
        public string Prodot_utente_denominazione { set; get; }
        public int? Prodot_utente_id { set; get; }
        public int? Prodot_T_Stapro_Id { set; get; }
        public string T_Stapro_color { set; get; }
        public int? Prodot_Stima_Prod_Anno{ set; get; }
        public double? Prodot_Perc_Scarto{ set; get; }
        public decimal? Prodot_Costo_Tariffa { set; get; }
        public decimal? Prodot_Tariffa_Proposta { set; get; }
        public string Prodot_HASHKEY { set; get; }

        public string Prodot_UnitaMisura_descrizione_Sec { set; get; }
        public int? Prodot_UnitaMisura_ID_Sec { set; get; }
        public string Prodot_PercCostInd { set; get; }

        public string Prodot_Documento { set; get; }
        //Ric#3
        public bool? Prodot_flg_assegn_al_gruppo { set; get; }
        //Ric#3
        public string Prodot_T_Stapro_codice { set; get; }
    }
    public class Gruppo
    {
        public int GruppoID { set; get; }
        public string GruppoDesc { set; get; }
        public bool? FlgPrincipale { set; get; }
        public bool FlgIsReparto{ set; get; }
    }
    public class Profili
    {
        public int ProfiloID { set; get; }
        public string ProfiloDesc { set; get; }
        public string ProfiloCodice { set; get; }
        public List<Gruppo> ElencoGruppi { set; get; }

        public List<Gruppo> ElencoGruppiProdotto
        {
            get {
                List<Gruppo> lista = new List<Gruppo>();

                foreach(Gruppo g in this.ElencoGruppi)
                {
                    if(g.FlgIsReparto)
                        lista.Add(g);
                }

                return lista;
            }
        }

        public List<Gruppo> ElencoGruppiProva
        {
            get
            {
                List<Gruppo> lista = new List<Gruppo>();

                foreach (Gruppo g in this.ElencoGruppi)
                {
                    if (!g.FlgIsReparto)
                        lista.Add(g);
                }

                return lista;
            }
        }
    }
    public class PrioritaRichiesta
    {
        public int T_Ricpri_id { get; set; }
        public string T_Ricpri_desc { get; set; }
        public string T_Ricpri_color { get; set; }
        public string T_Ricpri_codice { get; set; }
    }
    public class TrackingProdotti
    {
        public int Trkpro_Id { get; set; }
        public int Trkpro_Prodot_id { get; set; }
        public DateTime Trkpro_Data_ins { get; set; }
        public int Trkpro_Utente_id { get; set; }
        public string Trkpro_Utente_Nome { get; set; }
        public string Trkpro_Utente_Cognome { get; set; }
        public int Trkpro_Prodot_utente_id { get; set; }
        public string Trkpro_Prodot_Utente_Nome { get; set; }
        public string Trkpro_Prodot_Utente_Cognome { get; set; }
        public int Trkpro_T_Stapro_id { get; set; }
        public string Trkpro_T_Stapro_codice { get; set; }
        public string Trkpro_T_Stapro_desc { get; set; }
        public int? Trkpro_Dim_lotto { get; set; }
        public decimal? Trkpro_Costounitario { get; set; }
        public decimal? Trkpro_Costounitario_delibe { get; set; }
        public int? Trkpro_T_Unimis_id { get; set; }
        public string Trkpro_T_Unimis_Codice { get; set; }
        public string Trkpro_T_Unimis_desc { get; set; }
        public int? Trkpro_Nr_camp_qualita { get; set; }
        public int? Trkpro_Reparto_grurep_id { get; set; }
        public string Trkpro_Reparto_codice { get; set; }
        public bool? Trkpro_Flg_interno { get; set; }
        public string Trkpro_Desc { get; set; }
        public string Trkpro_Codice { get; set; }
    }
    public class TrackingAnalisi
    {
        public int Trkana_id { get; set; }
        public int Trkana_Analisi_id { get; set; }
        public DateTime Trkana_DataIns { get; set; }
        public int Trkana_Utente_id { get; set; }
        public string Trkana_Utente_Nome { get; set; }
        public string Trkana_Utente_Cognome { get; set; }
        public int Trkana_Valori_Utente_id { get; set; }
        public string Trkana_Valori_Utente_Nome { get; set; }
        public string Trkana_Valori_Utente_Cognome { get; set; }
        public int? Trkana_DimLotto_id { get; set; }
        public decimal? Trkana_CostoTot_id { get; set; }
        public bool Trkana_FlgPonderazione { get; set; }
        public int? Trkana_PesoPositivo { get; set; }
        public int Trkana_T_Staval_id { get; set; }
        public string Trkana_T_Staval_codice { get; set; }
        public string Trkana_T_Staval_desc { get; set; }
        public string Trkana_Desc_Intermedio { get; set; }
        public string Trkana_Codice_Intermedio { get; set; }
        
    }
    public class TrackingRichiesta
    {
        public int Trkric_id { get; set; }
        public int Trkric_richie_id  { get; set; }
        public DateTime Trkric_data_ins  { get; set; }
        public int Trkric_UTENTE_ID { get; set; }
        public int? Trkric_t_staric_id { get; set; }
        public string Trkric_codice { get; set; }
        public string Trkric_titolo { get; set; }
        public string Trkric_testo { get; set; }
        public int? Trkric_t_ricpri_id { get; set; }
        public string Trkric_t_richie_desc { set; get; }
        public string Trkric_t_richie_color { set; get; }
        public string Trkric_t_staric_desc { get; set; }
        public string Trkric_utente_nome { set; get; }
        public string Trkric_utente_cognome { set; get; }
        public string Trkric_t_ricpri_desc { set; get; }
        public string Trkric_t_ricpri_color { set; get; }

    }
    public class TrackingFigProf
    {
        public int Trkfig_id { get; set; }
        public int Trkfig_FigPro_id { get; set; }
        public DateTime Trkfig_DataIns { get; set; }
        public int Trkfig_Utente_id { get; set; }
        public string Trkfig_Utente_Nome { get; set; }
        public string Trkfig_Utente_Cognome { get; set; }
        public decimal? Trkfig_Costo { get; set; }
    }
    public class MySollecito
    { 
        public int Sollec_id { set; get; }
        public DateTime Sollec_data_ins { set; get; }
        public int Sollec_richie_id  { set; get; }
        public int Sollec_sollecitante_utente_id { set; get; }
        public int Sollec_sollecitato_utente_id { set; get; }
        public string Sollec_messaggio { set; get; }
        public DateTime? Sollec_data_scadenza { set; get; }
        public string Sollec_sollecitante_nome { set; get;}
        public string Sollec_sollecitante_cognome { set; get; }
        public string Sollec_sollecitato_nome { set; get;}
        public string Sollec_sollecitato_cognome { set; get; }
        public string Sollec_rich_codice { set; get; }
        public bool? Sollec_archiviato { set; get; } 
    }
    public class MyProdottoDistinto
    {
        public MyProdottoDistinto()
        { 
            initalize(); 
        }
        public MyProdottoDistinto(int prodot_id)
        {
            this.Prodot_id = prodot_id;
            initalize(); 
        }
        public bool IsSimilar(MyProdottoDistinto pd)
        {
            bool lret = false;
            if (this.ElencoAnalisi_id.Count() == pd.ElencoAnalisi_id.Count() &&
                this.ElencoProdot_id.Count() == pd.ElencoProdot_id.Count() &&
                this.ElencoFigProf_id.Count() == pd.ElencoFigProf_id.Count() &&
                this.ElencoMacchi_id.Count() == pd.ElencoMacchi_id.Count())
            {
                
                foreach (int anal_id in this.ElencoAnalisi_id)
                {
                    if (!pd.ElencoAnalisi_id.Contains(anal_id))
                    {
                        return lret;
                    }
                }

                foreach (int prod_id in this.ElencoProdot_id)
                {
                    if (!pd.ElencoProdot_id.Contains(prod_id))
                    {
                        return lret;
                    }
                }
                foreach (int figprof_id in this.ElencoFigProf_id)
                {
                    if (!pd.ElencoFigProf_id.Contains(figprof_id))
                    {
                        return lret;
                    }
                }
                foreach (int macc_id in this.ElencoMacchi_id)
                {
                    if (!pd.ElencoMacchi_id.Contains(macc_id))
                    {
                        return lret;
                    }
                }
                lret=true;
            }
            return lret;
        }
        private void initalize()
        {
            ElencoMacchi_id = new List<int>();
            ElencoAnalisi_id = new List<int>();
            ElencoProdot_id = new List<int>();
            ElencoFigProf_id = new List<int>();
        }
        public int Prodot_id { set; get; }
        public List<int> ElencoMacchi_id{ set; get; }
        public List<int> ElencoAnalisi_id { set; get; }
        public List<int> ElencoProdot_id { set; get; }
        public List<int> ElencoFigProf_id { set; get; }

        public int NumTotPosizioni 
        { get 
            { 
                return ( ElencoAnalisi_id.Count()+ 
                     ElencoMacchi_id.Count() +
                     ElencoProdot_id.Count()+
                     ElencoFigProf_id.Count());
             } 
        }
    }

    public class LoadEntities
    {

        public MySettings GetSettingsElement(int settigs_ID)
        {
            return GetSettingsList().Where(z => z.Settings_Id == settigs_ID).SingleOrDefault(); 
        }
        public List<MySettings> GetSettingsList()
        {
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<MySettings> a =
                   (
                       from sett in ctx.T_SETTIN_SETTINGS
                       select new
                       {
                           Settings_Id = sett.T_SETTIN_ID,
                           Settings_Codice = sett.T_SETTIN_CODICE,
                           Settings_Value = sett.T_SETTIN_VALUE
                       }
                       ).Select(o => new MySettings
                       {
                           Settings_Id =o.Settings_Id ,
                           Settings_Codice =o.Settings_Codice ,
                           Settings_Value =o.Settings_Value
                       }).ToList<MySettings>();

            return a;
        }
        private string dateTime2String(DateTime dt)
        {
            string lret = "";
            lret += String.Format("{0:00}", dt.Day);
            lret += "-";
            lret += String.Format("{0:00}", dt.Month);
            lret += "-";
            lret += String.Format("{0:00}", dt.Year);
            return lret;
        }
        private string decimal2String(Decimal? d)
        {
            string lret = "0";
            if (d.HasValue) return decimal2String(d.Value);
            return lret;
        }
        private string decimal2String(Decimal d)
        {
            string lret = "";
            lret = d.ToString("#.##");
            lret = lret.Replace(",", ".");
            return lret;
        }
        public List<MyGoogleChartDataAjax> GetTkvalPrezzi(int id, DateTime dataDa, DateTime dataA)
        { 
            List<MyGoogleChartDataAjax> lst = new List<MyGoogleChartDataAjax>();


            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

            List<TrackingValorizzazioni_Result> lstret = en.USPT_GET_TRACKING_VALORIZZAZIONI(id, dataDa, dataA).ToList < TrackingValorizzazioni_Result>();
            foreach (TrackingValorizzazioni_Result el in lstret)
            {
                MyGoogleChartDataAjax d = new MyGoogleChartDataAjax();
                d.Titolo = dateTime2String(el.TKVALP_DATA_INS);
                d.Val1 = decimal2String(el.TKVALP_COSTO_DELIB);
                d.Val2 = decimal2String(el.TKVALP_COSTO);
                d.Sospeso = el.TKVALP_FLG_BLOCCATO_MAGAZZINO ;
                lst.Add(d); 
            }
            /*

            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            var z = ( 
                    from tkv  in ctx.TKVALP_PREZZI_VALORIZZAZIONI_TRK 
                    where tkv.TKVALP_VALORI_ID == id 
                    && tkv.TKVALP_DATA_INS >= dataDa
                    && tkv.TKVALP_DATA_INS <= dataA
                    select new 
                    {
                        Titolo = tkv.TKVALP_DATA_INS,
                        Val1 = tkv.TKVALP_COSTO_DELIB,
                        Val2 = tkv.TKVALP_COSTO,
                        Sospeso = tkv.TKVALP_FLG_BLOCCATO_MAGAZZINO 
                    }).ToList();

            foreach (var o in z)
            {
                MyGoogleChartDataAjax d = new MyGoogleChartDataAjax();
                d.Titolo = dateTime2String(o.Titolo);
                d.Val1 = decimal2String(o.Val1);
                d.Val2 = decimal2String(o.Val2);
                d.Sospeso = o.Sospeso;
                lst.Add(d); 
            }    */
            return lst;
        }
        public List<MyGoogleChartDataAjax> GetTkproPrezzi(int id, DateTime dataDa, DateTime dataA)
        {
            List<MyGoogleChartDataAjax> lst = new List<MyGoogleChartDataAjax>();


            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();

            List<TrackingProdotti_Result> lstret = en.USPT_GET_TRACKING_PRODOTTI(id, dataDa, dataA).ToList<TrackingProdotti_Result>();
            foreach (TrackingProdotti_Result el in lstret)
            {
                MyGoogleChartDataAjax d = new MyGoogleChartDataAjax();
                d.Titolo = dateTime2String(el.TKPROP_DATA_INS);
                d.Val1 = decimal2String(el.TKPROP_COSTO_DELIB);
                d.Val2 = decimal2String(el.TKPROP_COSTO);
                d.Sospeso = el.TKPROP_FLG_BLOCCATO_MAGAZZINO;
                lst.Add(d);
            }
            /*
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
             var z = ( 
                    from tkp in ctx.TKPROP_PREZZI_PRODOTTI_TRK
                    where tkp.TKPROP_PRODOT_ID == id
                    && tkp.TKPROP_DATA_INS >= dataDa
                    && tkp.TKPROP_DATA_INS <= dataA
                    select new
                    {
                        Titolo = tkp.TKPROP_DATA_INS,
                        Val1 = tkp.TKPROP_COSTO_DELIB,
                        Val2 = tkp.TKPROP_COSTO,
                        Sospeso = tkp.TKPROP_FLG_BLOCCATO_MAGAZZINO 
                    }).ToList();

                foreach (var o in z)
                {
                    MyGoogleChartDataAjax d = new MyGoogleChartDataAjax();
                    d.Titolo = dateTime2String(o.Titolo);
                    d.Val1 = decimal2String(o.Val1);
                    d.Val2 = decimal2String(o.Val2);
                    d.Sospeso = o.Sospeso; 
                    lst.Add(d); 
                }    
            */
            return lst;
        }
        public List<MyFaseValorImporto> GetElencoFasiAnalisiSec(int analisi_id)
        {
            List<MyFaseValorImporto> lstFasi = new List<MyFaseValorImporto>();

            List<MyAnalisiPos> lstAnalisi = GetAnalisiPosSec(analisi_id);
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            foreach (MyAnalisiPos pos in lstAnalisi)
            {
                if (pos.AnalisiPos_Fase_id_MASTER.HasValue)
                {
                    int key = pos.AnalisiPos_Fase_id_MASTER.Value;
                    decimal costo = 0;
                    if (pos.AnalisiPos_TotCosto.HasValue)
                        costo = pos.AnalisiPos_TotCosto.Value;
                    if (dic.ContainsKey(key))
                    {
                        decimal val = dic[key];
                        val = val + costo;
                        dic[key] = val;
                    }
                    else
                    {
                        dic.Add(key, costo);
                    }
                }
            }

            foreach (int k in dic.Keys)
            {
                MyFaseValorImporto l = new MyFaseValorImporto();
                MyFase f = GetFase(k);
                l.Fase_ID = f.Fase_ID;
                l.Fase_Desc = f.Fase_Desc;
                l.Fase_Importo = dic[k];
                lstFasi.Add(l);
            }


            return lstFasi;
        }
        public List<MyFaseValorImporto> GetElencoFasiAnalisi(int analisi_id)
        { 
            List<MyFaseValorImporto> lstFasi = new List<MyFaseValorImporto>();

            List<MyAnalisiPos> lstAnalisi = GetAnalisiPos(analisi_id);
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            foreach (MyAnalisiPos pos in lstAnalisi)
            {
                if (pos.AnalisiPos_Fase_id_MASTER.HasValue)
                {
                    int key = pos.AnalisiPos_Fase_id_MASTER.Value;
                    decimal costo = 0;
                    if (pos.AnalisiPos_TotCosto.HasValue)
                        costo = pos.AnalisiPos_TotCosto.Value;
                    if (dic.ContainsKey(key))
                    {
                        decimal val = dic[key];
                        val = val + costo;
                        dic[key] = val;
                    }
                    else
                    {
                        dic.Add(key, costo);
                    }
                }
            }

            foreach (int k in dic.Keys)
            {
                MyFaseValorImporto l = new MyFaseValorImporto();
                MyFase f = GetFase(k);
                l.Fase_ID = f.Fase_ID;
                l.Fase_Desc = f.Fase_Desc;
                l.Fase_Importo = dic[k];
                lstFasi.Add(l);
            }


            return lstFasi;
        }
        public List<MyFaseValorImporto> GetElencoFasiProdotto(int prodot_id)
        {
            List<MyFaseValorImporto> lstFasi = new List<MyFaseValorImporto>();

            List<MyProdottoPos> lstPos =  GetProdottiPos(prodot_id);
            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();

            foreach (MyProdottoPos pos in lstPos)
            {
                if (pos.ProdottoPos_Fase_id_MASTER.HasValue)
                {
                    int key = pos.ProdottoPos_Fase_id_MASTER.Value;
                    decimal costo = 0;
                    if(pos.ProdottoPos_TotCosto.HasValue )
                        costo = pos.ProdottoPos_TotCosto.Value;
                    if (dic.ContainsKey(key))
                    {
                        decimal val = dic[key];
                        val = val + costo;
                        dic[key] = val;
                    }
                    else
                    { 
                        dic.Add (key,costo);
                    }
                }
            }

            foreach (int k in dic.Keys)
            {
                MyFaseValorImporto l = new MyFaseValorImporto();
                MyFase f = GetFase(k);
                l.Fase_ID = f.Fase_ID;
                l.Fase_Desc = f.Fase_Desc;
                l.Fase_Importo = dic[k];
                lstFasi.Add(l);
            }


            return lstFasi;
            
        }
        public List<MyProdottoDistinto> GetistaProdDistinto(int prodot_id,int NumTotPos)
        { 
            List<MyProdottoDistinto>  lst = new List<MyProdottoDistinto> ();
            List<MyProdotto> lstProd =  GetProdotti().Where(z=>z.Prodot_Flg_Interno == true && z.Prodot_ID != prodot_id).ToList<MyProdotto>();
            foreach( MyProdotto  p in lstProd )
            {
                MyProdottoDistinto pd = GetCheckProdDistinto(p.Prodot_ID);
                if(pd.NumTotPosizioni== NumTotPos)
                {
                  lst.Add(pd);  
                }
            }
            return lst;
        }
        public MyProdottoDistinto GetCheckProdDistinto(int prodot_id)
        {
            MyProdottoDistinto pd = new MyProdottoDistinto(prodot_id);
            foreach (MyProdottoPos mpos in this.GetProdottiPos(prodot_id))
            {
                if (mpos.ProdottoPos_FigProf_id.HasValue)
                {
                    pd.ElencoFigProf_id.Add(mpos.ProdottoPos_FigProf_id.Value);
                }
                if (mpos.ProdottoPos_Analisi_id.HasValue)
                {
                    pd.ElencoAnalisi_id.Add(mpos.ProdottoPos_Analisi_id.Value);
                }
                if (mpos.ProdottoPos_Macchinario_id.HasValue)
                {
                    pd.ElencoMacchi_id.Add(mpos.ProdottoPos_Macchinario_id.Value);
                }
                if (mpos.ProdottoPos_Prodotto_id.HasValue)
                {
                    pd.ElencoProdot_id.Add(mpos.ProdottoPos_Prodotto_id.Value);
                }
            }
            return pd;
        }
        public MyMacchinario GetMacchinario(int Macchi_ID)
        {
            return GetElencoMacchinari().Where(z => z.Macchi_ID == Macchi_ID).SingleOrDefault() ;
        }
        public List<MyMacchinario> GetElencoMacchinari(Profili profilo)
        {
            DateTime today = DateTime.Now.Date;
            List<int> elencoGruppiProfilo = new List<int>();
            foreach (Gruppo g in profilo.ElencoGruppi)
            {
                if (!elencoGruppiProfilo.Contains(g.GruppoID))
                    elencoGruppiProfilo.Add(g.GruppoID);
            }
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
             List<MyMacchinario> a = 
                    (
                        from mac in ctx.MACCHI_MACCHINARIO
                        join gru in ctx.GRUREP_GRUPPI_REPARTI 
                        on mac.MACCHI_GRUREP_ID equals gru.GRUREP_ID
                        into gruTable
                        from gru in gruTable.DefaultIfEmpty()
                        where (elencoGruppiProfilo.Contains(gru.GRUREP_ID) )
                        select new 
                        {
                            Macchi_ID = mac.MACCHI_ID ,
                            Macchi_Codice = mac.MACCHI_CODICE ,
                            Macchi_Desc = mac.MACCHI_DESC ,
                            Macchi_Prezzo = mac.MACCHI_PREZZO_UNITARIO ,
                            Macchi_Costo_Manutenzione_Annuo = mac.MACCHI_COSTO_MANUTENZIONE_ANNUALE ,
                            Macchi_Minuti_Anno=mac.MACCHI_MINUTI_ANNO ,
                            Macchi_Valore_Strumentazione = mac.MACCHI_VALORE_STRUMENTAZIONE ,
                            Macchi_Vita_Utile_Anni =mac.MACCHI_VITA_UTILE_ANNI ,
                            Macchi_Grurep_id = mac.MACCHI_GRUREP_ID ,
                            Macchi_Grurep_Desc = gru.GRUREP_DESC 
                        }
                        ).Select(o => new MyMacchinario 
                         {
                             Macchi_ID =o.Macchi_ID,
                             Macchi_Codice =o.Macchi_Codice,
                             Macchi_Desc =o.Macchi_Desc,
                             Macchi_Prezzo =o.Macchi_Prezzo,
                             Macchi_Costo_Manutenzione_Annuo = o.Macchi_Costo_Manutenzione_Annuo ,
                             Macchi_Minuti_Anno = o.Macchi_Minuti_Anno ,
                             Macchi_Valore_Strumentazione = o.Macchi_Valore_Strumentazione,
                             Macchi_Vita_Utile_Anni = o.Macchi_Vita_Utile_Anni ,
                             Macchi_Grurep_id =o.Macchi_Grurep_id ,
                             Macchi_Grurep_Desc = o.Macchi_Grurep_Desc 
                         }).ToList<MyMacchinario>();

            return a;
        }
        public List<MyMacchinario> GetElencoMacchinari()
        {
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<MyMacchinario> a = 
                    (
                        from mac in ctx.MACCHI_MACCHINARIO
                        join gru in ctx.GRUREP_GRUPPI_REPARTI 
                        on mac.MACCHI_GRUREP_ID equals gru.GRUREP_ID
                        into gruTable
                        from gru in gruTable.DefaultIfEmpty()
                        select new 
                        {
                            Macchi_ID = mac.MACCHI_ID ,
                            Macchi_Codice = mac.MACCHI_CODICE ,
                            Macchi_Desc = mac.MACCHI_DESC ,
                            Macchi_Prezzo = mac.MACCHI_PREZZO_UNITARIO ,
                            Macchi_Costo_Manutenzione_Annuo = mac.MACCHI_COSTO_MANUTENZIONE_ANNUALE ,
                            Macchi_Minuti_Anno=mac.MACCHI_MINUTI_ANNO ,
                            Macchi_Valore_Strumentazione = mac.MACCHI_VALORE_STRUMENTAZIONE ,
                            Macchi_Vita_Utile_Anni =mac.MACCHI_VITA_UTILE_ANNI ,
                            Macchi_Grurep_id = mac.MACCHI_GRUREP_ID ,
                            Macchi_Grurep_Desc = gru.GRUREP_DESC 
                        }
                        ).Select(o => new MyMacchinario 
                         {
                             Macchi_ID =o.Macchi_ID,
                             Macchi_Codice =o.Macchi_Codice,
                             Macchi_Desc =o.Macchi_Desc,
                             Macchi_Prezzo =o.Macchi_Prezzo,
                             Macchi_Costo_Manutenzione_Annuo = o.Macchi_Costo_Manutenzione_Annuo ,
                             Macchi_Minuti_Anno = o.Macchi_Minuti_Anno ,
                             Macchi_Valore_Strumentazione = o.Macchi_Valore_Strumentazione,
                             Macchi_Vita_Utile_Anni = o.Macchi_Vita_Utile_Anni ,
                             Macchi_Grurep_id =o.Macchi_Grurep_id ,
                             Macchi_Grurep_Desc = o.Macchi_Grurep_Desc 
                         }).ToList<MyMacchinario>();
            
            return a;
        }
        public List<MyUdM> GetElencoUDMVisible()
        {
            return GetElencoUDM().Where(z => z.Unimis_Visible == true).ToList(); 
        }
        public List<MyUdM> GetElencoUDM()
        {
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<MyUdM> a =
                (
                    from udm in ctx.T_UNIMIS_UNITA_MISURA 
                    join gru in ctx.T_GRUDMI_GRUPPI_UNITA_DI_MISURA 
                    on udm.T_UNIMIS_GRUDMI_ID equals gru.T_GRUDMI_ID 

                    select new
                    {
                        Unimis_Id = udm.T_UNIMIS_ID,
                        Unimis_Codice = udm.T_UNIMIS_CODICE ,
                        Unimis_Desc =  udm.T_UNIMIS_DESC,
                        Unimis_Grudmi_id = udm.T_UNIMIS_GRUDMI_ID,
                        Unimis_Conversione = udm.T_UNIMIS_CONVERSIONE,
                        Unidmi_Default = udm.T_UNIDMI_DEFAULT,
                        Grudmi_Codice = gru.T_GRUDMI_CODICE,
                        Grudmi_Desc = gru.T_GRUDMI_DESC,
                        Unimis_Visible = udm.T_UNIDMI_VISIBLE 
                    }
                 ).Select(o => new MyUdM
                 {
                     Unimis_Id = o.Unimis_Id,
                     Unimis_Codice = o.Unimis_Codice,
                     Unimis_Desc = o.Unimis_Desc,
                     Unimis_Grudmi_id = o.Unimis_Grudmi_id,
                     Unimis_Conversione = o.Unimis_Conversione,
                     Unidmi_Default = o.Unidmi_Default,
                     Grudmi_Codice = o.Grudmi_Codice,
                     Grudmi_Desc = o.Grudmi_Desc,
                     Unimis_Visible = o.Unimis_Visible 
                 }
                ).ToList<MyUdM>();
            return a;
        }

        public List<MyGruppoUdm>GetElencoGruppiUDM()
        {
             IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
             List<MyGruppoUdm> a =
                 (
                     from gru in ctx.T_GRUDMI_GRUPPI_UNITA_DI_MISURA
                     select new
                     {
                         Grudmi_Id = gru.T_GRUDMI_ID,
                         Grudmi_Codice = gru.T_GRUDMI_CODICE,
                         Grudmi_Desc = gru.T_GRUDMI_DESC
                     }
                  ).Select(o => new MyGruppoUdm 
                    {
                     Grudmi_Id = o.Grudmi_Id,
                     Grudmi_Codice = o.Grudmi_Codice,
                     Grudmi_Desc = o.Grudmi_Desc
                    }
                 ).ToList<MyGruppoUdm>();
                return a;
        }

        public List<TrackingProdotti> GetTrackingProdotto(int prodotto_id)
        { 
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<TrackingProdotti> a =
                (
                    from tck in ctx.TRKPRO_PRODOTTO_TRACKING.Where(z => z.TRKPRO_PRODOT_ID == prodotto_id)
                    join ts in ctx.T_STAPRO_STATO_PRODOTTO
                    on tck.TRKPRO_T_STAPRO_ID equals ts.T_STAPRO_ID
                    join ut in ctx.UTENTE
                    on tck.TRKPRO_UTENTE_ID equals ut.UTENTE_ID
                    into utTable
                    from ut in utTable.DefaultIfEmpty()
                    join utP in ctx.UTENTE
                    on tck.TRKPRO_PRODOT_UTENTE_ID equals utP.UTENTE_ID
                    into utPTable
                    from utP in utPTable.DefaultIfEmpty()
                    join tudm in ctx.T_UNIMIS_UNITA_MISURA
                    on tck.TRKPRO_T_UNIMIS_ID  equals tudm.T_UNIMIS_ID
                    into tudmTable
                    from tudm in tudmTable.DefaultIfEmpty()

                    join gr in ctx.GRUREP_GRUPPI_REPARTI 
                    on tck.TRKPRO_REPARTO_GRUREP_ID equals gr.GRUREP_ID 
                    into grTable
                    from gr in grTable.DefaultIfEmpty ()
                    

                    select new
                    {
                        Trkpro_Codice = tck.TRKPRO_CODICE ,
                        Trkpro_Costounitario = tck.TRKPRO_COSTOUNITARIO ,
                        Trkpro_Costounitario_delibe = tck.TRKPRO_COSTOUNITARIO_DELIBE ,
                        Trkpro_Data_ins = tck.TRKPRO_DATA_INS ,
                        Trkpro_Desc = tck.TRKPRO_DESC ,
                        Trkpro_Dim_lotto = tck.TRKPRO_DIM_LOTTO ,
                        Trkpro_Flg_interno = tck.TRKPRO_FLG_INTERNO ,
                        Trkpro_Id = tck.TRKPRO_ID ,
                        Trkpro_Nr_camp_qualita = tck.TRKPRO_NR_CAMP_QUALITA ,
                        Trkpro_Prodot_id = tck.TRKPRO_PRODOT_ID ,
                        Trkpro_Prodot_Utente_Cognome = utP.UTENTE_COGNOME, 
                        Trkpro_Prodot_utente_id = tck.TRKPRO_PRODOT_UTENTE_ID ,
                        Trkpro_Prodot_Utente_Nome = utP.UTENTE_NOME,
                        Trkpro_Reparto_codice = gr.GRUREP_CODICE,
                        Trkpro_Reparto_grurep_id = tck.TRKPRO_REPARTO_GRUREP_ID ,
                        Trkpro_T_Stapro_codice = ts.T_STAPRO_CODICE,
                        Trkpro_T_Stapro_desc = ts.T_STAPRO_DESCRIZIONE,
                        Trkpro_T_Stapro_id = tck.TRKPRO_T_STAPRO_ID ,
                        Trkpro_T_Unimis_Codice = tudm.T_UNIMIS_CODICE,
                        Trkpro_T_Unimis_desc = tudm.T_UNIMIS_DESC ,
                        Trkpro_T_Unimis_id = tck.TRKPRO_T_UNIMIS_ID ,
                        Trkpro_Utente_Cognome = ut.UTENTE_COGNOME,
                        Trkpro_Utente_id = tck.TRKPRO_UTENTE_ID ,
                        Trkpro_Utente_Nome = ut.UTENTE_NOME
                    }
                    ).Select(o => new TrackingProdotti 
                    {
                        Trkpro_Codice = o.Trkpro_Codice ,
                        Trkpro_Costounitario = o.Trkpro_Costounitario,
                        Trkpro_Costounitario_delibe = o.Trkpro_Costounitario_delibe ,
                        Trkpro_Data_ins = o.Trkpro_Data_ins ,
                        Trkpro_Desc = o.Trkpro_Desc ,
                        Trkpro_Dim_lotto = o.Trkpro_Dim_lotto,
                        Trkpro_Flg_interno = o.Trkpro_Flg_interno ,
                        Trkpro_Id = o.Trkpro_Id ,
                        Trkpro_Nr_camp_qualita = o.Trkpro_Nr_camp_qualita ,
                        Trkpro_Prodot_id = o.Trkpro_Prodot_id ,
                        Trkpro_Prodot_Utente_Cognome = o.Trkpro_Prodot_Utente_Cognome ,
                        Trkpro_Prodot_utente_id = o.Trkpro_Prodot_utente_id ,
                        Trkpro_Prodot_Utente_Nome = o.Trkpro_Prodot_Utente_Nome ,
                        Trkpro_Reparto_codice = o.Trkpro_Reparto_codice ,
                        Trkpro_Reparto_grurep_id = o.Trkpro_Reparto_grurep_id ,
                        Trkpro_T_Stapro_codice = o.Trkpro_T_Stapro_codice ,
                        Trkpro_T_Stapro_desc = o.Trkpro_T_Stapro_desc ,
                        Trkpro_T_Stapro_id = o.Trkpro_T_Stapro_id ,
                        Trkpro_T_Unimis_Codice = o.Trkpro_T_Unimis_Codice ,
                        Trkpro_T_Unimis_desc = o.Trkpro_T_Unimis_desc ,
                        Trkpro_T_Unimis_id = o.Trkpro_T_Unimis_id,
                        Trkpro_Utente_Cognome = o.Trkpro_Utente_Cognome ,
                        Trkpro_Utente_id = o.Trkpro_Utente_id ,
                        Trkpro_Utente_Nome = o.Trkpro_Utente_Nome 



                    }
                ).ToList<TrackingProdotti>();
            return a;
        }
        public List<TrackingFigProf> GetTrackingFigProf(int Figprof_id)
        { 
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<TrackingFigProf> a =
                (
                    from tck in ctx.TRKFIG_FIGPRO_TRACKING.Where(z => z.TRKFIG_FIGPRO_ID == Figprof_id)
                    join ut in ctx.UTENTE
                    on tck.TRKFIG_UTENTE_ID equals ut.UTENTE_ID
                    into utTable
                    from ut in utTable.DefaultIfEmpty()
                    select new
                    {
                        Trkfig_id = tck.TRKFIG_ID,
                        Trkfig_FigPro_id = tck.TRKFIG_FIGPRO_ID ,
                        Trkfig_DataIns = tck.TRKFIG_DATA_INS,
                        Trkfig_Utente_id = tck.TRKFIG_UTENTE_ID ,
                        Trkfig_Utente_Cognome = ut.UTENTE_COGNOME ,
                        Trkfig_Utente_Nome= ut.UTENTE_NOME,
                        Trkfig_Costo = tck.TRKFIG_FIGPRO_COSTO
                    }).Select(o => new TrackingFigProf 
                    {
                        Trkfig_id = o.Trkfig_id,
                        Trkfig_FigPro_id = o.Trkfig_FigPro_id,
                        Trkfig_DataIns = o.Trkfig_DataIns ,
                        Trkfig_Costo =o.Trkfig_Costo,
                        Trkfig_Utente_id = o.Trkfig_Utente_id ,
                        Trkfig_Utente_Cognome  =o.Trkfig_Utente_Cognome,
                        Trkfig_Utente_Nome = o.Trkfig_Utente_Nome
                    }
                ).ToList<TrackingFigProf>();
            return a;
        }
        public List<TrackingAnalisi> GetTrackingAnalisi(int analisi_id)
        { 
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<TrackingAnalisi> a =
                (
                    from tck in ctx.TRKVAL_VALORIZZAZIONI_TRACKING.Where(z => z.TRKVAL_VALORI_ID == analisi_id)
                    join ts in ctx.T_STAVAL_STATO_VALORIZZAZIONE
                    on tck.TRKVAL_T_STAVAL_ID equals ts.T_STAVAL_ID
                    join ut in ctx.UTENTE
                    on tck.TRKVAL_UTENTE_ID equals ut.UTENTE_ID
                    into utTable
                    from ut in utTable.DefaultIfEmpty()
                    join utV in ctx.UTENTE
                    on tck.TRKVAL_VALORI_UTENTE_ID equals utV.UTENTE_ID
                    into utVTable
                    from utV in utVTable.DefaultIfEmpty()
                    select new
                    {
                        Trkana_id = tck.TRKVAL_ID,
                        Trkana_Analisi_id = tck.TRKVAL_VALORI_ID,
                        Trkana_DataIns = tck.TRKVAL_DATA_INS,
                        Trkana_Utente_id = tck.TRKVAL_UTENTE_ID,
                        Trkana_Utente_Nome = ut.UTENTE_NOME,
                        Trkana_Utente_Cognome = ut.UTENTE_COGNOME,
                        Trkana_Valori_Utente_id = tck.TRKVAL_VALORI_UTENTE_ID,
                        Trkana_Valori_Utente_Nome = utV.UTENTE_NOME,
                        Trkana_Valori_Utente_Cognome = utV.UTENTE_COGNOME,
                        Trkana_DimLotto_id = tck.TRKVAL_DIM_LOTTO,
                        Trkana_CostoTot_id = tck.TRKVAL_COSTO_TOT,
                        Trkana_FlgPonderazione = tck.TRKVAL_FLG_PONDERAZIONE,
                        Trkana_PesoPositivo = tck.TRKVAL_PESO_POSITIVO,
                        Trkana_T_Staval_id = tck.TRKVAL_T_STAVAL_ID,
                        Trkana_T_Staval_codice = ts.T_STAVAL_CODICE,
                        Trkana_T_Staval_desc = ts.T_STAVAL_DESC,
                        Trkana_Codice_Intermedio =tck.TRKVAL_CODICE_INTERMEDIO ,
                        Trkana_Desc_Intermedio = tck.TRKVAL_DESC_INTERMEDIO 
                    }).Select(o => new TrackingAnalisi 
                    {
                        Trkana_id = o.Trkana_id,
                        Trkana_Analisi_id = o.Trkana_Analisi_id,
                        Trkana_DataIns = o.Trkana_DataIns,
                        Trkana_Utente_id = o.Trkana_Utente_id,
                        Trkana_Utente_Nome = o.Trkana_Utente_Nome,
                        Trkana_Utente_Cognome = o.Trkana_Utente_Cognome,
                        Trkana_Valori_Utente_id = o.Trkana_Valori_Utente_id,
                        Trkana_Valori_Utente_Nome = o.Trkana_Valori_Utente_Nome,
                        Trkana_Valori_Utente_Cognome = o.Trkana_Valori_Utente_Cognome,
                        Trkana_DimLotto_id = o.Trkana_DimLotto_id,
                        Trkana_CostoTot_id = o.Trkana_CostoTot_id,
                        Trkana_FlgPonderazione = o.Trkana_FlgPonderazione,
                        Trkana_PesoPositivo = o.Trkana_PesoPositivo,
                        Trkana_T_Staval_id = o.Trkana_T_Staval_id,
                        Trkana_T_Staval_codice = o.Trkana_T_Staval_codice,
                        Trkana_T_Staval_desc = o.Trkana_T_Staval_desc,
                        Trkana_Codice_Intermedio = o.Trkana_Codice_Intermedio ,
                        Trkana_Desc_Intermedio = o.Trkana_Desc_Intermedio 
                    }
                ).ToList<TrackingAnalisi>();
            return a;
        }
        public List<MyGrurep> GetRepartiGruppiPerProfilo(Profili profilo)
        {

            List<int > elencoGruppiProfilo = new List<int> ();
            foreach(Gruppo g in profilo.ElencoGruppi )
            {
                if(!elencoGruppiProfilo.Contains (g.GruppoID))
                    elencoGruppiProfilo.Add(g.GruppoID);
            }
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
             List<MyGrurep> a = (from gr in context.GRUREP_GRUPPI_REPARTI
                                 where elencoGruppiProfilo.Contains(gr.GRUREP_ID)
                                select new
                                {
                                    Grurep_ID = gr.GRUREP_ID,
                                    Grurep_Codice = gr.GRUREP_CODICE,
                                    Grurep_Desc = gr.GRUREP_DESC,
                                    Grurep_DescEstesa = gr.GRUREP_DESC_ESTESA,
                                    Grurep_Flg_Reparto = gr.GRUREP_FLG_REPARTO,
                                    Grurep_Cost_Ind =gr.GRUREP_COST_IND ,
                                    Grurep_PrezzoUnit_Accettazione = gr.GRUREP_PREZZO_UNIT_ACCET
                                }
                                 ).Select(o => new MyGrurep
                                 {
                                     Grurep_ID = o.Grurep_ID,
                                     Grurep_Codice = o.Grurep_Codice,
                                     Grurep_Desc = o.Grurep_Desc,
                                     Grurep_DescEstesa = o.Grurep_DescEstesa,
                                     Grurep_Flg_Reparto = o.Grurep_Flg_Reparto,
                                     Grurep_Cost_Ind = o.Grurep_Cost_Ind ,
                                     Grurep_PrezzoUnit_Accettazione  = o.Grurep_PrezzoUnit_Accettazione 
                                 }).ToList<MyGrurep>();
            return a;
        }
        public List<MyGrurep> GetRepartiGruppi()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyGrurep> a = (from gr in context.GRUREP_GRUPPI_REPARTI
                                select new
                                {
                                    Grurep_ID = gr.GRUREP_ID,
                                    Grurep_Codice = gr.GRUREP_CODICE,
                                    Grurep_Desc = gr.GRUREP_DESC,
                                    Grurep_DescEstesa = gr.GRUREP_DESC_ESTESA,
                                    Grurep_Flg_Reparto = gr.GRUREP_FLG_REPARTO,
                                    Grurep_Cost_Ind = gr.GRUREP_COST_IND ,
                                    Grurep_PrezzoUnit_Accettazione= gr.GRUREP_PREZZO_UNIT_ACCET
                                }
                                 ).Select(o => new MyGrurep
                                 {
                                     Grurep_ID = o.Grurep_ID,
                                     Grurep_Codice = o.Grurep_Codice,
                                     Grurep_Desc = o.Grurep_Desc,
                                     Grurep_DescEstesa = o.Grurep_DescEstesa,
                                     Grurep_Flg_Reparto = o.Grurep_Flg_Reparto,
                                     Grurep_Cost_Ind = o.Grurep_Cost_Ind ,
                                     Grurep_PrezzoUnit_Accettazione = o.Grurep_PrezzoUnit_Accettazione 
                                 }).ToList<MyGrurep>();
            return a;
        } 
        public List<MyGrurep> GetReparti()
        { 
            return GetRepartiGruppi().Where(z => z.Grurep_Flg_Reparto == true).ToList<MyGrurep>();
        }                
        public List<MyGrurep> GetGruppi()
        { 
            return GetRepartiGruppi().Where(z => z.Grurep_Flg_Reparto == false).ToList<MyGrurep>();
        }
        public MyGrurep GetRepartoGruppo(int Grurep_id)
        {
            return GetRepartiGruppi().Where(z => z.Grurep_ID == Grurep_id).SingleOrDefault();
        } 
        public MyGrurep GetGruppo(int Grurep_id)
        {
            return GetGruppi().Where(z => z.Grurep_ID == Grurep_id).SingleOrDefault();
        }
        public List<MyUtente_Profilo> GetUtentiProfili(int Grurep_id)
        {
            List<MyUtente_Profilo> dic = new List<MyUtente_Profilo>();
            List<MyUtenti_Profili_Gruppi> listaUTPROF=GetUtenti_Profili_Gruppi().Where(z => z.M_Utprgr_Grurep_Id == Grurep_id).ToList<MyUtenti_Profili_Gruppi>();
            foreach (MyUtenti_Profili_Gruppi utprof in listaUTPROF)
            {
                MyUtente ut = GetUtente(utprof.M_Utprgr_Utente_Id);
                MyUtente_Profilo utp = new MyUtente_Profilo();
                utp.Utente_Cognome = ut.Utente_Cognome;
                utp.Utente_Nome = ut.Utente_Nome;
                utp.Utente_Email = ut.Utente_Email;
                utp.Utente_ID = ut.Utente_ID;
                utp.Utente_Lock = ut.Utente_Lock;
                utp.Utente_User = ut.Utente_User;
                utp.Utente_Profilo_ID = utprof.M_Utprgr_Profil_Id;
                utp.Utente_Flag_Principale = utprof.M_Utprgr_Flg_Principale;
                utp.Utente_Profilo_Descrizione = utprof.Profilo_desc;
                
                dic.Add(utp);
                
            }
            return dic;
        }

        public MyGrurep GetReparto(int Grurep_id)
        {
            return GetReparti().Where(z => z.Grurep_ID == Grurep_id).SingleOrDefault();
        }
         
        //public List<MyAttivita> GetAttivita(int fase_id)
        //{
        //    return GetAttivita().Where(z => z.Attivita_Fase_ID == fase_id).ToList<MyAttivita>() ;
        //}
        //public List<MyAttivita> GetAttivita()
        //{
        //    IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
        //    List<MyAttivita> a = (from att in context.ATTIVI_ATTIVITA 
        //                      join fs in context.FASE 
        //                      on att.ATTIVI_FASE_ID equals fs.FASE_ID 
                               
        //                      select new
        //                      {
                                  
        //                          Attivita_ID= att.ATTIVI_ID ,
        //                          Attivita_Codice= att.ATTIVI_CODICE ,
        //                          Attivita_Desc = att.ATTIVI_DESC ,
        //                          Attivita_FlgObbligatoria= att.ATTIVI_FLG_OBBLIGATORIA ,
        //                          Attivita_Fase_ID = att.ATTIVI_FASE_ID ,
        //                          Attivita_Fase_Desc = fs.FASE_DESC
        //                      }
        //                         ).Select(o => new MyAttivita
        //                         {
        //                             Attivita_ID = o.Attivita_ID,
        //                             Attivita_Codice = o.Attivita_Codice,
        //                             Attivita_Desc = o.Attivita_Desc,
        //                             Attivita_FlgObbligatoria = o.Attivita_FlgObbligatoria ,
        //                             Attivita_Fase_ID = o.Attivita_Fase_ID ,
        //                             Attivita_Fase_Desc = o.Attivita_Fase_Desc 
        //                         }).ToList<MyAttivita>();
        //    return a;
        //}
        public MyFigProf GetFigProfDaFigProf_ID(int FigProf_id)
        {
            return GetListFigProf().Where(z => z.FigProf_ID == FigProf_id).SingleOrDefault();
        }
        public List<MyFigProf> GetFigProf(int attivi_id)
        {
            return GetFigProf().Where(z=>z.FigProf_Attivi_ID == attivi_id).ToList <MyFigProf>();
        }
        public List<MyFigProf> GetListFigProf()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities(); 
            List<MyFigProf> a = (from fp in context.FIGPRO_FIGURA_PROFESSIONALE
                                 select new
                                 {
                                     FigProf_ID = fp.FIGPRO_ID,
                                     FigProf_Codice = fp.FIGPRO_CODICE,
                                     FigProf_Desc = fp.FIGPRO_DESC,
                                     FigProf_Costo = fp.FIGPRO_COSTO,
                                     FigProf_Attivi_ID = 0
                                 }
                                 ).Select(o => new MyFigProf
                                 {
                                     FigProf_ID = o.FigProf_ID,
                                     FigProf_Codice = o.FigProf_Codice,
                                     FigProf_Desc = o.FigProf_Desc,
                                     FigProf_Costo = o.FigProf_Costo,
                                     FigProf_Attivi_ID = o.FigProf_Attivi_ID

                                 }).ToList<MyFigProf>();
            return a;
        }

         public List<MyFigProf> GetFigProf()
        {   IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyFigProf> a = (from fp in context.FIGPRO_FIGURA_PROFESSIONALE
                                 join m in context.M_FIGATT_FIGURAPROF_ATTIVITA
                                 on fp.FIGPRO_ID equals m.M_FIGATT_FIGPRO_ID 
                              select new
                              {
                                  FigProf_ID = fp.FIGPRO_ID,
                                  FigProf_Codice = fp.FIGPRO_CODICE ,
                                  FigProf_Desc = fp.FIGPRO_DESC,
                                  FigProf_Costo = fp.FIGPRO_COSTO ,
                                  FigProf_Attivi_ID= m.M_FIGATT_FASE_ID 
                              }
                                 ).Select(o => new MyFigProf
                                 {
                                     FigProf_ID = o.FigProf_ID,
                                     FigProf_Codice = o.FigProf_Codice,
                                     FigProf_Desc =o.FigProf_Desc,
                                     FigProf_Costo = o.FigProf_Costo,
                                     FigProf_Attivi_ID = o.FigProf_Attivi_ID 

                                 }).ToList<MyFigProf>();
            return a;
        }
         public MyFase GetFase(int fase_id)
         {
             return GetFasi().Where(z => z.Fase_ID == fase_id).SingleOrDefault() ;
         }
         public List<MyFase> GetFasiDelProdotto(int prodotto_id)
         {
             IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();

             
             MyProdotto p = GetProdotti(prodotto_id);
             int codiceReparto = 0;
             if (p.Prodot_Reparto_ID.HasValue)
             {
                 codiceReparto = p.Prodot_Reparto_ID.Value;
             }
             List<MyFase> aMaster = getFasiMaster(codiceReparto);
             List<MyFase> a = new List<MyFase>();
             foreach (MyFase curr in aMaster)
             {
                 a.Add(curr);

                 List<MyFase> aSlave = getSottoFasi(curr, codiceReparto);
                 a.AddRange(aSlave);
             }

             return a;
         }
         public List<MyFase> GetFasiDelAnalisi(int analisi_id)
         {
             IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
             
             MyAnalisi an = GetAnalisi(analisi_id);
             int codiceGruppo = 0;
             if (an.Analisi_Gruppo_id.HasValue)
                 codiceGruppo = an.Analisi_Gruppo_id.Value;
             else
             {
                 //per gli intermedi di prodotto
                 if (an.Analisi_Reparto_id.HasValue)
                     codiceGruppo = an.Analisi_Reparto_id.Value;
             }
             List<MyFase> aMaster = getFasiMaster(codiceGruppo);
             List<MyFase> a = new List<MyFase>();
             foreach (MyFase curr in aMaster)
             {
                 a.Add(curr);

                 List<MyFase> aSlave = getSottoFasi(curr, codiceGruppo);
                 a.AddRange(aSlave);
             }

             return a;
         }
         private List<MyFase> getFasiMaster(int? grurep_id)
         {
             List<MyFase> aMaster = new List<MyFase>();
             IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
             if (!grurep_id.HasValue)
             {
                aMaster = (from fs in context.FASE
                            join gru in context.GRUREP_GRUPPI_REPARTI
                            on fs.FASE_GRUREP_ID equals gru.GRUREP_ID
                            into grutable
                            from gru in grutable.DefaultIfEmpty()

                            where fs.FASE_FASE_ID == null
                            orderby fs.FASE_DESC
                            select new
                            {
                                Fase_ID = fs.FASE_ID,
                                Fase_Codice = fs.FASE_CODICE,
                                Fase_Desc = fs.FASE_DESC,
                                Fase_Fase_ID = fs.FASE_FASE_ID,
                                Fase_Grurep_ID = fs.FASE_GRUREP_ID,
                                Fase_Grurep_desc = gru.GRUREP_DESC,
                                Fase_Desc_Clear = fs.FASE_DESC
                            }
                        ).Select(o => new MyFase
                        {
                            Fase_ID = o.Fase_ID,
                            Fase_Codice = o.Fase_Codice,
                            Fase_Desc = !o.Fase_Fase_ID.HasValue ? o.Fase_Desc : "   " + o.Fase_Desc,  // Attenzione gli spazi sono stati inseriti con Alt+0160 questo perche' il carattere "spazio" non veniva renderizzato
                            Fase_Fase_ID = o.Fase_Fase_ID,
                            Fase_Grurep_ID = o.Fase_Grurep_ID,
                            Fase_Grurep_desc = o.Fase_Grurep_desc,
                            Fase_Desc_Clear = o.Fase_Desc_Clear
                        }).ToList<MyFase>();
             }
             else
             {
                 aMaster = (from fs in context.FASE
                            join gru in context.GRUREP_GRUPPI_REPARTI
                            on fs.FASE_GRUREP_ID equals gru.GRUREP_ID
                            into grutable
                            from gru in grutable.DefaultIfEmpty()

                            where fs.FASE_FASE_ID == null
                            && (fs.FASE_GRUREP_ID == grurep_id.Value || fs.FASE_GRUREP_ID == null)
                            orderby fs.FASE_DESC
                            select new
                            {
                                Fase_ID = fs.FASE_ID,
                                Fase_Codice = fs.FASE_CODICE,
                                Fase_Desc = fs.FASE_DESC,
                                Fase_Fase_ID = fs.FASE_FASE_ID,
                                Fase_Grurep_ID = fs.FASE_GRUREP_ID,
                                Fase_Grurep_desc = gru.GRUREP_DESC,
                                Fase_Desc_Clear = fs.FASE_DESC
                            }
                            ).Select(o => new MyFase
                            {
                                Fase_ID = o.Fase_ID,
                                Fase_Codice = o.Fase_Codice,
                                Fase_Desc = !o.Fase_Fase_ID.HasValue ? o.Fase_Desc : "   " + o.Fase_Desc,  // Attenzione gli spazi sono stati inseriti con Alt+0160 questo perche' il carattere "spazio" non veniva renderizzato
                                Fase_Fase_ID = o.Fase_Fase_ID,
                                Fase_Grurep_ID = o.Fase_Grurep_ID,
                                Fase_Grurep_desc = o.Fase_Grurep_desc,
                                Fase_Desc_Clear = o.Fase_Desc_Clear
                            }).ToList<MyFase>();
             }
             return aMaster;
         }
         private List<MyFase> getSottoFasi(MyFase faseMaster, int? grurep_id)
         {
             List<MyFase> aSlave = new List<MyFase>();
             IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
             if (!grurep_id.HasValue)
             {
                 aSlave = (from fs in context.FASE
                           join gru in context.GRUREP_GRUPPI_REPARTI
                           on fs.FASE_GRUREP_ID equals gru.GRUREP_ID
                           into grutable
                           from gru in grutable.DefaultIfEmpty()
                           where fs.FASE_FASE_ID == faseMaster.Fase_ID
                           orderby fs.FASE_DESC
                            select new
                            {
                                Fase_ID = fs.FASE_ID,
                                Fase_Codice = fs.FASE_CODICE,
                                Fase_Desc = fs.FASE_DESC,
                                Fase_Fase_ID = fs.FASE_FASE_ID,
                                Fase_Grurep_ID = fs.FASE_GRUREP_ID,
                                Fase_Grurep_desc = gru.GRUREP_DESC ,
                                Fase_Desc_Clear =  fs.FASE_DESC
                            }
                        ).Select(o => new MyFase
                        {
                            Fase_ID = o.Fase_ID,
                            Fase_Codice = o.Fase_Codice,
                            Fase_Desc = !o.Fase_Fase_ID.HasValue ? o.Fase_Desc : "   " + o.Fase_Desc,  // Attenzione gli spazi sono stati inseriti con Alt+0160 questo perche' il carattere "spazio" non veniva renderizzato
                            Fase_Fase_ID = o.Fase_Fase_ID,
                            Fase_Grurep_ID = o.Fase_Grurep_ID,
                            Fase_Grurep_desc = o.Fase_Grurep_desc,
                            Fase_Desc_Clear = o.Fase_Desc_Clear 
                        }).ToList<MyFase>();
             }
             else
             {
                 aSlave = (from fs in context.FASE
                           join gru in context.GRUREP_GRUPPI_REPARTI 
                           on fs.FASE_GRUREP_ID equals gru.GRUREP_ID
                           into grutable
                           from gru in grutable.DefaultIfEmpty()
                           where fs.FASE_FASE_ID == faseMaster.Fase_ID &&  ( fs.FASE_GRUREP_ID == grurep_id.Value  || fs.FASE_GRUREP_ID ==null)
                           orderby fs.FASE_DESC
                           select new
                           {
                               Fase_ID = fs.FASE_ID,
                               Fase_Codice = fs.FASE_CODICE,
                               Fase_Desc = fs.FASE_DESC,
                               Fase_Fase_ID = fs.FASE_FASE_ID,
                               Fase_Grurep_ID = fs.FASE_GRUREP_ID,
                               Fase_Grurep_desc = gru.GRUREP_DESC ,
                               Fase_Desc_Clear = fs.FASE_DESC
                           }
                      ).Select(o => new MyFase
                      {
                          Fase_ID = o.Fase_ID,
                          Fase_Codice = o.Fase_Codice,
                          Fase_Desc = !o.Fase_Fase_ID.HasValue ? o.Fase_Desc : "   " + o.Fase_Desc,  // Attenzione gli spazi sono stati inseriti con Alt+0160 questo perche' il carattere "spazio" non veniva renderizzato
                          Fase_Fase_ID = o.Fase_Fase_ID,
                          Fase_Grurep_ID = o.Fase_Grurep_ID,
                          Fase_Grurep_desc = o.Fase_Grurep_desc,
                          Fase_Desc_Clear = o.Fase_Desc_Clear
                      }).ToList<MyFase>();
             }
             return aSlave;
         }
        public List<MyFase> GetFasi()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();

            List<MyFase> aMaster = getFasiMaster(null);
            List<MyFase> a = new List<MyFase> ();
            foreach (MyFase curr in aMaster)
            {
                a.Add(curr);

                List<MyFase> aSlave = getSottoFasi(curr,null);
                a.AddRange(aSlave);
            }

            return a;
        }

        public List<MyFiguraProfessionale> GetFiguraProf()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyFiguraProfessionale> a = (from fp in context.FIGPRO_FIGURA_PROFESSIONALE
                              select new
                              {
                                  Figpro_ID = fp.FIGPRO_ID,
                                  Figpro_Codice = fp.FIGPRO_CODICE,
                                  Figpro_Desc = fp.FIGPRO_DESC,
                                  Figpro_Costo = fp.FIGPRO_COSTO
                              }
                                 ).Select(o => new MyFiguraProfessionale
                                 {
                                     Figpro_Id = o.Figpro_ID,
                                     Figpro_Codice = o.Figpro_Codice,
                                     Figpro_Desc = o.Figpro_Desc,
                                     Figpro_Costo = o.Figpro_Costo
                                 }).ToList<MyFiguraProfessionale>();
            return a;
        }




        public List<MyUtenti_Profili_Gruppi> GetUtenti_Profili_Gruppi()
        { 
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();

            List<MyProfilo> LstElencoProfili = GetElencoProfili();
            List<MyUtenti_Profili_Gruppi> a = (from m in context.M_UTPRGR_UTENTI_PROFILI_GRUPPI
                                   join p in context.PROFIL_PROFILI
                                   on m.M_UTPRGR_PROFIL_ID equals p.PROFIL_ID
                                   into ptable
                                   from p in ptable.DefaultIfEmpty()
                                   join g in context.GRUREP_GRUPPI_REPARTI
                                   on m.M_UTPRGR_GRUREP_ID equals g.GRUREP_ID
                                   into gtable
                                   from g in gtable.DefaultIfEmpty()

                                   select new
                                   {
                                       M_Utprgr_Id = m.M_UTPRGR_ID,
                                       M_Utprgr_Utente_Id = m.M_UTPRGR_UTENTE_ID,
                                       M_Utprgr_Profil_Id = m.M_UTPRGR_PROFIL_ID,
                                       M_Utprgr_Grurep_Id = m.M_UTPRGR_GRUREP_ID,
                                       M_Utprgr_Flg_Principale = m.M_UTPRGR_FLG_PRINCIPALE,
                                       Profilo_desc = p != null ? p.PROFIL_DESC : null,
                                       Gruppo_desc = g != null ? g.GRUREP_DESC : null}
          ).Select(o => new MyUtenti_Profili_Gruppi
          {
              M_Utprgr_Id = o.M_Utprgr_Id,
              M_Utprgr_Utente_Id = o.M_Utprgr_Utente_Id,
              M_Utprgr_Profil_Id = o.M_Utprgr_Profil_Id,
              M_Utprgr_Grurep_Id = o.M_Utprgr_Grurep_Id,
              M_Utprgr_Flg_Principale = o.M_Utprgr_Flg_Principale,
              Profilo_desc = o.Profilo_desc,
              Gruppo_desc = o.Gruppo_desc

          }).ToList<MyUtenti_Profili_Gruppi>();
            foreach (MyUtenti_Profili_Gruppi mut in a)
            {
               // mut.ListaProfili = GetElencoProfili();
                mut.ListaProfili = LstElencoProfili;
            }
            return a;
        }
        public List<MyProfilo> GetElencoProfili()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
             List<MyProfilo> a = (from prof in context.PROFIL_PROFILI
                                   select new
                                    {Profilo_ID= prof.PROFIL_ID ,
                                      Profilo_Codice=  prof.PROFIL_CODICE ,
                                      Profilo_Descrizione=  prof.PROFIL_DESC 
                                    }
                                    ).Select (o=>new MyProfilo
                                    {
                                        Profilo_ID =o.Profilo_ID ,
                                        Profilo_Codice = o.Profilo_Codice ,
                                        Profilo_Descrizione =o.Profilo_Descrizione
                                    }).ToList<MyProfilo>();
              
            return a;
        }
        public List<MyUtente> GetUtenti()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyUtente> a = (from u in context.UTENTE
                                             select new
                                             {
                                                 Utente_ID = u.UTENTE_ID,
                                                 Utente_User = u.UTENTE_USER,
                                                 Utente_Email = u.UTENTE_EMAIL,
                                                 Utente_Nome = u.UTENTE_NOME,
                                                 Utente_Cognome = u.UTENTE_COGNOME,
                                                 Utente_Lock = u.UTENTE_LOCK
                                             }
                                 ).Select(o => new MyUtente
                                 {
                                     Utente_ID = o.Utente_ID,
                                     Utente_User = o.Utente_User,
                                     Utente_Email = o.Utente_Email,
                                     Utente_Nome = o.Utente_Nome,
                                     Utente_Cognome = o.Utente_Cognome,
                                     Utente_Lock = o.Utente_Lock
                                 }).ToList<MyUtente>();
            return a;
        }

        public MyUtente GetUtente(int Utente_id)
        {
            return GetUtenti().Where(z => z.Utente_ID == Utente_id).DefaultIfEmpty().ToList<MyUtente>()[0];
        }

        public List<MyFiguraProfessionale_attivita> GetFiguraProf_attivita()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyFiguraProfessionale_attivita> a = (from fp in context.M_FIGATT_FIGURAPROF_ATTIVITA
                                             select new
                                             {
                                                 M_Figatt_Id = fp.M_FIGATT_ID,
                                                 M_Figatt_Fase_Id = fp.M_FIGATT_FASE_ID,
                                                 M_Figatt_Figpro_Id = fp.M_FIGATT_FIGPRO_ID
                                             }
                                 ).Select(o => new MyFiguraProfessionale_attivita
                                 {
                                     M_Figatt_Id = o.M_Figatt_Id,
                                     M_Figatt_Fase_Id = o.M_Figatt_Fase_Id,
                                     M_Figatt_Figpro_Id = o.M_Figatt_Figpro_Id
                                 }).ToList<MyFiguraProfessionale_attivita>();
            return a;
        }



        public List<MyStatoValorizzazione> GetStatiValorizzazione()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyStatoValorizzazione> a = (from sv in context.T_STAVAL_STATO_VALORIZZAZIONE 
                                select new
                                {
                                    Staval_ID = sv.T_STAVAL_ID,
                                    Staval_Codice = sv.T_STAVAL_CODICE, 
                                    Staval_Desc = sv.T_STAVAL_DESC
                                }
                                 ).Select(o => new MyStatoValorizzazione
                                 {
                                     Staval_ID = o.Staval_ID ,
                                     Staval_Codice = o.Staval_Codice ,
                                     Staval_Desc = o.Staval_Desc 
                                   }).ToList<MyStatoValorizzazione>();
            return a;
        }

        public List<MyAnalisiPos> GetAnalisiPos(int Analisi_ID)
        {
            /*
            List<MyAnalisiPos> lst = GetAnalisiPos()
                .Where(z => z.AnalisiPos_MasterAnalisi_id == Analisi_ID)
                .OrderBy(z => z.AnalisiPos_Fase_ORDINE)
                .ToList<MyAnalisiPos>();
            foreach (MyAnalisiPos pos in lst)
            {
                //sim04
                //pos.AnalisiPos_ListaFasi = GetFasiDelAnalisi(Analisi_ID);
                if (pos.AnalisiPos_Fase_id.HasValue)
                    pos.AnalisiPos_ListaFigProf = GetFigProf(pos.AnalisiPos_Fase_id.Value);
                //sim04
                //pos.AnalisiPos_ListaUdM = GetElencoUDMVisible();
            }
            
            */
            //sim04
            List<MyAnalisiPos> lst = GetGenericAnalisiPosWhere(Analisi_ID)
                .Where(z => z.AnalisiPos_Secondaria == false)
                .OrderBy(z => z.AnalisiPos_id).OrderBy(z => z.AnalisiPos_Fase_ORDINE)
                .ToList<MyAnalisiPos>();
            foreach (MyAnalisiPos pos in lst)
            {
                if (pos.AnalisiPos_Fase_id.HasValue)
                    pos.AnalisiPos_ListaFigProf = GetFigProf(pos.AnalisiPos_Fase_id.Value);
            }
            return lst;
        }
        
        public List<MyAnalisiPos> GetAnalisiPosSec(int Analisi_ID)
        {
            /*
            List<MyAnalisiPos> lst= GetAnalisiPosSec()
                .Where(z => z.AnalisiPos_MasterAnalisi_id == Analisi_ID)
                .OrderBy(z => z.AnalisiPos_Fase_ORDINE)
                .ToList<MyAnalisiPos>();
            foreach (MyAnalisiPos pos in lst)
            {
                //sim04
                //pos.AnalisiPos_ListaFasi = GetFasiDelAnalisi(Analisi_ID);
                if(pos.AnalisiPos_Fase_id.HasValue )
                    pos.AnalisiPos_ListaFigProf = GetFigProf(pos.AnalisiPos_Fase_id.Value);
                //sim04
                //pos.AnalisiPos_ListaUdM = GetElencoUDMVisible();
            }
            */

            List<MyAnalisiPos> lst = GetGenericAnalisiPosWhere(Analisi_ID)
                .Where(z => z.AnalisiPos_Secondaria == true)
                .OrderBy(z => z.AnalisiPos_id).OrderBy(z => z.AnalisiPos_Fase_ORDINE)
                .ToList<MyAnalisiPos>();
            foreach (MyAnalisiPos pos in lst)
            {
                if(pos.AnalisiPos_Fase_id.HasValue )
                    pos.AnalisiPos_ListaFigProf = GetFigProf(pos.AnalisiPos_Fase_id.Value);
            }
            return lst;
        }
        public MyAnalisiPos GetGenericAnalisiPos(int analisi_pos_id)
        {
            return GetGenericAnalisiPos().Where(z => z.AnalisiPos_id == analisi_pos_id).SingleOrDefault();

        }
       
        public List<MyAnalisiPos> GetAnalisiPos()
        {
            return GetGenericAnalisiPos().Where(z => z.AnalisiPos_Secondaria == false).ToList<MyAnalisiPos>();
        }

        public List<MyAnalisiPos> GetAnalisiPosSec()
        {
            return GetGenericAnalisiPos().Where(z => z.AnalisiPos_Secondaria == true).ToList<MyAnalisiPos>();
        }

        private string isNull(string info)
        { if (info != null) return info;
            return "";
        }
        //sim
        public List<MyAnalisiPos_IntermediEsplosiDetail> GetAnalisiPos_IntermediEsplosiDetail(int Analisi_ID)
        {
            /*
            StreamWriter sw_entity = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga_entity = "pre_query_con_where" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            sw_entity.WriteLine(riga_entity);
            sw_entity.Close();
            */
            //return GetGenericAnalisiPos_IntermediEsplosiDetail().Where(z => z.AnalisiPos_Secondaria == false && z.AnalisiPos_MasterAnalisi_id == Analisi_ID).ToList<MyAnalisiPos_IntermediEsplosiDetail>();

            List<MyAnalisiPos_IntermediEsplosiDetail> tmp =new List<MyAnalisiPos_IntermediEsplosiDetail>();
            tmp =GetGenericAnalisiPos_IntermediEsplosiDetail().Where(z => z.AnalisiPos_Secondaria == false && z.AnalisiPos_MasterAnalisi_id == Analisi_ID).ToList<MyAnalisiPos_IntermediEsplosiDetail>();
            /*
            StreamWriter sw_post_entity = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga_post_entity = "post_query_con_where" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            sw_post_entity.WriteLine(riga_post_entity);
            sw_post_entity.Close();
            */
            return tmp;

        }
        //sim
        
        public List<MyAnalisiPos_IntermediEsplosiDetail> GetAnalisiPosSec_IntermediEsplosiDetail(int Analisi_ID)
        {
            //return GetGenericAnalisiPos_IntermediEsplosiDetail().Where(z => z.AnalisiPos_Secondaria == true && z.AnalisiPos_MasterAnalisi_id == Analisi_ID).ToList<MyAnalisiPos_IntermediEsplosiDetail>();
            /*
            StreamWriter sw_entity = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga_entity = "pre_query_Sec_con_where" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            riga_entity = riga_entity + " ID: " + Analisi_ID.ToString();
            sw_entity.WriteLine(riga_entity);
            sw_entity.Close();
            */
            List<MyAnalisiPos_IntermediEsplosiDetail> lst = new List<MyAnalisiPos_IntermediEsplosiDetail>();
            lst = GetGenericAnalisiPos_IntermediEsplosiDetail().Where(z => z.AnalisiPos_Secondaria == true && z.AnalisiPos_MasterAnalisi_id == Analisi_ID).ToList<MyAnalisiPos_IntermediEsplosiDetail>();
            /*
            StreamWriter sw_post_entity = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga_post_entity = "post_query_Sec_con_where" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            riga_post_entity = riga_post_entity + " ID: " + Analisi_ID.ToString();
            sw_post_entity.WriteLine(riga_post_entity);
            sw_post_entity.Close();
            */
            return lst;
        
        }
        
        //sim
        public List<MyAnalisiPos_IntermediEsplosiDetail> GetGenericAnalisiPos_IntermediEsplosiDetail()
        {
            /*
            StreamWriter sw = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga = "pre_query" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            sw.WriteLine(riga);
            sw.Close();
            */
            string suffIntermedio = "(Intermedio) ";

            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<MyAnalisiPos_IntermediEsplosiDetail> a =
                (
                    from an in ctx.VALPOS_POSIZIONI
                    
                    join val1 in ctx.VALORI_VALORIZZAZIONI
                    on an.VALPOS_INTERM_ID equals val1.VALORI_ID
                    into valTable
                    from val1 in valTable.DefaultIfEmpty()

                    join pr in ctx.PRODOT_PRODOTTI
                    on an.VALPOS_PRODOT_ID equals pr.PRODOT_ID
                    into prTable
                    from pr in prTable.DefaultIfEmpty()

                    join udmPr in ctx.T_UNIMIS_UNITA_MISURA
                    on pr.PRODOT_T_UNIMIS_ID equals udmPr.T_UNIMIS_ID
                    into udmPrTable
                    from udmPr in udmPrTable.DefaultIfEmpty()

                    join fs in ctx.FASE
                    on an.VALPOS_FASE_ID equals fs.FASE_ID
                    into fsTable
                    from fs in fsTable.DefaultIfEmpty()

                    join fp in ctx.FIGPRO_FIGURA_PROFESSIONALE
                    on an.VALPOS_FIGPRO_ID equals fp.FIGPRO_ID
                    into fpTable
                    from fp in fpTable.DefaultIfEmpty()

                    join udm in ctx.T_UNIMIS_UNITA_MISURA
                    on an.VALPOS_T_UNIMIS_ID equals udm.T_UNIMIS_ID
                    into udmTable
                    from udm in udmTable.DefaultIfEmpty()

                    select new
                    {
                        AnalisiPos_id = an.VALPOS_ID,
                        AnalisiPos_MasterAnalisi_id = an.VALPOS_VALORI_ID,
                        AnalisiPos_Analisi_id = an.VALPOS_INTERM_ID,
                        //AnalisiPos_Analisi_Desc = val1.VALORI_GRUPPO_GRUREP_ID !=null ? val1.VALORI_VN + " - " + val1.VALORI_MP_REV : val1.VALORI_CODICE_INTERMEDIO ,// val1.VALORI_DESC
                        //  AnalisiPos_Analisi_Desc = (val1.VALORI_FLG_INTERM || val1.VALORI_FLAG_MODELLO) ? val1.VALORI_CODICE_INTERMEDIO : val1.VALORI_VN + " - " + (val1.VALORI_CODICE_DESC != null ? val1.VALORI_CODICE_DESC : "") + " - " + val1.VALORI_MP_REV,
                        AnalisiPos_Analisi_Desc = (val1.VALORI_FLG_INTERM || val1.VALORI_FLAG_MODELLO) ? suffIntermedio + val1.VALORI_DESC : val1.VALORI_VN + " - " + (val1.VALORI_CODICE_DESC != null ? val1.VALORI_CODICE_DESC : "") + " - " + val1.VALORI_MP_REV,
                        AnalisiPos_Prodotto_id = an.VALPOS_PRODOT_ID,
                        //AnalisiPos_Prodotto_UDM_id = pr.PRODOT_T_UNIMIS_ID,
                        AnalisiPos_Prodotto_UDM_Desc = udmPr.T_UNIMIS_DESC,
                        AnalisiPos_Prodotto_Desc = pr.PRODOT_DESC,//pr.PRODOT_CODICE ,// pr.PRODOT_DESC,
                        //AnalisiPos_Fase_id = an.VALPOS_FASE_ID,
                        //AnalisiPos_Fase_ORDINE = (fs.FASE_ORDINE.HasValue) ? fs.FASE_ORDINE.Value : 10000,
                        //AnalisiPos_Fase_id_MASTER = fs.FASE_FASE_ID != null ? fs.FASE_FASE_ID : an.VALPOS_FASE_ID,
                        //AnalisiPos_FigProf_id = an.VALPOS_FIGPRO_ID,
                        //AnalisiPos_desc = an.VALPOS_DESC,
                        AnalisiPos_Quantita = an.VALPOS_QTA,
                        //AnalisiPos_QuantitaCosto = an.VALPOS_COSTO_QTA,
                        //AnalisiPos_TotCosto = an.VALPOS_TOT,
                        //AnalisiPos_CoeffConversione = an.VALPOS_COEFF_CONVERSIONE,
                        //AnalisiPos_UdM_id = an.VALPOS_T_UNIMIS_ID,
                        AnalisiPos_Secondaria = an.VALPOS_SECONDARIE,
                        AnalisiPos_Fase_desc = fs.FASE_DESC,
                        AnalisiPos_FigProf_desc = fp.FIGPRO_DESC,
                        AnalisiPos_UdM_desc = udm.T_UNIMIS_DESC,
                        //AnalisiPos_Macchinario_id = an.VALPOS_MACCHI_ID,
                        //AnalisiPos_Macchinario_Desc = mac.MACCHI_DESC, // mac.MACCHI_CODICE 
                        //AnalisiPos_Cod_Settore = an.VALPOS_COD_SETTORE,
                        AnalisiPos_Flg_Intermedio = val1.VALORI_FLG_INTERM

                    }
                ).Select(o => new MyAnalisiPos_IntermediEsplosiDetail
                {
                    AnalisiPos_id = o.AnalisiPos_id,
                    AnalisiPos_MasterAnalisi_id = o.AnalisiPos_MasterAnalisi_id,
                    AnalisiPos_Analisi_id = o.AnalisiPos_Analisi_id,
                    AnalisiPos_Analisi_Desc = o.AnalisiPos_Analisi_Desc,
                    AnalisiPos_Prodotto_id = o.AnalisiPos_Prodotto_id,
                    //AnalisiPos_Prodotto_UDM_id = o.AnalisiPos_Prodotto_UDM_id,
                    AnalisiPos_Prodotto_UDM_Desc = o.AnalisiPos_Prodotto_UDM_Desc,
                    AnalisiPos_Prodotto_Desc = o.AnalisiPos_Prodotto_Desc,
                    //AnalisiPos_Fase_id = o.AnalisiPos_Fase_id,
                    //AnalisiPos_Fase_ORDINE = o.AnalisiPos_Fase_ORDINE,
                    //AnalisiPos_FigProf_id = o.AnalisiPos_FigProf_id,
                    //AnalisiPos_desc = o.AnalisiPos_desc,
                    AnalisiPos_Quantita = o.AnalisiPos_Quantita,
                    //AnalisiPos_QuantitaCosto = o.AnalisiPos_QuantitaCosto,
                    //AnalisiPos_TotCosto = o.AnalisiPos_TotCosto,
                    //AnalisiPos_CoeffConversione = o.AnalisiPos_CoeffConversione,
                    //AnalisiPos_UdM_id = o.AnalisiPos_UdM_id,
                    AnalisiPos_Secondaria = o.AnalisiPos_Secondaria,
                    AnalisiPos_Fase_desc = o.AnalisiPos_Fase_desc,
                    AnalisiPos_FigProf_desc = o.AnalisiPos_FigProf_desc,
                    AnalisiPos_UdM_desc = o.AnalisiPos_UdM_desc,
                    //AnalisiPos_Macchinario_id = o.AnalisiPos_Macchinario_id,
                    //AnalisiPos_Macchinario_Desc = o.AnalisiPos_Macchinario_Desc,
                    //AnalisiPos_Fase_id_MASTER = o.AnalisiPos_Fase_id_MASTER,
                    //AnalisiPos_Cod_Settore = o.AnalisiPos_Cod_Settore,
                    AnalisiPos_Flg_Intermedio = o.AnalisiPos_Flg_Intermedio
                }
                ).ToList<MyAnalisiPos_IntermediEsplosiDetail>();
            /*
            StreamWriter sw_post = new StreamWriter("C:\\temp\\IZSLER_LOG.TXT", true);
            string riga_post = "post_query" + " " + DateTime.Now.Hour.ToString() + " " + DateTime.Now.Minute.ToString() + " " + DateTime.Now.Second.ToString();
            sw_post.WriteLine(riga_post);
            sw_post.Close();
            */
            return a;
                        
            
        }

        //SimStampaIntermedi
        public List<MyIntermediEsplosiAnalisi> GetIntermediEsplosiAnalisi_stampa(int valori_id)
        {

            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            List<MyIntermediEsplosiAnalisi> lstIntEsp = new List<MyIntermediEsplosiAnalisi>();
            List<IntermediEsplosiAnalisi_stampa_Result> lstIntEsp_st = new List<IntermediEsplosiAnalisi_stampa_Result>();

            lstIntEsp_st = en.IntermediEsplosiAnalisi_stampa(valori_id).ToList<IntermediEsplosiAnalisi_stampa_Result>();

            foreach (IntermediEsplosiAnalisi_stampa_Result r in lstIntEsp_st)
            {
                MyIntermediEsplosiAnalisi i = new MyIntermediEsplosiAnalisi();

                i.Intesp_ordine = r.ORDINE;
                i.Intesp_livello = r.LIVELLO;
                i.Intesp_id = r.ID;
                i.Intesp_id_padre = r.ID_PADRE;
                i.Intesp_posizione = r.POSIZIONE;
                i.Intesp_valori_id = r.VALORI_ID;
                i.Intesp_fase_desc = r.FASE_DESC;
                i.Intesp_valpos_qta = r.VALPOS_QTA;
                i.Intesp_valpos_tot = r.VALPOS_TOT;
                i.Intesp_codice = r.CODICE;
                i.Intesp_descrizione = r.DESCRIZIONE;
                i.Intesp_t_unimis_desc = r.T_UNIMIS_DESC;
                i.Intesp_valpos_desc = r.valpos_desc;
                i.Intesp_secondaria = r.SECONDARIA;

                lstIntEsp.Add(i);

            }

            return lstIntEsp;
        }
        

        //sim01
        public List<MyAnalisiPos> GetGenericAnalisiPosWhere(int Analisi_id)
        {
            string suffIntermedio = "(Intermedio) ";

            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<MyAnalisiPos> a =
                (
                    from an in ctx.VALPOS_POSIZIONI
                    join val in ctx.VALORI_VALORIZZAZIONI
                    on an.VALPOS_VALORI_ID equals val.VALORI_ID

                    join val1 in ctx.VALORI_VALORIZZAZIONI
                    on an.VALPOS_INTERM_ID equals val1.VALORI_ID
                    into valTable
                    from val1 in valTable.DefaultIfEmpty()

                    join pr in ctx.PRODOT_PRODOTTI
                    on an.VALPOS_PRODOT_ID equals pr.PRODOT_ID
                    into prTable
                    from pr in prTable.DefaultIfEmpty()

                    join udmPr in ctx.T_UNIMIS_UNITA_MISURA
                    on pr.PRODOT_T_UNIMIS_ID equals udmPr.T_UNIMIS_ID
                    into udmPrTable
                    from udmPr in udmPrTable.DefaultIfEmpty()

                    join fs in ctx.FASE
                    on an.VALPOS_FASE_ID equals fs.FASE_ID
                    into fsTable
                    from fs in fsTable.DefaultIfEmpty()

                    join fp in ctx.FIGPRO_FIGURA_PROFESSIONALE
                    on an.VALPOS_FIGPRO_ID equals fp.FIGPRO_ID
                    into fpTable
                    from fp in fpTable.DefaultIfEmpty()

                    join udm in ctx.T_UNIMIS_UNITA_MISURA
                    on an.VALPOS_T_UNIMIS_ID equals udm.T_UNIMIS_ID
                    into udmTable
                    from udm in udmTable.DefaultIfEmpty()

                    join mac in ctx.MACCHI_MACCHINARIO
                    on an.VALPOS_MACCHI_ID equals mac.MACCHI_ID
                    into macTable
                    from mac in macTable.DefaultIfEmpty()


                    select new
                    {
                        AnalisiPos_id = an.VALPOS_ID,
                        AnalisiPos_MasterAnalisi_id = an.VALPOS_VALORI_ID,
                        AnalisiPos_Analisi_id = an.VALPOS_INTERM_ID,
                        //AnalisiPos_Analisi_Desc = val1.VALORI_GRUPPO_GRUREP_ID !=null ? val1.VALORI_VN + " - " + val1.VALORI_MP_REV : val1.VALORI_CODICE_INTERMEDIO ,// val1.VALORI_DESC
                        //  AnalisiPos_Analisi_Desc = (val1.VALORI_FLG_INTERM || val1.VALORI_FLAG_MODELLO) ? val1.VALORI_CODICE_INTERMEDIO : val1.VALORI_VN + " - " + (val1.VALORI_CODICE_DESC != null ? val1.VALORI_CODICE_DESC : "") + " - " + val1.VALORI_MP_REV,
                        AnalisiPos_Analisi_Desc = (val1.VALORI_FLG_INTERM || val1.VALORI_FLAG_MODELLO) ? suffIntermedio + val1.VALORI_DESC : val1.VALORI_VN + " - " + (val1.VALORI_CODICE_DESC != null ? val1.VALORI_CODICE_DESC : "") + " - " + val1.VALORI_MP_REV,
                        AnalisiPos_Prodotto_id = an.VALPOS_PRODOT_ID,
                        AnalisiPos_Prodotto_UDM_id = pr.PRODOT_T_UNIMIS_ID,
                        AnalisiPos_Prodotto_UDM_Desc = udmPr.T_UNIMIS_DESC,
                        AnalisiPos_Prodotto_Desc = pr.PRODOT_DESC,//pr.PRODOT_CODICE ,// pr.PRODOT_DESC,
                        AnalisiPos_Fase_id = an.VALPOS_FASE_ID,
                        AnalisiPos_Fase_ORDINE = (fs.FASE_ORDINE.HasValue) ? fs.FASE_ORDINE.Value : 10000,
                        AnalisiPos_Fase_id_MASTER = fs.FASE_FASE_ID != null ? fs.FASE_FASE_ID : an.VALPOS_FASE_ID,
                        AnalisiPos_FigProf_id = an.VALPOS_FIGPRO_ID,
                        AnalisiPos_desc = an.VALPOS_DESC,
                        AnalisiPos_Quantita = an.VALPOS_QTA,
                        AnalisiPos_QuantitaCosto = an.VALPOS_COSTO_QTA,
                        AnalisiPos_TotCosto = an.VALPOS_TOT,
                        AnalisiPos_CoeffConversione = an.VALPOS_COEFF_CONVERSIONE,
                        AnalisiPos_UdM_id = an.VALPOS_T_UNIMIS_ID,
                        AnalisiPos_Secondaria = an.VALPOS_SECONDARIE,
                        AnalisiPos_Fase_desc = fs.FASE_DESC,
                        AnalisiPos_FigProf_desc = fp.FIGPRO_DESC,
                        AnalisiPos_UdM_desc = udm.T_UNIMIS_DESC,
                        AnalisiPos_Macchinario_id = an.VALPOS_MACCHI_ID,
                        AnalisiPos_Macchinario_Desc = mac.MACCHI_DESC, // mac.MACCHI_CODICE 
                        AnalisiPos_Cod_Settore = an.VALPOS_COD_SETTORE,
                        AnalisiPos_Flg_Intermedio = val1.VALORI_FLG_INTERM

                    }
                ).Select(o => new MyAnalisiPos
                {
                    AnalisiPos_id = o.AnalisiPos_id,
                    AnalisiPos_MasterAnalisi_id = o.AnalisiPos_MasterAnalisi_id,
                    AnalisiPos_Analisi_id = o.AnalisiPos_Analisi_id,
                    AnalisiPos_Analisi_Desc = o.AnalisiPos_Analisi_Desc,
                    AnalisiPos_Prodotto_id = o.AnalisiPos_Prodotto_id,
                    AnalisiPos_Prodotto_UDM_id = o.AnalisiPos_Prodotto_UDM_id,
                    AnalisiPos_Prodotto_UDM_Desc = o.AnalisiPos_Prodotto_UDM_Desc,
                    AnalisiPos_Prodotto_Desc = o.AnalisiPos_Prodotto_Desc,
                    AnalisiPos_Fase_id = o.AnalisiPos_Fase_id,
                    AnalisiPos_Fase_ORDINE = o.AnalisiPos_Fase_ORDINE,
                    AnalisiPos_FigProf_id = o.AnalisiPos_FigProf_id,
                    AnalisiPos_desc = o.AnalisiPos_desc,
                    AnalisiPos_Quantita = o.AnalisiPos_Quantita,
                    AnalisiPos_QuantitaCosto = o.AnalisiPos_QuantitaCosto,
                    AnalisiPos_TotCosto = o.AnalisiPos_TotCosto,
                    AnalisiPos_CoeffConversione = o.AnalisiPos_CoeffConversione,
                    AnalisiPos_UdM_id = o.AnalisiPos_UdM_id,
                    AnalisiPos_Secondaria = o.AnalisiPos_Secondaria,
                    AnalisiPos_Fase_desc = o.AnalisiPos_Fase_desc,
                    AnalisiPos_FigProf_desc = o.AnalisiPos_FigProf_desc,
                    AnalisiPos_UdM_desc = o.AnalisiPos_UdM_desc,
                    AnalisiPos_Macchinario_id = o.AnalisiPos_Macchinario_id,
                    AnalisiPos_Macchinario_Desc = o.AnalisiPos_Macchinario_Desc,
                    AnalisiPos_Fase_id_MASTER = o.AnalisiPos_Fase_id_MASTER,
                    AnalisiPos_Cod_Settore = o.AnalisiPos_Cod_Settore,
                    AnalisiPos_Flg_Intermedio = o.AnalisiPos_Flg_Intermedio
                }
                ).Where(z => z.AnalisiPos_MasterAnalisi_id == Analisi_id).ToList<MyAnalisiPos>();
            
            return a;
        }

        public List<MyAnalisiPos> GetGenericAnalisiPos()
        {
            string suffIntermedio = "(Intermedio) ";

            IZSLER_CAP_Entities ctx= new IZSLER_CAP_Entities();
            List<MyAnalisiPos> a =
                (
                    from an in ctx.VALPOS_POSIZIONI
                    join val in ctx.VALORI_VALORIZZAZIONI 
                    on an.VALPOS_VALORI_ID equals val.VALORI_ID

                    join val1 in ctx.VALORI_VALORIZZAZIONI
                    on an.VALPOS_INTERM_ID equals val1.VALORI_ID 
                    into valTable
                    from val1 in valTable.DefaultIfEmpty()
                    
                    join pr in ctx.PRODOT_PRODOTTI  
                    on an.VALPOS_PRODOT_ID equals pr.PRODOT_ID 
                    into prTable
                    from pr in prTable.DefaultIfEmpty ()

                    join udmPr in ctx.T_UNIMIS_UNITA_MISURA
                    on pr.PRODOT_T_UNIMIS_ID equals udmPr.T_UNIMIS_ID
                    into udmPrTable
                    from udmPr in udmPrTable.DefaultIfEmpty()

                    join fs in ctx.FASE 
                    on an.VALPOS_FASE_ID equals fs.FASE_ID 
                    into fsTable 
                    from fs in fsTable.DefaultIfEmpty ()

                    join fp in ctx.FIGPRO_FIGURA_PROFESSIONALE
                    on an.VALPOS_FIGPRO_ID equals fp.FIGPRO_ID 
                    into fpTable
                    from fp in fpTable.DefaultIfEmpty()

                    join udm in ctx.T_UNIMIS_UNITA_MISURA
                    on an.VALPOS_T_UNIMIS_ID equals udm.T_UNIMIS_ID 
                    into udmTable
                    from udm in udmTable.DefaultIfEmpty()

                    join mac in ctx.MACCHI_MACCHINARIO
                    on an.VALPOS_MACCHI_ID equals mac.MACCHI_ID 
                    into macTable
                    from mac in macTable.DefaultIfEmpty()


                    select new 
                    { 
                        AnalisiPos_id = an.VALPOS_ID ,
                        AnalisiPos_MasterAnalisi_id = an.VALPOS_VALORI_ID ,
                        AnalisiPos_Analisi_id = an.VALPOS_INTERM_ID ,
                        //AnalisiPos_Analisi_Desc = val1.VALORI_GRUPPO_GRUREP_ID !=null ? val1.VALORI_VN + " - " + val1.VALORI_MP_REV : val1.VALORI_CODICE_INTERMEDIO ,// val1.VALORI_DESC
                      //  AnalisiPos_Analisi_Desc = (val1.VALORI_FLG_INTERM || val1.VALORI_FLAG_MODELLO) ? val1.VALORI_CODICE_INTERMEDIO : val1.VALORI_VN + " - " + (val1.VALORI_CODICE_DESC != null ? val1.VALORI_CODICE_DESC : "") + " - " + val1.VALORI_MP_REV,
                        AnalisiPos_Analisi_Desc = (val1.VALORI_FLG_INTERM || val1.VALORI_FLAG_MODELLO) ? suffIntermedio + val1.VALORI_DESC : val1.VALORI_VN + " - " + (val1.VALORI_CODICE_DESC != null ? val1.VALORI_CODICE_DESC : "") + " - " + val1.VALORI_MP_REV,
                        AnalisiPos_Prodotto_id = an.VALPOS_PRODOT_ID ,
                        AnalisiPos_Prodotto_UDM_id = pr.PRODOT_T_UNIMIS_ID ,
                        AnalisiPos_Prodotto_UDM_Desc = udmPr.T_UNIMIS_DESC,
                        AnalisiPos_Prodotto_Desc =  pr.PRODOT_DESC,//pr.PRODOT_CODICE ,// pr.PRODOT_DESC,
                        AnalisiPos_Fase_id = an.VALPOS_FASE_ID ,
                        AnalisiPos_Fase_ORDINE = (fs.FASE_ORDINE.HasValue )?fs.FASE_ORDINE.Value :10000,
                        AnalisiPos_Fase_id_MASTER = fs.FASE_FASE_ID !=null?fs.FASE_FASE_ID:an.VALPOS_FASE_ID,
                        AnalisiPos_FigProf_id = an.VALPOS_FIGPRO_ID ,
                        AnalisiPos_desc= an.VALPOS_DESC,
                        AnalisiPos_Quantita= an.VALPOS_QTA,
                        AnalisiPos_QuantitaCosto = an.VALPOS_COSTO_QTA ,
                        AnalisiPos_TotCosto = an.VALPOS_TOT,
                        AnalisiPos_CoeffConversione = an.VALPOS_COEFF_CONVERSIONE ,
                        AnalisiPos_UdM_id = an.VALPOS_T_UNIMIS_ID,
                        AnalisiPos_Secondaria=an.VALPOS_SECONDARIE,
                        AnalisiPos_Fase_desc = fs.FASE_DESC,
                        AnalisiPos_FigProf_desc = fp.FIGPRO_DESC,
                        AnalisiPos_UdM_desc = udm.T_UNIMIS_DESC,
                        AnalisiPos_Macchinario_id= an.VALPOS_MACCHI_ID ,
                        AnalisiPos_Macchinario_Desc = mac.MACCHI_DESC, // mac.MACCHI_CODICE 
                        AnalisiPos_Cod_Settore = an.VALPOS_COD_SETTORE,
                        AnalisiPos_Flg_Intermedio = val1.VALORI_FLG_INTERM

                    }
                ).Select(o => new MyAnalisiPos 
                {
                    AnalisiPos_id = o.AnalisiPos_id,
                    AnalisiPos_MasterAnalisi_id = o.AnalisiPos_MasterAnalisi_id,
                    AnalisiPos_Analisi_id = o.AnalisiPos_Analisi_id,
                    AnalisiPos_Analisi_Desc = o.AnalisiPos_Analisi_Desc,
                    AnalisiPos_Prodotto_id = o.AnalisiPos_Prodotto_id,
                    AnalisiPos_Prodotto_UDM_id = o.AnalisiPos_Prodotto_UDM_id,
                    AnalisiPos_Prodotto_UDM_Desc = o.AnalisiPos_Prodotto_UDM_Desc,
                    AnalisiPos_Prodotto_Desc= o.AnalisiPos_Prodotto_Desc,
                    AnalisiPos_Fase_id = o.AnalisiPos_Fase_id,
                    AnalisiPos_Fase_ORDINE = o.AnalisiPos_Fase_ORDINE,
                    AnalisiPos_FigProf_id = o.AnalisiPos_FigProf_id,
                    AnalisiPos_desc = o.AnalisiPos_desc,
                    AnalisiPos_Quantita = o.AnalisiPos_Quantita,
                    AnalisiPos_QuantitaCosto = o.AnalisiPos_QuantitaCosto,
                    AnalisiPos_TotCosto = o.AnalisiPos_TotCosto,
                    AnalisiPos_CoeffConversione= o.AnalisiPos_CoeffConversione,
                    AnalisiPos_UdM_id = o.AnalisiPos_UdM_id ,
                    AnalisiPos_Secondaria= o.AnalisiPos_Secondaria,
                    AnalisiPos_Fase_desc = o.AnalisiPos_Fase_desc ,
                    AnalisiPos_FigProf_desc = o.AnalisiPos_FigProf_desc,
                    AnalisiPos_UdM_desc = o.AnalisiPos_UdM_desc ,
                    AnalisiPos_Macchinario_id = o.AnalisiPos_Macchinario_id,
                    AnalisiPos_Macchinario_Desc = o.AnalisiPos_Macchinario_Desc,
                    AnalisiPos_Fase_id_MASTER = o.AnalisiPos_Fase_id_MASTER,
                    AnalisiPos_Cod_Settore = o.AnalisiPos_Cod_Settore,
                    AnalisiPos_Flg_Intermedio = o.AnalisiPos_Flg_Intermedio
                }
                ).ToList<MyAnalisiPos>();
            
            return a;
        }

        #region prodotto

        //SimStampaIntermedi
        public List<MyIntermediEsplosiProdotto> GetIntermediEsplosiProdotto_stampa(int prodot_id)
        {
            
            IZSLER_CAP_Entities en = new IZSLER_CAP_Entities();
            List<MyIntermediEsplosiProdotto> lstIntEsp = new List<MyIntermediEsplosiProdotto>();
            List<IntermediEsplosiProdotto_stampa_Result> lstIntEsp_st = new List<IntermediEsplosiProdotto_stampa_Result>();

            lstIntEsp_st = en.IntermediEsplosiProdotto_stampa(prodot_id).ToList<IntermediEsplosiProdotto_stampa_Result>();

            foreach (IntermediEsplosiProdotto_stampa_Result r in lstIntEsp_st)
            {
                MyIntermediEsplosiProdotto i = new MyIntermediEsplosiProdotto();

                i.Intesp_ordine	= r.ORDINE;
                i.Intesp_livello = r.LIVELLO;	
                i.Intesp_id = r.ID;
                i.Intesp_id_padre = r.ID_PADRE;	
                i.Intesp_posizione = r.POSIZIONE;	
                i.Intesp_valori_id = r.VALORI_ID;
                i.Intesp_fase_desc = r.FASE_DESC;	
                i.Intesp_propos_qta	= r.PROPOS_QTA;
                i.Intesp_propos_tot	= r.PROPOS_TOT;
                i.Intesp_codice	= r.CODICE;
                i.Intesp_descrizione = r.DESCRIZIONE;
                i.Intesp_t_unimis_desc = r.T_UNIMIS_DESC;	
                i.Intesp_propos_desc = r.propos_desc;

                lstIntEsp.Add(i);

            }

            return lstIntEsp;
        }
        
        public List<MyProdottoPos> GetProdottiPos(int Prodotto_ID)
        {   //sim04
            /*
            List<MyProdottoPos> lst = GetProdottiPos()
                .Where(z => z.ProdottoPos_MasterProdotto_id == Prodotto_ID)
                .OrderBy(z=>z.ProdottoPos_Fase_ORDINE) 
                .ToList<MyProdottoPos>();
            foreach (MyProdottoPos pos in lst)
            {
                pos.ProdottoPos_ListaFasi = GetFasiDelProdotto(Prodotto_ID);
                if (pos.ProdottoPos_Fase_id.HasValue)
                    pos.ProdottoPos_ListaFigProf = GetFigProf(pos.ProdottoPos_Fase_id.Value);
                pos.ProdottoPos_ListaUdM = GetElencoUDMVisible();
            }
            */


            
            List<MyProdottoPos> lst = GetGenericProdottiPosWhere(Prodotto_ID)
                .OrderBy(z => z.ProdottoPos_id).OrderBy(z => z.ProdottoPos_Fase_ORDINE) 
                .ToList<MyProdottoPos>();
            foreach (MyProdottoPos pos in lst)
            {
                if (pos.ProdottoPos_Fase_id.HasValue)
                    pos.ProdottoPos_ListaFigProf = GetFigProf(pos.ProdottoPos_Fase_id.Value);
            }

            return lst;
        }
        public MyProdottoPos GetGenericProdottiPos(int analisi_pos_id)
        {
            return GetGenericProdottiPos().Where(z => z.ProdottoPos_id == analisi_pos_id).SingleOrDefault();
        }
        public List<MyProdottoPos> GetProdottiPos()
        {
            return GetGenericProdottiPos().ToList<MyProdottoPos>();
        }

        //sim
        public List<MyProdottoPos> GetGenericProdottiPosWhere(int Prodotto_id)
        {
            string suffIntermedio = "(Intermedio) ";
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<MyProdottoPos> a =
                (
                    from pp in ctx.PROPOS_POSIZIONI
                    join pro in ctx.PRODOT_PRODOTTI
                    on pp.PROPOS_PRODOT_ID equals pro.PRODOT_ID

                    join pro1 in ctx.PRODOT_PRODOTTI
                    on pp.PROPOS_PRODOT_ID_SEC equals pro1.PRODOT_ID
                    into proTable
                    from pro1 in proTable.DefaultIfEmpty()

                    join val in ctx.VALORI_VALORIZZAZIONI
                    on pp.PROPOS_INTERM_ID equals val.VALORI_ID
                    into valTable
                    from val in valTable.DefaultIfEmpty()

                    join mac in ctx.MACCHI_MACCHINARIO
                    on pp.PROPOS_MACCHI_ID equals mac.MACCHI_ID
                    into macTable
                    from mac in macTable.DefaultIfEmpty()

                    join fas in ctx.FASE
                    on pp.PROPOS_FASE_ID equals fas.FASE_ID
                    into fasTable
                    from fas in fasTable.DefaultIfEmpty()

                    join udmPr in ctx.T_UNIMIS_UNITA_MISURA
                    on pro1.PRODOT_T_UNIMIS_ID equals udmPr.T_UNIMIS_ID
                    into udmPrTable
                    from udmPr in udmPrTable.DefaultIfEmpty()

                    join udm in ctx.T_UNIMIS_UNITA_MISURA
                    on pp.PROPOS_T_UNIMIS_ID equals udm.T_UNIMIS_ID
                    into udmTable
                    from udm in udmTable.DefaultIfEmpty()

                    join fp in ctx.FIGPRO_FIGURA_PROFESSIONALE
                    on pp.PROPOS_FIGPRO_ID equals fp.FIGPRO_ID
                    into fpTable
                    from fp in fpTable.DefaultIfEmpty()

                    select new
                    {

                        ProdottoPos_id = pp.PROPOS_ID,
                        ProdottoPos_MasterProdotto_id = pp.PROPOS_PRODOT_ID,
                        ProdottoPos_Analisi_id = pp.PROPOS_INTERM_ID,
                        ProdottoPos_Analisi_Desc = (val.VALORI_FLG_INTERM) ? suffIntermedio + val.VALORI_DESC : val.VALORI_VN + " - " + val.VALORI_MP_REV,// val1.VALORI_DESC
                        ProdottoPos_Prodotto_id = pp.PROPOS_PRODOT_ID_SEC,
                        ProdottoPos_Prodotto_UDM_id = pro1.PRODOT_T_UNIMIS_ID,
                        ProdottoPos_Prodotto_UDM_Desc = udmPr.T_UNIMIS_DESC,
                        ProdottoPos_Prodotto_Desc = pro1.PRODOT_DESC,// pro1.PRODOT_CODICE,// pr.PRODOT_DESC,
                        ProdottoPos_Fase_id = pp.PROPOS_FASE_ID,
                        ProdottoPos_Fase_Desc = fas.FASE_DESC,
                        ProdottoPos_Fase_ORDINE = (fas.FASE_ORDINE.HasValue) ? fas.FASE_ORDINE.Value : 10000,
                        ProdottoPos_Fase_id_MASTER = fas.FASE_FASE_ID != null ? fas.FASE_FASE_ID : fas.FASE_ID,
                        ProdottoPos_FigProf_id = pp.PROPOS_FIGPRO_ID,
                        ProdottoPos_FigProf_desc = fp.FIGPRO_DESC,
                        ProdottoPos_desc = pp.PROPOS_DESC,
                        ProdottoPos_Quantita = pp.PROPOS_QTA,
                        ProdottoPos_QuantitaCosto = pp.PROPOS_COSTO_QTA,
                        ProdottoPos_TotCosto = pp.PROPOS_TOT,
                        ProdottoPos_CoeffConversione = pp.PROPOS_COEFF_CONVERSIONE,
                        ProdottoPos_UdM_id = pp.PROPOS_T_UNIMIS_ID,
                        ProdottoPos_Udm_desc = udm.T_UNIMIS_DESC,
                        ProdottoPos_Macchinario_id = pp.PROPOS_MACCHI_ID,
                        ProdottoPos_Macchinario_Desc = mac.MACCHI_DESC, // mac.MACCHI_CODICE ,
                        ProdottoPos_Cod_Settore = pp.PROPOS_COD_SETTORE,
                        ProdottoPos_Flg_Intermedio = val.VALORI_FLG_INTERM
                    }
                ).Select(o => new MyProdottoPos
                {
                    ProdottoPos_id = o.ProdottoPos_id,
                    ProdottoPos_MasterProdotto_id = o.ProdottoPos_MasterProdotto_id,
                    ProdottoPos_Analisi_id = o.ProdottoPos_Analisi_id,
                    ProdottoPos_Analisi_Desc = o.ProdottoPos_Analisi_Desc,
                    ProdottoPos_Prodotto_id = o.ProdottoPos_Prodotto_id,
                    ProdottoPos_Prodotto_UDM_id = o.ProdottoPos_Prodotto_UDM_id,
                    ProdottoPos_Prodotto_UDM_Desc = o.ProdottoPos_Prodotto_UDM_Desc,
                    ProdottoPos_Prodotto_Desc = o.ProdottoPos_Prodotto_Desc,
                    ProdottoPos_Fase_Desc = o.ProdottoPos_Fase_Desc,
                    ProdottoPos_Fase_id = o.ProdottoPos_Fase_id,
                    ProdottoPos_Fase_ORDINE = o.ProdottoPos_Fase_ORDINE,
                    ProdottoPos_FigProf_id = o.ProdottoPos_FigProf_id,
                    ProdottoPos_FigProf_desc = o.ProdottoPos_FigProf_desc,
                    ProdottoPos_desc = o.ProdottoPos_desc,
                    ProdottoPos_Quantita = o.ProdottoPos_Quantita,
                    ProdottoPos_QuantitaCosto = o.ProdottoPos_QuantitaCosto,
                    ProdottoPos_TotCosto = o.ProdottoPos_TotCosto,
                    ProdottoPos_CoeffConversione = o.ProdottoPos_CoeffConversione,
                    ProdottoPos_UdM_id = o.ProdottoPos_UdM_id,
                    ProdottoPos_UdM_desc = o.ProdottoPos_Udm_desc,
                    ProdottoPos_Macchinario_id = o.ProdottoPos_Macchinario_id,
                    ProdottoPos_Macchinario_Desc = o.ProdottoPos_Macchinario_Desc,
                    ProdottoPos_Fase_id_MASTER = o.ProdottoPos_Fase_id_MASTER,
                    ProdottoPos_Cod_Settore = o.ProdottoPos_Cod_Settore,
                    ProdottoPos_Flg_Intermedio = o.ProdottoPos_Flg_Intermedio
                }
                ).Where(z => z.ProdottoPos_MasterProdotto_id == Prodotto_id).ToList<MyProdottoPos>();
            return a;
        }


        public List<MyProdottoPos> GetGenericProdottiPos()
        {
            string suffIntermedio = "(Intermedio) ";
            IZSLER_CAP_Entities ctx= new IZSLER_CAP_Entities();
            List<MyProdottoPos> a =
                (
                    from pp in ctx.PROPOS_POSIZIONI
                    join pro in ctx.PRODOT_PRODOTTI
                    on pp.PROPOS_PRODOT_ID  equals pro.PRODOT_ID

                    join pro1 in ctx.PRODOT_PRODOTTI
                    on pp.PROPOS_PRODOT_ID_SEC equals pro1.PRODOT_ID 
                    into proTable
                    from pro1 in proTable.DefaultIfEmpty()
                    
                    join val in ctx.VALORI_VALORIZZAZIONI 
                    on pp.PROPOS_INTERM_ID equals val.VALORI_ID 
                    into valTable
                    from val in valTable.DefaultIfEmpty()

                    join mac in ctx.MACCHI_MACCHINARIO
                    on pp.PROPOS_MACCHI_ID equals mac.MACCHI_ID 
                    into macTable
                    from mac in macTable.DefaultIfEmpty()

                    join fas in ctx.FASE 
                    on pp.PROPOS_FASE_ID  equals  fas.FASE_ID 
                    into fasTable
                    from fas in fasTable.DefaultIfEmpty()

                    join udmPr in ctx.T_UNIMIS_UNITA_MISURA
                    on pro1.PRODOT_T_UNIMIS_ID equals udmPr.T_UNIMIS_ID
                    into udmPrTable
                    from udmPr in udmPrTable.DefaultIfEmpty()

                    join udm in ctx.T_UNIMIS_UNITA_MISURA
                    on pp.PROPOS_T_UNIMIS_ID equals udm.T_UNIMIS_ID
                    into udmTable
                    from udm in udmTable.DefaultIfEmpty()

                    join fp in ctx.FIGPRO_FIGURA_PROFESSIONALE
                    on pp.PROPOS_FIGPRO_ID equals fp.FIGPRO_ID
                    into fpTable
                    from fp in fpTable.DefaultIfEmpty()

                    select new 
                    { 
                        
                        ProdottoPos_id = pp.PROPOS_ID ,
                        ProdottoPos_MasterProdotto_id = pp.PROPOS_PRODOT_ID,
                        ProdottoPos_Analisi_id = pp.PROPOS_INTERM_ID,
                        ProdottoPos_Analisi_Desc = (val.VALORI_FLG_INTERM) ? suffIntermedio + val.VALORI_DESC : val.VALORI_VN + " - " + val.VALORI_MP_REV,// val1.VALORI_DESC
                        ProdottoPos_Prodotto_id = pp.PROPOS_PRODOT_ID_SEC,
                        ProdottoPos_Prodotto_UDM_id = pro1.PRODOT_T_UNIMIS_ID,
                        ProdottoPos_Prodotto_UDM_Desc = udmPr.T_UNIMIS_DESC,
                        ProdottoPos_Prodotto_Desc = pro1.PRODOT_DESC,// pro1.PRODOT_CODICE,// pr.PRODOT_DESC,
                        ProdottoPos_Fase_id = pp.PROPOS_FASE_ID,
                        ProdottoPos_Fase_Desc = fas.FASE_DESC ,
                        ProdottoPos_Fase_ORDINE = (fas.FASE_ORDINE.HasValue )?fas.FASE_ORDINE.Value :10000,
                        ProdottoPos_Fase_id_MASTER = fas.FASE_FASE_ID !=null?fas.FASE_FASE_ID:fas.FASE_ID,
                        ProdottoPos_FigProf_id = pp.PROPOS_FIGPRO_ID,
                        ProdottoPos_FigProf_desc = fp.FIGPRO_DESC,
                        ProdottoPos_desc = pp.PROPOS_DESC,
                        ProdottoPos_Quantita = pp.PROPOS_QTA,
                        ProdottoPos_QuantitaCosto = pp.PROPOS_COSTO_QTA,
                        ProdottoPos_TotCosto = pp.PROPOS_TOT,
                        ProdottoPos_CoeffConversione = pp.PROPOS_COEFF_CONVERSIONE,
                        ProdottoPos_UdM_id = pp.PROPOS_T_UNIMIS_ID,
                        ProdottoPos_Udm_desc = udm.T_UNIMIS_DESC,
                        ProdottoPos_Macchinario_id =pp.PROPOS_MACCHI_ID ,
                        ProdottoPos_Macchinario_Desc = mac.MACCHI_DESC, // mac.MACCHI_CODICE ,
                        ProdottoPos_Cod_Settore = pp.PROPOS_COD_SETTORE,
                        ProdottoPos_Flg_Intermedio = val.VALORI_FLG_INTERM
                    }
                ).Select(o => new MyProdottoPos 
                {
                    ProdottoPos_id = o.ProdottoPos_id,
                    ProdottoPos_MasterProdotto_id = o.ProdottoPos_MasterProdotto_id,
                    ProdottoPos_Analisi_id = o.ProdottoPos_Analisi_id,
                    ProdottoPos_Analisi_Desc = o.ProdottoPos_Analisi_Desc,
                    ProdottoPos_Prodotto_id = o.ProdottoPos_Prodotto_id,
                    ProdottoPos_Prodotto_UDM_id = o.ProdottoPos_Prodotto_UDM_id,
                    ProdottoPos_Prodotto_UDM_Desc = o.ProdottoPos_Prodotto_UDM_Desc,
                    ProdottoPos_Prodotto_Desc = o.ProdottoPos_Prodotto_Desc,
                    ProdottoPos_Fase_Desc  = o.ProdottoPos_Fase_Desc ,
                    ProdottoPos_Fase_id = o.ProdottoPos_Fase_id,
                    ProdottoPos_Fase_ORDINE = o.ProdottoPos_Fase_ORDINE,
                    ProdottoPos_FigProf_id = o.ProdottoPos_FigProf_id,
                    ProdottoPos_FigProf_desc = o.ProdottoPos_FigProf_desc,
                    ProdottoPos_desc = o.ProdottoPos_desc,
                    ProdottoPos_Quantita = o.ProdottoPos_Quantita,
                    ProdottoPos_QuantitaCosto = o.ProdottoPos_QuantitaCosto,
                    ProdottoPos_TotCosto = o.ProdottoPos_TotCosto,
                    ProdottoPos_CoeffConversione = o.ProdottoPos_CoeffConversione,
                    ProdottoPos_UdM_id = o.ProdottoPos_UdM_id,
                    ProdottoPos_UdM_desc = o.ProdottoPos_Udm_desc,
                    ProdottoPos_Macchinario_id = o.ProdottoPos_Macchinario_id,
                    ProdottoPos_Macchinario_Desc=o.ProdottoPos_Macchinario_Desc,
                    ProdottoPos_Fase_id_MASTER = o.ProdottoPos_Fase_id_MASTER,
                    ProdottoPos_Cod_Settore = o.ProdottoPos_Cod_Settore,
                    ProdottoPos_Flg_Intermedio = o.ProdottoPos_Flg_Intermedio
                }
                ).ToList<MyProdottoPos>();
            return a;
        }


        #endregion
        
        public List<MyAnalisi> GetAnalisiPerProfilo(Profili profilo)
        {
            DateTime today = DateTime.Now.Date; 
            List<int > elencoGruppiProfilo = new List<int> ();
            foreach(Gruppo g in profilo.ElencoGruppi )
            {
                if(!elencoGruppiProfilo.Contains (g.GruppoID))
                    elencoGruppiProfilo.Add(g.GruppoID);
            }
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            List<MyAnalisi> a =
                (
                    from an in ctx.VALORI_VALORIZZAZIONI
                    join tv in ctx.T_STAVAL_STATO_VALORIZZAZIONE
                    on an.VALORI_T_STAVAL_ID equals tv.T_STAVAL_ID
                    join ut in ctx.UTENTE
                    on an.VALORI_UTENTE_ID equals ut.UTENTE_ID

                    join gr in ctx.GRUREP_GRUPPI_REPARTI.Where(z => z.GRUREP_FLG_REPARTO == false)
                    on an.VALORI_GRUPPO_GRUREP_ID equals gr.GRUREP_ID
                    into grTable
                    from gr in grTable.DefaultIfEmpty()
                    join rep in ctx.GRUREP_GRUPPI_REPARTI.Where(z => z.GRUREP_FLG_REPARTO == true)
                    on an.VALORI_REPARTO_GRUREP_ID equals rep.GRUREP_ID
                    into repTable
                    from rep in repTable.DefaultIfEmpty()
                    where (elencoGruppiProfilo.Contains(gr.GRUREP_ID) || elencoGruppiProfilo.Contains(rep.GRUREP_ID))
                    
                    select new
                    {
                        Analisi_id = an.VALORI_ID,
                        Analisi_utente_id = an.VALORI_UTENTE_ID,
                        Analisi_utente_des_cognome = ut.UTENTE_COGNOME,
                        Analisi_utente_des_nome = ut.UTENTE_NOME,
                        Analisi_flgInterno = an.VALORI_FLG_INTERNO,
                        Analisi_flgBloccato = an.VALORI_FLG_BLOCCATO,
                        Analisi_Gruppo_id = an.VALORI_GRUPPO_GRUREP_ID,
                        Analisi_Gruppo_desc = gr != null ? gr.GRUREP_DESC : null,
                        Analisi_Reparto_id = an.VALORI_REPARTO_GRUREP_ID,
                        Analisi_Reparto_desc = rep != null ? rep.GRUREP_DESC : null,
                        Analisi_VN = an.VALORI_VN,
                        Analisi_MP_Rev = an.VALORI_MP_REV,
                        Analisi_Descrizione = an.VALORI_DESC,
                        Analisi_legge = an.VALORI_LEGGE,
                        Analisi_Codice_Descrizione =an.VALORI_CODICE_DESC, 
                       // Analisi_Codice_Descrizione = an.V
                        Analisi_Tecnica = an.VALORI_TECNICA,
                        Analisi_Dim_Lotto = an.VALORI_DIM_LOTTO,
                        Analisi_Nr_Camp_Qualita = an.VALORI_NR_CAMP_QUALITA ,
                        Analisi_Matrice = an.VALORI_MATRICE,
                        Analisi_CostoTot = an.VALORI_COSTO_TOT,
                        Analisi_flgPonderazione = an.VALORI_FLG_PONDERAZIONE,
                        Analisi_Peso_Positivo = an.VALORI_PESO_POSITIVO,
                        Analisi_T_Staval_id = an.VALORI_T_STAVAL_ID,
                        Analisi_T_Staval_desc = tv != null ? tv.T_STAVAL_DESC : null,
                        Analisi_flgIntermedio = an.VALORI_FLG_INTERM,
                        Analisi_CodiceIntermedio = an.VALORI_CODICE_INTERMEDIO ,
                        Analisi_CostoTotDelib = an.VALORI_COSTO_TOT_DELIB ,
                        Analisi_CostoTariffaDelib = an.VALORI_COSTO_TARIFFA_DELIBERATO ,
                        Analisi_CostoTariffa_D_Delib = an.VALORI_COSTO_TARIFFA_D_DELIBERATO,
                        Analisi_flg_non_Programmabili= an.VALORI_FLG_NON_PROGRAMMABILI ,
                        Analisi_VN_Data_A = an.VALORI_DATA_VN_A ,
                        Analisi_VN_Data_Da = an.VALORI_DATA_VN_DA,
                        Analisi_MP_Rev_Data_Scadenza = an.VALORI_DATA_MP_REV_SCADENZA,
                        Analisi_flgModello = an.VALORI_FLAG_MODELLO,
                        Analisi_PercCostInd = gr.GRUREP_COST_IND ,
                        Analisi_CostoDiretto = an.VALORI_COSTO_DIRETTO ,
                        Analisi_COD_VN_MP_REV_SETTORE = an.VALORI_COD_VN_MP_REV_SETTORE,
                        Analisi_COD_VN_MP_REV_SETTORE_D = an.VALORI_COD_VN_MP_REV_SETTORE_D,
                        Analisi_COD_VN_MP_REV_SETTORE_V = an.VALORI_COD_VN_MP_REV_SETTORE_V,
                        Analisi_Documento = an.VALORI_DOCUMENTO,
                        Analisi_Allegato1 = an.VALORI_ALLEGATO1,
                        Analisi_Allegato2 = an.VALORI_ALLEGATO2
                    }
                    ).Select(o => new MyAnalisi
                    {
                        Analisi_id = o.Analisi_id,
                        Analisi_utente_id = o.Analisi_utente_id.Value,
                        Analisi_utente_des_cognome = o.Analisi_utente_des_cognome,
                        Analisi_utente_des_nome = o.Analisi_utente_des_nome,
                        Analisi_flgInterno = o.Analisi_flgInterno,
                        Analisi_flgBloccato = o.Analisi_flgBloccato,
                        Analisi_Gruppo_id = o.Analisi_Gruppo_id,
                        Analisi_Gruppo_desc = o.Analisi_Gruppo_desc,
                        Analisi_Reparto_id = o.Analisi_Reparto_id,
                        Analisi_Reparto_desc = o.Analisi_Reparto_desc,
                        Analisi_VN = o.Analisi_VN,
                        Analisi_MP_Rev = o.Analisi_MP_Rev,
                        Analisi_Descrizione = o.Analisi_Descrizione,
                        Analisi_Tecnica = o.Analisi_Tecnica,
                        Analisi_Dim_Lotto = o.Analisi_Dim_Lotto,
                        Analisi_Matrice = o.Analisi_Matrice,
                        Analisi_CostoTot = o.Analisi_CostoTot,
                        Analisi_flgPonderazione = o.Analisi_flgPonderazione,
                        Analisi_Peso_Positivo = o.Analisi_Peso_Positivo,
                        Analisi_T_Staval_id = o.Analisi_T_Staval_id,
                        Analisi_T_Staval_desc = o.Analisi_T_Staval_desc,
                        Analisi_flgIntermedio = o.Analisi_flgIntermedio,
                        Analisi_CodiceGenerico = (o.Analisi_flgIntermedio || o.Analisi_flgModello) ? o.Analisi_CodiceIntermedio : o.Analisi_VN + " - " + o.Analisi_Codice_Descrizione + " - " + o.Analisi_MP_Rev,
                        Analisi_GruppoRepartoGenerico_Desc = o.Analisi_Gruppo_id != null ? o.Analisi_Gruppo_desc : o.Analisi_Reparto_desc,
                        Analisi_CostoTotDelib = o.Analisi_CostoTotDelib,
                        Analisi_flg_non_Programmabili = o.Analisi_flg_non_Programmabili,
                        Analisi_VN_Data_A = o.Analisi_VN_Data_A ,
                        Analisi_VN_Data_Da = o.Analisi_VN_Data_Da,
                        Analisi_MP_Rev_Data_Scadenza = o.Analisi_MP_Rev_Data_Scadenza ,
                        Analisi_Nr_Camp_Qualita = o.Analisi_Nr_Camp_Qualita ,
                        Analisi_flgModello = o.Analisi_flgModello ,
                        Analisi_Codice_Descrizione = o.Analisi_Codice_Descrizione,
                        Analisi_legge = o.Analisi_legge,
                        Analisi_CostoTariffaDelib = o.Analisi_CostoTariffaDelib,
                        Analisi_CostoTariffa_D_Delib=o.Analisi_CostoTariffa_D_Delib,
                        //Ric#8 Analisi_flgObsoleta = (o.Analisi_VN_Data_Da <= today && o.Analisi_VN_Data_A >= today && o.Analisi_MP_Rev_Data_Scadenza >= today) ? false : true ,
                        Analisi_flgObsoleta = (o.Analisi_VN_Data_A >= today && o.Analisi_MP_Rev_Data_Scadenza >= today) ? false : true, //Ric#8
                        Analisi_PercCostInd = o.Analisi_PercCostInd,
                        Analisi_CostoDiretto=o.Analisi_CostoDiretto,
                        Analisi_COD_VN_MP_REV_SETTORE = o.Analisi_COD_VN_MP_REV_SETTORE ,
                        Analisi_COD_VN_MP_REV_SETTORE_D = o.Analisi_COD_VN_MP_REV_SETTORE_D,
                        Analisi_COD_VN_MP_REV_SETTORE_V = o.Analisi_COD_VN_MP_REV_SETTORE_V,
                        Analisi_Documento = o.Analisi_Documento,
                        Analisi_Allegato1 = o.Analisi_Allegato1,
                        Analisi_Allegato2 = o.Analisi_Allegato2
                    }
                ).ToList<MyAnalisi>();

            return a;
        }
        public List<MyAnalisi> GetIntermediDaAnalisiPosizione(int valPos_id)
        {
            MyAnalisiPos aPos = GetGenericAnalisiPos().Where(z => z.AnalisiPos_id == valPos_id).SingleOrDefault();
            MyAnalisi analisi = GetAnalisi(aPos.AnalisiPos_MasterAnalisi_id);
            return GetAnalisi().Where(Z => Z.Analisi_flgIntermedio == true && Z.Analisi_Gruppo_id == analisi.Analisi_Gruppo_id).ToList<MyAnalisi>();
        }
        public List<MyAnalisi> GetIntermediDaProdottoPosizione(int prodPos_id)
        {
            MyProdottoPos pp =GetProdottiPos().Where(z => z.ProdottoPos_id == prodPos_id).SingleOrDefault();
            MyProdotto prod =GetProdotti(pp.ProdottoPos_MasterProdotto_id);
            return GetAnalisi().Where(z => z.Analisi_flgIntermedio == true && z.Analisi_Reparto_id == prod.Prodot_Reparto_ID).ToList<MyAnalisi>();
        }
        public MyAnalisi GetAnalisi(int analisi_id)
        {
            MyAnalisi analisi= GetAnalisi().Where(z => z.Analisi_id == analisi_id).SingleOrDefault();
            return analisi;
        }
        public MyProdotto GetAnalisiPosProdottoDaProdotto(int prodpos_id)
        {
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            PROPOS_POSIZIONI pp = ctx.PROPOS_POSIZIONI.Where(z => z.PROPOS_ID == prodpos_id).SingleOrDefault();
            int? prodot_id = pp.PROPOS_PRODOT_ID_SEC;
            if (prodot_id.HasValue)
                return GetProdotti(prodot_id.Value);
            return null;
        }
        public MyProdotto GetAnalisiPosProdotto(int valpos_id)
        {
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            VALPOS_POSIZIONI vp = ctx.VALPOS_POSIZIONI.Where(z => z.VALPOS_ID == valpos_id).SingleOrDefault();
            int? prodot_id = vp.VALPOS_PRODOT_ID;
            if (prodot_id.HasValue)
                return GetProdotti(prodot_id.Value);
            return null;
        }
        public MyMacchinario GetAnalisiPosMacchinario(int valpos_id)
        {
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            VALPOS_POSIZIONI vp = ctx.VALPOS_POSIZIONI.Where(z => z.VALPOS_ID == valpos_id).SingleOrDefault();
            int? macchi_id = vp.VALPOS_MACCHI_ID;
            if (macchi_id.HasValue)
                return GetMacchinario(macchi_id.Value);
            return null;
        }
        public MyMacchinario GetAnalisiPosMacchinarioDaProdotto(int prodpos_id)
        {
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            PROPOS_POSIZIONI vp = ctx.PROPOS_POSIZIONI.Where(z => z.PROPOS_ID == prodpos_id).SingleOrDefault();
            int? macchi_id = vp.PROPOS_MACCHI_ID;
            if (macchi_id.HasValue)
                return GetMacchinario(macchi_id.Value);
            return null;
        }
        public MyAnalisi GetAnalisiPosIntermezzo(int valpos_id)
        { 
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities(); 
            VALPOS_POSIZIONI vp=ctx.VALPOS_POSIZIONI.Where (z=>z.VALPOS_ID == valpos_id ).SingleOrDefault(); 
            int? analisi_id = vp.VALPOS_INTERM_ID ;
            if (analisi_id.HasValue)
                return GetAnalisi(analisi_id.Value);
            return null;
        }
        public MyAnalisi GetAnalisiPosIntermedioDaProdotto(int prodpos_id)
        {
            IZSLER_CAP_Entities ctx = new IZSLER_CAP_Entities();
            PROPOS_POSIZIONI vp = ctx.PROPOS_POSIZIONI.Where(z => z.PROPOS_ID == prodpos_id).SingleOrDefault();
            int? analisi_id = vp.PROPOS_INTERM_ID;
            if (analisi_id.HasValue)
                return GetAnalisi(analisi_id.Value);
            return null;
        }
        public MyAnalisi GetAnalisiGenericaDaValPos_ID(int analisiPos_id)
        {
            MyAnalisiPos anPos= GetGenericAnalisiPos().Where(z => z.AnalisiPos_id == analisiPos_id).SingleOrDefault();
            return GetAnalisi(anPos.AnalisiPos_MasterAnalisi_id);
        }

        public MyProdotto GetProdottoGenericaDaProdPos_ID(int prodottoPos_id)
        {
            MyProdottoPos prodPos = GetGenericProdottiPos().Where(z => z.ProdottoPos_id == prodottoPos_id).SingleOrDefault();
            return GetProdotti(prodPos.ProdottoPos_MasterProdotto_id);
        }
        public List<MyAnalisi> GetAnalisi()
        {
            DateTime today = DateTime.Now.Date; 
            IZSLER_CAP_Entities ctx= new IZSLER_CAP_Entities();
            List<MyAnalisi> a =
                (
                    from an in ctx.VALORI_VALORIZZAZIONI
                    join tv in ctx.T_STAVAL_STATO_VALORIZZAZIONE
                    on an.VALORI_T_STAVAL_ID equals tv.T_STAVAL_ID
                    join ut in ctx.UTENTE
                    on an.VALORI_UTENTE_ID equals ut.UTENTE_ID

                    join gr in ctx.GRUREP_GRUPPI_REPARTI.Where(z => z.GRUREP_FLG_REPARTO == false)
                    on an.VALORI_GRUPPO_GRUREP_ID equals gr.GRUREP_ID
                    into grTable
                    from gr in grTable.DefaultIfEmpty()
                    join rep in ctx.GRUREP_GRUPPI_REPARTI.Where(z => z.GRUREP_FLG_REPARTO == true)
                    on an.VALORI_REPARTO_GRUREP_ID equals rep.GRUREP_ID
                    into repTable
                    from rep in repTable.DefaultIfEmpty()
                    select new
                    {
                        Analisi_id = an.VALORI_ID,
                        Analisi_utente_id = an.VALORI_UTENTE_ID,
                        Analisi_utente_des_cognome = ut.UTENTE_COGNOME,
                        Analisi_utente_des_nome = ut.UTENTE_NOME,
                        Analisi_flgInterno = an.VALORI_FLG_INTERNO,
                        Analisi_flgBloccato = an.VALORI_FLG_BLOCCATO,
                        Analisi_Gruppo_id = an.VALORI_GRUPPO_GRUREP_ID,
                        Analisi_Gruppo_desc = gr != null ? gr.GRUREP_DESC : null,
                        Analisi_Reparto_id = an.VALORI_REPARTO_GRUREP_ID,
                        Analisi_Reparto_desc = rep != null ? rep.GRUREP_DESC : null,
                        Analisi_VN = an.VALORI_VN,
                        Analisi_MP_Rev = an.VALORI_MP_REV,
                        Analisi_Descrizione = an.VALORI_DESC,
                        Analisi_Codice_Descrizione = an.VALORI_CODICE_DESC ,
                        Analisi_Tecnica = an.VALORI_TECNICA,
                        Analisi_Dim_Lotto = an.VALORI_DIM_LOTTO,
                        Analisi_Nr_Camp_Qualita = an.VALORI_NR_CAMP_QUALITA,
                        Analisi_Matrice = an.VALORI_MATRICE,
                        Analisi_CostoTot = an.VALORI_COSTO_TOT,
                        Analisi_CostoTotDelib = an.VALORI_COSTO_TOT_DELIB,
                        Analisi_CostoTariffaDelib = an.VALORI_COSTO_TARIFFA_DELIBERATO,
                        Analisi_CostoTariffa_D_Delib = an.VALORI_COSTO_TARIFFA_D_DELIBERATO, 
                        Analisi_flgPonderazione = an.VALORI_FLG_PONDERAZIONE,
                        Analisi_Peso_Positivo = an.VALORI_PESO_POSITIVO,
                        Analisi_T_Staval_id = an.VALORI_T_STAVAL_ID,
                        Analisi_T_Staval_desc = tv != null ? tv.T_STAVAL_DESC : null,
                        Analisi_flgIntermedio = an.VALORI_FLG_INTERM,
                        Analisi_CodiceIntermedio = an.VALORI_CODICE_INTERMEDIO ,
                        Analisi_legge = an.VALORI_LEGGE,
                        Analisi_flg_non_Programmabili = an.VALORI_FLG_NON_PROGRAMMABILI,
                        Analisi_VN_Data_A = an.VALORI_DATA_VN_A,
                        Analisi_VN_Data_Da = an.VALORI_DATA_VN_DA,
                        Analisi_MP_Rev_Data_Scadenza = an.VALORI_DATA_MP_REV_SCADENZA,
                        Analisi_flgModello = an.VALORI_FLAG_MODELLO ,
                        Analisi_PercCostInd = gr.GRUREP_COST_IND ,
                        Analisi_CostoDiretto = an.VALORI_COSTO_DIRETTO ,
                        Analisi_COD_VN_MP_REV_SETTORE = an.VALORI_COD_VN_MP_REV_SETTORE,
                        Analisi_COD_VN_MP_REV_SETTORE_D = an.VALORI_COD_VN_MP_REV_SETTORE_D,
                        Analisi_COD_VN_MP_REV_SETTORE_V = an.VALORI_COD_VN_MP_REV_SETTORE_V,
                        Analisi_Documento = an.VALORI_DOCUMENTO,
                        Analisi_Allegato1 = an.VALORI_ALLEGATO1,
                        Analisi_Allegato2 = an.VALORI_ALLEGATO2,
                        Analisi_flg_assegn_al_gruppo = an.VALORI_FLG_ASSEGN_AL_GRUPPO,
                        Analisi_T_Staval_codice = tv != null ? tv.T_STAVAL_CODICE : null,
                    }
                    ).Select(o => new MyAnalisi
               {
                    Analisi_id                  =o.Analisi_id,                
                    Analisi_utente_id           =o.Analisi_utente_id.Value,         
                    Analisi_utente_des_cognome  =o.Analisi_utente_des_cognome,
                    Analisi_utente_des_nome     =o.Analisi_utente_des_nome,   
                    Analisi_flgInterno          =o.Analisi_flgInterno,        
                    Analisi_flgBloccato         =o.Analisi_flgBloccato,       
                    Analisi_Gruppo_id           =o.Analisi_Gruppo_id,         
                    Analisi_Gruppo_desc         =o.Analisi_Gruppo_desc,       
                    Analisi_Reparto_id          =o.Analisi_Reparto_id,        
                    Analisi_Reparto_desc        =o.Analisi_Reparto_desc,      
                    Analisi_VN                  =o.Analisi_VN,                
                    Analisi_MP_Rev              =o.Analisi_MP_Rev,            
                    Analisi_Descrizione         =o.Analisi_Descrizione,       
                    Analisi_Tecnica             =o.Analisi_Tecnica,           
                    Analisi_Dim_Lotto           =o.Analisi_Dim_Lotto,         
                    Analisi_Matrice             =o.Analisi_Matrice,           
                    Analisi_CostoTot            =o.Analisi_CostoTot,  
                    Analisi_CostoTotDelib       =o.Analisi_CostoTotDelib, 
                    Analisi_flgPonderazione     =o.Analisi_flgPonderazione,   
                    Analisi_Peso_Positivo       =o.Analisi_Peso_Positivo,     
                    Analisi_T_Staval_id         =o.Analisi_T_Staval_id,       
                    Analisi_T_Staval_desc       =o.Analisi_T_Staval_desc,     
                    Analisi_flgIntermedio       =o.Analisi_flgIntermedio,
                    Analisi_CodiceGenerico      = (o.Analisi_flgIntermedio || o.Analisi_flgModello) ? o.Analisi_CodiceIntermedio : o.Analisi_VN + " - " + o.Analisi_MP_Rev,
                    Analisi_GruppoRepartoGenerico_Desc = o.Analisi_Gruppo_id != null ? o.Analisi_Gruppo_desc : o.Analisi_Reparto_desc,
                    Analisi_flg_non_Programmabili = o.Analisi_flg_non_Programmabili,
                    Analisi_VN_Data_A = o.Analisi_VN_Data_A ,
                    Analisi_VN_Data_Da = o.Analisi_VN_Data_Da,
                    Analisi_MP_Rev_Data_Scadenza = o.Analisi_MP_Rev_Data_Scadenza ,
                    Analisi_Nr_Camp_Qualita = o.Analisi_Nr_Camp_Qualita,
                    Analisi_flgModello = o.Analisi_flgModello,
                    Analisi_Codice_Descrizione = o.Analisi_Codice_Descrizione ,
                    Analisi_legge = o.Analisi_legge,
                    Analisi_CostoTariffaDelib = o.Analisi_CostoTariffaDelib,
                    Analisi_CostoTariffa_D_Delib = o.Analisi_CostoTariffa_D_Delib,
                    //Ric#8 Analisi_flgObsoleta = (o.Analisi_VN_Data_Da <= today && o.Analisi_VN_Data_A >= today && o.Analisi_MP_Rev_Data_Scadenza >= today) ? false : true,
                    Analisi_flgObsoleta = (o.Analisi_VN_Data_A >= today && o.Analisi_MP_Rev_Data_Scadenza >= today) ? false : true,
                    Analisi_PercCostInd = o.Analisi_PercCostInd,
                    Analisi_CostoDiretto = o.Analisi_CostoDiretto,
                    Analisi_COD_VN_MP_REV_SETTORE = o.Analisi_COD_VN_MP_REV_SETTORE,
                    Analisi_COD_VN_MP_REV_SETTORE_D = o.Analisi_COD_VN_MP_REV_SETTORE_D,
                    Analisi_COD_VN_MP_REV_SETTORE_V = o.Analisi_COD_VN_MP_REV_SETTORE_V,
                    Analisi_Documento = o.Analisi_Documento,
                    Analisi_Allegato1 = o.Analisi_Allegato1,
                    Analisi_Allegato2 = o.Analisi_Allegato2,
                    Analisi_flg_assegn_al_gruppo = o.Analisi_flg_assegn_al_gruppo,
                    Analisi_T_Staval_codice = o.Analisi_T_Staval_codice
               }
                ).ToList<MyAnalisi>();

            return a;
        }
        public Profili GetProfilo(int User_id,int profili_id)
        {
        //    Profili profilo = GetProfili(User_id).Where(z => z.ProfiloID == profili_id).SingleOrDefault();
          //  return profilo;
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            
            
            Profili profilo  =
                (from prof in context.PROFIL_PROFILI.Where(pro => pro.PROFIL_ID == profili_id)
                     select new { prof.PROFIL_ID, prof.PROFIL_DESC, prof.PROFIL_CODICE }).
                     Select(c => new Profili
                     {
                         ProfiloID = c.PROFIL_ID,
                         ProfiloDesc = c.PROFIL_DESC,
                         ProfiloCodice = c.PROFIL_CODICE
                     }).SingleOrDefault() ;

            //foreach (Profili p in a)
            //{

            
            List<Gruppo> lstGr =
                (
                           from pr in context.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(p1 => p1.M_UTPRGR_PROFIL_ID == profili_id && p1.M_UTPRGR_UTENTE_ID == User_id && p1.M_UTPRGR_GRUREP_ID !=null)
                           from gr in context.GRUREP_GRUPPI_REPARTI
                           .Where(g => g.GRUREP_ID == pr.M_UTPRGR_GRUREP_ID).DefaultIfEmpty()

                           select new { gr.GRUREP_ID, gr.GRUREP_DESC, pr.M_UTPRGR_FLG_PRINCIPALE, gr.GRUREP_FLG_REPARTO })

                           .Select(c => new Gruppo
                           {
                               GruppoID = c.GRUREP_ID,
                               GruppoDesc = c.GRUREP_DESC,
                               FlgPrincipale = c.M_UTPRGR_FLG_PRINCIPALE,
                               FlgIsReparto = c.GRUREP_FLG_REPARTO
                           }).ToList<Gruppo>();

            profilo.ElencoGruppi = new List<Gruppo>(lstGr);

            //}
            return profilo;
        }
        

        public List<Profili> GetProfili(int User_id)
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<Profili> a = (
                     from pr in context.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(p2 => p2.M_UTPRGR_UTENTE_ID == User_id && p2.M_UTPRGR_PROFIL_ID !=null && p2.M_UTPRGR_GRUREP_ID !=null)
                     from prof in context.PROFIL_PROFILI.Where(pro => pro.PROFIL_ID == pr.M_UTPRGR_PROFIL_ID).Distinct().DefaultIfEmpty()

                     select new { prof.PROFIL_ID, prof.PROFIL_DESC,prof.PROFIL_CODICE}).Distinct().Select(c => new Profili
                     {
                         ProfiloID = c.PROFIL_ID,
                         ProfiloDesc = c.PROFIL_DESC,
                         ProfiloCodice= c.PROFIL_CODICE
                     }).ToList<Profili>();

            foreach (Profili p in a)
            {

                int idProf = p.ProfiloID;
                List<Gruppo> lstGr =
                    (
                               from pr in context.M_UTPRGR_UTENTI_PROFILI_GRUPPI.Where(p1 => p1.M_UTPRGR_PROFIL_ID == idProf && p1.M_UTPRGR_GRUREP_ID != null && p1.M_UTPRGR_UTENTE_ID == User_id)
                               from gr in context.GRUREP_GRUPPI_REPARTI
                               .Where(g => g.GRUREP_ID == pr.M_UTPRGR_GRUREP_ID).DefaultIfEmpty()

                               select new { gr.GRUREP_ID, gr.GRUREP_DESC, pr.M_UTPRGR_FLG_PRINCIPALE ,gr.GRUREP_FLG_REPARTO })

                               .Select(c => new Gruppo
                               {
                                   GruppoID = c.GRUREP_ID,
                                   GruppoDesc = c.GRUREP_DESC,
                                   FlgPrincipale = c.M_UTPRGR_FLG_PRINCIPALE,
                                   FlgIsReparto = c.GRUREP_FLG_REPARTO 
                               }).ToList<Gruppo>();

                p.ElencoGruppi = new List<Gruppo>(lstGr);

            }
            return a;
        }

        public MyProdotto GetProdotti(int prodot_id)
        {
            return GetProdotti().Where(z => z.Prodot_ID == prodot_id).SingleOrDefault();
        }
        
        public List<MyProdotto> GetProdotti()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyProdotto> a = (from pr in context.PRODOT_PRODOTTI
                                  join ts in context.T_STAPRO_STATO_PRODOTTO
                                  on pr.PRODOT_T_STAPRO_ID equals ts.T_STAPRO_ID
                                  into tempTable
                                  from ts in tempTable.DefaultIfEmpty()
                                  join um in context.T_UNIMIS_UNITA_MISURA
                                  on pr.PRODOT_T_UNIMIS_ID equals um.T_UNIMIS_ID
                                  into umtable
                                  from um in umtable.DefaultIfEmpty()
                                  
                                  join umSec in context.T_UNIMIS_UNITA_MISURA
                                  on pr.PRODOT_T_UNIMIS_ID_SEC equals umSec.T_UNIMIS_ID
                                  into umSectable
                                  from umSec in umSectable.DefaultIfEmpty()


                                  join ut in context.UTENTE
                                  on pr.PRODOT_UTENTE_ID equals ut.UTENTE_ID
                                  into uttable
                                  from ut in uttable.DefaultIfEmpty()
                                  join gr in context.GRUREP_GRUPPI_REPARTI 
                                  on pr.PRODOT_REPARTO_GRUREP_ID equals gr.GRUREP_ID 
                                  into grTable 
                                  from gr in grTable.DefaultIfEmpty ()
                                  select new
                                  {
                                      Prodot_ID = pr.PRODOT_ID,
                                      Prodot_Codice = pr.PRODOT_CODICE,
                                      Prodot_CodiceGenerico = pr.PRODOT_CODICE!=null?pr.PRODOT_CODICE :"" + pr.PRODOT_CODICE_DESC !=null?" - " + pr.PRODOT_CODICE_DESC :"" ,
                                      Prodot_Desc = pr.PRODOT_DESC,
                                      Prodot_Desc_Estesa = pr.PRODOT_DESC_ESTESA,
                                      Prodot_Codice_Desc= pr.PRODOT_CODICE_DESC ,
                                      Prodotto_CoeffConversione= pr.PRODOT_COEFF_CONVERSIONE ,
                                      Prodot_CostoUnitario = pr.PRODOT_COSTOUNITARIO ,
                                      Prodot_CostoUnitario_Deliberato = pr.PRODOT_COSTOUNITARIO_DELIBE,
                                      Prodot_UnitaMisura_descrizione = um != null ? um.T_UNIMIS_DESC : null,
                                      Prodot_UnitaMisura_descrizione_Sec =umSec !=null? umSec.T_UNIMIS_DESC :null,
                                      Prodot_UnitaMisura_ID = pr.PRODOT_T_UNIMIS_ID ,
                                      Prodot_UnitaMisura_ID_Sec = pr.PRODOT_T_UNIMIS_ID_SEC ,

                                      Prodot_Flg_Bloccato = pr.PRODOT_FLG_BLOCCATO,
                                      Prodot_Stato_Descrizione = ts != null ? ts.T_STAPRO_DESCRIZIONE : null,
                                      Prodot_utente_denominazione = ut != null ? (ut.UTENTE_COGNOME + " " + ut.UTENTE_NOME) : null,
                                      Prodot_utente_id = pr.PRODOT_UTENTE_ID,
                                      Prodot_T_Stapro_Id = pr.PRODOT_T_STAPRO_ID,
                                      T_Stapro_color = ts != null ? ts.T_STAPRO_COLOR : null,
                                      Prodot_Dim_Lotto=pr.PRODOT_DIM_LOTTO,
                                      Prodot_Nr_Camp_Qualita = pr.PRODOT_NR_CAMP_QUALITA,
                                      Prodot_Reparto_ID = pr.PRODOT_REPARTO_GRUREP_ID,
                                      Prodot_Reparto_Desc = gr.GRUREP_DESC ,
                                      Prodot_Flg_Interno= pr.PRODOT_FLG_INTERNO ,
                                      Prodot_Flg_Bloccato_Magazzino=pr.PRODOT_FLG_BLOCCATO_MAGAZZINO ,
                                      Prodot_Stima_Prod_Anno = pr.PRODOT_STIMA_PROD_ANNO ,
                                      Prodot_Perc_Scarto = pr.PRODOT_PERC_SCARTO ,
                                      Prodot_Costo_Tariffa = pr.PRODOT_TARIFFA ,
                                      Prodot_Tariffa_Proposta =pr.PRODOT_TARIFFA_PROPOSTA ,
                                      Prodot_HASHKEY = pr.PRODOT_HASHKEY ,
                                      Prodot_PercCostInd = gr.GRUREP_COST_IND ,
                                      Prodot_Documento = pr.PRODOT_DOCUMENTO,
                                      //Ric#3
                                      Prodot_flg_assegn_al_gruppo = pr.PRODOT_FLG_ASSEGN_AL_GRUPPO,
                                      //Ric#3
                                      Prodot_T_Stapro_codice = ts.T_STAPRO_CODICE

                                  }
               ).Select(o => new MyProdotto
               {
                   Prodot_ID = o.Prodot_ID,
                   Prodot_Codice = o.Prodot_Codice,
                   Prodot_Desc = o.Prodot_Desc,
                   Prodot_Desc_Estesa =o.Prodot_Desc_Estesa,
                   Prodot_CostoUnitario = o.Prodot_CostoUnitario,
                   Prodot_CostoUnitario_Deliberato= o.Prodot_CostoUnitario_Deliberato,
                   Prodot_UnitaMisura_descrizione = o.Prodot_UnitaMisura_descrizione,
                   Prodot_UnitaMisura_ID = o.Prodot_UnitaMisura_ID ,
                   Prodot_UnitaMisura_descrizione_Sec  = o.Prodot_UnitaMisura_descrizione_Sec ,
                   Prodot_UnitaMisura_ID_Sec  = o.Prodot_UnitaMisura_ID_Sec ,
                   Prodot_Flg_Bloccato = o.Prodot_Flg_Bloccato,
                   Prodot_Stato_Descrizione = o.Prodot_Stato_Descrizione,
                   Prodot_utente_denominazione = o.Prodot_utente_denominazione,
                   Prodot_utente_id = o.Prodot_utente_id,
                   Prodot_T_Stapro_Id = o.Prodot_T_Stapro_Id,
                   T_Stapro_color = o.T_Stapro_color,
                   Prodot_Flg_Interno = o.Prodot_Flg_Interno ,
                   Prodot_Reparto_ID = o.Prodot_Reparto_ID ,
                   Prodot_Reparto_Desc = o.Prodot_Reparto_Desc ,
                   Prodot_Dim_Lotto = o.Prodot_Dim_Lotto ,
                   Prodot_Nr_Camp_Qualita = o.Prodot_Nr_Camp_Qualita,
                   Prodot_Flg_Bloccato_Magazzino = o.Prodot_Flg_Bloccato_Magazzino,
                   Prodot_Codice_Desc =o.Prodot_Codice_Desc ,
                   Prodotto_CoeffConversione =o.Prodotto_CoeffConversione,
                   Prodot_CodiceGenerico = o.Prodot_CodiceGenerico ,
                   Prodot_Stima_Prod_Anno = o.Prodot_Stima_Prod_Anno,
                   Prodot_Perc_Scarto = o.Prodot_Perc_Scarto ,
                   Prodot_Costo_Tariffa = o.Prodot_Costo_Tariffa ,
                   Prodot_Tariffa_Proposta =o.Prodot_Tariffa_Proposta,
                   Prodot_HASHKEY = o.Prodot_HASHKEY,
                   Prodot_PercCostInd =o.Prodot_PercCostInd,
                   Prodot_Documento = o.Prodot_Documento,
                   //ric#3
                   Prodot_flg_assegn_al_gruppo = o.Prodot_flg_assegn_al_gruppo,
                   //ric#3
                   Prodot_T_Stapro_codice = o.Prodot_T_Stapro_codice
               }).ToList<MyProdotto>()

               ;
            return  a;
        }

        public List<MyProdotto> GetProdottiPerProfilo(Profili profilo)
        {
            List<int> elencoGruppiProfilo = new List<int>();
            foreach (Gruppo g in profilo.ElencoGruppi)
            {
                if (!elencoGruppiProfilo.Contains(g.GruppoID))
                    elencoGruppiProfilo.Add(g.GruppoID);
            }
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyProdotto> a = (from pr in context.PRODOT_PRODOTTI
                                  join ts in context.T_STAPRO_STATO_PRODOTTO
                                  on pr.PRODOT_T_STAPRO_ID equals ts.T_STAPRO_ID
                                  into tempTable
                                  from ts in tempTable.DefaultIfEmpty()
                                  join um in context.T_UNIMIS_UNITA_MISURA
                                  on pr.PRODOT_T_UNIMIS_ID equals um.T_UNIMIS_ID
                                  into umtable
                                  from um in umtable.DefaultIfEmpty()

                                  join umSec in context.T_UNIMIS_UNITA_MISURA
                                  on pr.PRODOT_T_UNIMIS_ID_SEC equals umSec.T_UNIMIS_ID
                                  into umSectable
                                  from umSec in umSectable.DefaultIfEmpty()

                                  join ut in context.UTENTE
                                  on pr.PRODOT_UTENTE_ID equals ut.UTENTE_ID
                                  into uttable
                                  from ut in uttable.DefaultIfEmpty()
                                  join gr in context.GRUREP_GRUPPI_REPARTI
                                  on pr.PRODOT_REPARTO_GRUREP_ID equals gr.GRUREP_ID
                                  into grTable
                                  from gr in grTable.DefaultIfEmpty()
                                  where (elencoGruppiProfilo.Contains(gr.GRUREP_ID))
                                  select new
                                  {
                                      Prodot_ID = pr.PRODOT_ID,
                                      Prodot_Codice = pr.PRODOT_CODICE,
                                      Prodot_CodiceGenerico = pr.PRODOT_CODICE != null ? pr.PRODOT_CODICE : "" + pr.PRODOT_CODICE_DESC != null ? " - " + pr.PRODOT_CODICE_DESC : "",
                                      Prodot_Desc = pr.PRODOT_DESC,
                                      Prodot_Desc_Estesa = pr.PRODOT_DESC_ESTESA,
                                      Prodot_Codice_Desc = pr.PRODOT_CODICE_DESC,
                                      Prodotto_CoeffConversione = pr.PRODOT_COEFF_CONVERSIONE,
                                      Prodot_CostoUnitario = pr.PRODOT_COSTOUNITARIO,
                                      Prodot_CostoUnitario_Deliberato = pr.PRODOT_COSTOUNITARIO_DELIBE,
                                      Prodot_UnitaMisura_descrizione = um != null ? um.T_UNIMIS_DESC : null,
                                      Prodot_UnitaMisura_descrizione_Sec = umSec != null ? umSec.T_UNIMIS_DESC : null,
                                      Prodot_UnitaMisura_ID = pr.PRODOT_T_UNIMIS_ID,
                                      Prodot_UnitaMisura_ID_Sec = pr.PRODOT_T_UNIMIS_ID_SEC,

                                      Prodot_Flg_Bloccato = pr.PRODOT_FLG_BLOCCATO,
                                      Prodot_Stato_Descrizione = ts != null ? ts.T_STAPRO_DESCRIZIONE : null,
                                      Prodot_utente_denominazione = ut != null ? (ut.UTENTE_COGNOME + " " + ut.UTENTE_NOME) : null,
                                      Prodot_utente_id = pr.PRODOT_UTENTE_ID,
                                      Prodot_T_Stapro_Id = pr.PRODOT_T_STAPRO_ID,
                                      T_Stapro_color = ts != null ? ts.T_STAPRO_COLOR : null,
                                      Prodot_Dim_Lotto = pr.PRODOT_DIM_LOTTO,
                                      Prodot_Nr_Camp_Qualita = pr.PRODOT_NR_CAMP_QUALITA,
                                      Prodot_Reparto_ID = pr.PRODOT_REPARTO_GRUREP_ID,
                                      Prodot_Reparto_Desc = gr.GRUREP_DESC,
                                      Prodot_Flg_Interno = pr.PRODOT_FLG_INTERNO,
                                      Prodot_Flg_Bloccato_Magazzino = pr.PRODOT_FLG_BLOCCATO_MAGAZZINO ,
                                      Prodot_Stima_Prod_Anno = pr.PRODOT_STIMA_PROD_ANNO,
                                      Prodot_Perc_Scarto = pr.PRODOT_PERC_SCARTO, 
                                      Prodot_Costo_Tariffa = pr.PRODOT_TARIFFA ,
                                      Prodot_Tariffa_Proposta =pr.PRODOT_TARIFFA_PROPOSTA ,
                                      Prodot_HASHKEY = pr.PRODOT_HASHKEY ,
                                      Prodot_PercCostInd =gr.GRUREP_COST_IND ,
                                      Prodot_Documento = pr.PRODOT_DOCUMENTO

                                  }
              ).Select(o => new MyProdotto
              {
                  Prodot_ID = o.Prodot_ID,
                  Prodot_Codice = o.Prodot_Codice,
                  Prodot_Desc = o.Prodot_Desc,
                  Prodot_Desc_Estesa = o.Prodot_Desc_Estesa,
                  Prodot_CostoUnitario = o.Prodot_CostoUnitario,
                  Prodot_CostoUnitario_Deliberato = o.Prodot_CostoUnitario_Deliberato,
                  Prodot_UnitaMisura_descrizione = o.Prodot_UnitaMisura_descrizione,
                  Prodot_UnitaMisura_ID = o.Prodot_UnitaMisura_ID,
                  Prodot_UnitaMisura_descrizione_Sec = o.Prodot_UnitaMisura_descrizione_Sec,
                  Prodot_UnitaMisura_ID_Sec = o.Prodot_UnitaMisura_ID_Sec,
                  Prodot_Flg_Bloccato = o.Prodot_Flg_Bloccato,
                  Prodot_Stato_Descrizione = o.Prodot_Stato_Descrizione,
                  Prodot_utente_denominazione = o.Prodot_utente_denominazione,
                  Prodot_utente_id = o.Prodot_utente_id,
                  Prodot_T_Stapro_Id = o.Prodot_T_Stapro_Id,
                  T_Stapro_color = o.T_Stapro_color,
                  Prodot_Flg_Interno = o.Prodot_Flg_Interno,
                  Prodot_Reparto_ID = o.Prodot_Reparto_ID,
                  Prodot_Reparto_Desc = o.Prodot_Reparto_Desc,
                  Prodot_Dim_Lotto = o.Prodot_Dim_Lotto,
                  Prodot_Nr_Camp_Qualita = o.Prodot_Nr_Camp_Qualita,
                  Prodot_Flg_Bloccato_Magazzino = o.Prodot_Flg_Bloccato_Magazzino,
                  Prodot_Codice_Desc = o.Prodot_Codice_Desc,
                  Prodotto_CoeffConversione = o.Prodotto_CoeffConversione ,
                  Prodot_CodiceGenerico = o.Prodot_CodiceGenerico,
                  Prodot_Stima_Prod_Anno = o.Prodot_Stima_Prod_Anno,
                  Prodot_Perc_Scarto = o.Prodot_Perc_Scarto,
                  Prodot_Costo_Tariffa = o.Prodot_Costo_Tariffa,
                  Prodot_Tariffa_Proposta =o.Prodot_Tariffa_Proposta ,
                  Prodot_HASHKEY = o.Prodot_HASHKEY,
                  Prodot_PercCostInd = o.Prodot_PercCostInd,
                  Prodot_Documento = o.Prodot_Documento
              }).ToList<MyProdotto>();
            return a;

        }

        public MyRichiesta GetRichiesta(int richie_id)
        {
            return GetRichieste().Where(z => z.Richie_id == richie_id).DefaultIfEmpty().ToList<MyRichiesta>()[0] ;
        }
        public List<MyRichiesta> GetRichieste()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MyRichiesta> a = (from ri in context.RICHIE_RICHIESTE
                                   join tr in context.T_RICHIE_TIPO_RICHIESTA
                                   on ri.RICHIE_T_RICHIE_ID equals tr.T_RICHIE_ID
                                   into trTable
                                   from tr in trTable.DefaultIfEmpty()
                                   join ur in context.UTENTE
                                   on ri.RICHIE_RICHIEDENTE_UTENTE_ID equals ur.UTENTE_ID
                                   into urTable
                                   from ur in urTable.DefaultIfEmpty()
                                   join ud in context.UTENTE
                                   on ri.RICHIE_DESTINATARIO_UTENTE_ID equals ud.UTENTE_ID
                                   into udTable
                                   from ud in udTable.DefaultIfEmpty()
                                   join st in context.T_STARIC_STATO_RICHIESTA
                                   on ri.RICHIE_T_STARIC_ID equals st.T_STARIC_ID
                                   into stTable
                                   from st in stTable.DefaultIfEmpty()
                                   join va in context.VALORI_VALORIZZAZIONI
                                   on ri.RICHIE_VALORI_ID equals va.VALORI_ID
                                   into vaTable
                                   from va in vaTable.DefaultIfEmpty()
                                   join pr in context.T_RICPRI_PRIORITA_RICHIESTA
                                   on ri.RICHIE_T_RICPRI_ID equals pr.T_RICPRI_ID
                                   into prTable
                                   from pr in prTable.DefaultIfEmpty()
                                   join prod in context.PRODOT_PRODOTTI 
                                   on ri.RICHIE_PRODOT_ID equals prod.PRODOT_ID 
                                   into prodTable
                                   from prod in prodTable.DefaultIfEmpty()
                                   select new
                                   {
                                       Richie_id = ri.RICHIE_ID,
                                       Richie_codice = ri.RICHIE_CODICE,
                                       Richie_titolo = ri.RICHIE_TITOLO,
                                       Richie_data_richiesta = ri.RICHIE_DATA_RICHIESTA,
                                       Richie_t_richie_id = ri.RICHIE_T_RICHIE_ID,
                                       Richie_richiedente_utente_id = ri.RICHIE_RICHIEDENTE_UTENTE_ID,
                                       Richie_destinatario_utente_id = ri.RICHIE_DESTINATARIO_UTENTE_ID,
                                       Richie_testo = ri.RICHIE_TESTO,
                                       Richie_t_staric_id = ri.RICHIE_T_STARIC_ID,
                                       Richie_valori_id = ri.RICHIE_VALORI_ID,
                                       Richie_prodotto_id=ri.RICHIE_PRODOT_ID ,
                                       Richie_t_ricpri_id = ri.RICHIE_T_RICPRI_ID,
                                       T_Richie_desc = tr != null ? tr.T_RICHIE_DESC : null,

                                       T_Richie_color = tr != null ? tr.T_RICHIE_COLOR : null,
                                       Richie_utente_ric_nome = ur != null ? ur.UTENTE_NOME : null,
                                       Richie_utente_ric_cognome = ur != null ? ur.UTENTE_COGNOME : null,
                                       Richie_utente_des_nome = ud != null ? ud.UTENTE_NOME : null,
                                       Richie_utente_des_cognome = ud != null ? ud.UTENTE_COGNOME : null,
                                       Richie_utente_destinatario = ud != null ? (ud.UTENTE_COGNOME + " " + ud.UTENTE_NOME) : null,
                                       T_staric_desc = st != null ? st.T_STARIC_DESC : null,
                                       Richie_valorizzazione = va != null ? (  (va.VALORI_FLG_INTERM || va.VALORI_FLAG_MODELLO) ? va.VALORI_CODICE_INTERMEDIO : va.VALORI_VN + " - " + va.VALORI_CODICE_DESC + " - " + va.VALORI_MP_REV ) :null,
                                       T_Ricpri_desc = pr != null ? pr.T_RICPRI_DESC : null,
                                       T_Ricpri_color = pr != null ? pr.T_RICPRI_COLOR : null,
                                       T_Richie_codice = tr != null ? tr.T_RICHIE_CODICE : null,
                                       //Richie_prodotto_descrizione = prod!=null ? prod.PRODOT_CODICE:null 
                                       Richie_prodotto_descrizione = prod != null ? (prod.PRODOT_CODICE != null ? prod.PRODOT_CODICE : "" + prod.PRODOT_CODICE_DESC != null ? " - " + prod.PRODOT_CODICE_DESC : "") : null,
                                       Richie_flg_assegn_al_gruppo = ri.RICHIE_FLG_ASSEGN_AL_GRUPPO

                                   }
          ).Select(o => new MyRichiesta
          {
              Richie_id = o.Richie_id,
              Richie_codice = o.Richie_codice,
              Richie_titolo = o.Richie_titolo,
              Richie_data_richiesta = o.Richie_data_richiesta,
              Richie_t_richie_id = o.Richie_t_richie_id,
              Richie_richiedente_utente_id = o.Richie_richiedente_utente_id,
              Richie_destinatario_utente_id = o.Richie_destinatario_utente_id,
              Richie_testo = o.Richie_testo,
              Richie_t_staric_id = o.Richie_t_staric_id,
              Richie_valori_id = o.Richie_valori_id,
              Richie_prodotto_id = o.Richie_prodotto_id,
              Richie_t_ricpri_id = o.Richie_t_ricpri_id,
              T_Richie_desc = o.T_Richie_desc,
              T_Richie_color = o.T_Richie_color,
              Richie_utente_ric_nome = o.Richie_utente_ric_nome,
              Richie_utente_ric_cognome = o.Richie_utente_ric_cognome,
              Richie_utente_des_nome = o.Richie_utente_des_nome,
              Richie_utente_des_cognome = o.Richie_utente_des_cognome,
              T_staric_desc = o.T_staric_desc,
              Richie_valorizzazione = o.Richie_valorizzazione,
              T_Ricpri_desc = o.T_Ricpri_desc,
              T_Ricpri_color = o.T_Ricpri_color,
              T_Richie_codice = o.T_Richie_codice,
              Richie_prodotto_descrizione = o.Richie_prodotto_descrizione,
              Richie_flg_assegn_al_gruppo = o.Richie_flg_assegn_al_gruppo
          }).OrderByDescending(x => x.Richie_data_richiesta).ToList<MyRichiesta>();
            return a;
        }
        public List<TrackingRichiesta> GetRichiestaWorkflow(int richie_id)
        {
            return GetTrackingRichiesta().Where(z => z.Trkric_richie_id == richie_id).ToList<TrackingRichiesta>(); 
        }

        public List<MySettings> GetSettings()
        { 
             IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            
            List<MySettings> a =(from set  in context.T_SETTIN_SETTINGS 
                                 select new {
                                     Settings_Id = set.T_SETTIN_ID  ,
                                     Settings_Codice = set.T_SETTIN_CODICE ,
                                     Settings_Value = set.T_SETTIN_VALUE 
                                 }
                                 ).Select (o=>new MySettings 
                                 {
                                     Settings_Id = o.Settings_Id ,
                                     Settings_Codice = o.Settings_Codice ,
                                     Settings_Value = o.Settings_Value 
                                 })
                                 .ToList<MySettings>();
                return a;

        }
        public string GetSettings(string codice)
        {
            try
            {
                return GetSettings().Where(z => z.Settings_Codice == codice).SingleOrDefault().Settings_Value;
            }
            catch { }
            return ""; 
        }

        public string GetCostoInd(MyAnalisi analisi)
        {
            try
            {
               string ret = GetGruppo(analisi.Analisi_Gruppo_id.Value).Grurep_Cost_Ind;
               if (ret == null) return "0";
               return ret;
            }
            catch { }
            return "0";
        }
        public string GetCostoInd(MyProdotto prodotto)
        {
            try
            {
                string ret = GetReparto(prodotto.Prodot_Reparto_ID.Value).Grurep_Cost_Ind;
                if (ret == null) return "0";
                return ret;
            }
            catch { }
            return "0";
        }
        public List<Indirizzi> GetIndirizzi()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<Indirizzi> a =
                        (from ut in context.UTENTE.Where(i => i.UTENTE_LOCK == false)
                         join m in context.M_UTPRGR_UTENTI_PROFILI_GRUPPI
                         on ut.UTENTE_ID equals m.M_UTPRGR_UTENTE_ID
                         join pr in context.PROFIL_PROFILI
                         on m.M_UTPRGR_PROFIL_ID  equals pr.PROFIL_ID
                         select new
                         {
                             Id = ut.UTENTE_ID,
                             Nome = ut.UTENTE_NOME,
                             Cognome = ut.UTENTE_COGNOME ,
                             Email = ut.UTENTE_EMAIL,
                             prof_cod = pr.PROFIL_CODICE,
                             prof_id = pr.PROFIL_ID
                         }
                         ).Select
                         (k =>
                             new Indirizzi
                             {
                                 Id = k.Id,
                                 Nome = k.Nome,
                                 Cognome = k.Cognome,
                                 Email = k.Email,
                                 Profilo_cod = k.prof_cod,
                                 Profilo_id = k.prof_id
                             }
                          ).ToList<Indirizzi>();

            return a;
        }

        public List<PrioritaRichiesta> GetPrioritaRichiesta()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<PrioritaRichiesta> a = (from pr in context.T_RICPRI_PRIORITA_RICHIESTA

                                  select new
                                  {   
                                      T_Ricpri_id = pr.T_RICPRI_ID,
                                      T_Ricpri_desc = pr.T_RICPRI_DESC,
                                      T_Ricpri_color = pr.T_RICPRI_COLOR,
                                      T_Ricpri_codice = pr.T_RICPRI_CODICE
                                  }
               ).Select(o => new PrioritaRichiesta
               {
                   T_Ricpri_id = o.T_Ricpri_id,
                   T_Ricpri_desc = o.T_Ricpri_desc,
                   T_Ricpri_color = o.T_Ricpri_color,
                   T_Ricpri_codice = o.T_Ricpri_codice
               }).OrderBy(x => x.T_Ricpri_id).ToList<PrioritaRichiesta>()

               ;
            return a;
        }

        public List<TrackingRichiesta> GetTrackingRichiesta()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<TrackingRichiesta> a = (from trk in context.TRKRIC_RICHIESTE_TRACKING
                                         join ric in context.RICHIE_RICHIESTE
                                         on trk.TRKRIC_RICHIE_ID equals ric.RICHIE_ID 
                                         
                                         join t_ric in context.T_RICHIE_TIPO_RICHIESTA 
                                         on ric.RICHIE_T_RICHIE_ID  equals  t_ric.T_RICHIE_ID
                                       
                                         join pr in context.T_RICPRI_PRIORITA_RICHIESTA
                                         on trk.TRKRIC_T_RICPRI_ID equals pr.T_RICPRI_ID
                                         into prTable
                                         from pr in prTable.DefaultIfEmpty()
                                         join st in context.T_STARIC_STATO_RICHIESTA
                                         on trk.TRKRIC_T_STARIC_ID equals st.T_STARIC_ID
                                         into stTable
                                         from st in stTable.DefaultIfEmpty()
                                         join ut in context.UTENTE
                                         on trk.TRKRIC_UTENTE_ID equals ut.UTENTE_ID
                                         into utTable
                                         from ut in utTable.DefaultIfEmpty()

                                         select new
                                         {
                                             
                                            Trkric_id  = trk.TRKRIC_ID,
                                            Trkric_richie_id = trk.TRKRIC_RICHIE_ID,
                                            Trkric_data_ins  = trk.TRKRIC_DATA_INS,
                                            Trkric_UTENTE_ID  = trk.TRKRIC_UTENTE_ID,
                                            Trkric_t_staric_id  = trk.TRKRIC_T_STARIC_ID,
                                            Trkric_codice  = ric.RICHIE_CODICE,
                                            Trkric_titolo  = trk.TRKRIC_TITOLO,
                                            Trkric_testo  = trk.TRKRIC_TESTO,
                                            Trkric_t_ricpri_id  = trk.TRKRIC_T_RICPRI_ID,
                                            Trkric_t_staric_desc  =  st != null ? st.T_STARIC_DESC : null,
                                            Trkric_utente_nome  = ut != null ? ut.UTENTE_NOME : null,
                                            Trkric_utente_cognome =  ut != null ? ut.UTENTE_COGNOME : null,
                                            Trkric_t_ricpri_desc = pr != null ? pr.T_RICPRI_DESC : null,
                                            Trkric_t_richie_desc = t_ric.T_RICHIE_DESC ,
                                            Trkric_t_richie_color =t_ric.T_RICHIE_COLOR,
                                            Trkric_t_ricpri_color = pr != null ? pr.T_RICPRI_COLOR :null,
                                         
                                             
                                         }
               ).Select(o => new TrackingRichiesta
               {
                   
                    Trkric_id  = o.Trkric_id,
                    Trkric_richie_id = o.Trkric_richie_id,
                    Trkric_data_ins  = o.Trkric_data_ins,
                    Trkric_UTENTE_ID = o.Trkric_UTENTE_ID, 
                    Trkric_t_staric_id = o.Trkric_t_staric_id, 
                    Trkric_codice = o.Trkric_codice,
                    Trkric_titolo = o.Trkric_titolo,
                    Trkric_testo = o.Trkric_testo, 
                    Trkric_t_ricpri_id = o.Trkric_t_ricpri_id,  
                    Trkric_t_staric_desc = o.Trkric_t_staric_desc, 
                    Trkric_utente_nome = o.Trkric_utente_nome,
                    Trkric_utente_cognome = o.Trkric_utente_cognome,
                    Trkric_t_ricpri_desc = o.Trkric_t_ricpri_desc,
                    Trkric_t_richie_desc = o.Trkric_t_richie_desc,
                    Trkric_t_richie_color = o.Trkric_t_richie_color,
                    Trkric_t_ricpri_color = o.Trkric_t_ricpri_color
                    

               }).OrderBy(x => x.Trkric_id).ToList<TrackingRichiesta>()

               ;
            return a;
        }

        public List<MySollecito> GetSollecito()
        {
            IZSLER_CAP_Entities context = new IZSLER_CAP_Entities();
            List<MySollecito> a = (from sol in context.SOLLEC_SOLLECITI
                                       join ric in context.RICHIE_RICHIESTE
                                       on sol.SOLLEC_RICHIE_ID equals ric.RICHIE_ID
                                       join ut in context.UTENTE
                                       on sol.SOLLEC_SOLLECITANTE_UTENTE_ID equals ut.UTENTE_ID
                                       into utTable
                                       from ut in utTable.DefaultIfEmpty()
                                       join ute in context.UTENTE
                                       on sol.SOLLEC_SOLLECITATO_UTENTE_ID equals ute.UTENTE_ID
                                       into uteTable
                                       from ute in uteTable.DefaultIfEmpty()


                                         select new
                                         {
                                             Sollec_id = sol.SOLLEC_ID,
                                             Sollec_data_ins = sol.SOLLEC_DATA_INS,
                                             Sollec_richie_id = sol.SOLLEC_RICHIE_ID,
                                             Sollec_sollecitante_utente_id = sol.SOLLEC_SOLLECITANTE_UTENTE_ID,
                                             Sollec_sollecitato_utente_id = sol.SOLLEC_SOLLECITATO_UTENTE_ID,
                                             Sollec_messaggio = sol.SOLLEC_MESSAGGIO,
                                             Sollec_data_scadenza = sol.SOLLEC_DATA_SCADENZA,
                                             Sollec_sollecitante_nome = ut != null ? ut.UTENTE_NOME : null,
                                             Sollec_sollecitante_cognome = ut != null ? ut.UTENTE_COGNOME : null,
                                             Sollec_sollecitato_nome = ute != null ? ute.UTENTE_NOME : null,
                                             Sollec_sollecitato_cognome = ute != null ? ute.UTENTE_COGNOME : null,
                                             Sollec_rich_codice = ric.RICHIE_CODICE,
                                             Sollec_archiviato = sol.SOLLEC_ARCHIVIATO
                                         }
               ).Select(o => new MySollecito
               {

                   Sollec_id = o.Sollec_id,
                   Sollec_data_ins = o.Sollec_data_ins,
                   Sollec_richie_id = o.Sollec_richie_id,
                   Sollec_sollecitante_utente_id = o.Sollec_sollecitante_utente_id,
                   Sollec_sollecitato_utente_id = o.Sollec_sollecitato_utente_id,
                   Sollec_messaggio = o.Sollec_messaggio,
                   Sollec_data_scadenza = o.Sollec_data_scadenza,
                   Sollec_sollecitante_nome = o.Sollec_sollecitante_nome,
                   Sollec_sollecitante_cognome = o.Sollec_sollecitante_cognome,
                   Sollec_sollecitato_nome = o.Sollec_sollecitato_nome,
                   Sollec_sollecitato_cognome = o.Sollec_sollecitato_cognome,
                   Sollec_rich_codice = o.Sollec_rich_codice,
                   Sollec_archiviato = o.Sollec_archiviato


               }).OrderBy(x => x.Sollec_id).ToList<MySollecito>()

               ;
            return a;
        }
        public IEnumerable<SelectListItem> ListMyFaseToSLI(List<MyFase> elenco)
        {
            return ListMyFaseToSLI(elenco, null, true);
        }
        public IEnumerable<SelectListItem> ListMyFaseToSLI(List<MyFase> elenco, int? selObjId, bool voidField)
        {
            int idTarget = 0;
            if (selObjId.HasValue)
            {
                idTarget = selObjId.Value ;
            }
            List<SelectListItem> lst = new List<SelectListItem>();
            if (voidField)
            {
                SelectListItem sliVoid = new SelectListItem();
                sliVoid.Text = "";
                sliVoid.Value = "0";
                if (idTarget == 0)
                {
                    sliVoid.Selected = true;
                }
                lst.Add(sliVoid);
            }
            foreach (MyFase f in elenco)
            { 
                SelectListItem sli =  new SelectListItem ();
                sli.Text =f.Fase_Desc ;
                sli.Value = f.Fase_ID.ToString() ;
                sli.Selected = false;
                if (idTarget != 0)
                {
                    if (f.Fase_ID == idTarget)
                        sli.Selected = true;
                }
                lst.Add(sli);
            }
            return lst;
        }
        public IEnumerable<SelectListItem> ListMyProfiloToSLI(List<MyProfilo> elenco)
        {
            return ListMyProfiloToSLI(elenco, null, true);
        }
        public IEnumerable<SelectListItem> ListMyProfiloToSLI(List<MyProfilo> elenco, int? selObjId, bool voidField)
        {
            int idTarget = 0;
            if (selObjId.HasValue)
            {
                idTarget = selObjId.Value;
            }
            List<SelectListItem> lst = new List<SelectListItem>();
            if (voidField)
            {
                SelectListItem sliVoid = new SelectListItem();
                sliVoid.Text = "";
                sliVoid.Value = "0";
                if (idTarget == 0)
                {
                    sliVoid.Selected = true;
                }
                lst.Add(sliVoid);
            }
            foreach (MyProfilo f in elenco)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = f.Profilo_Descrizione ;
                sli.Value = f.Profilo_ID.ToString();
                sli.Selected = false;
                if (idTarget != 0)
                {
                    if (f.Profilo_ID == idTarget)
                        sli.Selected = true;
                }
                lst.Add(sli);
            }
            return lst;
        }
        public IEnumerable<SelectListItem> ListMyUdMToSLI(List<MyUdM> elenco)
        {
            return ListMyUdMToSLI(elenco, null, true);
        }
        public IEnumerable<SelectListItem> ListMyUdMToSLI(List<MyUdM> elenco, int? selObjId, bool voidField)
        {
            int idTarget = 0;
            if (selObjId.HasValue)
            {
                idTarget = selObjId.Value;
            }
            List<SelectListItem> lst = new List<SelectListItem>();
            if (voidField)
            {
                SelectListItem sliVoid = new SelectListItem();
                sliVoid.Text = "";
                sliVoid.Value = "0";
                if (idTarget == 0)
                {
                    sliVoid.Selected = true;
                }
                lst.Add(sliVoid);
            }
            foreach (MyUdM f in elenco)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = f.Unimis_Desc;// Unimis_Codice;
                sli.Value = f.Unimis_Id.ToString();
                sli.Selected = false;
                if (idTarget != 0)
                {
                    if (f.Unimis_Id== idTarget)
                        sli.Selected = true;
                }
                lst.Add(sli);
            }
            return lst;
        }
        
        public IEnumerable<SelectListItem> ListMyAttivitaToSLI(List<MyAttivita> elenco)
        {
            return ListMyAttivitaToSLI(elenco, null, true);
        }
        public IEnumerable<SelectListItem> ListMyAttivitaToSLI(List<MyAttivita> elenco, MyAttivita selObj, bool voidField)
        {
            int idTarget = 0;
            if (selObj != null)
            {
                idTarget = selObj.Attivita_ID;
            }
            List<SelectListItem> lst = new List<SelectListItem>();
            if (voidField)
            {
                SelectListItem sliVoid = new SelectListItem();
                sliVoid.Text = "";
                sliVoid.Value = "0";
                if (idTarget == 0)
                {
                    sliVoid.Selected = true;
                }
                lst.Add(sliVoid);
            }
            foreach (MyAttivita f in elenco)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = f.Attivita_Desc;
                sli.Value = f.Attivita_ID.ToString();
                sli.Selected = false;
                if (idTarget != 0)
                {
                    if (f.Attivita_ID == idTarget)
                        sli.Selected = true;
                }
                lst.Add(sli);
            }
            return lst;
        }

        public IEnumerable<SelectListItem> ListMyFigProfessionaleToSLI(List<MyFigProf> elenco)
        {
            return ListMyFigProfessionaleToSLI(elenco, null, true);
        }
        public IEnumerable<SelectListItem> ListMyFigProfessionaleToSLI(List<MyFigProf> elenco, int? selObjId, bool voidField)
        {
            int idTarget = 0;
            if (selObjId.HasValue)
            {
                idTarget = selObjId.Value;
            }
            List<SelectListItem> lst = new List<SelectListItem>();
            if (voidField)
            {
                SelectListItem sliVoid = new SelectListItem();
                sliVoid.Text = "";
                sliVoid.Value = "0";
                if (idTarget == 0)
                {
                    sliVoid.Selected = true;
                }
                lst.Add(sliVoid);
            }
            if (elenco != null && elenco.Count > 0)
            {
                foreach (MyFigProf f in elenco)
                {
                    SelectListItem sli = new SelectListItem();
                    sli.Text = f.FigProf_Desc;
                    sli.Value = f.FigProf_ID.ToString()+"|"+f.FigProf_Costo.ToString() ;
                    sli.Selected = false;
                    if (idTarget != 0)
                    {
                        if (f.FigProf_ID == idTarget)
                            sli.Selected = true;
                    }
                    lst.Add(sli);
                }
            }
            return lst;
        }
    }
}