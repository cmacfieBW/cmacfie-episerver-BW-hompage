using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Business.Extensions;
using StartProjectGuide.Models.Blocks;
using StartProjectGuide.Models.Media;

namespace StartProjectGuide.Business
{
    public static class BlockHelper
    {

        private static readonly IContentRepository _repo;
        private static readonly IContentLoader _contentLoader;
        private static readonly ContentAssetHelper _contentAssetHelper;

        static BlockHelper()
        {
            _repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            _contentAssetHelper = ServiceLocator.Current.GetInstance<ContentAssetHelper>();
        }

        /// <summary>
        /// Creates a new ImageBlock saved to @parent's assets folder
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static ImageBlock CreateImageBlock(ContentReference parent, string url, string title)
        {
            ImageFile imageFile = ImageDownloader.DownloadImage(parent, url, title);
            var t = title ?? "untitled";
            if (imageFile != null)
            {
                ImageBlock imgBlock =
                    parent.CreateGenericBlockForPage<ImageBlock>($"ImageBlock - {t}");
                imgBlock.ImageReference = imageFile.ContentLink;
                imgBlock.Title = t;
                return imgBlock;
            }

            return null;
        }

        /// <summary>
        /// Creates a new TextBlock, saved to @parent's assets folder
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// <param name="blockName"></param>
        /// <returns></returns>
        public static TextBlock CreateTextBlock(ContentReference parent, string text, string blockName)
        {
            if (parent.IsNullOrEmpty())
            {
                return null;
            }
            TextBlock textBlock = parent.CreateGenericBlockForPage<TextBlock>(blockName);
            textBlock.Body = new XhtmlString(text);
            return textBlock;
        }

    }
}