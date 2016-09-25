using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public class Logger : ILogger
  {
    private class ThreadInfo
    {
      public int number;
      public List<string> message;
    }

    private List<string> buffer;
    private int bufferLimit;
    private volatile List<Tuple<object, int, int>> threadBlock; //first -lock object, second -current write block, third- last write block
    private ILoggerTarget[] targets;

    public Logger(int bufferLimit,ILoggerTarget[] targets)
    {
      this.bufferLimit = CheckBufferLimit(bufferLimit);
      buffer = new List<string>(bufferLimit);

      threadBlock = new List<Tuple<object, int, int>>(bufferLimit);
  
      for (int i = 0; i < bufferLimit; i++)
        threadBlock[i] = new Tuple<object, int, int>(new object(),0,0);
      this.targets = targets;
    }

    private int CheckBufferLimit(int bufferLimit)
    {
      return bufferLimit >= 0 ? bufferLimit : 0;
    }

    public void Log(LogLevel level, string message)
    {
      if (bufferLimit > 0)
      {
        buffer.Add(FormattedMessage(message));
        if (buffer.Count == bufferLimit)
        {
          WriteLog();
          ClearBuffer();
        }
      }
    }

    private void WriteLog()
    {
      for (int i = 0; i < targets.Length; i++)
      {
        var currentThreadBlock = threadBlock.ElementAt(i);
        currentThreadBlock.
        ThreadPool.QueueUserWorkItem(FlushBuffer, new ThreadInfo { number = i, message = buffer });
      }
    }

    private void ClearBuffer()
    {
      buffer = new List<string>(bufferLimit);
    }

    private string FormattedMessage(string message)
    {
      return string.Format(string.Format("{0}\n", message));
    }

    private void FlushBuffer(object state)
    {
      ThreadInfo value = state as ThreadInfo;
      if (value != null)
      {
        Monitor.Enter(locker.ElementAt(value.number));
        targets.ElementAt(value.number).Flush(value.message);
        Monitor.Exit(locker.ElementAt(value.number));
      }
    } 
  }
}