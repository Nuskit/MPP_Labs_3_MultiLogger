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
      ILogger logger = new Logger(1, new[] { new LoggerTarget(new TargetFileStream()) });
      for (int i = 0; i < 1000; i++)
        logger.Log(LogLevel.Debug, i.ToString());
      Thread.Sleep(1500);
    }
  }
}
