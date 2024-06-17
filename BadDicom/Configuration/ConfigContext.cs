using YamlDotNet.Serialization;

namespace BadDicom.Configuration;

[YamlStaticContext]
[YamlSerializable(typeof(Config))]
[YamlSerializable(typeof(TargetDatabase))]
[YamlSerializable(typeof(ExplicitUIDs))]
public sealed partial class ConfigContext;