using InsiscoCore.Base.Data;
using InsiscoCore.Service;
using PrestaQi.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrestaQi.Service.WriteServices
{
    public class LicenseDepositsWriteService : WriteService<LicenseDeposits>
    {
        IRetrieveRepository<LicenseDeposits> _licenseDepositsRetrieveService;

        public LicenseDepositsWriteService(
            IWriteRepository<LicenseDeposits> repository,
            IRetrieveRepository<LicenseDeposits> licenseDepositsRetrieveService
        ) : base(repository) {
            this._licenseDepositsRetrieveService = licenseDepositsRetrieveService;
        }

        public override bool Create(LicenseDeposits entity)
        {

            var exist = this._licenseDepositsRetrieveService.Where(deposit => deposit.IdEF == entity.IdEF).ToList();

            entity.created_at = DateTime.Now;
            entity.updated_at = DateTime.Now;

            if (exist.Count > 0 || entity.id > 0)
            {
                entity.id = exist.First().id;

                return base.Update(entity);
            }

            return base.Create(entity);
        }

        public override bool Create(IEnumerable<LicenseDeposits> entities)
        {
            entities.ToList().ForEach(deposit =>
            {
                this.Create(deposit);
            });

            return true;
        }
    }
}
