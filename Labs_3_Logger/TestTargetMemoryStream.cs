using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public class TestTargetMemoryStream : ITargetStream
  {
    MemoryStream currentStream;
    int count;

    private MemoryStream CreateStream()
    {
      count++;
      return currentStream ?? new MemoryStream();
    }

    public void Close()
    {
      if (count == 0)
      {
        currentStream.Close();
        currentStream = null;
      }
      else
        count--;
    }

    public byte[] GetMessage()
    {
      return currentStream.ToArray();
    }

    public void Write(byte[] message)
    {
      currentStream = CreateStream();
      currentStream.Write(message, 0, message.Length);
    }

    public bool Flush(IEnumerable<string> buffer)
    {
      currentStream.Flush();
      return true;
    }

    public async Task<bool> FlushAsync(IEnumerable<string> buffer)
    {
      await currentStream.FlushAsync();
      return true;
    }
  }
}
