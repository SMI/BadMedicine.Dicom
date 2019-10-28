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

        /// <summary>
        /// Distribution of time of day (in hours only) that tests were taken
        /// </summary>
        public static BucketList<int> HourOfDay;

        /// <summary>
        /// CT Image Type
        /// </summary>
        public static BucketList<string> ImageType;

        private DicomDataGeneratorStats(Random r)
        {
            InitializeTagValuesByModalityAndTag();
            InitializeModalityFrequency(r);
            InitializeImageType();

            InitializeHourOfDay(r);
        }

        private void InitializeHourOfDay(Random r)
        {
            //Provenance:
            //select DATEPART(HOUR,StudyTime),work.dbo.get_aggregate_value(count(*)) from CT_Godarts_StudyTable group by DATEPART(HOUR,StudyTime)

            HourOfDay = new BucketList<int>();
            
            HourOfDay.Add(1,1);
            HourOfDay.Add(4,1);
            HourOfDay.Add(5,1);
            HourOfDay.Add(6,1);
            HourOfDay.Add(8,15);
            HourOfDay.Add(9,57);
            HourOfDay.Add(10,36);
            HourOfDay.Add(11,41);
            HourOfDay.Add(12,51);
            HourOfDay.Add(13,55);
            HourOfDay.Add(14,54);
            HourOfDay.Add(15,42);
            HourOfDay.Add(16,44);
            HourOfDay.Add(17,42);
            HourOfDay.Add(18,33);
            HourOfDay.Add(19,1);
            HourOfDay.Add(20,7);
            HourOfDay.Add(21,5);
            HourOfDay.Add(22,8);        
        }

        /// <summary>
        /// Generates a random time of day with a frequency that matches the times when the most images are captured (e.g. more images are
        /// taken at 1pm than at 8pm
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public TimeSpan GetRandomTimeOfDay(Random r)
        {
            var ts = new TimeSpan(0,HourOfDay.GetRandom(r),r.Next(60),r.Next(60),0);
            
            ts = ts.Subtract(new TimeSpan(ts.Days,0,0,0));

            if(ts.Days != 0)
                throw new Exception("What!");

            return ts;
        }

        public string GetRandomImageType(Random r)
        {
            return ImageType.GetRandom(r);
        }

        /// <summary>
        /// returns a random string e.g. T101H12451352 where the first letter indiciates Tayside and 5th letter indicates Hospital
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public string GetRandomAccessionNumber(Random r)
        {
            return 'T' + r.Next(4) + r.Next(2) + r.Next(5) + "H" + r.Next(9999999);
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

            ModalityFrequency = new BucketList<ModalityStats>();

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
                        (double)dr["StandardDeviationImagesPerSeries"],
                        r
                        ));
                
                ModalityIndexes.Add(modality,idx++);
            }
                
        }

        private void InitializeTagValuesByModalityAndTag()
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
                    TagValuesByModalityAndTag[modality].Add(tag,new BucketList<string>());

                int frequency = (int) dr["Frequency"];
                TagValuesByModalityAndTag[modality][tag].Add(frequency,(string)dr["Value"]);
            }
        }

        private void InitializeImageType()
        {
            ImageType = new BucketList<string>();
            
            ImageType.Add(96,"ORIGINAL\\PRIMARY\\AXIAL");
            ImageType.Add(1,"ORIGINAL\\PRIMARY\\LOCALIZER");
            ImageType.Add(3,"DERIVED\\SECONDARY");
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