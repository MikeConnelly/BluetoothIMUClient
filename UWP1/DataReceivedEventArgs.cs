using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP1
{
    class DataReceivedEventArgs : EventArgs
    {
        public int FieldIndex { get; set; }
        public float Value { get; set; }
    }
}
