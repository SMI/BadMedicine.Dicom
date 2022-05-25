using FellowOakDicom;
using NUnit.Framework;
using System;

namespace BadMedicine.Dicom.Tests
{
    class StudyTests
    {
        [Test]
        public void Test_CreatingNewStudy_HasSomeImages()
        {

            var r = new Random(100);

            var generator = new DicomDataGenerator(r,null);

            var p = new Person(r);
            
            Study study = new(generator,p,new("MR",2,0,50,0,r),r);

            Assert.AreEqual(2,study.Series.Count);
            Assert.AreEqual(50,study.Series[0].Datasets.Count);


            foreach(DicomDataset ds in study.Series[0])
            {
                Assert.AreEqual("MR",ds.GetValues<string>(DicomTag.Modality)[0]);
                Assert.AreEqual(study.StudyTime,ds.GetSingleValue<DateTime>(DicomTag.StudyTime).TimeOfDay);
            }

            generator.Dispose();

        }

        [Test]
        public void Test_UsingExplicitUIDs()
        {
            UIDAllocator.StudyUIDs.Enqueue("999");
            UIDAllocator.SeriesUIDs.Enqueue("888");
            UIDAllocator.SOPUIDs.Enqueue("777");

            var r = new Random(100);

            var generator = new DicomDataGenerator(r, null);

            var p = new Person(r);

            Study study = new(generator, p, new("MR", 2, 0, 50, 0, r), r);

            Assert.AreEqual("999", study.StudyUID.UID);
            Assert.AreEqual("888", study.Series[0].SeriesUID.UID);

            var image1 = study.Series[0].Datasets[0];
            Assert.AreEqual("999", image1.GetSingleValue<DicomUID>(DicomTag.StudyInstanceUID).UID);
            Assert.AreEqual("888", image1.GetSingleValue<DicomUID>(DicomTag.SeriesInstanceUID).UID);
            Assert.AreEqual("777", image1.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).UID);

            generator.Dispose();

        }
    }
}
