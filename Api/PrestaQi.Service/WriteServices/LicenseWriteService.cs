using InsiscoCore.Base.Data;
using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Configurations;
using PrestaQi.Service.Tools;
using System;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class LicenseWriteService : WriteService<License>
    {
        IRetrieveService<License> _licenseRetrieveService;
        IProcessService<User> _UserProcessService;
        IWriteRepository<LicensePriceRange> _licensePriceWriteService;
        IRetrieveService<LicensePriceRange> _licensePriceRetrieveService;

        public LicenseWriteService(
            IWriteRepository<License> repository,
            IProcessService<User> userProcessService,
            IRetrieveService<License> licenseRetrieveService,
            IWriteRepository<LicensePriceRange> licensePriceWriteService,
            IRetrieveService<LicensePriceRange> licensePriceRetrieveService
            ) : base(repository)
        {
            this._licenseRetrieveService = licenseRetrieveService;
            this._UserProcessService = userProcessService;
            this._licensePriceWriteService = licensePriceWriteService;
            this._licensePriceRetrieveService = licensePriceRetrieveService;
        }

        public override bool Create(License entity)
        {
            this._UserProcessService.ExecuteProcess<string, bool>(entity.Mail);

            try
            {
                string password = Utilities.GetPasswordRandom();
                entity.Password = InsiscoCore.Utilities.Crypto.MD5.Encrypt(password);
                entity.Enabled = entity.FirstLogin = true;
                entity.created_at = DateTime.Now;
                entity.updated_at = DateTime.Now;

                base.Create(entity);

                foreach (var price in entity.Prices)
                {
                    if (price.InitialQuantity >= price.FinalQuantity)
                    {
                        throw new SystemValidationException("Uno o más rangos de precios son incorrectos");
                    }

                    price.LicenseId = entity.id;

                    this._licensePriceWriteService.Create(price);
                }

                return true;
            } catch(Exception exception)
            {
                throw new SystemValidationException($"Error al crear la licencia: {exception.Message}");
            }
        }

        public override bool Update(License entity)
        {
            License license = this._licenseRetrieveService.Find(entity.id);

            if (license is null)
            {
                throw new SystemValidationException("La licencia no existe");
            }

            try {
                entity.updated_at = DateTime.Now;
                entity.created_at = license.created_at;
                entity.LicenseNumber = license.LicenseNumber;

                var prices = this._licensePriceRetrieveService.Where((price) => price.LicenseId == entity.id).ToList();

                this._licensePriceWriteService.Delete(prices);

                foreach (var price in entity.Prices)
                {
                    if (price.InitialQuantity >= price.FinalQuantity)
                    {
                        throw new SystemValidationException("Uno o más rangos de precios son incorrectos");
                    }

                    price.LicenseId = entity.id;

                    this._licensePriceWriteService.Create(price);
                }

                return base.Update(entity);
            }
            catch (Exception exception)
            {
                throw new SystemValidationException($"Error al crear la licencia: {exception.Message}");
            }
        }
    }
}
