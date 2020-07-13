using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using System;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class DeviceWriteService : WriteService<Device>
    {
        IRetrieveService<Device> _DeviceRetrieveService;

        public DeviceWriteService(
            IWriteRepository<Device> repository,
            IRetrieveService<Device> deviceRetrieveService
            ) : base(repository)
        {
            this._DeviceRetrieveService = deviceRetrieveService;
        }

        public override bool Create(Device entity)
        {
            var deviceFound = this._DeviceRetrieveService.Where(p => p.Device_Id == entity.Device_Id).FirstOrDefault();
            if (deviceFound != null)
                throw new SystemValidationException("El Token de Dispositivo ya se encuentra agregado");

            entity.Enabled = true;
            entity.created_at = DateTime.Now;
            entity.updated_at = DateTime.Now;
            return base.Create(entity);
        }
    }
}
