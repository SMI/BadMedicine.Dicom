using FAnsi;

namespace BadDicom.Configuration
{
    public class TargetDatabase
    {
        public DatabaseType DatabaseType { get; set; }
        public string ConnectionString { get; set; }
        
        public string DatabaseName { get; set; }
        
        public string Template { get; set; }
    }
}
