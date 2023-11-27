using FellowOakDicom;
using NUnit.Framework;
using System;

namespace BadMedicine.Dicom.Tests;

internal class StudyTests
{
    [Test]
    public void Test_CreatingNewStudy_HasSomeImages()
    {
        var r = new Random(100);

        using var generator = new DicomDataGenerator(r,null) {NoPixels = true};

        var p = new Person(r);

        Study study = new(generator,p,new ModalityStats("MR",2,0,50,0,r),r);

        Assert.That(study.Series, Has.Count.EqualTo(2));
        Assert.That(study.Series[0].Datasets, Has.Count.EqualTo(50));


        foreach(var ds in study.Series[0])
        {
            Assert.Multiple(() =>
            {
                Assert.That(ds.GetValues<string>(DicomTag.Modality)[0], Is.EqualTo("MR"));
                Assert.That(ds.GetSingleValue<DateTime>(DicomTag.StudyTime).TimeOfDay, Is.EqualTo(study.StudyTime));
            });
        }
    }

    [Test]
    public void Test_UsingExplicitUIDs()
    {
        UIDAllocator.StudyUIDs.Enqueue("999");
        UIDAllocator.SeriesUIDs.Enqueue("888");
        UIDAllocator.SOPUIDs.Enqueue("777");

        var r = new Random(100);

        using var generator = new DicomDataGenerator(r, null) {NoPixels = true};

        var p = new Person(r);

        Study study = new(generator, p, new ModalityStats("MR", 2, 0, 50, 0, r), r);

        Assert.Multiple(() =>
        {
            Assert.That(study.StudyUID.UID, Is.EqualTo("999"));
            Assert.That(study.Series[0].SeriesUID.UID, Is.EqualTo("888"));
        });

        var image1 = study.Series[0].Datasets[0];
        Assert.Multiple(() =>
        {
            Assert.That(image1.GetSingleValue<DicomUID>(DicomTag.StudyInstanceUID).UID, Is.EqualTo("999"));
            Assert.That(image1.GetSingleValue<DicomUID>(DicomTag.SeriesInstanceUID).UID, Is.EqualTo("888"));
            Assert.That(image1.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).UID, Is.EqualTo("777"));
        });
    }
}