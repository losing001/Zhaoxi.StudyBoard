using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Zhaoxi.StudyBoard.Base
{
    public class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string propName = "")
        {
            if (field == null || !field.Equals(value)) 
            {
                field = value;
                this.RaisePropertyChanged(propName);
            }
        }

        public void RaisePropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
