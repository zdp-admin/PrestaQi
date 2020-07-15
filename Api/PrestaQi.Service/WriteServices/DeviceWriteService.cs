using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using Org.BouncyCastle.Asn1.Cms;
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
            {
                deviceFound.User_Id = entity.User_Id;
                deviceFound.User_Type = entity.User_Type;
                return base.Update(deviceFound);
            }
            else
            {
                entity.Enabled = true;
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;
                return base.Create(entity);
            }
        }

        public bool Delete(string token)
        {
            var device = this._DeviceRetrieveService.Where(p => p.Device_Id == token).FirstOrDefault();

            if (device == null)
                throw new SystemValidationException("Dispositivo no encontrado");

            return base.Delete(device);
        }
    }
}
