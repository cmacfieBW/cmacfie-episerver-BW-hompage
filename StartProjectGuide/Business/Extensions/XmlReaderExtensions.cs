using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace StartProjectGuide.Business.Extensions
{
    public static class XmlReaderExtensions
    {

        private static int _counter;

        static XmlReaderExtensions()
        {
            _counter = 0;
        }

        public static int GetCounter(this XmlReader reader)
        {
            return _counter;
        }

        public static bool ReadAndCount(this XmlReader reader)
        {
            _counter++;
            return reader.Read();
        }


        /// <summary>
        /// Checks if the current line is an end-element with a certain name
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static bool IsEndOfElementSection(this XmlReader reader, string nodeName)
        {
            return (reader.Name == nodeName && reader.NodeType == XmlNodeType.EndElement);
        }

        public static bool IsStartOfElementSection(this XmlReader reader, string nodeName)
        {
            return (reader.Name == nodeName && reader.NodeType == XmlNodeType.Element && reader.IsStartElement());
        }

        public static int GetDepthOfSection(this XmlReader reader, string stream)
        {
            XmlReaderSettings settings = reader.Settings;
            var c = 0;
            var xmlReaderClone = XmlReader.Create(stream, settings);
            while (xmlReaderClone.Read() && c < _counter - 1)
            {
                c++;
            }

            var name = xmlReaderClone.Name;
            var line = xmlReaderClone.Value;


            c = 0;
            var depth = xmlReaderClone.Depth;
            while (xmlReaderClone.Read() &&
                   !(xmlReaderClone.NodeType == XmlNodeType.EndElement && xmlReaderClone.Name == name))
            {
                if (xmlReaderClone.Depth == depth + 1 && xmlReaderClone.NodeType != XmlNodeType.EndElement)
                {
                    c++;
                }
            }

            return c;
        }

        public static bool SectionHasDepth(this XmlReader reader, string stream, int depth = 1)
        {
            return GetDepthOfSection(reader, stream) > depth;
        }
    }
}