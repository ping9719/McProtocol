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

        /// <summary>
        /// 得到字节数据
        /// </summary>
        /// <param name="iDeviceName">如：M1000</param>
        /// <param name="iSize">读取长度：>0</param>
        /// <returns>数据</returns>
        Task<int[]> GetBitDevice(string iDeviceName, int iSize = 1);
        /// <summary>
        /// 得到字节数据
        /// </summary>
        /// <param name="iType">设备类型，如：M</param>
        /// <param name="iAddress">如：M1000</param>
        /// <param name="iSize">读取长度：>0</param>
        /// <returns>数据</returns>
        Task<int[]> GetBitDevice(PlcDeviceType iType, int iAddress, int iSize = 1);


        /// <summary>
        /// 设置字节数据
        /// </summary>
        /// <param name="iDeviceName">如：M1000</param>
        /// <param name="iData">设置的数据</param>
        Task SetBitDevice(string iDeviceName, params int[] iData);
        /// <summary>
        /// 设置字节数据
        /// </summary>
        /// <param name="iType">设备类型，如：M</param>
        /// <param name="iAddress">如：M1000</param>
        /// <param name="iData">设置的数据</param>
        Task SetBitDevice(PlcDeviceType iType, int iAddress, params int[] iData);


        /// <summary>
        /// 得到数据块
        /// </summary>
        /// <param name="iDeviceName">如：D1000</param>
        /// <param name="iSize">读取长度：>0</param>
        /// <returns>数据</returns>
        Task<int[]> ReadDeviceBlock(string iDeviceName, int iSize = 1);
        /// <summary>
        /// 得到数据块
        /// </summary>
        /// <param name="iType">设备类型，如：D</param>
        /// <param name="iAddress">如：D1000</param>
        /// <param name="iSize">读取长度：>0</param>
        /// <returns>数据</returns>
        Task<int[]> ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize = 1);


        /// <summary>
        /// 写入数据块
        /// </summary>
        /// <param name="iDeviceName">如：D1000</param>
        /// <param name="iData">写入10进制数据</param>
        Task WriteDeviceBlock(string iDeviceName, params int[] iData);
        /// <summary>
        /// 写入数据块
        /// </summary>
        /// <param name="iType">设备类型，如：D</param>
        /// <param name="iAddress">如：D1000</param>
        /// <param name="iData">写入10进制数据</param>
        Task WriteDeviceBlock(PlcDeviceType iType, int iAddress, params int[] iData);

        #endregion

        #region 扩展(v2.2)

        //Task<T> GetData<T>(string name);
        //void SetData<T>(string name, T data);
        //Task<T> GetData<T>(PlcDeviceType iType, int iAddress);
        //void SetData<T>(PlcDeviceType iType, int iAddress, T data);

        //Task<string> GetData(string name, int iSize, Encoding encoding, bool hiLoReversal = false);
        //Task<string> GetData(PlcDeviceType iType, int iAddress, int iSize, Encoding encoding, bool hiLoReversal = false);

        //Task<List<T>> GetData<T>(string name, int iSize);
        //void SetData<T>(string name, int iSize, T data);
        //Task<T> GetData<T>(PlcDeviceType iType, int iAddress, int iSize);
        //void SetData<T>(PlcDeviceType iType, int iAddress, int iSize, T data);
        #endregion
    }
}
