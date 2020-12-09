using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Pages
{
    [ContentType(DisplayName = "Generic landing page", GUID = "ceae0817-7c7d-43eb-a9f5-8ccf83939ce4", Description = "", GroupName = Global.PageTypes.LandingPage)]
    [SiteImageUrl]
    public class LandingPage : BasePageData
    {

        [Display(Name = "Content Area", Description = "Main Content Area", GroupName = SystemTabNames.Content, Order = 2)]
        public virtual ContentArea MainContentArea { get; set; }
    }
}