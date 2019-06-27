using BadMedicine.Dicom;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace BadDicom
{
       class ProgramOptions
    {
        [Value(0,HelpText = "Output directory to create CSV files in",Required=true)]
        public string OutputDirectory { get; set; }

        [Value(1, HelpText = "The number of unique patient identifiers to generate up front and then use in test data",Default = 500)]
        public int NumberOfPatients { get; set; } = 500;

        [Value(2, HelpText = "The number of dicom files to generate", Default = 2000)]
        public int NumberOfRows { get; set; } = 2000;

        [Option('s', "Seeds the random number generator with a specific number", Default = -1)]
        public int Seed { get; set; } = -1;

        [Value(3, HelpText = "Comma separated list of modalities to generate from", Default = "CT")]
        public string Modalities { get; set; } = "CT";
        
        [Option('i', HelpText= "True to also generate EHR records for the patients for whome dicom images are being generated (biochemistry, prescribing etc)", Default = false)]
        public bool IncludeEhrDatasets{ get; set; }

        [Option('d',HelpText= "Only applies if IncludeEhrDatasets is true.  The dataset to generate, must be a class name e.g. 'TestDemography'.  If this option is not specified then all EHR datasets will be generated")]
        public string Dataset{get; set; }

        [Option("NoPixels",HelpText= "Generate dicom files without pixel data (only tags).  This results in much smaller file sizes")]
        public bool NoPixels{get;set;}

        [Option('l',"Layout",HelpText= "The file system layout to use, defaults to Flat",Default = FileSystemLayout.Flat)]
        public FileSystemLayout Layout{get;set;} = FileSystemLayout.Flat;

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return 
                    new Example("Generate test data",
                                new ProgramOptions { OutputDirectory = @"c:/temp" });

                yield return
                    new Example("Generate a custom amount of data", new ProgramOptions { OutputDirectory = @"c:/temp",NumberOfPatients = 5000, NumberOfRows = 20000});
                    
            }
        }

}
}
