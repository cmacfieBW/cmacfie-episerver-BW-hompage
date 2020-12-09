using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Models.Blocks;

namespace StartProjectGuide.Models.Pages
{
    [ContentType(DisplayName = "Service landing page", GUID = "ea5410ce-697e-46b5-813a-5024bbec05f3", Description = "", GroupName = Global.PageTypes.LandingPage)]
    [AvailableContentTypes(
        Availability.Specific,
        Include = new[] { typeof(ServicePage) })]
    [SiteImageUrl]
    public class ServiceLandingPage : BasePageData
    {

        [Display(
            Name = "Main content",
            GroupName = SystemTabNames.Content,
            Order = 20)]
        [AllowedTypes(typeof (WorksPreviewBlock))]
        public virtual ContentArea MainContentArea { get; set; }

    }
}