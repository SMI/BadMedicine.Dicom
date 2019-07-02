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

        public string StudyDescription {get; private set;}
        public string AccessionNumber { get; }
        public TimeSpan StudyTime { get; }

        private List<Series> _series = new List<Series>();

        public Study(DicomDataGenerator parent,Person person, ModalityStats modalityStats,Random r)
        {
            /////////////////////// Generate all the Study Values ////////////////////
            Parent = parent;
            StudyUID = DicomUID.Generate();
            StudyDate = person.GetRandomDateDuringLifetime(r).Date;

            var stats = DicomDataGeneratorStats.GetInstance(r);

            //if we know about the frequency of tag values for this modality?
            if(stats.TagValuesByModalityAndTag.ContainsKey(modalityStats.Modality))
                foreach(KeyValuePair<DicomTag, BucketList<string>> dict in stats.TagValuesByModalityAndTag[modalityStats.Modality])
                {
                    //for each tag we know about

                    //if it's a study level one record it here
                    if(dict.Key == DicomTag.StudyDescription)
                        StudyDescription = dict.Value.GetRandom();                    
                }

            AccessionNumber = stats.GetRandomAccessionNumber(r);
            StudyTime = stats.GetRandomTimeOfDay(r);

            /////////////////////  Generate all the Series (will also generate images) /////////////////////
            
            //have a random number of series (based on average and standard deviation for that modality)
            //but have at least 1 series!
            int seriesCount = Math.Max(1,(int)modalityStats.SeriesPerStudyNormal.Sample());
         
            Series = new ReadOnlyCollection<Series>(_series);
            
            for(int i=0;i<seriesCount;i++)
                _series.Add(new Series(this,person,modalityStats,r));
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
