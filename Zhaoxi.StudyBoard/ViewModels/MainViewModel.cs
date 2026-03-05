using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Zhaoxi.StudyBoard.Base;
using Zhaoxi.StudyBoard.Models;

namespace Zhaoxi.StudyBoard.ViewModels
{
    public class MainViewModel : NotifyBase
    {
		DispatcherTimer timer = new DispatcherTimer();

		private DateTime _currentTime;

		public DateTime CurrentTime
		{
			get { return _currentTime; }
			set { SetProperty<DateTime>(ref _currentTime, value); }
		}

        #region 串口下拉与相关属性
        private string _portName;

		public string PortName
		{
			get { return _portName; }
			set { SetProperty<string>(ref _portName, value); }
		}
		public int BaudRate { get; set; } = 9600;
		public int DataBits { get; set; } = 8;
		public string StopBits { get; set; } = "One";
		public string Parity { get; set; } = "None";

        public List<string> PortList { get; set; }
		public List<int> BaudRateList { get; set; } = new List<int>()
		{
			4800,9600,14400,19200,38400,115200
		};
		public List<int> DataBitsList { get; set; } = new List<int>
		{
			5,7,8
		};
		public List<string> StopBitsList { get; set; }
        public List<string> ParityList { get; set; }
        #endregion

        #region 运行环境状态数据
        private double _currentCPU;

		public double CurrentCPU
		{
			get { return _currentCPU; }
			set { SetProperty<double>(ref _currentCPU, value); }
		}
		private double _currentMem;

		public double CurrentMem
		{
			get { return _currentMem; }
			set { SetProperty<double>(ref _currentMem, value); }
		}
		private PerformanceCounter cpuCounter;
		private ManagementClass memCounter;
        #endregion

        private bool _startState;

		public bool StartState
		{
			get { return _startState; }
			set 
			{
				_startState = value;

				try 
				{
					OnStart();
                }
				catch (Exception ex)
				{
					//消息提示
					this.ShowMessage(ex.Message, "通信异常", "Orange");
				}
			}
		}


        #region 图表数据属性
        public ChartValues<double> TemperatureValues { get; set; } = new ChartValues<double>() {
            38,70,57,62,67,27,75,56,79,20,77,46,33,63,49,56,79,20,77,46
        };
        public ChartValues<double> HumidityValues { get; set; } = new ChartValues<double>() {
            46,20,30,56,57,33,76,54,74,65,66,24,71,77,58,20,30,56,57,33
        };
        public ChartValues<double> BrightnessValues { get; set; } = new ChartValues<double>() {
            56,40,20,86,17,33,56,34,74,95,16,44,11,97,18,86,17,33,56,34
        };
		public ObservableCollection<string> XLabels { get; set; } = new ObservableCollection<string>();
		#endregion

		#region OLED数据处理
		public string SendText { get; set; } = "Hello Zhaoxi";
		private string _oledText;

		public string OledText
		{
			get { return _oledText; }
			set { SetProperty<string>(ref _oledText, value); }
		}
        public Command SendTextCommand { get; set; }
        public Command ResendTextCommand { get; set; }

        public ObservableCollection<SendLogModel> LogList { get; set; } =
			new ObservableCollection<SendLogModel>();

        #endregion

        public DeviceModel DeviceInfo { get; set; } = new DeviceModel();
        public MessageModel MessageInfo { get; set; } = new MessageModel();


        #region 状态灯
        public List<LightModel> LightList { get; set; } = new List<LightModel>();
        public Command LightCommand { get; set; }
		#endregion

		private bool _enabled = true;

		public bool Enabled
		{
			get { return _enabled; }
			set { SetProperty<bool>(ref _enabled, value); }
		}


