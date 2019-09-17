using Dicom;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

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
            var generator = new DicomDataGenerator(r,null,"CT");
            
            //create a dataset in memory
            DicomDataset dataset = generator.GenerateTestDataset(person);

            //values should match the patient details
            Assert.AreEqual(person.CHI,dataset.GetValue<string>(DicomTag.PatientID,0));
            Assert.GreaterOrEqual(dataset.GetValue<DateTime>(DicomTag.StudyDate,0),person.DateOfBirth);

            //should have a study description
            Assert.IsNotNull(dataset.GetValue<string>(DicomTag.StudyDescription,0));
            //should have a study description
            Assert.IsNotNull(dataset.GetSingleValue<DateTime>(DicomTag.StudyTime).TimeOfDay);
            
            generator.Dispose();
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
                var ds = generator.GenerateTestDataset(person);
                Assert.AreEqual("CT",ds.GetSingleValue<string>(DicomTag.Modality));
            }

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
                var ds = generator.GenerateTestDataset(person);
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
    }
}
