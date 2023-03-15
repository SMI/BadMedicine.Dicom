using FellowOakDicom;

namespace BadMedicine.Dicom;

/// <summary>
/// <para>
/// Describes a commonly seen occurrence of a given triplet of values
/// <see cref="StudyDescription"/>, <see cref="BodyPartExamined"/> and
/// <see cref="SeriesDescription"/> in scottish medical imaging data.
/// </para>
/// <para>
/// This class (and its corresponding DicomDataGeneratorDescBodyPart.csv) allow
/// synthetic data in the description tags to make sense when comparing to the other
/// 2 tags listed.  It prevents for example a study being generated called CT Head with
/// a Series Description of 'Foot Scan'
/// </para>
/// 
/// </summary>
public class DescBodyPart
{
    /// <summary>
    /// A known value of <see cref="DicomTag.StudyDescription"/> which is consistent with
    /// <see cref="BodyPartExamined"/> and <see cref="SeriesDescription"/> (of this class)
    /// </summary>
    public string StudyDescription { get; init; }

    /// <summary>
    /// A known value of <see cref="DicomTag.BodyPartExamined"/> which is consistent with
    /// <see cref="StudyDescription"/> and <see cref="SeriesDescription"/> (of this class)
    /// </summary>
    public string BodyPartExamined { get; init; }

    /// <summary>
    /// A known value of <see cref="DicomTag.SeriesDescription"/> which is consistent with
    /// <see cref="BodyPartExamined"/> and <see cref="StudyDescription"/> (of this class)
    /// </summary>
    public string SeriesDescription { get; init; }
}