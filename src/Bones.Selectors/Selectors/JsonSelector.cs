using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Bones.Selectors.Interfaces;

namespace Bones.Selectors
{
    public class JsonSelector : IJsonSelector
    {
        private static readonly char JSON_QUERY_SEPARATOR = '.';
        private static readonly string JSON_QUERY_ENUMERATOR = "*";

        public JsonSelector() { }

        public IEnumerable<string> Select(JsonElement rootElement, string selector)
        {
            string[] paths = selector.Split(new char[] { JSON_QUERY_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<JsonElement> selectedElements = BrowseJson(new[] { rootElement }, paths);
            return GetRawElements(selectedElements);
        }





        private IEnumerable<JsonElement> BrowseJson(IEnumerable<JsonElement> currentElements, IEnumerable<string> query)
        {
            if (!query.Any()) return currentElements;

            List<JsonElement> childrenOfCurrent = new List<JsonElement>();
            string currentPath = query.First();

            foreach (JsonElement current in currentElements)
            {
                try
                {
                    if (IsIndexSelector(currentPath, out var index)) AddOneElement(childrenOfCurrent, current, index);
                    else if (IsArraySelector(currentPath)) AddAllElements(childrenOfCurrent, current);
                    else AddPropertyElement(childrenOfCurrent, current, currentPath);
                }
                catch (Exception e)
                {
                    throw new SelectorException($"An error occurred while browsing ! Current step: {currentPath}", e);
                }
            }

            return BrowseJson(childrenOfCurrent, query.Skip(1));
        }

        private IEnumerable<string> GetRawElements(IEnumerable<JsonElement> selectedElements)
        {
            List<string> results = new List<string>();

            foreach (JsonElement el in selectedElements)
            {
                try
                {
                    results.Add(el.ToString());
                }
                catch (Exception e)
                {
                    throw new SelectorException($"An error occurred while getting raw text !", e);
                }
            }

            return results;
        }

        // Helper Methods

        private bool IsIndexSelector(string selector, out int index)
        {
            return int.TryParse(selector, out index);
        }

        private void AddOneElement(List<JsonElement> elements, JsonElement currentElement, int index)
        {
            elements.Add(currentElement[index]);
        }

        private bool IsArraySelector(string selector)
        {
            return JSON_QUERY_ENUMERATOR.Equals(selector);
        }

        private void AddAllElements(List<JsonElement> elements, JsonElement currentElement)
        {
            foreach (var arrayElt in currentElement.EnumerateArray())
            {
                elements.Add(arrayElt);
            }
        }

        private void AddPropertyElement(List<JsonElement> elements, JsonElement currentElement, string propertyName)
        {
            elements.Add(currentElement.GetProperty(propertyName));
        }
    }
}