using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using Castle.Core.Internal;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.ServiceLocation;
using StartProjectGuide.Models.Media;

namespace StartProjectGuide.Business
{
    public static class ImageDownloaderHelper
    {
        private static readonly string[] FileExtensions = { ".jpg", ".jpeg", ".png", ".svg" };
        private static readonly IBlobFactory BlobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();
        private static readonly ContentAssetHelper ContentAssetHelper = ServiceLocator.Current.GetInstance<ContentAssetHelper>();
        private static readonly IContentRepository Repo = ServiceLocator.Current.GetInstance<IContentRepository>();

        /// <summary>
        /// Downloads an image and returns it as a blob
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Blob DownloadImageBlob(string url)
        {
            if (!url.IsNullOrEmpty() && url.StartsWith("http"))
            {
                byte[] data = null;
                using (var webClient = new WebClient())
                {
                    data = webClient.DownloadData(url);
                }

                var imageFile = new ImageFile();

                var urlSubstring = url.Split('?')[0];
                urlSubstring = url.Split('/').Last();
                var extension = urlSubstring.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";

                if (FileExtensions.Contains(extension))
                {
                    var blob = BlobFactory.CreateBlob(imageFile.BinaryDataContainer, extension);
                    using (var s = blob.OpenWrite())
                    {
                        var w = new StreamWriter(s);
                        w.BaseStream.Write(data, 0, data.Length);
                        w.Flush();
                    }

                    return blob;
                }
            }

            return null;
        }

        /// <summary>
        /// Downloads an image from the url and saves to parents assets folder
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static ImageFile DownloadImage(ContentReference parent, string url, string title)
        {
            var imageFile = Repo.GetDefault<ImageFile>(ContentAssetHelper.GetOrCreateAssetFolder(parent).ContentLink);
            imageFile.Name = $"image-{title}";
            var blob = ImageDownloaderHelper.DownloadImageBlob(url);
            if (blob != null)
            {
                imageFile.BinaryData = blob;
                Repo.Save(imageFile, SaveAction.Publish);
                return imageFile;
            }

            return null;
        }

    }
}