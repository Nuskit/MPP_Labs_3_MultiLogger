using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Labs_3_Logger
{
  public class AddressInformation
  {
    public string ip;
    public int port;
  }

  public class TargetUdp : ITargetStream
  {
    private UdpClient udpClient;
    private AddressInformation server;
    private AddressInformation client;

    public void Close()
    {
      udpClient.Close();
      udpClient = null;
    }

    public TargetUdp(AddressInformation server, AddressInformation client)
    {
      this.server = server;
      this.client = client;
    }

    public bool Flush(IEnumerable<string> buffer)
    {
      return true;
    }

    private bool isNullclient()
    {
      return udpClient == null;
    }

    public async Task<bool> FlushAsync(IEnumerable<string> buffer)
    {
      return true;
    }

    public void Write(byte[] message)
    {
      if (checkInitializeUdpClient())
        udpClient.Send(message, message.Length);
    }

    private bool checkInitializeUdpClient()
    {
      return isNullclient() ? InitializeUdpClient() : true;
    }

    private bool InitializeUdpClient()
    {
      bool isConnected = true;
      try
      {
        udpClient = new UdpClient(GenerateEndPoint(client));
        udpClient.Connect(GenerateEndPoint(server));
      }
      catch
      {
        isConnected = false;
      }
      return isConnected;
    }

    private IPEndPoint GenerateEndPoint(AddressInformation addressInformation)
    {
      return new IPEndPoint(GenerateIpAddress(addressInformation.ip), addressInformation.port);
    }

    private IPAddress GenerateIpAddress(string ip)
    {
      return IPAddress.Parse(ip);
    }
  }
}
