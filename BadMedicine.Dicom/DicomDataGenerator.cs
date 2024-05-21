using SynthEHR.Datasets;
using FellowOakDicom;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using CsvHelper;
using SynthEHR;

namespace BadMedicine.Dicom;

/// <summary>
/// <see cref="DataGenerator"/> which produces dicom files on disk and accompanying metadata
/// </summary>
public class DicomDataGenerator : DataGenerator,IDisposable
{
    /// <summary>
    /// Location on disk to output dicom files to
    /// </summary>
    public DirectoryInfo? OutputDir { get; }

    /// <summary>
    /// Set to true to generate <see cref="DicomDataset"/> without any pixel data.
    /// </summary>
    public bool NoPixels { get; set; }

    /// <summary>
    /// Set to true to discard the generated DICOM files, usually for testing.
    /// </summary>
    private bool DevNull { get; }

    private static readonly string DevNullPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)?"NUL":"/dev/null";

    /// <summary>
    /// Set to true to run <see cref="DicomAnonymizer"/> on the generated <see cref="DicomDataset"/> before writing to disk.
    /// </summary>
    public bool Anonymise {get;set;}

    /// <summary>
    /// True to output Study / Series / Image level CSV files containing all the tag data.  Setting this option
    /// disables image file output
    /// </summary>
    public bool Csv { get; set; }

    /// <summary>
    /// The subdirectories layout to put dicom files into when writing to disk
    /// </summary>
    public FileSystemLayout Layout{
        get => _pathProvider.Layout;
        set => _pathProvider = new FileSystemLayoutProvider(value);
    }

    /// <summary>
    /// The maximum number of images to generate regardless of how many calls to <see cref="GenerateTestDataRow"/>,  Defaults to int.MaxValue
    /// </summary>
    public int MaximumImages { get; set; } = int.MaxValue;

    private FileSystemLayoutProvider _pathProvider = new(FileSystemLayout.StudyYearMonthDay);

    private readonly int[]? _modalities;

    private static readonly List<DicomTag> StudyTags = new()
    {
        DicomTag.PatientID,
        DicomTag.StudyInstanceUID,
        DicomTag.StudyDate,
        DicomTag.StudyTime,
        DicomTag.ModalitiesInStudy,
        DicomTag.StudyDescription,
        DicomTag.PatientAge,
        DicomTag.NumberOfStudyRelatedInstances,
        DicomTag.PatientBirthDate
    };
    private static readonly List<DicomTag> SeriesTags = new()
    {
        DicomTag.StudyInstanceUID,
        DicomTag.SeriesInstanceUID,
        DicomTag.SeriesDate,
        DicomTag.SeriesTime,
        DicomTag.Modality,
        DicomTag.ImageType,
        DicomTag.SourceApplicationEntityTitle,
        DicomTag.InstitutionName,
        DicomTag.ProcedureCodeSequence,
        DicomTag.ProtocolName,
        DicomTag.PerformedProcedureStepID,
        DicomTag.PerformedProcedureStepDescription,
        DicomTag.SeriesDescription,
        DicomTag.BodyPartExamined,
        DicomTag.DeviceSerialNumber,
        DicomTag.NumberOfSeriesRelatedInstances,
        DicomTag.SeriesNumber
    };
    private static readonly List<DicomTag> ImageTags= new()
        {
            DicomTag.SeriesInstanceUID,
            DicomTag.SOPInstanceUID,
            DicomTag.BurnedInAnnotation,
            DicomTag.SliceLocation,
            DicomTag.SliceThickness,
            DicomTag.SpacingBetweenSlices,
            DicomTag.SpiralPitchFactor,
            DicomTag.KVP,
            DicomTag.ExposureTime,
            DicomTag.Exposure,
            DicomTag.ManufacturerModelName,
            DicomTag.Manufacturer,
            DicomTag.XRayTubeCurrent,
            DicomTag.PhotometricInterpretation,
            DicomTag.ContrastBolusRoute,
            DicomTag.ContrastBolusAgent,
            DicomTag.AcquisitionNumber,
            DicomTag.AcquisitionDate,
            DicomTag.AcquisitionTime,
            DicomTag.ImagePositionPatient,
            DicomTag.PixelSpacing,
            DicomTag.FieldOfViewDimensions,
            DicomTag.FieldOfViewDimensionsInFloat,
            DicomTag.DerivationDescription,
            DicomTag.TransferSyntaxUID,
            DicomTag.LossyImageCompression,
            DicomTag.LossyImageCompressionMethod,
            DicomTag.LossyImageCompressionRatio,
            DicomTag.ScanOptions
        };
    private string _lastStudyUID = "";
    private string _lastSeriesUID = "";
    private CsvWriter? _studyWriter, _seriesWriter, _imageWriter;
    private readonly DicomAnonymizer _anonymiser = new();

    /// <summary>
    /// Name of the file that contains distinct Study level records for all images when <see cref="Csv"/> is true
    /// </summary>
    public const string StudyCsvFilename = "study.csv";

    /// <summary>
    /// Name of the file that contains distinct Series level records for all images when <see cref="Csv"/> is true
    /// </summary>
    public const string SeriesCsvFilename = "series.csv";

    /// <summary>
    /// Name of the file that contains distinct Image level records for all images when <see cref="Csv"/> is true
    /// </summary>
    public const string ImageCsvFilename = "image.csv";

    private bool csvInitialized;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="r"></param>
    /// <param name="outputDir"></param>
    /// <param name="modalities">List of modalities to generate from e.g. CT,MR.  The frequency of images generated is based on
    /// the popularity of that modality in a clinical PACS.  Passing nothing results in all supported modalities being generated</param>
    public DicomDataGenerator(Random r, string? outputDir, params string[] modalities) : base(r)
    {
        DevNull = outputDir?.Equals("/dev/null", StringComparison.InvariantCulture) != false;
        OutputDir = DevNull ? null : Directory.CreateDirectory(outputDir!);

        var stats = DicomDataGeneratorStats.GetInstance();

        var modalityList = new HashSet<string>(modalities);
        // Iterate through known modalities, listing their offsets within the BucketList
        _modalities = stats.ModalityFrequency.Select(static i => i.item.Modality).Select(static (m, i) => (m, i))
            .Where(i => modalityList.Count == 0 || modalityList.Contains(i.m)).Select(static i => i.i).ToArray();

        if (modalityList.Count != 0 && modalityList.Count != _modalities.Length)
            throw new ArgumentException($"Modality list '{modalities}' not supported, valid values are '{string.Join(' ',stats.ModalityFrequency.Select(i=>i.item.Modality))}'");
    }

    /// <summary>
    /// Creates a new dicom dataset
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public override object?[] GenerateTestDataRow(Person p)
    {
        if(!csvInitialized && Csv)
            InitialiseCSVOutput();

        //The currently extracting study
        string? studyUID = null;

        foreach (var ds in GenerateStudyImages(p, out var study))
        {
            //don't generate more than the maximum number of images
            if (MaximumImages-- <= 0)
            {
                break;
            }

            studyUID = study.StudyUID.UID; //all images will have the same study

            // ACH : additions to produce some CSV data
            if(Csv)
                AddDicomDatasetToCSV(
                    ds,
                    _studyWriter ?? throw new InvalidOperationException(),
                    _seriesWriter ?? throw new InvalidOperationException(),
                    _imageWriter ?? throw new InvalidOperationException());
            else
            {
                var f = new DicomFile(ds);

                FileInfo? fi=null;
                if (!DevNull)
                {
                    fi = _pathProvider.GetPath(OutputDir!, f.Dataset);
                    if (fi.Directory is { Exists: false })
                        fi.Directory.Create();
                }

                using var outFile = new FileStream(fi?.FullName ?? DevNullPath, FileMode.Create);
                f.Save(outFile);
            }
        }

        //in the CSV write only the StudyUID
        return new object?[]{studyUID };
    }

    /// <summary>
    /// Returns headers for the inventory file produced during <see cref="GenerateTestDataset(SynthEHR.Person,System.Random)"/>
    /// </summary>
    /// <returns></returns>
    protected override string[] GetHeaders()
    {
        return new[]{ "Studies Generated" };
    }

    /// <summary>
    /// Creates a dicom study for the <paramref name="p"/> with tag values that make sense for that person.  This call
    /// will generate an entire with a (sensible) random number of series and a random number of images per series
    /// (e.g. for CT studies you might get 2 series of ~100 images each).
    /// </summary>
    /// <param name="p"></param>
    /// <param name="study"></param>
    /// <returns></returns>
    public DicomDataset[] GenerateStudyImages(Person p, out Study study)
    {
        //generate a study
        study = new Study(this,p,GetRandomModality(r),r);

        return study.SelectMany(series=>series).ToArray();
    }

    /// <summary>
    /// Generates a new <see cref="DicomDataset"/> for the given <see cref="Person"/>.  This will be a single image single series study
    /// </summary>
    /// <param name="p"></param>
    /// <param name="_r"></param>
    /// <returns></returns>
    public DicomDataset GenerateTestDataset(Person p,Random _r)
    {
        //get a random modality
        var modality = GetRandomModality(_r);
        return GenerateTestDataset(p,new Study(this,p,modality,_r).Series[0]);
    }

    private ModalityStats GetRandomModality(Random _r) =>
        _modalities is null
            ? DicomDataGeneratorStats.GetInstance().ModalityFrequency.GetRandom(_r)
            : _modalities.Length == 1
                ? DicomDataGeneratorStats.GetInstance().ModalityFrequency.Skip(_modalities[0]).First().item
                : DicomDataGeneratorStats.GetInstance().ModalityFrequency.GetRandom(_modalities, _r);

    /// <summary>
    /// Returns a new random dicom image for the <paramref name="p"/> with tag values that make sense for that person
    /// </summary>
    /// <param name="p"></param>
    /// <param name="series"></param>
    /// <returns></returns>
    public DicomDataset GenerateTestDataset(Person p,Series series)
    {
        var ds = new DicomDataset();

        ds.AddOrUpdate(DicomTag.StudyInstanceUID,series.Study.StudyUID);
        ds.AddOrUpdate(DicomTag.SeriesInstanceUID,series.SeriesUID);

        var sopInstanceUID = UIDAllocator.GenerateSOPInstanceUID();
        ds.AddOrUpdate(DicomTag.SOPInstanceUID,sopInstanceUID);
        ds.AddOrUpdate(DicomTag.SOPClassUID , DicomUID.SecondaryCaptureImageStorage);

        //patient details
        ds.AddOrUpdate(DicomTag.PatientID, p.CHI);
        ds.AddOrUpdate(DicomTag.PatientName, $"{p.Forename} {p.Surname}");
        ds.AddOrUpdate(DicomTag.PatientBirthDate, p.DateOfBirth);

        if (p.Address != null)
        {
            var s =
                $"{p.Address.Line1} {p.Address.Line2} {p.Address.Line3} {p.Address.Line4} {p.Address.Postcode.Value}";

            ds.AddOrUpdate(DicomTag.PatientAddress,
                s[..Math.Min(s.Length,64)] //LO only allows 64 characters
            );
        }

        ds.AddOrUpdate(new DicomDate(DicomTag.StudyDate, series.Study.StudyDate));
        ds.AddOrUpdate(new DicomTime(DicomTag.StudyTime, DateTime.Today + series.Study.StudyTime));

        ds.AddOrUpdate(new DicomDate(DicomTag.SeriesDate, series.SeriesDate));
        ds.AddOrUpdate(new DicomTime(DicomTag.SeriesTime, DateTime.Today + series.SeriesTime));

        ds.AddOrUpdate(DicomTag.Modality,series.Modality);
        ds.AddOrUpdate(DicomTag.AccessionNumber, series.Study.AccessionNumber ?? "");

        if(series.Study.StudyDescription != null)
            ds.AddOrUpdate(DicomTag.StudyDescription,series.Study.StudyDescription);

        if(series.SeriesDescription != null)
            ds.AddOrUpdate(DicomTag.SeriesDescription, series.SeriesDescription);

        if (series.BodyPartExamined != null)
            ds.AddOrUpdate(DicomTag.BodyPartExamined, series.BodyPartExamined);

        // Calculate the age of the patient at the time the series was taken
        var age = series.SeriesDate.Year - p.DateOfBirth.Year;
        // Go back to the year the person was born in case of a leap year
        if (p.DateOfBirth.Date > series.SeriesDate.AddYears(-age)) age--;
        ds.AddOrUpdate(new DicomAgeString(DicomTag.PatientAge, $"{age:000}Y"));

        if(!NoPixels)
            PixelDrawer.DrawBlackBoxWithWhiteText(ds,500,500,sopInstanceUID.UID);

        // Additional DICOM tags added for the generation of CSV files
        ds.AddOrUpdate(DicomTag.ModalitiesInStudy, series.Modality);
        ds.AddOrUpdate(DicomTag.NumberOfStudyRelatedInstances, series.Study.NumberOfStudyRelatedInstances);
        //// Series DICOM tags
        ds.AddOrUpdate(DicomTag.ImageType, series.ImageType);
        //ds.AddOrUpdate(DicomTag.ProcedureCodeSequence, "0"); //TODO
        ds.AddOrUpdate(DicomTag.PerformedProcedureStepID, "0");
        ds.AddOrUpdate(DicomTag.NumberOfSeriesRelatedInstances, series.NumberOfSeriesRelatedInstances);
        ds.AddOrUpdate(DicomTag.SeriesNumber, "0");
        //// Image DICOM tags
        ds.AddOrUpdate(DicomTag.BurnedInAnnotation, "NO");
        ds.AddOrUpdate(DicomTag.SliceLocation, "");
        ds.AddOrUpdate(DicomTag.SliceThickness, "");
        ds.AddOrUpdate(DicomTag.SpacingBetweenSlices, "");
        ds.AddOrUpdate(DicomTag.SpiralPitchFactor, "0");
        ds.AddOrUpdate(DicomTag.KVP, "0");
        ds.AddOrUpdate(DicomTag.ExposureTime, "0");
        ds.AddOrUpdate(DicomTag.Exposure, "0");
        ds.AddOrUpdate(DicomTag.XRayTubeCurrent, "0");
        ds.AddOrUpdate(DicomTag.PhotometricInterpretation, "");
        ds.AddOrUpdate(DicomTag.AcquisitionNumber, "0");
        ds.AddOrUpdate(DicomTag.AcquisitionDate, series.SeriesDate);
        ds.AddOrUpdate(new DicomTime(DicomTag.AcquisitionTime, DateTime.Today + series.SeriesTime));
        ds.AddOrUpdate(DicomTag.ImagePositionPatient, "0","0","0");
        ds.AddOrUpdate(new DicomDecimalString(DicomTag.PixelSpacing,"0.3","0.25"));
        ds.AddOrUpdate(DicomTag.FieldOfViewDimensions, "0");
        ds.AddOrUpdate(DicomTag.FieldOfViewDimensionsInFloat, "0");
        //ds.AddOrUpdate(DicomTag.TransferSyntaxUID, "1.2.840.10008.1.2"); this seems to break saving of files lets not set it
        ds.AddOrUpdate(DicomTag.LossyImageCompression, "00");
        ds.AddOrUpdate(DicomTag.LossyImageCompressionMethod, "ISO_10918_1");
        ds.AddOrUpdate(DicomTag.LossyImageCompressionRatio, "1");

        if (!Anonymise) return ds;
        _anonymiser.AnonymizeInPlace(ds);
        ds.AddOrUpdate(DicomTag.StudyInstanceUID,series.Study.StudyUID);
        ds.AddOrUpdate(DicomTag.SeriesInstanceUID, series.SeriesUID);
        return ds;
    }

    // ACH - Methods for CSV output added below

    private void InitialiseCSVOutput()
    {
        // Write the headers
        if(csvInitialized)
            return;
        csvInitialized = true;

        if (OutputDir == null) return;
        // Create/open CSV files
        _studyWriter = new CsvWriter(new StreamWriter(Path.Combine(OutputDir.FullName, StudyCsvFilename)),CultureInfo.CurrentCulture);
        _seriesWriter = new CsvWriter(new StreamWriter(Path.Combine(OutputDir.FullName, SeriesCsvFilename)),CultureInfo.CurrentCulture);
        _imageWriter = new CsvWriter(new StreamWriter(Path.Combine(OutputDir.FullName, ImageCsvFilename)),CultureInfo.CurrentCulture);

        // Write header
        WriteData(_studyWriter, StudyTags.Select(i => i.DictionaryEntry.Keyword));
        WriteData(_seriesWriter, SeriesTags.Select(i => i.DictionaryEntry.Keyword));
        WriteData(_imageWriter, ImageTags.Select(i => i.DictionaryEntry.Keyword));
    }

    private static void WriteData(CsvWriter sw, IEnumerable<string> data)
    {
        foreach (var s in data)
            sw.WriteField(s);

        sw.NextRecord();
    }

    private void AddDicomDatasetToCSV(DicomDataset ds,CsvWriter studies,CsvWriter series,CsvWriter images)
    {
        if (_lastStudyUID != ds.GetString(DicomTag.StudyInstanceUID))
        {
            _lastStudyUID = ds.GetString(DicomTag.StudyInstanceUID);

            WriteTags(studies, StudyTags, ds);
        }

        if (_lastSeriesUID != ds.GetString(DicomTag.SeriesInstanceUID))
        {
            _lastSeriesUID = ds.GetString(DicomTag.SeriesInstanceUID);

            WriteTags(series, SeriesTags, ds);
        }

        WriteTags(images, ImageTags, ds);
    }

    private static void WriteTags(CsvWriter sw, IEnumerable<DicomTag> tags, DicomDataset ds)
    {
        var columnData = tags.Select(tag => ds.Contains(tag) ? ds.GetString(tag) : "NULL");
        WriteData(sw, columnData);
        sw.Flush();
    }

    /// <summary>
    /// Closes all writers and flushes to disk
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _studyWriter?.Dispose();
        _seriesWriter?.Dispose();
        _imageWriter?.Dispose();
    }
}