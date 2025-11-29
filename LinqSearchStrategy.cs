using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using lab2Graduates.Models;

namespace lab2Graduates.Strategies
{
    public class LinqSearchStrategy : IXmlSearchStrategy
    {
        public string Name => "LINQ to XML";

        public List<Graduate> Search(
            string xmlPath,
            string? attributeName,
            string? attributeValue,
            string? keyword)
        {
            var doc = XDocument.Load(xmlPath);

            var elements = doc.Root?.Elements("graduate") ?? Enumerable.Empty<XElement>();

            var result = elements
                .Select(e => ReadGraduateWithGroup(e))
                
                .Where(g =>
                    MatchesAttributeFilter(g.Graduate, g.GroupFromXml, attributeName, attributeValue))
                
                .Where(g =>
                    MatchesKeywordFilter(g.Graduate, g.GroupFromXml, keyword))
               
                .Select(g => g.Graduate)
                .ToList();

            return result;
        }

        private static (Graduate Graduate, string GroupFromXml) ReadGraduateWithGroup(XElement element)
        {
            var graduate = new Graduate
            {
                FullName = (string?)element.Attribute("fullName") ?? string.Empty,
                Faculty = (string?)element.Attribute("faculty") ?? string.Empty,
                Department = (string?)element.Attribute("department") ?? string.Empty,
                Speciality = (string?)element.Attribute("speciality") ?? string.Empty,
                AdmissionYear = ParseNullableInt((string?)element.Attribute("admissionYear")),
                GraduationYear = ParseNullableInt((string?)element.Attribute("graduationYear"))
            };

            var groupFromXml = (string?)element.Attribute("group") ?? string.Empty;

            foreach (var cm in element.Elements("careerMove"))
            {
                var text = (cm.Value ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    graduate.CareerMoves.Add(text);
            }

            return (graduate, groupFromXml);
        }

        private static int? ParseNullableInt(string? value)
        {
            if (int.TryParse(value, out var number))
                return number;
            return null;
        }

        private static bool MatchesAttributeFilter(
            Graduate graduate,
            string groupFromXml,
            string? attributeName,
            string? attributeValue)
        {
            if (string.IsNullOrWhiteSpace(attributeName) ||
                string.IsNullOrWhiteSpace(attributeValue))
            {
                return true;
            }

            var expected = attributeValue.Trim();

            string? actual = attributeName switch
            {
                "fullName" => graduate.FullName,
                "faculty" => graduate.Faculty,
                "department" => graduate.Department,
                "speciality" => graduate.Speciality,
                "group" => groupFromXml,
                "admissionYear" => graduate.AdmissionYear?.ToString(),
                "graduationYear" => graduate.GraduationYear?.ToString(),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(actual))
                return false;

            return string.Equals(
                actual.Trim(),
                expected,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesKeywordFilter(
            Graduate graduate,
            string groupFromXml,
            string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return true;

            var k = keyword.Trim();

            bool Contains(string? s) =>
                !string.IsNullOrEmpty(s) &&
                s.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0;

            if (Contains(graduate.FullName)) return true;
            if (Contains(graduate.Faculty)) return true;
            if (Contains(graduate.Department)) return true;
            if (Contains(graduate.Speciality)) return true;
            if (Contains(groupFromXml)) return true;
            if (Contains(graduate.StudyPeriod)) return true;

            foreach (var move in graduate.CareerMoves)
            {
                if (Contains(move)) return true;
            }

            return false;
        }
    }
}
