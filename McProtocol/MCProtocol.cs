#define old //Now that .NET Standard is supported in UWP old code is Good
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using System.IO;
#if !old
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Foundation;
#endif
using System.Runtime.InteropServices;


namespace MCProtocol
{
    public class PLCData
    {
        public static Mitsubishi.Plc PLC;

    }
    public class PLCData<T> : PLCData
    {
        Mitsubishi.PlcDeviceType DeviceType;
        int Address;
        int Length;
        int LENGTH;//Length in bytes
        byte[] bytes;
        public PLCData(Mitsubishi.PlcDeviceType DeviceType, int Address, int Length)
        {
            this.DeviceType = DeviceType;
            this.Address = Address;

            string t = typeof(T).Name;
            switch (t)
            {
                case "Boolean":
                    this.LENGTH = (Length / 16 + (Length % 16 > 0 ? 1 : 0)) * 2;
                    this.Length = Length;
                    break;
                case "Int32":
                    this.LENGTH = 4 * Length;
                    this.Length = Length * 2;
                    break;
                case "Int16":
                    this.LENGTH = 2 * Length;
                    this.Length = Length;
                    break;
                case "UInt16":
                    this.LENGTH = 2 * Length;
                    this.Length = Length;
                    break;
                case "UInt32":
                    this.LENGTH = 4 * Length;
                    this.Length = Length * 2;
                    break;
                case "Single":
                    this.LENGTH = 4 * Length;
                    this.Length = Length * 2;
                    break;
                case "Double":
                    this.LENGTH = 8 * Length;
                    this.Length = Length * 4;
                    break;
                case "Char":
                    this.LENGTH = Length;
                    this.Length = Length;
                    break;
                default:
                    throw new Exception("Type not supported by PLC.");
            }
            this.bytes = new byte[this.LENGTH];

        }
        public T this[int i]
        {
            get
            {
                Union u = new Union();
                string t = typeof(T).Name;
                switch (t)
                {
                    case "Boolean":
                        return (T)Convert.ChangeType(((this.bytes[i / 8] >> (i % 8)) % 2 == 1), typeof(T));
                    case "Int32":
                        u.a = this.bytes[i * 4];
                        u.b = this.bytes[i * 4 + 1];
                        u.c = this.bytes[i * 4 + 2];
                        u.d = this.bytes[i * 4 + 3];
                        return (T)Convert.ChangeType(u.DINT, typeof(T));
                    case "Int16":
                        u.a = this.bytes[i * 2];
                        u.b = this.bytes[i * 2 + 1];
                        return (T)Convert.ChangeType(u.INT, typeof(T));
                    case "UInt16":
                        u.a = this.bytes[i * 2];
                        u.b = this.bytes[i * 2 + 1];
                        return (T)Convert.ChangeType(u.UINT, typeof(T));
                    case "UInt32":
                        u.a = this.bytes[i * 4];
                        u.b = this.bytes[i * 4 + 1];
                        u.c = this.bytes[i * 4 + 2];
                        u.d = this.bytes[i * 4 + 3];
                        return (T)Convert.ChangeType(u.UDINT, typeof(T));
                    case "Single":
                        u.a = this.bytes[i * 4];
                        u.b = this.bytes[i * 4 + 1];
                        u.c = this.bytes[i * 4 + 2];
                        u.d = this.bytes[i * 4 + 3];
                        return (T)Convert.ChangeType(u.REAL, typeof(T));
                    case "Char":
                        return (T)Convert.ChangeType(this.ToString()[i], typeof(T));
                    default:
                        throw new Exception("Type not recognized.");
                }
            }
            set
            {
                Union u = new Union();
                string t = typeof(T).Name;
                switch (t)
                {
                    case "Boolean":
                        bool arg = Convert.ToBoolean(value);
                        if (arg && (this.bytes[i / 8] >> (i % 8)) % 2 == 0)
                            this.bytes[i / 8] += (byte)(1 << (i % 8));
                        else if (!arg && (this.bytes[i / 8] >> (i % 8)) % 2 == 1)
                            this.bytes[i / 8] -= (byte)(1 << (i % 8));
                        return;
                    case "Int32":
                        u.DINT = Convert.ToInt32(value);
                        this.bytes[i * 4] = u.a;
                        this.bytes[i * 4 + 1] = u.b;
                        this.bytes[i * 4 + 2] = u.c;
                        this.bytes[i * 4 + 3] = u.d;
                        return;
                    case "Int16":
                        u.INT = Convert.ToInt16(value);
                        this.bytes[i * 2] = u.a;
                        this.bytes[i * 2 + 1] = u.b;
                        return;
                    case "UInt32":
                        u.UDINT = Convert.ToUInt32(value);
                        this.bytes[i * 4] = u.a;
                        this.bytes[i * 4 + 1] = u.b;
                        this.bytes[i * 4 + 2] = u.c;
                        this.bytes[i * 4 + 3] = u.d;
                        return;
                    case "UInt16":
                        u.UINT = Convert.ToUInt16(value);
                        this.bytes[i * 2] = u.a;
                        this.bytes[i * 2] = u.b;
                        return;
                    case "Single":
                        u.REAL = Convert.ToSingle(value);
                        this.bytes[i * 4] = u.a;
                        this.bytes[i * 4 + 1] = u.b;
                        this.bytes[i * 4 + 2] = u.c;
                        this.bytes[i * 4 + 3] = u.d;
                        return;
                    default:
                        throw new Exception("Type not recognized.");
                }
            }
        }

