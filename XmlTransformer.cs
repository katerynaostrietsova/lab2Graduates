using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace lab2Graduates.Services
{
    public class XmlTransformer
    {
        public void TransformToHtml(string xmlPath, string xslPath, string outputHtmlPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
                throw new ArgumentException("Шлях до XML-файлу не може бути порожнім.", nameof(xmlPath));

            if (string.IsNullOrWhiteSpace(xslPath))
                throw new ArgumentException("Шлях до XSL-файлу не може бути порожнім.", nameof(xslPath));

            if (string.IsNullOrWhiteSpace(outputHtmlPath))
                throw new ArgumentException("Шлях до вихідного HTML-файлу не може бути порожнім.", nameof(outputHtmlPath));

            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("XML-файл не знайдено.", xmlPath);

            if (!File.Exists(xslPath))
                throw new FileNotFoundException("XSL-файл не знайдено.", xslPath);

            var transform = new XslCompiledTransform();

            transform.Load(xslPath);

            using var xmlReader = XmlReader.Create(xmlPath);

            using var writer = new StreamWriter(outputHtmlPath, false, Encoding.UTF8);

            transform.Transform(xmlReader, null, writer);
        }

        public string TransformToHtmlString(string xmlPath, string xslPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
                throw new ArgumentException("Шлях до XML-файлу не може бути порожнім.", nameof(xmlPath));

            if (string.IsNullOrWhiteSpace(xslPath))
                throw new ArgumentException("Шлях до XSL-файлу не може бути порожнім.", nameof(xslPath));

            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("XML-файл не знайдено.", xmlPath);

            if (!File.Exists(xslPath))
                throw new FileNotFoundException("XSL-файл не знайдено.", xslPath);

            var transform = new XslCompiledTransform();
            transform.Load(xslPath);

            using var xmlReader = XmlReader.Create(xmlPath);
            using var stringWriter = new StringWriter();

            transform.Transform(xmlReader, null, stringWriter);

            return stringWriter.ToString();
        }
    }
}
