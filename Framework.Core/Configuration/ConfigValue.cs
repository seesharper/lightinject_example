using System;

namespace Framework.Core.Configuration
{
    /// <summary>
    /// Assume that it takes a value from the configuration file, for example, during application startup.
    /// </summary>
    public class ConfigValue : ISomeConfigValue
    {
        private readonly string _value;

        public ConfigValue()
        {
            _value = Guid.NewGuid().ToString();
        }

        public string GetValue()
        {
            return _value;
        }

        public void Refresh()
        {
            //see Program.cs
            throw new NotImplementedException();
        }
    }
}