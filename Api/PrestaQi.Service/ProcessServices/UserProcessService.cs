using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualBasic.FileIO;
using MoreLinq;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace PrestaQi.Service.ProcessServices
{
    public class UserProcessService : ProcessService<User>
    {
        IRetrieveService<User> _UserRetrieveService;
        IRetrieveService<Investor> _InvestorRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Institution> _InstitutionRetrieveService;
        IRetrieveService<Gender> _GenderRetrieveService;
        IRetrieveService<Company> _CompanyRetrieveService;
        IRetrieveService<Period> _PeriodRetrieveService;
        IRetrieveService<TypeContract> _TypeContract;
        IRetrieveService<License> _LicenseRetrieveService;
        IWriteService<Company> _CompanyWriteService;
        private string pathProyect;

        public UserProcessService(
            IRetrieveService<User> userRetrieveService,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Institution> institutionRetrieveService,
            IRetrieveService<Gender> genderRetrieveService,
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<TypeContract> typeContract,
            IRetrieveService<License> licenseRetrieveService,
            IWriteService<Company> companyWriteService,
            IHostingEnvironment hostingEnvironment
            )
        {
            this._UserRetrieveService = userRetrieveService;
            this._InvestorRetrieveService = investorRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._InstitutionRetrieveService = institutionRetrieveService;
            this._GenderRetrieveService = genderRetrieveService;
            this._CompanyRetrieveService = companyRetrieveService;
            this._PeriodRetrieveService = periodRetrieveService;
            this._TypeContract = typeContract;
            this._LicenseRetrieveService = licenseRetrieveService;
            this._CompanyWriteService = companyWriteService;
            this.pathProyect = hostingEnvironment.ContentRootPath;
        }

        public bool ExecuteProcess(string mail)
        {
            bool exist = VerifiyMail(mail);
           
            if (!exist)
                throw new SystemValidationException("The mail is already registered");

            return true;
        }

        public RecoveryPasswordData ExecuteProcess(RecoveryPassword recoveryPassword)
        {
            RecoveryPasswordData recoveryPasswordData = new RecoveryPasswordData();

            var userMail = this._UserRetrieveService.Where(p => p.Mail.ToLower() == recoveryPassword.Mail.ToLower() && p.Deleted_At == null).FirstOrDefault();
            var accreditedMail = this._AccreditedRetrieveService.Where(p => p.Mail.ToLower() == recoveryPassword.Mail.ToLower() && p.Deleted_At == null).FirstOrDefault();
            var investorMail = this._InvestorRetrieveService.Where(p => p.Mail.ToLower() == recoveryPassword.Mail.ToLower() && p.Deleted_At == null).FirstOrDefault();

            if (userMail == null && accreditedMail == null && investorMail == null)
                throw new SystemValidationException("The mail is not registered");

            string password = Utilities.GetPasswordRandom();
            recoveryPasswordData.Password = password;
            string passwordEncrypt = InsiscoCore.Utilities.Crypto.MD5.Encrypt(password);
            
            if (userMail != null)
            {
                recoveryPasswordData.Mail = userMail.Mail;
                userMail.Password = passwordEncrypt;
                recoveryPasswordData.Data = userMail;
                recoveryPasswordData.UserType = Model.Enum.PrestaQiEnum.UserType.Administrador;
            }

            if (accreditedMail != null)
            {
                recoveryPasswordData.Mail = accreditedMail.Mail;
                accreditedMail.Password = passwordEncrypt;
                recoveryPasswordData.Data = accreditedMail;
                recoveryPasswordData.UserType = Model.Enum.PrestaQiEnum.UserType.Acreditado;
            }

            if (investorMail != null)
            {
                recoveryPasswordData.Mail = investorMail.Mail;
                investorMail.Password = passwordEncrypt;
                recoveryPasswordData.Data = investorMail;
                recoveryPasswordData.UserType = Model.Enum.PrestaQiEnum.UserType.Inversionista;
            }

            return recoveryPasswordData;
        }

        public bool ExecuteProcess(SendMailRecoveryPassword sendMailRecoveryPassword)
        {
            try
            {
                var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
                var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
                var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "RECOVERY_PASSWORD");
                var messageMail = JsonConvert.DeserializeObject<MessageMail>(messageConfig.Configuration_Value);
                string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), messageMail.Message));
                textHtml = textHtml.Replace("{PASSWORD}", sendMailRecoveryPassword.Password);
                textHtml = textHtml.Replace("{NAME}", sendMailRecoveryPassword.Name);
                textHtml = textHtml.Replace("{WHATSAPP}", sendMailRecoveryPassword.Contacts.Find(p => p.id == 1).Contact_Data);
                textHtml = textHtml.Replace("{MAIL_SOPORTE}", sendMailRecoveryPassword.Contacts.Find(p => p.id == 2).Contact_Data);
                textHtml = textHtml.Replace("{PHONE}", sendMailRecoveryPassword.Contacts.Find(p => p.id == 3).Contact_Data);

                messageMail.Message = textHtml;

                Utilities.SendEmail(new List<string> { sendMailRecoveryPassword.Mail }, messageMail, mailConf);
            }
            catch (Exception)
            {

            }
            

            return true;
        }

        public ResponseFile ExecuteProcess(FileUser fileUser)
        {
            try
            {
                if (fileUser.Type == (int)PrestaQiEnum.UserType.Administrador)
                {
                    var users = ProcessFileUser(fileUser.File);
                    return users;
                }

                if (fileUser.Type == (int)PrestaQiEnum.UserType.Inversionista)
                {
                    var investors = ProcessFileInvestor(fileUser.File);
                    return investors;
                }

                if (fileUser.Type == (int)PrestaQiEnum.UserType.Acreditado)
                {
                    var accrediteds = ProcessFileAccredited(fileUser.File);
                    return accrediteds;
                }

                return null;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error save file: {exception.Message}");
            }
        }

        public ResponseFile ExecuteProcess(FileSnac fileSnac)
        {
            try
            {
                var users = ProcessFileEmployeeSnac(fileSnac.File, fileSnac.companyId);

                return users;
            }
            catch(Exception e)
            {
                throw new SystemValidationException($"Error save file: {e.Message}");
            }
        }

        ResponseFile ProcessFileUser(byte[] file)
        {
            List<User> users = new List<User>();
            StringBuilder messages = new StringBuilder();

            using (var stream = new MemoryStream(file))
            {
                using (var reader = new StreamReader(stream))
                {
                    using (TextFieldParser parser = new TextFieldParser(reader))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        int row = 0;
                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();

                            if (row == 0)
                            {
                                row += 1;
                                continue;
                            }

                            User user = new User()
                            {
                                First_Name = fields[0],
                                Last_Name = fields[1],
                                Employee_Number = fields[2],
                                Mail = fields[3],
                                Phone = fields[4]
                            };

                            if (fields[5].Split(',').Length > 0)
                            {
                                var modules = fields[5].Split(',');
                                user.Modules = modules.Select(p => new UserModule()
                                {
                                     module_id = Convert.ToInt32(p)
                                }).ToList();
                            }

                            if (VerifiyMail(user.Mail))
                                users.Add(user);
                            else
                                messages.Append($"{Environment.NewLine} - {user.Mail} ya existe. Línea: {row.ToString()}");

                            row += 1;
                        }
                    }
                }
            }

            if (users.Count > 0 || messages.Length > 0)
                return new ResponseFile() { Entities = users, Message = messages };

            throw new SystemValidationException("No records found");
        }

        ResponseFile ProcessFileInvestor(byte[] file)
        {
            List<Investor> investors = new List<Investor>();
            StringBuilder messages = new StringBuilder();

            using (var stream = new MemoryStream(file))
            {
                using (var reader = new StreamReader(stream))
                {
                    using (TextFieldParser parser = new TextFieldParser(reader))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        int row = 1;
                        while (!parser.EndOfData)
                        {
                            bool save = true;

                            string[] fields = parser.ReadFields();

                            if (row == 1)
                            {
                                row += 1;
                                continue;
                            }

                            Investor investor = new Investor()
                            {
                                First_Name = fields[0],
                                Last_Name = fields[1],
                                Total_Amount_Agreed = Convert.ToDouble(fields[2]),
                                Start_Date_Prestaqi = Convert.ToDateTime(fields[3]),
                                Limit_Date = Convert.ToDateTime(fields[4]),
                                Rfc = fields[5],
                                Clabe = fields[7],
                                Account_Number = fields[8],
                                Is_Moral_Person = Convert.ToInt32(fields[9]) == 1 ? true : false,
                                Mail = fields[10]
                            };

                            if (investor.Account_Number.Length > 1 && investor.Account_Number.Length <= 11)
                            {

                                long accountNumber = 0;
                                long.TryParse(investor.Account_Number, out accountNumber);

                                if (accountNumber == 0)
                                {
                                    save = false;
                                    messages.Append($"{Environment.NewLine} - El Número de cuenta [{investor.Account_Number}] no tiene el formato correcto. Línea: {row}");
                                }
                            }
                            else
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - El Número de cuenta [{investor.Account_Number}] debe estar formado por menos de 12 dígitos. Línea: {row}");
                            }

                            if (investor.Clabe.Length != 18)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - La Clabe [{investor.Clabe}] debe estar formado por 18 dígitos. Línea: {row}");
                            }
                            else
                            {
                                long clabe = 0;
                                long.TryParse(investor.Clabe, out clabe);

                                if (clabe == 0)
                                {
                                    save = false;
                                    messages.Append($"{Environment.NewLine} - La Clabe [{investor.Clabe}] no tiene el formato correcto. Línea: {row}");
                                }
                            }

                            var institution = this._InstitutionRetrieveService.Where(p => p.Code == int.Parse(fields[6])).FirstOrDefault();

                            if (institution != null)
                            {
                                investor.Institution_Id = institution.id;

                                if (VerifiyMail(investor.Mail))
                                {
                                    if (save)
                                        investors.Add(investor);
                                }
                                else
                                    messages.Append($"{Environment.NewLine} - {investor.Mail} ya existe. Línea: {row}");
                            }
                            else
                                messages.Append($"{Environment.NewLine} - Código de Banco [{fields[6]}] no encontrado. Línea: {row}");

                            row += 1;
                        }
                    }
                }
            }

            if (investors.Count > 0 || messages.Length > 0)
                return new ResponseFile() { Entities = investors, Message = messages };

            throw new SystemValidationException("No se encontraron registros");
        }

        ResponseFile ProcessFileEmployeeSnac(byte[] file, int companyId)
        {
            List<string> curps = new List<string>();
            List<Accredited> accrediteds = new List<Accredited>();
            StringBuilder messages = new StringBuilder();
            var typeContract = this._TypeContract.Where(t => t.Code.ToLower() == "sueldoysalario").FirstOrDefault();
            var genders = this._GenderRetrieveService.Where(p => p.Enabled).ToList();
            var period = this._PeriodRetrieveService.Where(p => p.Description == "Quincenal" && p.User_Type == 2).FirstOrDefault();
            var institution = this._InstitutionRetrieveService.Where(p => true).FirstOrDefault();
            var license = this._LicenseRetrieveService.Where(license => license.Name == "SNAC").FirstOrDefault();
            var listAccreditedLicense = this._AccreditedRetrieveService.RetrieveResult<Func<Accredited, bool>, List<Accredited>>(a => a.License_Id == license.id && a.Deleted_At == null);
            var listAccredited = listAccreditedLicense.Where(a => a.Company_Id == companyId).ToList();
            var undeleteAccredited = new List<Accredited>();

            using (var stream = new MemoryStream(file))
            {
                using (var reader = new StreamReader(stream))
                {
                    using (TextFieldParser parse = new TextFieldParser(reader))
                    {
                        parse.TextFieldType = FieldType.Delimited;
                        parse.SetDelimiters(",");
                        int row = 1;

                        while(!parse.EndOfData)
                        {
                            bool save = true;
                            string[] fields = parse.ReadFields();

                            var existAccredited = listAccredited.Where(a => a.Curp.Trim().ToLower() == fields[11].Trim().ToLower()).FirstOrDefault();
                            if (row == 1 || existAccredited != null )
                            {
                                if (existAccredited != null)
                                {
                                    undeleteAccredited.Add(existAccredited);
                                }
                                
                                row += 1;
                                continue;
                            }

                            var splitname = fields[2].Split(' ');
                            var firstname = "";
                            var lastname = "";
                            for (int i = 0; i < splitname.Length; i++)
                            {
                                if (i == (splitname.Length - 2) || i == (splitname.Length - 1))
                                {
                                    lastname += $"{splitname[i]} ";
                                    continue;
                                }

                                firstname += $"{splitname[i]} ";
                            }

                            var sueldo = fields[6].Replace("$", "").Replace(",", "");
                            double.TryParse(sueldo, out double neto);

                            neto = neto * 30;

                            var fechaAltaSplit = fields[8].Split("/");
                            var months = 0;

                            if (fechaAltaSplit.Length >= 3)
                            {
                                DateTime now = DateTime.Now;
                                bool processYear = int.TryParse(fechaAltaSplit[2], out int yearp);
                                bool processMonth = int.TryParse(fechaAltaSplit[1], out int month);
                                bool processDay = int.TryParse(fechaAltaSplit[0], out int day);
                                DateTime alta = new DateTime(processYear ? yearp : now.Year, processMonth ? month : now.Month, processDay ? day : now.Day);

                                months = (int) (now - alta).TotalDays / 30;
                            }

                            var fechaNacimientoSplit = fields[7].Split("/");
                            DateTime birthDay = DateTime.Now;
                            int year = 0;

                            if (fechaNacimientoSplit.Length >= 3)
                            {
                                bool processYear = int.TryParse(fechaAltaSplit[2], out int yearB);
                                bool processMonth = int.TryParse(fechaAltaSplit[1], out int monthB);
                                bool processDay = int.TryParse(fechaAltaSplit[0], out int dayB);
                                DateTime date = new DateTime(processYear ? yearB : birthDay.Year, processMonth ? monthB : birthDay.Month, processDay ? dayB : birthDay.Day);
                                year = (int)(DateTime.Now - date).TotalDays / 365;

                                birthDay = date;
                            }


                            Accredited accredited = new Accredited()
                            {
                                First_Name = firstname,
                                Last_Name = lastname,
                                Address = "",
                                Colony = "",
                                Municipality = "",
                                Zip_Code = "",
                                State = "",
                                Identify = fields[0],
                                Contract_number = fields[0],
                                Position = fields[3],
                                Net_Monthly_Salary = neto,
                                Gross_Monthly_Salary = neto * .7,
                                Other_Obligations = 0,
                                Rfc = fields[10],
                                Curp = fields[11],
                                Interest_Rate = 60,
                                Seniority_Company = months.ToString(),
                                Birth_Date = birthDay,
                                Age = year,
                                Account_Number = "",
                                Clabe = "",
                                Moratoruim_Interest_Rate = 60,
                                Mail = $"{fields[11]}@snac.com",
                                Mail_Mandate_Latter = "",
                                End_Day_Payment = null,
                                Period_Start_Date = 1,
                                Period_End_Date = 15,
                                External = true,
                                NumberEmployee = fields[0],
                                ApprovedDocuments = false
                            };
                              
                            accredited.Company_Id = (int) companyId;

                            if (typeContract == null)
                            {
                                var typeContractDefault = this._TypeContract.Where(t => true).FirstOrDefault();
                                accredited.Type_Contract_Id = typeContractDefault?.id;
                            }
                            else
                            {
                                accredited.Type_Contract_Id = typeContract.id;
                            }

                            var gender = genders.Where(p => p.Description.ToLower() == fields[9].ToLower()).FirstOrDefault();
                            if (gender == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Género [{fields[9]}] no encontrado. Línea: {row}");
                            }
                            else
                            {
                                accredited.Gender_Id = gender.id;
                            }

                            if (period == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Perdiodo [Quincenal] no encontrado. Línea: {row}");
                            }
                            else
                            {
                                accredited.Period_Id = period.id;
                            }

                            if (institution == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Código de Banco no encontrado. Línea: {row}");
                            }
                            else
                            {
                                accredited.Institution_Id = institution.id;
                            }

                            if (license == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - La Licencia SNAC no esta dado de alta. Linea: {row}");
                            } else
                            {
                                accredited.License_Id = license.id;

                                var verifyNumber = listAccredited.Where(a => a.Curp?.ToLower() == fields[11].ToLower()).Count() > 0;

                                if (verifyNumber)
                                {
                                    save = false;
                                    messages.Append($"{Environment.NewLine} - La CURP ({fields[11]}) ya existe. Linea: {row}");
                                }
                            }

                            var verifyEmail = listAccreditedLicense.Where(a => a.Mail == accredited.Mail).Count() > 0;

                            if (verifyEmail)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - {accredited.Mail} ya existe. Línea: {row}");
                            }

                            if (save) {
                                curps.Add(accredited.Curp.Trim().ToLower());
                                accrediteds.Add(accredited);
                            }

                            row += 1;
                        }
                    }
                }
            }

            if (accrediteds.Count > 0 || messages.Length > 0)
            {
                var deleteAccredited = this._AccreditedRetrieveService.RetrieveResult<Func<Accredited, bool>, List<Accredited>>(accredited => !curps.Contains(accredited?.Curp?.Trim().ToLower()) && accredited.Company_Id == companyId).ToList();

                return new ResponseFile()
                {
                    Entities = accrediteds,
                    Message = messages,
                    ForDelete = deleteAccredited,
                    ForUnDelete = undeleteAccredited
                };
            }

            throw new SystemValidationException("No se encontraron registros");
        }

        ResponseFile ProcessFileAccredited(byte[] file)
        {
            List<Accredited> accrediteds = new List<Accredited>();
            StringBuilder messages = new StringBuilder();

            using (var stream = new MemoryStream(file))
            {
                using (var reader = new StreamReader(stream))
                {
                    using (TextFieldParser parser = new TextFieldParser(reader))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        int row = 1;
                        while (!parser.EndOfData)
                        {
                            bool save = true;

                            string[] fields = parser.ReadFields();

                            if (row == 1)
                            {
                                row += 1;
                                continue;
                            }

                            Accredited accredited = new Accredited()
                            {
                                First_Name = fields[0],
                                Last_Name = fields[1],
                                Address = fields[2],
                                Colony = fields[3],
                                Municipality = fields[4],
                                Zip_Code = fields[5],
                                State = fields[6],
                                Identify = fields[10],
                                Contract_number = fields[11],
                                Position = fields[12],
                                Net_Monthly_Salary = Convert.ToDouble(fields[13]),
                                Gross_Monthly_Salary = Convert.ToDouble(fields[14]),
                                Other_Obligations = Convert.ToDouble(fields[15]),
                                Rfc = fields[16],
                                Interest_Rate = Convert.ToInt32(fields[17]),
                                Seniority_Company = fields[18],
                                Birth_Date = Convert.ToDateTime(fields[19]),
                                Age = 0,
                                Account_Number = fields[24],
                                Clabe = fields[25],
                                Moratoruim_Interest_Rate = Convert.ToInt32(fields[26]),
                                Mail = fields[27],
                                Mail_Mandate_Latter = fields[28],
                                End_Day_Payment = Convert.ToDateTime(fields[29]),
                                Period_Start_Date = Convert.ToInt32(fields[30]),
                                Period_End_Date = Convert.ToInt32(fields[31])
                            };

                            var institution = this._InstitutionRetrieveService.Where(p => p.Code == int.Parse(fields[23])).FirstOrDefault();
                            if (institution == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Código de Banco [{fields[22]}] no encontrado. Línea: {row}");
                            }
                            else
                                accredited.Institution_Id = institution.id;

                            var gender = this._GenderRetrieveService.Where(p => p.Description.ToLower() == fields[20].ToLower()).FirstOrDefault();
                            if (gender == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Género [{fields[20]}] no encontrado. Línea: {row}");
                            }
                            else
                                accredited.Gender_Id = gender.id;

                            var company = this._CompanyRetrieveService.Where(p => p.Description.ToLower() == fields[7].ToLower()).FirstOrDefault();
                            if (company == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Empresa [{fields[7]}] no encontrada. Línea: {row}");
                                accredited.Company_Name = fields[7];
                            }
                            else
                                accredited.Company_Id = company.id;

                            var outsourcing = this._CompanyRetrieveService.Where(p => p.Description == fields[8]).FirstOrDefault();
                            if (outsourcing == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Empresa Outsourcing [{fields[8]}] no encontrada. Línea: {row}");
                                accredited.Outsourcing_Name = fields[8];
                            }
                            else
                                accredited.Outsourcing_id = outsourcing.id;

                            if (accredited.Zip_Code.Length != 5)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - El Código Postal [{accredited.Zip_Code}] debe estar formado por 5 dígitos. Línea: {row}");
                            }
                            else
                            {
                                int postalCode = 0;
                                int.TryParse(accredited.Zip_Code, out postalCode);

                                if (postalCode == 0)
                                {
                                    save = false;
                                    messages.Append($"{Environment.NewLine} - El Código Postal [{accredited.Zip_Code}] no tiene el formato correcto. Línea: {row}");
                                }
                            }

                            if (accredited.Account_Number.Length > 1 && accredited.Account_Number.Length <= 15 )
                            {
                                long accountNumber = 0;
                                long.TryParse(accredited.Account_Number, out accountNumber);

                                if (accountNumber == 0)
                                {
                                    save = false;
                                    messages.Append($"{Environment.NewLine} - El Número de cuenta [{accredited.Account_Number}] no tiene el formato correcto. Línea: {row}");
                                }
                            }
                            else
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - El Número de cuenta [{accredited.Account_Number}] debe estar formado por 10 dígitos. Línea: {row}");
                            }

                            if (accredited.Clabe.Length > 18)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - La Clabe [{accredited.Clabe}] debe estar formado por 18 dígitos. Línea: {row}");
                            }
                            else
                            {
                                long clabe = 0;
                                long.TryParse(accredited.Clabe, out clabe);

                                if (clabe == 0)
                                {
                                    save = false;
                                    messages.Append($"{Environment.NewLine} - La Clabe [{accredited.Clabe}] no tiene el formato correcto. Línea: {row}");
                                }
                            }

                            var period = this._PeriodRetrieveService.Where(p => p.Description == fields[21] && p.User_Type == 2).FirstOrDefault();
                            if (period == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Perdiodo [{fields[21]}] no encontrado. Línea: {row}");
                            }
                            else
                                accredited.Period_Id = period.id;

                            var typeContract = this._TypeContract.Where(t => t.Description.ToLower() == fields[9].ToLower()).FirstOrDefault();
                            if (typeContract == null)
                            {
                                var typeContractDefault = this._TypeContract.Where(t => true).FirstOrDefault();
                                accredited.Type_Contract_Id = typeContractDefault?.id;
                            }
                            else
                                accredited.Type_Contract_Id = typeContract.id;


                            if (!VerifiyMail(accredited.Mail))
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - {accredited.Mail} ya existe. Línea: {row}");
                            }


                            if (save)
                                accrediteds.Add(accredited);


                            row += 1;
                        }
                    }
                }
            }

            if (accrediteds.Count > 0 || messages.Length > 0)
                return new ResponseFile() { Entities = accrediteds, Message = messages };

            throw new SystemValidationException("No se encontraron registros");
        }

        bool VerifiyMail(string mail)
        {
            var userCount = this._UserRetrieveService.Where(p => p.Mail.ToLower() == mail.ToLower() && p.Deleted_At == null).FirstOrDefault();
            var accreditedCount = this._AccreditedRetrieveService.Where(p => p.Mail.ToLower() == mail.ToLower() && p.Deleted_At == null).FirstOrDefault();
            var investorCount = this._InvestorRetrieveService.Where(p => p.Mail.ToLower() == mail.ToLower() && p.Deleted_At == null).FirstOrDefault();
            var license = this._LicenseRetrieveService.Where(l => l.Mail != null && l.Mail.ToLower().Equals(mail.ToLower()) && l.Enabled).FirstOrDefault();
            if (userCount != null || accreditedCount != null || investorCount != null || license != null)
                return false;

            return true;
        }

        public bool ExecuteProcess(SendMailChangePassword sendMailChangePassword)
        {
            var configurations = this._ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");
            var messageConfig = configurations.FirstOrDefault(p => p.Configuration_Name == "CHANGE_PASS");
            var messageMail = JsonConvert.DeserializeObject<MessageMail>(messageConfig.Configuration_Value);
            string textHtml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), messageMail.Message));
            textHtml = textHtml.Replace("{NAME}", sendMailChangePassword.Name);
            textHtml = textHtml.Replace("{WHATSAPP}", sendMailChangePassword.Contacts.Find(p => p.id == 1).Contact_Data);
            textHtml = textHtml.Replace("{MAIL_SOPORTE}", sendMailChangePassword.Contacts.Find(p => p.id == 2).Contact_Data);
            textHtml = textHtml.Replace("{PHONE}", sendMailChangePassword.Contacts.Find(p => p.id == 3).Contact_Data);
            messageMail.Message = textHtml;

            Utilities.SendEmail(sendMailChangePassword.Mails, messageMail, mailConf);

            return true;
        }
    
        public AcceptConvenioAndAutorizacion ExecuteProcess(CartaTransferenciaDeDatos carta)
        {
            var response = new AcceptConvenioAndAutorizacion();
            response.success = false;

            try
            {

                Accredited accredited = this._AccreditedRetrieveService.RetrieveResult<Func<Accredited, bool>, List<Accredited>>(a => a.id == carta.AccreditedId).FirstOrDefault();
                if (accredited != null)
                {
                    Company company = this._CompanyRetrieveService.Where(c => c.id == accredited.Company_Id).FirstOrDefault();

                    if (company != null)
                    {
                        if (!Directory.Exists($"{this.pathProyect}/DocsFirma"))
                        {
                            Directory.CreateDirectory($"{this.pathProyect}/DocsFirma");
                        }

                        DateTime date = DateTime.Now;
                        var code = $"{accredited.id}{date.Year}{date.Month}{date.Day}";
                        string textHtml = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Documents/CartaTransferenciaDatosPersonales.html"));

                        textHtml = textHtml.Replace("{NAME}", $"{accredited.First_Name} {accredited.Last_Name}");
                        textHtml = textHtml.Replace("{COMPANY_NAME}", company.Description);
                        textHtml = textHtml.Replace("{URL_PAGE}", "https://snactehaceelparo.com");
                        textHtml = textHtml.Replace("{DAY}", date.ToString("dd"));
                        textHtml = textHtml.Replace("{MONTH}", date.ToString("MM"));
                        textHtml = textHtml.Replace("{YEAR}", date.ToString("yyyy"));

                        textHtml = HttpUtility.HtmlDecode(textHtml);

                        var memoryStream = new MemoryStream();
                        using (var pdfWriter = new PdfWriter(memoryStream))
                        {
                            pdfWriter.SetCloseStream(false);
                            using (var document = HtmlConverter.ConvertToDocument(textHtml, pdfWriter))
                            {
                            }
                        }

                        memoryStream.Position = 0;

                        using (FileStream fs = new FileStream($"{this.pathProyect}/DocsFirma/CTDP-{code}.pdf", FileMode.OpenOrCreate))
                        {
                            memoryStream.CopyTo(fs);
                            fs.Flush();
                        }

                        memoryStream.Close();

                        memoryStream = new MemoryStream();


                        string textHtmlConvenio = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Documents/Convenio.html"));

                        textHtmlConvenio = textHtmlConvenio.Replace("{COMPANY_NAME}", company.Description);
                        textHtmlConvenio = textHtmlConvenio.Replace("{NUMBER_CONVENIO}", code.PadLeft(12, '0'));

                        textHtmlConvenio = HttpUtility.HtmlDecode(textHtmlConvenio);

                        using (var pdfWriter = new PdfWriter(memoryStream))
                        {
                            pdfWriter.SetCloseStream(false);
                            using (var document = HtmlConverter.ConvertToDocument(textHtmlConvenio, pdfWriter))
                            {
                            }
                        }

                        memoryStream.Position = 0;

                        using (FileStream fs = new FileStream($"{this.pathProyect}/DocsFirma/CONVENIO-{code}.pdf", FileMode.OpenOrCreate))
                        {
                            memoryStream.CopyTo(fs);
                            fs.Flush();
                        }

                        memoryStream.Close();

                        response.success = true;
                    }
                }

            } catch(Exception exception)
            {
                throw new SystemValidationException($"Error generate contract: {exception.Message}");
            }

            return response;
        }
    }
}
