using BadMedicine;
using BadMedicine.Datasets;
using BadMedicine.Dicom;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BadDicom.Configuration;
using Dicom;
using DicomTypeTranslation;
using DicomTypeTranslation.TableCreation;
using FAnsi.Discovery;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using YamlDotNet.Serialization;

namespace BadDicom
{
      class Program
    {
        private static int returnCode;
        public const string ConfigFile = "./BadDicom.yaml";

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


            if(File.Exists(ConfigFile))
            {
                Config config;

                try
                {
                    var d = new Deserializer();
                    config = d.Deserialize<Config>(File.ReadAllText(ConfigFile));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error deserializing '{ConfigFile}'");
                    Console.Write(e.ToString());
                    returnCode = -1;
                    return;
                }

                if (config.Database != null)
                {
                    try
                    {
                        returnCode = RunDatabaseTarget(config.Database, opts);
                        return;
                    }
                    catch (Exception e)
                    {
                        
                        Console.WriteLine(e);
                        returnCode = 3;
                        return;
                    }
                }
            }


            try
            {
                IPersonCollection identifiers = GetPeople(opts, out Random r);
                using(var dicomGenerator = GetDataGenerator(opts, identifiers,r, out DirectoryInfo dir))
                {
                    var targetFile = new FileInfo(Path.Combine(dir.FullName, "DicomFiles.csv"));
                    dicomGenerator.GenerateTestDataFile(identifiers,targetFile,opts.NumberOfStudies);
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

        private static DicomDataGenerator GetDataGenerator(ProgramOptions opts, IPersonCollection identifiers,Random r, out DirectoryInfo dir)
        {
            dir = Directory.CreateDirectory(opts.OutputDirectory);

            //Generate the dicom files (of the modalities that the user requested)
            string[] modalities = !string.IsNullOrWhiteSpace(opts.Modalities)? opts.Modalities.Split(",") :new string[0];

            return new DicomDataGenerator(r, dir, modalities)
            {
                NoPixels = opts.NoPixels,
                Layout = opts.Layout,
                MaximumImages = opts.MaximumImages,
                Csv = opts.csv,
            };
        }

        private static IPersonCollection GetPeople(ProgramOptions opts, out Random r)
        {
            r = opts.Seed == -1 ? new Random() : new Random(opts.Seed);

            //create a cohort of people
            IPersonCollection identifiers = new PersonCollection();
            identifiers.GeneratePeople(opts.NumberOfPatients,r);

            return identifiers;
        }

        private static int RunDatabaseTarget(TargetDatabase configDatabase, ProgramOptions opts)
        {

            if (!File.Exists(configDatabase.Template))
            {
                Console.WriteLine($"Listed template file '{configDatabase.Template}' does not exist");
                return -1;
            }

            ImageTableTemplateCollection template;
            try
            {
                template = ImageTableTemplateCollection.LoadFrom(File.ReadAllText(configDatabase.Template));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading yaml from '{configDatabase.Template}'");
                Console.WriteLine(e.ToString());
                return -2;
            }

            ImplementationManager.Load<MySqlImplementation>();
            ImplementationManager.Load<PostgreSqlImplementation>();
            ImplementationManager.Load<OracleImplementation>();
            ImplementationManager.Load<MicrosoftSQLImplementation>();

            var server = new DiscoveredServer(configDatabase.ConnectionString, configDatabase.DatabaseType);
           
            try
            {
                server.TestConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not reach target server '{server.Name}'");
                Console.WriteLine(e);
                return -2;
            }

            
            var db = server.ExpectDatabase(configDatabase.DatabaseName);

            if (!db.Exists())
            {
                Console.WriteLine($"Creating Database '{db.GetRuntimeName()}'");
                db.Create();
                Console.WriteLine($"Database Created");
            }
            else
            {
                Console.WriteLine($"Found Database '{db.GetRuntimeName()}'");
            }

            var creator = new ImagingTableCreation(db.Server.GetQuerySyntaxHelper());
            
            Console.WriteLine($"Image template contained '{template.Tables.Count}'.  Looking for existing tables..");
            
            //setting up bulk inserters
            
            DataTable[] batches = new DataTable[template.Tables.Count];
            IBulkCopy[] uploaders= new IBulkCopy[template.Tables.Count];

            for (var i = 0; i < template.Tables.Count; i++)
            {
                var tableSchema = template.Tables[i];
                var tbl = db.ExpectTable(tableSchema.TableName);

                if (tbl.Exists())
                    Console.WriteLine(
                        $"Table '{tbl.GetFullyQualifiedName()}' already existed (so will not be created)");
                else
                {
                    Console.WriteLine($"About to create '{tbl.GetFullyQualifiedName()}'");
                    creator.CreateTable(tbl, tableSchema);
                    Console.WriteLine($"Successfully created create '{tbl.GetFullyQualifiedName()}'");
                }

                Console.WriteLine($"Creating uploader for '{tbl.GetRuntimeName()}''");

                //fetch schema
                var dt = tbl.GetDataTable();
                dt.Rows.Clear();

                batches[i] = dt; 
                uploaders[i] = tbl.BeginBulkInsert();
            }

            try
            {
                IPersonCollection identifiers = GetPeople(opts, out Random r);
                using(var dicomGenerator = GetDataGenerator(opts, identifiers,r, out _))
                {
                    for (int i = 0; i < opts.NumberOfStudies; i++)
                    {
                        var p = identifiers.People[r.Next(identifiers.People.Length)];
                        var ds = dicomGenerator.GenerateStudyImages(p,out Study s);

                        foreach (DicomDataset dataset in ds)
                        {
                            var rows = new DataRow[batches.Length];

                            for (int j = 0; j < batches.Length; j++) 
                                rows[j] = batches[j].NewRow();

                            foreach (DicomItem item in dataset)
                            {
                                var column = DicomTypeTranslaterReader.GetColumnNameForTag(item.Tag, false);
                                var value = DicomTypeTranslater.Flatten(DicomTypeTranslaterReader.GetCSharpValue(dataset, item));

                                foreach (DataRow row in rows)
                                {
                                    if (row.Table.Columns.Contains(column))
                                        row[column] = value ?? DBNull.Value;
                                }
                            }

                            for (int j = 0; j < batches.Length; j++)
                                batches[j].Rows.Add(rows[j]);
                        }

                        //every 100 and last batch
                        if(i % 100 == 0 || i == opts.NumberOfStudies -1)
                            for (var j = 0; j < uploaders.Length; j++)
                            {
                                uploaders[j].Upload(batches[j]);
                                batches[j].Rows.Clear();
                                Console.WriteLine($"Done {j} rows");
                            }
                    }
                }
            }
            finally
            {
                for (var i = 0; i < uploaders.Length; i++)
                {
                    uploaders[i].Dispose();
                    batches[i].Dispose();
                }
            }
            
            return 0;
        }
    }
}
