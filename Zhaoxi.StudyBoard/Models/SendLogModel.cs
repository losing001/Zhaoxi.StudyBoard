using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhaoxi.StudyBoard.Models
{
    public class SendLogModel
    {
        public string LogInfo { get; set; }
        public DateTime LogTime { get; set; } = DateTime.Now;
    }
}
