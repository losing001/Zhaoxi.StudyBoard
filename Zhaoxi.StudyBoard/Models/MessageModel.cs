using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhaoxi.StudyBoard.Base;

namespace Zhaoxi.StudyBoard.Models
{
    public class MessageModel : NotifyBase
    {
		private string _msgColor = "#00CFF8";

		public string MsgColor
		{
			get { return _msgColor; }
			set { SetProperty<string>(ref _msgColor, value); }
		}
		private string _title="系统提示";

		public string Title
		{
			get { return _title; }
			set { SetProperty<string>(ref _title, value); }
		}
		private string _message = "等待设备连接....";

		public string Message
		{
			get { return _message; }
			set { SetProperty<string>(ref _message, value); }
		}
		private DateTime _msgTime = DateTime.Now;

		public DateTime MsgTime
		{
			get { return _msgTime; }
			set { SetProperty<DateTime>(ref _msgTime, value); }
		}

	}
}
