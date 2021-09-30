using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Dto.Output.Pliis;
using PrestaQi.Model.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace PrestaQi.Service.RetrieveServices
{
    public class UserRetrieveService : RetrieveService<User>
    {
        IRetrieveService<Investor> _InvestorRetrieveService;
        IRetrieveService<Accredited> _AccreditedRetrieveService;
        IRetrieveService<UserModule> _UserModuleRetrieveService;
        IRetrieveService<License> _LicenseRetrieveService;

        public UserRetrieveService(
            IRetrieveRepository<User> repository,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<UserModule> userModuleRetrieveService,
            IRetrieveService<License> licenseRetrieveService
            ) : base(repository)
        {
            this._InvestorRetrieveService = investorRetrieveService;
            this._AccreditedRetrieveService = accreditedRetrieveService;
            this._UserModuleRetrieveService = userModuleRetrieveService;
            this._LicenseRetrieveService = licenseRetrieveService;
        }

        public override IEnumerable<User> Where(Func<User, bool> predicate)
        {
            var users = this._Repository.Where(predicate).ToList();

            users.ForEach(p =>
            {
                p.Modules = this._UserModuleRetrieveService.Where(m => m.user_id == p.id).ToList();
                p.Type = (int)PrestaQiEnum.UserType.Administrador;
                p.TypeName = PrestaQiEnum.UserType.Administrador.ToString();
            });

            return users;
        }

        public UserLogin RetrieveResult(Login login)
        {
            User user = this._Repository.Where(p => p.Mail.ToLower() == login.Mail.ToLower() && p.Deleted_At == null).FirstOrDefault();
            Investor investor = this._InvestorRetrieveService.Where(p => p.Mail.ToLower() == login.Mail.ToLower() && p.Deleted_At == null).FirstOrDefault();
            Accredited accredited = this._AccreditedRetrieveService.Where(p => p.Mail.ToLower() == login.Mail.ToLower() && p.Deleted_At == null).FirstOrDefault();
            License license = this._LicenseRetrieveService.Where(l => l.Mail != null && l.Mail.ToLower().Equals(login.Mail.ToLower()) && l.Enabled).FirstOrDefault();

            if (user != null)
            {
                if (!user.Enabled)
                    throw new SystemValidationException("Inactive User!");

                if (user.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                user.Modules = this._UserModuleRetrieveService.Where(p => p.user_id == user.id).ToList();

                return new UserLogin() { Type = 1, User = user };
            }

            if (investor != null)
            {
                if (!investor.Enabled)
                    throw new SystemValidationException("Inactive User!");

                if (investor.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                return new UserLogin() { Type = 2, User = investor };
            }

            if (accredited != null)
            {
                if (!accredited.Enabled)
                    throw new SystemValidationException("Inactive User!");

                if (accredited.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                accredited.Advances = null;

                return new UserLogin() { Type = 3, User = accredited };
            }

            if (license != null)
            {
                if (license.Password != InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password))
                    throw new SystemValidationException("Invalid Password!");

                return new UserLogin() { Type = 4, User = license };
            }

            Accredited accreditedExternal = this.LoginExternal(login);

            if (accreditedExternal != null)
            {
                return new UserLogin() { Type = 3, User = accreditedExternal };
            }

            throw new SystemValidationException("User not found!");
        }

        public UserLogin RetrieveResult(DisableUser disableUser)
        { 
            if (disableUser.Type == 1)
            {
                User user = this._Repository.Where(p => p.id == disableUser.UserId).FirstOrDefault();
                if (disableUser.IsDelete)
                {
                    user.Enabled = false;
                    user.Deleted_At = DateTime.Now;
                }
                else
                    user.Enabled = disableUser.Value;
                
                return new UserLogin() { User = user };
            }

            if (disableUser.Type == 2)
            {
                Investor investor = this._InvestorRetrieveService.Where(p => p.id == disableUser.UserId).FirstOrDefault();
                if (disableUser.IsDelete)
                {
                    investor.Enabled = false;
                    investor.Deleted_At = DateTime.Now;
                }
                else
                    investor.Enabled = disableUser.Value;

                return new UserLogin() { User = investor };
            }

            if (disableUser.Type == 3)
            {
                Accredited accredited = this._AccreditedRetrieveService.Where(p => p.id == disableUser.UserId).FirstOrDefault();
                if (disableUser.IsDelete)
                {
                    accredited.Enabled = false;
                    accredited.Deleted_At = DateTime.Now;
                }
                else
                    accredited.Enabled = disableUser.Value;

                return new UserLogin() { User = accredited };
            }

            throw new SystemValidationException("User not found!");
        }

        public UserLogin RetrieveResult(UserByType userByType)
        {
            if (userByType.UserType == Model.Enum.PrestaQiEnum.UserType.Administrador)
            {
                var user = this._Repository.Find(userByType.User_Id);
                user.Modules = this._UserModuleRetrieveService.Where(p => p.user_id == user.id).ToList();
                return new UserLogin() { User = user };
            }

            if (userByType.UserType == Model.Enum.PrestaQiEnum.UserType.Inversionista)
            {
                var user = this._InvestorRetrieveService.Find(userByType.User_Id);
                return new UserLogin() { User = user };
            }

            if (userByType.UserType == Model.Enum.PrestaQiEnum.UserType.Acreditado)
            {
                var user = this._AccreditedRetrieveService.Find(userByType.User_Id);
                user.Advances = null;
                return new UserLogin() { User = user };
            }

            throw new SystemValidationException("Type not found!");
        }

        public Accredited LoginExternal(Login login)
        {
            LoginPliis loginPliis = new LoginPliis() { nick = login.Mail, passw = login.Password };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(loginPliis);

            var request = (HttpWebRequest)WebRequest.Create(@"https://pliis.mx/PLIIS/auth/login");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            LoginResponse responseBody;

            using (WebResponse response2 = request.GetResponse())
            {
                using (Stream strReader = response2.GetResponseStream())
                {
                    using (StreamReader objReader = new StreamReader(strReader))
                    {
                        var response = objReader.ReadToEnd();
                        if (response == String.Empty)
                        {
                            throw new SystemValidationException("Invalid Credentials!");
                        }

                        responseBody = JsonSerializer.Deserialize<LoginResponse>(response);
                    }
                }
            }

            if (responseBody == null)
            {
                throw new SystemValidationException("User Not Found!");
            }

            var existAccredited = this._AccreditedRetrieveService.Where(accredited => accredited.Rfc.ToLower().Trim() == responseBody.dataUser.rfc.ToLower().Trim()).FirstOrDefault();

            if (existAccredited != null)
            {
                return existAccredited;
            }

            License license = this._LicenseRetrieveService.Where(license => license.Name == login.LicenceName && license.Enabled).FirstOrDefault();

            if (login.LicenceName is null)
            {
                license = this._LicenseRetrieveService.Where(license => license.Enabled).FirstOrDefault();
            }

            Accredited accredited = new Accredited();
            if (license != null)
            {
                accredited.License_Id = license.id;
            }
            accredited.id = 0;
            accredited.Account_Number = "";
            accredited.Company_Name = responseBody.dataUser.companyName ?? "SNAC";
            accredited.Outsourcing_Name = "";
            accredited.First_Name = responseBody.dataUser.firstName;
            accredited.Last_Name = responseBody.dataUser.lastName;
            accredited.Mail = responseBody.nick.Replace("@", "-SNAC@");
            accredited.Type_Contract_Id = (int) PrestaQiEnum.AccreditedContractType.WagesAndSalaries;
            accredited.Net_Monthly_Salary = 0;
            accredited.Gross_Monthly_Salary = responseBody.dataUser.parseGrossMonthlySalary();
            accredited.Rfc = responseBody.dataUser.rfc;
            accredited.Interest_Rate = 60;
            accredited.Moratoruim_Interest_Rate = 60;
            accredited.Birth_Date = DateTime.ParseExact(responseBody.dataUser.birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            accredited.Gender_Id = responseBody.dataUser.genderId == 0 ? 1 : 2;
            accredited.Address = responseBody.dataUser.address;
            accredited.Colony = responseBody.dataUser.colony ?? "";
            accredited.Municipality = responseBody.dataUser.municipality ?? "";
            accredited.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(login.Password);
            accredited.Period_Id = (int)PrestaQiEnum.PerdioAccredited.Quincenal;
            accredited.Period_Start_Date = 1;
            accredited.Period_End_Date = 15;
            accredited.Institution_Id = 18;
            accredited.External = true;
            accredited.created_at = DateTime.Now;
            accredited.updated_at = DateTime.Now;

            return accredited;
        }
      
    }
}
