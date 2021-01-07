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
using StartProjectGuide.Business.Helpers;
using StartProjectGuide.Models.Blocks;
using StartProjectGuide.Models.Media;
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Business.Jobs
{
    [ScheduledPlugIn(DisplayName = "Load Or Update Page Job", GUID = "98d3c96d-a225-435b-8d13-c77fe9eef840", Description = "Loads or updates a page from the database")]
    public class LoadOrUpdatePageJob : ScheduledJobBase
    {
        public Injected<ISiteDefinitionRepository> SiteDefinitionRepository { get; set; }

        /// <summary>
        /// Creates or updates an existing with page with data from a hardcoded XML-file.
        /// </summary>
        /// <returns></returns>
        public override string Execute()
        {
            OnStatusChanged($"Starting execution of {this.GetType()}");
            SiteDefinition.Current = SiteDefinitionRepository.Service.List().First();

            return LoadOrUpdatePageJobHelper.JobHandler();
        }
    }
}