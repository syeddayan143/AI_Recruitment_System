using System.IO.Compression;
using System.Xml.Linq;
using UglyToad.PdfPig;

namespace AIRecruitmentSystem.Services
{
    public class ResumeTextExtractor
    {
        public async Task<string> ExtractTextAsync(
            string filePath,
            string extension)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "File path is required.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The resume file could not be found.",
                    filePath);
            }

            extension = extension.ToLowerInvariant();

            if (extension == ".pdf")
            {
                return await Task.Run(() =>
                {
                    using var document = PdfDocument.Open(filePath);

                    var pageTexts = document.GetPages()
                        .Select(page => page.Text?.Trim() ?? string.Empty)
                        .Where(text => !string.IsNullOrWhiteSpace(text));

                    return string.Join(
                        Environment.NewLine + Environment.NewLine,
                        pageTexts);
                });
            }

            if (extension == ".docx")
            {
                return await Task.Run(() =>
                {
                    using var archive = ZipFile.OpenRead(filePath);

                    var documentEntry =
                        archive.GetEntry("word/document.xml");

                    if (documentEntry == null)
                    {
                        return string.Empty;
                    }

                    using var stream = documentEntry.Open();
                    var xml = XDocument.Load(stream);

                    XNamespace word =
                        "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

                    var paragraphs = xml
                        .Descendants(word + "p")
                        .Select(paragraph =>
                        {
                            var parts = paragraph
                                .Descendants()
                                .Where(element =>
                                    element.Name == word + "t"
                                    || element.Name == word + "tab"
                                    || element.Name == word + "br"
                                    || element.Name == word + "cr")
                                .Select(element =>
                                {
                                    if (element.Name == word + "t")
                                    {
                                        return element.Value;
                                    }

                                    if (element.Name == word + "tab")
                                    {
                                        return " ";
                                    }

                                    return Environment.NewLine;
                                });

                            return string.Concat(parts).Trim();
                        })
                        .Where(text => !string.IsNullOrWhiteSpace(text));

                    return string.Join(
                        Environment.NewLine,
                        paragraphs);
                });
            }

            throw new InvalidOperationException(
                "Only PDF and DOCX files are supported.");
        }
    }
}