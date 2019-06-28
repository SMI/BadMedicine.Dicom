using MathNet.Numerics.Distributions;
using System;

namespace BadMedicine.Dicom
{
    public class ModalityStats
    {
            
        public string Modality{get;private set;}

        public double SeriesPerStudyAverage {get;private set;}
        public double SeriesPerStudyStandardDeviation {get;private set;}
        public Normal SeriesPerStudyNormal {get; private set; }

        public double ImagesPerSeriesAverage {get;set;}

        public double ImagesPerSeriesStandardDeviation{get;set;}
        public Normal ImagesPerSeriesNormal {get; private set; }        

        public ModalityStats(string modality, double averageSeriesPerStudy,double standardDeviationSeriesPerStudy,double averageImagesPerSeries,double standardDeviationImagesPerSeries, Random r)
        {
            Modality = modality;
            SeriesPerStudyAverage = averageSeriesPerStudy;
            SeriesPerStudyStandardDeviation = standardDeviationSeriesPerStudy;
            ImagesPerSeriesAverage = averageImagesPerSeries;
            ImagesPerSeriesStandardDeviation = standardDeviationImagesPerSeries;

            SeriesPerStudyNormal = new Normal(SeriesPerStudyAverage,SeriesPerStudyStandardDeviation,r);
            ImagesPerSeriesNormal = new Normal(ImagesPerSeriesAverage,ImagesPerSeriesStandardDeviation,r);
        }
    }
}