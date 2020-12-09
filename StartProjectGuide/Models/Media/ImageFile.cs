using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;

namespace StartProjectGuide.Models.Media
{
    [ContentType(DisplayName = "DefaultMedia1", GUID = "b270a886-c87f-4f48-8f04-5e1e9c1a0d7e", Description = "")]
    [MediaDescriptor(ExtensionString = "jpg, png, svg, jpeg, gif")]
    public class ImageFile : ImageData
    {

    }
}