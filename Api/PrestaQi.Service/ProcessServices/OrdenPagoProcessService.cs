using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Spei;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace PrestaQi.Service.ProcessServices
{
    public class OrdenPagoProcessService : ProcessService<ordenPagoWS>
    {
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;
        IRetrieveService<Institution> _InstitutionRetrieveService;


        public OrdenPagoProcessService(
           IRetrieveService<Accredited> accreditedRetrieveService,
           IRetrieveService<Configuration> configurationRetrieveService,
           IRetrieveService<Advance> advanceRetrieveService,
           IRetrieveService<Institution> institutionRetrieveService
            )
        {
            
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
            this._InstitutionRetrieveService = institutionRetrieveService;
        }

        public ResponseSpei ExecuteProcess(OrderPayment orderPayment)
        {
            

            try
            {
                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                byte[] file = Utilities.GetFile(configurations, configurations.Find(p => p.Configuration_Name == "CERTIFIED_FTP").Configuration_Value);

                var accredited = this._AccreditedRetrieveService.Find(orderPayment.Accredited_Id);
                
                string prefix = configurations.Find(p => p.Configuration_Name == "PREFIX_ORDER_PAYMENT").Configuration_Value;
                int advanceCount = this._AdvanceRetrieveService.Where(p => p.Accredited_Id == accredited.id).Count() + 2;
                var institutions = this._InstitutionRetrieveService.Where(p => p.Enabled == true).ToList();

                CryptoHandler crypto = new CryptoHandler();
                crypto.password = configurations.Find(p => p.Configuration_Name == "CERTIFIED_PASSWORD").Configuration_Value;
                ordenPagoWS ordenPago = new ordenPagoWS();
                ordenPago.empresa = configurations.Find(p => p.Configuration_Name == "COMPANY_NAME").Configuration_Value;
                ordenPago.claveRastreo = $"{prefix}{DateTime.Now:yyyyMMdd}{accredited.id}{advanceCount}";
                ordenPago.referenciaNumerica = Convert.ToInt32(DateTime.Now.ToString("yyMMdd"));
                ordenPago.cuentaBeneficiario = accredited.Clabe;
                ordenPago.tipoPago = Convert.ToInt32(configurations.Find(p => p.Configuration_Name == "PAYMENT_TYPE").Configuration_Value);
                ordenPago.nombreBeneficiario = $"{accredited.First_Name} {accredited.Last_Name}";
                ordenPago.rfcCurpBeneficiario = accredited.Rfc;
                ordenPago.topologia = configurations.Find(p => p.Configuration_Name == "TOPOLOGY").Configuration_Value;
                ordenPago.institucionOperante = Convert.ToInt32(configurations.Find(p => p.Configuration_Name == "OPERATING_INSTITUTION").Configuration_Value);
                ordenPago.tipoCuentaBeneficiario = Convert.ToInt32(configurations.Find(p => p.Configuration_Name == "TYPE_BENEFICIARY_ACCOUNT").Configuration_Value);
                ordenPago.conceptoPago = configurations.Find(p => p.Configuration_Name == "PAYMENT_CONCEPT").Configuration_Value;
                ordenPago.institucionContraparte = institutions.Find(p => p.id == accredited.Institution_Id).Code;
                ordenPago.monto = decimal.Add(Convert.ToDecimal(orderPayment.Advance.Amount), .00m);
                ordenPago.tipoCuentaBeneficiarioSpecified = true;
                ordenPago.institucionOperanteSpecified = true;
                ordenPago.institucionContraparteSpecified = true;
                ordenPago.tipoPagoSpecified = true;
                ordenPago.referenciaNumericaSpecified = true;
                ordenPago.montoSpecified = true;
                ordenPago.firma = crypto.sign(ordenPago, file);

                var resultado = CallService(ordenPago, configurations);

                if (resultado.resultado.id > 0) resultado.resultado.claveRastreo = ordenPago.claveRastreo;
                return resultado;
            }
            catch (Exception exception)
            {
                return new ResponseSpei()
                {
                    resultado = new resultado()
                    {
                        id = -1,
                        descripcionError = exception.Message 
                    }
                };
            }
        }

        ResponseSpei CallService(ordenPagoWS ordenPagoWS, List<Configuration> configurations)
        {
            string baseUri = configurations.Find(p => p.Configuration_Name == "API_SPEI").Configuration_Value;
            string method = configurations.Find(p => p.Configuration_Name == "API_ORDER_REGISTER").Configuration_Value;

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ordenPagoWS);

            var request = (HttpWebRequest)WebRequest.Create($"{baseUri}{method}");
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            using (WebResponse response2 = request.GetResponse())
            {
                using (Stream strReader = response2.GetResponseStream())
                {
                    if (strReader == null) return null;
                    using (StreamReader objReader = new StreamReader(strReader))
                    {
                        string responseBody = objReader.ReadToEnd();
                        return JsonConvert.DeserializeObject<ResponseSpei>(responseBody);
                    }
                }
            }
        }

        public bool ExecuteProcess(SendSpeiMail sendSpeiMail)
        {
            try
            {
                var accredited = this._AccreditedRetrieveService.Find(sendSpeiMail.Accredited_Id);
                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();

                var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
                var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "MAI_ADVANCE");

                var messageMail = JsonConvert.DeserializeObject<MessageMail>(messageConfig.Configuration_Value);

                string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations, messageMail.Message))).ReadToEnd();
                textHtml = textHtml.Replace("{KEY_TRACKING}", sendSpeiMail.Tracking_Key);
                textHtml = textHtml.Replace("{AMOUNT}", $"${sendSpeiMail.Amount.ToString()}");
                messageMail.Message = textHtml;

                Utilities.SendEmail(new List<string> { accredited.Mail }, messageMail, mailConf);

                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        
    }
}
