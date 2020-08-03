using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Enum;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PrestaQi.Api
{
    public class MonitoringService
    {
        static  NotificationsMessageHandler _NotificationsMessageHandler { get; set; }
        static IWriteService<Model.Notification> _NotificationWriteService;
        static IRetrieveService<Model.Notification> _NotificationRetrieveService;
        static IProcessService<Capital> _CapitalProcessService;
        static IRetrieveService<User> _UserRetrieveService;
        static IRetrieveService<Model.Configuration> _ConfigurationRetrieveService;
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

            int days = (DateTime.Now.Date - list.FirstOrDefault().Pay_Day_Limit.Date).Days;

            if (days == 10 || days == 5 || days == 3 || days == 0)
                SendNotificationInterestPayment(days);
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
            },mailConf);

        }
    }
}
