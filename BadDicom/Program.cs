using BadMedicine;
using BadMedicine.Dicom;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BadDicom.Configuration;
using FellowOakDicom;
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
        private static int _returnCode;
        public const string ConfigFile = "./BadDicom.yaml";

        public static int Main(string[] args)
        {
            _returnCode = 0;

            Parser.Default.ParseArguments<ProgramOptions>(args)
                .WithParsed(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed((errs) => HandleParseError(errs));


            return _returnCode;
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            // if user wants help then return exit code 0 otherwise return a failed to parse error code
            _returnCode = errs.Any(e => e.Tag == ErrorType.HelpRequestedError) ? 0 : 500;
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
                    _returnCode = -1;
                    return;
                }

                if (config.Database != null)
                {
                    try
                    {
                        _returnCode = RunDatabaseTarget(config.Database, opts);
                        return;
                    }
                    catch (Exception e)
                    {
                        
                        Console.WriteLine(e);
                        _returnCode = 3;
                        return;
                    }
                }
            }


            try
            {
                IPersonCollection identifiers = GetPeople(opts, out Random r);
                using var dicomGenerator = GetDataGenerator(opts, identifiers,r, out DirectoryInfo dir);
                Console.WriteLine($"{DateTime.Now} Starting file generation (to {dir?.FullName ?? "/dev/null"})" );
                var targetFile = new FileInfo(dir==null?(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "NUL" : "/dev/null") :Path.Combine(dir.FullName, "DicomFiles.csv"));
                dicomGenerator.GenerateTestDataFile(identifiers,targetFile,opts.NumberOfStudies);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _returnCode = 2;
                return;
            }

            Console.WriteLine($"{DateTime.Now} Finished" );

            _returnCode = 0;
        }

        private static DicomDataGenerator GetDataGenerator(ProgramOptions opts, IPersonCollection identifiers,Random r, out DirectoryInfo dir)
        {
            //Generate the dicom files (of the modalities that the user requested)
            string[] modalities = !string.IsNullOrWhiteSpace(opts.Modalities)? opts.Modalities.Split(",") :Array.Empty<string>();

            dir = opts.OutputDirectory.Equals("/dev/null",StringComparison.InvariantCulture) ? null : Directory.CreateDirectory(opts.OutputDirectory);
            return new(r, opts.OutputDirectory, modalities)
            {
                NoPixels = opts.NoPixels,
                Anonymise = opts.Anonymise,
                Layout = opts.Layout,
                MaximumImages = opts.MaximumImages,
                Csv = opts.csv,
            };
        }

        private static IPersonCollection GetPeople(ProgramOptions opts, out Random r)
        {
            r = opts.Seed == -1 ? new() : new Random(opts.Seed);

            //create a cohort of people
            IPersonCollection identifiers = new PersonCollection();
            identifiers.GeneratePeople(opts.NumberOfPatients,r);

            return identifiers;
        }

        private static int RunDatabaseTarget(TargetDatabase configDatabase, ProgramOptions opts)
        {
            var batchSize = Math.Max(1, configDatabase.Batches);

            //if we are going into a database we definitely do not need pixels!
            if (opts.NoPixels == false)
                opts.NoPixels = true;

            
            Stopwatch swTotal = new();

            swTotal.Start();

            string neverDistinct = "SOPInstanceUID";

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
                Console.WriteLine("Database Created");
            }
            else
            {
                Console.WriteLine($"Found Database '{db.GetRuntimeName()}'");
            }

            var creator = new ImagingTableCreation(db.Server.GetQuerySyntaxHelper());
            
            Console.WriteLine($"Image template contained schemas for {template.Tables.Count} tables.  Looking for existing tables..");
            
            //setting up bulk inserters
            DiscoveredTable[] tables = new DiscoveredTable[template.Tables.Count];
            DataTable[][] batches = new DataTable[batchSize][];

            for (var i = 0; i < batches.Length; i++) 
                batches[i] = new DataTable[template.Tables.Count];

            IBulkCopy[][] uploaders= new IBulkCopy[batchSize][];

            for (int i = 0; i < uploaders.Length; i++)
                uploaders[i] = new IBulkCopy[template.Tables.Count];

            string[] pks = new string[template.Tables.Count];

            for (var i = 0; i < template.Tables.Count; i++)
            {
                var tableSchema = template.Tables[i];
                var tbl = db.ExpectTable(tableSchema.TableName);
                tables[i] = tbl;

                if (configDatabase.MakeDistinct)
                {
                    var col = tableSchema.Columns.Where(c => c.IsPrimaryKey).ToArray();

                    if (col.Length > 1)
                        Console.WriteLine("MakeDistinct only works with single column primary keys e.g. StudyInstanceUID / SeriesInstanceUID");

                    pks[i] = col.SingleOrDefault()?.ColumnName;

                    if (pks[i] != null)
                    {
                        //if it is sop instance uid then we shouldn't be trying to deduplicate
                        if (string.Equals(pks[i], neverDistinct, StringComparison.CurrentCultureIgnoreCase))
                            pks[i] = null;
                        else
                        {
                            //we will make this a primary key later on
                            col.Single().IsPrimaryKey = false;
                            Console.WriteLine($"MakeDistinct will apply to '{pks[i]}' on '{tbl.GetFullyQualifiedName()}'");
                        }
                    }
                }

                bool create = true;

                if (tbl.Exists())
                {
                    if (configDatabase.DropTables)
                    {
                        Console.WriteLine($"Dropping existing table '{tbl.GetFullyQualifiedName()}'");
                        tbl.Drop();
                    }
                    else
                    {
                        Console.WriteLine($"Table '{tbl.GetFullyQualifiedName()}' already existed (so will not be created)");
                        create = false;
                    }
                }
                    
                if(create)
                {
                    Console.WriteLine($"About to create '{tbl.GetFullyQualifiedName()}'");
                    creator.CreateTable(tbl, tableSchema);
                    Console.WriteLine($"Successfully created create '{tbl.GetFullyQualifiedName()}'");
                }

                Console.WriteLine($"Creating uploader for '{tbl.GetRuntimeName()}''");

                for (int j = 0; j < batchSize; j++)
                {
                    //fetch schema
                    var dt = tbl.GetDataTable();
                    dt.Rows.Clear();

                    batches[j][i] = dt; 
                    uploaders[j][i] = tbl.BeginBulkInsert();
                }
            }
            var tasks = new Task[batchSize];
            
            IPersonCollection identifiers = GetPeople(opts, out Random r);

            for (int i = 0; i < batchSize; i++)
            {
                var batch = i;
                tasks[i] = new(() =>  // lgtm[cs/local-not-disposed]
                {
                    RunBatch(identifiers,opts,r,batches[batch],uploaders[batch]);

                });
                tasks[i].Start();
            }

            Task.WaitAll(tasks);
            
            swTotal.Stop();

            for (var i = 0; i < tables.Length; i++)
            {
                if(pks[i] == null)
                    continue;
                
                Console.WriteLine( $"{DateTime.Now} Making table '{tables[i]}' distinct (this may take a long time)");
                var tbl = tables[i];
                tbl.MakeDistinct(500000000);

                Console.WriteLine( $"{DateTime.Now} Creating primary key on '{tables[i]}' of '{pks[i]}'");
                tbl.CreatePrimaryKey(500000000,tbl.DiscoverColumn(pks[i]));
            }
            
            Console.WriteLine("Final Row Counts:");
            
            foreach (DiscoveredTable t in tables)
                Console.WriteLine($"{t.GetFullyQualifiedName()}: {t.GetRowCount():0,0}");

            Console.WriteLine($"Total Running Time:{swTotal.Elapsed}");
            return 0;
        }

        private static void RunBatch(IPersonCollection identifiers, ProgramOptions opts, Random r,DataTable[] batches, IBulkCopy[] uploaders)
        {
            Stopwatch swGeneration = new();
            Stopwatch swReading = new();
            Stopwatch swUploading = new();

            try
            {
                using var dicomGenerator = GetDataGenerator(opts, identifiers,r, out _);
                for (int i = 0; i < opts.NumberOfStudies; i++)
                {
                    swGeneration.Start();

                    var p = identifiers.People[r.Next(identifiers.People.Length)];
                    var ds = dicomGenerator.GenerateStudyImages(p,out Study s);
                        
                    swGeneration.Stop();

                    foreach (DicomDataset dataset in ds)
                    {
                        var rows = new DataRow[batches.Length];

                        for (int j = 0; j < batches.Length; j++) 
                            rows[j] = batches[j].NewRow();

                        swReading.Start();
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

                        swReading.Stop();
                    }

                    //every 100 and last batch
                    if (i % 100 == 0 || i == opts.NumberOfStudies - 1)
                    {
                        swUploading.Start();
                        for (var j = 0; j < uploaders.Length; j++)
                        {
                            uploaders[j].Upload(batches[j]);
                            batches[j].Rows.Clear();
                        }
                        swUploading.Stop();
                        Console.WriteLine($"{DateTime.Now} Done {i} studies");
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
            
            Console.WriteLine($"Total time Generating Dicoms:{swGeneration.Elapsed}");
            Console.WriteLine($"Total time Reading Dicoms:{swReading.Elapsed}");
            Console.WriteLine($"Total time Uploading Records:{swUploading.Elapsed}");

        }
    }
}
