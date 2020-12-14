using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.Activities;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Business.DynamicDataStore;
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Models.ViewModels
{
    [ContentType(DisplayName = "ExtraNetPageViewModel", GUID = "eb1afaa8-3d10-472d-8ca3-04495275aed2", Description = "")]
    public class ExtraNetPageViewModel : PageViewModel<ExtraNetPage>
    {

        public ExtraNetPageViewModel(ExtraNetPage currentPage) : base(currentPage)
        {
        }
    }
}