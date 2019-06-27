using BadMedicine.Datasets;
using Dicom;
using System;
using System.IO;

namespace BadMedicine.Dicom
{
    public class DicomDataGenerator : DataGenerator
    {
        public DirectoryInfo OutputDir { get; }
        PixelDrawer drawing = new PixelDrawer();

        public DicomDataGenerator(Random r, DirectoryInfo outputDir):base(r)
        {
            OutputDir = outputDir;
        }    

        public override object[] GenerateTestDataRow(Person p)
        {
            var ds = new DicomDataset();

            ds.AddOrUpdate(DicomTag.SOPInstanceUID,"1.2.3");
            ds.AddOrUpdate(DicomTag.SOPClassUID , DicomUID.SecondaryCaptureImageStorage);
            
            drawing.DrawBlackBoxWithWhiteText(ds,500,500,"1.2.3");

            var f =new DicomFile(ds);
            string fileName = Path.Combine(OutputDir.FullName,"1.2.3.dcm");
            f.Save(fileName);

            return new string[]{fileName };
        }

        protected override string[] GetHeaders()
        {
            return new string[]{"Files Generated" };
        }
    }
}
