using Dicom;
using NUnit.Framework;
using System;
using System.IO;

namespace BadMedicine.Dicom.Tests
{
    public class DicomDataGeneratorTests
    {
        [Test]
        public void Test_CreatingOnDisk_OneFile()
        {
            var r = new Random(500);
            var generator = new DicomDataGenerator(r,new DirectoryInfo(TestContext.CurrentContext.WorkDirectory));
            
            var person = new Person(r);
            string fileName = (string)generator.GenerateTestDataRow(person)[0];
            Assert.IsTrue(File.Exists(fileName));
            Console.WriteLine("Created file "+ fileName);
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
        }

        [Test]
        public void TestFail_CreatingInMemory_Modality_Unknown()
        {
            var r = new Random(23);
            Assert.Throws<ArgumentException>(()=>new DicomDataGenerator(r,new DirectoryInfo(TestContext.CurrentContext.WorkDirectory),"LOLZ"));

        }
    }
}

