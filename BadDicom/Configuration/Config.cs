using YamlDotNet.Serialization;

namespace BadDicom.Configuration;

[YamlSerializable]
internal class Config
{
    public TargetDatabase? Database { get;set; }
    public ExplicitUIDs? UIDs { get; set; }
}