using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Labs_3_Logger;
using System.Threading;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Labs_3_Logger_Tests
{
  [TestClass]
  public class LoggerTest
  {
    [TestMethod]
    public void TestMethod1()
    {
      var stream = new TestTargetMemoryStream();
      StringBuilder stringBuilder = new StringBuilder(5000);
      ILogger logger = new Logger(5, new[] { new LoggerTarget(stream) },new TestConvertMessage());
      for (int i = 0; i < 1000; i++)
      {
        logger.Log(LogLevel.Debug, i.ToString());
        stringBuilder.Append(i.ToString());
      }
      logger.SynchronizeThread();
      CollectionAssert.AreEqual(LoggerTarget.GetBytes(stringBuilder.ToString()), stream.GetMessage());
      stream.Close();
    }
  }
}
