using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Business.DynamicDataStore;
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Models.ViewModels
{
    [ContentType(DisplayName = "ExtraNetPageViewModel", GUID = "eb1afaa8-3d10-472d-8ca3-04495275aed2", Description = "")]
    public class ExtraNetPageViewModel : PageViewModel<ExtraNetPage>
    {
        public ExtraNetPageViewModel(ExtraNetPage currentPage, WeatherDds.Weather weather)
            : base(currentPage)
        {
            Weather = weather;
        }

        public WeatherDds.Weather Weather { get; private set; }
    }
}