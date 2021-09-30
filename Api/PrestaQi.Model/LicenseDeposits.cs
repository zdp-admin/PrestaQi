using Newtonsoft.Json;
using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace PrestaQi.Model
{
    [Table("license_deposits")]
    public class LicenseDeposits : Entity<int>
    {
        [JsonPropertyAttribute("clavePago")]
        [Column("payment_key")]
        public string PaymentKey { get; set; }
        [JsonPropertyAttribute("claveRastreo")]
        [Column("tracking_key")]
        public string TrackingKey { get; set; }
        [JsonPropertyAttribute("conceptoPago")]
        [Column("payment_concept")]
        public string PaymentConcept { get; set; }
        [JsonPropertyAttribute("conceptoPago2")]
        [Column("payment_concept2")]
        public string PaymentConcept2 { get; set; }
        [JsonPropertyAttribute("cuentaBeneficiario")]
        [Column("beneficiary_account")]
        public string BeneficiaryAccount { get; set; }
        [JsonPropertyAttribute("cuentaBeneficiario2")]
        [Column("beneficiary_account2")]
        public string BeneficiaryAccount2 { get; set; }

        [JsonPropertyAttribute("cuentaOrdenante")]
        [Column("originator_account")]
        public string OriginatorAccount { get; set; }

        [JsonPropertyAttribute("empresa")]
        [Column("company")]
        public string Company { get; set; }

        [JsonPropertyAttribute("estado")]
        [Column("status")]
        public string Status { get; set; }

        [Column("date_transaction")]
        public DateTime DateTransaction { get; set; }

        [JsonPropertyAttribute("idEF")]
        [Column("id_ef")]
        public int IdEF { get; set; }

        [JsonPropertyAttribute("institucionContraparte")]
        [Column("counterpart_institution")]
        public int CounterpartInstitution { get; set; }

        [JsonPropertyAttribute("institucionOperante")]
        [Column("operating_institution")]
        public int OperatingInstitution { get; set; }

        [JsonPropertyAttribute("medioEntrega")]
        [Column("half_delivery")]
        public int HalfDelivery { get; set; }

        [JsonPropertyAttribute("monto")]
        [Column("amount")]
        public double Amount { get; set; }

        [JsonPropertyAttribute("nombreBeneficiario")]
        [Column("beneficiary_name")]
        public string BeneficiaryName { get; set; }

        [JsonPropertyAttribute("nombreBeneficiario2")]
        [Column("beneficiary_name2")]
        public string BeneficiaryName2 { get; set; }

        [JsonPropertyAttribute("nombreCEP")]
        [Column("cep_name")]
        public string CepName { get; set; }

        [JsonPropertyAttribute("nombreOrdenante")]
        [Column("ordering_name")]
        public string OrderingName { get; set; }

        [JsonPropertyAttribute("prioridad")]
        [Column("priority")]
        public int Priority { get; set; }

        [JsonPropertyAttribute("referenciaCobranza")]
        [Column("collection_reference")]
        public string CollectionReference { get; set; }

        [JsonPropertyAttribute("referenciaNumerica")]
        [Column("numerical_reference")]
        public int NumericalReference { get; set; }

        [JsonPropertyAttribute("rfcCEP")]
        [Column("cep_rfc")]
        public string CepRFC { get; set; }

        [JsonPropertyAttribute("rfcCurpBeneficiario")]
        [Column("rfc_curp_beneficiary")]
        public string RFCCurpBeneficiary { get; set; }

        [JsonPropertyAttribute("rfcCurpBeneficiario2")]
        [Column("rfc_curp_beneficiary2")]
        public string RFCCurpBeneficiary2 { get; set; }

        [JsonPropertyAttribute("rfcCurpOrdenante")]
        [Column("rfc_curp_originator")]
        public string RFCCurpOriginator { get; set; }

        [JsonPropertyAttribute("sello")]
        [Column("sello")]
        public string Sello { get; set; }

        [JsonPropertyAttribute("tipoCuentaBeneficiario")]
        [Column("type_account_beneficiary")]
        public int TypeAccountBeneficiary { get; set; }

        [JsonPropertyAttribute("tipoCuentaOrdenante")]
        [Column("type_account_originator")]
        public int TypeAccountOriginator { get; set; }

        [JsonPropertyAttribute("tipoPago")]
        [Column("type_payment")]
        public int TypePayment { get; set; }

        [JsonPropertyAttribute("topologia")]
        [Column("topology")]
        public string Topology { get; set; }

        [JsonPropertyAttribute("tsCaptura")]
        [Column("ts_capture")]
        public long TsCapture { get; set; }

        [JsonPropertyAttribute("tsDevolucion")]
        [Column("ts_return")]
        public long TsReturn { get; set; }

        [JsonPropertyAttribute("tsEntrega")]
        [Column("ts_delivery")]
        public long TsDelivery { get; set; }

        [JsonPropertyAttribute("tsLiquidacion")]
        [Column("ts_liquidation")]
        public long TsLiquidation { get; set; }
        [Column("license_id")]
        public int LicenseId { get; set; }
        [JsonPropertyAttribute("fechaOperacion")]
        [NotMapped]
        public int DateOperation {
            get
            {
                return _dateoperation;
            }
            set {
                _dateoperation = value;
                this.DateTransaction = DateTime.Parse(value.ToString().Insert(6, "-").Insert(4, "-"));
            }
        }
        [NotMapped]
        private int _dateoperation { get; set; }
    }
}
