using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Data;
using SynthEHR.Datasets;
using SynthEHR;

namespace BadMedicine.Dicom;

internal class DicomDataGeneratorStats
{
    public static readonly DicomDataGeneratorStats Instance=new();


    /// <summary>
    /// Dictionary of Modality=>Tag=>FrequencyOfEachValue
    /// </summary>
    public readonly Dictionary<string, Dictionary<DicomTag, BucketList<string>>> TagValuesByModalityAndTag = new();
    public readonly BucketList<ModalityStats> ModalityFrequency=new();
    public readonly Dictionary<string,int> ModalityIndexes = new();

    public readonly Dictionary<string, BucketList<DescBodyPart>> DescBodyPartsByModality = new ();
    /// <summary>
    /// Distribution of time of day (in hours only) that tests were taken
    /// </summary>
    private readonly BucketList<int> _hourOfDay=new();

    /// <summary>
    /// CT Image Type
    /// </summary>
    private readonly BucketList<string> _imageType=new();

    private DicomDataGeneratorStats()
    {
        InitializeTagValuesByModalityAndTag();
        InitializeModalityFrequency(new Random());
        InitializeImageType();

        InitializeDescBodyPart();

        InitializeHourOfDay();
    }


    private void InitializeHourOfDay()
    {
        //Provenance:
        //select DATEPART(HOUR,StudyTime),work.dbo.get_aggregate_value(count(*)) from CT_GoDARTS_StudyTable group by DATEPART(HOUR,StudyTime)
        _hourOfDay.Add(1,1);
        _hourOfDay.Add(4,1);
        _hourOfDay.Add(5,1);
        _hourOfDay.Add(6,1);
        _hourOfDay.Add(8,15);
        _hourOfDay.Add(9,57);
        _hourOfDay.Add(10,36);
        _hourOfDay.Add(11,41);
        _hourOfDay.Add(12,51);
        _hourOfDay.Add(13,55);
        _hourOfDay.Add(14,54);
        _hourOfDay.Add(15,42);
        _hourOfDay.Add(16,44);
        _hourOfDay.Add(17,42);
        _hourOfDay.Add(18,33);
        _hourOfDay.Add(19,1);
        _hourOfDay.Add(20,7);
        _hourOfDay.Add(21,5);
        _hourOfDay.Add(22,8);        
    }

    /// <summary>
    /// Generates a random time of day with a frequency that matches the times when the most images are captured (e.g. more images are
    /// taken at 1pm than at 8pm
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public TimeSpan GetRandomTimeOfDay(Random r)
    {
        var ts = new TimeSpan(0,_hourOfDay.GetRandom(r),r.Next(60),r.Next(60),0);
            
        ts = ts.Subtract(new TimeSpan(ts.Days,0,0,0));

        if(ts.Days != 0)
            throw new Exception("What!");

        return ts;
    }

    public string GetRandomImageType(Random r) => _imageType.GetRandom(r);

    /// <summary>
    /// returns a random string e.g. T101H12451352 where the first letter indicates Tayside and 5th letter indicates Hospital
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public static string GetRandomAccessionNumber(Random r) => $"T{r.Next(4)}{r.Next(2)}{r.Next(5)}H{r.Next(9999999)}";

    private void InitializeModalityFrequency(Random r)
    {
        using DataTable dt = new();
        dt.BeginLoadData();
        dt.Columns.Add("Frequency", typeof(int));
        dt.Columns.Add("AverageSeriesPerStudy", typeof(double));
        dt.Columns.Add("StandardDeviationSeriesPerStudy", typeof(double));
        dt.Columns.Add("AverageImagesPerSeries", typeof(double));
        dt.Columns.Add("StandardDeviationImagesPerSeries", typeof(double));

        DataGenerator.EmbeddedCsvToDataTable(typeof(DicomDataGenerator), "DicomDataGeneratorModalities.csv", dt);
        dt.EndLoadData();

        var idx = 0;
        foreach (DataRow dr in dt.Rows)
        {
            var modality = (string)dr["Modality"];
            ModalityFrequency.Add((int)dr["Frequency"],
                new ModalityStats(
                    modality,
                    (double)dr["AverageSeriesPerStudy"],
                    (double)dr["StandardDeviationSeriesPerStudy"],
                    (double)dr["AverageImagesPerSeries"],
                    (double)dr["StandardDeviationImagesPerSeries"],
                    r
                ));

            ModalityIndexes.Add(modality, idx++);
        }
    }

    private void InitializeDescBodyPart()
    {
        using DataTable dt = new();
        dt.BeginLoadData();
        dt.Columns.Add("Modality", typeof(string));
        dt.Columns.Add("StudyDescription", typeof(string));
        dt.Columns.Add("BodyPartExamined", typeof(string));
        dt.Columns.Add("SeriesDescription", typeof(string));
        dt.Columns.Add("series_count", typeof(int));

        DataGenerator.EmbeddedCsvToDataTable(typeof(DicomDataGenerator), "DicomDataGeneratorDescBodyPart.csv", dt);
        dt.EndLoadData();
        foreach (DataRow dr in dt.Rows)
        {
            var modality = (string)dr["Modality"];

            // first time we have seen this modality
            if(!DescBodyPartsByModality.ContainsKey(modality))
            {
                DescBodyPartsByModality.Add(modality, new BucketList<DescBodyPart>());
            }

            var part = new DescBodyPart
            {
                StudyDescription = dr["StudyDescription"] as string,
                BodyPartExamined = dr["BodyPartExamined"] as string, // as string deals with DBNull.value
                SeriesDescription = dr["SeriesDescription"] as string
            };

            // record how often we see this part
            DescBodyPartsByModality[modality].Add((int)dr["series_count"], part);
        }
    }
    private void InitializeTagValuesByModalityAndTag()
    {
        using DataTable dt = new();
        dt.BeginLoadData();
        dt.Columns.Add("Frequency", typeof(int));

        DataGenerator.EmbeddedCsvToDataTable(typeof(DicomDataGenerator), "DicomDataGeneratorTags.csv", dt);
        dt.EndLoadData();

        foreach (DataRow dr in dt.Rows)
        {
            var modality = (string)dr["Modality"];
            var tag = DicomDictionary.Default[(string)dr["Tag"]];

            if (!TagValuesByModalityAndTag.ContainsKey(modality))
                TagValuesByModalityAndTag.Add(modality, new Dictionary<DicomTag, BucketList<string>>());

            if (!TagValuesByModalityAndTag[modality].ContainsKey(tag))
                TagValuesByModalityAndTag[modality].Add(tag, new BucketList<string>());

            var frequency = (int)dr["Frequency"];
            TagValuesByModalityAndTag[modality][tag].Add(frequency, (string)dr["Value"]);
        }
    }

    private void InitializeImageType()
    {
        _imageType.Add(96,"ORIGINAL\\PRIMARY\\AXIAL");
        _imageType.Add(1,"ORIGINAL\\PRIMARY\\LOCALIZER");
        _imageType.Add(3,"DERIVED\\SECONDARY");
    }

    /// <summary>
    /// Returns the existing stats for tag popularity, modality frequencies etc.
    /// </summary>
    /// <returns></returns>
    public static DicomDataGeneratorStats GetInstance()
    {
            return Instance;
    }
}