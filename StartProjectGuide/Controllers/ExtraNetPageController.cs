using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web;
using System.Web.Security;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Find.Helpers.Text;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Personalization;
using EPiServer.Security;
using EPiServer.Web.Mvc;
using StartProjectGuide.Business.DynamicDataStore;
using StartProjectGuide.Models.Pages;
using StartProjectGuide.Models.ViewModels;

namespace StartProjectGuide.Controllers
{
    public class ExtraNetPageController : PageController<ExtraNetPage>
    {
        public ActionResult Index(ExtraNetPage currentPage, string actionMessage)
        {
            var model = new ExtraNetPageViewModel(currentPage);
            ViewBag.ActionMessage = actionMessage;
            return View(model);
        }

        /// <summary>
        /// Deletes a weather post entry
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="entryId"></param>
        /// <returns></returns>
        public ActionResult DeleteWeatherEntry(ExtraNetPage currentPage, string entryId)
        {
            Identity id = Identity.Parse(entryId);
            WeatherDds.RemoveWeather(id);
            WeatherDds.Weather removed = WeatherDds.GetWeatherEntry(id);
            if (removed == null)
            {
                return RedirectToAction("Index", new { actionMessage = $"Removed post successfully" });
            }
            return RedirectToAction("Index", new { actionMessage = $"Could not remove post" });
        }

        /// <summary>
        /// Adds a new weather post
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="comment"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public ActionResult AddWeatherEntry(ExtraNetPage currentPage, string comment, string description)
        {
            if (comment != null && description != null)
            {
                WeatherDds.Weather weather = new WeatherDds.Weather
                {
                    WeatherDescription = description,
                    WeatherComment = comment,
                    TimeStamp = DateTime.Now,
                };
                WeatherDds.AddWeather(weather);
                return RedirectToAction("Index", new { actionMessage = "Post successful!" });
            }
            return RedirectToAction("Index", new { actionMessage = "Post failed!" });
        }

    }
}