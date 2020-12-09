using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Models.Blocks;

namespace StartProjectGuide.Business.BaseClasses
{
    /// <summary>
    ///     Base class for all page types
    /// </summary>
    public abstract class BasePageData : PageData
    {

        [Display(Name = "Intro section", GroupName = SystemTabNames.Content, Order = 10)]
        public virtual IntroBlock IntroSection { get; set; }

        [Display(GroupName = Global.GroupNames.MetaData, Order = 100)]
        [CultureSpecific]
        public virtual string MetaTitle
        {
            get
            {
                var metaTitle = this.GetPropertyValue(p => p.MetaTitle);
                return !string.IsNullOrWhiteSpace(metaTitle) ? metaTitle : PageName;
            }
            set { this.SetPropertyValue(p => p.MetaTitle, value); }
        }

        [Display(GroupName = Global.GroupNames.MetaData, Order = 300)]
        public virtual string MetaDescription { get; set; }

    }
}