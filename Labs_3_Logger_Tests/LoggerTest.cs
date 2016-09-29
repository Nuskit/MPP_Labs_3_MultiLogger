using Microsoft.VisualStudio.TestTools.UnitTesting;
using Labs_3_Logger;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Labs_3_Logger_Tests
{
  [TestClass]
  public class LoggerTest
  {
    [TestMethod]
    public void TestOneTarget()
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

    [TestMethod]
    public void TestTwoTarget()
    {
      var streamFirst = new TestTargetMemoryStream();
      var streamSecond = new TestTargetMemoryStream();
      StringBuilder stringBuilderFirst = new StringBuilder(8000);
      StringBuilder stringBuilderSecond = new StringBuilder(8000);
      ILogger logger = new Logger(50, new[] { new LoggerTarget(streamFirst), new LoggerTarget(streamSecond) }, new TestConvertMessage());
      for (int i = 0; i < 5000; i++)
      {
        logger.Log(LogLevel.Debug, i.ToString());
        stringBuilderFirst.Append(i.ToString());
        stringBuilderSecond.Append(i.ToString());
      }
      logger.SynchronizeThread();
      CollectionAssert.AreEqual(LoggerTarget.GetBytes(stringBuilderFirst.ToString()), streamFirst.GetMessage());
      streamFirst.Close();
      CollectionAssert.AreEqual(LoggerTarget.GetBytes(stringBuilderSecond.ToString()), streamSecond.GetMessage());
      streamSecond.Close();
    }

    [TestMethod]
    public void TestNoFlushing()
    {
      var stream = new TestTargetMemoryStream();
      ILogger logger = new Logger(150, new[] { new LoggerTarget(stream) }, new TestConvertMessage());
      for (int i = 0; i < 100; i++)
      {
        logger.Log(LogLevel.Debug, i.ToString());
      }
      logger.SynchronizeThread();
      Assert.AreEqual(stream.GetMessage().Length, 0);
      stream.Close();
    }

    [TestMethod]
    public void TestHardWorking() //enabled for full load
    {
      var streams = new List<TestTargetMemoryStream>(15);
      var stringBuilder = new StringBuilder(10000);
      for (int i=0;i<15;i++)
      {
        streams.Add(new TestTargetMemoryStream());
      }
      for (int j = 0; j < 20; j++)
      {
        ILogger logger = new Logger(j*2, streams.ToArray(), new TestConvertMessage());
        for (int i = 0; i < 1000*j; i++)
        {
          logger.Log(LogLevel.Debug, i.ToString());
          stringBuilder.Append(i.ToString());
        }
        logger.SynchronizeThread();
        byte[] stringByte = LoggerTarget.GetBytes(stringBuilder.ToString());
        for (int i = 0; i > 15; i++)
        {
          CollectionAssert.AreEqual(stringByte, streams[i].GetMessage());
          streams[i].Close();
        }
        stringBuilder.Clear();
      }
    }

    [TestMethod]
    public void TestUdpClient()
    {
      var udpServer= new TestUdpServer("127.0.0.1",10010);
      var stringBuilder = new StringBuilder(500);
      ILogger logger = new Logger(1, new[] { new LoggerTarget(
        new TargetUdp(
          new AddressInformation() {ip="127.0.0.1",port=10010 },
          new AddressInformation() {ip="0.0.0.0",port=0 }))
      }, new TestConvertMessage());
      udpServer.StartReceive();
      for (int i = 0; i < 200; i++)
      {
        logger.Log(LogLevel.Debug, i.ToString());
        stringBuilder.Append(i.ToString());
      }
      logger.SynchronizeThread();
      udpServer.Synchronize();
      udpServer.Close();
      CollectionAssert.AreEqual(udpServer.GetMessage(), LoggerTarget.GetBytes(stringBuilder.ToString()));
    }
  }
}
