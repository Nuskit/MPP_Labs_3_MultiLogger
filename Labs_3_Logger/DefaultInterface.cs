using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public interface ILoggerTarget
  {
    bool Flush(IEnumerable<string> buffer);
    Task<bool> FlushAsync(IEnumerable<string> buffer);
  }

  public interface ILogger
  {
    void Log(LogLevel level, string message);
    void SynchronizeThread();
  }

  public enum LogLevel
  {
    Debug,
    Info,
    Warning,
    Error
  }
}
