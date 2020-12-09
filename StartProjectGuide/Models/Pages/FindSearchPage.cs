using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Models.Pages
{
    [ContentType(DisplayName = "Search Page", GUID = "aea9fee8-326a-412e-9ab7-2e36bde3ab5b", Description = "")]
    public class FindSearchPage : BasePageData
    {

        [CultureSpecific]
        [Display(
            Name = "Results per page",
            Description = "Defines the number of results per page",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual int ResultsPerPage { get; set; }


        [Display(
            Name = "Page index",
            GroupName = SystemTabNames.Content,
            Order = 1), ReadOnly(true)]
        public virtual int PageIndex { get; set; }

    }
}