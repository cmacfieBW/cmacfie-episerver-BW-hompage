using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Business.Extensions
{
    public static class PageReferenceExtensions
    {

        internal static IContentRepository _repo;
        internal static ContentAssetHelper _contentAssetHelper;

        static PageReferenceExtensions()
        {

            _repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _contentAssetHelper = ServiceLocator.Current.GetInstance<ContentAssetHelper>();
        }

        /// <summary>
        /// Returns a new instance of a page with the page name set, or a clone of an existing one, if a page with than name already exists
        /// </summary>
        /// <param name="page"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T GetOrCreatePageWithName<T>(this PageReference parent, string pageName) where T : BasePageData
        {
            if (parent == null)
            {
                return null;
            }
            var page = parent.GetChildWithName(pageName);
            if (page != null)
            {
                var clone = (page.CreateWritableClone() as T);
                var assetsFolderForPage = _contentAssetHelper.GetOrCreateAssetFolder(clone.ContentLink);
                var children = _repo.GetChildren<IContent>(assetsFolderForPage.ContentLink);
                foreach (var child in children)
                {
                    _repo.Delete(child.ContentLink, true, AccessLevel.Read);
                }

                return clone;
            }
            var newPage = _repo.GetDefault<T>(parent);
            newPage.PageName = pageName;
            return newPage;
        }

    }
}