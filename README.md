# PMcProtocol

### 三菱plc通信MC协议的实现。此项目是McProtocol的分支。
##### This is a protocol for communicating with Mitsubishi PLCs. 
#

### 拷贝的原项目：[Copy to：]
### https://github.com/SecondShiftEngineer/McProtocol
#

### 原项目文档：[Original document：]
### https://www.nuget.org/packages/McProtocol#readme-body-tab
#

### 下载包 [download、install]
```CSharp
Install-Package PMcProtocol
```
#

### 列子:[ensample code:]
```CSharp
WindowsFormsApp1/Form1.cs
```
#

### 开始使用 [How To Use]

#### 方式1 [Method 1]
```CSharp
using McProtocol.Mitsubishi;

McProtocolTcp mcProtocolTcp = new McProtocolTcp("192.168.100.109", 1025, McFrame.MC3E);
await mcProtocolTcp.Open();

//通用方法（可能会淘汰，以后使用泛型的方式）
int[] oData1 = new int[10];//数据
await mcProtocolTcp.ReadDeviceBlock("D14520", 10, oData1);
int[] oData2 = new int[10];//数据
await mcProtocolTcp.GetBitDevice("M85", 10, oData2);

//版本v2.0以后的新扩展方法
var oDataNew1 = await mcProtocolTcp.ReadDeviceBlock("D7500", 10);//以int16方式读取10个
var oDataNew2 = await mcProtocolTcp.GetBitDevice("M85", 10);//以位方式读取10个
```

#### 方式2[Method 2]
```CSharp
using McProtocol;

//可能会淘汰，此方式存在一个程序只能存在一个的问题

//存储在静态字段 PLCData.PLC 中，全局使用
PLCData.PLC = new McProtocolTcp("192.168.100.109", 1025, McFrame.MC3E);
await PLCData.PLC.Open();

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
```


#
### 版本记录：[version history]
###### *表示部分代码可能与前版本不兼容 [*For some code is incompatible with previous versions]
## v2.1.6
###### 1.开放 PLCData[T].Bytes，可用于获取元数据 [Public PLCData[T].Bytes]
## v2.1.5
###### 1.对.Net5的支持 [Support for net5]
## v2.1.0
###### 1.增加读写锁，解决多线程网络拥堵的情况（在使用中请不要太频繁，可能会造成死锁） [add read/write lock,Solve network congestion under multi-threading]
## v2.0.1
###### 1.增加包注释 [add package comments]
## v2.0.0 *
###### 1.增加名称空间'McProtocol.Mitsubishi' [add using McProtocol.Mitsubishi]
###### 2.扩展接口IPlc方法 [interface IPlc add function]
###### 3.更新文档 [Update document]
## v1.2.7
###### 1.更新文档 [Update document]
## v1.2.6
###### 1.对net4.5的支持 [Support for net4.5]

