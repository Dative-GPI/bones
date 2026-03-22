using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml;
using Bones.Selectors.Interfaces;

namespace Bones.Selectors
{
    public class XmlSelector : IXmlSelector
    {
        public XmlSelector() { }

        // https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmldocument?view=net-6.0
        public IEnumerable<string> Select(XmlDocument document, string selector)
        {
            List<string> results = new List<string>();

            // Namespaces cause problems, but this would be complicated to resolve, ignored for now
            // XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            XmlNodeList nodeList = document.DocumentElement.SelectNodes(selector);

            foreach (XmlNode node in nodeList)
            {
                results.Add(node.InnerXml);
            }

            return results;
        }
    }
}