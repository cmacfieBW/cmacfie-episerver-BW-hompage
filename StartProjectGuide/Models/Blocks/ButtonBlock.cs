using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer;
using EPiServer.Cms.Shell.UI.ObjectEditing.EditorDescriptors.SelectionFactories;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;

namespace StartProjectGuide.Models.Blocks
{
    [ContentType(DisplayName = "Button", GUID = "c3e5c375-c4ec-402c-8707-f7c1824d134c", Description = "", AvailableInEditMode = false)]
    public class ButtonBlock : BlockData
    {

        [CultureSpecific]
        [Display(
            Name = "Text",
            Description = "The text of the button",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string Text { get; set; }

        [Display(
            Name = "URL",
            Description = "The url that the button leads to",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual Url Url { get; set; }

        [SelectOne(SelectionFactoryType = typeof(StyleSectionFactory))]
        public virtual string Style { get; set; }

        private class StyleSectionFactory : ISelectionFactory
        {

            public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
            {
                ISelectItem defaultValue = new SelectItem()
                {
                    Text = "Default button",
                    Value = "button"
                };

                ISelectItem[] list =  {
                    defaultValue,
                    new SelectItem()
                    {
                        Text = "Simple link",
                        Value = "link"
                    }
                };
                metadata.InitialValue = defaultValue;
                return list;
            }
        }

    }
}