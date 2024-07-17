using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        public StartStreamingCommand StartStreamingCommand { get; private set; }
        public StopStreamingCommand StopStreamingCommand { get; private set; }
        public NormalizeCommand NormalizeCommand { get; private set; }

        public void OnMouseMove_LeptonImage(MouseEventArgs args, double actualWidth, double actualHeight, System.Windows.Point position)
        {
            if (LeptonBitmap != null)
            {
                // Convert position to pixel coordinates
                int x = (int)(position.X * LeptonBitmap.PixelWidth / actualWidth);
                int y = (int)(position.Y * LeptonBitmap.PixelHeight / actualHeight);

                if (x >= 0 && x < LeptonBitmap.PixelWidth && y >= 0 && y < LeptonBitmap.PixelHeight)
                {
                    ushort pixelValue = this.GetGray16PixelValue(LeptonBitmap, x, y);
                    MouseCursorTemp = $"Pixel value at [{x}, {y}] is {LeptonClass.ConvertFrameToTemperature(pixelValue)}℃";
                    //LogWriter.AddLog($"Pixel value at ({x}, {y}) is {pixelValue}");
                }
            }
        }
        private ushort GetGray16PixelValue(WriteableBitmap bitmap, int x, int y)
        {
            // Calculate the stride (width in bytes)
            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);

            // Create a buffer to hold the pixel data
            byte[] pixelData = new byte[2]; // Gray16 is 2 bytes per pixel

            // Copy the pixel data from the bitmap
            bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixelData, stride, 0);

            // Convert the byte array to a 16-bit value
            return BitConverter.ToUInt16(pixelData, 0);
        }
        #endregion

        #region Variables
        public WriteableBitmap? LeptonBitmap;
        public LeptonClass? LeptonClass;
        public OpenCvClass OpenCvClass;
        #endregion


        public MainViewModel()
        {
            LeptonClass = new LeptonClass();
            LeptonClass.OnFrameReceived += OnReceived_LeptonFrame;

            // OpenCV Class
            OpenCvClass = new OpenCvClass();
            OpenCvClass.OnFrameReceived += OnReceived_OpenCvFrame;

            // Commands
            StartStreamingCommand = new StartStreamingCommand(this);
            StopStreamingCommand = new StopStreamingCommand(this);
            NormalizeCommand = new NormalizeCommand(this);
        }



        private void OnReceived_LeptonFrame(object? sender, OnReceivedLeptonFrameEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int stride = args.Width * 2;
                LeptonImage1 = BitmapSource.Create(args.Width, args.Height, 96, 96, PixelFormats.Gray16, null, args.Frame, stride);
                // しきい値の設定
                var (minTemp2, maxTemp2) = LeptonClass.ConvertCelciusToKelvin(MinTemp, MaxTemp);
                var frame2 = LeptonClass.MapValue16(args.Frame, minTemp2, maxTemp2);
                LeptonImage2 = BitmapSource.Create(args.Width, args.Height, 96, 96, PixelFormats.Gray16, null, frame2, stride);

                // 顔検出
                Mat frame = BitmapSourceConverter.ToMat(_leptonImage2);
                OpenCvClass?.DetectFromGrayScale(frame);

                // ビットマップの保存
                LeptonBitmap = new WriteableBitmap(_leptonImage1);

                // 最大・最低温度の算出
                if (LeptonClass != null)
                {
                    var temperature = LeptonClass.ConvertFrameToTemperature(args.Frame);
                    MinMaxTemp = $"Max Temp: {temperature.Max():F2}°C, Min Temp: {temperature.Min():F2}°C";
                }
            });
        }

        private void OnReceived_OpenCvFrame(object? sender, OnReceivedOpenCvFrameEventArgs e)
        {
            LeptonImage3 = e.Bitmap;
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
