using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DetectCodeAndCurrent {
    public enum eStateType {
        Finished = 0,
        Waiting,
        Unused
    }



    public partial class UCDetectItem : UserControl {
        public string detectItemName {
            get {
                return (lblItemName.Text);
            }

            set {
                lblItemName.Text = value;
            }

        }
        public UCDetectItem() {
            InitializeComponent();
        }

        public void UpdateItemShowState(int stateType) {
            Image newImage;
            switch (stateType) {
                case (int)eStateType.Finished:
                    newImage = Properties.Resources.Finish;
                    break;
                case (int)eStateType.Waiting:
                    newImage = Properties.Resources.Notification;
                    break;
                default:
                    newImage = null;
                    break;
            }
            if (this.InvokeRequired) {
                Invoke(new MethodInvoker(delegate () {
                    picCurState.Image = newImage;
                }));
            }
            else {
                picCurState.Image = newImage;

            }


        }
    }


}
