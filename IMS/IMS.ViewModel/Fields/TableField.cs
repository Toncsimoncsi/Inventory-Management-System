using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.ViewModel.Fields
{
    public class TableField : ViewModelBase
    {
        private String _color;
        private String _dir;

        public String Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }
        public String Direction
        {
            get { return _dir; }
            set
            {
                if (_dir != value)
                {
                    _dir = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Oszlop lekérdezése, vagy beállítása.
        /// </summary>
        public Int32 X { get; set; }

        /// <summary>
        /// Sor lekérdezése, vagy beállítása.
        /// </summary>
        public Int32 Y { get; set; }

        public DelegateCommand ViewField { get; set; }

        public Int32 Number { get; set; }


        /*
        public TableField(Int32 x, Int32 y, String color, Direction dir)
        {
            _color = color;
            _dir = Direction;
            X = x;
            Y = y;
        }
        */
    }
}
