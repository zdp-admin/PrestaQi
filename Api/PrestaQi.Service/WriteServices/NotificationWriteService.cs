using DocumentFormat.OpenXml.Office2010.ExcelAc;
using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using iText.Forms.Xfdf;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class NotificationWriteService : WriteService<Notification>
    {
        IRetrieveService<Model.Notification> _NotificationRetrieveService;
        IProcessService<Model.Notification> _NotificationProcessService;

        public NotificationWriteService(
            IWriteRepository<Notification> repository,
            IRetrieveService<Model.Notification> notificationRetrieveService,
            IProcessService<Model.Notification> notificationProcessService
            ) : base(repository)
        {
            this._NotificationRetrieveService = notificationRetrieveService;
            this._NotificationProcessService = notificationProcessService;
        }

        public override bool Create(Notification entity)
        {
            try
            {
                entity.Data_Text = JsonConvert.SerializeObject(entity.Data);
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;
                bool created = base.Create(entity);

                this._NotificationProcessService.ExecuteProcess<Notification, bool>(entity);

                return created;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error al guardar la notificación: {exception.Message}");
            }
            
        }

        public bool Update(DisableNotification notification)
        {
            try
            {
                var notifications = this._NotificationRetrieveService.Where(p => notification.NotificationIds.Contains(p.id)).ToList();

                if (notifications.Count > 0)
                {
                    notifications.ForEach(p => { p.updated_at = DateTime.Now; p.Is_Read = true; });

                    base.Update(notifications);
                }

                return true;
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error al desabilitar las notificaciones: {exception.Message}");
            }
        }
    }
}
