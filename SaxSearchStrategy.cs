using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using lab2Graduates.Models;

namespace lab2Graduates.Strategies
{
    public class SaxSearchStrategy : IXmlSearchStrategy
    {
        public string Name => "SAX";

        public List<Graduate> Search(
            string xmlPath,
            string? attributeName,
            string? attributeValue,
            string? keyword)
        {
            var result = new List<Graduate>();

            var settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using var reader = XmlReader.Create(xmlPath, settings);

            while (!reader.EOF)
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.Name == "graduate")
                {
                    var element = (XElement)XNode.ReadFrom(reader);

                    var graduate = ReadGraduate(element, out var groupFromXml);

                    if (!MatchesAttributeFilter(graduate, groupFromXml, attributeName, attributeValue))
                        continue;

                    if (!MatchesKeywordFilter(graduate, groupFromXml, keyword))
                        continue;

                    result.Add(graduate);
                }
                else
                {
                    reader.Read();
                }
            }

            return result;
        }

        private static Graduate ReadGraduate(XElement element, out string groupFromXml)
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

            groupFromXml = (string?)element.Attribute("group") ?? string.Empty;

            foreach (var cm in element.Elements("careerMove"))
            {
                var text = (cm.Value ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    graduate.CareerMoves.Add(text);
                }
            }

            return graduate;
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
