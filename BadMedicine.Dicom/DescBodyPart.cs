namespace BadMedicine.Dicom
{
    /// <summary>
    /// <para>
    /// Describes a commonly seen occurance of a given triplet of values
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
        public string StudyDescription { get; set; }
        public string BodyPartExamined { get; set; }
        public string SeriesDescription { get; set; }
    }
}