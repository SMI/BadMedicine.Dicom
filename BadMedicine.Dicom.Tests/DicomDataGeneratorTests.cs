using FellowOakDicom;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace BadMedicine.Dicom.Tests;

public class DicomDataGeneratorTests
{
    [Test]
    public void Test_CreatingOnDisk_OneFile()
    {
        var r = new Random(500);
        using var generator = new DicomDataGenerator(r, TestContext.CurrentContext.WorkDirectory) {Layout = FileSystemLayout.StudyUID, MaximumImages = 1};


        var person = new Person(r);

        //generates a study but because of maximum images 1 we should only get 1 image being generated
        var studyUid = (string)generator.GenerateTestDataRow(person)[0];

        //should be a directory named after the Study UID
        Assert.That(Directory.Exists(Path.Combine(TestContext.CurrentContext.WorkDirectory, studyUid)));

        //should be a single file
        var f = new FileInfo(Directory.GetFiles(Path.Combine(TestContext.CurrentContext.WorkDirectory, studyUid)).Single());
        Assert.That(f.Exists);

        var datasetCreated = DicomFile.Open(f.FullName);

        Assert.Multiple(() =>
        {
            Assert.That(datasetCreated.Dataset.GetValues<DicomUID>(DicomTag.StudyInstanceUID)[0].UID, Is.EqualTo(studyUid),
                    "UID in the dicom file generated did not match the one output into the CSV inventory file"
                );

            Assert.That(datasetCreated.Dataset.GetSingleValue<string>(DicomTag.AccessionNumber), Is.Not.Empty);
        });
    }


    [Test]
    public void ExampleUsage()
    {
        //create a test person
        var r = new Random(23);
        var person = new Person(r);

        //create a generator
        using var generator = new DicomDataGenerator(r, null, "CT");
        //create a dataset in memory
        var dataset = generator.GenerateTestDataset(person, r);

        Assert.Multiple(() =>
        {
            //values should match the patient details
            Assert.That(dataset.GetValue<string>(DicomTag.PatientID, 0), Is.EqualTo(person.CHI));
            Assert.That(dataset.GetValue<DateTime>(DicomTag.StudyDate, 0), Is.GreaterThanOrEqualTo(person.DateOfBirth));

            //should have a study description
            Assert.That(dataset.GetValue<string>(DicomTag.StudyDescription, 0), Is.Not.Null);
            //should have a study time
            Assert.That(dataset.Contains(DicomTag.StudyTime));
        });
    }

    [Test]
    public void Test_CreatingInMemory_ModalityCT()
    {
        var r = new Random(23);
        var person = new Person(r);
        using var generator = new DicomDataGenerator(r,new string(TestContext.CurrentContext.WorkDirectory),"CT") {NoPixels = true};

        //generate 100 images
        for(var i = 0 ; i < 100 ; i++)
        {
            //all should be CT because we said CT only
            var ds = generator.GenerateTestDataset(person, r);
            Assert.That(ds.GetSingleValue<string>(DicomTag.Modality), Is.EqualTo("CT"));
        }
    }

    [Test]
    public void Test_Anonymise()
    {
        var r = new Random(23);
        var person = new Person(r);

        using var generator = new DicomDataGenerator(r,new string(TestContext.CurrentContext.WorkDirectory),"CT");

        // without anonymisation (default) we get the normal patient ID
        var ds = generator.GenerateTestDataset(person, r);

        Assert.Multiple(() =>
        {
            Assert.That(ds.Contains(DicomTag.PatientID));
            Assert.That(ds.GetValue<string>(DicomTag.PatientID, 0), Is.EqualTo(person.CHI));
        });

        // with anonymisation
        generator.Anonymise = true;

        var ds2 = generator.GenerateTestDataset(person, r);

        Assert.Multiple(() =>
        {
            // we get a blank patient ID
            Assert.That(ds2.Contains(DicomTag.PatientID));
            Assert.That(ds2.GetString(DicomTag.PatientID), Is.EqualTo(string.Empty));
        });
    }
    [Test]
    public void Test_CreatingInMemory_Modality_CTAndMR()
    {
        var r = new Random(23);
        var person = new Person(r);

        using var generator = new DicomDataGenerator(r,new string(TestContext.CurrentContext.WorkDirectory),"CT","MR");

        //generate 100 images
        for(var i = 0 ; i < 100 ; i++)
        {
            //all should be CT because we said CT only
            var ds = generator.GenerateTestDataset(person, r);
            var modality = ds.GetSingleValue<string>(DicomTag.Modality);

            Assert.That(modality is "CT" or "MR",$"Unexpected modality {modality}");
        }
    }

    [Test]
    public void TestFail_CreatingInMemory_Modality_Unknown()
    {
        var r = new Random(23);
        Assert.Throws<ArgumentException>(()=>_=new DicomDataGenerator(r,new string(TestContext.CurrentContext.WorkDirectory),"LOLZ"));

    }

    [Test]
    public void Test_CsvOption()
    {
        var r = new Random(500);

        var outputDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,nameof(Test_CsvOption)));
        if (outputDir.Exists)
            outputDir.Delete(true);
        outputDir.Create();

        var people = new PersonCollection();
        people.GeneratePeople(100,r);

        using (var generator = new DicomDataGenerator(r,outputDir.FullName, "CT"))
        {
            generator.Csv = true;
            generator.NoPixels = true;
            generator.MaximumImages = 500;

            generator.GenerateTestDataFile(people,new FileInfo(Path.Combine(outputDir.FullName,"index.csv")),500);
        }

        //3 csv files + index.csv (the default one
        Assert.That(outputDir.GetFiles(), Has.Length.EqualTo(4));

        foreach (var f in outputDir.GetFiles())
        {
            using var reader = new CsvReader(new StreamReader(f.FullName),CultureInfo.CurrentCulture);
            var rowcount = 0;

            //confirms that the CSV is intact (no dodgy commas, unquoted newlines etc)
            while (reader.Read())
                rowcount++;

            //should be 1 row per image + 1 for header
            if(f.Name == DicomDataGenerator.ImageCsvFilename)
                Assert.That(rowcount, Is.EqualTo(501));
        }
    }
}