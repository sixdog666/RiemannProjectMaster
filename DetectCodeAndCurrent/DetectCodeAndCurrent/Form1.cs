using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Configuration;

namespace DetectCodeAndCurrent {
    public enum eUCStatuType {
        Init,
        Fail,
        Sucess
    }

    public struct sProductInfo{
        public string name;
        public string configCode;
        public string value;
        public string typeName;
        public UCDetectItem ucItem;
        public int station;
    }

    public struct sButtonInfo {
        public string dbColumnName;
        public UCDetectItem ucItem;
        public eStateType statuType;
        public int station;
        public string dataByte;
    }

    public struct sMessageType {
        public const string TIP = "提示";
        public const string ERROR = "错误";
        public const string WARING = "警告";
    }
    public partial class DectectMainForm : Form {
        //const string gConnStr = "data source=127.0.0.1;database=jixing_db;user id=root;password=riemann;pooling=false;charset=utf8";
        List<sProductInfo> gProductInfosList;
        List<sButtonInfo> gButtonInfosList;
        public DectectMainForm() {
            InitializeComponent();
        }
        private void DectectMainForm_Load(object sender, EventArgs e) {
            WorkProcess instance = WorkProcess.GetInstance();
            instance.Event_EndMickCurrentDetect += CurrentDetectEnd;
            instance.Event_EndLightCurrentDetect += LightCurrentDetectEnd;
            instance.Event_NewProduct += NewProduct_Start;
            instance.Event_NewPart += NewPart_Start;
            instance.Event_InitLastInfo += LastRecord_Init;
            instance.Event_Message += NewWorkMessage_Come;
            instance.Event_EndProduct += Product_End;
            instance.ButtonInfoPassBack += TEGResult_Show;
            MysqlConnector sqlInstance = MysqlConnector.GetInstance();
            sqlInstance.threadExceptionEventHandler = SqlError_Happend;
            InitControls();
            instance.InitialWork();
            InitMp3();
        }

        private void TEGResult_Show(string tipTestName, EventArgs e) {
            try {
                if (InvokeRequired) {
                    Invoke(new MethodInvoker(delegate () {
                        ShowTEGResult(tipTestName, e);
                    }));
                }
                else {
                    ShowTEGResult(tipTestName, e);
                }

            }
            catch (Exception) {
            }
        }
        private void ShowTEGResult(string tipTestName, EventArgs e) {
            if (tipTestName != string.Empty)
            {
                if (tipTestName != null)
                {
                    labTipTestButton.Text = tipTestName;
                    int i = FindCell(tipTestName);
                    if (i != -1)
                        dgvTEGShow.Rows[i].Selected = true;
                }
            }
            else
            {
                WorkProcess process = WorkProcess.GetInstance();
                process.CloseListeningButtonDown();
                labTipTestButton.Text = "检测未开始";
            }
            if (e != null) {

                TegAtgs teginfo = (TegAtgs)e;
                int index = FindCell(teginfo.buttonName);
                if (index != -1) {
                    dgvTEGShow.Rows[index].Cells["测量值"].Value= teginfo.resultValue;
                    DataGridViewCell cell = dgvTEGShow.Rows[index].Cells["测量值"];
                    if (teginfo.resultFlag == true) {
                        cell.Style.BackColor = Color.Green;
                        //cell.Value = "已完成";
                        
                    }
                    else {
                        cell.Style.BackColor = Color.Red;
                       // cell.Value = "未通过";
                        
                    }
                     
                }
                //teginfo.buttonName

            }
        }

