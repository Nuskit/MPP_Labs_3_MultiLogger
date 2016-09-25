using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public class LoggerTarget : ILoggerTarget
  {
    private ITargetStream targetStream;

    public LoggerTarget(ITargetStream targetStream)
    {
      this.targetStream = targetStream;
    }

    public bool Flush(IEnumerable<string> messageBuffer)
    {
      Stream currentStream = InitialStream();
      return currentStream != null ? TryWriteInStream(currentStream, messageBuffer) : false;
    }

    public async Task<bool> FlushAsync(IEnumerable<string> messageBuffer)
    {
      Stream currentStream = InitialStream();
      return currentStream != null ? await TryWriteInStreamAsync(currentStream,messageBuffer) : false;
    }
    
    private async Task<bool> TryWriteInStreamAsync(Stream currentStream, IEnumerable<string> messageBuffer)
    {
      WriteInStream(currentStream, messageBuffer);
      await StreamFlushAsync(currentStream);
      StreamClose(currentStream);
      return true;
    }

    private bool TryWriteInStream(Stream currentStream, IEnumerable<string> messageBuffer)
    {
      WriteInStream(currentStream, messageBuffer);
      StreamFlush(currentStream);
      StreamClose(currentStream);
      return true;
    }

    private void StreamClose(Stream currentStream)
    {
      currentStream.Close();
    }

    private async Task StreamFlushAsync(Stream currentStream)
    {
      await currentStream.FlushAsync();
    }

    private void StreamFlush(Stream currentStream)
    {
      currentStream.Flush();
    }

    private Stream InitialStream()
    {
      return targetStream.CreateStream();
    }

    private void WriteInStream(Stream currentStream, IEnumerable<string> messageBuffer)
    {
      byte[] bytes = messageBuffer.SelectMany(x => GetBytes(x)).ToArray();
      currentStream.Write(bytes, 0, bytes.Length);
    }

    private byte[] GetBytes(string str)
    {
      byte[] bytes = new byte[str.Length * sizeof(char)];
      System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
      return bytes;
    }
  }
}
