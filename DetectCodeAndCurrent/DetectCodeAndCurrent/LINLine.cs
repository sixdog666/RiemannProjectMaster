using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DetectCodeAndCurrent {
    public enum eCommType {
        Master,
        Slave
    }

    public enum INFO_PACKET_PORTOCOL {
        HEAD1,
        HEAD2,
        FUNCTION_CODE,
        INFO,
        MASTER_OR_SLAVE,
        ERROR_FRAME_SHIELD,
        OPEN_SENDBACK,
        OPEN_RECEIVEBACK,
        BAUD_RATE_1,
        BAUD_RATE_2,
        BAUD_RATE_3,
        BAUD_RATE_4,
        HEARTBEAT_TIME_1,
        HEARTBEAT_TIME_2,
        HEARTBEAT_TIME_3,
        HEARTBEAT_TIME_4,
        SUM=30,
        END 
    }
    public enum DATA_PACKET_PORTOCOL {
        HEAD1,
        HEAD2,
        FUNCTION_CODE,
        CONFIG,
        SERIAL_NUMBER,
        MASTER_ORDER_MODE,
        MASTER_ORDER_MODE_TYPE,
        ENABLE_SEND,
        ID_NUMBER,
        CHECKE_MODE,
        DATA_LENTH,
        DATA_1,
        DATA_2,
        DATA_3,
        DATA_4,
        DATA_5,
        DATA_6,
        DATA_7,
        DATA_8,
        SUM = 30,
        END
    }
    public enum CHECKMODE {
        STANDARD,
        ENHANCE,
        USERS,
        NULL
    }
    public class InfoPacket {
        byte masterOrSlave = 1;
        byte[] baudRate=new byte[4];
        byte heartbeatTime = 0;
    }
    class LINLine : Hid {
       // Hid hid;
        eCommType gCommType ;
        int gBaudRate ;
        byte gCheckMode;
        byte gIdNumber;
        internal const ushort VENDOR_ID =0x1993;
        internal const ushort PRODUCT_ID =0x2021;


        internal const string SERIAL_NUMBER = "SN.730014009075147533039302";
        public LINLine(eCommType commType = eCommType.Master, int baudRate = 10417, byte checkMode = (byte)CHECKMODE.ENHANCE, byte idNumber = 0) { 
            // hid = new Hid();
            
            //  hid.DataReceived += Hid_ReceivedData;
            gCommType = commType;
            gBaudRate = baudRate;
            gCheckMode = checkMode;
            gIdNumber = idNumber;

        }
        public bool ConnectState {
            get {
                return deviceOpened;
            }
        }

        public void ClosePINDevice() {
            this.CloseDevice();
        }
        public bool PINLineIniltial() {
            if (this.OpenDevice(VENDOR_ID, PRODUCT_ID, SERIAL_NUMBER) != Hid.HID_RETURN.SUCCESS) {
                return false;
            }
            byte[] infoPacketBytes = GetInfoPacket();
            report rp = new report(0x00, infoPacketBytes);
            if (this.Write(rp) != Hid.HID_RETURN.SUCCESS) {
                return false;
            }
            byte[] infoFilterBytes = WritePinFilter();
            rp = new report(0x00, infoFilterBytes);
            if (this.Write(rp) != Hid.HID_RETURN.SUCCESS) {
                return false;
            }
            SentStartPacket();
            // hid.Write(rp);
            return true;
        }
        public void SetMainReceivePacket() {

            // byte[] testByte = { 0x64, 0x80, 0xE4, 0x00, 0x64, 0xD0, 0xC0 };
            byte[] SET_ALL_LIGHT = { 0x64, 0x80, 0xE4, 0xE4, 0xE4, 0xD0, 0xC0 };
            //byte[] dateBytes = GetDataPacket(SET_ALL_LIGHT);
            byte[] dateBytes = GetSlaveDataPacket();
            report test = new report(0x00, dateBytes);
            //report test = new report(0x20, dateBytes);
            Hid.HID_RETURN res = this.Write(test);
        }
        public void SetMainSentPacket() {

            // byte[] testByte = { 0x64, 0x80, 0xE4, 0x00, 0x64, 0xD0, 0xC0 };
            byte[] SET_ALL_LIGHT = { 0x64, 0x80, 0xE4, 0xE4, 0xE4, 0xD0, 0xC0 };
            //byte[] dateBytes = GetDataPacket(SET_ALL_LIGHT);
            byte[] dateBytes = GetSlaveDataPacket();
            report test = new report(0x00, dateBytes);
            //report test = new report(0x20, dateBytes);
            Hid.HID_RETURN res = this.Write(test);
        }
        public byte[] GetDataPacket(byte[]message) {
            byte[] dataBytes = new byte[32];
            dataBytes[(int)DATA_PACKET_PORTOCOL.HEAD1] = 0x55;
            dataBytes[(int)DATA_PACKET_PORTOCOL.HEAD2] = 0xAA;
            dataBytes[(int)DATA_PACKET_PORTOCOL.FUNCTION_CODE] = 0x10;
            dataBytes[(int)DATA_PACKET_PORTOCOL.CONFIG] = 0x00;
            dataBytes[(int)DATA_PACKET_PORTOCOL.MASTER_ORDER_MODE] = 0x00;
            dataBytes[(int)DATA_PACKET_PORTOCOL.MASTER_ORDER_MODE_TYPE] = 0x01;
            dataBytes[(int)DATA_PACKET_PORTOCOL.ID_NUMBER] = 0x20;
            dataBytes[(int)DATA_PACKET_PORTOCOL.CHECKE_MODE] = gCheckMode;
            dataBytes[(int)DATA_PACKET_PORTOCOL.DATA_LENTH] = (byte)message.Length;
            for (int i = 0; i < message.Length; i++) {
                dataBytes[(int)DATA_PACKET_PORTOCOL.DATA_1 + i] = message[i];
            }
            dataBytes[(int)DATA_PACKET_PORTOCOL.SUM] = CalculateSum(dataBytes);
            dataBytes[(int)DATA_PACKET_PORTOCOL.END] = 0x88;
            return dataBytes;
        }
        public byte[] WritePinFilter() {
            byte[] buff;
            buff = new byte[32] { 0x55, 0xAA, 0x03, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            buff[30]= CalculateSum(buff);
            buff[31] = 0x88;
            return buff;
        }
        public byte[] GetSlaveDataPacket() {
            byte[] dataBytes = new byte[32];
            dataBytes[(int)DATA_PACKET_PORTOCOL.HEAD1] = 0x55;
            dataBytes[(int)DATA_PACKET_PORTOCOL.HEAD2] = 0xAA;
            dataBytes[(int)DATA_PACKET_PORTOCOL.FUNCTION_CODE] = 0x10;
            dataBytes[(int)DATA_PACKET_PORTOCOL.CONFIG] = 0x00;
            dataBytes[(int)DATA_PACKET_PORTOCOL.MASTER_ORDER_MODE] = 0x00;
            dataBytes[(int)DATA_PACKET_PORTOCOL.MASTER_ORDER_MODE_TYPE] = 0x02;//0x01;
            dataBytes[(int)DATA_PACKET_PORTOCOL.ID_NUMBER] = 0x21;
            dataBytes[(int)DATA_PACKET_PORTOCOL.CHECKE_MODE] = gCheckMode;
            dataBytes[(int)DATA_PACKET_PORTOCOL.DATA_LENTH] = 4;

            dataBytes[(int)DATA_PACKET_PORTOCOL.SUM] = CalculateSum(dataBytes);
            dataBytes[(int)DATA_PACKET_PORTOCOL.END] = 0x88;
            return dataBytes;
        }
        public void SentStartPacket() {
            byte[] buff;
            buff = new byte[32] { 0x55, 0xAA, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            buff[30] = CalculateSum(buff);
            buff[31] = 0x88;
            report test = new report(0x00, buff);
            //report test = new report(0x20, dateBytes);
            Hid.HID_RETURN res = this.Write(test);
        }


        public byte[] GetInfoPacket() {
            byte[] infoBytes = new byte[32];
            infoBytes[(int)INFO_PACKET_PORTOCOL.HEAD1] = 0x55;
            infoBytes[(int)INFO_PACKET_PORTOCOL.HEAD2] = 0xAA;
            infoBytes[(int)INFO_PACKET_PORTOCOL.FUNCTION_CODE] = 0x02;
            infoBytes[(int)INFO_PACKET_PORTOCOL.MASTER_OR_SLAVE] = Convert.ToByte(gCommType);
            infoBytes[(int)INFO_PACKET_PORTOCOL.ERROR_FRAME_SHIELD] = 0x01;
            infoBytes[(int)INFO_PACKET_PORTOCOL.OPEN_RECEIVEBACK] = 0x01;
            infoBytes[(int)INFO_PACKET_PORTOCOL.OPEN_SENDBACK] = 0x01;
            byte[] baudRateByte = ConvertBaudRateToByte(gBaudRate);
            infoBytes[(int)INFO_PACKET_PORTOCOL.BAUD_RATE_1] = baudRateByte[0];
            infoBytes[(int)INFO_PACKET_PORTOCOL.BAUD_RATE_2] = baudRateByte[1];
            infoBytes[(int)INFO_PACKET_PORTOCOL.BAUD_RATE_3] = baudRateByte[2];
            infoBytes[(int)INFO_PACKET_PORTOCOL.BAUD_RATE_4] = baudRateByte[3];
            infoBytes[(int)INFO_PACKET_PORTOCOL.SUM] = CalculateSum(infoBytes);
            infoBytes[(int)INFO_PACKET_PORTOCOL.END] = 0x88;
            return infoBytes;
        }

        public void RevertLin() {
            byte[] buff;
            buff = new byte[32] { 0x55, 0xAA, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x88 };
            report test = new report(0x00, buff);
            //report test = new report(0x20, dateBytes);
            Hid.HID_RETURN res = this.Write(test);
        }
        private byte[] ConvertBaudRateToByte(Int32 baudRate) {
            byte[] result = BitConverter.GetBytes(baudRate);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(result);
            }
            return result;
        }
        private byte CalculateSum(byte[] bytes) {
            byte sum = 0;
            if (bytes.Length == 32)
            for (int i = 0; i < 30; i++) {
                sum = (byte)(sum + bytes[i]);
            }

            return sum;
        }

    }
}
