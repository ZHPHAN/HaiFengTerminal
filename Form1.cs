using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Quote2015;
using Trade2015;
using Timer = System.Windows.Forms.Timer;
using System.IO;
using System.Reflection;

namespace hf_terminal
{
	public partial class Form1 : KryptonForm
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

		public Form1()
		{
			this.Hide();
			InitializeComponent();
		}


		private Trade _t;
		private Quote _q;
		private readonly ConcurrentQueue<int> _queueOrderFresh = new ConcurrentQueue<int>();
		private readonly ConcurrentQueue<string> _queueTradeFresh = new ConcurrentQueue<string>();
		private readonly ConcurrentQueue<string> _queuePositionFresh = new ConcurrentQueue<string>();

		private readonly DataTable _dtTick = new DataTable();
		private readonly DataTable _dtOrder = new DataTable();
		private readonly DataTable _dtTrade = new DataTable();
		private readonly DataTable _dtPosition = new DataTable();

		readonly string[] _confirmNames = { "Order", "Cancel", "CancelAll", "DblClose", "DblCancel" };
		readonly string[] _confirmTexts = { "委托确认", "撤单确认", "全撤确认", "双击平仓确认", "双击撤单确认" };

		private readonly Timer _timer = new Timer
		{
			Interval = 1200,
		};

