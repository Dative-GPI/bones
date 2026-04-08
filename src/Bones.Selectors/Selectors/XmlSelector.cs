using System.Collections.Generic;
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
            var results = new List<string>();

            var root = document.DocumentElement;
            if (root == null)
                return results;

            var nodeList = root.SelectNodes(selector);
            if (nodeList == null)
                return results;

            foreach (XmlNode node in nodeList)
            {
                results.Add(node.InnerXml);
            }

            return results;
        }
    }
}