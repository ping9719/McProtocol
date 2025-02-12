using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace McProtocol.Mitsubishi
{
    public class McProtocolUdp : McProtocolApp
    {
        public McProtocolUdp(int iPortNumber) : base("", iPortNumber) { }

        public McProtocolUdp(string iHostName, int iPortNumber, McFrame frame) : base(iHostName, iPortNumber, frame)
        {
            Client = new UdpClient(iPortNumber);
        }

        // &&&&& protected &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        override protected void DoConnect()
        {
            UdpClient c = Client;
            c.Connect(HostName, PortNumber);
        }
        // ====================================================================================
        override protected void DoDisconnect()
        {
            // UDPでは何もしない
        }
        // ================================================================================
        override protected byte[] Execute(byte[] iCommand)
        {
            UdpClient c = Client;
            // 送信
            c.Send(iCommand, iCommand.Length);

            using (var ms = new MemoryStream())
            {
                IPAddress ip = IPAddress.Parse(HostName);
                var ep = new IPEndPoint(ip, PortNumber);
                do
                {
                    // 受信
                    byte[] buff = c.Receive(ref ep);
                    ms.Write(buff, 0, buff.Length);
                } while (0 < c.Available);
                return ms.ToArray();
            }
        }
        // &&&&& private &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        private UdpClient Client { get; set; }

        public override bool Connected => throw new NotImplementedException();
    }
}
