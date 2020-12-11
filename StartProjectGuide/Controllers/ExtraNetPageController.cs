using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Find.Helpers.Text;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using StartProjectGuide.Business.DynamicDataStore;
using StartProjectGuide.Models.Pages;
using StartProjectGuide.Models.ViewModels;

namespace StartProjectGuide.Controllers
{
    public class ExtraNetPageController : PageController<ExtraNetPage>
    {
        public ActionResult Index(ExtraNetPage currentPage, string weatherComment, string weatherDesc)
        {
            if (weatherComment.IsNotNullOrEmpty() || weatherDesc.IsNotNullOrEmpty())
            {
                return View(PageViewModel.Create(currentPage));
            }
            WeatherDds.Weather w = new WeatherDds.Weather();
            w.WeatherComment = weatherComment;
            w.WeatherDescription = weatherDesc;
            w.TimeStamp = new DateTime();
            var model = new ExtraNetPageViewModel(currentPage, w);
            return View(model);
        }
    }
}