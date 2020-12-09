using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(DisplayName = "TextBlock", GUID = "7f9ac117-cb2e-456d-aef9-15f754c71540", Description = "")]
    public class TextBlock : BaseBlockData
    {
        [CultureSpecific]
        [Display(
            Name = "Header",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string Header { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Body text",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString Body { get; set; }



    }
}