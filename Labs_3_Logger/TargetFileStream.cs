using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public class TargetFileStream : ITargetStream
  {
    Stream currentStream;
    String fileName;

    public void Close()
    {
      currentStream.Close();
    }

    public TargetFileStream(String fileName)
    {
      this.fileName = fileName;
    }

    private Stream CreateStream()
    {
      return new FileStream(fileName, FileMode.Append);
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

    public void Write(byte[] message)
    {
      currentStream = CreateStream();
      currentStream.Write(message, 0, message.Length);
    }
  }
}
