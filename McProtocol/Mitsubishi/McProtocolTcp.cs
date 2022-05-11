using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace McProtocol.Mitsubishi
{
    public class McProtocolTcp : McProtocolApp
    {
        private TcpClient Client { get; set; }
        private NetworkStream Stream { get; set; }

        // コンストラクタ
        public override bool Connected
        {
            get
            {
                return Client.Connected;
            }
        }

        public McProtocolTcp() : this("", 0, McFrame.MC3E)
        {

        }

        public McProtocolTcp(string iHostName, int iPortNumber, McFrame frame) : base(iHostName, iPortNumber, frame)
        {
            CommandFrame = frame;
            Client = new TcpClient();
        }

        async override protected Task<int> DoConnect()
        {
            //TcpClient c = Client;
            if (!Client.Connected)
            {
                // Keep Alive機能の実装
                var ka = new List<byte>(sizeof(uint) * 3);
                ka.AddRange(BitConverter.GetBytes(1u));
                ka.AddRange(BitConverter.GetBytes(45000u));
                ka.AddRange(BitConverter.GetBytes(5000u));
                Client.Client.IOControl(IOControlCode.KeepAliveValues, ka.ToArray(), null);
                Client.Connect(HostName, PortNumber);
                Stream = Client.GetStream();
            }
            return 0;
        }

        override protected void DoDisconnect()
        {
            TcpClient c = Client;
            if (c.Connected)
            {
                c.Close();
            }
        }

        private readonly object balanceLock = new object();
        async override protected Task<byte[]> Execute(byte[] iCommand)
        {
            lock (balanceLock)
            {
                List<byte> list = new List<byte>();

                NetworkStream ns = Stream;
                ns.Write(iCommand, 0, iCommand.Length);
                ns.Flush();

                using (var ms = new MemoryStream())
                {
                    var buff = new byte[256];
                    do
                    {
                        int sz = ns.Read(buff, 0, buff.Length);
                        if (sz == 0)
                        {
                            throw new Exception("切断されました");
                        }
                        ms.Write(buff, 0, sz);
                    }
                    while (ns.DataAvailable);

                    return ms.ToArray();
                }
            }
        }
    }
}
