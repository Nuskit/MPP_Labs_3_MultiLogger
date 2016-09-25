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
      public volatile Pair<int, int> threadBlock;
      public int currentBlock;
      public List<string> message;
      public ILoggerTarget target;
    }

    private List<string> buffer;
    private int bufferLimit;
    private volatile List<Pair<int,int>> threadsBlock; //first -current write block, second -last write block
    private ILoggerTarget[] targets;

    public Logger(int bufferLimit,ILoggerTarget[] targets)
    {
      this.bufferLimit = CheckBufferLimit(bufferLimit);
      buffer = new List<string>(bufferLimit);

      threadsBlock = new List<Pair<int, int>>(new Pair<int,int>[bufferLimit]);

      for (int i = 0; i < bufferLimit; i++)
        threadsBlock[i] = new Pair<int, int>();
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
        var currentThreadBlock = threadsBlock.ElementAt(i);
        if (currentThreadBlock.First == currentThreadBlock.Second)
          currentThreadBlock.First = currentThreadBlock.Second = 0;

        ThreadPool.QueueUserWorkItem(FlushBuffer, 
          new ThreadInfo
          {
            threadBlock = currentThreadBlock,
            currentBlock = currentThreadBlock.Second++,
            message = buffer,
            target=targets.ElementAt(i)
          });
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
        try
        {
          Monitor.Enter(value.threadBlock);
          while (value.threadBlock.First != value.currentBlock)
            Monitor.Wait(value.threadBlock);
          value.target.Flush(value.message);
          value.threadBlock.First++;
          Monitor.PulseAll(value.threadBlock);
        }
        finally
        {
          Monitor.Exit(value.threadBlock);
        }
      }
    } 
  }
}