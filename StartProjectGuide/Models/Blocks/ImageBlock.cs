using System;
using System.ComponentModel.DataAnnotations;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(
        GUID = "09854019-91A5-4B93-8623-17F038346001",
        AvailableInEditMode = false,
        GroupName = Global.GroupNames.SiteSettings)]
    public class ImageBlock : BlockData
    {
        [DefaultDragAndDropTarget]
        [UIHint(UIHint.Image)]
        public virtual Url Url
        {
            get
            {
                var currentUrl = this.GetPropertyValue(curr => curr.Url);
                return currentUrl != null && !currentUrl.IsEmpty() ? currentUrl : new Url("~/Static/images/no-image.png");
            }
            set
            {
                this.SetPropertyValue(curr => curr.Url, value);
            }
        }
        [CultureSpecific]
        public virtual string Title { get; set; }

    }
}