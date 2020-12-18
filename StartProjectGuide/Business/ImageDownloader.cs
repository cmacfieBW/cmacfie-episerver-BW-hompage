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
    public static class ImageDownloader
    {
        private static readonly string[] FileExtensions = { ".jpg", ".jpeg", ".png", ".svg" };
        private static readonly IBlobFactory BlobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();

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
    }
}