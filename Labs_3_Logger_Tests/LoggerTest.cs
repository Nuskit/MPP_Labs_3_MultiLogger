using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Labs_3_Logger;
using System.Threading;

namespace Labs_3_Logger_Tests
{
  [TestClass]
  public class LoggerTest
  {
    [TestMethod]
    public void TestMethod1()
    {
      ILogger logger = new Logger(5, new[] { new LoggerTarget(new TargetFileStream()) });
      for (int i = 0; i < 10000; i++)
      {
        logger.Log(LogLevel.Debug, string.Format("{0} {1}", i.ToString(), DateTime.Now));
      }
      logger.SynchronizeThread();
    }
  }
}
