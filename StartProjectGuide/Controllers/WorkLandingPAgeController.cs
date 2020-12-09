using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using StartProjectGuide.Models.Pages;
using StartProjectGuide.Models.ViewModels;

namespace StartProjectGuide.Controllers
{
    public class WorkLandingPageController : PageControllerBase<WorkLandingPage>
    {
        public ActionResult Index(WorkLandingPage currentPage)
        {
            PageViewModel<WorkLandingPage> model = PageViewModel.Create(currentPage);
            return View(model);
        }
    }
}