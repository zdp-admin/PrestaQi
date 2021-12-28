using InsiscoCore.Base.Service;
using InsiscoCore.Service;
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

        public UserProcessService(
            IRetrieveService<User> userRetrieveService,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Institution> institutionRetrieveService,
            IRetrieveService<Gender> genderRetrieveService,
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Period> periodRetrieveService,
            IRetrieveService<TypeContract> typeContract
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
                                Account_Number = fields[23],
                                Clabe = fields[24],
                                Moratoruim_Interest_Rate = Convert.ToInt32(fields[25]),
                                Mail = fields[26],
                                Mail_Mandate_Latter = fields[27],
                                End_Day_Payment = Convert.ToDateTime(fields[28]),
                                Period_Start_Date = Convert.ToInt32(fields[29]),
                                Period_End_Date = Convert.ToInt32(fields[30])
                            };

                            var institution = this._InstitutionRetrieveService.Where(p => p.Code == int.Parse(fields[22])).FirstOrDefault();
                            if (institution == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Código de Banco [{fields[22]}] no encontrado. Línea: {row}");
                            }
                            else
                                accredited.Institution_Id = institution.id;

                            var gender = this._GenderRetrieveService.Where(p => p.Description == fields[20]).FirstOrDefault();
                            if (gender == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Género [{fields[20]}] no encontrado. Línea: {row}");
                            }
                            else
                                accredited.Gender_Id = gender.id;

                            var company = this._CompanyRetrieveService.Where(p => p.Description == fields[7]).FirstOrDefault();
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

                            if (accredited.Account_Number.Length > 1 && accredited.Account_Number.Length <= 11 )
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

                            if (accredited.Clabe.Length != 18)
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
            if (userCount != null || accreditedCount != null || investorCount != null)
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
    }
}
