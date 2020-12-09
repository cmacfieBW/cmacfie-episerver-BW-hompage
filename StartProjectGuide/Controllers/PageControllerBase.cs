using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using StartProjectGuide.Models.Pages;
using EPiServer.Shell.Security;
using StartProjectGuide.Models.ViewModels;
using StartProjectGuide.Business.BaseClasses;

namespace StartProjectGuide.Controllers
{
    public abstract class PageControllerBase<T> : PageController<T> where T: BasePageData
    {

    }
}