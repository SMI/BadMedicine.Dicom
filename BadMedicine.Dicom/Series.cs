using Dicom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BadMedicine.Dicom
{
    public class Series : IEnumerable<DicomDataset>
    {
        public DicomUID SeriesUID {get; private set; }
        public Study Study{get; private set; }

        public IReadOnlyList<DicomDataset> Datasets{get;private set;}

        private readonly List<DicomDataset> _datasets = new List<DicomDataset>();
        
        public Person person;
        public ModalityStats ModalityStats {get;  private set;}
        
        public Series(Study study, Person person, ModalityStats modalityStats)
        {
            SeriesUID = DicomUID.Generate();

            this.Study = study;
            this.person = person;
            this.ModalityStats = modalityStats;

            int imageCount = (int)((study.Parent.GetGaussian() +1.0) * modalityStats.AverageImagesPerSeries);

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
