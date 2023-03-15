using FellowOakDicom;
using System.Collections.Concurrent;

namespace BadMedicine.Dicom;

/// <summary>
/// Allocates <see cref="DicomUID"/> values from an explicit list(s) or
/// by calling <see cref="DicomUID.Generate"/>.
/// </summary>
public class UIDAllocator
{
    /// <summary>
    /// Explicit <see cref="DicomUID"/> string values to use when allocating uids for studies
    /// </summary>
    public static readonly ConcurrentQueue<string> StudyUIDs = new ();


    /// <summary>
    /// Explicit <see cref="DicomUID"/> string values to use when allocating uids for series
    /// </summary>
    public static readonly ConcurrentQueue<string> SeriesUIDs = new();

    /// <summary>
    /// Explicit <see cref="DicomUID"/> string values to use when allocating uids for images
    /// </summary>
    public static readonly ConcurrentQueue<string> SOPUIDs = new();

    /// <summary>
    /// Returns a new <see cref="DicomUID"/> from <see cref="StudyUIDs"/> or allocated
    /// with <see cref="DicomUID.Generate"/>
    /// </summary>
    /// <returns></returns>
    public static DicomUID GenerateStudyInstanceUID() =>
        StudyUIDs.TryDequeue(out var result)
            ? new DicomUID(result, "Local UID", DicomUidType.Unknown)
            : DicomUID.Generate();

    /// <summary>
    /// Returns a new <see cref="DicomUID"/> from <see cref="SeriesUIDs"/> or allocated
    /// with <see cref="DicomUID.Generate"/>
    /// </summary>
    /// <returns></returns>
    public static DicomUID GenerateSeriesInstanceUID() => SeriesUIDs.TryDequeue(out var result)
        ? new DicomUID(result, "Local UID", DicomUidType.Unknown)
        : DicomUID.Generate();

    /// <summary>
    /// Returns a new <see cref="DicomUID"/> from <see cref="SOPUIDs"/> or allocated
    /// with <see cref="DicomUID.Generate"/>
    /// </summary>
    /// <returns></returns>
    public static DicomUID GenerateSOPInstanceUID() => SOPUIDs.TryDequeue(out var result)
        ? new DicomUID(result, "Local UID", DicomUidType.Unknown)
        : DicomUID.Generate();
}