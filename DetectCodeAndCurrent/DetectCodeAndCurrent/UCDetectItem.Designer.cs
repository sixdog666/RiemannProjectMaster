namespace DetectCodeAndCurrent {
    partial class UCDetectItem {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent() {
            this.lblItemName = new System.Windows.Forms.Label();
            this.picCurState = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picCurState)).BeginInit();
            this.SuspendLayout();
            // 
            // lblItemName
            // 
            this.lblItemName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblItemName.AutoSize = true;
            this.lblItemName.Font = new System.Drawing.Font("黑体", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblItemName.Location = new System.Drawing.Point(11, 39);
            this.lblItemName.Name = "lblItemName";
            this.lblItemName.Size = new System.Drawing.Size(163, 30);
            this.lblItemName.TabIndex = 0;
            this.lblItemName.Text = "检测项名称";
            // 
            // picCurState
            // 
            this.picCurState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picCurState.Cursor = System.Windows.Forms.Cursors.Default;
            this.picCurState.Location = new System.Drawing.Point(262, 3);
            this.picCurState.Name = "picCurState";
            this.picCurState.Size = new System.Drawing.Size(111, 97);
            this.picCurState.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCurState.TabIndex = 1;
            this.picCurState.TabStop = false;
            // 
            // UCDetectItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.picCurState);
            this.Controls.Add(this.lblItemName);
            this.Name = "UCDetectItem";
            this.Size = new System.Drawing.Size(376, 106);
            ((System.ComponentModel.ISupportInitialize)(this.picCurState)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblItemName;
        private System.Windows.Forms.PictureBox picCurState;
    }
}
