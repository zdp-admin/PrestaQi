using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Microsoft.Extensions.Configuration;
using OpenXmlPowerTools;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.FireBase;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace PrestaQi.Service.ProcessServices
{
    public class NotificationProcessService : ProcessService<Notification>
    {
        IRetrieveService<Configuration> _ConfigurationRetrieveService;
        IRetrieveService<Device> _DeviceRetrieveService;
        public IConfiguration Configuration { get; }

        public NotificationProcessService(
            IRetrieveService<Configuration> configurationRetrieveService,
            IRetrieveService<Device> deviceRetrieveService,
            IConfiguration configuration
            )
        {
            this._ConfigurationRetrieveService = configurationRetrieveService;
            this._DeviceRetrieveService = deviceRetrieveService;
            this.Configuration = configuration;
        }

        public bool ExecuteProcess(Notification notification)
        {
			try
			{

                if (Configuration["environment"] == "dev")
                {
                    return true;
                }

                var devices = this._DeviceRetrieveService.Where(p => p.User_Id == notification.User_Id && p.User_Type == notification.User_Type)
                .Select(p => p.Device_Id).ToList();

                if (devices.Count > 0) {
                    var url = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "FIREBASE_URL_SEND").FirstOrDefault().Configuration_Value;
                    var token = this._ConfigurationRetrieveService.Where(p => p.Configuration_Name == "FIREBASE_KEY").FirstOrDefault().Configuration_Value;


                    SendFirebase sendFirebase = new SendFirebase()
                    {
                        priority = "high",
                        notification = new FireNotification() { body = notification.Message, title = notification.Title, badge = 1 },
                        registration_ids = devices
                    };
                    sendFirebase.data = new ExpandoObject();
                    sendFirebase.data.click_action = "FLUTTER_NOTIFICATION_CLICK";
                    sendFirebase.data.icon = notification.Icon;
                    sendFirebase.data.created_at = notification.created_at;
                    sendFirebase.data.notification_id = notification.id;

                    if (notification.NotificationType == Model.Enum.PrestaQiEnum.NotificationType.SetPaymentPeriod)
                    {
                        sendFirebase.data.Period_Id = notification.Data.Period_Id;
                        sendFirebase.data.Capital_Id = notification.Data.Capital_Id;
                        sendFirebase.data.Payment = notification.Data.Payment;
                        sendFirebase.data.Investor_Id = notification.Data.Investor_Id;
                    }

                    if (notification.NotificationType == Model.Enum.PrestaQiEnum.NotificationType.CapitalCall)
                    {
                        sendFirebase.data.Capital_Id = notification.Data.Capital_Id;
                        sendFirebase.data.Amount = notification.Data.Amount;
                    }

                    if (notification.NotificationType == Model.Enum.PrestaQiEnum.NotificationType.ChangeStatusCapital)
                        sendFirebase.data.Capital_Id = notification.Data.Capital_Id;


                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendFirebase);

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Accept = "application/json";
                    request.Headers.Add(HttpRequestHeader.Authorization, token);

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
                            using (StreamReader objReader = new StreamReader(strReader))
                            {
                                string responseBody = objReader.ReadToEnd();
                            }
                        }
                    }
                }

                return true;
            }
			catch (Exception exception)
			{
                throw new SystemValidationException($"Error al enviar la notificación con FireBase: {exception.Message}");
			}
        }
    }
}
