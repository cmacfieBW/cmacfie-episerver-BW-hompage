using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using StartProjectGuide.Models.Pages;

namespace StartProjectGuide.Business.Jobs
{
    /// <summary>
    /// Job that updates the string field Date in StartPage
    /// </summary>
    [ScheduledPlugIn(DisplayName = "Write Date Job", GUID = "d6619008-3e76-4886-b3c7-9a025a0c2603")]
    public class WriteDateJob : ScheduledJobBase
    {
        private bool _stopSignaled;
        private string jobName = "Write Date Job";
        public Injected<ISiteDefinitionRepository> SiteDefinitionRepository { get; set; }

        public WriteDateJob()
        {
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
        /// Updates the Date property in StartPage with current time in yyyy-MM-dd HH:mm:ss format.
        /// </summary>
        /// <returns>A string with either a success or failure message </returns>
        public override string Execute()
        {
            //Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged(String.Format("Starting execution of {0}", this.GetType()));
            SiteDefinition.Current = SiteDefinitionRepository.Service.List().First();
            IContentRepository repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            StartPage startPageClone =
                repo.Get<StartPage>(SiteDefinition.Current.StartPage).CreateWritableClone() as StartPage;
            if (startPageClone == null)
            {
                return $"Could not execute {jobName}. Could not find the start page.";
            }

            if (_stopSignaled)
            {
                return $"{jobName} was stopped manually";
            }

            string nowString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            startPageClone.Date += nowString + "<br/>";
            repo.Save(startPageClone, SaveAction.Publish, AccessLevel.Read);

            return $"Success. {jobName} finished with result {nowString}";
        }
    }
}