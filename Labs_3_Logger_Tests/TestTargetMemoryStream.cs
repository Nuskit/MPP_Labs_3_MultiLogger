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
        if ((!isNullStream()))
        {
          currentStream.Close();
          currentStream = null;
        }
      }
      else
        count--;
    }

    public byte[] GetMessage()
    {
      return (isNullStream()) ? new byte[0] : currentStream.ToArray();
    }

    public void Write(byte[] message)
    {
      currentStream = CreateStream();
      currentStream.Write(message, 0, message.Length);
    }

    public bool Flush(IEnumerable<string> buffer)
    {
      if (!isNullStream())
        currentStream.Flush();
      return true;
    }

    private bool isNullStream()
    {
      return currentStream == null;
    }

    public async Task<bool> FlushAsync(IEnumerable<string> buffer)
    {
      if (!isNullStream())
        await currentStream.FlushAsync();
      return true;
    }
  }
}
