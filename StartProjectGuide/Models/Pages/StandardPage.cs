using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Pages
{
    [ContentType(DisplayName = "Standard Page", GUID = "b110e7ba-a846-4215-b0ae-6ecd89f55e13", Description = "", GroupName = Global.PageTypes.OtherPage )]
    [SiteImageUrl]
    public class StandardPage : BasePageData
    {

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 320)]
        public virtual ContentArea MainContentArea { get; set; }

    }
}