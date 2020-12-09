using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Models.Blocks;

namespace StartProjectGuide.Models.Pages
{
    [ContentType(DisplayName = "ContactPage", GUID = "a483b698-26a6-44a6-a002-354f6bc6008e", Description = "", GroupName = Global.PageTypes.ChildPage)]
    [SiteImageUrl]

    public class ContactPage : BasePageData
    {
        [Display(Name = "Image", GroupName = SystemTabNames.Content, Order = 5)]
        public virtual ImageBlock Image { get; set; }

        [CultureSpecific]
        [Display(Name = "Header", GroupName = SystemTabNames.Content, Order = 10)]
        public virtual string Header { get; set; }

        [Display(Name = "Telepohne", GroupName = SystemTabNames.Content, Order = 10)]
        public virtual string Telephone { get; set; }

        [Display(Name = "Email", GroupName = SystemTabNames.Content, Order = 10)]
        public virtual string Email { get; set; }

        [Display(Name = "Adress", GroupName = SystemTabNames.Content, Order = 10)]
        public virtual string Adress { get; set; }

        [Display(Name = "Icon", GroupName = SystemTabNames.Content, Order = 20)]
        public virtual ImageBlock Icon { get; set; }
    }
}