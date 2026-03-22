using System.Collections.Generic;
using System.Text.Json;
using System.Xml;

namespace Bones.Selectors.Interfaces
{
    public interface IXmlSelector
    {
        IEnumerable<string> Select(XmlDocument document, string selector);
    }
}