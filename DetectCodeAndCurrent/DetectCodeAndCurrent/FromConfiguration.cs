using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DetectCodeAndCurrent {
    public partial class FromConfiguration : Form {
        public FromConfiguration() {
            InitializeComponent();
        }

        private void FromConfiguration_Load(object sender, EventArgs e) {
            UpdateShowProducts();
            UpdateShowPartTypes();
            UpdateShowButtonVoltRange();
        }

        private void UpdateShowButtonVoltRange() {
            try {
                sButtonVoltRanges ranges = SqlOperation.GetButtonVoltRange();
                tbxPhoneMax.Text = ranges.phoneMax.ToString();
                tbxPhoneMin.Text = ranges.phoneMin.ToString();
                tbxSosMax.Text = ranges.sosMax.ToString();
                tbxSosMin.Text = ranges.sosMin.ToString();
                tbxStarMax.Text = ranges.onStartMax.ToString();
                tbxStarMin.Text = ranges.onStartMin.ToString();
            }
            catch (Exception ex) {
                MessageBox.Show("修改范围失败" + ex.Message);
            }
        }

        private void UpdateShowProducts() {
            List<string> productNameList = SqlOperation.GetProductInfosFromSQL();
            cmbProductType.Items.Clear();
            lstProducts.Items.Clear();
            foreach (string productName in productNameList) {
                cmbProductType.Items.Add(productName);
                lstProducts.Items.Add(productName);
            }

        }
        private void UpdateShowPartTypes() {
            List<string> partTypeList = SqlOperation.GetPartTypeFromSQL();
            cmbPartType.Items.Clear();
            cmbParts.Items.Clear();
            foreach (string partType in partTypeList) {
                cmbPartType.Items.Add(partType);
                cmbParts.Items.Add(partType);
            }
        }

        private void dgvAssembleConfig_CurrentCellChanged(object sender, EventArgs e) {
            if (dgvAssembleConfig.CurrentRow != null) {
                if (dgvAssembleConfig.CurrentCell.Value.GetType().Name != "DBNull") {
                    cmbPartType.SelectedItem = dgvAssembleConfig.CurrentRow.Cells["部件类型"].Value.ToString();
                    cmbPartCount.Text = dgvAssembleConfig.CurrentRow.Cells["数量"].Value.ToString();
                    cmbStation.SelectedItem = dgvAssembleConfig.CurrentRow.Cells["工位"].Value.ToString();
                }
                else {
                    cmbPartType.SelectedItem = null;
                    cmbPartCount.Text = null;
                    cmbStation.SelectedItem = null;
                }
            }
        }

        private void cmbProductType_SelectedValueChanged(object sender, EventArgs e) {
            if (cmbProductType.SelectedItem != null)
                UpdateDataControl(cmbProductType.SelectedItem.ToString());
        }

        private void UpdateDataControl(string ProductType) {
            float upper = 0;
            float lower = 0;
            float lightUpper = 0;
            float lightLower = 0;
            float upperCurrent = 0;
            string productCodeId = "";
            BindingSource bs = new BindingSource();
            bs.DataSource = SqlOperation.GetConfigurationFromSQL(ProductType);
            dgvAssembleConfig.DataSource = bs.DataSource;
            bdnAssembleConfig.BindingSource = bs;
            SqlOperation.GetProductConfigCodeFromSQL(ProductType, out productCodeId);
            SqlOperation.GetProductCurrentRange(productCodeId, out upper, out lower ,out upperCurrent);
            SqlOperation.GetProductLightCurrentRange(productCodeId, out lightUpper, out lightLower);
            txtLower.Text = lower.ToString();
            txtUpper.Text = upper.ToString();
            txtLightCurrentLow.Text = lightLower.ToString();
            txtLightCurrentUp.Text = lightUpper.ToString();
            txtUpperCurrent.Text = upperCurrent.ToString();
            
        }

        private void cmbPartType_SelectedIndexChanged(object sender, EventArgs e) {
            if (cmbPartType.SelectedItem != null) {
                cmbPartName.Items.Clear();
                List<string> partTypeNameList = SqlOperation.GetPartFromSQL(cmbPartType.SelectedItem.ToString());
                foreach (string productName in partTypeNameList) {
                    cmbPartName.Items.Add(productName);
                }
                if (dgvAssembleConfig.CurrentRow != null) {
                    cmbPartName.SelectedItem = dgvAssembleConfig.CurrentRow.Cells["部件名"].Value.ToString();
                }

            }

        }

        private void btnConfirm_Click(object sender, EventArgs e) {

            if (dgvAssembleConfig.CurrentRow != null) {
                string productName = cmbProductType.SelectedItem.ToString();
                string productID = "";
                 SqlOperation.GetProductConfigCodeFromSQL(productName,out productID);
                string opartName = dgvAssembleConfig.CurrentRow.Cells["部件名"].Value.ToString();
                string partName = cmbPartName.SelectedItem.ToString();
                int station = Convert.ToInt16(cmbStation.Text);
                int count = Convert.ToInt16(cmbPartCount.Text);
                if (opartName == null || opartName == "") {
                    int changeRowCount = SqlOperation.InsertAssemble(productID, partName, count, station);
                    if (changeRowCount > 0 && cmbProductType.SelectedItem != null)
                        UpdateDataControl(cmbProductType.SelectedItem.ToString());
                }
                else {
                    int changeRowCount = SqlOperation.UpdateAssemble(productID, opartName, partName, count, station);
                    if (changeRowCount > 0 && cmbProductType.SelectedItem != null)
                        UpdateDataControl(cmbProductType.SelectedItem.ToString());
                }
            }


        }

        private void btnRevert_Click(object sender, EventArgs e) {
            string productCodeId = "";
            try {
                float lower = Convert.ToSingle(txtLower.Text);
                float upper = Convert.ToSingle(txtUpper.Text);
                float lightLower = Convert.ToSingle(txtLightCurrentLow.Text);
                float lightUpper = Convert.ToSingle(txtLightCurrentUp.Text);
                float upperCurrent = Convert.ToSingle(txtUpperCurrent.Text);
                if (lower > upper || lightLower > lightUpper) throw new Exception("参数输入范围错误，请检查后重试！");
                if (cmbProductType.SelectedItem != null) {
                    SqlOperation.GetProductConfigCodeFromSQL(cmbProductType.SelectedItem.ToString(), out productCodeId);
                    SqlOperation.SetCurrentValue(productCodeId, upper, lower, upperCurrent, lightUpper, lightLower);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("修改失败！"+ex.Message);
            }
          
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e) {
              if (cmbPartName.SelectedItem != null && cmbProductType.SelectedItem != null) {
                //    if (MessageBox.Show("确认删除选中行？") == DialogResult.OK) {
                string productID = "";
                string productName = cmbProductType.SelectedItem.ToString();
                string partName = cmbPartName.SelectedItem.ToString();
                SqlOperation.GetProductConfigCodeFromSQL(productName, out productID);
                SqlOperation.DeleteAssemble(productID, partName);
         //   }

              }

        }
        private void UpdateShowParts() {
            if (cmbParts.SelectedItem != null) {
                lstParts.Items.Clear();
                List<string> partTypeNameList = SqlOperation.GetPartFromSQL(cmbParts.SelectedItem.ToString());
                foreach (string productName in partTypeNameList) {
                    lstParts.Items.Add(productName);
                }
            }

        }
        private void lstParts_SelectedValueChanged(object sender, EventArgs e) {
            if (lstParts.SelectedItem != null) {
                txtPartName.Text = (string)lstParts.SelectedItem;
                string strPartNum = "";
                SqlOperation.GetPartConfigCode(txtPartName.Text,out strPartNum);
                txtPartCode.Text = strPartNum;
            }
            else {
                txtPartName.Text = "";
                txtPartCode.Text = "";
            }
        }

        private void lstProducts_SelectedValueChanged(object sender, EventArgs e) {
            if (lstProducts.SelectedItem != null) {
                txtProductName.Text = (string)lstProducts.SelectedItem;
                string productNum = "";
                SqlOperation.GetProductConfigCodeFromSQL(txtProductName.Text, out productNum);
                txtProductCode.Text = productNum;
            }
            else {
                txtProductName.Text = "";
                txtProductCode.Text = "";

            }
        }

        private void btnProductAdd_Click(object sender, EventArgs e) {
            string productName = txtProductName.Text.Trim(' ');
            string productNum = txtProductCode.Text.Trim(' ');
            string strTipMessage = "确认增加产品：" + productName + "，产品总装码：" + productNum +"?";
            if (MessageBox.Show(strTipMessage, "提示", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                SqlOperation.InsertProductInfo(productName, productNum);
                UpdateShowProducts();
            }
        }

        private void btnProductRevert_Click(object sender, EventArgs e) {
            if (lstProducts.SelectedItem != null) {
                string productName = (string)lstProducts.SelectedItem;
                string productNum = txtProductCode.Text.Trim(' ');
                string productOldNum = "";
                SqlOperation.GetProductConfigCodeFromSQL(productName, out productOldNum);
                SqlOperation.UpdateProductInfo(productName,productNum,productOldNum);
                UpdateShowProducts();
            }
        }

        private void btnProductDelete_Click(object sender, EventArgs e) {
            if (lstProducts.SelectedItem != null) {
                string productName = (string)lstProducts.SelectedItem;
                string productNum = txtProductCode.Text.Trim(' ');
                SqlOperation.DeleteProductInfo(productNum);
                UpdateShowProducts();
            }
        }

        private void btnPartsAdd_Click(object sender, EventArgs e) {
            string partName = txtPartName.Text.Trim(' ');
            string partNum = txtPartCode.Text.Trim(' ');
            string partTypeName = (string)cmbParts.SelectedItem;
            string partType = "";
            SqlOperation.GetPartTypeForPartName(partTypeName, out partType);
            string strTipMessage = "确认增加零件：" + partName + "，零件号：" + partNum + "?";
            if (MessageBox.Show(strTipMessage, "提示", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                SqlOperation.InsertPartsInfo(partName, partNum,partType);
                UpdateShowParts();
            }
        }

        private void btnPartRevert_Click(object sender, EventArgs e) {
            if (lstParts.SelectedItem != null) {
                string partName = (string)lstParts.SelectedItem;
                string partNum = txtPartCode.Text.Trim(' ');
                string partTypeName = (string)cmbParts.SelectedItem;
                string partOldNum = "";
                string partType = "";
                SqlOperation.GetPartTypeForPartName(partTypeName, out partType);
                SqlOperation.GetPartConfigCode(partName, out partOldNum);
                SqlOperation.UpdatePartsInfo(partName, partNum, partOldNum, partType);
                UpdateShowParts();
            }
        }

        private void btnPartDelete_Click(object sender, EventArgs e) {
            if (lstParts.SelectedItem != null) {
                string partName = (string)lstProducts.SelectedItem;
                string partNum = txtPartCode.Text.Trim(' ');
                string partTypeName = (string)cmbParts.SelectedItem;
                string partType= "";
                SqlOperation.GetPartTypeForPartName(partTypeName, out partType);
                SqlOperation.DeletePartsInfo(partNum, partType);
                UpdateShowParts();
            }
        }

        private void cmbParts_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateShowParts();
        }

        private void button1_Click(object sender, EventArgs e) {
            sButtonVoltRanges ranges = new sButtonVoltRanges();
            ranges.onStartMax =Convert.ToDouble( tbxStarMax.Text);
            ranges.onStartMin = Convert.ToDouble(tbxStarMin.Text);
            ranges.phoneMax = Convert.ToDouble(tbxPhoneMax.Text);
            ranges.phoneMin = Convert.ToDouble(tbxPhoneMin.Text);
            ranges.sosMax = Convert.ToDouble(tbxSosMax.Text);
            ranges.sosMin = Convert.ToDouble(tbxSosMin.Text);
            SqlOperation.UpdateButtonVoltRange(ranges);
        }
    }
}
