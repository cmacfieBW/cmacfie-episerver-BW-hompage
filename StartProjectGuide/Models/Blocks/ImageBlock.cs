using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using StartProjectGuide.Business.BaseClasses;
using StartProjectGuide.Models.Media;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(
        GUID = "09854019-91A5-4B93-8623-17F038346001",
        GroupName = Global.GroupNames.SiteSettings)]
    public class ImageBlock : BaseBlockData
    {
        [DefaultDragAndDropTarget]
        [AllowedTypes(typeof(ImageFile))]
        [UIHint(UIHint.Image)]
        public virtual ContentReference ImageReference { get; set; }
        
        [CultureSpecific]
        public virtual string Title { get; set; }

    }
}