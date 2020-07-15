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
    public partial class UserConfrimFrom : Form {
        string passWord = "jixing";
        bool result = false;
        public bool checkResult {
            get {
                return result;
            }
        }
        
        public UserConfrimFrom() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            Confrim();
        }

        private void tbPassWord_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                Confrim();
            }
        }
        private void Confrim() {
            if (tbPassWord.Text == passWord) {
                result = true;
                this.Close();
            }
            else {
                MessageBox.Show("密码错误！");
            }
        }
    }
}