        private int FindCell(string buttonName) {
            for (int i = 0; i < dgvTEGShow.Rows.Count; i++) {
                DataGridViewCell cell = dgvTEGShow.Rows[i].Cells["检测项"];
                if (cell.Value!=null && cell.Value.ToString() == buttonName) {
                    return cell.RowIndex;
                }
            }
            return -1;
        }
        private void InitTEGShow() {
            WorkProcess instance = WorkProcess.GetInstance();
            DataTable dt = SqlOperation.GetButtonConfigInfo(instance.strCurrentProd);
            dgvTEGShow.DataSource = dt;
            //DataGridViewCellStyle dgvCellStyle = new DataGridViewCellStyle();
            //dgvCellStyle.BackColor = Color.Red;
            if(dgvTEGShow.ColumnCount!=0)
            dgvTEGShow.Columns["ID"].Visible = false;

            // dgvTEGShow.DataSource

        }
        private void LightCurrentDetectEnd(object sender,EventArgs e) {
            sLightCurrentResult result = (sLightCurrentResult)sender;
            Show_LightCurrentResult( result.currentValue,result.resultFlag);

        }
        private void Show_LightCurrentResult(string value,bool result) {
            if (this.InvokeRequired) {
                Invoke(new MethodInvoker(delegate () {
                 
                    if (value == string.Empty) {
                        tbxLightCurrentValue.BackColor = Color.White;
                        
                    }
                    else if (result) {
                        tbxLightCurrentValue.BackColor = Color.Blue;
                    }
                    else
                        tbxLightCurrentValue.BackColor = Color.Red;
                    tbxLightCurrentValue.Text = value;
                }));
            }
            else {
                if (value == string.Empty) {
                    tbxLightCurrentValue.BackColor = Color.White;
                  
                }else
                if (result) {
                    tbxLightCurrentValue.BackColor = Color.Green;
                }
                else
                    tbxLightCurrentValue.BackColor = Color.Red;
                tbxLightCurrentValue.Text = value;
            }
        }
        private void SqlError_Happend(Exception ex) {
           
            ShowMessage(sMessageType.ERROR, ex.Message);
        }
        private void NewWorkMessage_Come(object sender, EventArgs e) {
            ShowMessage(sMessageType.ERROR, (string)sender);
        }
        private void NewPart_Start(object sender,EventArgs e) {
            sProductInfo info = gProductInfosList.Find(s => s.name.Equals((string)sender));
            if(info.name != null)
            info.ucItem.UpdateItemShowState((int)eStateType.Finished);
        }
        private void CurrentDetectEnd(object sender,EventArgs e) {
            sCurrentResultImage result = (sCurrentResultImage)sender;
            if (this.InvokeRequired) {
                Invoke(new MethodInvoker(delegate () {
                    panelMikeCurrent.BackColor = Color.Transparent;
                    pbxMick1.Image = result.mick1;
                    pbxMick2.Image = result.mick2;
                    pbxMick3.Image = result.mick3;
                    pbxMick4.Image = result.mick4;
                }));
            }
            else {
                panelMikeCurrent.BackColor = Color.Transparent;
                pbxMick1.Image = result.mick1;
                pbxMick2.Image = result.mick2;
                pbxMick3.Image = result.mick3;
                pbxMick4.Image = result.mick4;
            }

        }
        private void InitalTSSBtn() {
            if (labPostion.Text == "1") {
                //tabGroup.TabPages.Remove(tabDetect);
                tabGroup.SelectedTab = tabScanCodePage;
                tabControl1.TabPages.Remove(tabPage3);
                tsslabLin.Visible = false;
                tsslLinRevice.Visible = false;
            }
            else {
                tabGroup.SelectedTab = tabTEGResult;
              //  InitTEGShow();
            }
            if (this.InvokeRequired) {
                Invoke(new MethodInvoker(delegate () {
                    pbxMick1.Image = Properties.Resources.Notification;
                    pbxMick2.Image = Properties.Resources.Notification;
                    pbxMick3.Image = Properties.Resources.Notification;
                    pbxMick4.Image = Properties.Resources.Notification;
                }));
            }
            else {
                pbxMick1.Image = Properties.Resources.Notification;
                pbxMick2.Image = Properties.Resources.Notification;
                pbxMick3.Image = Properties.Resources.Notification;
                pbxMick4.Image = Properties.Resources.Notification;
            }
        }
        private void NewProduct_Start(object sender, EventArgs e) {
            LoadPartsShow();
            InitTEGShow();
        }
        private void Product_End(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate () {
                    cmbProduct.Enabled = true;
                    ShowMessage(sMessageType.TIP, "当前产品" + (string)sender + "检测完成");
                    flowLayoutPanel1.Controls.Clear();
                    panelMikeCurrent.BackColor = Color.Transparent;
                    txtProductCode.Text = string.Empty;
                    InitalTSSBtn();
                    Show_LightCurrentResult("",true);
                }));

            }
            else
            {
                txtProductCode.Text = string.Empty;
                cmbProduct.Enabled = true;
                ShowMessage(sMessageType.TIP, "当前产品" + (string)sender + "检测完成");
                flowLayoutPanel1.Controls.Clear();
                panelMikeCurrent.BackColor = Color.Transparent;
                Show_LightCurrentResult("", true);
                InitalTSSBtn();
            }
           
        }
        private void LastRecord_Init(object sender, EventArgs e) {
            DataTable dt = (DataTable)sender;
            cmbProduct.Enabled = false;
            if (dt != null && dt.Rows.Count > 0) {
                cmbProduct.SelectedItem = (string)dt.Rows[0]["product"];
                txtProductCode.Text = (string)dt.Rows[0]["productCode"];
                Show_CurrentProductCodeDisplayInfo();
                InitTEGShow();
                ShowMessage(sMessageType.TIP,"当前总装条形码为："+ txtProductCode.Text);
                
            }

        }
       


        private void ButtonControlsState_Show() {
            foreach (sButtonInfo info in gButtonInfosList) {
                info.ucItem.UpdateItemShowState((int)info.statuType); 
            }
        }

        private void ButtonControlsState_Show(string name) {
         sButtonInfo info =  gButtonInfosList.Find(s => s.dbColumnName.Equals((string)name));
            info.ucItem.UpdateItemShowState((int)info.statuType);
        }

        private void InitControls() {
            InitalTSSBtn();
            lstMessage.View = View.Details;
            lstMessage.Columns.Add("", 10, HorizontalAlignment.Left);
            lstMessage.Columns.Add("时间", 250, HorizontalAlignment.Center);
            lstMessage.Columns.Add("类型", 150, HorizontalAlignment.Left);
            lstMessage.Columns.Add("消息内容", lstMessage.Width - 310, HorizontalAlignment.Left);
            WorkProcess instance = WorkProcess.GetInstance();
            int station = Convert.ToInt32(labPostion.Text);
            if (instance.InitialStationInfo(station)) {
                List<string> productNames = SqlOperation.GetProductInfosFromSQL();
                foreach (string productName in productNames) {
                    cmbProduct.Items.Add(productName);
                }
                if (cmbProduct.Items.Count > 0) cmbProduct.SelectedItem = System.Configuration.ConfigurationManager.AppSettings["ProductTypeName"];
                //instance.InitialLastRunningInfo();
                ShowMessage(sMessageType.TIP, "系统初始化完成");
            }
            else {
                ShowMessage(sMessageType.ERROR, "系统错误");
            }

            timerShowState.Enabled = true;
        }
        private void SaveLastProductType(string selectName) {
            try {
                Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                cfa.AppSettings.Settings["ProductTypeName"].Value = selectName;
                cfa.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception) {
          
            }
        }

        private void ShowMessage(string messageType, string message) {


            if (this.InvokeRequired) {
                Invoke(new MethodInvoker(delegate () {
                    UpdateCheckBox(messageType, message);
                }));

            }
            else {
                UpdateCheckBox(messageType, message);
            }

        }
        private void UpdateCheckBox(string messageType, string message) {
            string TimeString = System.DateTime.Now.ToString();
            while (lstMessage.Items.Count > 999) {
                lstMessage.Items.RemoveAt(lstMessage.Items.Count - 1);
            }
            //显示列表
            ListViewItem item = new ListViewItem(new string[] { "", TimeString, messageType, message }, 0);
           // if (Error) item.BackColor = Color.Red;
            lstMessage.Items.Insert(0, item);
        }


        private void LoadPartsShow() {
            if (this.InvokeRequired) {
                Invoke(new MethodInvoker(delegate () {
                    cmbProduct.Enabled = false;
                    WorkProcess instance = WorkProcess.GetInstance();
                    string tegValue;
                    string mikeValue;
                    instance.InitLastTEGResult(instance.strCurrentCode, out tegValue,out mikeValue);
                    if (mikeValue != string.Empty&&mikeValue.ToString()!=1.ToString()) panelMikeCurrent.BackColor = Color.Red;
                    tbxLightCurrentValue.Text = tegValue.ToString();
                    txtProductCode.Text = instance.strCurrentCode;
                    gProductInfosList = SqlOperation.GetProductAssembleFromSQL(instance.strCurrentProd);
                    SqlOperation.GetCurrentPartRecordFromSQL(instance.strCurrentCode, instance.strCurrentProd, ref gProductInfosList);
                    InitCurrentControlList();
                }));

            }
            else {
                cmbProduct.Enabled = false;
                WorkProcess instance = WorkProcess.GetInstance();
                string tegValue;
                string mikeValue;
                instance.InitLastTEGResult(instance.strCurrentCode, out tegValue, out mikeValue);
                if (mikeValue != string.Empty && mikeValue.ToString() != 1.ToString()) panelMikeCurrent.BackColor = Color.Red;
                tbxLightCurrentValue.Text = tegValue.ToString();
                txtProductCode.Text = instance.strCurrentCode;
                gProductInfosList = SqlOperation.GetProductAssembleFromSQL(instance.strCurrentProd);
                SqlOperation.GetCurrentPartRecordFromSQL(instance.strCurrentCode, instance.strCurrentProd, ref gProductInfosList);
                InitCurrentControlList();
            }
           

        }
        private void InitCurrentControlList() {
            flowLayoutPanel1.Controls.Clear();
            foreach (sProductInfo info in gProductInfosList) {
                info.ucItem.detectItemName = info.typeName;
                if (info.value != null && info.value != "") {
                    UploadStatu(info.ucItem, eStateType.Finished);
                }
                else {
                    UploadStatu(info.ucItem, eStateType.Waiting);
                }
                if (info.station != WorkProcess.GetInstance().iStation) {
                    info.ucItem.Enabled = false;
                }
                else {
                    info.ucItem.Enabled = true;
                }
                flowLayoutPanel1.Controls.Add(info.ucItem);
            }

        }
        private void UploadStatu(UCDetectItem ucItem,eStateType type) {
               ucItem.UpdateItemShowState((int)type);
        }

  
        private void cmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedItem == null) return;
            SaveLastProductType( cmbProduct.SelectedItem.ToString());
            float upper = 0, lower= 0, upperCurrent = 0;
            string currentProduct = "";
            WorkProcess instance = WorkProcess.GetInstance();
            gProductInfosList = SqlOperation.GetProductAssembleFromSQL(instance.strCurrentProd);
            SqlOperation.GetProductConfigCodeFromSQL(cmbProduct.SelectedItem.ToString(), out currentProduct);
            instance.strCurrentProd = currentProduct;
            SqlOperation.GetProductCurrentRange(currentProduct, out upper, out lower, out upperCurrent);
            tsslProductConfigCode.Text = currentProduct;
            tsslCurrentLower.Text = lower.ToString();
            tsslCurrentUpper.Text = upper.ToString();
            tsslUpperCurrent.Text = upperCurrent.ToString();
        }

        //private void btnConfirm_Click(object sender, EventArgs e) {
        //    WorkProcess.GetInstance().strCurrentCode = txtProductCode.Text;
        //    Show_CurrentProductCodeDisplayInfo();
        //}

        private void Show_CurrentProductCodeDisplayInfo() {

            WorkProcess instance = WorkProcess.GetInstance();

        //    gProductInfosList = SqlOperation.GetProductAssembleFromSQL(instance.strCurrentProd);
            if (instance.CheckProdNumEqualProdCode()) {
                LoadPartsShow();
            }
            else {
                 //  WorkProcess.OutAlarm();
            }
        }
        private void button1_Click(object sender, EventArgs e) {
            string strCode = txtInputCode.Text;
            WorkProcess.GetInstance().Task_Work(strCode);
        }



        private void btnSearch_Click(object sender, EventArgs e) {
            UpdateDateGridView();
        }

        private void UpdateDateGridView() {
            DateTime startTime = new DateTime(2000, 1, 1);
            DateTime endTime = DateTime.Now;
            string code = txtAssembleCode.Text.Trim(' ');
            if (chkStartDate.Checked) {
                startTime = dtpStartDate.Value;
            }
            if (chkEndDate.Checked) {
                endTime = dtpEndDate.Value;
            }
            BindingSource bs = new BindingSource();
            bs.DataSource = SqlOperation.GetRecordFormSQL(code, startTime, endTime);
            bdgSearch.BindingSource = bs;
            dgvRecords.DataSource = bs.DataSource;
        }

        private void btnConfig_Click(object sender, EventArgs e) {
            FromConfiguration configForm = new FromConfiguration();
            UserConfrimFrom userFrom = new UserConfrimFrom();
            userFrom.ShowDialog();
            if (userFrom.checkResult == true) {
                configForm.ShowDialog();
                ChangePermission(true);
            }
            else {
                ChangePermission(false);
            }
        }

        private void ChangePermission(bool value) {
            if (value == true) {
                tsbDelete.Enabled = true;
                tsbDeleteTEGResult.Enabled = true;
                tsbComfrim.Enabled = true;
            }
            else {
                tsbDelete.Enabled = false;
                tsbComfrim.Enabled = false;
                tsbDeleteTEGResult.Enabled = false;

            }

        }

        private void toolStripButton1_Click(object sender, EventArgs e) {
            
            if (dgvRecords.CurrentRow != null) {
                if (MessageBox.Show("确认删除选中行？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                    string strProductCode = (string)dgvRecords.CurrentRow.Cells["总装条形码"].Value;
                    SqlOperation.DeleteProductCodeRecode(strProductCode);
                    UpdateDateGridView();
                }

            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e) {
            if (dgvRecords.CurrentRow != null && tscmb.SelectedItem!=null) {
                int newStatu = 0;
                string currentCode = (string)dgvRecords.CurrentRow.Cells["总装条形码"].Value;

                switch (tscmb.SelectedIndex) {
                    case 0:
                        newStatu = (int)eStation1_WorkProcess.StartAssembly;
                        break;
                    case 1:
                        newStatu = (int)eStation1_WorkProcess.EndAssembly;
                        break;
                    case 2:
                        newStatu = (int)eStation2_WorkProcess.StartAssembly;
                        break;
                    case 3:
                        newStatu = (int)eStation2_WorkProcess.EndAssembly;
                        break;
                    default:
                        newStatu = -1;
                        break;
                }
                if (newStatu != -1) {
                    SqlOperation.UpdateProductCodeStatuRecord(currentCode, newStatu);
                    UpdateDateGridView();
                }
            }
        }

        private void dgvRecords_CurrentCellChanged(object sender, EventArgs e) {
            if (dgvRecords.CurrentRow != null) {
                string code = (string)dgvRecords.CurrentRow.Cells["总装条形码"].Value;
                int statu = SqlOperation.GetProductCodeStatu(code);
                switch (statu) {
                    case (int)eStation1_WorkProcess.StartAssembly: 
                    case (int)eStation1_WorkProcess.Detecting:
                    case (int)eStation1_WorkProcess.EndDect:
                        tscmb.SelectedIndex = 0;
                        break;
                    case (int)eStation1_WorkProcess.EndAssembly:
                        tscmb.SelectedIndex = 1;
                        break;
                    case (int)eStation2_WorkProcess.StartAssembly:
                    case (int)eStation2_WorkProcess.Detecting:
                    case (int)eStation2_WorkProcess.EndDect:
                        tscmb.SelectedIndex = 2;
                        break;
                    case (int)eStation2_WorkProcess.EndAssembly:
                        tscmb.SelectedIndex = 3;
                        break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            WorkProcess instance = WorkProcess.GetInstance();
            switch (btnPartsSwitch.Text) {
                case "附件开关打开":
                    if (instance.LinThreadFlag == false) {
                        instance.OpenReceiveLin(true);
                    }
                    if (instance.OpenPartSwitch()) {
                        
                        btnPartsSwitch.Text = "附件开关关闭";
                        btnPartsSwitch.BackColor = Color.Green;
                        timeShow.Enabled = true;
                    }
                    break;
                case "附件开关关闭":
                    if (instance.LinThreadFlag == true) {
                        instance.CloseReceivePIN();
                    }
                    if (instance.ClosePartSwitch(false)) {                       
                        btnPartsSwitch.Text = "附件开关打开";
                        btnPartsSwitch.BackColor = Color.Gray;
                        timeShow.Enabled = false;
                    }
                    break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            WorkProcess instance = WorkProcess.GetInstance();
            string mick1, mick2, mick3, mick4;
            instance.GetMickCurrent(out mick1, out mick2, out mick3, out mick4);
            tbxMick1Current.Text = mick1;
            tbxMick2Current.Text = mick2;
            tbxMick3Current.Text = mick3;
            tbxMick4Current.Text = mick4;
            instance.GetMickVolt(out mick1, out mick2, out mick3, out mick4);
            tbxMick1Volt.Text = mick1;
            tbxMick2Volt.Text = mick2;
            tbxMick3Volt.Text = mick3;
            tbxMick4Volt.Text = mick4;
            tbxLightCurrent.Text =string.Format("{0:0.0000}",instance.GetLightCurrent());
            tbxButtonVolt.Text = string.Format("{0:0.0000}",instance.GetButtonVolt());
        }

        private void pictureBox2_Click(object sender, EventArgs e) {

        }

        private void DectectMainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (labPostion.Text == "2")
            {
                WorkProcess instance = WorkProcess.GetInstance();
                instance.CloseListeningButtonDown();
                instance.ClosePinDevice();
                instance.ClosePartSwitch(false);

            }
             
        }

        private void timerShowState_Tick(object sender, EventArgs e) {
            WorkProcess process = WorkProcess.GetInstance();
            
            tsslabDigital.Image= process.DIConnectFlag? Properties.Resources.succeed:Properties.Resources.fail;
            tsslabLin.Image=process.PINConnectFlag? Properties.Resources.succeed : Properties.Resources.fail;
            tsslabRegist1.Image = process.AD1ConnectFlag ? Properties.Resources.succeed : Properties.Resources.fail;
            tsslabRegist2.Image = process.AD2ConnectFlag ? Properties.Resources.succeed : Properties.Resources.fail;
            if (process.LinReceiveFlag) {
                tsslLinRevice.Image = Properties.Resources.succeed;
                //process.LinReceiveFlag = false;
            }
            else {
                tsslLinRevice.Image = Properties.Resources.fail;
            }
          
            //  tsslabLin.Image = process.PINConnectFlag ? Properties.Resources.succeed : Properties.Resources.fail;

        }

        private void button2_Click_1(object sender, EventArgs e) {
            WorkProcess process = WorkProcess.GetInstance();
           // process.RestoreLin();
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e) {
            if (dgvRecords.CurrentRow != null) {
                if (MessageBox.Show("确认删除选中行电检记录？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                    string strProductCode = (string)dgvRecords.CurrentRow.Cells["总装条形码"].Value;
                    SqlOperation.ClearTegResult(strProductCode);
                    UpdateDateGridView();
                }
            }
           
        }
        private void InitMp3() {
            Mp3Player mp3Play = new Mp3Player();
            mp3Play.FileName = System.IO.Directory.GetCurrentDirectory() + @"\VoiceRes\3CLabel.mp3";
        }
        private void button3_Click(object sender, EventArgs e) {
            //Mp3Player mp3Play = new Mp3Player() {
            //    FileName = @"E:\MyCode\RiemannProjectMaster\DetectCodeAndCurrent\DetectCodeAndCurrent\bin\Debug\VoiceRes\3CLabel.mp3"
            //};

            // mp3Play.play();
        }

        private void button4_Click(object sender, EventArgs e) {
            WorkProcess p = WorkProcess.GetInstance();
            p.PlayVoice("3CLabel");
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("确认强制放行当前工件？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    SqlOperation.UpdateProductCodeStatuRecord(WorkProcess.GetInstance().strCurrentCode, (int)eStation2_WorkProcess.EndAssembly);
                    InitTEGShow();
                }
            }
            catch (Exception) {
                MessageBox.Show("强制放行失败");
            }
            

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
           
            try
            {
                if (MessageBox.Show("确认删除当前工件电检记录？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    SqlOperation.ClearTegResult(txtProductCode.Text);
                    InitTEGShow();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("删除当前工件电检记录失败");
            }
        }
    }
}
