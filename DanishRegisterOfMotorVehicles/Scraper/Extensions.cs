using Newtonsoft.Json;

namespace DanishRegisterOfMotorVehicles.Api.Scraper
{
    public static class RequestExtensions
    {
        public static string ToJson(this Request model)
        {
            var formatting = Formatting.Indented;
            var settings = new JsonSerializerSettings();
            return JsonConvert.SerializeObject(model, formatting, settings);
        }
    }
}
