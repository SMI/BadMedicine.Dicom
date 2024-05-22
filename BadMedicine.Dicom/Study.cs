using FellowOakDicom;
using SynthEHR;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BadMedicine.Dicom;

/// <summary>
/// Represents a whole DICOM Study (a collection of Series objects).
/// Stores the DICOM tags that fit at the study/patient level hierarchy
/// (and are modelled by BadMedicine.Dicom).
/// </summary>
public class Study : IEnumerable<Series>
{
    /// <summary>
    /// The Series objects within this Study
    /// </summary>
    public IReadOnlyList<Series> Series => _series.AsReadOnly();

    /// <summary>
    /// The DicomDataGenerator which created this Study
    /// </summary>
    public DicomDataGenerator Parent;

    /// <summary>
    /// The DICOM UID of this Study
    /// </summary>
    public DicomUID StudyUID{get; }

    /// <summary>
    /// The timestamp on this Study
    /// </summary>
    public DateTime StudyDate { get; internal set; }

    /// <summary>
    /// Free-text description of this Study
    /// </summary>
    public string? StudyDescription {get; }
    /// <summary>
    /// The Accession Number for this Study, usually used to associate the study with clinical data in the RIS
    /// </summary>
    public string AccessionNumber { get; }
    /// <summary>
    /// Starting time of the Study, empty if unknown
    /// </summary>
    public TimeSpan StudyTime { get; }

    /// <summary>
    /// Count of Instances within this Study
    /// </summary>
    public int NumberOfStudyRelatedInstances { get; }

    private readonly List<Series> _series = new();

    /// <summary>
    /// Constructor for a new Study on a specified Person
    /// </summary>
    /// <param name="parent">The DicomDataGenerator creating this Study</param>
    /// <param name="person">The Person representing this patient</param>
    /// <param name="modalityStats">Statistical distributions to use</param>
    /// <param name="r">Seeded PRNG to use</param>
    public Study(DicomDataGenerator parent, Person person, ModalityStats modalityStats, Random r)
    {
        /////////////////////// Generate all the Study Values ////////////////////
        Parent = parent;
        StudyUID = UIDAllocator.GenerateStudyInstanceUID();
        StudyDate = person.GetRandomDateDuringLifetime(r).Date;

        var stats = DicomDataGeneratorStats.GetInstance();

        string imageType;
        NumberOfStudyRelatedInstances = 1;
        var imageCount = 2;

        //if we know about the frequency of StudyDescription values for this modality?
        if(stats.TagValuesByModalityAndTag.TryGetValue(modalityStats.Modality, out var descriptions))
            StudyDescription = descriptions.GetRandom(r);

        AccessionNumber = DicomDataGeneratorStats.GetRandomAccessionNumber(r);
        StudyTime = DicomDataGeneratorStats.Instance.GetRandomTimeOfDay(r);

        /////////////////////  Generate all the Series (will also generate images) /////////////////////

        //have a random number of series (based on average and standard deviation for that modality)
        //but have at least 1 series!

        if(modalityStats.Modality == "CT")
        {
            // Set ImageType
            imageType = DicomDataGeneratorStats.Instance.GetRandomImageType(r);
            if(imageType == "ORIGINAL\\PRIMARY\\AXIAL")
            {
                NumberOfStudyRelatedInstances = Math.Max(1,(int)modalityStats.SeriesPerStudyNormal.Sample());
                imageCount = Math.Max(1,(int)modalityStats.ImagesPerSeriesNormal.Sample());
            }
        }
        else
        {
            imageType = "ORIGINAL\\PRIMARY";
            NumberOfStudyRelatedInstances = Math.Max(1,(int)modalityStats.SeriesPerStudyNormal.Sample());
            imageCount = Math.Max(1,(int)modalityStats.ImagesPerSeriesNormal.Sample());
        }

        // see if we have a better StudyDescription / SeriesDescription / BodyPart value set for
        // this modality
        DescBodyPart? part = null;

        if (stats.DescBodyPartsByModality.TryGetValue(modalityStats.Modality, out var stat))
        {
            part = stat.GetRandom(r);
            StudyDescription = part?.StudyDescription;
        }

        for (var i=0;i<NumberOfStudyRelatedInstances;i++)
            _series.Add(new Series(this, person, modalityStats.Modality, imageType, imageCount,part));
    }

    /// <summary>
    /// Returns enumeration of <see cref="Series"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Series> GetEnumerator()
    {
        return _series.GetEnumerator();
    }
    /// <summary>
    /// Returns IEnumerable of <see cref="Series"/>
    /// </summary>
    /// <returns></returns>

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _series.GetEnumerator();
    }
}