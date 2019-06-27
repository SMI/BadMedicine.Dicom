using BadMedicine.Datasets;
using Dicom;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BadMedicine.Dicom
{
    public class DicomDataGenerator : DataGenerator
    {
        public DirectoryInfo OutputDir { get; }

        /// <summary>
        /// Set to true to generate <see cref="DicomDataset"/> without any pixel data.
        /// </summary>
        public bool NoPixels { get; set; }

        /// <summary>
        /// The subdirectories layout to put dicom files into when writting to disk
        /// </summary>
        public FileSystemLayout Layout{get {return _pathProvider.Layout; } set { _pathProvider = new FileSystemLayoutProvider(value);}}
        private FileSystemLayoutProvider _pathProvider = new FileSystemLayoutProvider(FileSystemLayout.Flat);

        PixelDrawer drawing = new PixelDrawer();

        private int[] _modalities;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="outputDir"></param>
        /// <param name="modalities">List of modalities to generate from e.g. CT,MR.  The frequency of images generated is based on
        /// the popularity of that modality in a clinical PACS.  Passing nothing results in all supported modalities being generated</param>
        public DicomDataGenerator(Random r, DirectoryInfo outputDir, params string[] modalities):base(r)
        {
            OutputDir = outputDir;
            
            var stats = DicomDataGeneratorStats.GetInstance(r);

            if(modalities.Length == 0)
            {
                _modalities = stats.ModalityIndexes.Values.ToArray();
            }
            else
            {
                foreach(var m in modalities)
                {
                    if(!stats.ModalityIndexes.ContainsKey(m))
                        throw new ArgumentException("Modality '" + m + "' was not supported, supported modalities are:" + string.Join(",",stats.ModalityIndexes.Select(kvp=>kvp.Key)));
                }

                _modalities = modalities.Select(m=>stats.ModalityIndexes[m]).ToArray();
            }
        }

        public override object[] GenerateTestDataRow(Person p)
        {
            var f = new DicomFile(GenerateTestDataset(p));
            
            var fi = _pathProvider.GetPath(OutputDir,f.Dataset);
            if(!fi.Directory.Exists)
                fi.Directory.Create();

            string fileName = fi.FullName;
            f.Save(fileName);
            
            return new object[]{fileName };
        }

        protected override string[] GetHeaders()
        {
            return new string[]{"Files Generated" };
        }

        public DicomDataset GenerateTestDataset(Person p)
        {
            return GenerateTestDataset(p,null);
        }
        /// <summary>
        /// Returns a new random dicom image for the <paramref name="p"/> with tag values that make sense for that person
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public DicomDataset GenerateTestDataset(Person p,Series series)
        {
            var stats = DicomDataGeneratorStats.GetInstance(r);

            var ds = new DicomDataset();
            
            //generate UIDs
            if(series != null)
            {
                ds.AddOrUpdate(DicomTag.StudyInstanceUID,series.Study.StudyUID);
                ds.AddOrUpdate(DicomTag.SeriesInstanceUID,series.SeriesUID);
            }
            else
            {
                ds.AddOrUpdate(DicomTag.StudyInstanceUID,DicomUID.Generate());
                ds.AddOrUpdate(DicomTag.SeriesInstanceUID,DicomUID.Generate());
            }

            //use the series modality (or get a random one)
            var modality = series?.ModalityStats ?? stats.ModalityFrequency.GetRandom(_modalities);

            DicomUID sopInstanceUID = DicomUID.Generate();
            ds.AddOrUpdate(DicomTag.SOPInstanceUID,sopInstanceUID);
            ds.AddOrUpdate(DicomTag.SOPClassUID , DicomUID.SecondaryCaptureImageStorage);
            
            //patient details
            ds.AddOrUpdate(DicomTag.PatientID, p.CHI);

            var dt = p.GetRandomDateDuringLifetime(r);
            ds.AddOrUpdate(new DicomDate(DicomTag.StudyDate,dt));
            
            
            ds.AddOrUpdate(DicomTag.Modality,modality.Modality);
            
            if(!NoPixels)
                drawing.DrawBlackBoxWithWhiteText(ds,500,500,sopInstanceUID.UID);

            return ds;
        }
    }
}
