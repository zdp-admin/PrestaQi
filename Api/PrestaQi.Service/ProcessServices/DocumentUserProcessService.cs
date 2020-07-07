using DocumentFormat.OpenXml.Drawing;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PrestaQi.Service.ProcessServices
{
    public class DocumentUserProcessService : ProcessService<DocumentUser>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Institution> _InstitutionRetrieveService;

        public DocumentUserProcessService(
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Institution> institutionRetrieveService
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._InstitutionRetrieveService = institutionRetrieveService;
        }

        public string ExecuteProcess(Investor investor)
        {
            try
            {
                var institution = this._InstitutionRetrieveService.Find(investor.Institution_Id);
                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations, 
                    configurations.Where(p => p.Configuration_Name == "INVESTOR_CONTRACT").FirstOrDefault().Configuration_Value))).ReadToEnd();

                textHtml = textHtml.Replace("{CONTRACT_NUMBER}", investor.Contract_number);
                textHtml = textHtml.Replace("{INVESTOR_NAME}", $"{investor.First_Name} {investor.Last_Name}");
                textHtml = textHtml.Replace("{DATE}", DateTime.Now.ToString("dd/MM/yyyy"));
                textHtml = textHtml.Replace("{AMOUNT}", investor.Total_Amount_Agreed.ToString());
                textHtml = textHtml.Replace("{INSTITUTION_NAME}", institution.Description);
                textHtml = textHtml.Replace("{CLABE}", investor.Clabe);
                textHtml = textHtml.Replace("{ACCOUNT_NUMBER}", investor.Account_Number);
                textHtml = Regex.Replace(textHtml, @"\t|\n|\r", "");

                textHtml = HttpUtility.HtmlDecode(textHtml);

                return textHtml;

            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error generate contract: {exception.Message}");
            }
        }

        public string ExecuteProcess(DocumentAccredited documentAccredited)
        {
            try
            {
                var accredited = documentAccredited.Accredited;
                var advance = documentAccredited.Advance;

                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations,
                    configurations.Where(p => p.Configuration_Name == "ACCREDITED_CONTRACT").FirstOrDefault().Configuration_Value))).ReadToEnd();

                textHtml = textHtml.Replace("{CONTRACT_NUMBER}", accredited.Contract_number);
                textHtml = textHtml.Replace("{ACCREDITED_NAME}", $"{accredited.First_Name} {accredited.Last_Name}");
                textHtml = textHtml.Replace("{COMPANY_NAME}", accredited.Company_Name);
                textHtml = textHtml.Replace("{RFC}", accredited.Rfc);
                textHtml = textHtml.Replace("{INSTITUTION_NAME}", accredited.Institution_Name);
                textHtml = textHtml.Replace("{CLABE}", accredited.Clabe);
                textHtml = textHtml.Replace("{ACCOUNT_NUMBER}", accredited.Account_Number);
                textHtml = textHtml.Replace("{DAY}", DateTime.Now.ToString("dd"));
                textHtml = textHtml.Replace("{MONTH}", DateTime.Now.ToString("MM"));
                textHtml = textHtml.Replace("{YEAR}", DateTime.Now.ToString("yyyy"));
                textHtml = textHtml.Replace("{AMOUNT}", advance.Amount.ToString());

                return textHtml;

            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error generate contract: {exception.Message}");
            }
        }
    }
}
