using System.Collections.Generic;

namespace DanishRegisterOfMotorVehicles.Api.Scraping
{
    public class Request
    {
        public bool Success { get; set; }
        public int Count => Result == null ? 0 : Result.Count;
        public string Token { get; set; }
        public string Message { get; set; }
        public List<Entity> Result { get; set; }
    }

    public class Entity
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}