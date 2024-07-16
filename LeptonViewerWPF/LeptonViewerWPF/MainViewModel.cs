using System.ComponentModel;

namespace LeptonViewerWPF
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Binding Variables
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
