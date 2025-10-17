using System.Collections.Generic;
using System.Collections.ObjectModel;
using FellowOakDicom;
using SynthEHR;

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
/// </summary>
/// <param name="StudyDescription">
/// A known value of <see cref="DicomTag.StudyDescription"/> which is consistent with
/// <see cref="BodyPartExamined"/> and <see cref="SeriesDescription"/> (of this class)
/// </param>
/// <param name="BodyPartExamined">
/// A known value of <see cref="DicomTag.BodyPartExamined"/> which is consistent with
/// <see cref="StudyDescription"/> and <see cref="SeriesDescription"/> (of this class)
/// </param>
/// <param name="SeriesDescription">
/// A known value of <see cref="DicomTag.SeriesDescription"/> which is consistent with
/// <see cref="BodyPartExamined"/> and <see cref="StudyDescription"/> (of this class)
/// </param>
public readonly record struct DescBodyPart(
    string? StudyDescription,
    string? BodyPartExamined,
    string? SeriesDescription)
{
    /// <summary>
    /// Dictionary mapping modality codes to weighted lists of DescBodyPart triplets.
    /// Data is generated at compile-time from DicomDataGeneratorDescBodyPart.csv.
    /// </summary>
    internal static readonly ReadOnlyDictionary<string, BucketList<DescBodyPart>> d = DescBodyPartData.Data;
}
