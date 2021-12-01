﻿using DocumentFormat.OpenXml.Packaging;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.VisualBasic.FileIO;
using OpenXmlPowerTools;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        IRetrieveService<Capital> _CapitalRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Company> _CompaniesRetriveService;

        public DocumentUserProcessService(
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Institution> institutionRetrieveService,
            IRetrieveService<Capital> capitalRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Company> companiesRetriveService
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._InstitutionRetrieveService = institutionRetrieveService;
            this._CapitalRetrieveService = capitalRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._CompaniesRetriveService = companiesRetriveService;
        }

        public string ExecuteProcess(int AccreditedId)
        {
            DateTime date = DateTime.Now;
            string textHtml = "";

            try
            {
                Accredited accredited = this._AccreditedRetrieveService.RetrieveResult<Func<Accredited, bool>, List<Accredited>>(a => a.id == AccreditedId).FirstOrDefault();
                if (accredited != null)
                {
                    Company company = this._CompaniesRetriveService.Where(c => c.id == accredited.Company_Id).FirstOrDefault();

                    if (company != null)
                    {
                        textHtml = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Documents/Convenio.html"));
                        var code = $"{accredited.id}{date.Year}{date.Month}{date.Day}";

                        textHtml = textHtml.Replace("{COMPANY_NAME}", company.Description);
                        textHtml = textHtml.Replace("{NUMBER_CONVENIO}", code.PadLeft(12, '0'));

                        textHtml = HttpUtility.HtmlDecode(textHtml);
                    }
                }

                return textHtml;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error generate contract: {exception.Message}");
            }
        }

        public string ExecuteProcess(CartaTransferenciaDeDatos carta)
        {
            DateTime date = DateTime.Now;
            string textHtml = "";

            try
            {
                Accredited accredited = this._AccreditedRetrieveService.RetrieveResult<Func<Accredited, bool>, List<Accredited>>(a => a.id == carta.AccreditedId).FirstOrDefault();
                if (accredited != null)
                {
                    Company company = this._CompaniesRetriveService.Where(c => c.id == accredited.Company_Id).FirstOrDefault();

                    if (company != null)
                    {
                        textHtml = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Documents/CartaTransferenciaDatosPersonales.html"));
                        var code = $"{accredited.id}{date.Year}{date.Month}{date.Day}";

                        textHtml = textHtml.Replace("{NAME}", $"{accredited.First_Name} {accredited.Last_Name}");
                        textHtml = textHtml.Replace("{COMPANY_NAME}", company.Description);
                        textHtml = textHtml.Replace("{URL_PAGE}", "https://snactehaceelparo.com");
                        textHtml = textHtml.Replace("{DAY}", date.ToString("dd"));
                        textHtml = textHtml.Replace("{MONTH}", date.ToString("MM"));
                        textHtml = textHtml.Replace("{YEAR}", date.ToString("yyyy"));
                        textHtml = textHtml.Replace("class=\"x\"", "class=\"checkHide\"");

                        textHtml = HttpUtility.HtmlDecode(textHtml);
                    }
                }

                return textHtml;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error generate contract: {exception.Message}");
            }
        }

        public string ExecuteProcess(DocumentInvestor documentInvestor)
        {
            try
            {
                var investor = documentInvestor.Investor;
                var institution = this._InstitutionRetrieveService.Find(investor.Institution_Id);
                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                var capital = this._CapitalRetrieveService.Find(documentInvestor.CapitalId);

                string textHtml = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), configurations.Where(p => p.Configuration_Name == "INVESTOR_CONTRACT").FirstOrDefault().Configuration_Value));

                textHtml = textHtml.Replace("{CONTRACT_NUMBER}", investor.Contract_number);
                textHtml = textHtml.Replace("{INVESTOR_NAME}", $"{investor.First_Name} {investor.Last_Name}");
                textHtml = textHtml.Replace("{DATE}", DateTime.Now.ToString("dd/MM/yyyy"));
                textHtml = textHtml.Replace("{AMOUNT}", capital.Amount.ToString("C2"));
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


                MemoryStream file = new MemoryStream(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), configurations.Where(p => p.Configuration_Name == "ACCREDITED_CONTRACT").FirstOrDefault().Configuration_Value)));

                using (WordprocessingDocument doc = WordprocessingDocument.Open(file, true))
                {
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{CONTRACT_NUMBER}", replace: accredited.Contract_number, matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{ACCREDITED_NAME}", replace: $"{accredited.First_Name} {accredited.Last_Name}", matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{COMPANY_NAME}", replace: accredited.Outsourcing_id == null ? accredited.Company_Name : accredited.Outsourcing_Name, matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{RFC}", replace: accredited.Rfc, matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{INSTITUTION_NAME}", replace: accredited.Institution_Name, matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{CLABE}", replace: accredited.Clabe, matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{ACCOUNT_NUMBER}", replace: accredited.Account_Number, matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{DAY}", replace: DateTime.Now.ToString("dd"), matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{MONTH}", replace: DateTime.Now.ToString("MM"), matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{YEAR}", replace: DateTime.Now.ToString("yyyy"), matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{AMOUNT}", replace: advance.Amount.ToString(), matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{AMOUNT_LETTER}", replace: new Moneda().Convertir(advance.Amount.ToString(), true, "PESOS"), matchCase: false);
                    TextReplacer.SearchAndReplace(wordDoc: doc, search: "{RETENTION}", replace: advance.Total_Withhold.ToString(), matchCase: false);

                    doc.SaveAs(Path.Combine(Directory.GetCurrentDirectory(), @"Temporal\" + accredited.Contract_number + ".docx")).Close();
                }

                MemoryStream fileModified = new MemoryStream(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), @"Temporal\" + accredited.Contract_number + ".docx")));

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
                string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), configurations.Where(p => p.Configuration_Name == "ACCREDITED_CONTRACT_LOGIN").FirstOrDefault().Configuration_Value));

                textHtml = textHtml.Replace("{CONTRACT_NUMBER}", accredited.Contract_number);
                textHtml = textHtml.Replace("{ACCREDITED_NAME}", $"{accredited.First_Name} {accredited.Last_Name}");
                textHtml = textHtml.Replace("{COMPANY_NAME}", accredited.Outsourcing_id == null ? accredited.Company_Name : accredited.Outsourcing_Name);
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

        public string ExecuteProcess(CartaAvisoGeneral cartaAvisoGeneral)
        {
            try
            {
                string fileConfig = "ACCREDITED_AS_CARTA_AVISO_GENERAL";

                if (cartaAvisoGeneral.accredited.TypeContract?.Code == "sueldoysalario")
                {
                    fileConfig = "ACCREDITED_SS_CARTA_AVISO_GENERAL";
                }


                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), configurations.Where(p => p.Configuration_Name == fileConfig).FirstOrDefault().Configuration_Value));

                textHtml = textHtml.Replace("{NUMBER_CONTRACT}", cartaAvisoGeneral.contractMutuo.id.ToString());
                textHtml = textHtml.Replace("{NUMBER_CONTRACT_USER}", cartaAvisoGeneral.accredited.Contract_number);
                textHtml = textHtml.Replace("{NAME_ACREDITED}", $"{cartaAvisoGeneral.accredited.First_Name} {cartaAvisoGeneral.accredited.Last_Name}");
                textHtml = textHtml.Replace("{NAME_COMPANY}", cartaAvisoGeneral.accredited.Outsourcing_id == null ? cartaAvisoGeneral.accredited.Company_Name : cartaAvisoGeneral.accredited.Outsourcing_Name);
                textHtml = textHtml.Replace("{RFC}", cartaAvisoGeneral.accredited.Rfc);
                textHtml = textHtml.Replace("{ADDRESS}", cartaAvisoGeneral.accredited.Address);
                textHtml = textHtml.Replace("{COLONY}", cartaAvisoGeneral.accredited.Colony);
                textHtml = textHtml.Replace("{MUNICIPALITY}", cartaAvisoGeneral.accredited.Municipality);
                textHtml = textHtml.Replace("{ZIP_CODE}", cartaAvisoGeneral.accredited.Zip_Code);
                textHtml = textHtml.Replace("{STATE}", cartaAvisoGeneral.accredited.State);
                textHtml = textHtml.Replace("{BANK}", cartaAvisoGeneral.accredited.Institution_Name);
                textHtml = textHtml.Replace("{NUMBER_ACCOUNT}", cartaAvisoGeneral.accredited.Account_Number);
                textHtml = textHtml.Replace("{CLABE}", cartaAvisoGeneral.accredited.Clabe);

                textHtml = Regex.Replace(textHtml, @"\t|\n|\r", "");

                textHtml = HttpUtility.HtmlDecode(textHtml);

                return textHtml;


            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error: {exception.Message}");
            }
        }

        public string ExecuteProcess(CartaMandato cartaMandato)
        {
            try
            {
                DateTime date = DateTime.Now;

                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Documents/CartaCompromiso.html"));

                textHtml = textHtml.Replace("{NAME}", $"{cartaMandato.accredited.First_Name.ToUpper()}  {cartaMandato.accredited.Last_Name.ToUpper()}");
                textHtml = textHtml.Replace("{DAY}", date.Day.ToString());
                textHtml = textHtml.Replace("{MONTH}", date.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")));
                textHtml = textHtml.Replace("{YEAR}", date.Year.ToString());
                textHtml = textHtml.Replace("{AMOUNT}", cartaMandato.advance.Amount.ToString("C2"));
                textHtml = textHtml.Replace("{AMOUNT_LETTER}", new Moneda().Convertir(cartaMandato.advance.Amount.ToString(), true, "PESOS"));
                textHtml = textHtml.Replace("{TOTAL_AMOUNT}", cartaMandato.advance.Total_Withhold.ToString("C2"));
                textHtml = textHtml.Replace("{TOTAL_AMOUNT_LETTER}", new Moneda().Convertir(cartaMandato.advance.Total_Withhold.ToString(), true, "PESOS"));
                textHtml = textHtml.Replace("{AMOUNT_WEEK}", cartaMandato.totalWeek.ToString("C2"));
                textHtml = textHtml.Replace("{AMOUNT_WEEK_LETTER}", new Moneda().Convertir(cartaMandato.totalWeek.ToString(), true, "PESOS"));

                var ulDates = "<ol>";

                if (cartaMandato.dates != String.Empty && cartaMandato.dates != null)
                {
                    var splitDates = cartaMandato.dates.Split(',');
                    foreach(String dateSplit in splitDates)
                    {
                        ulDates += $"<li>{dateSplit}</li>";
                    }
                }

                ulDates += "</ol>";

                textHtml = textHtml.Replace("{LIST_DATES}", ulDates);

                if (cartaMandato.CheckedHide)
                {
                    textHtml = textHtml.Replace("class=\"x\"", "class=\"checkHide\"");
                }

                textHtml = Regex.Replace(textHtml, @"\t|\n|\r", "");

                textHtml = HttpUtility.HtmlDecode(textHtml);

                return textHtml;


            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error: {exception.Message}");
            }
        }

        public string ExecuteProcess(ContratoMutuo contratoMutuo)
        {
            try
            {
                DateTime date = DateTime.Now;
                string fileConfig = "ACCREDITED_AS_CONTRATO_MUTUO";

                if (contratoMutuo.accredited.TypeContract?.Code == "sueldoysalario")
                {
                    fileConfig = "ACCREDITED_SS_CONTRATO_MUTUO";
                }


                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), configurations.Where(p => p.Configuration_Name == fileConfig).FirstOrDefault().Configuration_Value));


                textHtml = textHtml.Replace("{NUMBER_CONTRACT}", contratoMutuo.contractMutuo.id.ToString());
                textHtml = textHtml.Replace("{NAME_ACREDITED}", $"{contratoMutuo.accredited.First_Name.ToUpper()}  {contratoMutuo.accredited.Last_Name.ToUpper()}");
                textHtml = textHtml.Replace("{COMPANY_NAME}", contratoMutuo.accredited.Outsourcing_id == null ? contratoMutuo.accredited.Company_Name : contratoMutuo.accredited.Outsourcing_Name);
                textHtml = textHtml.Replace("{AMOUNT_MAX}", contratoMutuo.advance.Maximum_Amount.ToString("C2"));
                textHtml = textHtml.Replace("{AMOUNT_MAX_LETTER}", new Moneda().Convertir(contratoMutuo.advance.Maximum_Amount.ToString(), true, "PESOS"));
                textHtml = textHtml.Replace("{INTERES_RATE}", contratoMutuo.accredited.Interest_Rate.ToString());
                textHtml = textHtml.Replace("{INTERES_RATE_LETTER}", new Moneda().Convertir(contratoMutuo.accredited.Interest_Rate.ToString(), true, "PESOS"));
                textHtml = textHtml.Replace("{INTEREST_MORATORIO}", contratoMutuo.accredited.Moratoruim_Interest_Rate.ToString());
                textHtml = textHtml.Replace("{INTEREST_MORATORIO_LETTER}", new Moneda().Convertir(contratoMutuo.accredited.Moratoruim_Interest_Rate.ToString(), true, "PESOS"));
                textHtml = textHtml.Replace("{ADDRESS_ACREDITED}", $"{contratoMutuo.accredited.Address}, {contratoMutuo.accredited.Colony}, {contratoMutuo.accredited.Municipality}, {contratoMutuo.accredited.Zip_Code}, {contratoMutuo.accredited.State}");
                textHtml = textHtml.Replace("{EMAIL_ACREDITED}", contratoMutuo.accredited.Mail);
                textHtml = textHtml.Replace("{DAY}", date.Day.ToString());
                textHtml = textHtml.Replace("{MONTH}", date.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")));
                textHtml = textHtml.Replace("{YEAR}", date.Year.ToString());


                textHtml = Regex.Replace(textHtml, @"\t|\n|\r", "");

                textHtml = HttpUtility.HtmlDecode(textHtml);

                return textHtml;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error: {exception.Message}");
            }
        }

        public string ExecuteProcess(TransferenciaDatosPersonales transferenciaDatosPersonales)
        {
            try
            {
                DateTime date = DateTime.Now;
                string fileConfig = "ACCREDITED_TRANSFERT_DATA_PERSONAL";


                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), configurations.Where(p => p.Configuration_Name == fileConfig).FirstOrDefault().Configuration_Value));

                textHtml = textHtml.Replace("{COMPANY}", transferenciaDatosPersonales.accredited.Outsourcing_id == null ? transferenciaDatosPersonales.accredited.Company_Name : transferenciaDatosPersonales.accredited.Outsourcing_Name);
                textHtml = textHtml.Replace("{DAY}", date.Day.ToString());
                textHtml = textHtml.Replace("{MONTH}", date.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")));
                textHtml = textHtml.Replace("{YEAR}", date.Year.ToString());
                textHtml = textHtml.Replace("{NAME}", $"{transferenciaDatosPersonales.accredited.First_Name.ToUpper()} {transferenciaDatosPersonales.accredited.Last_Name.ToUpper()}");


                textHtml = Regex.Replace(textHtml, @"\t|\n|\r", "");

                textHtml = HttpUtility.HtmlDecode(textHtml);

                return textHtml;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error: {exception.Message}");
            }
        }

        public string ExecuteProcess(TerminosCondiciones terminosCondiciones)
        {
            try
            {
                DateTime date = DateTime.Now;
                string fileConfig = "URL_TERMINOSYCONDICIONES";


                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), configurations.Where(p => p.Configuration_Name == fileConfig).FirstOrDefault().Configuration_Value));

                textHtml = Regex.Replace(textHtml, @"\t|\n|\r", "");

                textHtml = HttpUtility.HtmlDecode(textHtml);

                return textHtml;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error: {exception.Message}");
            }
        }

        public string ExecuteProcess(AvisoPrivacidad avisoPrivacidad)
        {
            try
            {
                DateTime date = DateTime.Now;
                string fileConfig = "URL_AVISOPROVACIDAD";


                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), configurations.Where(p => p.Configuration_Name == fileConfig).FirstOrDefault().Configuration_Value));

                textHtml = Regex.Replace(textHtml, @"\t|\n|\r", "");

                textHtml = HttpUtility.HtmlDecode(textHtml);

                return textHtml;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error: {exception.Message}");
            }
        }

    }
}