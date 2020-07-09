using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using InsiscoCore.Utilities.IO;
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
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
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

        public UserProcessService(
            IRetrieveService<User> userRetrieveService,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Institution> institutionRetrieveService,
            IRetrieveService<Gender> genderRetrieveService,
            IRetrieveService<Company> companyRetrieveService,
            IRetrieveService<Period> periodRetrieveService
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

            var userMail = this._UserRetrieveService.Where(p => p.Mail == recoveryPassword.Mail).FirstOrDefault();
            var accreditedMail = this._AccreditedRetrieveService.Where(p => p.Mail == recoveryPassword.Mail).FirstOrDefault();
            var investorMail = this._InvestorRetrieveService.Where(p => p.Mail == recoveryPassword.Mail).FirstOrDefault();

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
                string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations, messageMail.Message))).ReadToEnd();
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
                                messages.Append($"{Environment.NewLine} - {user.Mail} already exists. Line: {row.ToString()}");

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

                            var institution = this._InstitutionRetrieveService.Where(p => p.Code == int.Parse(fields[6])).FirstOrDefault();

                            if (institution != null)
                            {
                                investor.Institution_Id = institution.id;

                                if (VerifiyMail(investor.Mail))
                                    investors.Add(investor);
                                else
                                    messages.Append($"{Environment.NewLine} - {investor.Mail} already exists. Line: {row}");
                            }
                            else
                                messages.Append($"{Environment.NewLine} - Bank code [{fields[6]}] was not found. Line: {row}");

                            row += 1;
                        }
                    }
                }
            }

            if (investors.Count > 0 || messages.Length > 0)
                return new ResponseFile() { Entities = investors, Message = messages };

            throw new SystemValidationException("No records found");
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
                                Identify = fields[3],
                                Contract_number = fields[4],
                                Position = fields[5],
                                Net_Monthly_Salary = Convert.ToDouble(fields[6]),
                                Rfc = fields[7],
                                Interest_Rate = Convert.ToInt32(fields[8]),
                                Seniority_Company = fields[9],
                                Birth_Date = Convert.ToDateTime(fields[10]),
                                Age = Convert.ToInt32(fields[11]),
                                Account_Number = fields[15],
                                Clabe = fields[16],
                                Moratoruim_Interest_Rate = Convert.ToInt32(fields[17]),
                                Mail = fields[18],
                                Mail_Mandate_Latter = fields[19]
                            };

                            var institution = this._InstitutionRetrieveService.Where(p => p.Code == int.Parse(fields[14])).FirstOrDefault();
                            if (institution == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Bank code [{fields[14]}] was not found. Line: {row}");
                            }
                            else
                                accredited.Institution_Id = institution.id;

                            var gender = this._GenderRetrieveService.Where(p => p.Description == fields[12]).FirstOrDefault();
                            if (gender == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Gender [{fields[12]}] not found. Line: {row}");
                            }
                            else
                                accredited.Gender_Id = gender.id;

                            var company = this._CompanyRetrieveService.Where(p => p.Description == fields[2]).FirstOrDefault();
                            if (company == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Company [{fields[2]}] not found. Line: {row}");
                            }
                            else
                                accredited.Company_Id = company.id;

                            var period = this._PeriodRetrieveService.Where(p => p.Description == fields[13]).FirstOrDefault();
                            if (period == null)
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - Period [{fields[13]}] not found. Line: {row}");
                            }
                            else
                                accredited.Period_Id = period.id;


                            if (!VerifiyMail(accredited.Mail))
                            {
                                save = false;
                                messages.Append($"{Environment.NewLine} - {accredited.Mail} already exists. Line: {row}");
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

            throw new SystemValidationException("No records found");
        }

        bool VerifiyMail(string mail)
        {
            var userCount = this._UserRetrieveService.Where(p => p.Mail == mail && p.Deleted_At == null).FirstOrDefault();
            var accreditedCount = this._AccreditedRetrieveService.Where(p => p.Mail == mail && p.Deleted_At == null).FirstOrDefault();
            var investorCount = this._InvestorRetrieveService.Where(p => p.Mail == mail && p.Deleted_At == null).FirstOrDefault();
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
            string textHtml = new StreamReader(new MemoryStream(Utilities.GetFile(configurations, messageMail.Message))).ReadToEnd();
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
