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
using System.Web.Services.Description;
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
        private XmlReaderSettings _xmlReaderSettings;
        private const string Stream = "http://13.53.100.131/index.xml";
        //private const string Stream = "C:/Users/bombayworks/source/repos/EpiServerStartProject/StartProjectGuide/Business/database/db.xml";
        private const string JobName = "Load or update Page job";

        private XmlReader _xmlReader;
        private bool _stopSignaled;
        private ContentReference rootReference;
        private readonly IContentRepository _repo;
        private readonly IContentLoader _contentLoader;
        private readonly ContentAssetHelper _contentAssetHelper;
        public Injected<ISiteDefinitionRepository> SiteDefinitionRepository { get; set; }

        public LoadOrUpdatePageJob()
        {
            _xmlReaderSettings = new XmlReaderSettings();
            _xmlReaderSettings.IgnoreComments = true;
            _xmlReaderSettings.IgnoreProcessingInstructions = true;
            _xmlReaderSettings.IgnoreWhitespace = true;
            _repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            _contentAssetHelper = ServiceLocator.Current.GetInstance<ContentAssetHelper>();
            _xmlReader = XmlReader.Create(Stream, _xmlReaderSettings);
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
            var t = title ?? "untitled";
            if (imageFile != null)
            {
                ImageBlock imgBlock =
                    CreateGenericBlockForPage<ImageBlock>(parent, $"ImageBlock - {t}");
                imgBlock.Url = UrlResolver.Current.GetUrl(imageFile.ContentLink);
                imgBlock.Title = t;
                return imgBlock;
            }

            return null;
        }


        /// <summary>
        /// Recursively goes through the XML file and generates a block-tree.
        /// </summary>
        /// <param name="parentReference"></param>
        /// <param name="currentBlocksList"></param>
        /// <param name="parentWrapper"></param>
        /// <returns>A BaseBlockData with a ContentArea with all blocks in a tree structure </returns>
        private BaseBlockData GenerateNewPageFromXMLSection(ContentReference parentReference, IList<BaseBlockData> currentBlocksList, GenericContainerBlock parentWrapper)
        {
            while (_xmlReader.ReadAndCount())
            {
                switch (_xmlReader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        //Save Element  

                        //Return the blocks within a section wrapper
                        if (currentBlocksList.Count > 1)
                        {
                            if (parentWrapper.MainContentArea == null)
                            {
                                parentWrapper.MainContentArea = new ContentArea();
                            }

                            foreach (var genericContainerBlock in currentBlocksList)
                            {
                                _repo.SaveAndPublish(genericContainerBlock);
                                ContentAreaItem item = new ContentAreaItem
                                {
                                    ContentLink = (genericContainerBlock as IContent).ContentLink
                                };
                                parentWrapper.MainContentArea.Items.Add(item);
                            }

                            if (parentReference != rootReference)
                            {
                                _repo.SaveAndPublish(parentWrapper);
                            }

                            return parentWrapper;
                        }
                        //Return just the single block
                        else if (currentBlocksList.Any())
                        {
                            var newBlock = currentBlocksList.First();
                            _repo.SaveAndPublish(newBlock);
                            return newBlock;
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
                        TextBlock textBlock = CreateTextBlock(parentReference, html, $"TextBlock - {text.Substring(0, Math.Min(text.Length, 20))}");
                        if (textBlock != null)
                        {
                            currentBlocksList.Add(textBlock);
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
                                            currentBlocksList.Add(imgBlock);
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
                                        currentBlocksList.Add(t);
                                    }

                                    break;
                            }
                        }
                        else if (_xmlReader.IsStartElement())
                        {
                            var background = _xmlReader.GetAttribute("background");
                            GenericContainerBlock newWrapperBlock =
                                CreateGenericBlockForPage<GenericContainerBlock>(parentReference, $"Container- {_xmlReader.Name.CapitalizeFirstLetter()}");
                            var sectionReference = ContentReference.EmptyReference;
                            if (_xmlReader.SectionHasDepth(Stream))
                            {
                                sectionReference = _repo.SaveAndPublish(newWrapperBlock);
                            }
                            if (!background.IsNullOrEmpty())
                            {
                                newWrapperBlock.BackgroundColor = background;
                            }

                            var reference = sectionReference.IsNullOrEmpty() ? parentReference : sectionReference;
                            var block = GenerateNewPageFromXMLSection(reference, new List<BaseBlockData>(), newWrapperBlock);
                            if (block != null)
                            {
                                currentBlocksList.Add(block);
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
            var reference = parentPageReference.IsNullOrEmpty() ? rootReference : parentPageReference;
            var assetsFolderForPage = _contentAssetHelper.GetOrCreateAssetFolder(reference);
            var blockInstance = _repo.GetDefault<T>(assetsFolderForPage.ContentLink);
            var blockForPage = blockInstance as IContent;
            blockForPage.Name = newBlockName;
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
            _repo.SaveAndPublish(currPage);
            return currPage;
        }
        private IList<ContentReference> GenerateRelatedPages()
        {
            IList<ContentReference> references = new List<ContentReference>();
            var serviceLandingPage = ContentShortcuts.GetServiceLandingPageReference();
            var services = serviceLandingPage.GetChildren().Select(x => x as ServicePage).ToList();
            if (!serviceLandingPage.IsNullOrEmpty())
            {
                while (_xmlReader.ReadAndCount() && !_xmlReader.IsEndOfElementSection("services"))
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

        /// <summary>
        /// Returns a string value from an element with 1-level depth
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private string GetStringFromShallowElement(string elementType)
        {
            string value = null;
            while (_xmlReader.ReadAndCount() && !_xmlReader.IsEndOfElementSection(elementType))
            {
                if (value == null)
                {
                    value = _xmlReader.Value;
                }

            }
            return value;
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
            GenericContainerBlock rootWrapper = CreateGenericBlockForPage<GenericContainerBlock>(currPage.ContentLink, "root");
            rootWrapper = GenerateNewPageFromXMLSection(currPage.ContentLink, new List<BaseBlockData>(), rootWrapper) as GenericContainerBlock;
            ContentArea area = rootWrapper.MainContentArea;
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
            OnStatusChanged($"Starting execution of {this.GetType()}");
            SiteDefinition.Current = SiteDefinitionRepository.Service.List().First();

            while (_xmlReader.ReadAndCount())
            {
                if (_xmlReader.IsStartElement("case"))
                {
                    var longtitle = _xmlReader.GetAttribute("longtitle");
                    var id = _xmlReader.GetAttribute("id");
                    var imageUrl = _xmlReader.GetAttribute("image");

                    OnStatusChanged($"Executing job: {id}");

                    WorkPage currPage = CreateOrGetExistingPage<WorkPage>();
                    rootReference = currPage.ContentLink;

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
                    while (_xmlReader.ReadAndCount() && !_xmlReader.IsEndOfElementSection("case"))
                    {
                        switch (_xmlReader.Name)
                        {
                            case "tagline":
                                var tagline = GetStringFromShallowElement("tagline");
                                if (!tagline.IsNullOrEmpty())
                                {
                                    currPage.TeaserDescription = new XhtmlString(tagline);
                                }
                                break;
                            case "description":
                                var description = GetStringFromShallowElement("description");
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

                    _repo.SaveAndPublish(currPage);
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