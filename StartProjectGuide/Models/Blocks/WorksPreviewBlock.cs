using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(DisplayName = "WorksPreviewBlock", GUID = "5ab24fe7-95bf-45a6-ae81-760b1d4c8ab8", Description = "")]
    public class WorksPreviewBlock : BlockData
    {

        [CultureSpecific]
        [Display(
            Name = "Work preview",
            Description = "Selected works to show",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        [AllowedTypes(typeof(WorkPage))]
        public virtual IList<ContentReference> Work { get; set; }

        [Display(
            Name = "Link button",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual ButtonBlock Button { get; set; }

    }
}