﻿using FellowOakDicom;
using System;
using System.IO;

namespace BadMedicine.Dicom;

internal class FileSystemLayoutProvider
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
                return  new FileInfo(Path.Combine(root.FullName,filename));

            case FileSystemLayout.StudyYearMonthDay:

                if(date.Length > 0)
                {
                    return  new FileInfo(Path.Combine(
                        root.FullName,
                        date[0].Year.ToString(),
                        date[0].Month.ToString(),
                        date[0].Day.ToString(),
                        filename));
                }
                break;

            case FileSystemLayout.StudyYearMonthDayAccession:

                var acc = ds.GetSingleValue<string>(DicomTag.AccessionNumber);

                if(date.Length > 0 && !string.IsNullOrWhiteSpace(acc))
                {
                    return  new FileInfo(Path.Combine(
                        root.FullName,
                        date[0].Year.ToString(),
                        date[0].Month.ToString(),
                        date[0].Day.ToString(),
                        acc,
                        filename));
                }
                break;

            case FileSystemLayout.StudyUID:

                return  new FileInfo(Path.Combine(
                    root.FullName,
                    ds.GetSingleValue<DicomUID>(DicomTag.StudyInstanceUID).UID,
                    filename));

            default: throw new ArgumentOutOfRangeException(nameof(Layout));
        }

        return  new FileInfo(Path.Combine(root.FullName,filename));
    }

}