        public async Task WriteData()
        {
            await PLC.WriteDeviceBlock(this.DeviceType, this.Address, Length, bytes);
        }
        public async Task ReadData()
        {
            this.bytes = await PLC.ReadDeviceBlock(DeviceType, this.Address, this.Length);
        }

    }
    [StructLayout(LayoutKind.Explicit)]
    public class Union
    {
        [FieldOffset(0)]
        public float REAL;
        [FieldOffset(0)]
        public short INT;
        [FieldOffset(0)]
        public uint UINT;
        [FieldOffset(0)]
        public int DINT;
        [FieldOffset(0)]
        public uint UDINT;
        [FieldOffset(0)]
        public char letter;
        [FieldOffset(0)]
        public byte bite;
        [FieldOffset(0)]
        public byte a;
        [FieldOffset(1)]
        public byte b;
        [FieldOffset(2)]
        public byte c;
        [FieldOffset(3)]
        public byte d;
    }

    public class Mitsubishi
    {
        //const int frameSize = 14;//11, 15, 20
        // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
        public enum McFrame
        {
            MC1E = 4,
            MC3E = 11,
            MC4E = 15

        }

        // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
        // PLCデバイスの種類を定義した列挙体
        public enum PlcDeviceType
        {
            // PLC用デバイス
            M = 0x90
          , SM = 0x91
          , L = 0x92
          , F = 0x93
          , V = 0x94
          , S = 0x98
          , X = 0x9C
          , Y = 0x9D
          , B = 0xA0
          , SB = 0xA1
          , DX = 0xA2
          , DY = 0xA3
          , D = 0xA8
          , SD = 0xA9
          , R = 0xAF
          , ZR = 0xB0
          , W = 0xB4
          , SW = 0xB5
          , TC = 0xC0
          , TS = 0xC1
          , TN = 0xC2
          , CC = 0xC3
          , CS = 0xC4
          , CN = 0xC5
          , SC = 0xC6
          , SS = 0xC7
          , SN = 0xC8
          , Z = 0xCC
          , TT
          , TM
          , CT
          , CM
          , A
          , Max
        }

        // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        // PLCと接続するための共通のインターフェースを定義する
        public interface Plc : IDisposable
        {
            bool Connected { get; }
            Task<int> Open();
            int Close();
            Task<int> SetBitDevice(string iDeviceName, int iSize, int[] iData);
            Task<int> SetBitDevice(PlcDeviceType iType, int iAddress, int iSize, int[] iData);
            Task<int> GetBitDevice(string iDeviceName, int iSize, int[] oData);
            Task<int> GetBitDevice(PlcDeviceType iType, int iAddress, int iSize, int[] oData);
            Task<int> WriteDeviceBlock(string iDeviceName, int iSize, int[] iData);
            Task<int> WriteDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] iData);
            Task<int> WriteDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, byte[] bData);
            Task<byte[]> ReadDeviceBlock(string iDeviceName, int iSize, int[] oData);
            Task<byte[]> ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] oData);
            Task<byte[]> ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize);
            Task<int> SetDevice(string iDeviceName, int iData);
            Task<int> SetDevice(PlcDeviceType iType, int iAddress, int iData);
            Task<int> GetDevice(string iDeviceName);
            Task<int> GetDevice(PlcDeviceType iType, int iAddress);
        }
        // ########################################################################################
        abstract public class McProtocolApp : Plc
        {
            public abstract bool Connected { get; }
            // ====================================================================================
            public McFrame CommandFrame { get; set; }   // 使用フレーム
            public string HostName { get; set; }   // ホスト名またはIPアドレス
            public int PortNumber { get; set; }    // ポート番号
            public int Device { private set; get; }
            // ====================================================================================
            // コンストラクタ
            protected McProtocolApp(string iHostName, int iPortNumber, McFrame frame)
            {
                CommandFrame = frame;
                //C70 = MC3E

                HostName = iHostName;
                PortNumber = iPortNumber;
            }

            // ====================================================================================
            // 後処理
            public void Dispose()
            {
                Close();
            }

            // ====================================================================================
            public async Task<int> Open()
            {
                await DoConnect();
                Command = new McCommand(CommandFrame);
                return 0;
            }
            // ====================================================================================
            public int Close()
            {
                DoDisconnect();
                return 0;
            }
            // ====================================================================================
            public async Task<int> SetBitDevice(string iDeviceName, int iSize, int[] iData)
            {
                PlcDeviceType type;
                int addr;
                GetDeviceCode(iDeviceName, out type, out addr);
                return await SetBitDevice(type, addr, iSize, iData);
            }
            // ====================================================================================
            public async Task<int> SetBitDevice(PlcDeviceType iType, int iAddress, int iSize, int[] iData)
            {
                var type = iType;
                var addr = iAddress;
                var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                var d = (byte)iData[0];
                var i = 0;
                while (i < iData.Length)
                {
                    if (i % 2 == 0)
                    {
                        d = (byte)iData[i];
                        d <<= 4;
                    }
                    else
                    {
                        d |= (byte)(iData[i] & 0x01);
                        data.Add(d);
                    }
                    ++i;
                }
                if (i % 2 != 0)
                {
                    data.Add(d);
                }
                int length = (int)Command.FrameType;// == McFrame.MC3E) ? 11 : 15;
                byte[] sdCommand = Command.SetCommandMC3E(0x1401, 0x0001, data.ToArray());
                byte[] rtResponse = await TryExecution(sdCommand, length);
                int rtCode = Command.SetResponse(rtResponse);
                return rtCode;
            }
            // ====================================================================================
            public async Task<int> GetBitDevice(string iDeviceName, int iSize, int[] oData)
            {
                PlcDeviceType type;
                int addr;
                GetDeviceCode(iDeviceName, out type, out addr);
                return await GetBitDevice(type, addr, iSize, oData);
            }
            // ====================================================================================
            public async Task<int> GetBitDevice(PlcDeviceType iType, int iAddress, int iSize, int[] oData)
            {

                PlcDeviceType type = iType;
                int addr = iAddress;
                var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                byte[] sdCommand = Command.SetCommandMC3E(0x0401, 0x0001, data.ToArray());
                int length = (Command.FrameType == McFrame.MC3E) ? 11 : 15;
                byte[] rtResponse = await TryExecution(sdCommand, length);
                int rtCode = Command.SetResponse(rtResponse);
                byte[] rtData = Command.Response;
                for (int i = 0; i < iSize; ++i)
                {
                    if (i % 2 == 0)
                    {
                        oData[i] = (rtCode == 0) ? ((rtData[i / 2] >> 4) & 0x01) : 0;
                    }
                    else
                    {
                        oData[i] = (rtCode == 0) ? (rtData[i / 2] & 0x01) : 0;
                    }
                }
                return rtCode;
            }
            // ====================================================================================
            public async Task<int> WriteDeviceBlock(string iDeviceName, int iSize, int[] iData)
            {
                PlcDeviceType type;
                int addr;
                GetDeviceCode(iDeviceName, out type, out addr);
                return await WriteDeviceBlock(type, addr, iSize, iData);
            }
            // ====================================================================================
            public async Task<int> WriteDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] iData)
            {

                PlcDeviceType type = iType;
                int addr = iAddress;
                List<byte> data;

                List<byte> DeviceData = new List<byte>();
                foreach (int t in iData)
                {
                    DeviceData.Add((byte)t);
                    DeviceData.Add((byte)(t >> 8));
                }

                byte[] sdCommand;
                int length;
                //TEST Create this write switch statement
                switch (CommandFrame)
                {
                    case McFrame.MC3E:
                        data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                        data.AddRange(DeviceData.ToArray());
                        sdCommand = Command.SetCommandMC3E(0x1401, 0x0000, data.ToArray());
                        length = 11;
                        break;
                    case McFrame.MC4E:
                        data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                        data.AddRange(DeviceData.ToArray());
                        sdCommand = Command.SetCommandMC4E(0x1401, 0x0000, data.ToArray());
                        length = 15;
                        break;
                    case McFrame.MC1E:
                        data = new List<byte>(6)
                   {
                          (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) (addr >> 24)
                      , 0x20
                      , 0x44
                      , (byte) iSize
                      , 0x00
                    };
                        data.AddRange(DeviceData.ToArray());
                        //Add data
                        sdCommand = Command.SetCommandMC1E(0x03, data.ToArray());
                        length = 2;
                        break;
                    default:
                        throw new Exception("Message frame not supported");
                }

                //TEST take care of the writing
                byte[] rtResponse = await TryExecution(sdCommand, length);
                int rtCode = Command.SetResponse(rtResponse);
                return rtCode;
            }
            public async Task<int> WriteDeviceBlock(PlcDeviceType iType, int iAddress, int devicePoints, byte[] bData)
            {
                //FIXME
                int iSize = devicePoints;
                PlcDeviceType type = iType;
                int addr = iAddress;
                List<byte> data;
                byte[] sdCommand;
                int length;
                //TEST Create this write switch statement
                switch (CommandFrame)
                {
                    case McFrame.MC3E:
                        data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                        data.AddRange(bData);
                        sdCommand = Command.SetCommandMC3E(0x1401, 0x0000, data.ToArray());
                        length = 11;
                        break;
                    case McFrame.MC4E:
                        data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                        data.AddRange(bData);
                        sdCommand = Command.SetCommandMC4E(0x1401, 0x0000, data.ToArray());
                        length = 15;
                        break;
                    case McFrame.MC1E:
                        data = new List<byte>(6)
                   {
                          (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) (addr >> 24)
                      , 0x20
                      , 0x44
                      , (byte) iSize
                      , 0x00
                    };
                        data.AddRange(bData);
                        //Add data
                        sdCommand = Command.SetCommandMC1E(0x03, data.ToArray());
                        length = 2;
                        break;
                    default:
                        throw new Exception("Message frame not supported");
                }
                //TEST take care of the writing
                byte[] rtResponse = await TryExecution(sdCommand, length);
                int rtCode = Command.SetResponse(rtResponse);
                return rtCode;
            }
            // ====================================================================================
            public async Task<byte[]> ReadDeviceBlock(string iDeviceName, int iSize, int[] oData)
            {
                PlcDeviceType type;
                int addr;
                GetDeviceCode(iDeviceName, out type, out addr);
                return await ReadDeviceBlock(type, addr, iSize, oData);
            }
            // ====================================================================================
            public async Task<byte[]> ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] oData)
            {

                PlcDeviceType type = iType;
                int addr = iAddress;
                List<byte> data;
                byte[] sdCommand;
                int length;

                switch (CommandFrame)
                {
                    case McFrame.MC3E:
                        data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                        sdCommand = Command.SetCommandMC3E(0x0401, 0x0000, data.ToArray());
                        length = 11;
                        break;
                    case McFrame.MC4E:
                        data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                        sdCommand = Command.SetCommandMC4E(0x0401, 0x0000, data.ToArray());
                        length = 15;
                        break;
                    case McFrame.MC1E:
                        data = new List<byte>(6)
                    {
                          (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) (addr >> 24)
                      , 0x20
                      , 0x44
                      , (byte) iSize
                      , 0x00
                    };
                        sdCommand = Command.SetCommandMC1E(0x01, data.ToArray());
                        length = 2;
                        break;
                    default:
                        throw new Exception("Message frame not supported");
                }

                byte[] rtResponse = await TryExecution(sdCommand, length);
                //TEST verify read responses
                int rtCode = Command.SetResponse(rtResponse);
                byte[] rtData = Command.Response;
                for (int i = 0; i < iSize; ++i)
                {
                    oData[i] = (rtCode == 0) ? BitConverter.ToInt16(rtData, i * 2) : 0;
                }
                return rtData;
            }
            public async Task<byte[]> ReadDeviceBlock(PlcDeviceType iType, int iAddress, int devicePoints)
            {
                int iSize = devicePoints;
                PlcDeviceType type = iType;
                int addr = iAddress;
                List<byte> data;
                byte[] sdCommand;
                int length;

                switch (CommandFrame)
                {
                    case McFrame.MC3E:
                        data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                        sdCommand = Command.SetCommandMC3E(0x0401, 0x0000, data.ToArray());
                        length = 11;
                        break;
                    case McFrame.MC4E:
                        data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
                        sdCommand = Command.SetCommandMC4E(0x0401, 0x0000, data.ToArray());
                        length = 15;
                        break;
                    case McFrame.MC1E:
                        data = new List<byte>(6)
                    {
                          (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) (addr >> 24)
                      , 0x20
                      , 0x44
                      , (byte) iSize
                      , 0x00
                    };
                        sdCommand = Command.SetCommandMC1E(0x01, data.ToArray());
                        length = 2;
                        break;
                    default:
                        throw new Exception("Message frame not supported");
                }

                byte[] rtResponse = await TryExecution(sdCommand, length);
                //TEST verify read responses
                int rtCode = Command.SetResponse(rtResponse);
                byte[] rtData = Command.Response;
                return rtData;
            }
            // ====================================================================================
            public async Task<int> SetDevice(string iDeviceName, int iData)
            {
                PlcDeviceType type;
                int addr;
                GetDeviceCode(iDeviceName, out type, out addr);
                return await SetDevice(type, addr, iData);
            }
            // ====================================================================================
            public async Task<int> SetDevice(PlcDeviceType iType, int iAddress, int iData)
            {

                PlcDeviceType type = iType;
                int addr = iAddress;
                var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , 0x01
                      , 0x00
                      , (byte) iData
                      , (byte) (iData >> 8)
                    };
                byte[] sdCommand = Command.SetCommandMC3E(0x1401, 0x0000, data.ToArray());
                int length = (Command.FrameType == McFrame.MC3E) ? 11 : 15;
                byte[] rtResponse = await TryExecution(sdCommand, length);
                int rtCode = Command.SetResponse(rtResponse);
                return rtCode;
            }
            // ====================================================================================
            public async Task<int> GetDevice(string iDeviceName)
            {
                PlcDeviceType type;
                int addr;
                GetDeviceCode(iDeviceName, out type, out addr);
                return await GetDevice(type, addr);
            }
            // ====================================================================================
            public async Task<int> GetDevice(PlcDeviceType iType, int iAddress)
            {
                PlcDeviceType type = iType;
                int addr = iAddress;
                var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , 0x01
                      , 0x00
                    };
                byte[] sdCommand = Command.SetCommandMC3E(0x0401, 0x0000, data.ToArray());
                int length = (Command.FrameType == McFrame.MC3E) ? 11 : 15;
                ; byte[] rtResponse = await TryExecution(sdCommand, length);
                int rtCode = Command.SetResponse(rtResponse);
                if (0 < rtCode)
                {
                    this.Device = 0;
                }
                else
                {
                    byte[] rtData = Command.Response;
                    this.Device = BitConverter.ToInt16(rtData, 0);
                }
                return rtCode;
            }
            // ====================================================================================
            //public int GetCpuType(out string oCpuName, out int oCpuType)
            //{
            //    int rtCode = Command.Execute(0x0101, 0x0000, new byte[0]);
            //    oCpuName = "dummy";
            //    oCpuType = 0;
            //    return rtCode;
            //}
            // ====================================================================================
            public static PlcDeviceType GetDeviceType(string s)
            {
                return (s == "M") ? PlcDeviceType.M :
                       (s == "SM") ? PlcDeviceType.SM :
                       (s == "L") ? PlcDeviceType.L :
                       (s == "F") ? PlcDeviceType.F :
                       (s == "V") ? PlcDeviceType.V :
                       (s == "S") ? PlcDeviceType.S :
                       (s == "X") ? PlcDeviceType.X :
                       (s == "Y") ? PlcDeviceType.Y :
                       (s == "B") ? PlcDeviceType.B :
                       (s == "SB") ? PlcDeviceType.SB :
                       (s == "DX") ? PlcDeviceType.DX :
                       (s == "DY") ? PlcDeviceType.DY :
                       (s == "D") ? PlcDeviceType.D :
                       (s == "SD") ? PlcDeviceType.SD :
                       (s == "R") ? PlcDeviceType.R :
                       (s == "ZR") ? PlcDeviceType.ZR :
                       (s == "W") ? PlcDeviceType.W :
                       (s == "SW") ? PlcDeviceType.SW :
                       (s == "TC") ? PlcDeviceType.TC :
                       (s == "TS") ? PlcDeviceType.TS :
                       (s == "TN") ? PlcDeviceType.TN :
                       (s == "CC") ? PlcDeviceType.CC :
                       (s == "CS") ? PlcDeviceType.CS :
                       (s == "CN") ? PlcDeviceType.CN :
                       (s == "SC") ? PlcDeviceType.SC :
                       (s == "SS") ? PlcDeviceType.SS :
                       (s == "SN") ? PlcDeviceType.SN :
                       (s == "Z") ? PlcDeviceType.Z :
                       (s == "TT") ? PlcDeviceType.TT :
                       (s == "TM") ? PlcDeviceType.TM :
                       (s == "CT") ? PlcDeviceType.CT :
                       (s == "CM") ? PlcDeviceType.CM :
                       (s == "A") ? PlcDeviceType.A :
                                     PlcDeviceType.Max;
            }

            // ====================================================================================
            public static bool IsBitDevice(PlcDeviceType type)
            {
                return !((type == PlcDeviceType.D)
                      || (type == PlcDeviceType.SD)
                      || (type == PlcDeviceType.Z)
                      || (type == PlcDeviceType.ZR)
                      || (type == PlcDeviceType.R)
                      || (type == PlcDeviceType.W));
            }

            // ====================================================================================
            public static bool IsHexDevice(PlcDeviceType type)
            {
                return (type == PlcDeviceType.X)
                    || (type == PlcDeviceType.Y)
                    || (type == PlcDeviceType.B)
                    || (type == PlcDeviceType.W);
            }

            // ====================================================================================
            public static void GetDeviceCode(string iDeviceName, out PlcDeviceType oType, out int oAddress)
            {
                string s = iDeviceName.ToUpper();
                string strAddress;

                // 1文字取り出す
                string strType = s.Substring(0, 1);
                switch (strType)
                {
                    case "A":
                    case "B":
                    case "D":
                    case "F":
                    case "L":
                    case "M":
                    case "R":
                    case "V":
                    case "W":
                    case "X":
                    case "Y":
                        // 2文字目以降は数値のはずなので変換する
                        strAddress = s.Substring(1);
                        break;
                    case "Z":
                        // もう1文字取り出す
                        strType = s.Substring(0, 2);
                        // ファイルレジスタの場合     : 2
                        // インデックスレジスタの場合 : 1
                        strAddress = s.Substring(strType.Equals("ZR") ? 2 : 1);
                        break;
                    case "C":
                        // もう1文字取り出す
                        strType = s.Substring(0, 2);
                        switch (strType)
                        {
                            case "CC":
                            case "CM":
                            case "CN":
                            case "CS":
                            case "CT":
                                strAddress = s.Substring(2);
                                break;
                            default:
                                throw new Exception("Invalid format.");
                        }
                        break;
                    case "S":
                        // もう1文字取り出す
                        strType = s.Substring(0, 2);
                        switch (strType)
                        {
                            case "SD":
                            case "SM":
                                strAddress = s.Substring(2);
                                break;
                            default:
                                throw new Exception("Invalid format.");
                        }
                        break;
                    case "T":
                        // もう1文字取り出す
                        strType = s.Substring(0, 2);
                        switch (strType)
                        {
                            case "TC":
                            case "TM":
                            case "TN":
                            case "TS":
                            case "TT":
                                strAddress = s.Substring(2);
                                break;
                            default:
                                throw new Exception("Invalid format.");
                        }
                        break;
                    default:
                        throw new Exception("Invalid format.");
                }

                oType = GetDeviceType(strType);
                oAddress = IsHexDevice(oType) ? Convert.ToInt32(strAddress, BlockSize) :
                                                Convert.ToInt32(strAddress);
            }
            // &&&&& protected &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            abstract protected Task<int> DoConnect();
            abstract protected void DoDisconnect();
            abstract protected Task<byte[]> Execute(byte[] iCommand);
            // &&&&& private &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            private const int BlockSize = 0x0010;
            private McCommand Command { get; set; }



            // ================================================================================
            private async Task<byte[]> TryExecution(byte[] iCommand, int minlength)
            {

                byte[] rtResponse;
                int tCount = 10;
                do
                {
                    rtResponse = await Execute(iCommand);
                    --tCount;
                    if (tCount < 0)
                    {
                        throw new Exception("PLCから正しい値が取得できません.");
                    }
                } while (Command.IsIncorrectResponse(rtResponse, minlength));
                return rtResponse;
            }
            // ####################################################################################
            // 通信に使用するコマンドを表現するインナークラス
            class McCommand
            {
                public McFrame FrameType { get; private set; }  // フレーム種別
                private uint SerialNumber { get; set; }  // シリアル番号
                private uint NetworkNumber { get; set; } // ネットワーク番号
                private uint PcNumber { get; set; }      // PC番号
                private uint IoNumber { get; set; }      // 要求先ユニットI/O番号
                private uint ChannelNumber { get; set; } // 要求先ユニット局番号
                private uint CpuTimer { get; set; }      // CPU監視タイマ
                private int ResultCode { get; set; }     // 終了コード
                public byte[] Response { get; private set; }    // 応答データ
                                                                // ================================================================================
                                                                // コンストラクタ
                public McCommand(McFrame iFrame)
                {
                    FrameType = iFrame;
                    SerialNumber = 0x0001u;
                    NetworkNumber = 0x0000u;
                    PcNumber = 0x00FFu;
                    IoNumber = 0x03FFu;
                    ChannelNumber = 0x0000u;
                    CpuTimer = 0x0010u;
                }
                // ================================================================================
                public byte[] SetCommandMC1E(byte Subheader, byte[] iData)
                {
                    List<byte> ret = new List<byte>(iData.Length + 4);
                    ret.Add(Subheader);
                    ret.Add((byte)this.PcNumber);
                    ret.Add((byte)CpuTimer);
                    ret.Add((byte)(CpuTimer >> 8));
                    ret.AddRange(iData);
                    return ret.ToArray();
                }
                public byte[] SetCommandMC3E(uint iMainCommand, uint iSubCommand, byte[] iData)
                {
                    var dataLength = (uint)(iData.Length + 6);
                    List<byte> ret = new List<byte>(iData.Length + 20);
                    uint frame = 0x0050u;
                    ret.Add((byte)frame);
                    ret.Add((byte)(frame >> 8));

                    ret.Add((byte)NetworkNumber);

                    ret.Add((byte)PcNumber);

                    ret.Add((byte)IoNumber);
                    ret.Add((byte)(IoNumber >> 8));
                    ret.Add((byte)ChannelNumber);
                    ret.Add((byte)dataLength);
                    ret.Add((byte)(dataLength >> 8));


                    ret.Add((byte)CpuTimer);
                    ret.Add((byte)(CpuTimer >> 8));
                    ret.Add((byte)iMainCommand);
                    ret.Add((byte)(iMainCommand >> 8));
                    ret.Add((byte)iSubCommand);
                    ret.Add((byte)(iSubCommand >> 8));

                    ret.AddRange(iData);
                    return ret.ToArray();
                }
                public byte[] SetCommandMC4E(uint iMainCommand, uint iSubCommand, byte[] iData)
                {
                    var dataLength = (uint)(iData.Length + 6);
                    var ret = new List<byte>(iData.Length + 20);
                    uint frame = 0x0054u;
                    ret.Add((byte)frame);
                    ret.Add((byte)(frame >> 8));
                    ret.Add((byte)SerialNumber);
                    ret.Add((byte)(SerialNumber >> 8));
                    ret.Add(0x00);
                    ret.Add(0x00);
                    ret.Add((byte)NetworkNumber);
                    ret.Add((byte)PcNumber);
                    ret.Add((byte)IoNumber);
                    ret.Add((byte)(IoNumber >> 8));
                    ret.Add((byte)ChannelNumber);
                    ret.Add((byte)dataLength);
                    ret.Add((byte)(dataLength >> 8));
                    ret.Add((byte)CpuTimer);
                    ret.Add((byte)(CpuTimer >> 8));
                    ret.Add((byte)iMainCommand);
                    ret.Add((byte)(iMainCommand >> 8));
                    ret.Add((byte)iSubCommand);
                    ret.Add((byte)(iSubCommand >> 8));

                    ret.AddRange(iData);
                    return ret.ToArray();
                }
                // ================================================================================
                public int SetResponse(byte[] iResponse)
                {
                    int min;
                    switch (FrameType)
                    {
                        case McFrame.MC1E:
                            min = 2;
                            if (min <= iResponse.Length)
                            {
                                //There is a subheader, end code and data.                                    

                                ResultCode = (int)iResponse[min - 2];
                                Response = new byte[iResponse.Length - 2];
                                Buffer.BlockCopy(iResponse, min, Response, 0, Response.Length);
                            }
                            break;
                        case McFrame.MC3E:
                            min = 11;
                            if (min <= iResponse.Length)
                            {
                                var btCount = new[] { iResponse[min - 4], iResponse[min - 3] };
                                var btCode = new[] { iResponse[min - 2], iResponse[min - 1] };
                                int rsCount = BitConverter.ToUInt16(btCount, 0);
                                ResultCode = BitConverter.ToUInt16(btCode, 0);
                                Response = new byte[rsCount - 2];
                                Buffer.BlockCopy(iResponse, min, Response, 0, Response.Length);
                            }
                            break;
                        case McFrame.MC4E:
                            min = 15;
                            if (min <= iResponse.Length)
                            {
                                var btCount = new[] { iResponse[min - 4], iResponse[min - 3] };
                                var btCode = new[] { iResponse[min - 2], iResponse[min - 1] };
                                int rsCount = BitConverter.ToUInt16(btCount, 0);
                                ResultCode = BitConverter.ToUInt16(btCode, 0);
                                Response = new byte[rsCount - 2];
                                Buffer.BlockCopy(iResponse, min, Response, 0, Response.Length);
                            }
                            break;
                        default:
                            throw new Exception("Frame type not supported.");

                    }
                    return ResultCode;
                }
                // ================================================================================
                public bool IsIncorrectResponse(byte[] iResponse, int minLenght)
                {
                    //TEST add 1E frame
                    switch (this.FrameType)
                    {
                        case McFrame.MC1E:
                            return ((iResponse.Length < minLenght));

                        case McFrame.MC3E:
                        case McFrame.MC4E:
                            var btCount = new[] { iResponse[minLenght - 4], iResponse[minLenght - 3] };
                            var btCode = new[] { iResponse[minLenght - 2], iResponse[minLenght - 1] };
                            var rsCount = BitConverter.ToUInt16(btCount, 0) - 2;
                            var rsCode = BitConverter.ToUInt16(btCode, 0);
                            return (rsCode == 0 && rsCount != (iResponse.Length - minLenght));

                        default:
                            throw new Exception("Type Not supported");

                    }
                }
            }
        }

        // ########################################################################################
        public class McProtocolTcp : McProtocolApp
        {
            // ====================================================================================
            // コンストラクタ
            public override bool Connected
            {
                get
                {
                    return Client.Connected;
                }
            }
            public McProtocolTcp() : this("", 0, McFrame.MC3E) { }
            public McProtocolTcp(string iHostName, int iPortNumber, McFrame frame)
                : base(iHostName, iPortNumber, frame)
            {
                CommandFrame = frame;
#if !old
                this.Host = new HostName(iHostName);

                this.streamSocket = new StreamSocket();

                this.Port = iPortNumber;
#endif
                Client = new TcpClient();
            }

            // &&&&& protected &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            async override protected Task<int> DoConnect()
            {
#if !old
                this.streamSocket.Control.KeepAlive = true;

                await this.streamSocket.ConnectAsync(this.Host, "" + this.Port);
#endif
#if old
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
#endif
                return 0;
            }
            // ====================================================================================
            override protected void DoDisconnect()
            {
#if !old
                this.streamSocket.Dispose();
#endif
#if old
                TcpClient c = Client;
                if (c.Connected)
                {
                    c.Close();
                }
#endif
            }
            // ================================================================================
            async override protected Task<byte[]> Execute(byte[] iCommand)
            {
                List<byte> list = new List<byte>();
#if windows

                //Write to the buffer


                await this.streamSocket.CancelIOAsync();

                Windows.Storage.Streams.DataWriter writer = new Windows.Storage.Streams.DataWriter(this.streamSocket.OutputStream);
                writer.WriteBytes(iCommand);
                await writer.StoreAsync();
                writer.DetachStream();

                
                //Read back from the buffer

                Windows.Storage.Streams.DataReader reader = new Windows.Storage.Streams.DataReader(this.streamSocket.InputStream);
                reader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;
                //Load 4 bytes off the buffer as the header
                //DONE Fix the read length
                //do
                //{
                await reader.LoadAsync(256);
                //if (reader.UnconsumedBufferLength == 0) break;
                byte[] bytes = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(bytes);//Should be 4
                list.AddRange(bytes);
                reader.DetachStream();
                //} while (true);
                return list.ToArray();
#endif

#if old

#endif
#if reading
                    await reader.LoadAsync(header[header.Length - 1]);//The last byte should be the remaining lenght
                    //
                    byte[] data = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(data);
                    byte[] buffer = new byte[header.Length + data.Length];
                    header.CopyTo(buffer, 0);
                    data.CopyTo(buffer, header.Length);
                    
#else


#endif



#if old

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
                    } while (ns.DataAvailable);
                    return ms.ToArray();
                }
#endif

            }
            // &&&&& private &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
#if !old
            private HostName Host { get; set; }
            private StreamSocket streamSocket { get; set; }
            private int Port { get; set; }
#endif
#if old
            private TcpClient Client { get; set; }
            private NetworkStream Stream { get; set; }
#endif
        }
#if udp
        // ########################################################################################
        public class McProtocolUdp : McProtocolApp
        {
            // ====================================================================================
            // コンストラクタ
            public McProtocolUdp(int iPortNumber) : this("", iPortNumber) { }
            public McProtocolUdp(string iHostName, int iPortNumber)
                : base(iHostName, iPortNumber)
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
        }
#endif
    }
}