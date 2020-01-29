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
    }
}
