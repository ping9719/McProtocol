using McProtocol;
using McProtocol.Mitsubishi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Method 1

        McProtocolTcp mcProtocolTcp;
        private async void button1_Click(object sender, EventArgs e)
        {
            mcProtocolTcp = new McProtocolTcp("192.168.100.109", 1025, McFrame.MC3E);
            await mcProtocolTcp.Open();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //通用方法
            int[] oData1 = new int[10];//数据
            await mcProtocolTcp.ReadDeviceBlock("D14520", 10, oData1);
            int[] oData2 = new int[10];//数据
            await mcProtocolTcp.GetBitDevice("M85", 10, oData2);

            //版本v2.0以后的新扩展方法
            var oDataNew1 = await mcProtocolTcp.ReadDeviceBlock("D7500", 10);//以int16方式读取10个
            var oDataNew2 = await mcProtocolTcp.GetBitDevice("M85", 10);//以位方式读取10个
        }
        #endregion

        #region Method 2

        //此方式存储在静态字段 PLCData.PLC 中，全局使用
        //此方式的源文档：https://www.nuget.org/packages/McProtocol
        private async void button3_Click(object sender, EventArgs e)
        {
            PLCData.PLC = new McProtocolTcp("192.168.100.109", 1025, McFrame.MC3E);
            await PLCData.PLC.Open();
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            //使用位方式
            PLCData<bool> ints = new PLCData<bool>(PlcDeviceType.M, 85, 6);
            await ints.ReadData();
            //获取结果
            var data = ints[0];
            //修改数据
            ints[0] = !data;
            //保存数据
            await ints.WriteData();


            //使用16位整数方式
            PLCData<Int16> ints2 = new PLCData<Int16>(PlcDeviceType.D, 14520, 6);
            await ints2.ReadData();
            //获取结果
            var data2 = ints2[0];
            //修改数据
            ints2[0] = 3;
            //写入数据
            await ints2.WriteData();

        }

        #endregion




    }
}
