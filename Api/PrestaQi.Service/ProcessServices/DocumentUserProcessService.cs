using DocumentFormat.OpenXml.Packaging;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using OpenXmlPowerTools;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Service.Tools;
using System;
using System.IO;
using System.Linq;
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

        public Stream ExecuteProcess(DocumentAccredited documentAccredited)
        {
            try
            {
                var accredited = documentAccredited.Accredited;
                var advance = documentAccredited.Advance;

                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();

                MemoryStream file = new MemoryStream(Utilities.GetFile(configurations,
                     configurations.Where(p => p.Configuration_Name == "ACCREDITED_CONTRACT").FirstOrDefault().Configuration_Value));

                 using (WordprocessingDocument doc = WordprocessingDocument.Open(file, true))
                 {
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{CONTRACT_NUMBER}", replace: accredited.Contract_number, matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{ACCREDITED_NAME}", replace: $"{accredited.First_Name} {accredited.Last_Name}", matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{COMPANY_NAME}", replace: accredited.Company_Name, matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{RFC}", replace: accredited.Rfc, matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{INSTITUTION_NAME}", replace: accredited.Institution_Name, matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{CLABE}", replace: accredited.Clabe, matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{ACCOUNT_NUMBER}", replace: accredited.Account_Number, matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{DAY}", replace: DateTime.Now.ToString("dd"), matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{MONTH}", replace: DateTime.Now.ToString("MM"), matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{YEAR}", replace: DateTime.Now.ToString("yyyy"), matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{AMOUNT}", replace: advance.Amount.ToString(), matchCase: false);
                     TextReplacer.SearchAndReplace(wordDoc: doc, search: "{AMOUNT_LETTER}", replace: new Moneda().Convertir(advance.Amount.ToString(), true, "PESOS"), matchCase: false);

                     doc.SaveAs(Path.Combine(Directory.GetCurrentDirectory(), @"Temporal\" + accredited.Contract_number + ".docx")).Close();
                 }

                MemoryStream fileModified = new MemoryStream(Utilities.GetFile(configurations,
                   configurations.Where(p => p.Configuration_Name == "ACCREDITED_CONTRACT_MODIFIED").FirstOrDefault().Configuration_Value  + accredited.Contract_number + ".docx"));

                return fileModified;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error generate contract: {exception.Message}");
            }
        }

        public string ExecuteProcess(Accredited accredited)
        {
            try
            {
                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations,
                    configurations.Where(p => p.Configuration_Name == "ACCREDITED_CONTRACT_LOGIN").FirstOrDefault().Configuration_Value))).ReadToEnd();

                textHtml = textHtml.Replace("{CONTRACT_NUMBER}", accredited.Contract_number);
                textHtml = textHtml.Replace("{ACCREDITED_NAME}", $"{accredited.First_Name} {accredited.Last_Name}");
                textHtml = textHtml.Replace("{COMPANY_NAME}", accredited.Company_Name);
                textHtml = textHtml.Replace("{RFC}", accredited.Rfc);
                textHtml = textHtml.Replace("{INSTITUTION_NAME}", accredited.Institution_Name);
                textHtml = textHtml.Replace("{CLABE}", accredited.Clabe);
                textHtml = textHtml.Replace("{ACCOUNT_NUMBER}", accredited.Account_Number);
                textHtml = Regex.Replace(textHtml, @"\t|\n|\r", "");

                textHtml = HttpUtility.HtmlDecode(textHtml);

                return textHtml;

            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error generate contract: {exception.Message}");
            }
        }


    }
}
