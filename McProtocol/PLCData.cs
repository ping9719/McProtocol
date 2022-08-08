using McProtocol.Help;
using McProtocol.Mitsubishi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McProtocol
{
    public class PLCData
    {
        public static IPlc PLC;

    }

    /// <summary>
    /// 初始化。T：Boolean，Int16，Int32，UInt16，UInt32，Single，Double，Char
    /// </summary>
    /// <typeparam name="T">T：Boolean，Int16，Int32，UInt16，UInt32，Single，Double，Char</typeparam>
    public class PLCData<T> : PLCData
    {
        PlcDeviceType DeviceType;
        int Address;
        int Length;
        int LENGTH;//Length in bytes
        /// <summary>
        /// 读取到的数据
        /// </summary>
        public byte[] Bytes { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <param name="Address"></param>
        /// <param name="Length"></param>
        public PLCData(PlcDeviceType DeviceType, int Address, int Length = 1)
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
            this.Bytes = new byte[this.LENGTH];

        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="i">数据所在索引</param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                Union u = new Union();
                string t = typeof(T).Name;
                switch (t)
                {
                    case "Boolean":
                        return (T)Convert.ChangeType(((this.Bytes[i / 8] >> (i % 8)) % 2 == 1), typeof(T));
                    case "Int32":
                        u.a = this.Bytes[i * 4];
                        u.b = this.Bytes[i * 4 + 1];
                        u.c = this.Bytes[i * 4 + 2];
                        u.d = this.Bytes[i * 4 + 3];
                        return (T)Convert.ChangeType(u.DINT, typeof(T));
                    case "Int16":
                        u.a = this.Bytes[i * 2];
                        u.b = this.Bytes[i * 2 + 1];
                        return (T)Convert.ChangeType(u.INT, typeof(T));
                    case "UInt16":
                        u.a = this.Bytes[i * 2];
                        u.b = this.Bytes[i * 2 + 1];
                        return (T)Convert.ChangeType(u.UINT, typeof(T));
                    case "UInt32":
                        u.a = this.Bytes[i * 4];
                        u.b = this.Bytes[i * 4 + 1];
                        u.c = this.Bytes[i * 4 + 2];
                        u.d = this.Bytes[i * 4 + 3];
                        return (T)Convert.ChangeType(u.UDINT, typeof(T));
                    case "Single":
                        u.a = this.Bytes[i * 4];
                        u.b = this.Bytes[i * 4 + 1];
                        u.c = this.Bytes[i * 4 + 2];
                        u.d = this.Bytes[i * 4 + 3];
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
                        if (arg && (this.Bytes[i / 8] >> (i % 8)) % 2 == 0)
                            this.Bytes[i / 8] += (byte)(1 << (i % 8));
                        else if (!arg && (this.Bytes[i / 8] >> (i % 8)) % 2 == 1)
                            this.Bytes[i / 8] -= (byte)(1 << (i % 8));
                        return;
                    case "Int32":
                        u.DINT = Convert.ToInt32(value);
                        this.Bytes[i * 4] = u.a;
                        this.Bytes[i * 4 + 1] = u.b;
                        this.Bytes[i * 4 + 2] = u.c;
                        this.Bytes[i * 4 + 3] = u.d;
                        return;
                    case "Int16":
                        u.INT = Convert.ToInt16(value);
                        this.Bytes[i * 2] = u.a;
                        this.Bytes[i * 2 + 1] = u.b;
                        return;
                    case "UInt32":
                        u.UDINT = Convert.ToUInt32(value);
                        this.Bytes[i * 4] = u.a;
                        this.Bytes[i * 4 + 1] = u.b;
                        this.Bytes[i * 4 + 2] = u.c;
                        this.Bytes[i * 4 + 3] = u.d;
                        return;
                    case "UInt16":
                        u.UINT = Convert.ToUInt16(value);
                        this.Bytes[i * 2] = u.a;
                        this.Bytes[i * 2] = u.b;
                        return;
                    case "Single":
                        u.REAL = Convert.ToSingle(value);
                        this.Bytes[i * 4] = u.a;
                        this.Bytes[i * 4 + 1] = u.b;
                        this.Bytes[i * 4 + 2] = u.c;
                        this.Bytes[i * 4 + 3] = u.d;
                        return;
                    default:
                        throw new Exception("Type not recognized.");
                }
            }
        }

        /// <summary>
        /// 开始读取数据
        /// </summary>
        /// <returns></returns>
        public async Task ReadData()
        {
            this.Bytes = await PLC.ReadDeviceBlockByte(DeviceType, this.Address, this.Length);
        }

        /// <summary>
        /// 开始写入数据
        /// </summary>
        /// <returns></returns>
        public async Task WriteData()
        {
            await PLC.WriteDeviceBlockByte(this.DeviceType, this.Address, Length, Bytes);
        }


        ///// <summary>
        ///// 将Bytes转为字符串
        ///// </summary>
        ///// <param name="encoding">编码格式，如：Encoding.UTF8</param>
        ///// <param name="hiLoReversal">高低位是否反转</param>
        ///// <returns>字符串</returns>
        //public string GetStringData(Encoding encoding, bool hiLoReversal = false)
        //{
        //    return ByteHelp.ByteToString(Bytes, encoding, hiLoReversal);
        //}
    }

}
