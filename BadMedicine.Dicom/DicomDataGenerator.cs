﻿using BadMedicine.Datasets;
using Dicom;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using CsvHelper;

namespace BadMedicine.Dicom
{
    /// <summary>
    /// <see cref="DataGenerator"/> which produces dicom files on disk and accompanying metadata
    /// </summary>
    public class DicomDataGenerator : DataGenerator,IDisposable
    {
        /// <summary>
        /// Location on disk to output dicom files to
        /// </summary>
        public DirectoryInfo OutputDir { get; }

        /// <summary>
        /// Set to true to generate <see cref="DicomDataset"/> without any pixel data.
        /// </summary>
        public bool NoPixels { get; set; }

        /// <summary>
        /// True to output Study / Series / Image level CSV files containing all the tag data.  Setting this option
        /// disables image file output
        /// </summary>
        public bool Csv { get; set; }

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

        private List<DicomTag> _studyTags;
        private List<DicomTag> _seriesTags;
        private List<DicomTag> _imageTags;
        private string _lastStudyUID = "";
        private string _lastSeriesUID = "";
        private CsvWriter studyWriter, seriesWriter, imageWriter;

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

        private bool csvInitialized = false;

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
            if(!csvInitialized && Csv)
                InitialiseCSVOutput();

            //The currently extracting study
            Study study;
            string studyUID = null;

            foreach (var ds in GenerateStudyImages(p, out study))
            {
                //don't generate more than the maximum number of images
                if (MaximumImages-- <= 0)
                {
                    break;
                }
                else
                    studyUID = study.StudyUID.UID; //all images will have the same study

                // ACH : additions to produce some CSV data
                if(Csv)
                    AddDicomDatasetToCSV(ds);
                else
                {
                    var f = new DicomFile(ds);

                    var fi = _pathProvider.GetPath(OutputDir, f.Dataset);
                    if(!fi.Directory.Exists)
                        fi.Directory.Create();

                    string fileName = fi.FullName;
                    f.Save(fileName);
                }
            }

            //in the CSV write only the StudyUID
            return new object[]{studyUID };
        }

        /// <summary>
        /// Returns headers for the inventory file produced during <see cref="GenerateTestDataset(BadMedicine.Person,System.Random)"/>
        /// </summary>
        /// <returns></returns>
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
        /// <param name="r"></param>
        /// <returns></returns>
        public DicomDataset GenerateTestDataset(Person p,Random r)
        {
            //get a random modality
            var modality = GetRandomModality(r);
            return GenerateTestDataset(p,new Study(this,p,modality,r).Series[0]);
        }

        private ModalityStats GetRandomModality(Random r)
        {
            return DicomDataGeneratorStats.GetInstance(r).ModalityFrequency.GetRandom(_modalities,r);
        }

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

            DicomUID sopInstanceUID = DicomUID.Generate();
            ds.AddOrUpdate(DicomTag.SOPInstanceUID,sopInstanceUID);
            ds.AddOrUpdate(DicomTag.SOPClassUID , DicomUID.SecondaryCaptureImageStorage);
            
            //patient details
            ds.AddOrUpdate(DicomTag.PatientID, p.CHI);
            ds.AddOrUpdate(DicomTag.PatientName, p.Forename + " " + p.Surname);
            ds.AddOrUpdate(DicomTag.PatientBirthDate, p.DateOfBirth);

            if (p.Address != null)
            {
                string s = p.Address.Line1 + " " + p.Address.Line2 + " " + p.Address.Line3 + " " + p.Address.Line4 +
                           " " + p.Address.Postcode.Value;

                ds.AddOrUpdate(DicomTag.PatientAddress,
                    s.Substring(0,Math.Min(s.Length,64)) //LO only allows 64 characters
                    );
            }

            ds.AddOrUpdate(new DicomDate(DicomTag.StudyDate, series.Study.StudyDate));
            ds.AddOrUpdate(new DicomTime(DicomTag.StudyTime, DateTime.Today + series.Study.StudyTime));

