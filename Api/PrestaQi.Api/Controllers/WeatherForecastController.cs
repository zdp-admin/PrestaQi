using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PrestaQi.Model.Dto;
using PrestaQi.Model.Dto.Output;

namespace PrestaQi.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            byte[] file = System.IO.File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Key/devKey.p12"));
            Service.Tools.CryptoHandler crypto = new Service.Tools.CryptoHandler();
            crypto.password = "prestaqi2020*";
            string sign = crypto.signForConstaOrdenEnviadasRecibidas("001_QI", new DateTime(2021, 7, 12), file);

            OrdenesEnviadasRecibidas ordenes = new OrdenesEnviadasRecibidas();
            ordenes.firma = sign;
            ordenes.empresa = "001_QI";
            ordenes.estado = "R";

            return Ok(sign);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ordenes);

            var request = (HttpWebRequest)WebRequest.Create("https://demo.stpmex.com:7024/speiws/rest/ordenPago/consOrdenesFech");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            using (var streamWrite = new StreamWriter(request.GetRequestStream()))
            {
                streamWrite.Write(json);
                streamWrite.Flush();
                streamWrite.Close();
            }

            using (WebResponse response = request.GetResponse())
            {
                using(Stream strReader = response.GetResponseStream())
                {
                    if (strReader == null) return null;

                    using(StreamReader objReader = new StreamReader(strReader))
                    {
                        string responseBody = objReader.ReadToEnd();
                        var responseObject = JsonConvert.DeserializeObject<ResponseAccountBalance>(responseBody);

                        Console.WriteLine(responseObject);
                    }
                }
            }

            return Ok(sign);
        }
    }
}
