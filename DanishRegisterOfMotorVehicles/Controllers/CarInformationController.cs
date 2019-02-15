using System;
using System.Threading.Tasks;
using DanishRegisterOfMotorVehicles.Api.Scraper;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DanishRegisterOfMotorVehicles.Api.Controllers
{
    
    public class CarInformationController : Controller
    {
        private Scraper.Scraper _scraper;

        public CarInformationController()
        {
            _scraper = new Scraper.Scraper();
        }
        [HttpGet("api/carinformation/{numberplate}")]
        public async Task<string> GetCarInformation(string numberplate)
        {
            var model = _scraper.LookupVehicle(numberplate);
            var token = model.Token;
            return model.ToJson();
        }
    }
}
