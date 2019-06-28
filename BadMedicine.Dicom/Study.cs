using BadMedicine.Datasets;
using Dicom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BadMedicine.Dicom
{
    public class Study : IEnumerable<Series>
    {
        public IReadOnlyList<Series> Series{get;}

        public DicomDataGenerator Parent;
        
        public DicomUID StudyUID{get; private set; }
        public DateTime StudyDate { get; internal set; }

        private List<Series> _series = new List<Series>();

        public Study(DicomDataGenerator parent,Person person, ModalityStats modalityStats,Random r)
        {
            Parent = parent;
            StudyUID = DicomUID.Generate();
            StudyDate = person.GetRandomDateDuringLifetime(r);

            //have a random number of series (based on average and standard deviation for that modality)
            //but have at least 1 series!
            int seriesCount = Math.Max(1,(int)modalityStats.SeriesPerStudyNormal.Sample());
         
            for(int i=0;i<seriesCount;i++)
                _series.Add(new Series(this,person,modalityStats,r));

            Series = new ReadOnlyCollection<Series>(_series);
        }


        public IEnumerator<Series> GetEnumerator()
        {
            return _series.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _series.GetEnumerator();
        }
    }
}
