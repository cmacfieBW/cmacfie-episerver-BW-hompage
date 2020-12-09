using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using StartProjectGuide.Models.Pages;
using StartProjectGuide.Models.ViewModels;

namespace StartProjectGuide.Controllers
{
    [TemplateDescriptor(Default = true)]
    public class FindSearchPageController : PageController<FindSearchPage>
    {
        public ActionResult Index(FindSearchPage currentPage, string q)
        {
            var model = new FindSearchPageViewModel(currentPage, q);
            if (String.IsNullOrEmpty(q))
            {
                return View(model);
            }
            var unifiedSearch = SearchClient.Instance.UnifiedSearchFor(q);
            model.Results = unifiedSearch.GetResult();
            return View(model);
        }
    }
}