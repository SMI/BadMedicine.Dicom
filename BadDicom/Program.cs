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
            if (opts.NumberOfRows <= 0)
                opts.NumberOfRows = 2000;

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
                    NoPixels = opts.NoPixels ,
                    Layout = opts.Layout,
                };
                
                var targetFile = new FileInfo(Path.Combine(dir.FullName, "DicomFiles.csv"));

                dicomGenerator.GenerateTestDataFile(identifiers,targetFile,opts.NumberOfRows);

                //if they also want EHR records for these patients generate those too (uses base BadMedicine code)
                if (opts.IncludeEhrDatasets)
                {
                    var factory = new DataGeneratorFactory();
                    var generators = factory.GetAvailableGenerators().ToList();
            
                    //if the user only wants to extract a single dataset
                    if(!string.IsNullOrEmpty(opts.Dataset))
                    {
                        var match = generators.FirstOrDefault(g => g.Name.Equals(opts.Dataset));
                        if(match == null)
                        {
                            Console.WriteLine("Could not find dataset called '" + opts.Dataset + "'");
                            Console.WriteLine("Generators found were:" + Environment.NewLine + string.Join(Environment.NewLine,generators.Select(g=>g.Name)));
                            returnCode = 2;
                            return;
                        }

                        generators = new List<Type>(new []{match});
                    }
                    
                    //for each generator
                    foreach (var g in generators)
                    {
                        var instance = factory.Create(g,r);

                        targetFile = new FileInfo(Path.Combine(dir.FullName, g.Name + ".csv"));
                        instance.GenerateTestDataFile(identifiers,targetFile,opts.NumberOfRows);
                    }
                }
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