		SerialPort serialPort = new SerialPort();
		public MainViewModel()
		{
			SendTextCommand = new Command(OnSendText);
			ResendTextCommand = new Command(OnResendText);
			LightCommand = new Command(OnLight);

            timer.Interval = new TimeSpan(0, 0, 1);
			timer.Tick += (se, ev) => 
			{
				CurrentTime = DateTime.Now;


				if (PortList == null)
				{
					PortList = SerialPort.GetPortNames().ToList();
					if (PortList.Count > 0)
						this.PortName = PortList[0];
					this.RaisePropertyChanged("PortList");
				}
				else if (!PortList.SequenceEqual(SerialPort.GetPortNames()))
				{
                    PortList = SerialPort.GetPortNames().ToList();
					this.RaisePropertyChanged("PortList");
					//如果新列表中没有当前显示的串口名称的话，将新列表的第0个子项赋值给当前串口
                    if (!PortList.Exists(p => p == PortName))
					{
						PortName = "";
                        if (PortList.Count > 0)
                            this.PortName = PortList[0];
                    }
                }
			};
			timer.Start();

			StopBitsList = Enum.GetNames(typeof(StopBits)).ToList();
			ParityList = Enum.GetNames(typeof(Parity)).ToList();

			for (int i = 19; i >=0; i--)
			{
				var time = DateTime.Now;
				XLabels.Add(time.AddSeconds(i * -1).ToString("ss"));
			}

			for (ushort i = 0; i <= 5; i++)
			{
				LightList.Add(new LightModel { Address = i });
			}
			for (int i = 0; i < 10; i++)
			{
				LogList.Add(new SendLogModel { LogInfo = $"Hello Zhaoxi - [{i.ToString("000")}]" });
			}

			cpuCounter = new PerformanceCounter();
			cpuCounter.CategoryName = "Processor";
			cpuCounter.CounterName = "% Processor Time";
			cpuCounter.InstanceName = "_Total";

			memCounter = new ManagementClass();

			Task.Run(async () =>
			{
				while (true)
				{
					this.CurrentCPU = this.cpuCounter.NextValue();
					this.CurrentMem = this.GetMemInfo();
					await Task.Delay(1000);
                }
			});
		}

		bool isWriting = false;
		private void OnLight(object obj)
		{
			if (!serialPort.IsOpen) return;

			isWriting = true;
			try
			{
				if (obj.ToString() == "all")
				{
					byte state = (byte)(LightList[5].State ? 0x1F : 0x00);
					byte[] bytes = new byte[]{
						0x01,
						0x0F,
						0x00,
						0x00,
						0x00,
						0x05,
						0x01,
						state,
						0x00,0x00
					};
					CRC16(bytes);

					this.SendAndReceive(bytes);

					LightList.ForEach(l => l.State = LightList[5].State);
				}
				else
				{
					int.TryParse(obj.ToString(), out int index);
					byte state = (byte)(LightList[index].State ? 0xFF : 0x00);

                    byte[] bytes = new byte[]{
                        0x01,
                        0x05,
                        0x00,
                        (byte)index,
                        state,
                        0x00,
                        0x00,0x00
                    };
                    CRC16(bytes);

                    this.SendAndReceive(bytes);

					LightList[5].State =
						LightList[0].State &&
						LightList[1].State &&
						LightList[2].State &&
						LightList[3].State &&
						LightList[4].State;
                }
			}
			catch (Exception ex)
			{
                this.ShowMessage(ex.Message, "发送异常", "Orange");
            }
			finally
			{
				isWriting = false;
            }
		}

		private void OnSendText(object obj)
		{
			if (!serialPort.IsOpen) return;

			try
			{
				if (string.IsNullOrEmpty(this.SendText))
					throw new Exception("请输入有效的英文字符");
				else if (this.SendText.Length > 60)
					throw new Exception("输入的字符长度不能超过60");
				else if (this.SendText.ToList().Exists(s => (int)s > 127))
					throw new Exception("包含无效字符");

				this.SendOledText(this.SendText);
            }
			catch (Exception ex)
			{
				this.ShowMessage(ex.Message, "发送异常", "Orange");
			}
		}

		private void OnResendText(object obj)
		{
			if (!serialPort.IsOpen) return;

			try
			{
				string text = obj.ToString();
				this.SendOledText(text);
			}
			catch (Exception ex)
            {
                this.ShowMessage(ex.Message, "发送异常", "Orange");
            }
		}

