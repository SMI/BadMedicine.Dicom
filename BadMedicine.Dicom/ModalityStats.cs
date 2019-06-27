namespace BadMedicine.Dicom
{
    public class ModalityStats
    {
            
        public string Modality{get;set;}

        public double AverageSeriesPerStudy {get;set;}
        public double StandardDeviationSeriesPerStudy {get;set;}
        public double AverageImagesPerSeries {get;set;}
        public double StandardDeviationImagesPerSeries{get;set;}

        public ModalityStats(string modality, double averageSeriesPerStudy,double standardDeviationSeriesPerStudy,double averageImagesPerSeries,double standardDeviationImagesPerSeries)
        {
            Modality = modality;
            AverageSeriesPerStudy = averageSeriesPerStudy;
            StandardDeviationSeriesPerStudy = standardDeviationSeriesPerStudy;
            AverageImagesPerSeries = averageImagesPerSeries;
            StandardDeviationImagesPerSeries = standardDeviationImagesPerSeries;
        }
    }
}