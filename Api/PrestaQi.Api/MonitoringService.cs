using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Dto.Output.Stp;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace PrestaQi.Api
{
    public class MonitoringService
    {
        static NotificationsMessageHandler _NotificationsMessageHandler { get; set; }
        static IWriteService<Model.Notification> _NotificationWriteService;
        static IRetrieveService<Model.Notification> _NotificationRetrieveService;
        static IProcessService<Capital> _CapitalProcessService;
        static IRetrieveService<User> _UserRetrieveService;
        static IRetrieveService<Model.Configuration> _ConfigurationRetrieveService;

        static IRetrieveService<License> _licenseRetrieveService;
        static IRetrieveService<LicensePriceRange> _licensePriceRetrieveService;
        static IRetrieveService<Accredited> _accreditedRetrieveService;
        static IRetrieveService<Advance> _advanceRetrieveService;
        static IWriteService<LicenseDeposits> _licenseDepositWriteService;
        static IWriteService<License> _licenseWriteService;

        static IConfiguration _Configuration;
        static System.Timers.Timer _Timer;

        static void GenerateInstances(IServiceProvider serviceProvider)
        {
            _NotificationsMessageHandler = serviceProvider.GetService<NotificationsMessageHandler>();
            _NotificationWriteService = serviceProvider.GetService<IWriteService<Model.Notification>>();
            _CapitalProcessService = serviceProvider.GetService<IProcessService<Capital>>();
            _UserRetrieveService = serviceProvider.GetService<IRetrieveService<User>>();
            _NotificationRetrieveService = serviceProvider.GetService<IRetrieveService<Model.Notification>>();
            _ConfigurationRetrieveService = serviceProvider.GetService<IRetrieveService<Model.Configuration>>();

            _licenseRetrieveService = serviceProvider.GetService<IRetrieveService<License>>();
            _licensePriceRetrieveService = serviceProvider.GetService<IRetrieveService<LicensePriceRange>>();
            _accreditedRetrieveService = serviceProvider.GetService<IRetrieveService<Accredited>>();
            _advanceRetrieveService = serviceProvider.GetService<IRetrieveService<Advance>>();
            _licenseDepositWriteService = serviceProvider.GetService<IWriteService<LicenseDeposits>>();
            _licenseWriteService = serviceProvider.GetService<IWriteService<License>>();
        }

        public static void Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            GenerateInstances(serviceProvider);
            _Configuration = configuration;

            _Timer = new System.Timers.Timer()
            {
                Interval = 600000,
            };

            _Timer.Elapsed += Timer_Elapsed;
            _Timer.Enabled = true;
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _Timer.Enabled = false;
            VerifyInteresetPayment();
            ProcessFundingControl();
            ProcessFundLicenses();
            _Timer.Enabled = true;
        }

        static void VerifyInteresetPayment()
        {
            if (_NotificationRetrieveService.Where(p => p.NotificationType == PrestaQiEnum.NotificationType.PaymentInterest &&
                p.updated_at.Date == DateTime.Now.Date
            ).Count() > 0)
                return;

            var result = _CapitalProcessService.ExecuteProcess<AnchorByFilter, AnchorControlPagination>(new AnchorByFilter()
            {
                Type = 0,
                Filter = ""
            });

            DateTime nextDatePayment = DateTime.Now;
            List<MyInvestment> list = new List<MyInvestment>();

            result.AnchorControls.ForEach(p =>
            {
                p.MyInvestments.ForEach(z =>
                {
                    if (z.Enabled == "Activo")
                        list.Add(z);
                });
            });

            list = list.OrderBy(p => p.Pay_Day_Limit).ToList();

            var first = list.FirstOrDefault();

            if (first != null)
            {
                int days = (DateTime.Now.Date - list.FirstOrDefault().Pay_Day_Limit.Date).Days;

                if (days == 10 || days == 5 || days == 3 || days == 0)
                    SendNotificationInterestPayment(days);
            }
        }

        static void SendNotificationInterestPayment(int days)
        {
            var notificationAdmin = days < 0 ? _Configuration.GetSection("Notification").GetSection("PayInterest").Get<Model.Notification>() :
                _Configuration.GetSection("Notification").GetSection("PayInterest1").Get<Model.Notification>();

            if (days < 0)
            {
                notificationAdmin.Title = string.Format(notificationAdmin.Title, days);
                notificationAdmin.Message = string.Format(notificationAdmin.Message, days);
            }

            notificationAdmin.NotificationType = PrestaQiEnum.NotificationType.PaymentInterest;
            notificationAdmin.User_Type = (int)PrestaQiEnum.UserType.Administrador;
            notificationAdmin.Icon = PrestaQiEnum.NotificationIconType.info.ToString();

            var admintratorList = _UserRetrieveService.Where(p => p.Enabled == true && p.Deleted_At == null).ToList();

            foreach (var item in admintratorList)
            {
                notificationAdmin.User_Id = item.id;
                _NotificationWriteService.Create(notificationAdmin);
                _ = _NotificationsMessageHandler.SendMessageToAllAsync(notificationAdmin);
                notificationAdmin.id = 0;
            }

            var configurations = _ConfigurationRetrieveService.Where(p => p.Enabled == true).ToList();
            var mailConf = configurations.FirstOrDefault(p => p.Configuration_Name == "EMAIL_CONFIG");

            Utilities.SendEmail(admintratorList.Select(p => p.Mail).ToList(), new MessageMail()
            {
                Message = notificationAdmin.Message,
                Subject = notificationAdmin.Title
            }, mailConf);

        }

        static void ProcessPayCentroCosto()
        {
            var licenseActive = _licenseRetrieveService.Where(license => license.Enabled && license.DatePayment >= DateTime.Now && license.DatePayment.Day == DateTime.Now.Day).ToList();

            foreach (License license in licenseActive)
            {
                var accreditedIds = _accreditedRetrieveService.Where(accredited => accredited.License_Id == license.id).Select(accredited => accredited.id).ToList();
                var advances = _advanceRetrieveService.Where(advance => accreditedIds.Contains(advance.Accredited_Id)).Count();

                var price = _licensePriceRetrieveService.Where(price => ((advances >= price.InitialQuantity && advances <= price.FinalQuantity) || (advances >= price.InitialQuantity && price.FinalQuantity is null) || (advances <= price.FinalQuantity && price.InitialQuantity is null) && price.LicenseId == license.id)).First();

                var priceDefault = _licensePriceRetrieveService.Where(price => ((advances >= price.InitialQuantity && advances <= price.FinalQuantity) || (advances >= price.InitialQuantity && price.FinalQuantity is null) || (advances <= price.FinalQuantity && price.InitialQuantity is null) && price.LicenseId == null)).First();

                if (price != null)
                {
                    //insert payment
                } else if (priceDefault != null)
                {
                    //insert payment
                }
            }
        }

        static void ProcessFundingControl()
        {
            if (!(DateTime.Now.Hour == 7 || DateTime.Now.Hour == 19)) return;

            string baseUri = "https://demo.stpmex.com:7024/";
            string urlMethod = "speiws/rest/ordenPago/consOrdenesFech";
            string directoryKey = "Key/devKey.p12";
            string passKey = "prestaqi2020*";
            OrdenesEnviadasRecibidas orderBody = new OrdenesEnviadasRecibidas();

            if (_Configuration["environment"] == "prod")
            {
                baseUri = _ConfigurationRetrieveService.Where(license => license.Enabled).ToList().Find(p => p.Configuration_Name == "API_SPEI").Configuration_Value;
                directoryKey = _ConfigurationRetrieveService.Where(license => license.Enabled).ToList().Find(p => p.Configuration_Name == "CERTIFIED_FTP").Configuration_Value;
                passKey = _ConfigurationRetrieveService.Where(license => license.Enabled).ToList().Find(p => p.Configuration_Name == "CERTIFIED_PASSWORD").Configuration_Value;
            }

            byte[] file = System.IO.File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), directoryKey));
            Service.Tools.CryptoHandler crypto = new Service.Tools.CryptoHandler();
            crypto.password = passKey;

            var licenses = _licenseRetrieveService.Where(license => license.Enabled).ToList();

            licenses.ForEach((license) =>
            {
                ResponseOrderPay responseOrderPay = new ResponseOrderPay();
                string sign = crypto.signForConstaOrdenEnviadasRecibidas(license.CostCenter, DateTime.Now, file);
                orderBody.firma = sign;
                orderBody.empresa = license.CostCenter;
                orderBody.estado = "R";
                orderBody.fechaOperacion = DateTime.Now.ToString("yyyyMMdd");

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create($"{baseUri}{urlMethod}");
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Accept = "application/json";

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(orderBody);

                    using (var streamWrite = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWrite.Write(json);
                        streamWrite.Flush();
                        streamWrite.Close();
                    }

                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream strReader = response.GetResponseStream())
                        {
                            if (strReader != null)
                            {
                                using (StreamReader objReader = new StreamReader(strReader))
                                {
                                    string responseBody = objReader.ReadToEnd();
                                    responseOrderPay = JsonConvert.DeserializeObject<ResponseOrderPay>(responseBody);
                                }
                            }
                        }
                    }

                    if (responseOrderPay.resultado.lst != null) {
                        responseOrderPay.resultado.lst.ForEach(deposit =>
                        {
                            deposit.LicenseId = license.id;
                        });
                    }

                    _licenseDepositWriteService.Create(responseOrderPay.resultado.lst);
                }
                catch (Exception e) {

                }
            });
        }

        static void ProcessFundLicenses()
        {
            if (!(DateTime.Now.Hour == 7 || DateTime.Now.Hour == 19)) return;

            string baseUri = "https://demo.stpmex.com:7024/";
            string urlMethod = "speiws/rest/ordenPago/consOrdenesFech";
            string directoryKey = "Key/devKey.p12";
            string passKey = "prestaqi2020*";

            if (_Configuration["environment"] == "prod")
            {
                baseUri = _ConfigurationRetrieveService.Where(license => license.Enabled).ToList().Find(p => p.Configuration_Name == "API_SPEI").Configuration_Value;
                directoryKey = _ConfigurationRetrieveService.Where(license => license.Enabled).ToList().Find(p => p.Configuration_Name == "CERTIFIED_FTP").Configuration_Value;
                passKey = _ConfigurationRetrieveService.Where(license => license.Enabled).ToList().Find(p => p.Configuration_Name == "CERTIFIED_PASSWORD").Configuration_Value;
            }

            byte[] file = System.IO.File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), directoryKey));
            Service.Tools.CryptoHandler crypto = new Service.Tools.CryptoHandler();
            crypto.password = passKey;

            var licenses = _licenseRetrieveService.Where(license => license.Enabled).ToList();

            licenses.ForEach((license) =>
            {
                AccountBalanceOutput accountBalance = new AccountBalanceOutput();
                string sign = crypto.signForSaldoCuenta(license.OriginatorAccount ?? "", file);

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create($"{baseUri}{urlMethod}");
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Accept = "application/json";

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(new AccountBalance()
                    {
                        firma = sign,
                        cuentaOrdenante = license.OriginatorAccount
                    });

                    using (var streamWrite = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWrite.Write(json);
                        streamWrite.Flush();
                        streamWrite.Close();
                    }

                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream strReader = response.GetResponseStream())
                        {
                            if (strReader != null)
                            {
                                using (StreamReader objReader = new StreamReader(strReader))
                                {
                                    string responseBody = objReader.ReadToEnd();
                                    var responseObject = JsonConvert.DeserializeObject<ResponseAccountBalance>(responseBody);

                                    accountBalance = responseObject.resultado.saldoCuenta;
                                }
                            }
                        }
                    }

                    if (accountBalance != null)
                    {
                        license.Balance = (double)accountBalance.saldo;
                        license.Prices = _licensePriceRetrieveService.Where(price => price.LicenseId == license.id).ToList();

                        _licenseWriteService.Update(license);
                    }

                } catch(Exception e)
                {

                }
            });
        }
    }
}