        private void SendOledText(string text)
		{
			byte[] text_bytes = Encoding.ASCII.GetBytes(text);

			List<byte> bytes = new List<byte>();
			bytes.Add(0x01);
			bytes.Add(0x10);
			bytes.Add(0x00);
			bytes.Add(0x08);
			bytes.Add((byte)(Math.Ceiling(text_bytes.Length * 1.0 / 2) / 256));
			bytes.Add((byte)(Math.Ceiling(text_bytes.Length * 1.0 / 2) % 256));
			byte len = (byte)text_bytes.Length;
			len += (byte)(len % 2);
            bytes.Add(len);
			bytes.AddRange(text_bytes);
			if (text_bytes.Length % 2 == 1)
				bytes.Add(0x00);

			bytes.Add(0x00);
			bytes.Add(0x00);
			byte[] byteArray = bytes.ToArray();
			CRC16(byteArray);

			this.SendAndReceive(byteArray);

			this.OledText = text;

			this.LogList.Insert(0, new SendLogModel { LogInfo = text });
			if (this.LogList.Count > 30)
				this.LogList.RemoveAt(this.LogList.Count - 1);
		}

		private double GetMemInfo()
		{
			memCounter.Path = new ManagementPath("Win32_PhysicalMemory");
			ManagementObjectCollection moc = memCounter.GetInstances();
			double available = 0, capacity = 0;

            foreach (ManagementObject mo in moc)
            {
				capacity += ((Math.Round(Int64.Parse(mo.Properties["Capacity"].Value.ToString()) / 1024 / 1024 / 1024.0, 1)));
            }
			moc.Dispose();


            memCounter.Path = new ManagementPath("Win32_PerfFormattedData_PerfOS_Memory");
            moc = memCounter.GetInstances();
            foreach (ManagementObject mo2 in moc)
            {
                available += ((Math.Round(Int64.Parse(mo2.Properties["AvailableMBytes"].Value.ToString()) / 1024.0, 1)));
            }
            moc.Dispose();

            return (capacity - available) / capacity * 100;
        }

		private void ShowMessage(string message, string title = "运行提示", string color = "#00CFF8")
		{
			MessageInfo.Message = message;
			MessageInfo.Title = title;
			MessageInfo.MsgColor = color;
            MessageInfo.MsgTime = DateTime.Now;
        }

		private void OnStart()
		{
			if (serialPort.IsOpen)
			{
				serialPort.Close();
                this.ShowMessage("连接已断开，等待连接设备....");
				Enabled = true;
            }
			else
			{
				if (string.IsNullOrEmpty(PortName))
					throw new Exception("请先选择串口");

				serialPort.PortName = PortName;
				serialPort.BaudRate = BaudRate;
				serialPort.DataBits = DataBits;
				serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits);
                serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), Parity);

