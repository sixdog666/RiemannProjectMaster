using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Threading;
using System.Text.RegularExpressions;
using System.Drawing;


namespace DetectCodeAndCurrent {
    public enum eCodeType {
        Assembly,
        Part,
        StartCurrent,
        DetectLightCurrent,
        EndCurrent,
        Finish1,
        Finish2,
        Err
    }
    public enum eTegResultType {
        Button,
        Light,
        Mike
    }
    public delegate void DelegatePassBackButtonInfo(string waitingTestButtonName, EventArgs e);

    public class TegAtgs : EventArgs {
        public readonly string buttonName;
        public readonly string resultValue;
        public readonly bool resultFlag;
        public TegAtgs(string name, string value, bool flag) {
            buttonName = name;
            resultFlag = flag;
            resultValue = value;
        }
    }
    public struct sCurrentResultImage {
        public Image mick1;
        public Image mick2;
        public Image mick3;
        public Image mick4;

    }
    public struct sButtonPinInfo {
        public string colName;
        public byte[] byteInfo;

    }
    public struct sLightCurrentResult {
        public string currentValue;
        public bool resultFlag;
    }

    public enum eStation1_WorkProcess {
        Waiting = -1,
        StartAssembly = 1,
        Detecting,
        EndDect,
        EndAssembly
    }
    public enum eStation2_WorkProcess {
        StartAssembly = 5,
        Detecting,
        EndDect,
        EndAssembly
    }

  
    public struct sPlayVoiceAdress {
        public const string Null = "";
        public const string FinishSation1 = "FinishSation1";
        public const string FinishSation2 = "FinishSation2";
        public const string Fail_ScanCode = "Fail_ScanCode";
        public const string CodeFail_RightSunVisor = "CodeFail_RightSunVisor";
        public const string CodeFail_LeftSunVisor = "CodeFail_LeftSunVisor";
        public const string CodeFail_TopPanel = "CodeFail_TopPanel";
        public const string CodeFail_Mick = "CodeFail_Mick";
        public const string CodeFail_LeftHandrailLamp = "CodeFail_LeftHandrailLamp";
        public const string CodeFail_RightHandrailLamp = "CodeFail_RightHandrailLamp";
        public const string CodeFail_MidLamp = "CodeFail_MidLamp";
        public const string CodeFail_Wire = "CodeFail_Wire";
        public const string CodeSucceed_RightSunVisor = "rightSunVisor";
        public const string CodeSucceed_LeftSunVisor = "leftSunVisor";
        public const string CodeSucceed_TopPanel = "topPanel";
        public const string CodeSucceed_Mick1 = "mick1";
        public const string CodeSucceed_Mick2 = "mick2";
        public const string CodeSucceed_Mick3 = "mick3";
        public const string CodeSucceed_Mick4 = "mick4";
        public const string CodeSucceed_LeftHandrailLamp = "leftHandrailLamp";
        public const string CodeSucceed_RightHandrailLamp = "righthandrailLamp";
        public const string CodeSucceed_MidLamp = "midLamp";
        public const string CodeSucceed_Wire = "wire";
        public const string UnFinish = "UnFinish";
        public const string CurrentFail_Mick1 = "CurrentFail_Mick1";
        public const string CurrentFail_Mick2 = "CurrentFail_Mick2";
        public const string CurrentFail_Mick3 = "CurrentFail_Mick3";
        public const string CurrentFail_Mick4 = "CurrentFail_Mick4";
        public const string CurrentFinished = "CurrentFinished";
        public const string _3CLabel = "3CLabel";
        public const string LightCurrentError = "LightCurrentError";
     //   public const string LightCurrentSucceed = "LightCurrentSucceed";
        public const string Finish = "Finish";
        public const string UnFinishTeg = "UnFinishTeg";
        public const string CodeSucceed = "CodeSucceed";
    }

    struct sInputDigitalSignal {
        public const string signal1 = "1";
        public const string signal2 = "2";
        public const string signal3 = "3";
        public const string signal4 = "4";
        public const string signal5 = "5";
        public const string signal6 = "6";
        public const string signal7 = "7";
        public const string signal8 = "8";
    }
    struct sOutputDigitalSignal {
        public const string on_off = "0";
        public const string alarm = "1";
        public const string finish2 = "2";
        public const string finish1 = "3";
        public const string signal5 = "4";
        public const string signal6 = "5";
        public const string signal7 = "6";
        public const string signal8 = "7";
    }
    struct sInputRegistSignal {
        public const string averge = "256";
        public const string mick1_volt = "257";
        public const string mick2_volt = "258";
        public const string mick3_volt = "259";
        public const string mick4_volt = "260";
        public const string mick1_current = "261";
        public const string mick2_current = "262";
        public const string mick3_current = "263";
        public const string mick4_current = "264";

    }

    struct sInputRegistSignal2 {
        public const string averge = "256";
        public const string buttonVolt = "257";
        public const string mick2_volt = "258";
        public const string mick3_volt = "259";
        public const string mick4_volt = "260";
        public const string lightCurrent = "261";
        public const string mick2_current = "262";
        public const string mick3_current = "263";
        public const string mick4_current = "264";

    }
    struct sOutputRegistSignal {
        public const string averge = "257";
        public const string signal1 = "258";
        public const string signal2 = "259";
        public const string signal3 = "260";
        public const string signal4 = "261";
        public const string signal5 = "262";
        public const string signal6 = "263";
        public const string signal7 = "264";
        public const string signal8 = "265";

    }
    struct sProductStatu {
        public const string Station = "Station";
        public const string Start = "Start";
        public const string End = "End";
        public const string CurrentStart = "CurrentStart";
        public const string CurrentEnd = "CurrentEnd";
    }
    public struct sButtonVoltRanges{
        public double onStartMin;
        public double onStartMax;
        public double phoneMin;
        public double phoneMax;
        public double sosMin;
        public double sosMax;
    }


    public class WorkProcess {
        public EventHandler Event_NewProduct;
        public EventHandler Event_NewPart;
        public EventHandler Event_InitLastInfo;
        public EventHandler Event_Message;
        public EventHandler Event_EndProduct;
        public EventHandler Event_EndMickCurrentDetect;
        public EventHandler Event_EndLightCurrentDetect;
        private MyModbusTCP DeviceDIO_1;
        private MyModbusTCP DeviceAD_1;
        //private MyModbusTCP DeviceDIO_2;
        private MyModbusTCP DeviceAD_2;
        private string gCurrentButtonName;
        private bool gIsPINConnected = false;
        //  private LINLine lin;
        private USBLin usbLin;
        private string gCurrentProdNum = "";
        private string gCurrentCode = "";
        private int gStation = 1;
        private int gRunningStatu = -1;
        private DataTable gDTStationInfo;
        private ScanerHook barCode;
     //   private static bool[] gTEGResult;
     //   private bool gSwitchPin = false;

