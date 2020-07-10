using InsiscoCore.Base.Data;
using InsiscoCore.Service;
using PrestaQi.Model;
using System;

namespace PrestaQi.Service.WriteServices
{
    public class NotificationWriteService : WriteService<Notification>
    {
        public NotificationWriteService(IWriteRepository<Notification> repository) : base(repository)
        {
        }

        public override bool Create(Notification entity)
        {
            entity.created_at = DateTime.Now;
            entity.updated_at = DateTime.Now; 
            return base.Create(entity);
        }
    }
}
