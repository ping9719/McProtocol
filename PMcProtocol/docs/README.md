
### 开始使用 [How To Use]

```CSharp
using PMcProtocol.Mitsubishi;

//端口一般为6000
McProtocolTcp mcProtocolTcp = new McProtocolTcp("127.0.0.1", 6000, McFrame.MC3E);
await mcProtocolTcp.Open();
```

```
//方式1（v>=2.0）
var oDataNew1 = await mcProtocolTcp.ReadDeviceBlock("D7500", 10);//以int16方式读取10个
var oDataNew2 = await mcProtocolTcp.GetBitDevice("M85", 10);//以位方式读取10个
await mcProtocolTcp.SetDevice("D7500", 1);//写
await mcProtocolTcp.SetBitDevice("M85", 1);//写

//方式2（历史遗留）
int[] oData1 = new int[10];//数据
await mcProtocolTcp.ReadDeviceBlock("D14520", 10, oData1);
int[] oData2 = new int[10];//数据
await mcProtocolTcp.GetBitDevice("M85", 10, oData2);

//方式3（历史遗留）
PLCData<bool> ints = new PLCData<bool>(PlcDeviceType.M, 85, 6);//使用位方式
await ints.ReadData();
var data = ints[0];//获取结果
ints[0] = !data;//修改数据
await ints.WriteData();//保存数据

PLCData<Int16> ints2 = new PLCData<Int16>(PlcDeviceType.D, 14520, 6);//使用16位整数方式
await ints2.ReadData();
var data2 = ints2[0];//获取结果
ints2[0] = 3;//修改数据
await ints2.WriteData();//写入数据
```