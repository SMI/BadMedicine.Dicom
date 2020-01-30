using FAnsi;

namespace BadDicom.Configuration
{
    public class TargetDatabase
    {
        public DatabaseType DatabaseType { get; set; }
        public string ConnectionString { get; set; }
        
        public string DatabaseName { get; set; }
        
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
}