		private void Form1_Load(object sender, EventArgs e)
		{
			FormLogin fLogin = new FormLogin();
			if (fLogin.ShowDialog(this) == DialogResult.Cancel)
			{
				this.Close();
				return;
			}

			_t = fLogin.trade;
			_q = fLogin.quote;
			_t.OnRtnCancel += trade_OnRtnCancel;
			_t.OnRtnError += trade_OnRtnError;
			_t.OnRtnExchangeStatus += trade_OnRtnExchangeStatus;
			_t.OnRtnNotice += trade_OnRtnNotice;
			_t.OnRtnOrder += trade_OnRtnOrder;
			_t.OnRtnTrade += trade_OnRtnTrade;
			if (_q != null)
				_q.OnRtnTick += quote_OnRtnTick;

			SetConfig("server", fLogin.Server);
			SetConfig("investor", fLogin.trade.Investor);

			fLogin.Close();

			this.Text += string.Format("({0}@{1})", GetConfig("investor"), GetConfig("server"));

			//初始化表格
			InitView(this.kryptonDataGridViewTick, this.kryptonDataGridViewOrder, this.kryptonDataGridViewTrade, this.kryptonDataGridViewPosition);
			this.kryptonDataGridViewOrder.CellDoubleClick += kryptonDataGridViewOrder_CellDoubleClick;
			this.kryptonDataGridViewPosition.CellDoubleClick += kryptonDataGridViewPosition_CellDoubleClick;
			this.kryptonDataGridViewOrder.CellFormatting += kryptonDataGridView_CellFormatting;
			this.kryptonDataGridViewTrade.CellFormatting += kryptonDataGridView_CellFormatting;
			this.kryptonDataGridViewPosition.CellFormatting += kryptonDataGridView_CellFormatting;


			//样式
			foreach (var v in Enum.GetNames(typeof(PaletteModeManager)))
			{
				var item = this.ToolStripMenuItemStyle.DropDownItems.Add(v);
				item.Click += (obj, arg) =>
				{
					this.kryptonManager1.GlobalPaletteMode = (PaletteModeManager)Enum.Parse(typeof(PaletteModeManager), ((ToolStripDropDownItem)obj).Text);
				};
			}

			//创建确认选项
			for (int i = 0; i < _confirmNames.Length; ++i)
			{
				ToolStripMenuItem item = new ToolStripMenuItem
				{
					Name = _confirmNames[i],
					Text = _confirmTexts[i],
					Checked = true,
					CheckOnClick = true,
				};
				this.ToolStripMenuItemOption.DropDownItems.Add(item);
			}
			this.ToolStripMenuItemOption.DropDownItems.Add("-");//增加隔断

			//加载合约列表
			this.kryptonComboBoxInstrument.Items.AddRange(_t.DicInstrumentField.Keys.ToArray());

			//加载插件
			if (Directory.Exists(Application.StartupPath + "\\plugin"))
				foreach (var file in new DirectoryInfo(Application.StartupPath + "\\plugin").GetFiles("*.dll"))
				{
					Assembly ass = Assembly.LoadFile(file.FullName);
					var t = ass.GetTypes().FirstOrDefault(n => n.BaseType == typeof(UserControl));
					if (t != null)
					{
						this.tabControl1.TabPages.Add(t.FullName, t.Name);
						TabPage tp = this.tabControl1.TabPages[t.FullName];
						var uc = (UserControl)Activator.CreateInstance(t, _t, _q);
						uc.Dock = DockStyle.Fill;
						tp.Controls.Add(uc);
					}
				}

			_timer.Tick += _timer_Tick;
			_timer.Start();


			//恢复配置
			if (!string.IsNullOrEmpty(GetConfig("WindowState")))
				this.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), GetConfig("WindowState"));
			if (this.WindowState != FormWindowState.Maximized)
			{
				if (!string.IsNullOrEmpty(GetConfig("Left")))
					this.Left = int.Parse(GetConfig("Left"));
				if (!string.IsNullOrEmpty(GetConfig("Top")))
					this.Top = int.Parse(GetConfig("Top"));
				if (!string.IsNullOrEmpty(GetConfig("Height")))
					this.Height = int.Parse(GetConfig("Height"));
				if (!string.IsNullOrEmpty(GetConfig("Width")))
					this.Width = int.Parse(GetConfig("Width"));
			}
			if (!string.IsNullOrEmpty(GetConfig("GlobalPaletteMode")))
				this.kryptonManager1.GlobalPaletteMode = (PaletteModeManager)Enum.Parse(typeof(PaletteModeManager), GetConfig("GlobalPaletteMode"));	//更新样式

			//选项
			foreach (var v in _confirmNames)
			{
				((ToolStripMenuItem)this.ToolStripMenuItemOption.DropDownItems[v]).Checked = string.IsNullOrEmpty(GetConfig(v)) || bool.Parse(GetConfig(v));
			}
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			SetConfig("Left", this.Left.ToString(CultureInfo.InvariantCulture));
			SetConfig("Top", this.Top.ToString(CultureInfo.InvariantCulture));
			SetConfig("Height", this.Height.ToString(CultureInfo.InvariantCulture));
			SetConfig("Width", this.Width.ToString(CultureInfo.InvariantCulture));
			SetConfig("WindowState", this.WindowState.ToString());
			SetConfig("GlobalPaletteMode", this.kryptonManager1.GlobalPaletteMode.ToString());
			//保存选项
			if (this.ToolStripMenuItemOption.DropDownItems.Count > 0)
				foreach (var v in _confirmNames)
				{
					SetConfig(v, ((ToolStripMenuItem)this.ToolStripMenuItemOption.DropDownItems[v]).Checked.ToString());
				}

			if (_t != null && _t.IsLogin)
			{
				_t.ReqUserLogout();
			}
			if (_q != null && _q.IsLogin)
			{
				foreach (var v in _q.DicTick.Keys)
				{
					_q.ReqUnSubscribeMarketData(v);
				}
				_q.ReqUserLogout();
			}
		}

		void InitView(params KryptonDataGridView[] pViews)
		{
			MarketData o = new MarketData();
			string[] names =
			{
				"AvgPrice","Custom", "Direction","Hedge","InsertTime","InstrumentID","IsLocal","LimitPrice","Offset","OrderID","Status","TradeTime","TradeVolume","Volume","VolumeLeft",
				"Price","TradeID","TradingDay","ExchangeID",
				"Position","TdPosition","YdPosition",
				"AskPrice","AskVolume","AveragePrice","BidPrice","BidVolume","LastPrice","LowerLimitPrice","OpenInterest","UpdateMillisec","UpdateTime","UpperLimitPrice"
			};
			string[] txts =
			{
				"成交均价","自定义","买卖","投保","委托时间","合约","本地单","报价","开平","编号","状态","成交时间","成交数量","数量","剩余量",
				"价格","编号","交易日","交易所",
				"总持仓","今仓","昨仓",
				"卖价","卖量","均价","买价","买量","最新价","跌板价","持仓量","毫秒","更新时间","涨板价"
			};
			foreach (var view in pViews)
			{
				Type t = null;
				DataTable dt = null;
				string keyCulumn = string.Empty;
				switch (view.Name)
				{
					case "kryptonDataGridViewTick":
						t = typeof(MarketData);
						dt = _dtTick;
						keyCulumn = "InstrumentID";
						break;
					case "kryptonDataGridViewOrder":
						t = typeof(OrderField);
						dt = _dtOrder;
						keyCulumn = "OrderID";
						break;
					case "kryptonDataGridViewTrade":
						t = typeof(TradeField);
						dt = _dtTrade;
						keyCulumn = "TradeID";
						break;
					case "kryptonDataGridViewPosition":
						t = typeof(PositionField);
						dt = _dtPosition;
						keyCulumn = "InstrumentID,Direction";
						break;
				}
				if (t == null)
					return;

				foreach (var v in t.GetFields())
				{
					dt.Columns.Add(v.Name, v.FieldType);
				}

				dt.PrimaryKey = keyCulumn.Split(',').Select(v => dt.Columns[v]).ToArray();

				view.DataSource = dt;
				//列格式
				foreach (var v in t.GetFields())
				{
					//表头
					DataGridViewColumn col = view.Columns[v.Name];
					if (col == null)
						continue;
					int idx = names.ToList().IndexOf(v.Name);
					if (idx >= 0)
					{
						col.HeaderText = txts[idx];
					}


					//格式化
					if (v.FieldType == typeof(double))
					{
						col.DefaultCellStyle.Format = "N2";
						col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
					}
					else if (v.FieldType == typeof(int))
					{
						col.DefaultCellStyle.Format = "N0";
						col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
					}
					else if (v.FieldType.IsEnum)
					{
						col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
					}
					else if (v.Name == "ExchangeID")
						col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				}

				view.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
				view.Sort(view.Columns[keyCulumn.Split(',')[0]], ListSortDirection.Ascending);
			}
		}

		//显示消息提醒
		internal void ShowMsg(string pMsg)
		{
			this.BeginInvoke(new Action(() =>
			{
				if (this.ActiveControl == null || this.ActiveControl == this.kryptonComboBoxInfo)
					this.ActiveControl = this.kryptonComboBoxInstrument;
				Control pre = this.ActiveControl;
				this.kryptonComboBoxInfo.Items.Insert(0, string.Format(@"{0:hh\:mm\:ss}  {1}", DateTime.Now.TimeOfDay, pMsg));
				this.kryptonComboBoxInfo.SelectedIndex = 0;
				this.ActiveControl = pre;
			}));
			if (!_timer.Enabled)
				return;

			new Thread(() =>
			{
				int cnt = this.kryptonComboBoxInfo.Items.Count;
				this.BeginInvoke(new Action(() => this.kryptonComboBoxInfo.StateActive.ComboBox.Content.Color1 = Color.Red));
				Thread.Sleep(1000);
				if (!_timer.Enabled)
					return;
				this.BeginInvoke(new Action(() => this.kryptonComboBoxInfo.StateActive.ComboBox.Content.Color1 = Color.LawnGreen));
				Thread.Sleep(1000);
				if (!_timer.Enabled)
					return;
				this.BeginInvoke(new Action(() => this.kryptonComboBoxInfo.StateActive.ComboBox.Content.Color1 = Color.Red));
				Thread.Sleep(1000);
				if (!_timer.Enabled)
					return;
				this.BeginInvoke(new Action(() => this.kryptonComboBoxInfo.StateActive.ComboBox.Content.Color1 = Color.LawnGreen));
				Thread.Sleep(1000);
				if (!_timer.Enabled)
					return;
				this.BeginInvoke(new Action(() => this.kryptonComboBoxInfo.StateActive.ComboBox.Content.Color1 = Color.Red));
				Thread.Sleep(1000);
				if (!_timer.Enabled)
					return;
				if (cnt == this.kryptonComboBoxInfo.Items.Count)
					this.BeginInvoke(new Action(() => this.kryptonComboBoxInfo.StateActive.ComboBox.Content.Color1 = Color.FromName("0")));
			}).Start();
		}

		//界面刷新
		void _timer_Tick(object sender, EventArgs e)
		{
			//刷新价格
			if (_q != null)
			{
				MarketData tick;
				if (_q.DicTick.TryGetValue(this.kryptonComboBoxInstrument.Text, out tick))
				{
					if (this.kryptonLabel5.Text == @"跟盘")
						this.kryptonNumericUpDownPrice.Value = (decimal)tick.LastPrice;
				}
			}

			//刷新委托
			if (this.kryptonDataGridViewOrder.RowCount == 0)
			{
				foreach (var v in _t.DicOrderField)
				{
					var row = _dtOrder.Rows.Find(v.Key);
					if (row == null)
					{
						row = _dtOrder.NewRow();
						foreach (var fi in typeof(OrderField).GetFields())
							row[fi.Name] = fi.GetValue(v.Value);
						_dtOrder.Rows.Add(row);
					}
				}
			}

			int orderID;
			while (_queueOrderFresh.TryDequeue(out orderID))
			{
				OrderField of;
				if (_t.DicOrderField.TryGetValue(orderID, out of))
				{
					var row = _dtOrder.Rows.Find(orderID);
					if (row == null)
					{
						row = _dtOrder.NewRow();
						foreach (var fi in typeof(OrderField).GetFields())
							row[fi.Name] = fi.GetValue(of);
						_dtOrder.Rows.Add(row);
					}
					else
					{
						foreach (var v in typeof(OrderField).GetFields())
							row[v.Name] = v.GetValue(of);
					}
				}
			}

			//刷新成交
			foreach (var v in _t.DicTradeField)
			{
				var row = _dtTrade.Rows.Find(v.Key);
				if (row == null)
				{
					row = _dtTrade.NewRow();
					foreach (var fi in typeof(TradeField).GetFields())
						row[fi.Name] = fi.GetValue(v.Value);
					_dtTrade.Rows.Add(row);
				}
			}

			//刷新持仓
			foreach (var v in _t.DicPositionField)
			{
				var row = _dtPosition.Rows.Find(new object[] { v.Value.InstrumentID, v.Value.Direction });
				if (row == null)
				{
					row = _dtPosition.NewRow();
					foreach (var fi in typeof(PositionField).GetFields())
						row[fi.Name] = fi.GetValue(v.Value);
					_dtPosition.Rows.Add(row);
				}
				else
					foreach (var fi in typeof(PositionField).GetFields())
						row[fi.Name] = fi.GetValue(v.Value);
			}

			//刷新权益
			this.toolStripLabelAvaliable.Text = _t.TradingAccount.Available.ToString("N1");
			this.toolStripLabelFund.Text = _t.TradingAccount.Fund.ToString("N1");

			//刷新行情
			if (_q != null)
				foreach (var v in _q.DicTick)
				{
					var row = _dtTick.Rows.Find(v.Key);
					if (row == null)
					{
						row = _dtTick.NewRow();
						foreach (var fi in typeof(MarketData).GetFields())
							row[fi.Name] = fi.GetValue(v.Value);
						_dtTick.Rows.Add(row);
					}
					else
					{
						foreach (var fi in typeof(MarketData).GetFields())
							row[fi.Name] = fi.GetValue(v.Value);
					}
				}
		}

		//合约有变化,更新最小变动
		private void kryptonComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_q != null && _q.IsLogin)
			{
				_q.ReqSubscribeMarketData(this.kryptonComboBoxInstrument.Text);
			}
			InstrumentField instField;
			if (_t.DicInstrumentField.TryGetValue(this.kryptonComboBoxInstrument.Text, out instField))
			{
				string pt = instField.PriceTick.ToString(CultureInfo.InvariantCulture);
				this.kryptonNumericUpDownPrice.DecimalPlaces = pt.IndexOf('.') < 0 ? 0 : (pt.Length - pt.IndexOf('.') - 1);
				this.kryptonNumericUpDownPrice.Increment = (decimal)instField.PriceTick;
			}
		}

		//选择合约前加载合约列表
		private void kryptonComboBoxInstrument_Enter(object sender, EventArgs e)
		{
			if (_t == null)
				return;
			if (this.kryptonComboBoxInstrument.Items.Count == 0)
				this.kryptonComboBoxInstrument.Items.AddRange(_t.DicInstrumentField.Keys.ToArray());
		}

		//格式化表格
		void kryptonDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}
			string[] keys = { "Buy", "Sell", "Open", "Close", "CloseToday", "Speculation", "Arbitrage", "Hedge", "Normal", "Canceled", "Partial", "Filled" };
			string[] values = { "  买", "卖  ", "开仓", "平仓", "平今", "投机", "套利", "套保", "委托", "已撤单", "部成", "全成" };

			DataGridViewCell cell = ((KryptonDataGridView)sender)[e.ColumnIndex, e.RowIndex];
			if (cell.ValueType.IsEnum)
			{
				string val = Enum.GetName(cell.ValueType, e.Value);
				int idx = keys.ToList().IndexOf(val);
				if (idx >= 0)
				{
					e.Value = values[idx];
					switch (values[idx])
					{
						case "  买":
							cell.Style.ForeColor = Color.Red;
							cell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
							break;
						case "卖  ":
							cell.Style.ForeColor = Color.Green;
							cell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
							break;
					}
				}
			}
			else if (cell.ValueType == typeof(string))
			{
				switch ((string)cell.Value)
				{
					case "DCE":
						e.Value = "大商所";
						break;
					case "CZCE":
						e.Value = "郑商所";
						break;
					case "SHFE":
						e.Value = "上期所";
						break;
					case "CFFEX":
						e.Value = "中金所";
						break;
				}
			}

		}

		//价格:指定/跟随
		private void kryptonLabel5_Click(object sender, EventArgs e)
		{
			Color pre = kryptonLabel5.StateNormal.ShortText.Color1;
			kryptonLabel5.StateNormal.ShortText.Color1 = kryptonLabel5.StateNormal.ShortText.Color2;
			kryptonLabel5.StateNormal.ShortText.Color2 = pre;
			this.kryptonLabel5.Text = this.kryptonLabel5.Text == @"跟盘" ? @"指定" : @"跟盘";
		}

		//双击平仓
		void kryptonDataGridViewPosition_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
				return;
			if (!Confirm("DblClose", "双击平仓"))
				return;
			var row = this.kryptonDataGridViewPosition.Rows[e.RowIndex];
			DirectionType dire = (DirectionType)row.Cells["Direction"].Value;
			dire = dire == DirectionType.Buy ? DirectionType.Sell : DirectionType.Buy;
			string inst = (string)row.Cells["InstrumentID"].Value;
			int lots = (int)row.Cells["Position"].Value;
			InstrumentField instField;
			if (_t.DicInstrumentField.TryGetValue(inst, out instField) && instField.ExchangeID == "SHFE")
			{
				int td = (int)row.Cells["TdPosition"].Value;
				MarketData tick;
				if (!_q.DicTick.TryGetValue(inst, out tick))
					return;
				_t.ReqOrderInsert(inst, dire, OffsetType.CloseToday, dire == DirectionType.Buy ? tick.UpperLimitPrice : tick.LowerLimitPrice, td);
				lots -= td;
				if (lots > 0)
					_t.ReqOrderInsert(inst, dire, OffsetType.Close, dire == DirectionType.Buy ? tick.UpperLimitPrice : tick.LowerLimitPrice, lots);
			}
			else
				_t.ReqOrderInsert(inst, dire, OffsetType.Close, 0, lots, pType: OrderType.Market);
		}

		//双击撤单
		void kryptonDataGridViewOrder_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (Confirm("DblCancel", "双击撤单"))
				this.kryptonButtonCancel.PerformClick();
		}

		//撤单
		private void kryptonButtonCancel_Click(object sender, EventArgs e)
		{
			if (sender == this.kryptonButtonCancel && Confirm("Cancel", "撤单确认"))
				foreach (DataGridViewRow row in this.kryptonDataGridViewOrder.SelectedRows)
				{
					if ((OrderStatus)row.Cells["Status"].Value == OrderStatus.Filled || (OrderStatus)row.Cells["Status"].Value == OrderStatus.Canceled)
						continue;
					_t.ReqOrderAction((int)row.Cells["OrderID"].Value);
				}
			else if (Confirm("CancelAll", "全部撤单确认"))
			{
				foreach (var v in _t.DicOrderField)
				{
					if (v.Value.Status == OrderStatus.Canceled || v.Value.Status == OrderStatus.Filled)
						continue;
					_t.ReqOrderAction(v.Key);
				}
			}
		}

		//委托
		private void kryptonButtonOrder_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.kryptonComboBoxInstrument.Text))
				return;

			string instrument = this.kryptonComboBoxInstrument.Text;
			double price = (double)this.kryptonNumericUpDownPrice.Value;
			int volume = (int)this.kryptonNumericUpDownVolume.Value;
			DirectionType dire = this.kryptonRadioButtonBuy.Checked ? DirectionType.Buy : DirectionType.Sell;
			OffsetType offset = this.kryptonRadioButtonOpen.Checked ? OffsetType.Open : this.kryptonRadioButtonCloseToday.Checked ? OffsetType.CloseToday : OffsetType.Close;
			OrderType type = OrderType.Limit;
			if (sender == this.kryptonButtonMarket)
				type = OrderType.Market;
			else if (sender == this.kryptonButtonFOK)
				type = OrderType.FOK;
			else if (sender == this.kryptonButtonFAK)
				type = OrderType.FAK;

			if (Confirm("Order", string.Format("委托\t{0} {1} {2} {3}手 {4}", dire, offset, instrument, volume, type == OrderType.Limit ? price.ToString(CultureInfo.InvariantCulture) : type.ToString())))
				_t.ReqOrderInsert(instrument, dire, offset, price, volume, HedgeType.Speculation, type);
		}

		bool Confirm(string pKey, string pMsg)
		{
			if (((ToolStripMenuItem)this.ToolStripMenuItemOption.DropDownItems[pKey]).Checked)
			{
				KryptonTaskDialog dialog = new KryptonTaskDialog
				{
					CheckboxText = @"以后不再确认",
					CheckboxState = false,
					WindowTitle = @"确认",
					MainInstruction = pMsg + @"\t",//不加\t中文显示缺最后一个字
					Icon = MessageBoxIcon.Question,
					CommonButtons = TaskDialogButtons.OK | TaskDialogButtons.Cancel
				};
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					((ToolStripMenuItem)this.ToolStripMenuItemOption.DropDownItems[pKey]).Checked = dialog.CheckboxState == false;
					return true;
				}
				return false;
			}
			return true;
		}

		void quote_OnRtnTick(object sender, TickEventArgs e)
		{

		}

		void trade_OnRtnError(object sender, Trade2015.ErrorEventArgs e)
		{
			ShowMsg(string.Format("帐号({0}),错误:{1}--{2}", ((Trade)sender).Investor, e.ErrorID, e.ErrorMsg));
			if (e.ErrorMsg.IndexOf("未处理请求超过许可数", StringComparison.Ordinal) >= 0)
			{
				//重发
				Thread.Sleep(20);
				OrderField of;
				if (((Trade)sender).DicOrderField.TryGetValue(e.ErrorID, out of))
					((Trade)sender).ReqOrderInsert(of.InstrumentID, of.Direction, of.Offset, of.AvgPrice, of.Volume, of.Hedge, Math.Abs(of.LimitPrice) < 1E-6 ? OrderType.Market : OrderType.Limit, of.Custom);
			}
		}

		void trade_OnRtnNotice(object sender, StringEventArgs e)
		{
			ShowMsg(string.Format("帐号({0}),提醒:{1}", ((Trade)sender).Investor, e.Value));
		}

		void trade_OnRtnExchangeStatus(object sender, StatusEventArgs e)
		{
			ShowMsg(string.Format("{0,-12}{1,-8}:{2}", ((Trade)sender).Investor, e.Exchange, e.Status));
		}

		void trade_OnRtnOrder(object sender, OrderArgs e)
		{
			_queueOrderFresh.Enqueue(e.Value.OrderID);	//刷新时用
		}

		void trade_OnRtnTrade(object sender, TradeArgs e)
		{
			_queueTradeFresh.Enqueue(e.Value.TradeID);	//刷新时用
			if (e.Value.Offset == OffsetType.Open)
				_queuePositionFresh.Enqueue(e.Value.InstrumentID + "_" + e.Value.Direction);	//刷持仓
			else
				_queuePositionFresh.Enqueue(e.Value.InstrumentID + "_" + (e.Value.Direction == DirectionType.Buy ? "Sell" : "Buy"));	//刷持仓
		}

		void trade_OnRtnCancel(object sender, OrderArgs e)
		{
			_queueOrderFresh.Enqueue(e.Value.OrderID);	//刷新时用
		}
	}
}
