using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Model
{
    public class SelectionChangedEventArgs
    {
        public Int32 X { get; private set; }
        public Int32 Y { get; private set; }
        public Boolean IsSelected { get; private set; }
        public SelectionChangedEventArgs(int x, int y, bool isSelected)
        {
            X = x;
            Y = y;
            IsSelected = isSelected;
        }
    }
}
