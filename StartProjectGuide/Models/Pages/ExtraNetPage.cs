using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Pages
{
    [ContentType(DisplayName = "ExtraNetPage", GUID = "8e5459d0-2c97-41ea-afb0-db37754a4320", Description = "")]
    public class ExtraNetPage : BasePageData
    {

        [CultureSpecific]
        [Display(
            Name = "For admin",
            Description = "This part can only be seen by admins",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString ForAdmin { get; set; }

        [CultureSpecific]
        [Display(
            Name = "For users",
            Description = "This part can be seen by anyone",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString ForUsers { get; set; }

    }
}