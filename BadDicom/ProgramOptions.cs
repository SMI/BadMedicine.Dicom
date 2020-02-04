using BadMedicine.Dicom;
using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace BadDicom
{
       class ProgramOptions
    {
        [Value(0,HelpText = "Output directory to create CSV files in",Required=true)]
        public string OutputDirectory { get; set; }

        [Value(1, HelpText = "The number of unique patient identifiers to generate up front and then use in test data",Default = 500)]
        public int NumberOfPatients { get; set; } = 500;

        [Value(2, HelpText = "The number of dicom studies to generate (each study will have ", Default = 10)]
        public int NumberOfStudies { get; set; } = 10;

        [Option('s', "Seeds the random number generator with a specific number", Default = -1)]
        public int Seed { get; set; } = -1;

        [Value(3, HelpText = "Comma separated list of modalities to generate from", Default = "CT")]
        public string Modalities { get; set; } = "CT";
        
        [Option("NoPixels",HelpText= "Generate dicom files without pixel data (only tags).  This results in much smaller file sizes")]
        public bool NoPixels{get;set;}

        [Option("csv",HelpText= "Generate CSV files to be ingested in a database.  This results in no dicom images being generated (i.e. only csv tag data in flat files)")]
        public bool csv{get;set;}

        [Option('l',"Layout",HelpText= "The file system layout to use, defaults to Flat",Default = FileSystemLayout.StudyYearMonthDay)]
        public FileSystemLayout Layout{get;set;} = FileSystemLayout.StudyYearMonthDay;

        [Option('m',"MaxImages",HelpText= "The maximum number of images to generate (regardless of NumberOfStudies)",Default = int.MaxValue)]
        public int MaximumImages { get; set; } = int.MaxValue;

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return 
                    new Example("Generate test data",
                                new ProgramOptions { OutputDirectory = @"c:/temp" });

                yield return
                    new Example("Generate a custom amount of data", new ProgramOptions { OutputDirectory = @"c:/temp",NumberOfPatients = 5000, NumberOfStudies = 20000});
                    
            }
        }

}
}
