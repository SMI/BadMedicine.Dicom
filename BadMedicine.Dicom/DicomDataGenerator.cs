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

        PixelDrawer drawing = new PixelDrawer();

        /// <summary>
        /// Dictionary of Modality=>Tag=>FrequencyOfEachValue
        /// </summary>
        private static readonly Dictionary<string, Dictionary<DicomTag, BucketList<string>>> TagValuesByModalityAndTag = new Dictionary<string, Dictionary<DicomTag, BucketList<string>>>();
        private static BucketList<string> ModalityFrequency;
        private static Dictionary<string,int> ModalityIndexes = new Dictionary<string, int>();

        private static bool _initialized = false;
        private int[] _modalities;
        private static readonly object InitializedLock = new object();

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
            
            lock(InitializedLock)
                if (!_initialized)
                    Initialize();

            if(modalities.Length == 0)
            {
                _modalities = ModalityIndexes.Values.ToArray();
            }
            else
            {
                foreach(var m in modalities)
                {
                    if(!ModalityIndexes.ContainsKey(m))
                        throw new ArgumentException("Modality '" + m + "' was not supported, supported modalities are:" + string.Join(",",ModalityIndexes.Select(kvp=>kvp.Key)));
                }

                _modalities = modalities.Select(m=>ModalityIndexes[m]).ToArray();
            }            
        }

        private void Initialize()
        {
            InitializeTagValuesByModalityAndTag();
            InitializeModalityFrequency();
            _initialized = true;
        }

        private void InitializeModalityFrequency()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Frequency", typeof(int));

            EmbeddedCsvToDataTable(typeof(DicomDataGenerator), "DicomDataGeneratorModalities.csv",dt);

            ModalityFrequency = new BucketList<string>(r);

            int idx=0;
            foreach(DataRow dr in dt.Rows)
            {
                string modality = (string)dr["Modality"];
                ModalityFrequency.Add((int) dr["Frequency"],modality);
                ModalityIndexes.Add(modality,idx++);
            }
                
        }

        private void InitializeTagValuesByModalityAndTag()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Frequency", typeof(int));

            EmbeddedCsvToDataTable(typeof(DicomDataGenerator), "DicomDataGeneratorTags.csv",dt);          
            
            foreach (DataRow dr in dt.Rows)
            {
                var modality = (string)dr["Modality"];
                var tag = DicomDictionary.Default[(string) dr["Tag"]];           

                if(!TagValuesByModalityAndTag.ContainsKey(modality))
                    TagValuesByModalityAndTag.Add(modality,new Dictionary<DicomTag, BucketList<string>>());

                if(!TagValuesByModalityAndTag[modality].ContainsKey(tag))
                    TagValuesByModalityAndTag[modality].Add(tag,new BucketList<string>(r));

                int frequency = (int) dr["Frequency"];
                TagValuesByModalityAndTag[modality][tag].Add(frequency,(string)dr["Value"]);
            }
        }

        public override object[] GenerateTestDataRow(Person p)
        {
            var f = new DicomFile(GenerateTestDataset(p));
            
            string fileName = Path.Combine(OutputDir.FullName,f.Dataset.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).UID+".dcm");
            f.Save(fileName);
            
            return new object[]{fileName };
        }

        protected override string[] GetHeaders()
        {
            return new string[]{"Files Generated" };
        }

        /// <summary>
        /// Returns a new random dicom image for the <paramref name="p"/> with tag values that make sense for that person
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public DicomDataset GenerateTestDataset(Person p)
        {
            var ds = new DicomDataset();

            DicomUID sopInstanceUid = DicomUID.Generate();
            
            ds.AddOrUpdate(DicomTag.SOPInstanceUID,sopInstanceUid);
            ds.AddOrUpdate(DicomTag.SOPClassUID , DicomUID.SecondaryCaptureImageStorage);
            
            ds.AddOrUpdate(DicomTag.PatientID, p.CHI);
            
            var modality = ModalityFrequency.GetRandom(_modalities);
            ds.AddOrUpdate(DicomTag.Modality,modality);

            if(!NoPixels)
                drawing.DrawBlackBoxWithWhiteText(ds,500,500,sopInstanceUid.UID);

            return ds;
        }
    }
}
