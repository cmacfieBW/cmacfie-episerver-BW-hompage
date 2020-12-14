using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using EPiServer;
using EPiServer.Core;
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
    [ScheduledPlugIn(DisplayName = "LoadOrUpdatePageJob", GUID = "98d3c96d-a225-435b-8d13-c77fe9eef840", Description = "Loads or updates a page from the database")]
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
        private StandardPage GetPageWithName(string pageName)
        {
            PageReference startPage = ContentReference.StartPage;
            var page = startPage.GetChildWithName(pageName);
            if (page != null)
            {
                return (page.CreateWritableClone() as StandardPage);
            }
            var newPage = repo.GetDefault<StandardPage>(startPage);
            newPage.PageName = pageName;
            return newPage;
        }

        private string OpenHTMLElement(string className = "", string type = "div")
        {
            var classString = !string.IsNullOrEmpty(className) ? $@" class=""{className}""" : null;
            return $"<{type ?? "div"}{classString}>";
        }
        private string CloseHTMLElement(string type = "div")
        {
            return $"</{type}>";
        }

        private string OpenAndClosedElement(string textValue, string className, string type = "div")
        {
            return $"{OpenHTMLElement(className, type)}{textValue}{CloseHTMLElement(type)}";
        }

        private XhtmlString GenerateNewPageFromXML()
        {
            string title = xmlReader.GetAttribute("title");
            string id = xmlReader.GetAttribute("id");
            string longtitle = xmlReader.GetAttribute("longtitle");
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
                                case "header":
                                    sb.AppendLine(OpenAndClosedElement(xmlReader.GetAttribute("text"), "header", xmlReader.GetAttribute("type") ?? "h1"));
                                    break;
                                default:
                                    sb.AppendLine(OpenAndClosedElement(null, elementName));
                                    break;
                            }
                        }
                        else if (xmlReader.IsStartElement())
                        {
                            sb.AppendLine(OpenHTMLElement(elementName));
                        }

                        break;
                    case XmlNodeType.EndElement:
                        //End Element  
                        if (xmlReader.Name == "case")
                        {
                            caseNotDone = false;
                        }
                        sb.AppendLine(CloseHTMLElement());
                        break;
                    case XmlNodeType.Attribute:
                        Console.WriteLine("Attribute" + xmlReader.Value);
                        break;
                    case XmlNodeType.CDATA:
                        Console.WriteLine("CDATA" + xmlReader.Value);
                        sb.AppendLine(OpenAndClosedElement(xmlReader.Value, "text", "span"));
                        break;
                    case XmlNodeType.Text:
                        Console.WriteLine("Text" + xmlReader.Value);
                        sb.AppendLine(OpenAndClosedElement(xmlReader.Value, null, "span"));
                        break;

                }

            }
            return new XhtmlString(sb.ToString());
        }

        private StandardPage CreateOrUpdatePage()
        {
            string title = xmlReader.GetAttribute("title");
            string longTitle = xmlReader.GetAttribute("longtitle");
            XhtmlString text = GenerateNewPageFromXML();
            StandardPage currPage = GetPageWithName(title);
            currPage.VisibleInMenu = false;
            currPage.IntroSection.Header = longTitle ?? title;
            currPage.IntroSection.Preamble = text;
            var textBlock = repo.GetDefault<TextBlock>(ContentReference.GlobalBlockFolder);
            textBlock.Body = text;
            currPage.MainContentArea = new ContentArea();
            ContentAreaItem item = new ContentAreaItem
            {
                ContentLink = ((IContent)textBlock).ContentLink
            };
            currPage.MainContentArea.Items.Add(item);
            //currPage.MainContentArea.Items.Add();
            return currPage;
        }

        public override string Execute()
        {
            OnStatusChanged(String.Format("Starting execution of {0}", this.GetType()));

            SiteDefinition.Current = SiteDefinitionRepository.Service.List().First();
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "case")
                {
                    var newPage = CreateOrUpdatePage();
                    repo.Save(newPage, SaveAction.Publish);
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