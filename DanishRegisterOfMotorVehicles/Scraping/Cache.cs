using System;
using System.Collections.Generic;
using System.Linq;

namespace DanishRegisterOfMotorVehicles.Api.Scraping
{
    public class Cache : ICache
    {
        private const int MaxChacheSize = 20;
        private Dictionary<string, EntityContainer> _entityCache = new Dictionary<string, EntityContainer>();

        public void Add(EntityContainer entityContainer)
        {
            if (_entityCache.Count >= MaxChacheSize)
            {
                _entityCache.Remove(_entityCache.OrderBy(x => x.Value.Age).First().Key);
            }
            
            _entityCache.TryGetValue(entityContainer.LiscensePlate, out var cachedEntityContainer);

            if (cachedEntityContainer != null) 
            {
                if (cachedEntityContainer.Age + TimeSpan.FromDays(1) > DateTime.Now) return; //Less than a day old then dont change
                _entityCache.Remove(entityContainer.LiscensePlate);
                _entityCache.Add(entityContainer.LiscensePlate, entityContainer);
                return;
            }
            
            _entityCache.Add(entityContainer.LiscensePlate, entityContainer);
        }

        public EntityContainer Get(string key)
        {
            _entityCache.TryGetValue(key, out var cachedEntityContainer);
            if (cachedEntityContainer == null || cachedEntityContainer.Age + TimeSpan.FromDays(1) > DateTime.Now) //Less than a day old
                return cachedEntityContainer;
            
            _entityCache.Remove(key);
            return null;
        }
    }

    public interface ICache
    {
        void Add(EntityContainer entityContainer);
        EntityContainer Get(string key);
    }
}