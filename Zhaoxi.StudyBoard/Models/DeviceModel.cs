using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhaoxi.StudyBoard.Base;

namespace Zhaoxi.StudyBoard.Models
{
    public class DeviceModel : NotifyBase
    {
		private float _temperature;

		public float Temperature
		{
			get { return _temperature; }
			set 
			{ 
				SetProperty<float>(ref _temperature, value);

				if (MinTemperature > value) MinTemperature = value;
				if (MaxTemperature < value) MaxTemperature = value;
            }
		}

		private float _minTemperature = 100;

		public float MinTemperature
		{
			get { return _minTemperature; }
			set { SetProperty<float>(ref _minTemperature, value); }
		}

		private float _maxTemperature = -100;

        public float MaxTemperature
        {
            get { return _maxTemperature; }
            set { SetProperty<float>(ref _maxTemperature, value); }
        }


		private int _humidity;

		public int Humidity
		{
			get { return _humidity; }
			set 
			{ 
				SetProperty<int>(ref _humidity, value);
                if (MinHumidity > value) MinHumidity = value;
                if (MaxHumidity < value) MaxHumidity = value;
            }
		}

		private int _minHumidity = 200;

		public int MinHumidity
		{
			get { return _minHumidity; }
			set { SetProperty<int>(ref _minHumidity, value); }
		}

		private int _maxHumidity = -1;

		public int MaxHumidity
		{
			get { return _maxHumidity; }
			set { SetProperty<int>(ref _maxHumidity, value); }
		}



		private int _brightness;

		public int Brightness
		{
			get { return _brightness; }
			set 
			{
				SetProperty<int>(ref _brightness, value);
                if (MinBrightness > value) MinBrightness = value;
                if (MaxBrightness < value) MaxBrightness = value;
            }
		}
		private int _minBrightness = 101;

		public int MinBrightness
		{
			get { return _minBrightness; }
			set { SetProperty<int>(ref _minBrightness, value); }
		}
		private int _maxBrightness = -1;

		public int MaxBrightness
		{
			get { return _maxBrightness; }
			set { SetProperty<int>(ref _maxBrightness, value); }
		}

		


        public bool UseTemperatureSim { get; set; }
		public int MinTemperatureSim { get; set; } = -10;
		public int MaxTemperatureSim { get; set; } = 80;

        public bool UseHumiditySim { get; set; }
		public int MinHumiditySim { get; set; } = 0;
		public int MaxHumiditySim { get; set; } = 100;

        public bool UseBrightnessSim { get; set; }
		public int MinBrightnessSim { get; set; } = 0;
		public int MaxBrightnessSim { get; set; } = 100;
    }
}
