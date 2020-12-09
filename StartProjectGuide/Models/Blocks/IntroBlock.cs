using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(DisplayName = "Intro block", GUID = "6a922c9f-6f52-44cf-8f4c-b7329dad1530", Description = "", AvailableInEditMode = false)]
    public class IntroBlock : BaseBlockData
    {

        [CultureSpecific]
        [Display(
            Name = "Header",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string Header { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Body Text",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString Preamble { get; set; }

        [Display(Name = "Button", Description = "Optional link button", GroupName = SystemTabNames.Content, Order = 2)]
        public virtual ButtonBlock Button { get; set; }

    }
}