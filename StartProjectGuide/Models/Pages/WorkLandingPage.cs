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
    [ContentType(DisplayName = "Work landing page", GUID = "da02b6e3-ee09-49de-a529-23378899da87", Description = "", GroupName = Global.PageTypes.LandingPage)]
    [AvailableContentTypes(
        Availability.Specific,
        Include = new[] { typeof(WorkPage) })]
    [SiteImageUrl]
    public class WorkLandingPage : BasePageData
    {
    }
}