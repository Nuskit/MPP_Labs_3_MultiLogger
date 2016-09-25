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
    public Stream CreateStream()
    {
      return new FileStream("1.txt", FileMode.Append);
    }
  }
}