       // private static bool gPinRecevieFinish=false;//循环发送前保证上次数据已收到
        private static bool gIsNotAlarmFlag = true;
        private bool gSQLSeversFlag = false;
        private static bool gSwitchButton = false;
        private static bool gSwitchButtonVolt = false;
        private static bool gSwitchLin = false;
        private static bool gSwitchMike = false;
        private static bool gSwitchLightCurrent = false;
        private static bool gPinDataFlag=false;
        public DelegatePassBackButtonInfo ButtonInfoPassBack;

        public bool LinThreadFlag {
            
            get {
                return gSwitchLin;
            }
        }
        public bool LinReceiveFlag {
            get { 
                return gPinDataFlag;
            }
            set {
                gPinDataFlag = false;
            }
        }
        public bool PINConnectFlag {
            get {
                return usbLin.ConnectState;
            }
        }
        public bool SQLConnectFlag {
            get {
                return gSQLSeversFlag;
            }
        }
        public bool DIConnectFlag {
            get {
                return DeviceDIO_1.IsConnect;
            }

        }
        public bool AD1ConnectFlag {
            get {
                return DeviceAD_1.IsConnect;
            }

        }

        public bool AD2ConnectFlag {
            get {
                return DeviceAD_2.IsConnect;
            }

        }

        public string strCurrentProd {
            get { return gCurrentProdNum; }
            set { gCurrentProdNum = value; }
        }
        public string strCurrentCode {
            get { return gCurrentCode; }
            set { gCurrentCode = value; }

        }
        public int iStation {
            get { return gStation; }
        }
        private static WorkProcess instance;


        public WorkProcess() {
            //gTEGResult = new bool[3];
            barCode = new ScanerHook();
            //DeviceDIO_1 = new MyModbusTCP("192.168.19.80");
            //DeviceAD_1 = new MyModbusTCP("192.168.19.81");
            //DeviceAD_2 = new MyModbusTCP("192.168.19.82");
            DeviceDIO_1 = new MyModbusTCP("172.20.16.64");
            DeviceAD_1 = new MyModbusTCP("172.20.16.65");
            DeviceAD_2 = new MyModbusTCP("172.20.16.66");
            usbLin = new USBLin();
        }
        public bool OpenPartSwitch() {
            if (DeviceDIO_1.IsConnect) {
                DeviceDIO_1.WriteCoil(sOutputDigitalSignal.finish2, false);
                if (DeviceDIO_1.WriteCoil(sOutputDigitalSignal.on_off, true)) return true;
              
            }

            return false;
        }
        public bool ClosePartSwitch(bool isFinish) {

            if (DeviceDIO_1.IsConnect) {
                if (isFinish) DeviceDIO_1.WriteCoil(sOutputDigitalSignal.finish2, true);

                if (DeviceDIO_1.WriteCoil(sOutputDigitalSignal.on_off, false)) return true;
              
            }
            return false;
        }
        public void RestoreLin() {
            //lin.RevertLin();
           /// lin.PINLineIniltial();
        }
        public void InitLastTEGResult(string code,out string lightValue,out string mikeResult) {
            DataRow dr = SqlOperation.GetTEGResult(code);
            if (dr != null) {
                lightValue = dr["LigthCurrentResult"].ToString();
                mikeResult = dr["mikeTegResult"].ToString();
                CheckIfFinished();
            }
            else {
                lightValue = string.Empty;
                mikeResult= string.Empty;
            }
        }

        public void GetMickCurrent(out string mick1, out string mick2, out string mick3, out string mick4) {
            mick1 = "--";
            mick2 = "--";
            mick3 = "--";
            mick4 = "--";
            if (DeviceAD_1.IsConnect) {
                mick1 = string.Format("{0:0.0000}", CalculateCurrentRealValue(DeviceAD_1.Read(sInputRegistSignal.mick1_current)));
                mick2 = string.Format("{0:0.0000}", CalculateCurrentRealValue(DeviceAD_1.Read(sInputRegistSignal.mick2_current)));
                mick3 = string.Format("{0:0.0000}", CalculateCurrentRealValue(DeviceAD_1.Read(sInputRegistSignal.mick3_current)));
                mick4 = string.Format("{0:0.0000}", CalculateCurrentRealValue(DeviceAD_1.Read(sInputRegistSignal.mick4_current)));
            }

        }


        public void GetMickVolt(out string mick1, out string mick2, out string mick3, out string mick4) {
            mick1 = "--";
            mick2 = "--";
            mick3 = "--";
            mick4 = "--";
            if (DeviceAD_1.IsConnect) {
                mick1 = string.Format("{0:0.0000}", CalculateVoltRealValue(DeviceAD_1.Read(sInputRegistSignal.mick1_volt)));
                mick2 = string.Format("{0:0.0000}", CalculateVoltRealValue(DeviceAD_1.Read(sInputRegistSignal.mick2_volt)));
                mick3 = string.Format("{0:0.0000}", CalculateVoltRealValue(DeviceAD_1.Read(sInputRegistSignal.mick3_volt)));
                mick4 = string.Format("{0:0.0000}", CalculateVoltRealValue(DeviceAD_1.Read(sInputRegistSignal.mick4_volt)));
            }

        }

        public void InitialWork() {

            barCode.ScanerEvent += ReceivedNewCode_CallBack;
            barCode.Start();
            usbLin.OpenDevcie();
            if (!usbLin.InitDevice()) {
                gIsPINConnected = false;
                Event_Message?.Invoke("车顶LIN信号连接失败", null);
            }
            else gIsPINConnected = true;

            //pin.DataReceived += Hid_ReceivedData;
            if (!DeviceDIO_1.Connect()) {
                Event_Message?.Invoke("数字量模块未成功连接", null);
            }
            if (!DeviceAD_1.Connect()) {
                Event_Message?.Invoke("模拟量模块1未成功连接", null);
            }
            if (!DeviceAD_2.Connect()) {
                Event_Message?.Invoke("模拟量模块2未成功连接", null);
            }

            InitialLastRunningInfo();
        }

