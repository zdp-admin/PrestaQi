using InsiscoCore.Base.Service;
using Microsoft.AspNetCore.Mvc;
using PrestaQi.Api.Configuration;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovementController : CustomController
    {
        IRetrieveService<Advance> _advanceRetrieveService;
        IRetrieveService<Accredited> _accreditedRetrieveService;
        IRetrieveService<SpeiResponse> _speiResponseRetrieceService;

        public MovementController(
            IRetrieveService<Advance> advanceRetrieveService,
            IRetrieveService<Accredited> accreditedRetrieveService,
            IRetrieveService<SpeiResponse> speiResponseRetrieveService
        )
        {
            this._advanceRetrieveService = advanceRetrieveService;
            this._accreditedRetrieveService = accreditedRetrieveService;
            this._speiResponseRetrieceService = speiResponseRetrieveService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] MovementByFilter movementByFilter)
        {
            if (movementByFilter.Page <= 0)
            {
                movementByFilter.Page = 1;
            }

            if (movementByFilter.NumRecord <= 0)
            {
                movementByFilter.NumRecord = 50;
            }

            if (movementByFilter.Year <= 0)
            {
                movementByFilter.Year = DateTime.Now.Year;
            }

            if (movementByFilter.Month <= 0)
            {
                movementByFilter.Month = DateTime.Now.Month;
            }

            List<int> accrediteds = this._accreditedRetrieveService.Where(accredited => accredited.License_Id == movementByFilter.LicenseId).Select(accredited => accredited.id).ToList();

            List<Advance> advances = this._advanceRetrieveService.Where(advance => accrediteds.Contains(advance.Accredited_Id) && advance.created_at.Year == movementByFilter.Year && advance.created_at.Month == movementByFilter.Month).ToList();
            int countRows = advances.Count;

            advances = advances.Skip((movementByFilter.Page - 1) * movementByFilter.NumRecord).Take(movementByFilter.NumRecord).ToList();

            advances.ForEach((advance) =>
            {
                advance.accredited = this._accreditedRetrieveService.Where((accredited) => accredited.id == advance.Accredited_Id).FirstOrDefault();
                advance.speiResponse = this._speiResponseRetrieceService.Where((spei) => spei.advance_id == advance.id).FirstOrDefault();
            });

            return Ok(new ResponseWithPagination {
                data = advances,
                TotalRecord = countRows
            });
        }
    }
}
