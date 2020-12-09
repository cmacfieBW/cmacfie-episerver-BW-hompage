using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Razor.Parser.SyntaxTree;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(DisplayName = "Colored text block", GUID = "CC23DA98-74F9-4D12-9455-00CBE2C44687", Description = "", AvailableInEditMode = false)]
    public class ColorTextBlock : BaseBlockData
    {
        [Display(
            Name = "Text Block",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual TextBlock TextBlock { get; set; }

        [Display(
            Name = "Button",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual ButtonBlock Button { get; set; }
    }
}