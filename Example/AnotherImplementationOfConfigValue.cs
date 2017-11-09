using System;
using System.Threading;
using Framework.Core.Configuration;
using Framework.Core.Repository;

namespace Example
{
    /// <summary>
    ///     Assume that it takes a value from database, so the value could change over time.
    /// </summary>
    public class AnotherImplementationOfConfigValue : ConfigValue, ICacheable
    {
        private Lazy<string> _value;

        public AnotherImplementationOfConfigValue()
        {
            _value = new Lazy<string>(FetchValue);
        }

        public void Refresh()
        {
            _value = new Lazy<string>(FetchValue);
        }

        public string GetValue()
        {
            return _value.Value;
        }

        private string FetchValue()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return Guid.NewGuid().ToString();
        }
    }
}