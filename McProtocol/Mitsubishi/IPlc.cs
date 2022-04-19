using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McProtocol.Mitsubishi
{
    /// <summary>
    /// PLCと接続するための共通のインターフェースを定義する
    /// </summary>
    public interface IPlc : IDisposable
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

        Task<byte[]> ReadDeviceBlock(string iDeviceName, int iSize, int[] oData);
        Task<byte[]> ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] oData);

        Task<byte[]> ReadDeviceBlockByte(PlcDeviceType iType, int iAddress, int iSize);
        Task<int> WriteDeviceBlockByte(PlcDeviceType iType, int iAddress, int iSize, byte[] bData);

        Task<int> SetDevice(string iDeviceName, int iData);
        Task<int> SetDevice(PlcDeviceType iType, int iAddress, int iData);
        Task<int> GetDevice(string iDeviceName);
        Task<int> GetDevice(PlcDeviceType iType, int iAddress);

        #region 扩展(v2.0)

        Task<int[]> GetBitDevice(string iDeviceName, int iSize = 1);
        Task<int[]> GetBitDevice(PlcDeviceType iType, int iAddress, int iSize = 1);

        Task SetBitDevice(string iDeviceName, params int[] iData);
        Task SetBitDevice(PlcDeviceType iType, int iAddress, params int[] iData);

        Task<int[]> ReadDeviceBlock(string iDeviceName, int iSize = 1);
        Task<int[]> ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize = 1);

        Task WriteDeviceBlock(string iDeviceName, params int[] iData);
        Task WriteDeviceBlock(PlcDeviceType iType, int iAddress, params int[] iData);

        #endregion
    }
}
