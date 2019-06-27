using Dicom;
using System;
using System.Collections.Generic;
using System.Data;

namespace BadMedicine.Dicom
{
    internal class DicomDataGeneratorStats
    {
        private static DicomDataGeneratorStats _instance;
        private static readonly object InstanceLock = new object();


        /// <summary>
        /// Dictionary of Modality=>Tag=>FrequencyOfEachValue
        /// </summary>
        public readonly Dictionary<string, Dictionary<DicomTag, BucketList<string>>> TagValuesByModalityAndTag = new Dictionary<string, Dictionary<DicomTag, BucketList<string>>>();
        public BucketList<ModalityStats> ModalityFrequency;
        public Dictionary<string,int> ModalityIndexes = new Dictionary<string, int>();

        

        private DicomDataGeneratorStats(Random r)
        {
            InitializeTagValuesByModalityAndTag(r);
            InitializeModalityFrequency(r);
        }
        
        private void InitializeModalityFrequency(Random r)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Frequency", typeof(int));
            dt.Columns.Add("AverageSeriesPerStudy", typeof(double));
            dt.Columns.Add("StandardDeviationSeriesPerStudy", typeof(double));
            dt.Columns.Add("AverageImagesPerSeries", typeof(double));
            dt.Columns.Add("StandardDeviationImagesPerSeries", typeof(double));

            DicomDataGenerator.EmbeddedCsvToDataTable(typeof(DicomDataGenerator), "DicomDataGeneratorModalities.csv",dt);

            ModalityFrequency = new BucketList<ModalityStats>(r);

            int idx=0;
            foreach(DataRow dr in dt.Rows)
            {
                string modality = (string)dr["Modality"];
                ModalityFrequency.Add((int) dr["Frequency"],
                    new ModalityStats(
                        modality,
                        (double)dr["AverageSeriesPerStudy"],
                        (double)dr["StandardDeviationSeriesPerStudy"],
                        (double)dr["AverageImagesPerSeries"],
                        (double)dr["StandardDeviationImagesPerSeries"]
                        ));
                
                ModalityIndexes.Add(modality,idx++);
            }
                
        }

        private void InitializeTagValuesByModalityAndTag(Random r)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Frequency", typeof(int));

            DicomDataGenerator.EmbeddedCsvToDataTable(typeof(DicomDataGenerator), "DicomDataGeneratorTags.csv",dt);          
            
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

        /// <summary>
        /// Returns the existing stats for tag popularity, modality frequencies etc.  If stats have not been loaded they are loaded
        /// and primed with the Random <paramref name="r"/> (otherwise <paramref name="r"/> is ignored).
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static DicomDataGeneratorStats GetInstance(Random r)
        {
            lock(InstanceLock)
            {
                if (_instance == null)
                    _instance = new DicomDataGeneratorStats(r);
                
                return _instance;
            }
                
        }
    }
}