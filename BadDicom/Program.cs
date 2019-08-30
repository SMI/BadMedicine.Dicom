using BadMedicine;
using BadMedicine.Datasets;
using BadMedicine.Dicom;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadDicom
{
      class Program
    {
        private static int returnCode;

        public static int Main(string[] args)
        {
            returnCode = 0;

            Parser.Default.ParseArguments<ProgramOptions>(args)
                .WithParsed<ProgramOptions>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<ProgramOptions>((errs) => HandleParseError(errs));


            return returnCode;
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            returnCode = 500;
        }

        private static void RunOptionsAndReturnExitCode(ProgramOptions opts)
        {

            if (opts.NumberOfPatients <= 0)
                opts.NumberOfPatients = 500;
            if (opts.NumberOfStudies <= 0)
                opts.NumberOfStudies = 2000;

            var dir = Directory.CreateDirectory(opts.OutputDirectory);

            try
            {
                Random r = opts.Seed == -1 ? new Random() : new Random(opts.Seed);

                //create a cohort of people
                IPersonCollection identifiers = new PersonCollection();
                identifiers.GeneratePeople(opts.NumberOfPatients,r);
                                               
                //Generate the dicom files (of the modalities that the user requested)
                string[] modalities = !string.IsNullOrWhiteSpace(opts.Modalities)? opts.Modalities.Split(",") :new string[0];

                var dicomGenerator = new DicomDataGenerator(r,dir,modalities)
                {
                    NoPixels = opts.NoPixels,
                    Layout = opts.Layout,
                    MaximumImages = opts.MaximumImages,
                    csv = opts.csv,
                };
               
                var targetFile = new FileInfo(Path.Combine(dir.FullName, "DicomFiles.csv"));
                dicomGenerator.GenerateTestDataFile(identifiers,targetFile,opts.NumberOfStudies);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                returnCode = 2;
                return;
            }

            returnCode = 0;
        }
    }
}
