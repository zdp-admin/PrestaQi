using InsiscoCore.Base.Data;
using InsiscoCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Service.WriteServices
{
    public class DeviceWriteService : WriteService<Device>
    {
        public DeviceWriteService(IWriteRepository<Device> repository) : base(repository)
        {
        }

        public override bool Create(Device entity)
        {
            entity.Enabled = true;
            entity.created_at = DateTime.Now;
            entity.updated_at = DateTime.Now;
            return base.Create(entity);
        }
    }
}
