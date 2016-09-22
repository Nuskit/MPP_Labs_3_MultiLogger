using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public interface ILoggerTarget
  {
    bool Flush();
    Task<bool> FlushAsync();
  }

  public interface ILogger
  {
    void Log(LogLevel level, string message);
  }

  public enum LogLevel
  {
    Debug,
    Info,
    Warning,
    Error
  }
}
