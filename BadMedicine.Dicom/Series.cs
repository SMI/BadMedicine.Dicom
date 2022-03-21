using FellowOakDicom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BadMedicine.Dicom
{
    public class Series : IEnumerable<DicomDataset>
    {
        public DicomUID SeriesUID {get; }
        public Study Study{get; }

        public IReadOnlyList<DicomDataset> Datasets{get; }

        private readonly List<DicomDataset> _datasets = new();
        
        public Person person;
        public string Modality {get; }
        public string ImageType {get; }
        public DateTime SeriesDate { get; internal set; }
        public TimeSpan SeriesTime { get; internal set; }
        public int NumberOfSeriesRelatedInstances { get; }

        internal Series(Study study, Person person, string modality, string imageType, int imageCount)
        {
            SeriesUID = DicomUID.Generate();

            this.Study = study;
            this.person = person;
            this.Modality = modality;
            this.ImageType = imageType;
            this.NumberOfSeriesRelatedInstances = imageCount;
            
            //todo: for now just use the Study date, in theory secondary capture images could be generated later
            SeriesDate = study.StudyDate;
            SeriesTime = study.StudyTime;

            for(int i =0 ; i<imageCount;i++)
                _datasets.Add(Study.Parent.GenerateTestDataset(person,this));
            
            Datasets = new ReadOnlyCollection<DicomDataset>(_datasets);
        }

        public IEnumerator<DicomDataset> GetEnumerator()
        {
            return _datasets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _datasets.GetEnumerator();
        }
    }
}
