using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhaoxi.StudyBoard.Base;

namespace Zhaoxi.StudyBoard.Models
{
    public class LightModel : NotifyBase
    {
		private bool _state;

		public bool State
		{
			get { return _state; }
			set 
			{
				SetProperty<bool>(ref _state, value);
				LightColor = value ? "#F90" : "#888";
			}
		}

		private string _lightColor = "#888";

		public string LightColor
		{
			get { return _lightColor; }
			set { SetProperty<string>(ref _lightColor, value); }
		}

        public ushort Address { get; set; }
    }
}
