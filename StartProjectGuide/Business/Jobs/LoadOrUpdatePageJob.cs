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
        private readonly XmlReaderSettings _xmlReaderSettings;
        private const string Stream = "http://13.53.100.131/index.xml";
        //private const string Stream = "C:/Users/bombayworks/source/repos/EpiServerStartProject/StartProjectGuide/Business/database/db.xml";
        private const string JobName = "Load or update Page job";

        private readonly XmlReader _xmlReader;
        private bool _stopSignaled;
        private ContentReference _rootReference;
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

        private string FormatOpenAndClosedElement(string textValue, string className, string type = "div")
        {
            var classString = !string.IsNullOrEmpty(className) ? $@" class=""{className}""" : null;
            return $"<{type ?? "div"}{classString}>{textValue}</{type}>";
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

                            if (parentReference != _rootReference)
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
                        TextBlock textBlock = BlockHelper.CreateTextBlock(parentReference ?? _rootReference, html, $"TextBlock - {text.Substring(0, Math.Min(text.Length, 20))}");
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
                                        ImageBlock imgBlock = BlockHelper.CreateImageBlock(parentReference ?? _rootReference, url, title);
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
                                    TextBlock t = BlockHelper.CreateTextBlock(parentReference ?? _rootReference, header, $"HeaderBlock - {headerText.Substring(0, Math.Min(headerText.Length, 20))}");
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
                                (parentReference ?? _rootReference).CreateGenericBlockForPage<GenericContainerBlock>($"Container- {_xmlReader.Name.CapitalizeFirstLetter()}");
                            var sectionReference = ContentReference.EmptyReference;
                            if (_xmlReader.SectionHasManyChildren(Stream))
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
            GenericContainerBlock rootWrapper = currPage.ContentLink.CreateGenericBlockForPage<GenericContainerBlock>("root");
            rootWrapper = GenerateNewPageFromXMLSection(currPage.ContentLink, new List<BaseBlockData>(), rootWrapper) as GenericContainerBlock;
            ContentArea area = rootWrapper.MainContentArea;
            currPage.MainContentArea = area;

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
                    var id = _xmlReader.GetAttribute("id");
                    OnStatusChanged($"Executing job: {id ?? "unknown"}");

                    var title = _xmlReader.GetAttribute("title");
                    var longTitle = _xmlReader.GetAttribute("longtitle");
                    var imageUrl = _xmlReader.GetAttribute("image");

                    var workLanding = ContentShortcuts.GetWorkLandingPageReference() ?? ContentReference.StartPage;
                    var currPage = workLanding.ToPageReference().GetOrCreatePageWithName<WorkPage>(title);
                    if (currPage != null)
                    {
                        _repo.SaveAndPublish(currPage);
                    }

                    _rootReference = currPage.ContentLink;

                    if (!longTitle.IsNullOrEmpty())
                    {
                        currPage.IntroSection.Header = longTitle;
                    }

                    if (imageUrl != null)
                    {
                        var imgBlock = BlockHelper.CreateImageBlock(_rootReference, imageUrl, $"{id}-header-image");
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
                                var tagLine = _xmlReader.ReadShallowElementAndCount("tagline");
                                if (!tagLine.IsNullOrEmpty())
                                {
                                    currPage.TeaserDescription = new XhtmlString(tagLine);
                                }
                                break;
                            case "description":
                                var description = _xmlReader.ReadShallowElementAndCount("description");
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