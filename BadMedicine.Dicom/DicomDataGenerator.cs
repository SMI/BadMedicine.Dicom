using BadMedicine.Datasets;
using Dicom;
using System;
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
        
        /// <summary>
        /// The maximum number of images to generate regardless of how many calls to <see cref="GenerateTestDataRow"/>,  Defaults to int.MaxValue
        /// </summary>
        public int MaximumImages { get; set; } = int.MaxValue;

        private FileSystemLayoutProvider _pathProvider = new FileSystemLayoutProvider(FileSystemLayout.StudyYearMonthDay);

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
        
        /// <summary>
        /// Creates a new dicom dataset
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override object[] GenerateTestDataRow(Person p)
        {
            Study study;

            foreach(var ds in GenerateStudyImages(p, out study))
            {
                //don't generate more than the maximum number of images
                if(MaximumImages--<=0)
                {
                    study = null;
                    break; 
                }

                var f = new DicomFile(ds);
            
                var fi = _pathProvider.GetPath(OutputDir,f.Dataset);
                if(!fi.Directory.Exists)
                    fi.Directory.Create();

                string fileName = fi.FullName;
                f.Save(fileName);
            }

            //in the CSV write only the StudyUID
            return new object[]{study?.StudyUID?.UID };
        }

        protected override string[] GetHeaders()
        {
            return new string[]{ "Studies Generated" };
        }

        /// <summary>
        /// Creates a dicom study for the <paramref name="p"/> with tag values that make sense for that person.  This call
        /// will generate an entire with a (sensible) random number of series and a random number of images per series
        /// (e.g. for CT studies you might get 2 series of ~100 images each).
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public DicomDataset[] GenerateStudyImages(Person p, out Study study)
        {        
            //generate a study
            study = new Study(this,p,GetRandomModality(),r);

            return study.SelectMany(series=>series).Select(image=>image).ToArray();
        }

        public DicomDataset GenerateTestDataset(Person p)
        {
            //get a random modality
            var modality = GetRandomModality();
            return GenerateTestDataset(p,new Study(this,p,modality,r).Series[0]);
        }

        private ModalityStats GetRandomModality()
        {
            return DicomDataGeneratorStats.GetInstance(r).ModalityFrequency.GetRandom(_modalities);
        }

        /// <summary>
        /// Returns a new random dicom image for the <paramref name="p"/> with tag values that make sense for that person
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public DicomDataset GenerateTestDataset(Person p,Series series)
        {
            var ds = new DicomDataset();
                        
            ds.AddOrUpdate(DicomTag.StudyInstanceUID,series.Study.StudyUID);
            ds.AddOrUpdate(DicomTag.SeriesInstanceUID,series.SeriesUID);

            DicomUID sopInstanceUID = DicomUID.Generate();
            ds.AddOrUpdate(DicomTag.SOPInstanceUID,sopInstanceUID);
            ds.AddOrUpdate(DicomTag.SOPClassUID , DicomUID.SecondaryCaptureImageStorage);
            
            //patient details
            ds.AddOrUpdate(DicomTag.PatientID, p.CHI);
            ds.AddOrUpdate(DicomTag.PatientName, p.Forename + " " + p.Surname);
            ds.AddOrUpdate(DicomTag.PatientBirthDate, p.DateOfBirth);
            ds.AddOrUpdate(DicomTag.PatientAddress,p.Address.Line1 + " " + p.Address.Line2 + " " + p.Address.Line3 + " " + p.Address.Line4 + " " + p.Address.Postcode.Value);


            ds.AddOrUpdate(new DicomDate(DicomTag.StudyDate,series.Study.StudyDate));
            ds.AddOrUpdate(new DicomDate(DicomTag.SeriesDate,series.SeriesDate));
                        
            ds.AddOrUpdate(DicomTag.Modality,series.ModalityStats.Modality);
            
            // Calculate the age of the patient at the time the series was taken
            var age = series.SeriesDate.Year - p.DateOfBirth.Year;
            // Go back to the year the person was born in case of a leap year
            if (p.DateOfBirth.Date > series.SeriesDate.AddYears(-age)) age--;
            ds.AddOrUpdate(new DicomAgeString(DicomTag.PatientAge,age.ToString("000") + "Y"));

            
            if(!NoPixels)
                drawing.DrawBlackBoxWithWhiteText(ds,500,500,sopInstanceUID.UID);
            

            return ds;
        }
    }
}
