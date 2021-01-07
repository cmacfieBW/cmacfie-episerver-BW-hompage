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
        private static XmlReaderSettings _settings;
        private static string _stream;

        static XmlReaderExtensions()
        {
            _counter = 0;
        }

        public static XmlReader Create(this XmlReader reader, string stream, XmlReaderSettings settings)
        {
            _settings = settings;
            stream = _stream;
            return XmlReader.Create(stream, settings);
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

        public static XmlReader CreateClone(this XmlReader reader)
        {
            var clone = XmlReader.Create(_stream, _settings);
            var c = 0;
            while (clone.Read() && c < _counter - 1)
            {
                c++;
            }
            return clone;
        }

        /// <summary>
        /// Gets the number of children of a section, at a certain depth. Default is depth = 1
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int GetNumOfChildrenOfSection(this XmlReader reader, int maxDepth = 1)
        {
            var xmlReaderClone = reader.CreateClone();

            var c = 0;
            var name = xmlReaderClone.Name;
            var line = xmlReaderClone.Value;

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
        public static bool SectionHasManyChildren(this XmlReader reader, int numOfChildren = 1)
        {
            return GetNumOfChildrenOfSection(reader) > numOfChildren;
        }

        /// <summary>
        /// Returns a string value from an element with 1-level depth, and increases the reader index
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static string ReadShallowElementAndCount(this XmlReader reader, string elementType)
        {
            string value = null;
            if (reader.GetNumOfChildrenOfSection() == 1)
            {
                while (reader.ReadAndCount() && !reader.IsEndOfElementSection(elementType))
                {
                    if (value == null)
                    {
                        value = reader.Value;
                    }

                }
            }
            return value;
        }
    }
}