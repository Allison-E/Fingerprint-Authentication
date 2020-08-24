using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows;

namespace Fingerprint_Authentication
{
    public class Image : INotifyPropertyChanged
    {
        private Bitmap picture;

        public event PropertyChangedEventHandler PropertyChanged;

        public Bitmap Picture
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
