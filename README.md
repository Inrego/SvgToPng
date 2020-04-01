# SvgToPng
A console app to convert an .svg file to multiple png. For example to convert an SVG icon to multiple density versions for Android/iOS/UWP apps.
Color transformations are also supported, to more easily recolor icons found on the web, for the theme of your app.

## Prerequisites
You need .NET Core 3.1 to run this app. [Download here](https://dotnet.microsoft.com/download).

Besides console parameters, a config file is required to define details of the output files (dimensions, color and file name).
Look for `sampleprofile.json`in the repo for an sample config file.

## Usage
### Command line parameters
```
SvgToPng:
  Converts svg files to png images.

Usage:
  SvgToPng [options]

Options:
  -f, --inputFile <inputfile> (REQUIRED)            The relative or absolute path to the input file or directory.
  -o, --outputDir <outputdir> (REQUIRED)            The relative or absolute path to the output files.
  -c, --profileConfig <profileconfig> (REQUIRED)    The relative or absolute path to the profile config file.
  --version                                         Show version information
  -?, -h, --help                                    Show help and usage information
```

### Example
Convert all .svg files found in `C:/InputFiles/` and save all output files in `C:/OutputFiles/`
```
./SvgToPng -f C:/InputFiles/ -c C:/InputFiles/sampleprofile.json -o C:/OutputFiles/
```
Convert `C:/InputFiles/icon.svg` and save all output files in `C:/OutputFiles/`
```
./SvgToPng -f C:/InputFiles/ -c C:/InputFiles/sampleprofile.json -o C:/OutputFiles/
```