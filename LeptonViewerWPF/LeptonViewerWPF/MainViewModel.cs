using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace LeptonViewerWPF
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Binding Variables
        private float _minTemp = 22.0f;
        public float MinTemp
        {
            get => _minTemp;
            set
            {
                _minTemp = value;
                RaisePropertyChanged(nameof(MinTemp));
            }
        }
        private float _maxTemp = 40.0f;
        public float MaxTemp
        {
            get => _maxTemp;
            set
            {
                _maxTemp = value;
                RaisePropertyChanged(nameof(MaxTemp));
            }
        }

        private BitmapSource _leptonImage1;
        public BitmapSource LeptonImage1
        {
            get => _leptonImage1;
            set
            {
                _leptonImage1 = value;
                RaisePropertyChanged(nameof(LeptonImage1));
            }
        }
        private BitmapSource _leptonImage2;
        public BitmapSource LeptonImage2
        {
            get => _leptonImage2;
            set
            {
                _leptonImage2 = value;
                RaisePropertyChanged(nameof(LeptonImage2));
            }
        }
        private BitmapSource _leptonImage3;
        public BitmapSource LeptonImage3
        {
            get => _leptonImage3;
            set
            {
                _leptonImage3 = value;
                RaisePropertyChanged(nameof(LeptonImage3));
            }
        }

        private string _minMaxTemp = "Max Temp: 0.00°C, Min Temp: 0.00°C";
        public string MinMaxTemp
        {
            get => _minMaxTemp;
            set
            {
                _minMaxTemp = value;
                RaisePropertyChanged(nameof(MinMaxTemp));
            }
        }

        private string _mouseCursorTemp = "";
        public string MouseCursorTemp
        {
            get
            {
                return _mouseCursorTemp;
            }
            set
            {
                _mouseCursorTemp = value;
                RaisePropertyChanged(nameof(MouseCursorTemp));
            }
        }
        #endregion

        #region Commands
        #endregion

        #region Variables
        #endregion


        public MainViewModel()
        {

        }



        #region Core variables
        private string _version = "Text_Version";
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                RaisePropertyChanged(nameof(Version));
            }
        }
        private string _time = "Text_Time";
        public string Time
        {
            get { return _time; }
            set
            {
                _time = value;
                RaisePropertyChanged(nameof(Time));
            }
        }
        private string _logs = "text_Logs";
        public string Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                RaisePropertyChanged(nameof(Logs));
            }
        }
        #endregion



        // ============================================================================================

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var d = PropertyChanged;
            if (d != null)
                d(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
