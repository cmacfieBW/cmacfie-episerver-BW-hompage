using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using StartProjectGuide.Business.Extensions;
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Business
{
    public class ContentShortcuts
    {

        public static StartPage GetStartPage()
        {
            if (ContentReference.StartPage != null) return ContentReference.StartPage.Get<StartPage>() as StartPage;
            return new ContentReference(27).Get<StartPage>() as StartPage;
        }

        public static ContentReference GetServiceStartPage()
        {
            if (GetStartPage().ServiceStartPage != null)
                return GetStartPage().ServiceStartPage;
            return ContentReference.EmptyReference;
        }

        public static ContentReference GetWorkStartPage()
        {
            return GetStartPage().WorkStartPage ?? ContentReference.EmptyReference;
        }

        public static ContentReference GetContactPages()
        {
            return GetStartPage().ContactPage ?? ContentReference.EmptyReference;
        }

        public static ContentReference GetSearchPage()
        {
            return GetStartPage().SearchPage ?? ContentReference.EmptyReference;
        }


    }
}