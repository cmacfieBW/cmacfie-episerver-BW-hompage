using System.Collections.Generic;
using System;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using StartProjectGuide.Models.Pages;
using StartProjectGuide.Models.ViewModels;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Business.Interfaces;

namespace StartProjectGuide.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    public class DefaultPageController : PageControllerBase<BasePageData>
    {
        public ViewResult Index(BasePageData currentPage)
        {
            var model = CreateModel(currentPage);
            string viewName = string.Format("~/Views/{0}/Index.cshtml", currentPage.GetOriginalType().Name);
            return View(viewName, model);
        }

        public static IPageViewModel<BasePageData> CreateModel(BasePageData page)
        {
            var type = typeof(PageViewModel<>).MakeGenericType(page.GetOriginalType());
            return Activator.CreateInstance(type, page) as IPageViewModel<BasePageData>;
        }
    }
}