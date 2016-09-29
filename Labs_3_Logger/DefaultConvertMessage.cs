using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public class DefaultConvertMessage : IConvertMessage
  {
    public string ConvertMessage(LogLevel logLevel, string message)
    {
      return String.Format("{0}: {1} {2}\n", DateTime.UtcNow.ToLongDateString(), logLevel.ToString(), message);
    }
  }
}
