using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.SpecializedProperties;
using Sgml;

namespace StartProjectGuide
{
    public class Global : EPiServer.Global
    {

        [GroupDefinitions()]
        public static class GroupNames
        {
            [Display(Name = "Default", Order = 10)]
            public const string Default = "Default";
            [Display(Name = "Teaser ", Order = 15)]
            public const string Teaser = "Teaser";

            [Display(Name = "Meta Data", Order = 20)]
            public const string MetaData = "Meta Data";

            [Display(Name = "Site Settings", Order = 30)]
            public const string SiteSettings = "Site Settings";

            [Display(Name = "Social media", Order = 40)]
            public const string SocialMedia = "Social Media";


        }


        public static class PageTypes
        {
            [Display(GroupName = "Landing Page", Description = "Top level page type", Order = 10)]
            public const string LandingPage = "LandingPage";
            [Display(GroupName = "Child Page", Description = "Non-top level type of page", Order = 20)]
            public const string ChildPage = "ChildPage";
            [Display(GroupName = "Start Page", Description = "Special type of page", Order = 100)]
            public const string SpecialPage = "SpecialPage";
            [Display(GroupName = "Other Page", Description = "Other type of page", Order = 30)]
            public const string OtherPage = "OtherPage";
        }

        public class ContentAreaTags
        {
            public readonly static string BlockWidthFull = "span12";
            public readonly static string BlockWidthTwoThirds = "span8";
            public readonly static string BlockWidthHalf = "span6";
            public readonly static string BlockWidthOneThird = "span4";
            public readonly static string BlockWidthQuarter = "span3";

            public static List<ISelectItem> GetListOfWidths
            {
                get
                {
                    List<ISelectItem> Items = new List<ISelectItem>();
                    Items.Add((new SelectItem() { Text = "Full width", Value = BlockWidthFull }));
                    Items.Add(new SelectItem() { Text = "2/3 width", Value = BlockWidthTwoThirds });
                    Items.Add(new SelectItem() { Text = "Half width", Value = BlockWidthHalf });
                    Items.Add(new SelectItem() { Text = "1/3 width", Value = BlockWidthOneThird });
                    Items.Add(new SelectItem() { Text = "Quarter width", Value = BlockWidthQuarter });
                    return Items;

                }
            }
        }
    }
}
