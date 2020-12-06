using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Bones.Tests
{
    public class XunitLoggerFactory : ILoggerFactory
    {
        private ITestOutputHelper _output;

        public XunitLoggerFactory(ITestOutputHelper output)
        {
            _output = output;
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_output);
        }

        public void Dispose()
        {
        }
    }
}