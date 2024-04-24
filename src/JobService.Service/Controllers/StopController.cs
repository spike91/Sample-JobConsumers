namespace JobService.Service.Controllers
{
    using System.Threading.Tasks;
    using MassTransit;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;


    [ApiController]
    [Route("[controller]/[action]")]
    public class StopController :
        ControllerBase
    {
        readonly ILogger<StopController> _logger;

        public StopController(ILogger<StopController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Bus([FromServices] IBusControl bus)
        {
            _logger.LogInformation("Stop bus called");

             await bus.StopAsync();

            return Ok();
        }

        [HttpPost]
        public IActionResult Application([FromServices] IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger.LogInformation("Stop application called");

            if(!hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                hostApplicationLifetime.StopApplication();

            return Ok();
        }
    }
}