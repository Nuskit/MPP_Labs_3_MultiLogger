using Labs_3_Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labs_3_Logger_Tests
{
  class TestUdpServer
  {
    private AddressInformation addressInformation;
    private UdpClient udpClient;
    private List<byte> bytes;
    private volatile bool isReadSocket;
    private Task receiveAsync;

    private bool IsReadSocket
    {
      get
      {
        return isReadSocket;
      }
      set
      {
        isReadSocket = value;
      }
    }

    private void InitializeData()
    {
      addressInformation = new AddressInformation();
      bytes = new List<byte>();
    }

    public TestUdpServer(string ipServer,int port)
    {
      InitializeData();
      addressInformation.ip = ipServer;
      addressInformation.port = port;
    }

    public void StartReceive()
    {
      IsReadSocket = true;
      CreateServer();
      receiveAsync = Task.Factory.StartNew(() => TaskReceive());
    }

    private async Task TaskReceive()
    {
      bytes.Clear();
      while (IsReadSocket)
      {
        var receiveBytes=await udpClient.ReceiveAsync();
        bytes.AddRange(receiveBytes.Buffer);
      }
      return;
    }

    private void CreateServer()
    {
      udpClient = new UdpClient(GeneratePoint());
    }

    private IPEndPoint GeneratePoint()
    {
      return new IPEndPoint(IPAddress.Parse(addressInformation.ip), addressInformation.port);
    }

    public void Close()
    {
      udpClient.Close();
    }

    public void Synchronize()
    {
      IsReadSocket = false;
      receiveAsync.Wait(1500);
      receiveAsync.Dispose();
      receiveAsync = null;
    }

    public byte[] GetMessage()
    {
      return bytes.ToArray();
    }
  }
}
