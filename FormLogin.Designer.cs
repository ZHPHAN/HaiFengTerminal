namespace hf_terminal
{
	partial class FormLogin
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.kryptonTextBoxInvestor = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
			this.kryptonLabel1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
			this.kryptonComboBoxServer = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
			this.kryptonButtonLogin = new ComponentFactory.Krypton.Toolkit.KryptonButton();
			this.kryptonButtonExit = new ComponentFactory.Krypton.Toolkit.KryptonButton();
			this.kryptonLabel2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
			this.kryptonLabel3 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
			this.kryptonTextBoxPassword = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabelInfo = new System.Windows.Forms.ToolStripStatusLabel();
			this.kryptonManager1 = new ComponentFactory.Krypton.Toolkit.KryptonManager(this.components);
			((System.ComponentModel.ISupportInitialize)(this.kryptonComboBoxServer)).BeginInit();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// kryptonTextBoxInvestor
			// 
			this.kryptonTextBoxInvestor.Location = new System.Drawing.Point(166, 134);
			this.kryptonTextBoxInvestor.Name = "kryptonTextBoxInvestor";
			this.kryptonTextBoxInvestor.Size = new System.Drawing.Size(100, 20);
			this.kryptonTextBoxInvestor.TabIndex = 1;
			// 
			// kryptonLabel1
			// 
			this.kryptonLabel1.Location = new System.Drawing.Point(102, 107);
			this.kryptonLabel1.Name = "kryptonLabel1";
			this.kryptonLabel1.Size = new System.Drawing.Size(48, 20);
			this.kryptonLabel1.TabIndex = 1;
			this.kryptonLabel1.Values.Text = "服务器";
			// 
			// kryptonComboBoxServer
			// 
			this.kryptonComboBoxServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.kryptonComboBoxServer.DropDownWidth = 158;
			this.kryptonComboBoxServer.Location = new System.Drawing.Point(166, 107);
			this.kryptonComboBoxServer.Name = "kryptonComboBoxServer";
			this.kryptonComboBoxServer.Size = new System.Drawing.Size(158, 21);
			this.kryptonComboBoxServer.TabIndex = 0;
			// 
			// kryptonButtonLogin
			// 
			this.kryptonButtonLogin.Location = new System.Drawing.Point(102, 204);
			this.kryptonButtonLogin.Name = "kryptonButtonLogin";
			this.kryptonButtonLogin.Size = new System.Drawing.Size(90, 25);
			this.kryptonButtonLogin.TabIndex = 3;
			this.kryptonButtonLogin.Values.Text = "登  录";
			this.kryptonButtonLogin.Click += new System.EventHandler(this.kryptonButtonLogin_Click);
			// 
			// kryptonButtonExit
			// 
			this.kryptonButtonExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.kryptonButtonExit.Location = new System.Drawing.Point(234, 204);
			this.kryptonButtonExit.Name = "kryptonButtonExit";
			this.kryptonButtonExit.Size = new System.Drawing.Size(90, 25);
			this.kryptonButtonExit.TabIndex = 4;
			this.kryptonButtonExit.Values.Text = "退  出";
			this.kryptonButtonExit.Click += new System.EventHandler(this.kryptonButtonExit_Click);
			// 
			// kryptonLabel2
			// 
			this.kryptonLabel2.Location = new System.Drawing.Point(102, 134);
			this.kryptonLabel2.Name = "kryptonLabel2";
			this.kryptonLabel2.Size = new System.Drawing.Size(48, 20);
			this.kryptonLabel2.TabIndex = 1;
			this.kryptonLabel2.Values.Text = "帐　号";
			// 
			// kryptonLabel3
			// 
			this.kryptonLabel3.Location = new System.Drawing.Point(102, 160);
			this.kryptonLabel3.Name = "kryptonLabel3";
			this.kryptonLabel3.Size = new System.Drawing.Size(48, 20);
			this.kryptonLabel3.TabIndex = 1;
			this.kryptonLabel3.Values.Text = "密　码";
			// 
			// kryptonTextBoxPassword
			// 
			this.kryptonTextBoxPassword.Location = new System.Drawing.Point(166, 160);
			this.kryptonTextBoxPassword.Name = "kryptonTextBoxPassword";
			this.kryptonTextBoxPassword.PasswordChar = '*';
			this.kryptonTextBoxPassword.Size = new System.Drawing.Size(100, 20);
			this.kryptonTextBoxPassword.TabIndex = 2;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelInfo});
			this.statusStrip1.Location = new System.Drawing.Point(0, 266);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(340, 22);
			this.statusStrip1.TabIndex = 5;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabelInfo
			// 
			this.toolStripStatusLabelInfo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelInfo.Name = "toolStripStatusLabelInfo";
			this.toolStripStatusLabelInfo.Size = new System.Drawing.Size(44, 17);
			this.toolStripStatusLabelInfo.Text = "未登录";
			this.toolStripStatusLabelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// kryptonManager1
			// 
			this.kryptonManager1.GlobalPaletteMode = ComponentFactory.Krypton.Toolkit.PaletteModeManager.Office2010Black;
			// 
			// FormLogin
			// 
			this.AcceptButton = this.kryptonButtonLogin;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.kryptonButtonExit;
			this.ClientSize = new System.Drawing.Size(340, 288);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.kryptonButtonExit);
			this.Controls.Add(this.kryptonButtonLogin);
			this.Controls.Add(this.kryptonComboBoxServer);
			this.Controls.Add(this.kryptonLabel3);
			this.Controls.Add(this.kryptonLabel2);
			this.Controls.Add(this.kryptonLabel1);
			this.Controls.Add(this.kryptonTextBoxPassword);
			this.Controls.Add(this.kryptonTextBoxInvestor);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormLogin";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "登录";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLogin_FormClosed);
			this.Load += new System.EventHandler(this.FormLogin_Load);
			((System.ComponentModel.ISupportInitialize)(this.kryptonComboBoxServer)).EndInit();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ComponentFactory.Krypton.Toolkit.KryptonTextBox kryptonTextBoxInvestor;
		private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel1;
		private ComponentFactory.Krypton.Toolkit.KryptonComboBox kryptonComboBoxServer;
		private ComponentFactory.Krypton.Toolkit.KryptonButton kryptonButtonLogin;
		private ComponentFactory.Krypton.Toolkit.KryptonButton kryptonButtonExit;
		private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel2;
		private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel3;
		private ComponentFactory.Krypton.Toolkit.KryptonTextBox kryptonTextBoxPassword;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelInfo;
		private ComponentFactory.Krypton.Toolkit.KryptonManager kryptonManager1;
	}
}