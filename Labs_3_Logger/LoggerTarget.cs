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
      return TryWriteInStream(messageBuffer);
    }

    public async Task<bool> FlushAsync(IEnumerable<string> messageBuffer)
    {
      return await TryWriteInStreamAsync(messageBuffer);
    }
    
    private async Task<bool> TryWriteInStreamAsync(IEnumerable<string> messageBuffer)
    {
      WriteInStream(messageBuffer);
      await StreamFlushAsync(messageBuffer);
      StreamClose();
      return true;
    }

    private bool TryWriteInStream(IEnumerable<string> messageBuffer)
    {
      WriteInStream(messageBuffer);
      StreamFlush(messageBuffer);
      StreamClose();
      return true;
    }

    private void StreamClose()
    {
      targetStream.Close();
    }

    private async Task StreamFlushAsync(IEnumerable<string> messageBuffer)
    {
      await targetStream.FlushAsync(messageBuffer);
    }

    private void StreamFlush(IEnumerable<string> messageBuffer)
    {
      targetStream.Flush(messageBuffer);
    }

    private void WriteInStream(IEnumerable<string> messageBuffer)
    {
      targetStream.Write(messageBuffer.SelectMany(x => GetBytes(x)).ToArray());
    }

    public static byte[] GetBytes(string str)
    {
      byte[] bytes = new byte[str.Length * sizeof(char)];
      System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
      return bytes;
    }
  }
}
