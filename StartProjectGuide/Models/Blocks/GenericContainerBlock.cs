using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(DisplayName = "GenericContainerBlock", GUID = "59218936-0062-4f3f-bd2e-fe627e078e93", Description = "", AvailableInEditMode = false)]
    public class GenericContainerBlock : BaseBlockData
    {

        [CultureSpecific]
        [Display(
            Name = "Content Area",
            Description = "Main Content Area",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual ContentArea MainContentArea { get; set; }

        public virtual string BackgroundColor { get; set; }

    }
}