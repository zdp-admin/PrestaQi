using InsiscoCore.Base.Data;
using InsiscoCore.Service;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using InsiscoCore.Base.Service;

namespace PrestaQi.Service.RetrieveServices
{
    public class LicenseRetrieveService : RetrieveService<License>
    {
        IRetrieveService<LicensePriceRange> _pricesRetrieveService;
        public LicenseRetrieveService(
            IRetrieveRepository<License> repository,
            IRetrieveService<LicensePriceRange> pricesRetrieveService
        ) : base(repository) {
            this._pricesRetrieveService = pricesRetrieveService;
        }

        public ResponseWithPagination RetrieveResult(LicenseByFilter licenseByFilter)
        {
            List<License> list = this._Repository.Where(license => license.id > 0).ToList();
            int totalRecord = list.Count;

            if (licenseByFilter.Page <= 0)
            {
                licenseByFilter.Page = 1;
            }

            if (licenseByFilter.NumRecord <= 0)
            {
                licenseByFilter.NumRecord = 50;
            }

            if (licenseByFilter.Filter?.Length > 0)
            {
                var stringProperties = typeof(License).GetProperties().Where(prop => prop.PropertyType == licenseByFilter.Filter.GetType());

                totalRecord = list.Where(license => stringProperties.Any(prop => prop.GetValue(license, null) != null && prop.GetValue(license, null).ToString().ToLower().Contains(licenseByFilter.Filter.ToLower()))).ToList().Count;

                list = list.Where(license => stringProperties.Any(prop => prop.GetValue(license, null) != null && prop.GetValue(license, null).ToString().ToLower().Contains(licenseByFilter.Filter.ToLower()))).Skip((licenseByFilter.Page - 1) * licenseByFilter.NumRecord).Take(licenseByFilter.NumRecord).ToList();
            } else
            {
                list = list.Skip((licenseByFilter.Page - 1) * licenseByFilter.NumRecord).Take(licenseByFilter.NumRecord).ToList();
            }

            foreach(var license in list)
            {
                license.Prices = this._pricesRetrieveService.Where(price => price.LicenseId == license.id).ToList();
            }

            return new ResponseWithPagination() { data = list, TotalRecord = totalRecord };
        }
    }
}
