using Microsoft.AspNetCore.Mvc;
using PrestaQi.Model.Dto.Output;
using System.Reflection;

namespace PrestaQi.Api.Configuration
{
    public class CustomController : ControllerBase
    {
        public ActionResult Success(string message)
        {
            return base.Ok(new ResponseResult()
            {
                Success = true,
                Message = message
            });
        }

        public ActionResult Ok(object data, string message = "")
        {
            return base.Ok(new ResponseResult()
            {
                Success = true,
                Data = data,
                Message = message
            });
        }

        public ActionResult NotFound(string message)
        {
            return base.NotFound(new ResponseResult()
            {
                Message = message
            });
        }
    }
}
