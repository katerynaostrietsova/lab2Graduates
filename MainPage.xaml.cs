using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using lab2Graduates.Models;
using lab2Graduates.Services;
using lab2Graduates.Strategies;

namespace lab2Graduates;

public partial class MainPage : ContentPage
{
    private readonly List<IXmlSearchStrategy> _strategies;
    private readonly XmlTransformer _xmlTransformer = new();

    private string? _xmlPath;
    private string? _xslPath;

    // Назви атрибутів, які можна обирати в UI
    private readonly Dictionary<string, string> _attributeDisplayNames =
        new()
        {
            { "fullName", "ПІБ (fullName)" },
            { "faculty", "Факультет (faculty)" },
            { "department", "Кафедра (department)" },
            { "speciality", "Спеціальність (speciality)" },
            { "group", "Група (group)" },
            { "admissionYear", "Рік вступу (admissionYear)" },
            { "graduationYear", "Рік закінчення (graduationYear)" }
        };

    // Значення атрибутів, зчитані з XML (для підгрузки в Picker)
    private readonly Dictionary<string, HashSet<string>> _attributeValues =
        new();

    public MainPage()
    {
        InitializeComponent();

        _strategies = new List<IXmlSearchStrategy>
        {
            new SaxSearchStrategy(),
            new DomSearchStrategy(),
            new LinqSearchStrategy()
        };

        InitStrategyPicker();
    }

    // ------------ Стратегії ------------

    private void InitStrategyPicker()
    {
        StrategyPicker.Items.Clear();

        foreach (var strategy in _strategies)
        {
            StrategyPicker.Items.Add(strategy.Name);
        }

        if (_strategies.Count > 0)
        {
            StrategyPicker.SelectedIndex = 0;
        }
    }

    private IXmlSearchStrategy? GetSelectedStrategy()
    {
        if (StrategyPicker.SelectedIndex < 0)
            return null;

        var name = StrategyPicker.Items[StrategyPicker.SelectedIndex];
        return _strategies.FirstOrDefault(s => s.Name == name);
    }

    // ------------ Вибір XML ------------