            ds.AddOrUpdate(new DicomDate(DicomTag.SeriesDate, series.SeriesDate));
            ds.AddOrUpdate(new DicomTime(DicomTag.SeriesTime, DateTime.Today + series.SeriesTime));
                        
            ds.AddOrUpdate(DicomTag.Modality,series.Modality);
            
            if(series.Study.StudyDescription != null)
                ds.AddOrUpdate(DicomTag.StudyDescription,series.Study.StudyDescription);

            // Calculate the age of the patient at the time the series was taken
            var age = series.SeriesDate.Year - p.DateOfBirth.Year;
            // Go back to the year the person was born in case of a leap year
            if (p.DateOfBirth.Date > series.SeriesDate.AddYears(-age)) age--;
                ds.AddOrUpdate(new DicomAgeString(DicomTag.PatientAge,age.ToString("000") + "Y"));
            
            if(!NoPixels)
                drawing.DrawBlackBoxWithWhiteText(ds,500,500,sopInstanceUID.UID);

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
            ds.AddOrUpdate(DicomTag.SpiralPitchFactor, "0.0");
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

            return ds;
        }

        // ACH - Methods for CSV output added below

        private void InitialiseCSVOutput()
        {
            // Write the headers
            if(csvInitialized)
                return;
            csvInitialized = true;

            _studyTags = new List<DicomTag>()
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

            _seriesTags = new List<DicomTag>()
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


            _imageTags = new List<DicomTag>()
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

            if (OutputDir != null)
            {
                // Create/open CSV files
                studyWriter = new CsvWriter(new StreamWriter(System.IO.Path.Combine(OutputDir.FullName, StudyCsvFilename)),CultureInfo.CurrentCulture);
                seriesWriter = new CsvWriter(new StreamWriter(System.IO.Path.Combine(OutputDir.FullName, SeriesCsvFilename)),CultureInfo.CurrentCulture);
                imageWriter = new CsvWriter(new StreamWriter(System.IO.Path.Combine(OutputDir.FullName, ImageCsvFilename)),CultureInfo.CurrentCulture);
                
                // Write header
                WriteData("STUDY>>", studyWriter, _studyTags.Select(i => i.DictionaryEntry.Keyword));
                WriteData("SERIES>>", seriesWriter, _seriesTags.Select(i => i.DictionaryEntry.Keyword));
                WriteData("IMAGES>>", imageWriter, _imageTags.Select(i => i.DictionaryEntry.Keyword));
            }
        }

        private void WriteData(string fileId, CsvWriter sw, IEnumerable<string> data)
        {
            foreach (string s in data)
                sw.WriteField(s);
            
            sw.NextRecord();
        }

        private void AddDicomDatasetToCSV(DicomDataset ds)
        {
            if (_lastStudyUID != ds.GetString(DicomTag.StudyInstanceUID))
            {
                _lastStudyUID = ds.GetString(DicomTag.StudyInstanceUID);

                WriteTags("STUDY>>", studyWriter, _studyTags, ds);
            }

            if (_lastSeriesUID != ds.GetString(DicomTag.SeriesInstanceUID))
            {
                _lastSeriesUID = ds.GetString(DicomTag.SeriesInstanceUID);

                WriteTags("SERIES>>", seriesWriter, _seriesTags, ds);
            }

            WriteTags("IMAGE>>", imageWriter, _imageTags, ds);
        }

        private void WriteTags(string fileId, CsvWriter sw, List<DicomTag> tags, DicomDataset ds)
        {
            var columnData = new List<string>();
            foreach (DicomTag tag in tags)
            {
                if (ds.Contains(tag))
                {
                    columnData.Add(ds.GetString(tag));
                }
                else
                {
                    columnData.Add("NULL");
                }
            }

            WriteData(fileId, sw, columnData);
            sw.Flush();
        }

        /// <summary>
        /// Closes all writers and flushes to disk
        /// </summary>
        public void Dispose()
        {
            studyWriter?.Dispose();
            seriesWriter?.Dispose();
            imageWriter?.Dispose();
        }
    }
}