				serialPort.Open();
				this.ShowMessage("连接正常！正在监听接收学习卡数据");
				Enabled = false;
				//启动监听：while逻辑中，如果串口关闭，那么while也即终止
				Task.Run(async () =>
				{
					while (serialPort.IsOpen) 
					{
						this.OnMonitor();
						await Task.Delay(1000);
					}
				});
            }
		}

		int[] lastRandom = new int[3];
		bool[] order = new bool[3];
		Random random = new Random();
		private void OnMonitor()
		{
			try
			{
				//温湿亮度
				byte[] bytes = new byte[8];
				bytes[0] = 0x01;
				bytes[1] = 0x03;
				bytes[2] = 0x00;
				bytes[3] = 0x00;
				bytes[4] = 0x00;
				bytes[5] = 0x03;
				this.CRC16(bytes);

				byte[] resp = SendAndReceive(bytes);
				if (resp != null && resp.Length > 0 && resp[1] == 0x03)
				{
					if (DeviceInfo.UseTemperatureSim)
					{
						this.GenerateRandom(ref lastRandom[0], ref order[0],
							DeviceInfo.MinTemperatureSim, DeviceInfo.MaxTemperatureSim);
						DeviceInfo.Temperature = lastRandom[0];
					}
					else
						DeviceInfo.Temperature = BitConverter.ToInt16(new byte[] { resp[4], resp[3] }) * 0.1f;

					if (DeviceInfo.UseHumiditySim)
					{
						this.GenerateRandom(ref lastRandom[1], ref order[1],
							DeviceInfo.MinHumiditySim, DeviceInfo.MaxHumiditySim);
						DeviceInfo.Humidity = lastRandom[1];
					}
					else
						DeviceInfo.Humidity = BitConverter.ToUInt16(new byte[] { resp[6], resp[5] });

					if (DeviceInfo.UseBrightnessSim)
					{
						this.GenerateRandom(ref lastRandom[2], ref order[2],
							DeviceInfo.MinBrightnessSim, DeviceInfo.MaxBrightnessSim);
						DeviceInfo.Brightness = lastRandom[2];
					}
					else
						DeviceInfo.Brightness = BitConverter.ToUInt16(new byte[] { resp[8], resp[7] });


					this.TemperatureValues.Add(DeviceInfo.Temperature);
					this.HumidityValues.Add(DeviceInfo.Humidity);
					this.BrightnessValues.Add(DeviceInfo.Brightness);
					this.XLabels.Add(DateTime.Now.ToString("ss"));

					this.TemperatureValues.RemoveAt(0);
					this.HumidityValues.RemoveAt(0);
					this.BrightnessValues.RemoveAt(0);
					this.XLabels.RemoveAt(0);
				}


				//灯珠状态
				bytes[1] = 0x01;
				bytes[5] = 0x05;
				this.CRC16(bytes);
				resp = SendAndReceive(bytes);
				if (resp != null && resp.Length > 0 && resp[1] == 0x01 & !isWriting) 
				{
					LightList[0].State = (resp[3] & 1) != 0;
					LightList[1].State = (resp[3] & 2) != 0;
					LightList[2].State = (resp[3] & 4) != 0;
					LightList[3].State = (resp[3] & 8) != 0;
					LightList[4].State = (resp[3] & 16) != 0;
					//统一控制的状态
					LightList[5].State = (resp[3] & 31) == 31;
                }
			}
			catch (Exception ex)
			{
				this.ShowMessage(ex.Message, "通信异常", "Orange");
			}
        }

		private void GenerateRandom(ref int randomValue, ref bool order, int min, int max)
		{
			if (randomValue == 0)//升序
				randomValue = random.Next(min, max);
			else
			{
				int rv = randomValue;
				if (order)
				{
					var _max = Math.Min(rv + 20, max);
					do
					{
						randomValue = random.Next(rv - 5, _max);
					}
					while (randomValue > max);

					if (_max == max)
						order = false;
				}
				else//降序
				{
					var _min = Math.Max(rv - 20, min);
					do
					{
						randomValue = random.Next(_min, rv + 5);
					} while (randomValue < min);

					if (_min == min)
						order = true;
				}
			}
		}

		private static readonly object _lockObject = new object();
		private byte[] SendAndReceive(byte[] bytes)
		{
			lock (_lockObject)
			{
				serialPort.Write(bytes, 0, bytes.Length);

				Task.Delay(100).Wait();
				byte[] resp = new byte[serialPort.BytesToRead];
				serialPort.Read(resp, 0, resp.Length);
				return resp;
			}
		}

		private void CRC16(byte[] bytes)
		{
			if (bytes == null || !bytes.Any())
				throw new ArgumentException();

			ushort crc = 0xFFFF;
			for (int i = 0; i < bytes.Length - 2; i++) 
			{
				crc = (ushort)(crc ^ bytes[i]);
				for (int j = 0; j < 8; j++)
				{
					crc = (ushort)((crc & 1) != 0 ? (crc >> 1) ^ 0xA001 : (crc >> 1));
				}
			}
			byte hi = (byte)((crc & 0xFF00) >> 8);
			byte lo = (byte)(crc & 0x00FF);

			bytes[bytes.Length - 2] = lo;
			bytes[bytes.Length - 1] = hi;
		}
    }
}
