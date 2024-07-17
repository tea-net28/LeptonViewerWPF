using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.IO;
using System.Windows.Media.Imaging;

public class OpenCvClass
{
    private CascadeClassifier? _cascadeClassifier;

    public event EventHandler<OnReceivedOpenCvFrameEventArgs>? OnFrameReceived;



    /// <summary>
    /// コンストラクター
    /// </summary>
    public OpenCvClass()
    {
        string path = Path.GetFullPath(".\\Models\\haarcascade_frontalface_default.xml");
        LogWriter.AddLog(path);
        if (File.Exists(path))
            LogWriter.AddLog("File is exist");
        else
        {
            LogWriter.AddLog("File is NOT exist");
            return;
        }
        _cascadeClassifier = new CascadeClassifier();
        _cascadeClassifier.Load(path);
    }

    /// <summary>
    /// Lepton から得た情報を元に作成したグレースケール画像を元に
    /// 顔検出を行う
    /// </summary>
    /// <param name="frame">グレースケール画像</param>
    public void DetectFromGrayScale(Mat frame)
    {
        if (_cascadeClassifier != null)
        {
            // Lepton3.5 から得られる情報が16ビットのため
            // 16bit から 8bit に変換
            Mat frame2 = new Mat();
            Cv2.Normalize(frame, frame2, 0, 255, NormTypes.MinMax);
            frame2.ConvertTo(frame2, MatType.CV_8U);
            // 顔を検出
            Rect[] faces = _cascadeClassifier.DetectMultiScale(frame2, 1.1, 4, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));

            // カラーフォーマットを グレースケール > RGB に
            Cv2.CvtColor(frame2, frame, ColorConversionCodes.GRAY2RGB);

            foreach (var rect in faces)
                Cv2.Rectangle(frame, rect, Scalar.Green, 1);

            // コールバックの実行
            OnReceivedOpenCvFrameEventArgs args = new OnReceivedOpenCvFrameEventArgs(frame);
            this.OnFrameReceived?.Invoke(this, args);
        }
    }
}

public class OnReceivedOpenCvFrameEventArgs : EventArgs
{
    public Mat Frame { get; set; }
    public BitmapSource Bitmap
    {
        get => BitmapSourceConverter.ToBitmapSource(Frame);
    }

    public OnReceivedOpenCvFrameEventArgs(Mat frame)
    {
        this.Frame = frame;
    }
}