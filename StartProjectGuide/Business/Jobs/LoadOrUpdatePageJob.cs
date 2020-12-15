﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity.Core.Common.EntitySql;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Castle.Core.Internal;
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
        private T GetPageWithName<T>(string pageName) where T : BasePageData
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

        private string FormatOpenAndClosedElement(string textValue, string className, string type = "div")
        {
            var classString = !string.IsNullOrEmpty(className) ? $@" class=""{className}""" : null;
            return $"<{type ?? "div"}{classString}>{textValue}</{type}>";
        }

        private TextBlock CreateHeaderBlock(ContentReference parent)
        {
            var text = xmlReader.GetAttribute("text");
            var type = xmlReader.GetAttribute("type") ?? "h1";
            var html = FormatOpenAndClosedElement(text, "header", type);
            TextBlock textBlock =
                CreateGenericBlockForPage<TextBlock>(parent,
                    $"HeaderBlock - {text.Substring(0, 10)}");
            textBlock.Body = new XhtmlString(html);
            SaveGenericBlock(textBlock);
            return textBlock;
        }

        private TextBlock CreateTextBlock(ContentReference parent)
        {
            var text = xmlReader.GetAttribute("text") ?? xmlReader.Value;
            var html = FormatOpenAndClosedElement(text, "text", "span");
            TextBlock textBlock =
                CreateGenericBlockForPage<TextBlock>(parent,
                    $"TextBlock - {text.Substring(0, 10)}");
            textBlock.Body = new XhtmlString(html);
            SaveGenericBlock(textBlock);
            return textBlock;
        }

        private ImageBlock CreateImageBlock(ContentReference parent)
        {
            var title = xmlReader.GetAttribute("title");
            var url = xmlReader.GetAttribute("url");
            //TODO Check if img url can be resolved
            //if (!url.IsNullOrEmpty())
            //{
            //    ImageBlock imgBlock =
            //        CreateGenericBlockForPage<ImageBlock>(parent, $"image-{title}");
            //    imgBlock.Url = url;
            //    imgBlock.Title = title;
            //    SaveGenericBlock(imgBlock);
            //    return imgBlock;
            //}

            return null;
        }

        /// <summary>
        /// Recursively goes through the XML file and generates a block-tree.
        /// </summary>
        /// <param name="parentReference"></param>
        /// <param name="outerBlocks"></param>
        /// <param name="parentWrapper"></param>
        /// <returns>A BaseBlockData with a ContentArea with all blocks in a tree structure </returns>
        private BaseBlockData GenerateNewPageFromXMLSection(ContentReference parentReference, IList<BaseBlockData> outerBlocks, GenericContainerBlock parentWrapper)
        {
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
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
                        TextBlock textBlock = CreateTextBlock(parentReference);
                        if (textBlock != null)
                        {
                            outerBlocks.Add(textBlock);
                        }

                        break;
                    case (XmlNodeType.Element):
                        if (xmlReader.IsEmptyElement)
                        {
                            switch (xmlReader.Name)
                            {
                                case "video":
                                    //TODO
                                    break;
                                case "image":
                                    ImageBlock imgBlock = CreateImageBlock(parentReference);
                                    if (imgBlock != null)
                                    {
                                        outerBlocks.Add(imgBlock);
                                    }

                                    break;
                                case "header":
                                    TextBlock t = CreateHeaderBlock(parentReference);
                                    if (t != null)
                                    {
                                        outerBlocks.Add(t);
                                    }

                                    break;
                            }
                        }
                        else if (xmlReader.IsStartElement())
                        {
                            GenericContainerBlock newWrapperBlock =
                                CreateGenericBlockForPage<GenericContainerBlock>(parentReference, "Section wrapper");
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

        private void SaveGenericBlock<T>(T block) where T : BaseBlockData
        {
            repo.Save(block as IContent, SaveAction.Publish, AccessLevel.Read);
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
                    var id = xmlReader.GetAttribute("id");
                    OnStatusChanged(String.Format("Executing job: {0}", id));

                    var currPage = CreateOrGetExistingPage<StandardPage>();
                    currPage.MainContentArea = new ContentArea();
                    while (xmlReader.Name != "sections")
                    {
                        xmlReader.Read();
                    }
                    GenericContainerBlock sectionsWrapper = CreateGenericBlockForPage<GenericContainerBlock>(currPage.ContentLink, "sectionsWrapper");
                    GenericContainerBlock outMostWrapper = GenerateNewPageFromXMLSection(currPage.ContentLink, new List<BaseBlockData>(), sectionsWrapper) as GenericContainerBlock;
                    ContentArea area = outMostWrapper.MainContentArea;
                    currPage.MainContentArea = area;

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