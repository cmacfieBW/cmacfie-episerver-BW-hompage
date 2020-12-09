using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Filters;
using EPiServer.Framework.Web;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Business.Extensions
{

    public static class ContentExtensions
    {
        internal static UrlResolver UrlResolver;

        static ContentExtensions()
        {
            UrlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
        }

        /// <summary>
        ///     Shorthand for DataFactory.Instance.Get
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentLink"></param>
        /// <returns></returns>
        public static TContent Get<TContent>(this ContentReference contentLink) where TContent : IContent
        {
            return DataFactory.Instance.Get<TContent>(contentLink);
        }

        /// <summary>
        ///     Indicates whether the specified content reference is null or an EmptyReference.
        /// </summary>
        /// <param name="contentReference">Content reference to test.</param>
        /// <returns>true if content reference is null or EmptyReference else false</returns>
        public static bool IsNullOrEmpty(this ContentReference contentReference)
        {
            return ContentReference.IsNullOrEmpty(contentReference);
        }

        /// <summary>
        ///     Returns all the childpages of the current page
        /// </summary>
        /// <param name="contentReference"></param>
        /// <returns></returns>
        public static IEnumerable<PageData> GetChildren(this ContentReference contentReference)
        {
            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            try
            {
                return
                    repo.GetChildren<PageData>(contentReference);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Returns all the childpages of the current page
        /// </summary>
        /// <param name="contentReference"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetChildren<T>(this ContentReference contentReference) where T : IContent
        {
            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            try
            {
                return
                    repo.GetChildren<T>(contentReference);
            }
            catch
            {
                return null;
            }
        }
    }
}