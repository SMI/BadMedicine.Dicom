using Dicom;
using System;
using System.IO;

namespace BadMedicine.Dicom
{
    class FileSystemLayoutProvider
    {
        public FileSystemLayout Layout { get; }

        public FileSystemLayoutProvider(FileSystemLayout layout)
        {
            Layout = layout;
        }

        public FileInfo GetPath(DirectoryInfo root,DicomDataset ds)
        {
            DateTime date;

            switch(Layout)
            {
                case FileSystemLayout.Flat: 
                    return  new FileInfo(Path.Combine(
                        root.FullName,
                        ds.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).UID+".dcm"));

                case FileSystemLayout.StudyYearMonthDay:

                    date = ds.GetValues<DateTime>(DicomTag.StudyDate)[0]; 
                    
                    return  new FileInfo(Path.Combine(
                        root.FullName,
                        date.Year.ToString(),
                        date.Month.ToString(),
                        date.Day.ToString(),
                        ds.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).UID+".dcm"));

                case FileSystemLayout.StudyYearMonthDayAccession:
                    
                    date = ds.GetValues<DateTime>(DicomTag.StudyDate)[0];
                    
                    return  new FileInfo(Path.Combine(
                        root.FullName,
                        date.Year.ToString(),
                        date.Month.ToString(),
                        date.Day.ToString(),
                        ds.GetSingleValue<string>(DicomTag.AccessionNumber),
                        ds.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).UID+".dcm"));

                case FileSystemLayout.StudyUID:

                    return  new FileInfo(Path.Combine(
                        root.FullName,
                        ds.GetSingleValue<DicomUID>(DicomTag.StudyInstanceUID).UID,
                        ds.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).UID+".dcm"));

                default: throw new ArgumentOutOfRangeException();
            }
        }

    }
}
