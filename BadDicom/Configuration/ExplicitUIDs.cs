using BadMedicine.Dicom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace BadDicom.Configuration;

/// <summary>
/// Config section for loading explicit UIDs from disk and using those in file creation
/// </summary>
[YamlSerializable]
public class ExplicitUIDs
{
    /// <summary>
    /// Path to a file containing a list of study instance UIDs to use
    /// </summary>
    public string StudyInstanceUIDs { get; set; }

    /// <summary>
    /// Path to a file containing a list of series instance UIDs to use
    /// </summary>
    public string SeriesInstanceUIDs { get; set; }

    /// <summary>
    /// Path to a file containing a list of SOP instance UIDs to use
    /// </summary>
    public string SOPInstanceUIDs { get; set; }

    /// <summary>
    /// Loads the UID files referenced (if they exist) in the configuration
    /// and populates <see cref="UIDAllocator"/> with the values.
    /// </summary>
    public void Load()
    {
        // unlikely but if someone else has pre queued some stuff, this replaces that
        UIDAllocator.StudyUIDs.Clear();
        UIDAllocator.SeriesUIDs.Clear();
        UIDAllocator.SOPUIDs.Clear();

        foreach (var u in GetUIDsFrom(StudyInstanceUIDs))
            UIDAllocator.StudyUIDs.Enqueue(u);

        foreach (var u in GetUIDsFrom(SeriesInstanceUIDs))
            UIDAllocator.SeriesUIDs.Enqueue(u);

        foreach (var u in GetUIDsFrom(SOPInstanceUIDs))
            UIDAllocator.SOPUIDs.Enqueue(u);
    }

    private IEnumerable<string> GetUIDsFrom(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return Enumerable.Empty<string>();
            
        return File.ReadLines(StudyInstanceUIDs).Where(l => !string.IsNullOrWhiteSpace(l));
    }
}