    private async void OnLoadXmlClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XML-файл випускників"
            });

            if (result is null)
                return;

            _xmlPath = result.FullPath;
            XmlFileLabel.Text = Path.GetFileName(_xmlPath);

            LoadAttributeValuesFromXml(_xmlPath);
            InitAttributePicker();
            InitAttributeValuePicker(null);

            ResultsEditor.Text =
                $"XML-файл успішно завантажено: {Path.GetFileName(_xmlPath)}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка",
                $"Не вдалося завантажити XML-файл:\n{ex.Message}", "OK");
        }
    }

    // зчитує всі різні значення атрибутів із XML-файлу
    private void LoadAttributeValuesFromXml(string xmlPath)
    {
        _attributeValues.Clear();

        var doc = XDocument.Load(xmlPath);
        var graduates = doc.Descendants("graduate");

        foreach (var attrKey in _attributeDisplayNames.Keys)
        {
            var values = graduates
                .Select(g => (string?)g.Attribute(attrKey))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim())
                .Distinct()
                .ToList();

            if (values.Count > 0)
            {
                _attributeValues[attrKey] = new HashSet<string>(values);
            }
        }
    }

    // ------------ Атрибут + значення ------------

    // Заповнюємо Picker назвами атрибутів, які реально є в XML
    private void InitAttributePicker()
    {
        AttributePicker.Items.Clear();

        foreach (var kvp in _attributeDisplayNames)
        {
            if (_attributeValues.ContainsKey(kvp.Key))
            {
                AttributePicker.Items.Add(kvp.Value);
            }
        }

        AttributePicker.SelectedIndexChanged += OnAttributePickerSelectedIndexChanged;
    }

    // коли обрали атрибут – перезаливаємо список його значень
    private void OnAttributePickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        string? attrKey = GetSelectedAttributeKey();
        InitAttributeValuePicker(attrKey);
    }

    private void InitAttributeValuePicker(string? attributeKey)
    {
        AttributeValuePicker.Items.Clear();

        if (attributeKey is null)
        {
            AttributeValuePicker.SelectedIndex = -1;
            return;
        }

        if (_attributeValues.TryGetValue(attributeKey, out var values))
        {
            foreach (var value in values.OrderBy(v => v))
            {
                AttributeValuePicker.Items.Add(value);
            }

            if (AttributeValuePicker.Items.Count > 0)
            {
                AttributeValuePicker.SelectedIndex = 0;
            }
        }
    }

    // повертає внутрішній ключ атрибуту ("faculty", "group" тощо)
    private string? GetSelectedAttributeKey()
    {
        if (AttributePicker.SelectedIndex < 0)
            return null;

        var display = AttributePicker.Items[AttributePicker.SelectedIndex];

        foreach (var kvp in _attributeDisplayNames)
        {
            if (kvp.Value == display)
            {
                return kvp.Key;
            }
        }

        return null;
    }

    // ------------ Вибір XSL ------------

    private async void OnLoadXslClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XSL-файл для трансформації"
            });

            if (result is null)
                return;

            _xslPath = result.FullPath;

            await DisplayAlert(
                "XSL завантажено",
                $"XSL-файл:\n{Path.GetFileName(_xslPath)}",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка",
                $"Не вдалося завантажити XSL-файл:\n{ex.Message}", "OK");
        }
    }

    // ------------ Пошук ------------

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_xmlPath))
        {
            await DisplayAlert("Увага", "Спочатку оберіть XML-файл.", "OK");
            return;
        }

        var strategy = GetSelectedStrategy();
        if (strategy is null)
        {
            await DisplayAlert("Увага",
                "Оберіть стратегію пошуку (SAX/DOM/LINQ).", "OK");
            return;
        }

        string? attributeKey = GetSelectedAttributeKey();
        string? attributeValue = null;

        if (AttributeValuePicker.SelectedIndex >= 0)
        {
            attributeValue = AttributeValuePicker
                .Items[AttributeValuePicker.SelectedIndex];
        }

        string? keyword = string.IsNullOrWhiteSpace(KeywordEntry.Text)
            ? null
            : KeywordEntry.Text;

        try
        {
            var graduates = strategy.Search(
                _xmlPath,
                attributeKey,
                attributeValue,
                keyword);

            if (graduates.Count == 0)
            {
                ResultsEditor.Text =
                    "За заданими критеріями випускників не знайдено.";
            }
            else
            {
                var lines = graduates.Select(g => g.ToString());
                ResultsEditor.Text =
                    string.Join(Environment.NewLine + Environment.NewLine, lines);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка",
                $"Під час пошуку сталася помилка:\n{ex.Message}", "OK");
        }
    }

    // ------------ Трансформація в HTML ------------

    private async void OnTransformClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_xmlPath))
        {
            await DisplayAlert("Увага", "Спочатку оберіть XML-файл.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(_xslPath))
        {
            await DisplayAlert("Увага", "Спочатку оберіть XSL-файл.", "OK");
            return;
        }

        try
        {
            var outputFileName = "graduates.html";
            var folder = FileSystem.AppDataDirectory;
            var outputPath = Path.Combine(folder, outputFileName);

            _xmlTransformer.TransformToHtml(_xmlPath, _xslPath, outputPath);

            await DisplayAlert(
                "Готово",
                $"HTML-файл успішно згенеровано:\n{outputPath}",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка",
                $"Не вдалося виконати трансформацію:\n{ex.Message}", "OK");
        }
    }

    // ------------ Очистити ------------

    private void OnClearClicked(object sender, EventArgs e)
    {
        KeywordEntry.Text = string.Empty;
        ResultsEditor.Text = string.Empty;

        AttributePicker.SelectedIndex = -1;
        AttributeValuePicker.Items.Clear();
    }

    // ------------ Вийти ------------

    private async void OnExitClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert(
            "Вихід",
            "Чи дійсно ви хочете завершити роботу з програмою?",
            "Так",
            "Ні");

        if (answer)
        {
            Environment.Exit(0);
        }
    }
}
