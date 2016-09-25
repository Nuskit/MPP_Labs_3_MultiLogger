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
    private ILoggerTarget[] targets;
    private volatile Pair<ManualResetEvent, int> controlThread;
    private volatile List<Pair<int,int>> threadsBlock; //first -current write block, second -last write block
    

    private void IniializeControlThread()
    {
      controlThread = new Pair<ManualResetEvent, int>(new ManualResetEvent(true), 0);
    }

    private void InitializeBuffer(int bufferLimit)
    {
      this.bufferLimit = CheckBufferLimit(bufferLimit);
      buffer = new List<string>(bufferLimit);
    }

    private void InitializeTargets(ILoggerTarget[] targets)
    {
      this.targets = targets;
    }

    private void InitializeThreadsBlock(int bufferLimit)
    {
      threadsBlock = new List<Pair<int, int>>(new Pair<int, int>[bufferLimit]);

      for (int i = 0; i < bufferLimit; i++)
        threadsBlock[i] = new Pair<int, int>();
    }

    private void InitializeThreadsBlockWithBuffer(int bufferLimit)
    {
      InitializeBuffer(bufferLimit);
      InitializeThreadsBlock(this.bufferLimit);
  }

    public Logger(int bufferLimit, ILoggerTarget[] targets)
    {
      IniializeControlThread();
      InitializeTargets(targets);
      InitializeThreadsBlockWithBuffer(bufferLimit);
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

    public void SpawnAndWait(object state)
    {
      ThreadPool.QueueUserWorkItem(x => ControlThreadBlock(state));      
    }

    private void ControlThreadBlock(object state)
    {
      controlThread.First = controlThread.Second++ == 0 ? new ManualResetEvent(false) : controlThread.First;
      try
      {
        FlushBuffer(state);
      }
      finally
      {
        if (--controlThread.Second == 0)
          controlThread.First.Set();
      }
    }
    
    public void SynchronizeThread()
    {
      controlThread.First.WaitOne();
    }

    private void WriteLog()
    {
      for (int i = 0; i < targets.Length; i++)
      {
        var currentThreadBlock = threadsBlock.ElementAt(i);
        if (currentThreadBlock.First == currentThreadBlock.Second)
          currentThreadBlock.First = currentThreadBlock.Second = 0;

        SpawnAndWait(new ThreadInfo
        {
          threadBlock = currentThreadBlock,
          currentBlock = currentThreadBlock.Second++,
          message = buffer,
          target = targets.ElementAt(i)
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
        lock (value.threadBlock)
        { 
          while (value.threadBlock.First != value.currentBlock)
          {
            Monitor.Wait(value.threadBlock);
          }
          value.target.Flush(value.message);
          value.threadBlock.First++;
          Monitor.PulseAll(value.threadBlock);
        }
      }
    } 
  }
}