using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCUDWebIntegration.Logger
{
    public static class LoggerExtentions
    {
        public static ILoggerFactory AddFile(this ILoggerFactory factory, IConfiguration config)
        {
            factory.AddProvider(new FileLoggerProvider(config));
            return factory;
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder factory, IConfiguration config)
        {
            factory.AddProvider(new FileLoggerProvider(config));
            return factory;
        }
    }
}
