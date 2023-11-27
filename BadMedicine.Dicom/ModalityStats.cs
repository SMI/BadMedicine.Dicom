using MathNet.Numerics.Distributions;
using System;

namespace BadMedicine.Dicom;

/// <summary>
/// A set of statistical distribution parameters for a specific Modality
/// </summary>
public class ModalityStats
{

    /// <summary>
    /// Which Modality this relates to, for example 'MR'
    /// </summary>
    public string Modality{ get; }

    /// <summary>
    /// The mean number of Series in a Study of this Modality
    /// </summary>
    public double SeriesPerStudyAverage => SeriesPerStudyNormal.Mean;

    /// <summary>
    /// The standard deviation of the number of Series in a Study of this Modality
    /// </summary>
    public double SeriesPerStudyStandardDeviation => SeriesPerStudyNormal.StdDev;

    /// <summary>
    /// The parameterised Normal distribution used for the number of series per study
    /// </summary>
    public Normal SeriesPerStudyNormal { get; }

    /// <summary>
    /// The mean number of Images in a Series of this Modality
    /// </summary>
    public double ImagesPerSeriesAverage { get => ImagesPerSeriesNormal.Mean; set => ImagesPerSeriesNormal=new Normal(value,ImagesPerSeriesNormal.StdDev, Rng); }

    /// <summary>
    /// The standard deviation of the number of Images in a Series of this Modality
    /// </summary>
    public double ImagesPerSeriesStandardDeviation{ get => ImagesPerSeriesNormal.StdDev; set => ImagesPerSeriesNormal=new Normal(ImagesPerSeriesNormal.Mean,value,Rng); }

    /// <summary>
    /// The Normal distribution of the number of Images per Series for this Modality
    /// </summary>
    public Normal ImagesPerSeriesNormal {get; private set; }

    /// <summary>
    /// The Random pseudo-random number generator to be used
    /// </summary>
    private Random Rng { get; }

    /// <summary>
    /// Construct a new set of distributions for use with the specified Modality
    /// </summary>
    /// <param name="modality"></param>
    /// <param name="averageSeriesPerStudy"></param>
    /// <param name="standardDeviationSeriesPerStudy"></param>
    /// <param name="averageImagesPerSeries"></param>
    /// <param name="standardDeviationImagesPerSeries"></param>
    /// <param name="r"></param>
    public ModalityStats(string modality, double averageSeriesPerStudy,double standardDeviationSeriesPerStudy,double averageImagesPerSeries,double standardDeviationImagesPerSeries, Random r)
    {
        Rng = r;
        Modality = modality;
        SeriesPerStudyNormal = new Normal(averageSeriesPerStudy, standardDeviationSeriesPerStudy, r);
        ImagesPerSeriesNormal = new Normal(averageImagesPerSeries, standardDeviationImagesPerSeries, r);
    }
}