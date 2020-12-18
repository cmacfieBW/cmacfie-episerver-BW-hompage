using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Business.Jobs
{
    public static class IContentRepositoryExtensions
    {
        /// <summary>
        /// Saves and publishes content to a repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repo"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public static ContentReference SaveAndPublish<T>(this IContentRepository repo, T content)
        {
            return repo.Save(content as IContent, SaveAction.Publish, AccessLevel.Read);
        }
    }
}