using System.Collections.Generic;
using System.Text.Json;

namespace Bones.Selectors.Interfaces
{
    public interface IJsonSelector
    {
        IEnumerable<string> Select(JsonElement element, string selector);
    }
}