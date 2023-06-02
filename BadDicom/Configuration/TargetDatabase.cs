using FAnsi;
using YamlDotNet.Serialization;

namespace BadDicom.Configuration;

/// <summary>
/// Identify the target database and configuration for generated data
/// </summary>
[YamlSerializable]
public class TargetDatabase
{
    /// <summary>
    /// Which RDBMS the database is (MySQL, Microsoft SQL Server, etc)
    /// </summary>
    public DatabaseType DatabaseType { get; set; }
    /// <summary>
    /// The ConnectionString containing the server name, credentials and other parameters for the connection
    /// </summary>
    public string ConnectionString { get; set; }
        
    /// <summary>
    /// The name of database
    /// </summary>
    public string DatabaseName { get; set; }
        
    /// <summary>
    /// The filename of a YAML template file to be used for this database
    /// </summary>
    public string Template { get; set; }

    /// <summary>
    /// Pass true to create tables from template that do not have primary key.  Do bulk insert
    /// then deduplicate final tables and recreate primary key
    /// </summary>
    public bool MakeDistinct { get; set; }

    /// <summary>
    /// Set to true to drop and recreate tables described in the Template
    /// </summary>
    public bool DropTables { get; set; }

    /// <summary>
    /// The number of parallel batches to execute (each batch gets the full count of studies
    /// then they are merged at the end).
    /// </summary>
    public int Batches { get; set; } = 1;
}