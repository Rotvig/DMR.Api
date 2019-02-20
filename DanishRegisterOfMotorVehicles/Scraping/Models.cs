using System;
using System.Collections.Generic;

namespace DanishRegisterOfMotorVehicles.Api.Scraping
{
    public class Request
    {
        public bool Success { get; set; }
        public int Count => Result == null ? 0 : Result.Entities.Count;
        public string Message { get; set; }
        public EntityContainer Result { get; set; }
    }

    public class Entity
    {
        public string FieldName { get; set; }
        public string Value { get; set; }
        public string Slug { get; set; }

    }
    
    public class EntityContainer
    {
        public List<Entity> Entities { get; set; }
        public string LiscensePlate { get; set; }
        public DateTime Age { get; set; }

    }
}