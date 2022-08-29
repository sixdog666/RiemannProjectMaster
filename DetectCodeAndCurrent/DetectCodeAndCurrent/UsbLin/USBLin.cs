using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using USB2XXX;

namespace DetectCodeAndCurrent
{
   public class USBLin
    {
        public bool ConnectState { get; set; }
        usb_device.DEVICE_INFO DevInfo = new usb_device.DEVICE_INFO();
        Int32[] DevHandles = new Int32[20];
        Int32 DevHandle = 0;
#if DEBUG
        Byte LINIndex = 0;
#else
        Byte LINIndex = 1;
#endif

        bool state;
        Int32 DevNum, ret = 0;
        String[] MSGTypeStr = new String[10] { "UN", "MW", "MR", "SW", "SR", "BK", "SY", "ID", "DT", "CK" };
        String[] CKTypeStr = new String[5] { "STD", "EXT", "USER", "NONE", "ERROR" };
        public void OpenDevcie() {
           
            //扫描查找设备
            DevNum = usb_device.USB_ScanDevice(DevHandles);
            if (DevNum <= 0)
            {
                Console.WriteLine("No device connected!");
                ConnectState = false;
                return ;
            }
            else
            {
                Console.WriteLine("Have {0} device connected!", DevNum);
            }
            DevHandle = DevHandles[0];
            //打开设备
            state = usb_device.USB_OpenDevice(DevHandle);
            if (!state)
            {
                Console.WriteLine("Open device error!");
                ConnectState = false;
                return ;
            }
            else
            {
                Console.WriteLine("Open device success!");
            }
            //获取固件信息
            StringBuilder FuncStr = new StringBuilder(256);
            state = usb_device.DEV_GetDeviceInfo(DevHandle, ref DevInfo, FuncStr);
            if (!state)
            {
                Console.WriteLine("Get device infomation error!");
                ConnectState = false;
                return ;
            }
            else
            {
                Console.WriteLine("Firmware Info:");
                Console.WriteLine("    Name:" + Encoding.Default.GetString(DevInfo.FirmwareName));
                Console.WriteLine("    Build Date:" + Encoding.Default.GetString(DevInfo.BuildDate));
                Console.WriteLine("    Firmware Version:v{0}.{1}.{2}", (DevInfo.FirmwareVersion >> 24) & 0xFF, (DevInfo.FirmwareVersion >> 16) & 0xFF, DevInfo.FirmwareVersion & 0xFFFF);
                Console.WriteLine("    Hardware Version:v{0}.{1}.{2}", (DevInfo.HardwareVersion >> 24) & 0xFF, (DevInfo.HardwareVersion >> 16) & 0xFF, DevInfo.HardwareVersion & 0xFFFF);
                Console.WriteLine("    Functions:" + DevInfo.Functions.ToString("X8"));
                Console.WriteLine("    Functions String:" + FuncStr);
            }
            ConnectState = true;
          

        }
        //主机模式
        public bool InitDevice(int bautRate= 10417) {

            ret = USB2LIN_EX.LIN_EX_Init(DevHandle, LINIndex, bautRate, 1);

            if (ret != USB2LIN_EX.LIN_EX_SUCCESS)
            {
                Console.WriteLine("Config LIN failed!");
                return false;
            }
            else
            {
                Console.WriteLine("Config LIN Success!");
                
                return true;
            }
        }
        public void MasterBreak() {
           
            int MsgIndex = 0;
            Byte[] DataBuffer;
            USB2LIN_EX.LIN_EX_MSG[] LINMsg = new USB2LIN_EX.LIN_EX_MSG[5];
            USB2LIN_EX.LIN_EX_MSG[] LINOutMsg = new USB2LIN_EX.LIN_EX_MSG[10];
            //添加第一帧数据
            LINMsg[MsgIndex] = new USB2LIN_EX.LIN_EX_MSG();
            LINMsg[MsgIndex].MsgType = USB2LIN_EX.LIN_EX_MSG_TYPE_BK;//只发送BREAK信号，一般用于唤醒休眠中的从设备
            LINMsg[MsgIndex].Timestamp = 10;//发送该帧数据之后的延时时间，最小建议设置为1
            MsgIndex++;

            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN_EX.LIN_EX_MSG)) * MsgIndex);
            ret = USB2LIN_EX.LIN_EX_MasterSync(DevHandle, LINIndex, LINMsg, pt, MsgIndex);
            if (ret < USB2LIN_EX.LIN_EX_SUCCESS)
            {
                //Console.WriteLine("MasterSync LIN failed!");
                //释放内存
                Marshal.FreeHGlobal(pt);
                return;
            }

            //释放内存
            Marshal.FreeHGlobal(pt);
        }

        public void MasterWrite( byte[] data) {
            /************************************主机写数据************************************/
            int MsgIndex = 0;
            Byte[] DataBuffer;
            USB2LIN_EX.LIN_EX_MSG[] LINMsg = new USB2LIN_EX.LIN_EX_MSG[5];
            USB2LIN_EX.LIN_EX_MSG[] LINOutMsg = new USB2LIN_EX.LIN_EX_MSG[10];
           
            //添加第一帧数据
            LINMsg[MsgIndex] = new USB2LIN_EX.LIN_EX_MSG();
            LINMsg[MsgIndex].MsgType = USB2LIN_EX.LIN_EX_MSG_TYPE_MW;//主机发送数据
            LINMsg[MsgIndex].DataLen = 7;//实际要发送的数据字节数
            LINMsg[MsgIndex].Timestamp = 10;//发送该帧数据之后的延时时间
            LINMsg[MsgIndex].CheckType = USB2LIN_EX.LIN_EX_CHECK_EXT;//增强校验
            LINMsg[MsgIndex].PID = 0x20;//可以只传入ID，校验位底层会自动计算
            LINMsg[MsgIndex].Data = new Byte[8];//必须分配8字节空间
#if DEBUG
            DataBuffer = new Byte[7] { 0x64, 0x80, 0xE4, 0xE4, 0xE4, 0xD0, 0xC0 };
#else
           DataBuffer = new Byte[7] { 0x00, 0x00, 0x00, 0x80, 0x80, 0x10, 0x09 };
#endif
            for (int i = 0; i < LINMsg[MsgIndex].DataLen; i++)//循环填充8字节数据
            {
                LINMsg[MsgIndex].Data[i] = DataBuffer[i];
            }
            MsgIndex++;

            //将数组转换成指针
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN_EX.LIN_EX_MSG)) * MsgIndex);
            ret = USB2LIN_EX.LIN_EX_MasterSync(DevHandle, LINIndex, LINMsg, pt, MsgIndex);
            if (ret < USB2LIN_EX.LIN_EX_SUCCESS)
            {
                //Console.WriteLine("MasterSync LIN failed!");
                //释放内存
                Marshal.FreeHGlobal(pt);
                return;
            }
            else
            {
                //主机发送数据成功后，也会接收到发送出去的数据，通过接收回来的数据跟发送出去的数据对比，可以判断发送数据的时候，数据是否被冲突
                Console.WriteLine("MsgLen = {0}", ret);
                for (int i = 0; i < ret; i++)
                {
                    LINOutMsg[i] = (USB2LIN_EX.LIN_EX_MSG)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(USB2LIN_EX.LIN_EX_MSG))), typeof(USB2LIN_EX.LIN_EX_MSG));
                    //Console.Write("{0} SYNC[{1:X2}] PID[{2:X2}] ", MSGTypeStr[LINOutMsg[i].MsgType], LINOutMsg[i].Sync, LINOutMsg[i].PID);
                    for (int j = 0; j < LINOutMsg[i].DataLen; j++)
                    {
                      //  Console.Write("{0:X2} ", LINOutMsg[i].Data[j]);
                    }
                   // Console.WriteLine("[{0}][{1:X2}] [{2}:{3}:{4}.{5}]", CKTypeStr[LINOutMsg[i].CheckType], LINOutMsg[i].Check, (LINOutMsg[i].Timestamp / 36000000) % 60, (LINOutMsg[i].Timestamp / 600000) % 60, (LINOutMsg[i].Timestamp / 10000) % 60, (LINOutMsg[i].Timestamp / 10) % 1000);
                }

            }
            //释放内存
            Marshal.FreeHGlobal(pt);
        }
        public byte[] MasterRead() {

            int MsgIndex = 0;
            Byte[] DataBuffer;
            USB2LIN_EX.LIN_EX_MSG[] LINMsg = new USB2LIN_EX.LIN_EX_MSG[5];
            USB2LIN_EX.LIN_EX_MSG[] LINOutMsg = new USB2LIN_EX.LIN_EX_MSG[512];
            /************************************主机读数据************************************/
            MsgIndex = 0;
            
            LINMsg[MsgIndex] = new USB2LIN_EX.LIN_EX_MSG();
            LINMsg[MsgIndex].MsgType = USB2LIN_EX.LIN_EX_MSG_TYPE_MR;//主机读数据
            LINMsg[MsgIndex].Timestamp = 10;//发送该帧数据之后的延时时间
            LINMsg[MsgIndex].PID = 0x21;//可以只传入ID，校验位底层会自动计算
            MsgIndex++;
            /********************需要发送更多帧数据，请按照前面的方式继续添加********************/
            //将数组转换成指针
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USB2LIN_EX.LIN_EX_MSG)) * MsgIndex);
            try
            {
                ret = USB2LIN_EX.LIN_EX_MasterSync(DevHandle, LINIndex, LINMsg, pt, MsgIndex);
                if (ret < USB2LIN_EX.LIN_EX_SUCCESS)
                {
                   // Console.WriteLine("MasterSync LIN failed!");
                    //释放内存
                    Marshal.FreeHGlobal(pt);
                    return null;
                }
                else
                {
                    //主机发送数据成功后，也会接收到发送出去的数据，通过接收回来的数据跟发送出去的数据对比，可以判断发送数据的时候，数据是否被冲突
                    Console.WriteLine("MsgLen = {0}", ret);
                    for (int i = 0; i < ret; i++)
                    {
                        LINOutMsg[i] = (USB2LIN_EX.LIN_EX_MSG)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(USB2LIN_EX.LIN_EX_MSG))), typeof(USB2LIN_EX.LIN_EX_MSG));
                      //  Console.Write("{0} SYNC[{1:X2}] PID[{2:X2}] ", MSGTypeStr[LINOutMsg[i].MsgType], LINOutMsg[i].Sync, LINOutMsg[i].PID);
                        DataBuffer = new byte[LINOutMsg[i].DataLen];
                        for (int j = 0; j < LINOutMsg[i].DataLen; j++)//实际读取到的字节数据是LINOutMsg[i].DataLen的值
                        {
                           // Console.Write("{0:X2} ", LINOutMsg[i].Data[j]);
                            DataBuffer[j] = LINOutMsg[i].Data[j];
                        }
                        Marshal.FreeHGlobal(pt);
                        return DataBuffer;
                    }
                    //Marshal.FreeHGlobal(pt);
                    //return null;

                }
                //释放内存
                Marshal.FreeHGlobal(pt);
                return null;
            }
            catch (Exception) {
               
                Marshal.FreeHGlobal(pt);
                return null;
            }
            
        }
        public void CloseDevice() {
            Console.WriteLine("Close Device!");
            if (ConnectState == true) {
                //关闭设备
                usb_device.USB_CloseDevice(DevHandle);
            }
           
        }
    }

    
}
