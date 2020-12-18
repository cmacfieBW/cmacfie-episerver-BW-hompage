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

        /// <summary>
        /// Returns the current line number of the XmlReader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int GetLineNumber(this XmlReader reader)
        {
            return _counter;
        }

        /// <summary>
        /// Reads the next node of the stream and increases the line number counter
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static bool ReadAndCount(this XmlReader reader)
        {
            _counter++;
            return reader.Read();
        }


        /// <summary>
        /// Checks if the current line is an end-element with a certain node name
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static bool IsEndOfElementSection(this XmlReader reader, string nodeName)
        {
            return (reader.Name == nodeName && reader.NodeType == XmlNodeType.EndElement);
        }

        /// <summary>
        /// Checks if the current line is the start of a new element with a certain node name
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static bool IsStartOfElementSection(this XmlReader reader, string nodeName)
        {
            return (reader.Name == nodeName && reader.NodeType == XmlNodeType.Element && reader.IsStartElement());
        }

        /// <summary>
        /// Gets the number of children of a section, at a certain depth. Default is depth = 1
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int GetNumOfChildrenOfSection(this XmlReader reader, string stream, int maxDepth = 1)
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
                if (xmlReaderClone.Depth == depth + maxDepth && xmlReaderClone.NodeType != XmlNodeType.EndElement)
                {
                    c++;
                }
            }

            return c;
        }

        /// <summary>
        /// Checks if the current section has more than a number of children, default is numOfChildren = 1
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="stream"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static bool SectionHasManyChildren(this XmlReader reader, string stream, int numOfChildren = 1)
        {
            return GetNumOfChildrenOfSection(reader, stream) > numOfChildren;
        }
    }
}