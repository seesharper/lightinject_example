using Framework.Core.Repository;

namespace Framework.Core.Configuration
{
    public interface ISomeConfigValue : ICacheable
    {
        string GetValue();
    }
}