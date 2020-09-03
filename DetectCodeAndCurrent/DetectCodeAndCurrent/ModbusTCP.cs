using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HslCommunication.ModBus;
using HslCommunication;

namespace DetectCodeAndCurrent {
    class MyModbusTCP {
        //private ModbusUdpNet 
        private ModbusTcpNet busTcpClient;   // 实例化
        private bool connectState;
        public bool IsConnect{
            get {
                return connectState;
            }
        }
        public MyModbusTCP(string IpAdress) {
            busTcpClient = new ModbusTcpNet(IpAdress);   // 实例化
        }

        public bool Connect()
        {
            busTcpClient?.ConnectClose();
            OperateResult connect = busTcpClient.ConnectServer();
            if (connect.IsSuccess)
            {
                connectState = true;
                return true;
            }
            else
            {
                connectState = false;
                return false;
            }
        }

        public void DisConnect() {
            busTcpClient?.ConnectClose();
        }
        // 本类支持的读写操作提供了非常多的重载方法，总有你想要的方法
        public bool ReadCoil(string coilAdress) {
            OperateResult<bool> result_coil = busTcpClient.ReadCoil(coilAdress);
            if (result_coil.IsSuccess)
            {
                bool value = result_coil.Content;
                return value;
            }
            else
            {
                Exception ex = new Exception("error");
                throw  ex;
            }
        }
        public ushort ReadRegist(string channel) {
            ushort ushortRegist = busTcpClient.ReadUInt16("x=3;"+ channel).Content; // 读取寄存器100的ushort值
            return ushortRegist;

        }

        public ushort Read(string channel)
        {
            ushort ushortResult = busTcpClient.ReadUInt16("x=4;" + channel).Content; // 读取寄存器100的ushort值
            return ushortResult;
        }


        public bool WriteCoil(string channel,bool value)
        {
            if (busTcpClient.Write(channel, value).IsSuccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool WriteRegist(string channel,ushort value) {
            if (busTcpClient.Write(channel, value).IsSuccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CoilExample() {
            // 读取线圈示例
            bool coil100 = busTcpClient.ReadCoil("100").Content;

            // 判断是否读取成功
            OperateResult<bool> result_coil100 = busTcpClient.ReadCoil("100");
            if (result_coil100.IsSuccess) {
                // success
                bool value = result_coil100.Content;
            }
            else {
                // failed
            }


            // 假设读取站号10的线圈100的值
            bool coil_station_ten_100 = busTcpClient.ReadCoil("s=10;100").Content;



            // =============================================================================================
            // 写入也是同理，线圈100写通
            busTcpClient.Write("100", true);
          

            // 站号10的线圈写通
            busTcpClient.Write("s=10;100", true);

            // 想要判断是否写入成功
            if (busTcpClient.Write("s=10;100", true).IsSuccess) {
                // success
            }
            else {
                // failed
            }



            // ===========================================================================================
            // 批量读写也是类似，批量的读取
            bool[] coil10_19 = busTcpClient.ReadCoil("100", 10).Content;

            // 写入也是同理
            busTcpClient.Write("100", new bool[] { true, false, true, false, false, false, true, false, false, false });


            // 离散输入的数据读取同理
        }


        private void RegisterExample() {
            // 读取寄存器100的值
            short register100 = busTcpClient.ReadInt16("100").Content;

            // 批量读取寄存器100-109的值
            short[] register100_109 = busTcpClient.ReadInt16("100", 10).Content;

            // 写入寄存器100的值，注意，一定要强制转换short类型
            busTcpClient.Write("100", (short)123);

            // 批量写
            busTcpClient.Write("100", new short[] { 123, -123, 4244 });


            // ==============================================================================================
            // 以下是一些常规的操作，不再对是否成功的结果进行判断
            // 读取操作

            bool coil100 = busTcpClient.ReadCoil("100").Content;   // 读取线圈100的通断
            short short100 = busTcpClient.ReadInt16("100").Content; // 读取寄存器100的short值
            ushort ushort100 = busTcpClient.ReadUInt16("100").Content; // 读取寄存器100的ushort值
            int int100 = busTcpClient.ReadInt32("100").Content;      // 读取寄存器100-101的int值
            uint uint100 = busTcpClient.ReadUInt32("100").Content;   // 读取寄存器100-101的uint值
            float float100 = busTcpClient.ReadFloat("100").Content; // 读取寄存器100-101的float值
            long long100 = busTcpClient.ReadInt64("100").Content;    // 读取寄存器100-103的long值
            ulong ulong100 = busTcpClient.ReadUInt64("100").Content; // 读取寄存器100-103的ulong值
            double double100 = busTcpClient.ReadDouble("100").Content; // 读取寄存器100-103的double值
            string str100 = busTcpClient.ReadString("100", 5).Content;// 读取100到104共10个字符的字符串

            // 写入操作
            busTcpClient.Write("100", true);// 写入线圈100为通
            busTcpClient.Write("100", (short)12345);// 写入寄存器100为12345
            busTcpClient.Write("100", (ushort)45678);// 写入寄存器100为45678
            busTcpClient.Write("100", 123456789);// 写入寄存器100-101为123456789
            busTcpClient.Write("100", (uint)123456778);// 写入寄存器100-101为123456778
            busTcpClient.Write("100", 123.456);// 写入寄存器100-101为123.456
            busTcpClient.Write("100", 12312312312414L);//写入寄存器100-103为一个大数据
            busTcpClient.Write("100", 12634534534543656UL);// 写入寄存器100-103为一个大数据
            busTcpClient.Write("100", 123.456d);// 写入寄存器100-103为一个双精度的数据
            busTcpClient.Write("100", "K123456789");

            // ===============================================================================================
            // 读取输入寄存器
            short input_short100 = busTcpClient.ReadInt16("x=4;100").Content; // 读取寄存器100的short值
            ushort input_ushort100 = busTcpClient.ReadUInt16("x=4;100").Content; // 读取寄存器100的ushort值
            int input_int100 = busTcpClient.ReadInt32("x=4;100").Content;      // 读取寄存器100-101的int值
            uint input_uint100 = busTcpClient.ReadUInt32("x=4;100").Content;   // 读取寄存器100-101的uint值
            float input_float100 = busTcpClient.ReadFloat("x=4;100").Content; // 读取寄存器100-101的float值
            long input_long100 = busTcpClient.ReadInt64("x=4;100").Content;    // 读取寄存器100-103的long值
            ulong input_ulong100 = busTcpClient.ReadUInt64("x=4;100").Content; // 读取寄存器100-103的ulong值
            double input_double100 = busTcpClient.ReadDouble("x=4;100").Content; // 读取寄存器100-103的double值
            string input_str100 = busTcpClient.ReadString("x=4;100", 5).Content;// 读取100到104共10个字符的字符串
        }
    }
}
