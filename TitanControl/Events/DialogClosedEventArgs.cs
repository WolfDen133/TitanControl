using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Events
{
    public class DialogClosedEventArgs<T> : DialogClosedEventArgs
    {
        public new T? Result
        {
            get => (T?)base.Result;
            init => base.Result = value;
        }
    }

    public class DialogClosedEventArgs : EventArgs
    {
        public bool IsCanceled = false;

        public object? Result { get; set; } = null;
    }
}
