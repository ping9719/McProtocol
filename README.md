# PMcProtocol

### 三菱plc通信MC协议的实现。此项目是McProtocol的分支。
##### This is a protocol for communicating with Mitsubishi PLCs. 
#

### 拷贝的原项目 [Copy to]
###### https://github.com/SecondShiftEngineer/McProtocol

### 原项目文档 [Original document]
###### https://www.nuget.org/packages/McProtocol#readme-body-tab

### 下载包 [download、install]
```CSharp
Install-Package PMcProtocol
```

### 开始使用 [How To Use]

```CSharp
using McProtocol.Mitsubishi;

//端口一般为6000
McProtocolTcp mcProtocolTcp = new McProtocolTcp("127.0.0.1", 6000, McFrame.MC3E);
await mcProtocolTcp.Open();

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


#
### 版本记录：[version history]
###### *表示部分代码可能与前版本不兼容 [*For some code is incompatible with previous versions]

### v2.1.7
###### 1.构造函数增加默认端口6000 [Default port]
### v2.1.6
###### 1.开放 PLCData[T].Bytes，可用于获取元数据 [Public PLCData[T].Bytes]
### v2.1.5
###### 1.对.Net5的支持 [Support for net5]
### v2.1.0
###### 1.增加读写锁，解决多线程网络拥堵的情况（在使用中请不要太频繁，可能会造成死锁） [add read/write lock,Solve network congestion under multi-threading]
### v2.0.1
###### 1.增加包注释 [add package comments]
### v2.0.0 *
###### 1.增加名称空间'McProtocol.Mitsubishi' [add using McProtocol.Mitsubishi]
###### 2.扩展接口IPlc方法 [interface IPlc add function]
###### 3.更新文档 [Update document]
### v1.2.7
###### 1.更新文档 [Update document]
### v1.2.6
###### 1.对net4.5的支持 [Support for net4.5]

