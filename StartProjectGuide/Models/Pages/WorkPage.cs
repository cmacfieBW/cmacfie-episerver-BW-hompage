using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Models.Blocks;

namespace StartProjectGuide.Models.Pages
{
    [ContentType(DisplayName = "Work Page", GUID = "ab2b6e28-0d00-438e-a7fd-bec8721123c6", Description = "", GroupName = Global.PageTypes.ChildPage)]
    [SiteImageUrl]
    public class WorkPage : StandardPage
    {

        [Display(
            Name = "Image",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual ImageBlock Image { get; set; }
        /** Teaser */
        [Display(Name = "Description", Description = "Descriptive text", GroupName = Global.GroupNames.Teaser, Order = 1)]
        public virtual XhtmlString TeaserDescription { get; set; }

    }
}