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
    public class ContactLandingPageController : PageController<ContactLandingPage>
    {
        public ActionResult Index(ContactLandingPage currentPage)
        {
            PageViewModel<ContactLandingPage> model = PageViewModel.Create(currentPage);
            return View(model);
        }
    }
}