using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Quote2015;
using Trade2015;

namespace hf_terminal
{
	public partial class FormLogin : KryptonForm
	{
		#region 配置
		private readonly Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
		private string GetConfig(string pKey)
		{
			return _config.AppSettings.Settings[pKey] == null ? string.Empty : _config.AppSettings.Settings[pKey].Value;
		}
		private void SetConfig(string pKey, string pValue)
		{
			if (_config.AppSettings.Settings[pKey] == null)
				_config.AppSettings.Settings.Add(pKey, pValue);
			else
				_config.AppSettings.Settings[pKey].Value = pValue;
			_config.Save(ConfigurationSaveMode.Full);
			ConfigurationManager.RefreshSection("appSettings");
		}
		#endregion

		public FormLogin()
		{
			InitializeComponent();
		}
		readonly DataTable _dt = new DataTable("ServerConfig");
		internal Trade trade;
		internal Quote quote;
		internal string Server;

		private void FormLogin_Load(object sender, EventArgs e)
		{
			_dt.Columns.Add("txt");
			_dt.Columns.Add("val");
			_dt.PrimaryKey = new[] { _dt.Columns[0] };
			if (File.Exists("server.txt"))
			{
				foreach (string line in File.ReadAllLines("server.txt", Encoding.GetEncoding("GB2312")))
				{
					if (string.IsNullOrEmpty(line))
						continue;
					_dt.Rows.Add(line.Split(','));
				}
			}
			else
			{
				//broker|trade|quote
				_dt.Rows.Add("模拟", "ctp|1017|tcp://ctpmn1-front1.citicsf.com:51205|tcp://ctpmn1-front1.citicsf.com:51213");
				_dt.Rows.Add("股指仿真", "ctp|66666|tcp://ctpfz1-front1.citicsf.com:51205|tcp://ctpfz1-front1.citicsf.com:51213");
				_dt.Rows.Add("飞创", "xSpeed|galaxy|tcp://203.187.171.250:10910|tcp://203.187.171.250:10915");
				_dt.Rows.Add("飞马(公网)", "femas|0001|tcp://116.228.53.149:6666|tcp://116.228.53.149:6888");
				_dt.Rows.Add("飞马", "femas|2051|tcp://116.228.53.144:6666|tcp://116.228.53.144:8888");
			}

			foreach (DataRow dr in _dt.Rows)
				this.kryptonComboBoxServer.Items.Add(dr["txt"]);


			if (!string.IsNullOrEmpty(GetConfig("server")))
				this.kryptonComboBoxServer.Text = GetConfig("server");
			if (!string.IsNullOrEmpty(GetConfig("investor")))
			{
				this.kryptonTextBoxInvestor.Text = GetConfig("investor");
				this.ActiveControl = this.kryptonTextBoxPassword;
			}

			if (!string.IsNullOrEmpty(GetConfig("GlobalPaletteMode")))
				this.kryptonManager1.GlobalPaletteMode = (PaletteModeManager)Enum.Parse(typeof(PaletteModeManager), GetConfig("GlobalPaletteMode"));	//更新样式
		}

		private void FormLogin_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		private void kryptonButtonLogin_Click(object sender, EventArgs e)
		{
			ShowMsg("登录中...");
			string front = (string)_dt.Rows.Find(this.kryptonComboBoxServer.Text)[1];
			string[] fs = front.Split('|');

			if (!string.IsNullOrEmpty(fs[3]))
			{
				quote = new Quote(string.Format("{0}_Quote_proxy.dll", fs[0]))
				{
					Broker = fs[1],
					Server = fs[3],
					Investor = this.kryptonTextBoxInvestor.Text,
					Password = this.kryptonTextBoxPassword.Text,
				};
				quote.OnFrontConnected += quote_OnFrontConnected;
				quote.OnRspUserLogin += quote_OnRspUserLogin;
			}

			trade = new Trade(string.Format("{0}_Trade_proxy.dll", fs[0]))
			{
				Server = fs[2],
				Investor = this.kryptonTextBoxInvestor.Text,
				Password = this.kryptonTextBoxPassword.Text,
				Broker = fs[1],
			};
			trade.OnFrontConnected += trade_OnFrontConnected;
			trade.OnRspUserLogin += trade_OnRspUserLogin;
			trade.OnRtnExchangeStatus += trade_OnRtnExchangeStatus;
			trade.ReqConnect();
		}

		void trade_OnRtnExchangeStatus(object sender, StatusEventArgs e)
		{
			ShowMsg(e.Exchange + "=>" + e.Status);
		}

		void trade_OnRspUserLogin(object sender, Trade2015.IntEventArgs e)
		{
			if (e.Value == 0)
			{
				ShowMsg("登录成功");
				Thread.Sleep(1500);
				//交易登录成功后,登录行情
				if (quote == null)
					LoginSuccess();
				else
					quote.ReqConnect();
			}
			else
			{
				ShowMsg("登录错误");
				trade.ReqUserLogout();
				trade = null;
				quote = null;
			}
		}

		void trade_OnFrontConnected(object sender, EventArgs e)
		{
			((Trade)sender).ReqUserLogin();
		}

		void quote_OnRspUserLogin(object sender, Quote2015.IntEventArgs e)
		{
			LoginSuccess();
		}

		void quote_OnFrontConnected(object sender, EventArgs e)
		{
			((Quote)sender).ReqUserLogin();
		}

		//登录成功
		void LoginSuccess()
		{
			trade.OnFrontConnected -= trade_OnFrontConnected;
			trade.OnRspUserLogin -= trade_OnRspUserLogin;
			trade.OnRtnExchangeStatus -= trade_OnRtnExchangeStatus;
			if (quote != null)
			{
				quote.OnFrontConnected -= quote_OnFrontConnected;
				quote.OnRspUserLogin -= quote_OnRspUserLogin;
			}
			this.Invoke(new Action(() =>
			{
				Server = this.kryptonComboBoxServer.Text;
				this.DialogResult = DialogResult.OK;
			}));
		}

		void ShowMsg(string pMsg)
		{
			this.Invoke(new Action(() =>
			{
				var parentForm = (Form1)this.Owner;
				if (parentForm != null) parentForm.ShowMsg(pMsg);
				this.toolStripStatusLabelInfo.Text = DateTime.Now.ToString("HH:mm:ss") + "|" + pMsg;
			}));
		}

		private void kryptonButtonExit_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}
	}
}
