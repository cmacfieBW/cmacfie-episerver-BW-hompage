using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity.Core.Common.EntitySql;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using Castle.Core.Internal;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Business.Extensions;
using StartProjectGuide.Models.Blocks;
using StartProjectGuide.Models.Media;
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Business.Jobs
{
    [ScheduledPlugIn(DisplayName = "Load Or Update Page Job", GUID = "98d3c96d-a225-435b-8d13-c77fe9eef840", Description = "Loads or updates a page from the database")]
    public class LoadOrUpdatePageJob : ScheduledJobBase
    {
        private const string Stream = "http://13.53.100.131/index.xml";
        private const string JobName = "Load or update Page job";

        private XmlReader _xmlReader;
        private bool _stopSignaled;
        private readonly IContentRepository _repo;
        private readonly ContentAssetHelper _contentAssetHelper;
        public Injected<ISiteDefinitionRepository> SiteDefinitionRepository { get; set; }

        public LoadOrUpdatePageJob()
        {
            _repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _contentAssetHelper = ServiceLocator.Current.GetInstance<ContentAssetHelper>();
            _xmlReader = XmlReader.Create(Stream);
            IsStoppable = true;
        }

        /// <summary>
        /// Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
        /// </summary>
        public override void Stop()
        {
            _stopSignaled = true;
        }

        /// <summary>
        /// Returns a new instance of a page with the page name set, or a clone of an existing one, if a page with than name already exists
        /// </summary>
        /// <param name="page"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private T GetOrCreatePageWithName<T>(string pageName, PageReference parent) where T : BasePageData
        {
            if (parent == null)
            {
                parent = ContentReference.StartPage;
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

        private string FormatOpenAndClosedElement(string textValue, string className, string type = "div")
        {
            var classString = !string.IsNullOrEmpty(className) ? $@" class=""{className}""" : null;
            return $"<{type ?? "div"}{classString}>{textValue}</{type}>";
        }

        private TextBlock CreateTextBlock(ContentReference parent, string text, string blockName)
        {
            TextBlock textBlock =
                CreateGenericBlockForPage<TextBlock>(parent, blockName);
            textBlock.Body = new XhtmlString(text);
            SaveGenericBlock(textBlock);
            return textBlock;
        }

        private ImageFile DownloadImage(ContentReference parent, string url, string title)
        {
            var imageFile = _repo.GetDefault<ImageFile>(_contentAssetHelper.GetOrCreateAssetFolder(parent).ContentLink);
            imageFile.Name = $"image-{title}";
            var blob = ImageDownloader.DownloadImageBlob(url);
            if (blob != null)
            {
                imageFile.BinaryData = blob;
                _repo.Save(imageFile, SaveAction.Publish);
                return imageFile;
            }

            return null;
        }

        private ImageBlock CreateImageBlock(ContentReference parent, string url, string title)
        {
            ImageFile imageFile = DownloadImage(parent, url, title);
            if (imageFile != null)
            {
                ImageBlock imgBlock =
                    CreateGenericBlockForPage<ImageBlock>(parent, $"ImageBlock - {title}");
                imgBlock.Url = url;
                imgBlock.Title = title;
                SaveGenericBlock(imgBlock);
                return imgBlock;
            }

            return null;
        }

        //private IList<BaseBlockData> MergeBlocksToTextBlocks(ContentReference parent, IList<BaseBlockData> blocks)
        //{
        //    IList<BaseBlockData> mergedBlocks = new List<BaseBlockData>();
        //    TextBlock currentBlock = CreateGenericBlockForPage<TextBlock>(parent,
        //        $"MergedTextBlock");
        //    foreach (var block in blocks)
        //    {
        //        if (block is TextBlock textBlock)
        //        {
        //            var header = textBlock.Header;
        //            var body = textBlock.Body;
        //            if (currentBlock.Header != null && (header != null || currentBlock.Body != null))
        //            {
        //                mergedBlocks.Add(currentBlock);
        //                SaveGenericBlock(currentBlock);
        //                currentBlock = CreateGenericBlockForPage<TextBlock>(parent,
        //                    $"MergedTextBlock");
        //            }
        //            if (currentBlock.Header == null && header != null)
        //            {
        //                currentBlock.Header = header;
        //            }

        //            if (currentBlock.Body == null && body != null)
        //            {
        //                currentBlock.Body = body;
        //            }
        //        }
        //        else
        //        {
        //            mergedBlocks.Add(block);
        //        }
        //    }
        //    if (currentBlock.Header != null)
        //    {
        //        SaveGenericBlock(currentBlock);
        //        mergedBlocks.Add(currentBlock);
        //    }

        //    return mergedBlocks;

        //}

        /// <summary>
        /// Recursively goes through the XML file and generates a block-tree.
        /// </summary>
        /// <param name="parentReference"></param>
        /// <param name="outerBlocks"></param>
        /// <param name="parentWrapper"></param>
        /// <returns>A BaseBlockData with a ContentArea with all blocks in a tree structure </returns>
        private BaseBlockData GenerateNewPageFromXMLSection(ContentReference parentReference, IList<BaseBlockData> outerBlocks, GenericContainerBlock parentWrapper)
        {
            while (_xmlReader.Read())
            {
                switch (_xmlReader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        //Save Element  

                        //Return the blocks within a section wrapper
                        if (parentWrapper != null && outerBlocks.Count > 1)
                        {
                            if (parentWrapper.MainContentArea == null)
                            {
                                parentWrapper.MainContentArea = new ContentArea();
                            }


                            foreach (var genericContainerBlock in outerBlocks)
                            {
                                ContentAreaItem item = new ContentAreaItem
                                {
                                    ContentLink = (genericContainerBlock as IContent).ContentLink
                                };
                                parentWrapper.MainContentArea.Items.Add(item);
                            }
                            SaveGenericBlock(parentWrapper);
                            return parentWrapper;
                        }
                        //Return just the single block
                        else if (outerBlocks.Any())
                        {
                            if (parentWrapper != null)
                            {
                                _repo.Delete((parentWrapper as IContent).ContentLink, true, AccessLevel.Read);
                            }

                            SaveGenericBlock(outerBlocks.First());
                            return outerBlocks.First();
                        }
                        else
                        {
                            return null;
                        }
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                        //TEXT
                        //CDATA
                        var text = _xmlReader.GetAttribute("text") ?? _xmlReader.Value;
                        var html = FormatOpenAndClosedElement(text, "text", "p");
                        TextBlock textBlock = CreateTextBlock(parentReference, html,  $"TextBlock - {text.Substring(0,Math.Min(text.Length, 20))}");
                        if (textBlock != null)
                        {
                            outerBlocks.Add(textBlock);
                        }

                        break;
                    case (XmlNodeType.Element):
                        if (_xmlReader.IsEmptyElement)
                        {
                            switch (_xmlReader.Name)
                            {
                                case "video":
                                    //TODO
                                    break;
                                case "image":
                                    var url = _xmlReader.GetAttribute("url");
                                    var title = _xmlReader.GetAttribute("title");
                                    if (!url.IsNullOrEmpty())
                                    {
                                        ImageBlock imgBlock = CreateImageBlock(parentReference, url, title);
                                        if (imgBlock != null)
                                        {
                                            outerBlocks.Add(imgBlock);
                                        }
                                    }

                                    break;
                                case "header":
                                    var headerText = _xmlReader.GetAttribute("text");
                                    var type = _xmlReader.GetAttribute("type") ?? "h1";
                                    var header = FormatOpenAndClosedElement(headerText, "header", type);
                                    TextBlock t = CreateTextBlock(parentReference, header, $"HeaderBlock - {headerText.Substring(0, Math.Min(headerText.Length, 20))}");
                                    if (t != null)
                                    {
                                        outerBlocks.Add(t);
                                    }

                                    break;
                            }
                        }
                        else if (_xmlReader.IsStartElement())
                        {
                            var background = _xmlReader.GetAttribute("background");
                            GenericContainerBlock newWrapperBlock =
                                CreateGenericBlockForPage<GenericContainerBlock>(parentReference, "Section wrapper");
                            if (!background.IsNullOrEmpty())
                            {
                                newWrapperBlock.BackgroundColor = background;
                            }
                            var block = GenerateNewPageFromXMLSection((newWrapperBlock as IContent).ContentLink,
                                new List<BaseBlockData>(), newWrapperBlock);
                            if (block != null)
                            {
                                outerBlocks.Add(block);
                            }
                        }

                        break;
                }
            }

            return parentWrapper;
        }

        private T CreateGenericBlockForPage<T>(ContentReference parentPageReference, string newBlockName,
            SaveAction saveAction = SaveAction.Publish, AccessLevel accessLevel = AccessLevel.NoAccess) where T : BaseBlockData
        {
            var assetsFolderForPage = _contentAssetHelper.GetOrCreateAssetFolder(parentPageReference);
            var blockInstance = _repo.GetDefault<T>(assetsFolderForPage.ContentLink);
            var blockForPage = blockInstance as IContent;
            blockForPage.Name = newBlockName;
            _repo.Save(blockForPage, saveAction, accessLevel);
            return blockInstance;
        }

        private T CreateOrGetExistingPage<T>() where T : BasePageData
        {
            string title = _xmlReader.GetAttribute("title");
            string longTitle = _xmlReader.GetAttribute("longtitle");
            var workLanding = ContentShortcuts.GetWorkLandingPageReference();
            var currPage = GetOrCreatePageWithName<T>(title, workLanding.ToPageReference());

            currPage.VisibleInMenu = false;
            currPage.IntroSection.Header = longTitle ?? title;
            SaveGenericPage(currPage);
            return currPage;
        }

        private void SaveGenericBlock<T>(T block) where T : BaseBlockData
        {
            _repo.Save(block as IContent, SaveAction.Publish, AccessLevel.Read);
        }

        private void SaveGenericPage<T>(T block) where T : BasePageData
        {
            _repo.Save(block as IContent, SaveAction.Publish, AccessLevel.Read);
        }

        private IList<ContentReference> GenerateRelatedPages()
        {
            IList<ContentReference> references = new List<ContentReference>();
            var serviceLandingPage = ContentShortcuts.GetServiceLandingPageReference();
            var services = serviceLandingPage.GetChildren().Select(x => x as ServicePage).ToList();
            if (!serviceLandingPage.IsNullOrEmpty())
            {
                while (_xmlReader.Read() && _xmlReader.Name != "services")
                {
                    var title = _xmlReader.GetAttribute("title");
                    if (_xmlReader.Name == "service" && title != null)
                    {
                        var service = services.Find(x => x.Name.ToLower().Contains(title.ToLower()));
                        if (service != null)
                        {
                            ContentReference reference = service.ContentLink;
                            if (reference != null)
                            {
                                references.Add(reference);
                            }
                        }
                    }
                }
            }

            return references;
        }

        private string GetDescription()
        {
            while (_xmlReader.Read() && _xmlReader.NodeType != XmlNodeType.EndElement)
            {
                var value = _xmlReader.Value;
                if (value != null)
                {
                    return value;
                }

            }
            return null;
        }

        private string GetTagLine()
        {
            while (_xmlReader.Read() && _xmlReader.NodeType != XmlNodeType.EndElement)
            {
                var value = _xmlReader.Value;
                if (value != null)
                {
                    return value;
                }

            }

            return null;
        }

        private T AddServices<T>(T currPage) where T : StandardPage
        {
            var references = GenerateRelatedPages();
            if (currPage.RelatedPages.IsNullOrEmpty())
            {
                currPage.RelatedPages = new List<ContentReference>();
            }

            currPage.RelatedPages = references;
            return currPage;
        }

        private T AddSections<T>(T currPage) where T : StandardPage
        {
            GenericContainerBlock sectionsWrapper = CreateGenericBlockForPage<GenericContainerBlock>(currPage.ContentLink, "sectionsWrapper");
            GenericContainerBlock outMostWrapper = GenerateNewPageFromXMLSection(currPage.ContentLink, new List<BaseBlockData>(), sectionsWrapper) as GenericContainerBlock;
            ContentArea area = outMostWrapper.MainContentArea;
            currPage.MainContentArea = area;

            //Save
            return currPage;
        }

        /// <summary>
        /// Creates or updates an existing with page with data from a hardcoded XML-file.
        /// </summary>
        /// <returns></returns>
        public override string Execute()
        {
            OnStatusChanged(String.Format("Starting execution of {0}", this.GetType()));
            SiteDefinition.Current = SiteDefinitionRepository.Service.List().First();

            while (_xmlReader.Read())
            {
                if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.Name == "case")
                {
                    var longtitle = _xmlReader.GetAttribute("longtitle");
                    var id = _xmlReader.GetAttribute("id");
                    var imageUrl = _xmlReader.GetAttribute("image");
                    OnStatusChanged(String.Format("Executing job: {0}", id));

                    WorkPage currPage = CreateOrGetExistingPage<WorkPage>();
                    _repo.DeleteChildren(currPage.ContentLink, false, AccessLevel.Read);


                    if (!longtitle.IsNullOrEmpty())
                    {
                        currPage.IntroSection.Header = longtitle;
                    }

                    if (imageUrl != null)
                    {
                        var imgBlock = CreateImageBlock(currPage.ContentLink, imageUrl, $"{id}-header-image");
                        if (imgBlock != null)
                        {
                            currPage.Image = imgBlock;
                        }
                    }

                    currPage.MainContentArea = new ContentArea();
                    while (_xmlReader.Read() && _xmlReader.Name != "case")
                    {
                        switch (_xmlReader.Name)
                        {
                            case "tagline":
                                var tagline = GetTagLine();
                                if (!tagline.IsNullOrEmpty())
                                {
                                    currPage.TeaserDescription = new XhtmlString(tagline);
                                }
                                break;
                            case "description":
                                var description = GetDescription();
                                if (!description.IsNullOrEmpty())
                                {
                                    currPage.IntroSection.Preamble = new XhtmlString(description);
                                }
                                break;
                            case "sections":
                                currPage = AddSections(currPage);
                                break;
                            case "services":
                                currPage = AddServices(currPage);
                                break;
                        }
                    }
                    SaveGenericPage(currPage);
                }
            }

            if (_stopSignaled)
            {
                return $"{JobName} was stopped manually";
            }

            return $"Success. {JobName} finished.";
        }
    }
}