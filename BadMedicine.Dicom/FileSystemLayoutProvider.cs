using Dicom;
using DicomTypeTranslation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

                    date = (DateTime)DicomTypeTranslaterReader.GetCSharpValue(ds,DicomTag.StudyDate); 
                    
                    return  new FileInfo(Path.Combine(
                        root.FullName,
                        date.Year.ToString(),
                        date.Month.ToString(),
                        date.Day.ToString(),
                        ds.GetSingleValue<DicomUID>(DicomTag.StudyDate).UID+".dcm"));

                case FileSystemLayout.StudyYearMonthDayAccession:
                    
                    date = (DateTime)DicomTypeTranslaterReader.GetCSharpValue(ds,DicomTag.StudyDate); 
                    
                    return  new FileInfo(Path.Combine(
                        root.FullName,
                        date.Year.ToString(),
                        date.Month.ToString(),
                        date.Day.ToString(),
                        ds.GetSingleValue<string>(DicomTag.AccessionNumber),
                        ds.GetSingleValue<DicomUID>(DicomTag.StudyDate).UID+".dcm"));

                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
