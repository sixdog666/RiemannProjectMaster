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
    public delegate void DelegatePassBackButtonInfo(string waitingTestButtonName, EventArgs e);
    public class TegAtgs : EventArgs {
        public readonly string buttonName;
        public readonly string resultValue;
        public readonly bool resultFlag;
        public TegAtgs(string name, string value,bool flag) {
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
    public struct sButtonPinInfo{
        public string colName;
        public byte[] byteInfo;

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
        public const string LightCurrentSucceed = "LightCurrentSucceed";
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
    struct sOutputDigitalSignal
    {
        public const string on_off = "0";
        public const string alarm = "1";
        public const string signal3 = "2";
        public const string signal4 = "3";
        public const string signal5 = "4";
        public const string signal6 = "5";
        public const string signal7 = "6";
        public const string signal8 = "7";
    }
    struct sInputRegistSignal
    {
        public const string averge = "257";
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
        public const string lightCurrent = "257";
        public const string buttonVolt = "257";
        public const string mick2_volt = "258";
        public const string mick3_volt = "259";
        public const string mick4_volt = "260";
        public const string mick1_current = "261";
        public const string mick2_current = "262";
        public const string mick3_current = "263";
        public const string mick4_current = "264";

    }
    struct sOutputRegistSignal
    {
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


    public class WorkProcess {
        public EventHandler Event_NewProduct;
        public EventHandler Event_NewPart;
        public EventHandler Event_InitLastInfo;
        public EventHandler Event_Message;
        public EventHandler Event_EndProduct;
        public EventHandler Event_EndCurrentDetect;
        
        private MyModbusTCP DeviceDIO_1;
        private MyModbusTCP DeviceAD_1;
        //private MyModbusTCP DeviceDIO_2;
        private MyModbusTCP DeviceAD_2;
        private string gCurrentButtonName;
        private bool gIsPINConnected = false;
        private PINLine pin;
        private string gCurrentProdNum = "";
        private string gCurrentCode = "";
        private int gStation = 1;
        private int gRunningStatu = -1;
        private DataTable gDTStationInfo;
        private ScanerHook barCode;
        private bool gSwitchPin = false;
        private bool gSwitchListen = false;

        private bool gSQLSeversFlag = false;

        public DelegatePassBackButtonInfo ButtonInfoPassBack;

        public bool PINConnectFlag {
            get {
                return pin.ConnectState;
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
            barCode = new ScanerHook();
            DeviceDIO_1 = new MyModbusTCP("192.168.19.80");
            DeviceAD_1 = new MyModbusTCP("192.168.19.81");
            DeviceAD_2 = new MyModbusTCP("192.168.19.83");
            pin = new PINLine();
            //DeviceDIO_2 = new MyModbusTCP("192.168.19.82");
            //DeviceAD_2 = new MyModbusTCP("192.168.19.83");

            //DeviceDIO_2.Connect();
            //DeviceAD_2.Connect();
        }
        public bool OpenPartSwitch() {
            if (DeviceDIO_1.IsConnect) {
                if (DeviceDIO_1.WriteCoil(sOutputDigitalSignal.on_off, true)) return true;
            }
            return false;
        }
        public bool ClosePartSwitch() {

            if (DeviceDIO_1.IsConnect) {
                if (DeviceDIO_1.WriteCoil(sOutputDigitalSignal.on_off, false)) return true;
            }
            return false;
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
            if (!pin.PINLineIniltial()) {
                gIsPINConnected = false;
                Event_Message?.Invoke("车顶PIN信号连接失败", null);
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
        }

        private void StartListeningButtonDown() {   
            if (gSwitchListen == false) {
                gSwitchListen = true;
                Thread listenButtonDown = new Thread(ListenButton);
                if (listenButtonDown.ThreadState != ThreadState.Running) {
                    listenButtonDown.Start();
                }
            }

        }
        public void CloseListeningButtonDown() {
            gSwitchListen = false;
        }
        private void ListenButton() {
            pin.DataReceived += Hid_ReceivedData;
            bool isVoltTestEnd = false;
            string nextButtonName="";
            while (gSwitchListen) {
                try {
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
<<<<<<< HEAD
                        // if (result == null) continue;
                        if (result != null && (bool)result) {
=======
                       // if (result == null) continue;
                        if (result != null&&(bool)result) {
>>>>>>> c9f000e2af71d7227128b1a685b7541779b86b5c
                            DataRow dr = SqlOperation.GetButtonInfo(testButtonName, out nextButtonName);
                            SqlOperation.UpdateButtonState(gCurrentCode, dr["tbl_ColumnName"].ToString(), testValue.ToString());
                            isVoltTestEnd = false;
                            TegAtgs arg = new TegAtgs(testButtonName, testValue.ToString(), true);
                            gCurrentButtonName = nextButtonName;
                            //string strTip = "松开"+ testButtonName;
                            ButtonInfoPassBack?.Invoke(gCurrentButtonName, arg);
                        }
                        else {
<<<<<<< HEAD
                            if (result != null) {
                                TegAtgs arg = new TegAtgs(testButtonName, testValue.ToString(), false);
                                ButtonInfoPassBack?.Invoke(testButtonName, arg);
                            }
=======

                            TegAtgs arg = new TegAtgs(testButtonName, testValue.ToString(), false);
                            ButtonInfoPassBack?.Invoke(testButtonName, arg);
>>>>>>> c9f000e2af71d7227128b1a685b7541779b86b5c
                        }
                    }

                    Thread.Sleep(200);

                }
                catch (Exception ex) {
<<<<<<< HEAD
                    Event_Message?.Invoke(ex.Message, null);
=======
                    Event_Message?.Invoke(ex.Message,null);
>>>>>>> c9f000e2af71d7227128b1a685b7541779b86b5c
                }


            }
            ButtonInfoPassBack?.Invoke("已完成", null);
            pin.DataReceived -= Hid_ReceivedData;
        }

        private object WaitForButtonDown(string itemName, out double testValue) {
            string nextButtonName;
            testValue = 0;
            DataRow row = SqlOperation.GetButtonInfo(itemName, out nextButtonName);
            if (row!= null && Convert.ToBoolean(row["tbl_VCFlag"])) {
                double maxValue = (double)row["tbl_MaxValue"];
                double minValue = (double)row["tbl_MinValue"];
                testValue = GetButtonVolt();////////
                if (testValue <= maxValue && testValue >= minValue) {
                    return true;
                }
                else {
                    return false;
                }

            }
            return null;
        }
        private bool WaitForButtonUp() {
            double testValue = GetButtonVolt();/////
            if (testValue == 0) return true;
            return false;
        }


        public void InitTEG(string currentCode) {
            DataTable dt = SqlOperation.GetButtonConfigInfo();
            bool needTestFlag = false;
            gCurrentButtonName = string.Empty;
            for (int i = 0; i < dt.Rows.Count; i++) {
                DataRow dr = dt.Rows[i];
                string buttonColName = dr["ID"].ToString();
                string buttonName = dr["检测项"].ToString();
                string result = SqlOperation.GetButtonState(buttonColName, currentCode);
                if (result != string.Empty) {
                    if (i < dt.Rows.Count - 1) {
                        //gCurrentButtonName = buttonName;
                    }             
                    TegAtgs arg = new TegAtgs(buttonName, result,true);
                    ButtonInfoPassBack?.Invoke(null, arg);
                }
                else {
                    if (gCurrentButtonName == string.Empty)
                        gCurrentButtonName = buttonName;
                    needTestFlag = true;
                    TegAtgs arg = new TegAtgs(buttonName, result, false);
                    ButtonInfoPassBack?.Invoke(gCurrentButtonName, arg);
                }
            }
            if (needTestFlag) StartListeningButtonDown();
            else CloseListeningButtonDown();

        }

        private void Hid_ReceivedData(object sender, report e) {
            if (gSwitchListen == true) {
                string nextButtonName;
                DataRow button = SqlOperation.GetButtonInfo(gCurrentButtonName,out nextButtonName);
                if (button != null ) {
                    if (e.reportBuff == button["tbl_ByteString"]) {
                        SqlOperation.UpdateButtonState(gCurrentCode, button["tbl_ColumnName"].ToString(), "已检测完成");
                        TegAtgs arg = new TegAtgs(gCurrentButtonName, true.ToString(),true);
                        ButtonInfoPassBack?.Invoke(nextButtonName, arg);
                        gCurrentButtonName = nextButtonName;
                  
                    }
                }
            }
        }


        public void OpenPIN() {
            if (!gIsPINConnected) return;
            Thread pinThread = new Thread(Thread_PIN);
            if (gSwitchPin != true && pinThread.ThreadState != ThreadState.Running) {
                gSwitchPin = true;
                pinThread.Start();
            }
        }
        public void ClosePIN() {
            gSwitchPin = false;
        }
        private void Thread_PIN() {
            try {
                while (gSwitchPin) {
                    pin.SetDataPacket();
                    Thread.Sleep(500);
                }
            }
            catch {
            }
        }
        private void ResetDO() {
            if (DeviceDIO_1.IsConnect) {
                DeviceDIO_1.WriteCoil(sOutputDigitalSignal.alarm, false);
                DeviceDIO_1.WriteCoil(sOutputDigitalSignal.on_off, false);
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
                    InitTEG(gCurrentCode);
                    Event_InitLastInfo?.Invoke(dt, null);
                    gRunningStatu = (int)dt.Rows[0]["statu"];
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
                    InAssemblyStatu_Station1(strCode, out alarmFlag);
                    break;
                case eStation1_WorkProcess.Detecting://电流检测中
                    InCurrentDetectStatu_Station1(strCode, out alarmFlag);
                    break;
                case eStation1_WorkProcess.EndDect://电流检测结束
                    InEndDectStatu_Station2(strCode, out alarmFlag);
                    break;
                default:
                    OutAlarm("当前产品状态与工位信息不匹配", sPlayVoiceAdress.Fail_ScanCode);
                    break;
            }
            if (!alarmFlag) {
                ResetAlarmLight();
            }
        }
        private void Task_Station2(string strCode) {
            bool alarmFlag = false;
            switch ((eStation2_WorkProcess)gRunningStatu) {
                case eStation2_WorkProcess.EndAssembly://结束状态(初始状态)，允许产品条码
                    if (SqlOperation.GetProductCodeStatu(strCode) == (int)eStation1_WorkProcess.EndAssembly)
                        InInitialOrEndStatu_Station2(strCode, out alarmFlag);
                    else
                        OutAlarm("当前产品状态异常", sPlayVoiceAdress.Fail_ScanCode);
                    break;
                case eStation2_WorkProcess.StartAssembly://装配状态
                    InAssemblyStatu_Station2(strCode, out alarmFlag);
                    break;
                case eStation2_WorkProcess.Detecting://电流检测中
                    InCurrentDetectStatu_Station2(strCode, out alarmFlag);
                    break;
                case eStation2_WorkProcess.EndDect://电流检测结束
                    InEndDectStatu_Station2(strCode, out alarmFlag);
                    break;
                default:
                    OutAlarm("当前产品状态与工位信息不匹配", sPlayVoiceAdress.Fail_ScanCode);
                    break;
            }
            if (!alarmFlag)
            {
                ResetAlarmLight();
            }

        }

        private void InInitialOrEndStatu_Station1(string strCode, out bool resultFlag) {
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
        private void InInitialOrEndStatu_Station2(string strCode, out bool resultFlag)
        {
            switch (CheckInputCodeType(strCode))
            {
                case eCodeType.Assembly:
                    if (ProductCodeScan(strCode) == true)
                    {
                        InitTEG(gCurrentCode);
                        gRunningStatu = (int)eStation2_WorkProcess.StartAssembly;
                        resultFlag = true;
                        //SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);
                    }
                    resultFlag = false;
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    resultFlag = false;
                    break;
            }
        }
        private void InAssemblyStatu_Station1(string strCode, out bool resultFlag) {
            switch (CheckInputCodeType(strCode)) {
                case eCodeType.StartCurrent:
                    if (CurrentDectectStart(strCode)) {
                    //    gRunningStatu = (int)eStation1_WorkProcess.Detecting;
                  //      SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);
                        resultFlag = true;
                    }
                    else {
                        resultFlag = false;
                    }
                    break;
                case eCodeType.Part:
                    PartCodeScan(strCode);
                    resultFlag = true;
                    break;
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
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    resultFlag = false;
                    break;
            }
        }
        private void InAssemblyStatu_Station2(string strCode, out bool resultFlag)
        {
            switch (CheckInputCodeType(strCode))
            {
                case eCodeType.StartCurrent:
                    CurrentDectectStart(strCode);
                    resultFlag = true;
                    break;
                case eCodeType.Part:
                    PartCodeScan(strCode);
                    resultFlag = true;
                    break;
                case eCodeType.DetectLightCurrent:
                    if (LightCurrentDetectStart()) {
                        resultFlag = true;
                    }
                    else resultFlag = false;
                    break;
                case eCodeType.Finish2:
                    if (IsFinshScan())
                    {
                        gRunningStatu = (int)eStation2_WorkProcess.EndAssembly;
                        ProductEnd(strCode);
                        PlayVoice(sPlayVoiceAdress.FinishSation2);
                        resultFlag = true;
                    }
                    else
                    {
                        OutAlarm("当前扫码未完成", sPlayVoiceAdress.UnFinish);
                        resultFlag = false;
                    }
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    resultFlag = false;
                    break;
            }
        }
        private bool  LightCurrentDetectStart() {
            double testvalue;
            if (DetectLightCurrentValue(out testvalue)) {
                SqlOperation.UpdateLightCurrentValue(gCurrentCode, testvalue);
                PlayVoice(sPlayVoiceAdress.LightCurrentSucceed);
                return true;
            }
            else {
                OutAlarm("当前电流检测值异常"+ testvalue, sPlayVoiceAdress.LightCurrentError);
                PlayVoice(sPlayVoiceAdress.LightCurrentError);
                return false;
            }
        }  
        private void InCurrentDetectStatu_Station1(string strCode,out bool resultFlag) {
            switch (CheckInputCodeType(strCode)) {
                case eCodeType.EndCurrent:
                    CurrentDectectEnd(strCode);
                    resultFlag = true;
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    resultFlag = false;
                    break;
            }
        }
        private void InCurrentDetectStatu_Station2(string strCode, out bool resultFlag)
        {
            switch (CheckInputCodeType(strCode))
            {
                case eCodeType.EndCurrent:
                    CurrentDectectEnd(strCode);
                    resultFlag = true;
                    break;
                default:
                    OutAlarm("扫码错误", sPlayVoiceAdress.Fail_ScanCode);
                    resultFlag = false;
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
        private void InEndDectStatu_Station2(string strCode, out bool resultFlag)
        {
            switch (CheckInputCodeType(strCode))
            {
                case eCodeType.Finish2:
                    if (IsFinshScan()){
                        gRunningStatu = (int)eStation2_WorkProcess.EndAssembly;
                        ProductEnd(strCode);
                        PlayVoice(sPlayVoiceAdress.FinishSation2);
                        resultFlag = true;
                    }
                    else{
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
            if (IsPartCode(strCode)) {
                return eCodeType.Part;
            }
            if (IsEndCode1(strCode)) {
                return eCodeType.Finish1;
            }
            if (IsEndCode2(strCode)) {
                return eCodeType.Finish2;
            }
            if (IsDetectLightCurrentCode(strCode)) {
                return eCodeType.DetectLightCurrent;
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
            if (strCode == "") return true;
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
                PlayVoice(fieldName);
                if (SqlOperation.UpdateProductCodeRecord(gCurrentCode, fieldName, partSerialCode)) {
                    Event_NewPart?.Invoke(fieldName, null);
                }
            }
            else {
                OutAlarm("当前检测总成条形码状态错误", sPlayVoiceAdress.Fail_ScanCode);
            }
        }
        private void CurrentDectectEnd(string strCode)
        {
               gRunningStatu = (int)eStation2_WorkProcess.EndDect;
            SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);

        }
        private void ProductEnd(string strCode)
        {
            ClosePIN();
            CloseListeningButtonDown();
            ClosePartSwitch();
            SqlOperation.UpdateProductCodeStatuRecord(gCurrentCode, gRunningStatu);
            Event_EndProduct?.Invoke(gCurrentCode, null);
        }
        private bool CurrentDectectStart(string strCode) {
            return MeasureValue();
        }
        private bool MeasureValue() {
            try {
                //启动电源开关
                OpenPartSwitch();
                OpenPIN();
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
                SqlOperation.UpdateProductCodeRecord(gCurrentCode, "currentResult", currentResult);
                SqlOperation.UpdateProductCodeRecord(gCurrentCode, "voltResult", voltResut);
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
                Event_EndCurrentDetect?.Invoke(images, null);
                if (resultString == "") {
                    PlayVoice(sPlayVoiceAdress.CurrentFinished);
                    return true;
                }
                OutAlarm(resultString + "电流电压检测失败！", sPlayVoiceAdress.CurrentFail_Mick1);
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
            ClosePIN();
            ClosePartSwitch();
           // gSwitchPin = false;
            pin.ClosePINDevice();
        }
        /// <summary>
        /// 获取当前电流电压值并判断是否是范围内
        /// </summary>
        /// <param name="currentAdress"></param>
        /// <param name="voltAdress"></param>
        /// <param name="currentValue"></param>
        /// <param name="voltValue"></param>
        /// <returns>true 表示在范围内</returns>
        private bool CurrentAndVoltValue(string currentAdress, string voltAdress, out double currentValue, out double voltValue) {
            ushort currentAnalog = DeviceAD_1.Read(currentAdress);
            currentValue = CalculateCurrentRealValue(currentAnalog);
            ushort voltAnalog = DeviceAD_1.Read(voltAdress);
            voltValue = CalculateVoltRealValue(voltAnalog);
            float maxCurrent;
            float maxVolt;
            float minVolt;
            SqlOperation.GetProductCurrentRange(gCurrentProdNum, out maxVolt, out minVolt, out maxCurrent);
            if (currentValue > maxCurrent || voltValue > maxVolt || voltValue < minVolt) { return false; };
            return true;
        }
        private bool DetectLightCurrentValue(out double value) {
            float upper;
            float lower;
            double testValue;
            SqlOperation.GetProductLightCurrentRange(gCurrentProdNum,out upper,out lower);
            testValue = GetLightCurrent();
            value = testValue;
            if (testValue >= lower && testValue <= upper) return true;
            return false;

        }
        public double GetLightCurrent() {
            if (!DeviceAD_2.IsConnect) return 0;
            double max = 20;
            double min = 0;
<<<<<<< HEAD
            ushort realValue = DeviceAD_2.Read(sInputRegistSignal2.lightCurrent);
=======
            ushort realValue = DeviceAD_2.ReadRegist(sInputRegistSignal2.lightCurrent);
>>>>>>> c9f000e2af71d7227128b1a685b7541779b86b5c
            double value = realValue * (max - min) / 65535 + min;
            return value;
        }
        public double GetButtonVolt() {
            if (!DeviceAD_2.IsConnect) return 0;
            double max = 20;
            double min = 0;
<<<<<<< HEAD
            ushort realValue = DeviceAD_2.Read(sInputRegistSignal2.buttonVolt);
=======
            ushort realValue = DeviceAD_2.ReadRegist(sInputRegistSignal2.buttonVolt);
>>>>>>> c9f000e2af71d7227128b1a685b7541779b86b5c
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
        public void ResetAlarmLight() {
            try {
                if (DeviceDIO_1.IsConnect && DeviceDIO_1.ReadCoil(sOutputDigitalSignal.alarm)) {
                    DeviceDIO_1.WriteCoil(sOutputDigitalSignal.alarm, false);
                }
            }
            catch (Exception e) { }

        }

        private void PlayVoice(string adress) {
            Mp3Player mp3Play = new Mp3Player() {
                FileName = System.IO.Directory.GetCurrentDirectory() + @"\VoiceRes\" + adress + ".mp3",  
            };
            mp3Play.play();
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
            }
            if (strCode.Length == 8) partNum = strCode;
            partSerialNum = strCode;
        }
    }

}
