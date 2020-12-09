using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace StartProjectGuide.Models
{
    public class SiteImageUrl : ImageUrlAttribute
    {
        public SiteImageUrl() : base("~/Static/images/no-image.png")
        {

        }
    }
}