using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Providers.Entities;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace StartProjectGuide.Business.DynamicDataStore
{
    public class WeatherDds
    {

        private static EPiServer.Data.Dynamic.DynamicDataStore Store =>
            DynamicDataStoreFactory.Instance.GetStore("Weather")
            ?? DynamicDataStoreFactory.Instance.CreateStore(
                "Weather",
                typeof(Weather));

        /// <summary>
        /// Returns all weather entries from the store
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Weather> GetWeatherEntries()
        {
            return LoadStore().Items<Weather>();
        }

        /// <summary>
        /// Removes a weather entry from the store
        /// </summary>
        /// <param name="id"></param>
        public static void RemoveWeather(Identity id)
        {
            LoadStore().Delete(id);
        }

        /// <summary>
        /// Adds a weather entry to the store
        /// </summary>
        /// <param name="w"></param>
        public static void AddWeather(Weather w)
        {
            LoadStore().Save(w);
        }

        /// <summary>
        /// Gets a specific weather entry from the story
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Weather GetWeatherEntry(Identity id)
        {
            return LoadStore().Load<Weather>(id);
        }

        /// <summary>
        /// Returns the store
        /// </summary>
        /// <returns></returns>
        public static EPiServer.Data.Dynamic.DynamicDataStore LoadStore()
        {
            return Store;
        }

        public class Weather : IDynamicData
        {
            public DateTime TimeStamp { get; set; }
            public string WeatherDescription { get; set; }
            public string WeatherComment { get; set; }
            public ContentReference RelatedReviewPage { get; set; }
            public Identity Id { get; set; }
        }
    }
}