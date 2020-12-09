using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(DisplayName = "ImageTextBlock", GUID = "655f759b-f97d-40ca-b5a8-c03fa2af0736", Description = "A block width a horizontal 50-50 split of image and text")]
    public class ImageTextBlock : BaseBlockData
    {

        [Display(Name = "Text section", GroupName = SystemTabNames.Content, Order = 10)]
        public virtual TextBlock TextBlock { get; set; }

        [Display(
            Name = "Button",
            GroupName = SystemTabNames.Content,
            Order = 20)]
        public virtual ButtonBlock Button { get; set; }

        [Display(Name = "Image", GroupName = SystemTabNames.Content, Order = 30)]
        public virtual ImageBlock ImageBlock { get; set; }

    }
}