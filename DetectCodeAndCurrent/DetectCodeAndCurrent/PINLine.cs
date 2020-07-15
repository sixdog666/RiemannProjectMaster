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
        MASTER_OR_SLAVE,
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
    class PINLine {
        Hid hid;
        eCommType gCommType ;
        int gBaudRate ;
        byte gCheckMode;
        byte gIdNumber;
        internal const ushort VENDOR_ID = 0x1993;
        internal const ushort PRODUCT_ID = 0x2021;
        internal const string SERIAL_NUMBER = "SN.730014009075147533039302";
        public PINLine(eCommType commType = eCommType.Master, int baudRate = 14400, byte checkMode = (byte)CHECKMODE.ENHANCE, byte idNumber = 0) {
            hid = new Hid();
            gCommType = commType;
            gBaudRate = baudRate;
            gCheckMode = checkMode;
            gIdNumber = idNumber;

        }

        public void ClosePINDevice() {
            hid.CloseDevice();
        }
        public bool PINLineIniltial() {
            if (hid.OpenDevice(VENDOR_ID, PRODUCT_ID, SERIAL_NUMBER) != Hid.HID_RETURN.SUCCESS) {
                return false;
            }
            byte[] infoPacketBytes = GetInfoPacket();
            report rp = new report(0x00, infoPacketBytes);
            if (hid.Write(rp) != Hid.HID_RETURN.SUCCESS) {
                return false;
            }
           // hid.Write(rp);
            return true;
        }
        public void SetDataPacket() {

            // byte[] testByte = { 0x64, 0x80, 0xE4, 0x00, 0x64, 0xD0, 0xC0 };
            byte[] testByte = { 0x64, 0x80, 0xE4, 0xE4, 0xE4, 0xD0, 0xC0 };
            byte[] dateBytes = GetDataPacket(testByte);
            report test = new report(0x00, dateBytes);
            Hid.HID_RETURN res = hid.Write(test);
        }
        public byte[] GetDataPacket(byte[]message) {
            byte[] dataBytes = new byte[32];
            dataBytes[(int)DATA_PACKET_PORTOCOL.HEAD1] = 0x55;
            dataBytes[(int)DATA_PACKET_PORTOCOL.HEAD2] = 0xAA;
            dataBytes[(int)DATA_PACKET_PORTOCOL.FUNCTION_CODE] = 0x10;
            dataBytes[(int)DATA_PACKET_PORTOCOL.CONFIG] = 0x00;
            dataBytes[(int)DATA_PACKET_PORTOCOL.MASTER_ORDER_MODE] = 0x00;
            dataBytes[(int)DATA_PACKET_PORTOCOL.MASTER_ORDER_MODE_TYPE] = 0x01;
            dataBytes[(int)DATA_PACKET_PORTOCOL.ID_NUMBER] = gIdNumber;
            dataBytes[(int)DATA_PACKET_PORTOCOL.CHECKE_MODE] = gCheckMode;
            dataBytes[(int)DATA_PACKET_PORTOCOL.DATA_LENTH] = (byte)message.Length;
            for (int i = 0; i < message.Length; i++) {
                dataBytes[(int)DATA_PACKET_PORTOCOL.DATA_1 + i] = message[i];
            }
            dataBytes[(int)DATA_PACKET_PORTOCOL.SUM] = CalculateSum(dataBytes);
            dataBytes[(int)DATA_PACKET_PORTOCOL.END] = 0x88;
            return dataBytes;
        }

        public byte[] GetInfoPacket() {
            byte[] infoBytes = new byte[32];
            infoBytes[(int)INFO_PACKET_PORTOCOL.HEAD1] = 0x55;
            infoBytes[(int)INFO_PACKET_PORTOCOL.HEAD2] = 0xAA;
            infoBytes[(int)INFO_PACKET_PORTOCOL.FUNCTION_CODE] = 0x02;
            infoBytes[(int)INFO_PACKET_PORTOCOL.MASTER_OR_SLAVE] = Convert.ToByte(gCommType);
            byte[] baudRateByte = ConvertBaudRateToByte(gBaudRate);
            infoBytes[(int)INFO_PACKET_PORTOCOL.BAUD_RATE_1] = baudRateByte[0];
            infoBytes[(int)INFO_PACKET_PORTOCOL.BAUD_RATE_2] = baudRateByte[1];
            infoBytes[(int)INFO_PACKET_PORTOCOL.BAUD_RATE_3] = baudRateByte[2];
            infoBytes[(int)INFO_PACKET_PORTOCOL.BAUD_RATE_4] = baudRateByte[3];
            infoBytes[(int)INFO_PACKET_PORTOCOL.SUM] = CalculateSum(infoBytes);
            infoBytes[(int)INFO_PACKET_PORTOCOL.END] = 0x88;
            return infoBytes;
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
