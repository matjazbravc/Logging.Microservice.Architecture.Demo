using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace ServiceB.OpenApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly string _service_name;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _service_name = Assembly.GetExecutingAssembly().GetName().Name;
            _logger = logger;
        }

        [HttpGet("badcode")]
        public string BadCode()
        {
            var msg = $"{_service_name} -> Some bad code was executed!";
            throw new Exception(msg);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var msg = $"{_service_name} -> Value";
            _logger.LogInformation(msg);
            return Ok(msg);
        }

        [HttpGet("healthcheck")]
        public IActionResult Healthcheck()
        {
            var msg = $"{_service_name} is healthy";
            _logger.LogInformation(msg);
            return Ok(msg);
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var msg = $"{_service_name}, running on {Request.Host}";
            _logger.LogInformation(msg);
            return Ok(msg);
        }
    }
}
