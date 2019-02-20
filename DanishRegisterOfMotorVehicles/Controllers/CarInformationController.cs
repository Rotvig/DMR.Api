using System.Collections.Generic;
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
        private readonly ICache _cache;
        
        public CarInformationController(ICache cache)
        {
            _cache = cache;
        }
        
        [HttpGet("api/carinformation/{numberplate}")]
        public async Task<string> GetCarInformation(string numberplate)
        {
            var entityContainer = _cache.Get(numberplate) ?? await _scraper.LookupVehicle(numberplate);
            _cache.Add(entityContainer);

            var message = "OK";
            var success = true;
            if (entityContainer.Entities.Count == 0)
            {
                message = "Ingen køretøjer fundet";
                success = false;
            }
            return (new Request
            {
                Success = success,
                Message = message,
                Result = entityContainer
            }).ToJson();
        }
        
        [HttpGet("api/carinformation/{numberplate}/{slug}")]
        public async Task<string> GetCarInformation(string numberplate, string slug)
        {
            var entityContainer = _cache.Get(numberplate) ?? await _scraper.LookupVehicle(numberplate);
            _cache.Add(entityContainer);
            return entityContainer.Entities.FirstOrDefault(x => x.Slug == slug)?.Value.ToJson();
        }

        [HttpGet("api/carinformationaslist/{numberplate}")]
        public async Task<string> GetCarInformationAsList(string numberplate)
        {
            var entityContainer = _cache.Get(numberplate) ?? await _scraper.LookupVehicle(numberplate);
            _cache.Add(entityContainer);

            return entityContainer.Entities.ToJson();
        }
    }
}