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

        private List<Series> _series = new List<Series>();

        public Study(DicomDataGenerator parent,Person person, ModalityStats modalityStats)
        {
            Parent = parent;
            StudyUID = DicomUID.Generate();
            
            int seriesCount = (int)((parent.GetGaussian() +1.0) * modalityStats.AverageSeriesPerStudy);
         
            for(int i=0;i<seriesCount;i++)
                _series.Add(new Series(this,person,modalityStats));

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
