using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace PrestaQi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        public TestController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IActionResult Get()
        {
            return Ok(Configuration["environment"]);
        }
    }
}