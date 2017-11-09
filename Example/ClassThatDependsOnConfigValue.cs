using System;
using System.Threading;
using Framework.Core.Configuration;
using Framework.Core.Repository;

namespace Example
{
    /// <summary>
    ///     We want <see cref="ICacheable.Refresh" /> to be called on this instance if and after
    ///     it is called on <see cref="ISomeConfigValue" />
    /// </summary>
    public class ClassThatDependsOnConfigValue : ICacheable
    {
        private readonly ISomeConfigValue _config;
        private Lazy<Guid> _result;

        public ClassThatDependsOnConfigValue(ISomeConfigValue config)
        {
            _config = config;
            _result = new Lazy<Guid>(FetchResult);
        }

        public void Refresh()
        {
            _result = new Lazy<Guid>(FetchResult);
        }

        private Guid FetchResult()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1)); //access to database, for instance
            return Guid.Parse(_config.GetValue());
        }

        public Guid GetResult()
        {
            return _result.Value;
        }
    }
}