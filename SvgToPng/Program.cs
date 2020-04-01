using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Svg;
using SvgToPng.Config;

namespace SvgToPng
{
    class Program
    {
        private static Params _params;
        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand()
            {
                Description = "Converts svg files to png images."
            };
            rootCommand.AddOption(new Option(new[] { "--inputFile", "-f" }, "The relative or absolute path to the input file or directory.")
            {
                Required = true,
                Argument = new Argument<string>()
            });
            rootCommand.AddOption(new Option(new[] { "--outputDir", "-o" }, "The relative or absolute path to the output files.")
            {
                Required = true,
                Argument = new Argument<DirectoryInfo>(),
                Name = "OutputDirectory"
            });
            rootCommand.AddOption(new Option(new[] { "--profileConfig", "-c" }, "The relative or absolute path to the profile config file.")
            {
                Required = true,
                Argument = new Argument<FileInfo>()
            });

            rootCommand.Handler = CommandHandler.Create(async (Params parameters) =>
            {
                _params = parameters;
                DirectoryInfo inputDir = null;
                FileInfo inputFile = null;
                if (Directory.Exists(parameters.InputFile))
                {
                    inputDir = new DirectoryInfo(parameters.InputFile);
                }
                else if (File.Exists(parameters.InputFile))
                {
                    inputFile = new FileInfo(parameters.InputFile);
                }
                else
                {
                    Console.WriteLine("[ERROR]: Input path not found: {0}", parameters.InputFile);
                    return;
                }

                if (!parameters.OutputDirectory.Exists)
                {
                    Console.WriteLine("[ERROR]: Output directory not found: {0}", parameters.OutputDirectory);
                    return;
                }
                if (!parameters.ProfileConfig.Exists)
                {
                    Console.WriteLine("[ERROR]: Profile config file not found: {0}", parameters.ProfileConfig);
                    return;
                }

                ConversionProfile[] profiles;
                try
                {
                    using (var fs = parameters.ProfileConfig.OpenRead())
                    {
                        profiles = await JsonSerializer.DeserializeAsync<ConversionProfile[]>(fs);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR]: Invalid profile config file.");
                    return;
                }

                if (inputDir != null)
                {
                    var files = inputDir.GetFiles("*.svg");
                    await Task.WhenAll(files.Select(x => ProcessInputFile(x, profiles)));
                }
                else
                {
                    await ProcessInputFile(inputFile, profiles);
                }
            });
            await rootCommand.InvokeAsync(args);
        }

        private static async Task ProcessInputFile(FileInfo inputFile, ConversionProfile[] profiles)
        {
            var svgContent = await inputFile.OpenText().ReadToEndAsync();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(svgContent);
                var withoutNs = RemoveAllNamespaces(xmlDoc.DocumentElement);
                svgContent = await ToString(withoutNs);
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR]: Invalid svg file.");
                return;
            }

            var inputFileName = Path.GetFileNameWithoutExtension(inputFile.Name);
            foreach (var profile in profiles)
            {
                try
                {
                    await ProcessProfile(profile, svgContent, inputFileName);
                }
                catch (InvalidXPathException e)
                {
                    Console.WriteLine("[ERROR]: Invalid XPath: {0}", e.XPath);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR]: An error occurred: {0}", e);
                    return;
                }
            }
        }

        private static async Task ProcessProfile(ConversionProfile profile, string svgContent, string inputFileName)
        {
            var svg = new XmlDocument();
            svg.LoadXml(svgContent);
            if (profile.ColorConversions != null && profile.ColorConversions.Any())
            {
                ApplyColorConversions(svg, profile.ColorConversions);
            }

            svgContent = await ToString(svg);
            await Task.WhenAll(profile.Output.Select(x => ProcessOutput(x, svgContent, inputFileName)));
        }

        private static async Task ProcessOutput(Output output, string svgContent, string inputFileName)
        {
            var svg = new XmlDocument();
            svg.LoadXml(svgContent);
            if (output.ColorConversions != null && output.ColorConversions.Any())
            {
                ApplyColorConversions(svg, output.ColorConversions);
            }

            var svgDoc = SvgDocument.Open(svg);
            if (svgDoc == null)
                throw new Exception("Failed to load SVG.");

            if (output.Width.HasValue)
                svgDoc.Width = output.Width.Value;

            if (output.Height.HasValue)
                svgDoc.Height = output.Height.Value;

            var fileName = output.Path.Replace("{name}", inputFileName);
            fileName = Path.Combine(_params.OutputDirectory.FullName, fileName);
            var dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var bitmap = svgDoc.Draw())
            {
                bitmap.Save(fileName);
            }
        }

        private static void ApplyColorConversions(XmlDocument svg, ColorConversion[] colorConversions)
        {
            foreach (var colorConversion in colorConversions)
            {
                XmlNodeList nodes;
                try
                {
                    nodes = svg.SelectNodes(colorConversion.XPath);
                }
                catch (Exception e)
                {
                    throw new InvalidXPathException(colorConversion.XPath);
                }
                if (nodes.Count == 0)
                {
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    var attr = node.Attributes["fill"];
                    if (attr != null)
                    {
                        attr.Value = colorConversion.Color;
                    }
                    else
                    {
                        attr = svg.CreateAttribute("fill");
                        attr.Value = colorConversion.Color;
                        node.Attributes.SetNamedItem(attr);
                    }
                }
            }
        }

        private static async Task<string> ToString(XmlNode xmlDoc)
        {
            using (var sw = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, OmitXmlDeclaration = true }))
                {
                    xmlDoc.WriteTo(xmlWriter);
                    await xmlWriter.FlushAsync();
                    return sw.GetStringBuilder().ToString();
                }
            }
        }

        public static XmlNode RemoveAllNamespaces(XmlNode documentElement)
        {
            var xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
            var outerXml = documentElement.OuterXml;
            var matchCol = Regex.Matches(outerXml, xmlnsPattern);
            foreach (var match in matchCol)
                outerXml = outerXml.Replace(match.ToString(), "");

            var result = new XmlDocument();
            result.LoadXml(outerXml);

            return result;
        }
    }
}
