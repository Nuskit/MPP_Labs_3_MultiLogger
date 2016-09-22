using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public class Logger : ILogger
  {
    public Logger(int bufferLimit,ILoggerTarget[] targets)
    {

    }

    public void Log(LogLevel level, string message)
    {
      throw new NotImplementedException();
    }
  }
}
