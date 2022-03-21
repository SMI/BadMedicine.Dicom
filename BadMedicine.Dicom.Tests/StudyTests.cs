﻿using FellowOakDicom;
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
    }
}
