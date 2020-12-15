using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Common.EntitySql;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.DataAnnotations;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Business.Extensions;
using StartProjectGuide.Models.Blocks;
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Business.Jobs
{
    [ScheduledPlugIn(DisplayName = "Load Or Update Page Job", GUID = "98d3c96d-a225-435b-8d13-c77fe9eef840", Description = "Loads or updates a page from the database")]
    public class LoadOrUpdatePageJob : ScheduledJobBase
    {
        private XmlReader xmlReader;
        private string stream = "~/Business/database/db.xml";
        private bool _stopSignaled;
        private string jobName = "Load or update Page job";
        private IContentRepository repo;
        public Injected<ISiteDefinitionRepository> SiteDefinitionRepository { get; set; }

        public LoadOrUpdatePageJob()
        {
            repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            xmlReader = XmlReader.Create("C:/Users/bombayworks/source/repos/EpiServerStartProject/StartProjectGuide/Business/database/db.xml");
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
        private T GetPageWithName<T>(string pageName) where T: BasePageData
        {
            PageReference startPage = ContentReference.StartPage;
            var page = startPage.GetChildWithName(pageName);
            if (page != null)
            {
                return (page.CreateWritableClone() as T);
            }
            var newPage = repo.GetDefault<T>(startPage);
            newPage.PageName = pageName;
            return newPage;
        }

        private string FormatOpenHTMLElement(string className = "", string type = "div")
        {
            var classString = !string.IsNullOrEmpty(className) ? $@" class=""{className}""" : null;
            return $"<{type ?? "div"}{classString}>";
        }
        private string FormatCloseHTMLElement(string type = "div")
        {
            return $"</{type}>";
        }

        private string FormatOpenAndClosedElement(string textValue, string className, string type = "div")
        {
            return $"{FormatOpenHTMLElement(className, type)}{textValue}{FormatCloseHTMLElement(type)}";
        }

        private string FormatImageElement(string src, string title)
        {
            return $@"<img src=""{src}"" title={title} />";
        }

        private XhtmlString GenerateNewPageFromXML()
        {
            StringBuilder sb = new StringBuilder();
            bool caseNotDone = true;
            while (xmlReader.Read() && caseNotDone)
            {
                switch (xmlReader.NodeType)
                {
                    case (XmlNodeType.Element):
                        var elementName = xmlReader.Name;
                        if (xmlReader.IsEmptyElement)
                        {
                            switch (elementName)
                            {
                                case "service":
                                    sb.AppendLine(FormatOpenAndClosedElement(xmlReader.GetAttribute("title"), "service", "span"));
                                    break;
                                case "image":
                                    sb.AppendLine(FormatImageElement(xmlReader.GetAttribute("url"),
                                        xmlReader.GetAttribute("title")));
                                    break;
                                case "header":
                                    sb.AppendLine(FormatOpenAndClosedElement(xmlReader.GetAttribute("text"), "header", xmlReader.GetAttribute("type") ?? "h1"));
                                    break;
                                default:
                                    sb.AppendLine(FormatOpenAndClosedElement(null, elementName));
                                    break;
                            }
                        }
                        else if (xmlReader.IsStartElement())
                        {
                            sb.AppendLine(FormatOpenHTMLElement(elementName));
                        }

                        break;
                    case XmlNodeType.EndElement:
                        //End Element  
                        if (xmlReader.Name == "case")
                        {
                            caseNotDone = false;
                        }
                        sb.AppendLine(FormatCloseHTMLElement());
                        break;
                    case XmlNodeType.Attribute:
                        //Attribute
                        break;
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                        //TEXT
                        //CDATA
                        sb.AppendLine(FormatOpenAndClosedElement(xmlReader.Value, null, "span"));
                        break;

                }

            }
            return new XhtmlString(sb.ToString());
        }

        private T CreateGenericBlockForPage<T>(ContentReference parentPageReference, string newBlockName,
            SaveAction saveAction = SaveAction.Publish, AccessLevel accessLevel = AccessLevel.NoAccess) where T : BaseBlockData
        {
            var assetsFolderForPage = ServiceLocator.Current
                .GetInstance<ContentAssetHelper>()
                .GetOrCreateAssetFolder(parentPageReference);
            var blockInstance = repo.GetDefault<T>(assetsFolderForPage.ContentLink);
            var blockForPage = blockInstance as IContent;
            blockForPage.Name = newBlockName;
            repo.Save(blockForPage, saveAction, accessLevel);
            return blockInstance;
        }

        private T CreateOrGetExistingPage<T>() where T : BasePageData
        {
            string title = xmlReader.GetAttribute("title");
            string longTitle = xmlReader.GetAttribute("longtitle");

            var currPage = GetPageWithName<T>(title);

            currPage.VisibleInMenu = false;
            currPage.IntroSection.Header = longTitle ?? title;
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

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "case")
                {
                    var currPage = CreateOrGetExistingPage<StandardPage>();
                    XhtmlString text = GenerateNewPageFromXML();
                    
                    //Textblock
                    TextBlock textBlock = CreateGenericBlockForPage<TextBlock>(currPage.ContentLink, "GeneratedTextBlock", SaveAction.Publish, AccessLevel.Read);
                    textBlock.Body = text;
                    repo.Save(textBlock as IContent, SaveAction.Publish, AccessLevel.Read);

                    //Add textblock to Content Area
                    currPage.MainContentArea = new ContentArea();
                    ContentAreaItem item = new ContentAreaItem
                    {
                        ContentLink = (textBlock as IContent).ContentLink
                    };
                    currPage.MainContentArea.Items.Add(item);

                    //Save
                    repo.Save(currPage, SaveAction.Publish, AccessLevel.Read);
                }
            }

            if (_stopSignaled)
            {
                return $"{jobName} was stopped manually";
            }

            return $"Success. {jobName} finished.";
        }
    }
}