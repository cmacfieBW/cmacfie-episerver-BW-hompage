using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace StartProjectGuide.Business.DynamicDataStore
{
    public class WeatherDds
    {

        public void CreateWeatherStore(Weather weather)
        {
            var store = DynamicDataStoreFactory.Instance.CreateStore(typeof(Weather));
            store.Save(weather);
        }

        public static IEnumerable<Weather> GetWeatherEntries(ContentReference weatherPageReference)
        {
            var store = DynamicDataStoreFactory.Instance.CreateStore(typeof(Weather));
            return store.Items<Weather>().Where(x => x.RelatedReviewPage == weatherPageReference);
        }

        public void RemoveWeather(Weather weather)
        {
            var store = DynamicDataStoreFactory.Instance.CreateStore(typeof(Weather));
            store.Delete(weather.Id);
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