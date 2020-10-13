using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Client
{
    class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _numbers;
        private bool _status;

        public int Numbers
        {
            get
            {
                return _numbers;
            }

            set
            {
                _numbers = value;
            }
        }


        public bool Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }


        public Model()
        {
            _status = false;
            _numbers = 0;
        }

    }
}
