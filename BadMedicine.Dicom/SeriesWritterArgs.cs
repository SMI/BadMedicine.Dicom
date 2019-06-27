using Dicom;

namespace BadMedicine.Dicom
{
    public class SeriesWritterArgs
    {
        public DicomDataGenerator Parent {get;set;}
        public DicomUID StudyUid {get;set;}
        public string Modality {get;set;}
        public int NumberToWrite {get;set;}
    }
}