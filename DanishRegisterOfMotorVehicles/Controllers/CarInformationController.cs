using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DanishRegisterOfMotorVehicles.Api.Scraping;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DanishRegisterOfMotorVehicles.Api.Controllers
{
    public class CarInformationController : Controller
    {
        private readonly Scraper _scraper = new Scraper();

        [HttpGet("api/carinformation/{numberplate}")]
        public async Task<string> GetCarInformation(string numberplate)
        {
            return (await _scraper.LookupVehicle(numberplate)).ToJson();
        }

        [HttpGet("api/carinformationaslist/{numberplate}")]
        public async Task<string> GetCarInformationAsList(string numberplate)
        {
            return (await _scraper.LookupVehicle(numberplate, true)).ToJson();
        }
    }
}