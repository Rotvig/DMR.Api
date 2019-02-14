using System;
using System.Threading.Tasks;
using Dmr;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DanishRegisterOfMotorVehicles.Api.Controllers
{
    public class CarInformationController : Controller
    {
        [HttpGet("api/carinformation/{numberplate}")]
        public async Task<string> GetCarInformation(string numberplate)
        {
            var model = Dmr.Scraper.LookupVehicle(numberplate);
            var token = model.Token;
            return model.ToJson();
        }
    }
}
