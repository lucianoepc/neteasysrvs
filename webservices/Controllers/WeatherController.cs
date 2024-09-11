using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace PoC.EasySrvs.WebServices.Controllers
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherController : ControllerBase
    {
        private static readonly string[] Summaries = new[] { "Freezing1", "Bracing1", "Chilly1", "Cool1", "Mild1", "Warm1", "Balmy1", "Hot1", "Sweltering1", "Scorching1" };
        private static readonly string[] Locations = new[] { "Cocina1", "Lavadero1", "Cuarto1", "Sala1", "Comedor1", "Dormitorio1", "SalaEspera1" };

        private readonly ILogger<WeatherController> _logger;
        private List<Weather> mData;
        private int n;

        public WeatherController(ILogger<WeatherController> logger)
        {
            _logger = logger;

            n = 0;
            mData = new List<Weather>();
            Weather item;            
            for (int i=1; i<=5; i++)
            {
                n++;
                item = new Weather
                {
                    Id = n.ToString(),
                    Location = Locations[Random.Shared.Next(Locations.Length)],
                    Date = DateTime.Now.AddDays(i),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };
                mData.Add(item);                
            }

            if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("Ejemplo de Debug 1");
            //if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("Ejemplo de Error 1");

        }

        [HttpGet()]
        public IEnumerable<Weather> GetAll()
        {
			if (_logger.IsEnabled(LogLevel.Error))
            {
                StringBuilder sb= new StringBuilder();
                int n = Request.Headers.Count;
                sb.AppendLine("Analizando la HTTP Header:");
                sb.AppendLine("\tHTTP Header: " + n.ToString("00") + " header http encontrados");
                foreach (var header in Request.Headers)
                {
                    sb.AppendLine("\t\t[" + header.Key + "] : " + header.Value);
                }

                sb.AppendLine("\tQueryString: " + Request.QueryString);
                _logger.LogError(sb.ToString());
                
            }
            return mData;
        }

        [HttpGet("{pId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Weather> GetOne(string pId)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                StringBuilder sb = new StringBuilder();
                int n = Request.Headers.Count;
                sb.AppendLine("Analizando la HTTP Header:");
                sb.AppendLine("\tHTTP Header: " + n.ToString("00") + " header http encontrados");
                foreach (var header in Request.Headers)
                {
                    sb.AppendLine("\t\t[" + header.Key + "] : " + header.Value);
                }

                sb.AppendLine("\tQueryString: " + Request.QueryString);
                _logger.LogError(sb.ToString());

            }
            Weather? item= mData.Find(item => item.Id == pId);

            if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("Se ha encontrado un registro");
            
            if (item == null) return NotFound();
            return item;
        }
    }
}