        private void StartListeningButtonDown() {
            if (gSwitchButton == false) {
                gSwitchButton = true;
                Thread listenButtonDown = new Thread(ListenButton);
                if (listenButtonDown.ThreadState != ThreadState.Running) {
                    listenButtonDown.Start();
                }
            }

        }
        public void CloseListeningButtonDown() {
            gSwitchButton = false;
        }
       static byte[] receivedata;
        private void ListenButton() {
            bool isVoltTestEnd = false;
            string code = gCurrentCode;
            bool isFinsh=false;
            OpenPartSwitch();
            OpenReceiveLin(true);
            try {
                while (gSwitchButton)
                {
                    string testButtonName = gCurrentButtonName;
                    if (IfVoltButton(testButtonName) == null) break;
                    if ((bool)IfVoltButton(testButtonName))
                    {
                        
                        WaitForVoltButton(ref isVoltTestEnd);
                    }
                    else
                    {

                      isFinsh = WaitForLinButtonDown(receivedata);


                    }
                    Thread.Sleep(300);
                }
                if (isFinsh || gCurrentButtonName == string.Empty) {
                    SqlOperation.UpdateProductCodeRecord(code, "buttonTegResult", "1");
                    CheckFinish();
                    //CloseReceivePIN();
                    ButtonInfoPassBack?.Invoke("已完成", null);
                    return;
                }
               

            }
            catch (Exception ex) {
                Event_Message?.Invoke(ex.Message, null);
            }
        }
        private bool WaitForLinButtonDown(byte[] data) {
            if ((data != null) && data.Length >= 4)
            {
                string nextButtonName;
                DataRow button = SqlOperation.GetButtonInfo(gCurrentButtonName, gCurrentProdNum, out nextButtonName);
                if (button != null)
                {
                    //string buffer;
                    //ExchangePinData(data, out buffer);
                    //Event_Message?.Invoke("收到数据:" + buffer, null);
                    byte[] dataBytes;
                    bool resultFlag = true;
                    ExchangePinDataStringToByte(button["tbl_ByteString"].ToString(), out dataBytes);
                    for (int i = 1; i < 3; i++)
                    {
                        if (dataBytes[i] != data[i])
                        {
                            resultFlag = false;
                            break;
                        }
                    }
                    if (resultFlag == true)
                    {
                        SqlOperation.UpdateButtonState(gCurrentCode, button["tbl_ColumnName"].ToString(), "已检测完成");
                        TegAtgs arg = new TegAtgs(gCurrentButtonName, "已检测完成", true);
                        ButtonInfoPassBack?.Invoke(nextButtonName, arg);
                        gCurrentButtonName = nextButtonName;
                        PlayVoice(sPlayVoiceAdress.Finish);
                        
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        private bool CheckFinish() {
            bool tegResult = true;
            bool[] teg= CheckIfFinished();
            for (int i = 0; i < teg.Length; i++) {
                tegResult = tegResult & teg[i];
            }
            if (tegResult && gRunningStatu != (int)eStation2_WorkProcess.EndAssembly) {
                gRunningStatu = (int)eStation2_WorkProcess.EndAssembly;
                ProductEnd(gCurrentProdNum);
                PlayVoice(sPlayVoiceAdress.FinishSation2);
            }
            return tegResult;
        }
        //
        private void WaitForVoltButton_o(ref bool isVoltTestEnd) {

            string nextButtonName = "";
            double testValue;
            string testButtonName = gCurrentButtonName;
            if (!isVoltTestEnd) {

                if (WaitForButtonUp()) {
                    isVoltTestEnd = true;
                    ButtonInfoPassBack?.Invoke(gCurrentButtonName, null);
                }
                else {
                    string strTip = "请松开按键";
                    ButtonInfoPassBack?.Invoke(strTip, null);
                }
            }
            else {
                object result = WaitForButtonDown(testButtonName, out testValue);
                if (result != null && (bool)result) {
                    DataRow dr = SqlOperation.GetButtonInfo(testButtonName,gCurrentProdNum, out nextButtonName);
                    SqlOperation.UpdateButtonState(gCurrentCode, dr["tbl_ColumnName"].ToString(), testValue.ToString());
                    isVoltTestEnd = false;
                    TegAtgs arg = new TegAtgs(testButtonName, string.Format("{0:0.0000}", testValue), true);
                    gCurrentButtonName = nextButtonName;
                    //string strTip = "松开"+ testButtonName;
                    ButtonInfoPassBack?.Invoke(gCurrentButtonName, arg);
                }
            }
        }
        private void WaitForVoltButton( ref bool isVoltTestEnd) {  
            string nextButtonName = "";
            double testValue;
            string testButtonName = gCurrentButtonName;
                object result = WaitForButtonDown(testButtonName, out testValue);
            if (result != null && (bool)result) {
                DataRow dr = SqlOperation.GetButtonInfo(testButtonName,gCurrentProdNum, out nextButtonName);
                SqlOperation.UpdateButtonState(gCurrentCode, dr["tbl_ColumnName"].ToString(), testValue.ToString());
                isVoltTestEnd = false;
                TegAtgs arg = new TegAtgs(testButtonName, string.Format("{0:0.0000}", testValue), true);
                gCurrentButtonName = nextButtonName;
                PlayVoice(sPlayVoiceAdress.Finish);
                //string strTip = "松开"+ testButtonName;
                ButtonInfoPassBack?.Invoke(gCurrentButtonName, arg);
            }
        }
        
        private object IfVoltButton(string itemName) {
            DataRow row = SqlOperation.GetCheckVoltButton(itemName, gCurrentProdNum);
            if (row != null && Convert.ToBoolean(row["tbl_VCFlag"])) return true;
            if(row != null) return false;
            return null;
            }
        private object WaitForButtonDown(string itemName, out double testValue) {
            testValue = 0;
            DataRow row = SqlOperation.GetCheckVoltButton(itemName,gCurrentProdNum);
            if (row != null && Convert.ToBoolean(row["tbl_VCFlag"])) {
                double maxValue = (double)row["tbl_MaxValue"];
                double minValue = (double)row["tbl_MinValue"];
                return TestButtonDownVoltForTimes(minValue, maxValue,out testValue);
            }
            return null;
        }
        private bool WaitForButtonUp() {
            double value;
          return  TestButtonUpVoltForTimes(0, 0.05,out value);
        }
        private bool TestButtonUpVoltForTimes(double min,double max,out double value) {
            int validCount = 0;
            value = 0;
            for (int i = 0; i <= 3; i++) {
                double testValue = GetButtonVolt();
                if (testValue >= min && testValue <= max) {
                    value = value + testValue;
                    validCount++;
                }
                Thread.Sleep(10);
            }
            if (validCount >= 2) {
                value = value / validCount;
                value = Convert.ToDouble(string.Format("{0:0.0000}", value));
                return true;
            }

            else
                return false;

        }
        private bool TestButtonDownVoltForTimes(double min, double max, out double value) {

            value = 0;
            bool flag = false;
            double testValue = GetButtonVolt();
            testValue = Math.Round(testValue, 5);
            if (testValue >= min && testValue <= max) {
                value = testValue;
                flag = true;
            }
            else flag = false;
            
            TegAtgs arg = new TegAtgs(gCurrentButtonName, testValue.ToString(), false);
            ButtonInfoPassBack?.Invoke(null, arg);
            return flag;

        }

        private bool TestLightCurrentForTimes_0(double min, double max, out double value) {
            int validCount = 0;
            double testValue;
            testValue = GetLightCurrent();
            value = Math.Round(testValue, 5);
            if (testValue >= min && testValue <= max)
                return true;
            return false;
        }
        private bool TestLightCurrentForTimes(double min, double max, out double value) {
         
            double testValue;
            value = 0;
            for (int i = 0; i <= 5; i++) {
                testValue = GetLightCurrent();
                value = value + testValue;

                Thread.Sleep(100);
            }
            value = value / 6;

            value = Math.Round(value, 5);
            if (value >= min && value <= max)
                return true;
            else
                return false;


        }
        public void InitTEG(string currentCode)
        {

            DataTable dt = SqlOperation.GetButtonConfigInfo(gCurrentProdNum);
            bool needTestFlag = false;
            gCurrentButtonName = string.Empty;
            //   DataRow dr = SqlOperation.GetTEGResult(gCurrentCode);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string buttonColName = dr["ID"].ToString();
                string buttonName = dr["检测项"].ToString();
                string result = SqlOperation.GetButtonState(buttonColName, currentCode);
                if (result != string.Empty)
                {
                    if (i < dt.Rows.Count - 1)
                    {
                        //gCurrentButtonName = buttonName;
                    }
                    TegAtgs arg = new TegAtgs(buttonName, result, true);
                    ButtonInfoPassBack?.Invoke(null, arg);
                }
                else
                {
                    if (gCurrentButtonName == string.Empty)
                        gCurrentButtonName = buttonName;
                    needTestFlag = true;
                    TegAtgs arg = new TegAtgs(buttonName, result, false);
                    ButtonInfoPassBack?.Invoke(gCurrentButtonName, arg);
                }

            }
            DataRow drtemp = SqlOperation.GetTEGResult(gCurrentCode);
            bool[] tegResult = new bool[3];
            if (drtemp != null)
            {
                if (drtemp["buttonTegResult"] != null && drtemp["buttonTegResult"].GetType() != typeof(DBNull))
                {
                    tegResult[(int)eTegResultType.Button] = Convert.ToBoolean(drtemp["buttonTegResult"]);
                }
            }
            if (needTestFlag && !tegResult[(int)eTegResultType.Button]) 
                StartListeningButtonDown();
            else
            {
                CloseListeningButtonDown();
                if (LinThreadFlag == false)
                {
                    OpenReceiveLin(true);
                }
            }
        }


        private void ExchangePinData(byte[] byteBuffer,out string buffer) {
            buffer = "";
            if (byteBuffer.Length >= 4) {  
                for (int i = 0; i <= 3; i++) { 
                buffer = buffer + "0x"+Convert.ToString(byteBuffer[i], 16) + ' ';
                }
            }
            buffer = buffer.TrimEnd(' ');

        }
        private void ExchangePinDataStringToByte(string buffer, out byte[] byteBuffer) {
            byteBuffer = new byte[4] {byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };
            try {
             
                string[] strBytes = buffer.Split(' ');
                if (strBytes.Length >= 4) {
                    for (int i = 0; i < 4; i++) {
                        byteBuffer[i] = Convert.ToByte(strBytes[i],16);
                    }
                }
            }
            catch (Exception ex) {
            }
        }
   

        public void OpenReceiveLin(bool IsNeedEventHandleFlag) {

            if (!gIsPINConnected|| gSwitchLin == true) return;
            gSwitchLin = true;
            Thread pinThread = new Thread(Thread_ReviceLIN);           
            if ( pinThread.ThreadState != ThreadState.Running) {
            
                pinThread.Start();
            }
        }

        public void CloseReceivePIN() {
            gSwitchLin = false;
        }
        private void Thread_ReviceLIN() {
            try {
                DateTime time = System.DateTime.Now;
                usbLin.MasterBreak();
                while (gSwitchLin) {
                    try
                    {
                         receivedata = usbLin.MasterRead();
                        if ((receivedata != null) && receivedata.Length >= 4)
                        {
                            if (gPinDataFlag == false)
                                gPinDataFlag = true;
                        }
                        else
                        {
                            if (gPinDataFlag == true)
                                gPinDataFlag = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Event_Message?.Invoke(ex.Message, null);
                    }
                    Thread.Sleep(300);

                }
            }
            catch(Exception ex) {
                Event_Message?.Invoke(ex.Message, null);
            }
        }


       


        public static WorkProcess GetInstance(){
            if (instance == null) instance = new WorkProcess();
            return instance;
            
        }
        public bool InitialStationInfo(int iCurrentStation) {
            gStation = iCurrentStation;
            gDTStationInfo = SqlOperation.GetStationInfo(iCurrentStation);
            if (gDTStationInfo!=null && gDTStationInfo.Rows.Count > 0) return true;
            return false;
        }
        public bool InitialLastRunningInfo() {
            if (gDTStationInfo != null && gDTStationInfo.Rows.Count > 0) {
                DataTable dt = SqlOperation.GetCurrentRunningProduct((int)gDTStationInfo.Rows[0][sProductStatu.Start], (int)gDTStationInfo.Rows[0][sProductStatu.End]);
                if (dt.Rows.Count > 0) {
                    SqlOperation.GetProductConfigCodeFromSQL((string)dt.Rows[0]["product"], out gCurrentProdNum);
                    gCurrentCode = (string)dt.Rows[0]["productCode"];
                    Event_InitLastInfo?.Invoke(dt, null);
                    gRunningStatu = (int)dt.Rows[0]["statu"];
                    if(gRunningStatu == (int)eStation2_WorkProcess.StartAssembly|| gRunningStatu == (int)eStation2_WorkProcess.Detecting)
                        InitTEG(gCurrentCode);
                    if (gRunningStatu == (int)eStation2_WorkProcess.Detecting) {
                        StartTegMikeAndLight(gCurrentCode);
                    }
                    return true;
                }
                gRunningStatu = (int)gDTStationInfo.Rows[0][sProductStatu.End];
                return false;
            }
            return false;
        }

        public void Task_Work(string strCode) {
            if (gRunningStatu == -1) return;
            switch (gStation) {
                case 1:
                    Task_Station1(strCode);
                    break;
                case 2:
                    Task_Station2(strCode);
                    break;
                default:
                    return;
            }
        }

        private void Task_Station1(string strCode) {
            bool alarmFlag = false;
            switch ((eStation1_WorkProcess)gRunningStatu) {
                case eStation1_WorkProcess.EndAssembly://结束状态(初始状态)，允许产品条码
                    if (SqlOperation.GetProductCodeStatu(strCode) == (int)eStation1_WorkProcess.Waiting)
                        InInitialOrEndStatu_Station1(strCode,out alarmFlag);
                    else
                        OutAlarm("当前产品状态异常", sPlayVoiceAdress.Fail_ScanCode);
                    break;
                case eStation1_WorkProcess.StartAssembly://装配状态
                    InAssemblyStatu_Station1(strCode);
                    break;
                case eStation1_WorkProcess.Detecting://电流检测中
                    InCurrentDetectStatu_Station1(strCode);
                    break;
                case eStation1_WorkProcess.EndDect://电流检测结束
                    InEndDectStatu_Station2(strCode);
                    break;
                default:
                    OutAlarm("当前产品状态与工位信息不匹配", sPlayVoiceAdress.Fail_ScanCode);
                    break;
            }
            if (!gIsNotAlarmFlag) {
                gIsNotAlarmFlag = true;
                ResetAlarm();
            }
        }
        private void Task_Station2(string strCode) {
            bool alarmFlag = false;
            switch ((eStation2_WorkProcess)gRunningStatu) {
                case eStation2_WorkProcess.EndAssembly://结束状态(初始状态)，允许产品条码
                    if (SqlOperation.GetProductCodeStatu(strCode) == (int)eStation1_WorkProcess.EndAssembly)
                        InInitialOrEndStatu_Station2(strCode);
                    else
                        OutAlarm("当前产品状态异常", sPlayVoiceAdress.Fail_ScanCode);
                    break;
                case eStation2_WorkProcess.StartAssembly://装配状态
                    InAssemblyStatu_Station2(strCode);
                    break;
                case eStation2_WorkProcess.Detecting://电流检测中
                    InCurrentDetectStatu_Station2(strCode);
                    break;
                case eStation2_WorkProcess.EndDect://电流检测结束
                    InEndDectStatu_Station2(strCode);
                    break;
                default:
                    OutAlarm("当前产品状态与工位信息不匹配", sPlayVoiceAdress.Fail_ScanCode);
                    break;
            }
            if (!alarmFlag)
            {
                ResetAlarm();
            }

        }

        private void InInitialOrEndStatu_Station1(string strCode, out bool resultFlag) {
            ResetStation1Output();
            switch (CheckInputCodeType(strCode)) {
                case eCodeType.Assembly:
                    if (ProductCodeScan(strCode) == true) {
                        gRunningStatu = (int)eStation1_WorkProcess.StartAssembly;
                        resultFlag = true;
                    }
                    resultFlag = false;
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    resultFlag = false;
                    break;
            }
        }
        private void InInitialOrEndStatu_Station2(string strCode)
        {
            switch (CheckInputCodeType(strCode))
            {
                case eCodeType.Assembly:
                    if (ProductCodeScan(strCode) == true)
                    {
                        if (!CheckFinish()) {
                            OpenPartSwitch();
                            OpenReceiveLin(true);
                            InitTEG(gCurrentCode);
                        }
                        gRunningStatu = (int)eStation2_WorkProcess.StartAssembly;
                        gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                        //SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);
                    }
                    gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    break;
            }
        }
        private void InAssemblyStatu_Station1(string strCode) {
            switch (CheckInputCodeType(strCode)) {
                case eCodeType.StartCurrent:
                    MikeCurrentDectectStart(strCode);
                    LightCurrentDetectStart();
                    break;
                case eCodeType.Part:
                    PartCodeScan(strCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag&true;
                    break;
                case eCodeType.Finish1:
                    if (IsFinshScan()) {
                        
                        gRunningStatu = (int)eStation1_WorkProcess.EndAssembly;
                        ProductEnd(strCode);
                        PlayVoice(sPlayVoiceAdress.FinishSation1);
                        FinsiStation1Output();
                        gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                    }
                    else {
                        OutAlarm("当前扫码未完成", sPlayVoiceAdress.UnFinish);
                        gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    }
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    break;
            }
        }
        private void FinsiStation1Output() {
            if (DeviceDIO_1.IsConnect) {
                DeviceDIO_1.WriteCoil(sOutputDigitalSignal.finish1, true);
            }
           
        }
        private void ResetStation1Output()
        {
            if (DeviceDIO_1.IsConnect)
            {
                DeviceDIO_1.WriteCoil(sOutputDigitalSignal.finish1, false);
            }

        }
        private void InAssemblyStatu_Station2(string strCode)
        {
            switch (CheckInputCodeType(strCode))
            {
                case eCodeType.StartCurrent:
                    //  gTEGResult = new bool[3];
                    StartTegMikeAndLight(strCode);
                    gRunningStatu = (int)eStation2_WorkProcess.Detecting;
                    SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                    break;
                case eCodeType.Finish2:
                    if (IsFinshScan()) {
                        if (CheckFinish()) {
                            gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                        }
                        else {
                            OutAlarm("当前电检测未完成", sPlayVoiceAdress.UnFinishTeg);
                            gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                        }
                    }
                    else {
                        OutAlarm("当前组装未完成", sPlayVoiceAdress.UnFinish);
                        gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    }
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & false; 
                    break;
            }
        }
        private void StartTegMikeAndLight(string strCode) {
            MikeCurrentDectectStart(strCode);
            Thread_DetectLight();
        }
        private void Thread_DetectLight() {
            Thread thread = new Thread(LightCurrentDetectStart);
            if (thread.ThreadState != ThreadState.Running && gSwitchLightCurrent == false) {
                gSwitchLightCurrent = true;
                thread.Start();
            }
        }
        private void  LightCurrentDetectStart() {
            if (IsLightTegSucceed()) {
                gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                CheckFinish();
            }
            else {
                gIsNotAlarmFlag = gIsNotAlarmFlag & false;
            }
            gSwitchLightCurrent = false;
        }

        private bool IsLightTegSucceed() {
            try {
                string code = gCurrentCode;
                double testvalue;
                SqlOperation.UpdateProductCodeRecord(code, "lightTegResult", "0");
                if (DetectLightCurrentValue(out testvalue)) {
                    SqlOperation.UpdateLightCurrentValue(code, testvalue);
                    SqlOperation.UpdateProductCodeRecord(code, "lightTegResult", "1");
                   // PlayVoice(sPlayVoiceAdress.LightCurrentSucceed);
                    Event_EndLightCurrentDetect?.Invoke(new sLightCurrentResult() { currentValue = string.Format("{0:0.0000}", testvalue), resultFlag = true }, null);
                    return true;
                }
                else {
                    OutAlarm("当前电流检测值异常" + testvalue, sPlayVoiceAdress.LightCurrentError);
                    SqlOperation.UpdateLightCurrentValue(code, testvalue);
                    SqlOperation.UpdateProductCodeRecord(code, "lightTegResult", "0");
                    Event_EndLightCurrentDetect?.Invoke(new sLightCurrentResult() { currentValue = string.Format("{0:0.0000}", testvalue), resultFlag = false }, null);
                    return false;
                }
            } catch (Exception ex) {
                Event_Message?.Invoke(ex.Message, null);
                return false;
            }
           

        }
        private void InCurrentDetectStatu_Station1(string strCode) {
            switch (CheckInputCodeType(strCode)) {
                case eCodeType.EndCurrent:
                    CurrentDectectEnd(strCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    break;
            }
        }
        private void InCurrentDetectStatu_Station2(string strCode)
        {
            eCodeType type = CheckInputCodeType(strCode);
            switch (type)
            {
                case eCodeType.StartCurrent:
                    //  gTEGResult = new bool[3];
                    gRunningStatu = (int)eStation2_WorkProcess.Detecting;
                    SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);
                    StartTegMikeAndLight(strCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                    break;
                case eCodeType.Finish2:
                    if (IsFinshScan()) {
                        if (CheckFinish()) {
                            gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                        }
                        else {
                            OutAlarm("当前电检测未完成", sPlayVoiceAdress.UnFinishTeg);
                            gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                        }
                    }
                    else {
                        OutAlarm("当前组装未完成", sPlayVoiceAdress.UnFinish);
                        gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    }
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    break;
            }
        }
        private void InEndDectStatu_Station1(string strCode, out bool resultFlag) {
            switch (CheckInputCodeType(strCode)) {
                case eCodeType.Finish1:
                    if (IsFinshScan()) {
                        gRunningStatu = (int)eStation1_WorkProcess.EndAssembly;
                        ProductEnd(strCode);
                        PlayVoice(sPlayVoiceAdress.FinishSation1);
                        resultFlag = true;
                    }
                    else {
                        OutAlarm("当前扫码未完成", sPlayVoiceAdress.UnFinish);
                        resultFlag = false;
                    }
                    break;
                case eCodeType.Part:
                    PartCodeScan(strCode);
                    resultFlag = true;
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    resultFlag = false;
                    break;
            }
        }
        private void InEndDectStatu_Station2(string strCode)
        {
            switch (CheckInputCodeType(strCode))
            {
                case eCodeType.Finish2:
                    if (IsFinshScan()){
                        gRunningStatu = (int)eStation2_WorkProcess.EndAssembly;
                        ProductEnd(strCode);
                        PlayVoice(sPlayVoiceAdress.FinishSation2);
                        gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                    }
                    else{
                        OutAlarm("当前扫码未完成", sPlayVoiceAdress.UnFinish);
                        gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    }
                    break;
                case eCodeType.Part:
                    PartCodeScan(strCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & true;
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    gIsNotAlarmFlag = gIsNotAlarmFlag & false;
                    break;
            }
        }
        private bool IsFinshScan() {
            var productInfosList = SqlOperation.GetProductAssembleFromSQL(gCurrentProdNum);
            SqlOperation.GetCurrentPartRecordFromSQL(gCurrentCode, gCurrentProdNum, ref productInfosList);
            var test =  productInfosList.FindAll(s => s.station == gStation);
            foreach (var product in test) {
                if (product.value == null || product.value == "") return false;
            }
            return true;
        }

        private eCodeType CheckInputCodeType(string strCode) {
            if (IsProductCode(strCode)) {
                return eCodeType.Assembly;
            }
            if (IsDetectLightCurrentCode(strCode)) {
                return eCodeType.DetectLightCurrent;
            }
            if (IsPartCode(strCode)) {
                return eCodeType.Part;
            }
            if (IsEndCode1(strCode)) {
                return eCodeType.Finish1;
            }
            if (IsEndCode2(strCode)) {
                return eCodeType.Finish2;
            }

            if (IsCurrentDetectEndCode(strCode)) {
                return eCodeType.EndCurrent;
            }
            if (IsCurrentDetectStartCode(strCode)) {
                return eCodeType.StartCurrent;
            }
            return eCodeType.Err;
        }


        private void Task_Station2(eStation2_WorkProcess action, string strCode) {
            while (true) {
                switch (action) {
                    case eStation2_WorkProcess.StartAssembly:
                        break;
                    case eStation2_WorkProcess.EndAssembly:
                        break;
                    case eStation2_WorkProcess.Detecting:
                        break;
                    case eStation2_WorkProcess.EndDect:
                        break;
                }
            }
        }
        private void ReceivedNewCode_CallBack(ScanerHook.ScanerCodes codes) {
            string[] result = codes.Result.Split('%');
            Task_Work(codes.Result);
        }
        private bool ProductCodeScan(string code) {
            int result = SqlOperation.InsertProductCodeRecord(code, gCurrentProdNum, Convert.ToInt32(gDTStationInfo.Rows[0][sProductStatu.Start]));
            if (result > 0) {
                gCurrentCode = code;
                Event_NewProduct?.Invoke(code, null);
                PlayVoice(sPlayVoiceAdress.CodeSucceed);
                return true;

            }
            OutAlarm("新增总装失败", sPlayVoiceAdress.Fail_ScanCode);
            return false;
        }
        private bool IsProductCode(string code) {
            string productNum;
            string productCode;
            AnalysisPartCode(code,out productNum,out productCode) ;//实际条码规则

            if (gCurrentProdNum == productNum) {
                return true;
             
            }
            else {
                return false;
            }
        }
        private bool IsDetectLightCurrentCode(string strCode) {
            if (strCode =="") {//"[)>06Y400303980DPD0XP263424X012V421278712TJ09A2007080021") {
                return true;
            }
            return false;
        }

        private void PartCodeScan(string partCode) {
            string fieldName="";
            string partConfigCode = "";
            string partSerialCode = "";
            AnalysisPartCode(partCode, out partConfigCode, out partSerialCode);
            DataTable dt = SqlOperation.GetPartConfigType(gCurrentProdNum, partConfigCode);
            if (dt.Rows.Count > 0) {
                if ((int)dt.Rows[0]["Count"] == 1) {
                    fieldName = Convert.ToString(dt.Rows[0]["Type"]);
                    if (fieldName == "mick") fieldName = "mick1";
                }
                else {
                    for (int i = 1; i <= (int)dt.Rows[0]["Count"]; i++) {
                        fieldName = Convert.ToString(dt.Rows[0]["Type"]) + i;
                        if (!SqlOperation.IsPartCodeRecordExist(gCurrentCode, fieldName, partSerialCode))
                            break;
                    }
                }
                if (SqlOperation.UpdateProductCodeRecord(gCurrentCode, fieldName, partSerialCode)) {
                    Event_NewPart?.Invoke(fieldName, null);
                    PlayVoice(fieldName);
                }
                else {
                    OutAlarm("产品添加失败", sPlayVoiceAdress.Fail_ScanCode);
                }
              
            }
            else {
                OutAlarm("当前检测总成条形码状态错误", sPlayVoiceAdress.Fail_ScanCode);
            }
        }
        private void CurrentDectectEnd(string strCode)
        {
            CloseReceivePIN();
            gRunningStatu = (int)eStation2_WorkProcess.EndDect;
            SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);

        }
        private void ProductEnd(string strCode)
        {
            if (gStation == 2)
            {
                CloseReceivePIN();

                CloseListeningButtonDown();
                ClosePartSwitch(true);
            }
            SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);
            Event_EndProduct?.Invoke(gCurrentCode, null);
          
        }
        private void MikeCurrentDectectStart(string strCode) {                 
            Thread thread = new Thread(Thread_MeasureMike);
            if (thread.ThreadState != ThreadState.Running && gSwitchMike == false) {
                gSwitchMike = true;
                thread.Start();
            }
        }
        private bool[] CheckIfFinished() {
            DataRow dr = SqlOperation.GetTEGResult(gCurrentCode);
            bool[] tegResult = new bool[3];
            if (dr != null) {
             
                if (dr["buttonTegResult"] != null && dr["buttonTegResult"].GetType() != typeof(DBNull)) {
                    tegResult[(int)eTegResultType.Button] = Convert.ToBoolean(dr["buttonTegResult"]);
                }
                if (dr["mikeTegResult"] != null && dr["mikeTegResult"].GetType() != typeof(DBNull)) {
                    tegResult[(int)eTegResultType.Mike] = Convert.ToBoolean(dr["mikeTegResult"]);
                }
                if (dr["lightTegResult"] != null && dr["lightTegResult"].GetType() != typeof(DBNull)) {
                    tegResult[(int)eTegResultType.Light] = Convert.ToBoolean(dr["lightTegResult"]);
                }
            }
            
            return tegResult;
        }
        private void Thread_MeasureMike() {
            if (MeasureValue()) {
                ResetAlarm();
                CheckFinish();
            }
            gSwitchMike = false;

        }
        private bool MeasureValue() {
            try {

                string MesureCode = gCurrentCode;
                //启动电源开关
                //  OpenPartSwitch();
                // OpenReceiveLin();
                SqlOperation.UpdateProductCodeRecord(MesureCode, "mikeTegResult", "0");
                Thread.Sleep(2500);
                double mick1_currentValue, mick1_voltValue;
                double mick2_currentValue, mick2_voltValue;
                double mick3_currentValue, mick3_voltValue;
                double mick4_currentValue, mick4_voltValue;
                bool mick1Result = CurrentAndVoltValue(sInputRegistSignal.mick1_current, sInputRegistSignal.mick1_volt, out mick1_currentValue, out mick1_voltValue);
                bool mick2Result = CurrentAndVoltValue(sInputRegistSignal.mick2_current, sInputRegistSignal.mick2_volt, out mick2_currentValue, out mick2_voltValue);
                bool mick3Result = CurrentAndVoltValue(sInputRegistSignal.mick3_current, sInputRegistSignal.mick3_volt, out mick3_currentValue, out mick3_voltValue);
                bool mick4Result = CurrentAndVoltValue(sInputRegistSignal.mick4_current, sInputRegistSignal.mick4_volt, out mick4_currentValue, out mick4_voltValue);

                //关闭电源开关
                //保存值
                string currentResult = "电流值 1:" + string.Format("{0:0.000}", mick1_currentValue) + ",2:" + string.Format("{0:0.000}", mick2_currentValue) + ",3:" + string.Format("{0:0.000}", mick3_currentValue) + ",4:" + string.Format("{0:0.000}", mick4_currentValue);
                string voltResut = "电压值 1:" + string.Format("{0:0.000}", mick1_voltValue) + ",2:" + string.Format("{0:0.000}", mick2_voltValue) + ",3:" + string.Format("{0:0.000}", mick3_voltValue) + ",4:" + string.Format("{0:0.000}", mick4_voltValue);
                SqlOperation.UpdateProductCodeRecord(MesureCode, "currentResult", currentResult);
                SqlOperation.UpdateProductCodeRecord(MesureCode, "voltResult", voltResut);
                string resultString = "";
                sCurrentResultImage images;
                if (!mick1Result) {
                   
                    resultString = resultString + "位置1 ";
                    images.mick1 = Properties.Resources.fail;
                }
                else {
                    
                    images.mick1 = Properties.Resources.succeed;
                  
                }
                if (!mick2Result) {
                    resultString = resultString + "位置2 ";
                    images.mick2 = Properties.Resources.fail;
                }
                else {
                    images.mick2 = Properties.Resources.succeed;
             
                }
                if (!mick3Result) {
                    //PlayVoice(sPlayVoiceAdress.CurrentFail_Mick3);
                    resultString = resultString + "位置3 ";
                    images.mick3 = Properties.Resources.fail;
                }
                else {
                    images.mick3 = Properties.Resources.succeed;
                   
                }
                if (!mick4Result) {
                    //PlayVoice(sPlayVoiceAdress.CurrentFail_Mick4);
                    resultString = resultString + "位置4 ";
                    images.mick4 = Properties.Resources.fail;

                }
                else {
                    images.mick4 = Properties.Resources.succeed;
                   
                }
                Event_EndMickCurrentDetect?.Invoke(images, null);
                //   return true;
             
                if (resultString == "") {
                   // PlayVoice(sPlayVoiceAdress.CurrentFinished);
                    SqlOperation.UpdateProductCodeRecord(MesureCode, "mikeTegResult", "1");
                    return true;
                }
                OutAlarm(resultString + "电流电压检测失败！", sPlayVoiceAdress.CurrentFail_Mick1);
                SqlOperation.UpdateProductCodeRecord(MesureCode, "mikeTegResult", "0");
                //Event_Message?.Invoke(resultString + "麦克风电流电压检测失败！", null);
                return false;


            }
            catch (Exception ex) {
                OutAlarm("电流电压检测失败！",sPlayVoiceAdress.CurrentFail_Mick1);
                //Event_Message?.Invoke("电流电压检测失败！" + ex.Message, null);
                return false;
            }

           
        }
        public void ClosePinDevice() {
            CloseReceivePIN();
            usbLin.CloseDevice();
            // gSwitchPin = false;
            gIsPINConnected = false;
            
        }
        float MINCURRENT = Convert.ToSingle(System.Configuration.ConfigurationManager.AppSettings["MinMickCurrentValue"]);
        /// <summary>
        /// 获取当前电流电压值并判断是否是范围内
        /// </summary>
        /// <param name="currentAdress"></param>
        /// <param name="voltAdress"></param>
        /// <param name="currentValue"></param>
        /// <param name="voltValue"></param>
        /// <returns>true 表示在范围内</returns>
        /// 
        private bool CurrentAndVoltValue(string currentAdress, string voltAdress, out double currentValue, out double voltValue) {
            ushort currentAnalog = DeviceAD_1.Read(currentAdress);
            currentValue = CalculateCurrentRealValue(currentAnalog);
            ushort voltAnalog = DeviceAD_1.Read(voltAdress);
            voltValue = CalculateVoltRealValue(voltAnalog);
            float maxCurrent;
            float maxVolt;
            float minVolt;
        
            SqlOperation.GetProductCurrentRange(gCurrentProdNum, out maxVolt, out minVolt, out maxCurrent);
            if (currentValue > maxCurrent || voltValue > maxVolt || currentValue < MINCURRENT || voltValue < minVolt) { return false; };
            return true;
        }
        private bool DetectLightCurrentValue(out double value) {
            float upper;
            float lower;
            OpenPartSwitch();
            OpenReceiveLin(true);   
            SqlOperation.GetProductLightCurrentRange(gCurrentProdNum,out upper,out lower);
            bool result= TestLightCurrentForTimes(lower, upper, out value );
            value = Math.Round(value, 4);
            return result;

        }
        public double GetLightCurrent() {
            if (!DeviceAD_2.IsConnect) return 0;
            double max = 20;
            double min = 4;
            ushort realValue = DeviceAD_2.Read(sInputRegistSignal2.lightCurrent);
            double value = realValue * (max - min) / 65535 + min;

            return value;
        }
        public double GetButtonVolt() {
            if (!DeviceAD_2.IsConnect) return 0;

            double max = 10;
            double min = 0;
            ushort realValue = DeviceAD_2.Read(sInputRegistSignal2.buttonVolt);
            double value = realValue * (max - min) / 65535 + min;
            return value;
        }
        private double CalculateCurrentRealValue(float analogValue){
            double max = 20;
            double min = 0;
            double value =  analogValue * (max - min) / 65535 + min;   
            return value;
        }
        private float CalculateVoltRealValue(float analogValue) {
            float max = 10;
            float min = 0;
            float value = (max - min) / 65535 * analogValue  + min;
            return value;
        }
        /// <summary>
        /// unused
        /// </summary>
        private void CalculateAvenage() {
            int index = 1;
            float max = 14;
            float min = -14;
            float lastAverageValue = 0;
            while (true) {
                ushort wire1 = DeviceAD_1.Read(sInputRegistSignal.mick1_current);
                float value = (max - min) / wire1 * 65535 + min;
                if (lastAverageValue == 0) lastAverageValue = value;
                else lastAverageValue = (lastAverageValue + value) * index / (index + 1);
                index++;
                Thread.Sleep(200);
            }
        }

        private bool IsPartCode(string strCode) {
            string partConfigCode = "";
            string partSerialCode = "";
            AnalysisPartCode(strCode, out partConfigCode, out partSerialCode);
            string result = SqlOperation.CheckPartCodeType(gCurrentProdNum, partConfigCode, gStation);
            if (result == "Null") {
                return false;
            }
            return true;

        }
        private bool IsEndCode2(string strCode) {
            if (strCode == "[)>06Y400303980DDD0XP263424X012V421278712TJ09A2007080021") {
                return true;
            }
            return false;
        }
        private bool IsEndCode1(string strCode) {
            if (strCode == "[)>06Y400303980DPD0XP263424X012V421278712TJ09A2007080021") {
                return true;
            }
            return false;
        }

        private bool IsCurrentDetectStartCode(string strCode) {
            if (strCode == "[)>06Y400303980DPD0XP263424X012V42J2787I2TJ09A2007080021") {
                return true;
            }
            return false;
        }

        private bool IsCurrentDetectEndCode(string strCode) {
            if (strCode == "detect end") {
                return true;
            }
            return false;
        }

        public void OutAlarm(string strMessage,string voiceAdress) {
            if(voiceAdress != "") PlayVoice(voiceAdress);
            Event_Message?.Invoke(strMessage, null);
            SetAlarmLight();
        }

        public void SetAlarmLight() {
            try {
                if (DeviceDIO_1.IsConnect && DeviceDIO_1.ReadCoil(sOutputDigitalSignal.alarm)) {
                    DeviceDIO_1.WriteCoil(sOutputDigitalSignal.alarm, true);
                }
            }
            catch (Exception e) { }
            
        }
        public void ResetAlarm() {
            try {
                if (DeviceDIO_1.IsConnect && DeviceDIO_1.ReadCoil(sOutputDigitalSignal.alarm)) {
                    DeviceDIO_1.WriteCoil(sOutputDigitalSignal.alarm, false);
                }
            }
            catch (Exception e) { }

        }

        public void PlayVoice(string adress) {
            object locker="";
            lock (locker) {
                Mp3Player mp3Play = new Mp3Player();
                 mp3Play.FileName = System.IO.Directory.GetCurrentDirectory() + @"\VoiceRes\" + adress + ".mp3";
                mp3Play.play();

            }
        }
        public bool CheckProdNumEqualProdCode() {
            string productNum;
            string partSerialNum;
            AnalysisPartCode(gCurrentCode, out productNum, out partSerialNum);
            if (productNum == gCurrentProdNum) return true;
            return false;

        }
        private void AnalysisPartCode(string strCode ,out string partNum , out string partSerialNum) {
            char[] mark = {'X', 'N', 'P' };
            partNum = "";
            if (strCode.Length > 8) {
                if (strCode.Split(mark).Length > 2&& strCode.Split(mark)[2].Length>=8) {
                    partNum = strCode.Split(mark)[2].Substring(0, 8);
                }
                if (strCode[0] >= '0' && strCode[0] <= '9') {
                    partNum = strCode.Substring(0, 8);
                }
                String[] strs = strCode.Split(':');
                if ( strs.Length >= 4 && strs[0] == "SUPPLIER") {
                    partNum = strs[1].Trim(' ').Substring(0, 8);
                    strCode = strs[3].Trim(' ');
                }
            }
            //SUPPLIER: 84861905VPPS: 443.2DUNS: 52812235920200929HR00000233080
            
            if (strCode.Length == 8) partNum = strCode;
            partSerialNum = strCode;
        }
    }

}
