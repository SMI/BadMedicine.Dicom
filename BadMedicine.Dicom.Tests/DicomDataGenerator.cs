using NUnit.Framework;
using System;
using System.IO;

namespace BadMedicine.Dicom.Tests
{
    public class DicomDataGeneratorTests
    {
        [Test]
        public void TestCreatingOne()
        {
            var r = new Random(500);
            var generator = new DicomDataGenerator(r,new DirectoryInfo(TestContext.CurrentContext.WorkDirectory));
            
            var person = new Person(r);
            string fileName = (string)generator.GenerateTestDataRow(person)[0];
            Assert.IsTrue(File.Exists(fileName));
            Console.WriteLine("Created file "+ fileName);
        }
    }
}
