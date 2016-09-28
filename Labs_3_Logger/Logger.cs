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
    private IConvertMessage convertMessage;
    private volatile Pair<ManualResetEvent, int> controlThread; //first -signal for wait threadPool,second-number working thread
    private volatile List<Pair<int,int>> threadsBlock; //first -current write block, second -last write block

    private void IniializeControlThread()
    {
      controlThread = new Pair<ManualResetEvent, int>(new ManualResetEvent(true), 0);
    }

    private void InitializeBuffer(int bufferLimit)
    {
      this.bufferLimit = CheckBufferLimit(bufferLimit);
      buffer = new List<string>(this.bufferLimit);
    }

    private void InitializeTargets(ILoggerTarget[] targets)
    {
      this.targets = targets;
    }

    private void InitializeThreadsBlock()
    {
      threadsBlock = new List<Pair<int, int>>(new Pair<int, int>[targets.Length]);

      for (int i = 0; i < targets.Length; i++)
        threadsBlock[i] = new Pair<int, int>();
    }

    private void InitializeThreadsBlockWithBuffer(int bufferLimit)
    {
      InitializeBuffer(bufferLimit);
      InitializeThreadsBlock();
  }

    private void InitalizeConvertMessage(IConvertMessage convertMessage)
    {
      this.convertMessage = convertMessage;
    }

    public Logger(int bufferLimit, ILoggerTarget[] targets,IConvertMessage convertMessage)
    {
      IniializeControlThread();
      InitializeTargets(targets);
      InitalizeConvertMessage(convertMessage);
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
        buffer.Add(FormattedMessage(level, message));
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
      controlThread.first = Interlocked.Increment(ref controlThread.second)-1 == 0 ? new ManualResetEvent(false) : controlThread.first;
      try
      {
        FlushBuffer(state);
      }
      finally
      {
        
        if (Interlocked.Decrement(ref controlThread.second) == 0)
          controlThread.first.Set();
      }
    }
    
    public void SynchronizeThread()
    {
      controlThread.first.WaitOne();
    }

    private void WriteLog()
    {
      for (int i = 0; i < targets.Length; i++)
      {
        var currentThreadBlock = threadsBlock.ElementAt(i);
        if (currentThreadBlock.first == currentThreadBlock.second)
          currentThreadBlock.first = currentThreadBlock.second = 0;

        SpawnAndWait(new ThreadInfo
        {
          threadBlock = currentThreadBlock,
          currentBlock = Interlocked.Increment(ref currentThreadBlock.second)-1,
          message = buffer,
          target = targets.ElementAt(i)
        });
      }
    }

    private void ClearBuffer()
    {
      buffer = new List<string>(bufferLimit);
    }

    private string FormattedMessage(LogLevel logLevel, string message)
    {
      return convertMessage.ConvertMessage(logLevel, message);
    }

    private void FlushBuffer(object state)
    {
      ThreadInfo value = state as ThreadInfo;
      if (value != null)
      {
        lock (value.threadBlock)
        {
          while (value.threadBlock.first != value.currentBlock)
          {
            Monitor.Wait(value.threadBlock);
          }
          value.target.Flush(value.message);
          Interlocked.Increment(ref value.threadBlock.first);
          Monitor.PulseAll(value.threadBlock);
        }
      }
    } 
  }
}