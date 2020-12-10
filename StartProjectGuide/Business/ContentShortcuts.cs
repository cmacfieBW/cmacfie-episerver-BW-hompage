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
        /// <summary>
        /// Retrieves the Startpage of the application
        /// </summary>
        /// <returns>An object of StartPage class </returns>
        public static StartPage GetStartPage()
        {
            if (ContentReference.StartPage != null) return ContentReference.StartPage.Get<StartPage>() as StartPage;
            return new ContentReference(27).Get<StartPage>() as StartPage;
        }

        /// <summary>
        /// Gets a reference to the Service Landing page, if it exists, otherwise an empty reference
        /// </summary>
        /// <returns>A content reference to the ServiceLandingPage, or an empty reference</returns>
        public static ContentReference GetServiceLandingPageReference()
        {
            return GetStartPage().ServiceLandingPage ?? ContentReference.EmptyReference;
        }

        /// <summary>
        /// Gets a reference to the Work Landing page, if it exists, otherwise an empty reference
        /// </summary>
        /// <returns>A content reference to the WorkLandingPage, or an empty reference</returns>
        public static ContentReference GetWorkLandingPageReference()
        {
            return GetStartPage().WorkLandingPage ?? ContentReference.EmptyReference;
        }

        /// <summary>
        /// Gets a reference to the Contact landing page, if it exists, otherwise an empty reference
        /// </summary>
        /// <returns>A content reference to the ContactLandingPage, or an empty reference</returns>
        public static ContentReference GetContactLandingPageReference()
        {
            return GetStartPage().ContactLandingPage ?? ContentReference.EmptyReference;
        }

        /// <summary>
        /// Gets a reference to the Search landing page, if it exists, otherwise an empty reference
        /// </summary>
        /// <returns>A content reference to the SearchLandingPage, or an empty reference</returns>
        public static ContentReference GetSearchLandingPageReference()
        {
            return GetStartPage().SearchLandingPage ?? ContentReference.EmptyReference;
        }


    }
}