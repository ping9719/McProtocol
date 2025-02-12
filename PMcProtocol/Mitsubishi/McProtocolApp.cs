using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McProtocol.Mitsubishi
{
    /// <summary>
    /// 
    /// </summary>
    abstract public class McProtocolApp : IPlc
    {
        public abstract bool Connected { get; }
        /// <summary>
        /// 使用フレーム
        /// </summary>
        public McFrame CommandFrame { get; set; }
        /// <summary>
        /// ホスト名またはIPアドレス
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// ポート番号
        /// </summary>
        public int PortNumber { get; set; }
        public int Device { private set; get; }

        private const int BlockSize = 0x0010;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iHostName"></param>
        /// <param name="iPortNumber"></param>
        /// <param name="frame"></param>
        protected McProtocolApp(string iHostName, int iPortNumber, McFrame frame)
        {
            CommandFrame = frame;
            //C70 = MC3E

            HostName = iHostName;
            PortNumber = iPortNumber;
        }

        /// <summary>
        /// 後処理
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        public async Task<int> Open()
        {
            await DoConnect();
            Command = new McCommand(CommandFrame);
            return 0;
        }

        public int Close()
        {
            DoDisconnect();
            return 0;
        }



        public async Task<int> SetBitDevice(string iDeviceName, int iSize, int[] iData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return await SetBitDevice(type, addr, iSize, iData);
        }

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

        public async Task<int> GetBitDevice(string iDeviceName, int iSize, int[] oData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return await GetBitDevice(type, addr, iSize, oData);
        }

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

        public async Task<int> WriteDeviceBlock(string iDeviceName, int iSize, int[] iData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return await WriteDeviceBlock(type, addr, iSize, iData);
        }

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

        public async Task<int> WriteDeviceBlockByte(PlcDeviceType iType, int iAddress, int devicePoints, byte[] bData)
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

        public async Task<byte[]> ReadDeviceBlock(string iDeviceName, int iSize, int[] oData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return await ReadDeviceBlock(type, addr, iSize, oData);
        }

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

        public async Task<byte[]> ReadDeviceBlockByte(PlcDeviceType iType, int iAddress, int devicePoints)
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

        public async Task<int> SetDevice(string iDeviceName, int iData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return await SetDevice(type, addr, iData);
        }

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

        public async Task<int> GetDevice(string iDeviceName)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return await GetDevice(type, addr);
        }

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

        #region IPlcEx
        /// <summary>
        /// 得到字节数据
        /// </summary>
        /// <param name="iDeviceName">如：M1000</param>
        /// <param name="iSize">读取长度：>0</param>
        /// <returns>数据</returns>
        public async Task<int[]> GetBitDevice(string iDeviceName, int iSize = 1)
        {
            int[] oData = new int[iSize];
            await GetBitDevice(iDeviceName, iSize, oData);
            return oData;
        }
        /// <summary>
        /// 得到字节数据
        /// </summary>
        /// <param name="iType">设备类型，如：M</param>
        /// <param name="iAddress">如：M1000</param>
        /// <param name="iSize">读取长度：>0</param>
        /// <returns>数据</returns>
        public async Task<int[]> GetBitDevice(PlcDeviceType iType, int iAddress, int iSize = 1)
        {
            int[] oData = new int[iSize];
            await GetBitDevice(iType, iAddress, iSize, oData);
            return oData;
        }
        /// <summary>
        /// 设置字节数据
        /// </summary>
        /// <param name="iDeviceName">如：M1000</param>
        /// <param name="iData">设置的数据</param>
        public async Task SetBitDevice(string iDeviceName, params int[] iData)
        {
            await SetBitDevice(iDeviceName, iData.Length, iData);
        }
        /// <summary>
        /// 设置字节数据
        /// </summary>
        /// <param name="iType">设备类型，如：M</param>
        /// <param name="iAddress">如：M1000</param>
        /// <param name="iData">设置的数据</param>
        public async Task SetBitDevice(PlcDeviceType iType, int iAddress, params int[] iData)
        {
            await SetBitDevice(iType, iAddress, iData.Length, iData);
        }
        /// <summary>
        /// 得到数据块
        /// </summary>
        /// <param name="iDeviceName">如：D1000</param>
        /// <param name="iSize">读取长度：>0</param>
        /// <returns>数据</returns>
        public async Task<int[]> ReadDeviceBlock(string iDeviceName, int iSize = 1)
        {
            int[] oData = new int[iSize];
            await ReadDeviceBlock(iDeviceName, iSize, oData);
            return oData;
        }
        /// <summary>
        /// 得到数据块
        /// </summary>
        /// <param name="iType">设备类型，如：D</param>
        /// <param name="iAddress">如：D1000</param>
        /// <param name="iSize">读取长度：>0</param>
        /// <returns>数据</returns>
        public async Task<int[]> ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize = 1)
        {
            int[] oData = new int[iSize];
            await ReadDeviceBlock(iType, iAddress, iSize, oData);
            return oData;
        }
        /// <summary>
        /// 写入数据块
        /// </summary>
        /// <param name="iDeviceName">如：D1000</param>
        /// <param name="iData">写入10进制数据</param>
        public async Task WriteDeviceBlock(string iDeviceName, params int[] iData)
        {
            await WriteDeviceBlock(iDeviceName, iData.Length, iData);
        }
        /// <summary>
        /// 写入数据块
        /// </summary>
        /// <param name="iType">设备类型，如：D</param>
        /// <param name="iAddress">如：D1000</param>
        /// <param name="iData">写入10进制数据</param>
        public async Task WriteDeviceBlock(PlcDeviceType iType, int iAddress, params int[] iData)
        {
            await WriteDeviceBlock(iType, iAddress, iData.Length, iData);
        }
        #endregion

        //public int GetCpuType(out string oCpuName, out int oCpuType)
        //{
        //    int rtCode = Command.Execute(0x0101, 0x0000, new byte[0]);
        //    oCpuName = "dummy";
        //    oCpuType = 0;
        //    return rtCode;
        //}

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

        public static bool IsBitDevice(PlcDeviceType type)
        {
            return !((type == PlcDeviceType.D)
                  || (type == PlcDeviceType.SD)
                  || (type == PlcDeviceType.Z)
                  || (type == PlcDeviceType.ZR)
                  || (type == PlcDeviceType.R)
                  || (type == PlcDeviceType.W));
        }

        public static bool IsHexDevice(PlcDeviceType type)
        {
            return (type == PlcDeviceType.X)
                || (type == PlcDeviceType.Y)
                || (type == PlcDeviceType.B)
                || (type == PlcDeviceType.W);
        }

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
            oAddress = IsHexDevice(oType) ? Convert.ToInt32(strAddress, BlockSize) : Convert.ToInt32(strAddress);
        }

        abstract protected Task<int> DoConnect();
        abstract protected void DoDisconnect();
        abstract protected Task<byte[]> Execute(byte[] iCommand);
        private McCommand Command { get; set; }

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
}
