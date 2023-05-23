using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IZSLER_CAP.Helpers
{
    public class MyGoogleChartDataAjax
    {
        public string Titolo { set; get; }
        public string Val1{ set; get; }
        public string Val2 { set; get; }
        public bool Sospeso { set; get; }
    }
    public class MySettingAjax
    {
        public int Setting_ID { set; get; }
        public string Setting_Codice { set; get; }
        public string Setting_Valore { set; get; }
    
    }
    public class MyMacchinarioAjax
    {
        public int Macchi_ID { set; get; }
        public string Macchi_Codice { set; get; }
        public string Macchi_Desc  { set; get; }
        public int? Macchi_GruppoID { set; get; }
        public decimal? Macchi_Costo { set; get; }

        public decimal? Macchi_Valore_Strumentazione { set; get; }
        public decimal? Macchi_Costo_Manutenzione_Annuo { set; get; }
        public decimal? Macchi_Vita_Utile_Anni { set; get; }
        public decimal? Macchi_Minuti_Anno { set; get; }

        
    }
    public class MyModelloAjax
    {
        public int MasterID { set; get; }
        public int Modello_ID { set; get; }
        public bool FlagSec { set; get; }
        public bool FlagProdotto{ set; get; }
    }
    public class MyUtenteAjax
    {
        public int Utente_ID { set; get; }
        public string Utente_User { set; get; }
        public string Utente_Email { set; get; }
        public string Utente_Nome { set; get; }
        public string Utente_Cognome { set; get; }
        public bool Utente_Lock { set; get; }
    }

    public class MyUtenti_Profili_GruppiAjax
    {
        public int M_Utprgr_Id { set; get; }
        public int M_Utprgr_Utente_Id { set; get; }
        public int M_Utprgr_Profil_Id { set; get; }
        public int M_Utprgr_Grurep_Id { set; get; }
        public bool M_Utprgr_Flg_Principale { set; get; }
        public string Profilo_desc { set; get; }
        public string Gruppo_desc { set; get; }
    }

    public class MyFaseAjax
    {     public int Fase_ID { set; get; }
        public string Fase_Codice { set; get; }
        public string Fase_Desc { set; get; }
        public int? Fase_Fase_ID { set; get; }
        public int Fase_Grurep_ID { set; get; }
    }

    public class MyPaginAjax
    {
        public int NumEntities { get; set; }
        public int? CurrentPage{ get; set; }
        public string SearchDescription { get; set; }
        public int id { get; set; }
        public int valpos_id { get; set; }
        public int sec { get; set; }
        public int p { get; set; }
        public int Ut { get; set; }
        
        public int FiltroStato { get; set; }
        public int FiltroStatoObsoleta { get; set; }

        public int NumEntities_Down { get; set; }
        public int? CurrentPage_Down { get; set; }
        public string SearchDescription_Down { get; set; }


    }
    public class PopUpClass
    {
        public int Id { get; set; }
        public string Description { get; set; }

    }
    public class MyRichiestaAjax
    {
        public int Richie_id { set; get; }
        public string Richie_codice { set; get; }
        public string Richie_titolo { set; get; }
        public DateTime Richie_data_richiesta { set; get; }
        public int Richie_t_richie_id { set; get; }
        public int Richie_richiedente_utente_id { set; get; }
        public int Richie_destinatario_utente_id { set; get; }
        public string Richie_testo { set; get; }
        public int Richie_t_staric_id { set; get; }
        public int Richie_valori_id { set; get; }
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
        public string PaginaOrigine { set; get; }
        public int Richie_prodot_id { set; get; }
        public bool Richie_flg_assegn_al_gruppo { set; get; }
        

    }
    public class MyProdottoAjax
    {
        public int Prodotto_id { set; get; }
        public string Prodotto_Codice_Desc { set; get; }

        public int Prodotto_Dim_Lotto { set; get; }
        public int Prodotto_nr_Campioni { set; get; }
        public double Prodotto_perc_Scarto { set; get; }
        public int Prodotto_Stima_Prod_Anno{ set; get; }

        public string Prodotto_Matrice { set; get; }
        public string Prodotto_Motivo { set; get; }
        public int Prodotto_T_RICPRI_ID { set; get; }
        public string Prodotto_Descrizione { set; get; }
        public string Prodotto_CodiceIntermedio { set; get; }
        public int Prodotto_Gruppo_id { set; get; }
        public int Prodotto_Flag_Reparto { set; get; }
        public bool Prodotto_Flag_Interno { set; get; }
        public int Prodotto_UdM_ID { set; get; }
        public int Prodotto_UdM_ID_Sec { set; get; }
        public string Prodotto_Coeff_Conversione { set; get; }
        public string Prodotto_Tariffa_Proposta { set; get; }
        public List<int> ProdottoPosIds { set; get; }
        public int Prodotto_id_Master { set; get; }
        public int? Prodotto_utente_id { set; get; }
        public bool? Prodotto_flg_assegn_al_gruppo { set; get; }

    }
    public class MyAnalisiAjax
    {
        public int Analisi_id { set; get; }
        public int Analisi_id_Master { set; get; }
        public bool Analisi_flgPonderazione { set; get; }
        public int Analisi_Dim_Lotto { set; get; }
        public int Analisi_nr_Campioni { set; get; }
        public string Analisi_Matrice { set; get; }
        public int Analisi_Peso_Positivo { set; get; }
        public string Analisi_Motivo { set; get; }
        public int Analisi_T_RICPRI_ID { set; get; }
        public string Analisi_Descrizione{ set; get; }
        public string Analisi_CodiceIntermedio { set; get; }
        public int Analisi_Gruppo_id { set; get; }
        public int Analisi_Flag_Reparto { set; get; }
        public string Analisi_Codice_Descrizione { set; get; }
        public List<int> AnalisiPosIds { set; get; }
        public List<int> AnalisiPosSIds { set; get; }
        public int? Analisi_utente_id { set; get; }
        public bool? Analisi_flg_assegn_al_gruppo { set; get; }
    }
    public enum TipoSave
    { 
        Prodotto,
        AnalisiIntermedio,
        Macchinario,
        Descrizione,
        Quantita,
        Fase,
        Livello,
        PrezzoPosizione,
        UdM,
        PulisciCosto,
        UdMRatio,
        FaseAccettazione,
    }
    public class MyProdottoPosAjax
    {
        public string TipoSalvataggio { set; get; }
        public int ProdottoPos_id { set; get; }
        public int ProdottoPos_MasterProdotto_id { set; get; }
        public int? ProdottoPos_Analisi_id { set; get; }
        public string ProdottoPos_Analisi_Desc { set; get; }
        public int? ProdottoPos_Macchinario_id { set; get; }
        public string ProdottoPos_Macchianario_Desc { set; get; }
        public int? ProdottoPos_Prodotto_id { set; get; }
        public string ProdottoPos_Prodotto_Desc { set; get; }
        public int? ProdottoPos_Prodotto_UDM_ID { set; get; }
        public int? ProdottoPos_Fase_id { set; get; }
        public int? ProdottoPos_FigProf_id { set; get; }
        public string ProdottoPos_desc { set; get; }
        public decimal ProdottoPos_Quantita { set; get; }
        public decimal? ProdottoPos_QuantitaCosto { set; get; }
        public decimal? ProdottoPos_TotCosto { set; get; }
        public float? ProdottoPos_CoeffConversione { set; get; }
        public string ProdottoPos_CoeffConversioneString { set; get; }
        public int? ProdottoPos_UdM_id { set; get; }
        public string ProdottoPos_CodSettore { set; get; }
        
    }
    public class MyProdottoPosAjaxList
    {
        public List<int> ProdottoPosIds { set; get; }

    }
    public class MyAnalisiPosAjax
    {
        public string TipoSalvataggio { set; get; }
        public int AnalisiPos_id { set; get; }
        public int AnalisiPos_MasterAnalisi_id { set; get; }
        public int? AnalisiPos_Analisi_id { set; get; }
        public string AnalisiPos_Analisi_Desc { set; get; }
        public int? AnalisiPos_Macchinario_id { set; get; }
        public string AnalisiPos_Macchianario_Desc { set; get; }
        public int? AnalisiPos_Prodotto_id { set; get; }
        public string AnalisiPos_Prodotto_Desc { set; get; }
        public int? AnalisiPos_Prodotto_UDM_ID { set; get; }
        public int? AnalisiPos_Fase_id { set; get; }
        public int? AnalisiPos_FigProf_id { set; get; }
        public string AnalisiPos_desc { set; get; }
        public decimal AnalisiPos_Quantita { set; get; }
        public decimal? AnalisiPos_QuantitaCosto { set; get; }
        public decimal? AnalisiPos_TotCosto { set; get; }
        public float? AnalisiPos_CoeffConversione { set; get; }
        public string AnalisiPos_CoeffConversioneString { set; get; }
        public int? AnalisiPos_UdM_id { set; get; }
        public bool? AnalisiPos_Secondaria { set; get; }
        public string AnalisiPos_CodSettore { set; get; }
    }
    public class MyAnalisiPosAjaxList
   {
       public List<int> AnalisiPosIds { set; get; }
      
    }
    public class MySollecitoAjax
    {
        public int Sollec_id { set; get; }
        public int Sollec_Utente_id { set; get; }
        public int Sollec_Richie_id { set; get; }
        public string Sollec_Messaggio { set; get; }
        public DateTime Sollec_Datascadenza { set; get; }
        
    }

    public class MyFiguraProfessionale_attivitaAjax
    {
        
        public int M_Figatt_Fase_Id { set; get; }
        public int M_Figatt_Figpro_Id { set; get; }
        public bool M_Figatt_checked { set; get; }
    }

    public class MyGrurepAjax
    {
        public int Grurep_ID { set; get; }
        public string Grurep_Codice { set; get; }
        public string Grurep_Desc { set; get; }
        public string Grurep_DescEstesa { set; get; }
        public bool Grurep_Flg_Reparto { set; get; }
        public string Grurep_Cost_Ind { set; get; }
        public string Grurep_PrezzoUnit_Accettazione { set; get; }
    }
    public class MyFigProfAjax
    {
        public int FigProf_ID { set; get; }
        public string FigProf_Codice { set; get; }
        public string FigProf_Desc { set; get; }
        public string FigProf_Cost { set; get; }
    }
}