using Dicom;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace BadMedicine.Dicom.Tests
{
    public class DicomDataGeneratorTests
    {
        [Test]
        public void Test_CreatingOnDisk_OneFile()
        {
            var r = new Random(500);
            var root = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var generator = new DicomDataGenerator(r,root);
            generator.Layout = FileSystemLayout.StudyUID;
            generator.MaximumImages = 1; 

            var person = new Person(r);

            //generates a study but because of maximum images 1 we should only get 1 image being generated
            string studyUid = (string)generator.GenerateTestDataRow(person)[0];
            
            //should be a directory named after the Study UID
            Assert.IsTrue(Directory.Exists(Path.Combine(root.FullName,studyUid)));
            
            //should be a single file
            var f = new FileInfo(Directory.GetFiles(Path.Combine(root.FullName,studyUid)).Single());
            Assert.IsTrue(f.Exists);

            var datasetCreated = DicomFile.Open(f.FullName);
            
            Assert.AreEqual(studyUid,
            datasetCreated.Dataset.GetValues<DicomUID>(DicomTag.StudyInstanceUID)[0].UID,
            "UID in the dicom file generated did not match the one output into the CSV inventory file"
            );
            
            Console.WriteLine("Created file "+ f.FullName);

            generator.Dispose();
        }

        
        [Test]
        public void ExampleUsage()
        { 
            //create a test person
            var r = new Random(23);
            var person = new Person(r);

            //create a generator 
            using (var generator = new DicomDataGenerator(r, null, "CT"))
            {
                //create a dataset in memory
                DicomDataset dataset = generator.GenerateTestDataset(person, r);

                //values should match the patient details
                Assert.AreEqual(person.CHI,dataset.GetValue<string>(DicomTag.PatientID,0));
                Assert.GreaterOrEqual(dataset.GetValue<DateTime>(DicomTag.StudyDate,0),person.DateOfBirth);

                //should have a study description
                Assert.IsNotNull(dataset.GetValue<string>(DicomTag.StudyDescription,0));
                //should have a study description
                Assert.IsNotNull(dataset.GetSingleValue<DateTime>(DicomTag.StudyTime).TimeOfDay);
            }
        }

        [Test]
        public void Test_CreatingInMemory_ModalityCT()
        {
            var r = new Random(23);
            var person = new Person(r);

            var generator = new DicomDataGenerator(r,new DirectoryInfo(TestContext.CurrentContext.WorkDirectory),"CT");
            
            //generate 100 images
            for(int i = 0 ; i < 100 ; i++)
            {
                //all should be CT because we said CT only
                var ds = generator.GenerateTestDataset(person, r);
                Assert.AreEqual("CT",ds.GetSingleValue<string>(DicomTag.Modality));
            }

            generator.Dispose();
            
        }
        [Test]
        public void Test_Anonymise()
        {
            var r = new Random(23);
            var person = new Person(r);

            var generator = new DicomDataGenerator(r,new DirectoryInfo(TestContext.CurrentContext.WorkDirectory),"CT");
            
            // without anonymisation (default) we get the normal patient ID
            var ds = generator.GenerateTestDataset(person, r);
            
            Assert.IsTrue(ds.Contains(DicomTag.PatientID));
            Assert.AreEqual(person.CHI,ds.GetValue<string>(DicomTag.PatientID,0));
            
            // with anonymisation
            generator.Anonymise = true;
            
            var ds2 = generator.GenerateTestDataset(person, r);

            // we get a blank patient ID
            Assert.IsTrue(ds.Contains(DicomTag.PatientID));
            Assert.AreEqual(string.Empty,ds2.GetString(DicomTag.PatientID));

            generator.Dispose();
            
        }
        [Test]
        public void Test_CreatingInMemory_Modality_CTAndMR()
        {
            var r = new Random(23);
            var person = new Person(r);

            var generator = new DicomDataGenerator(r,new DirectoryInfo(TestContext.CurrentContext.WorkDirectory),"CT","MR");
            
            //generate 100 images
            for(int i = 0 ; i < 100 ; i++)
            {
                //all should be CT because we said CT only
                var ds = generator.GenerateTestDataset(person, r);
                var modality = ds.GetSingleValue<string>(DicomTag.Modality);

                Assert.IsTrue(modality == "CT" || modality == "MR","Unexpected modality {0}",modality);
            }

            generator.Dispose();
        }

        [Test]
        public void TestFail_CreatingInMemory_Modality_Unknown()
        {
            var r = new Random(23);
            Assert.Throws<ArgumentException>(()=>new DicomDataGenerator(r,new DirectoryInfo(TestContext.CurrentContext.WorkDirectory),"LOLZ"));

        }

        [Test]
        public void Test_CsvOption()
        {
            var r = new Random(500);

            var outputDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestCsv"));
            outputDir.Create();

            var people = new PersonCollection();
            people.GeneratePeople(100,r);

            using (var generator = new DicomDataGenerator(r,outputDir, "CT"))
            {
                generator.Csv = true;
                generator.NoPixels = true;
                generator.MaximumImages = 500;

                generator.GenerateTestDataFile(people,new FileInfo(Path.Combine(outputDir.FullName,"index.csv")),500);
            }

            //3 csv files + index.csv (the default one
            Assert.AreEqual(4,outputDir.GetFiles().Length);

            foreach (FileInfo f in outputDir.GetFiles())
            {
                using(var reader = new CsvReader(new StreamReader(f.FullName),CultureInfo.CurrentCulture))
                {
                    int rowcount = 0;

                    //confirms that the CSV is intact (no dodgy commas, unquoted newlines etc)
                    while (reader.Read())
                        rowcount++;

                    //should be 1 row per image + 1 for header
                    if(f.Name == DicomDataGenerator.ImageCsvFilename)
                        Assert.AreEqual(501,rowcount);
                }

            }
        }
    }
}
