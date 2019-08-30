using Dicom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BadMedicine.Dicom
{
    public class Series : IEnumerable<DicomDataset>
    {
        public DicomUID SeriesUID {get; private set; }
        public Study Study{get; private set; }

        public IReadOnlyList<DicomDataset> Datasets{get;private set;}

        private readonly List<DicomDataset> _datasets = new List<DicomDataset>();
        
        public Person person;
        public string Modality {get;  private set;}
        public string ImageType {get;  private set;}
        public DateTime SeriesDate { get; internal set; }
        public TimeSpan SeriesTime { get; internal set; }
        public int NumberOfSeriesRelatedInstances { get; }

        internal Series(Study study, Person person, string modality, string imageType, int imageCount, Random r)
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
