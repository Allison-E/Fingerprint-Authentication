using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fingerprint_Authentication
{
    public class Prompt: INotifyPropertyChanged
    {
        private string picture;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Picture
        {
            get
            {
                return picture;
            }
            set
            {
                if (picture != value)
                {
                    picture = value;
                    onPropertyChanged();
                }
            }
        }

        private void onPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
