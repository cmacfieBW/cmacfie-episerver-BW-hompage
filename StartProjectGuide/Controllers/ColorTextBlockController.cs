using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using StartProjectGuide.Models.Blocks;

namespace StartProjectGuide.Controllers
{
    public class ColorTextBlockController : BlockController<ColorTextBlock>
    {
        public override ActionResult Index(ColorTextBlock currentBlock)
        {
            return PartialView(currentBlock);
        }
    }
}
