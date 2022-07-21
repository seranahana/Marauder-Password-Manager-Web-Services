using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace SimplePM.WebAPI.Controllers
{
    [ApiController]
    public class ConnectivityWebController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ConnectivityWebController> _logger;

        public ConnectivityWebController(ILogger<ConnectivityWebController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("api/v1/test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public OkResult TestConnectivityAsync()
        {
            return Ok();
        }

        [HttpGet("api/v1/rsa")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public OkObjectResult GetRSAOpenKeyAsync()
        {
            return Ok(Program.PublicKey);
        }
    }
}