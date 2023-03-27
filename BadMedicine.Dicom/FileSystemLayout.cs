namespace BadMedicine.Dicom;

/// <summary>
/// How and whether to group the generated files into subdirectories
/// </summary>
public enum FileSystemLayout
{
    /// <summary>
    /// Files are created in the target directory without subdirectories
    /// </summary>
    Flat,

    /// <summary>
    /// Files are created in a subdirectory by Study year/month/day e.g. /2001/12/1/my.dcm
    /// </summary>
    StudyYearMonthDay,

    /// <summary>
    /// Files are created in a subdirectory by Study year then AccessionNumber e.g. /2001/12/1/N123/my.dcm
    /// </summary>
    StudyYearMonthDayAccession,

    /// <summary>
    /// Files are created in subdirectories by Study UID
    /// </summary>
    StudyUID

}