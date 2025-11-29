using System.Collections.Generic;
using lab2Graduates.Models;

namespace lab2Graduates.Strategies
{
    public interface IXmlSearchStrategy
    {
        string Name { get; }
        List<Graduate> Search(
            string xmlPath,
            string? attributeName,
            string? attributeValue,
            string? keyword);
    }
}
