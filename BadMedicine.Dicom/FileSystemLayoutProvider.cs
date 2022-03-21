using FellowOakDicom;
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
            var filename = $"{ds.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).UID}.dcm";
            var date = ds.GetValues<DateTime>(DicomTag.StudyDate);

            switch(Layout)
            {
                case FileSystemLayout.Flat: 
                    return  new(Path.Combine(root.FullName,filename));

                case FileSystemLayout.StudyYearMonthDay:
                    
                    if(date.Length > 0)
                    {
                        return  new(Path.Combine(
                        root.FullName,
                        date[0].Year.ToString(),
                        date[0].Month.ToString(),
                        date[0].Day.ToString(),
                        filename));
                    }
                    else
                        break;

                case FileSystemLayout.StudyYearMonthDayAccession:
                    
                    var acc = ds.GetSingleValue<string>(DicomTag.AccessionNumber);
                    
                    if(date.Length > 0 && !string.IsNullOrWhiteSpace(acc))
                    {
                        return  new(Path.Combine(
                        root.FullName,
                        date[0].Year.ToString(),
                        date[0].Month.ToString(),
                        date[0].Day.ToString(),
                        acc,
                        filename));
                    }
                    else
                        break;

                case FileSystemLayout.StudyUID:

                    return  new(Path.Combine(
                        root.FullName,
                        ds.GetSingleValue<DicomUID>(DicomTag.StudyInstanceUID).UID,
                        filename));

                default: throw new ArgumentOutOfRangeException();
            }
                   
            return  new(Path.Combine(root.FullName,filename));
        }

    }
}
