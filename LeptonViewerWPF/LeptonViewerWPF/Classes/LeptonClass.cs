using IR16Filters;
using Lepton;

public class LeptonClass
{
    private CCI.Handle? _leptonDevice;
    private CCI? _leptonCci;
    private IR16Capture? _capture;
    private bool _tlinear;

    public event EventHandler<OnReceivedLeptonFrameEventArgs>? OnFrameReceived;

    /// <summary>
    /// コンストラクター
    /// </summary>
    public LeptonClass()
    {
        // デバイスの取得
        var devices = Lepton.CCI.GetDevices();
        // 取得したデバイスの中から "PureThermal" がつくデバイスを取得
        var foundDevice = devices.FirstOrDefault(d => d.Name.StartsWith("PureThermal"));

        if (foundDevice ==  null)
        {
            LogWriter.AddErrorLog("デバイスが見つかりませんでした");
            return;
        }

        // デバイスに接続
        _leptonDevice = foundDevice;
        _leptonCci = _leptonDevice.Open();

        // 各種設定
        try
        {
            _leptonCci.sys.SetGainModeChecked(CCI.Sys.GainMode.HIGH);
            _leptonCci.rad.SetEnableStateChecked(CCI.Rad.Enable.ENABLE);
            _leptonCci.rad.SetTLinearEnableStateChecked(CCI.Rad.Enable.ENABLE);

            LogWriter.AddLog("This lepton supports tlinear", addLogFile: false);
            _tlinear = true;
        }
        catch (Exception error)
        {
            LogWriter.AddErrorLog(error);
            //LogWriter.AddLog("This lepton does not support tlinear", addLogFile: false);
            _tlinear = false;
        }

        // キャプチャークラスの初期化
        _capture = new IR16Capture();
        _capture.SetupGraphWithBytesCallback(new NewBytesFrameEvent(GotFrame));
    }

    /// <summary>
    /// フレームが送られてきたときに実行する処理
    /// </summary>
    /// <param name="bytes">データ</param>
    /// <param name="width">フレーム幅</param>
    /// <param name="height">フレーム高さ</param>
    private void GotFrame(ushort[] bytes, int width, int height)
    {
        // EventArgs の構成
        OnReceivedLeptonFrameEventArgs frameEventArgs = new OnReceivedLeptonFrameEventArgs(bytes, width, height);

        // コールバックの実行
        OnFrameReceived?.Invoke(this, frameEventArgs);
    }

    /// <summary>
    /// Lepton のセンサーをキャリブレーションする
    /// </summary>
    public void Normalize()
    {
        try
        {
            if (_leptonCci != null)
                _leptonCci.sys.RunFFCNormalizationChecked();
            else
                LogWriter.AddErrorLog("デバイスが存在しません", "Normalize");
        }
        catch (Exception error)
        {
            LogWriter.AddErrorLog(error, "Normalize");
        }
    }

    public void StartStreaming()
    {
        if (_capture == null)
        {
            LogWriter.AddErrorLog("デバイスが存在しません", "Start Streaming");
            return;
        }

        try
        {
            _capture.RunGraph();
            LogWriter.AddLog("ストリーミングを開始します", "StartStreaming", false);
        }
        catch (Exception error)
        {
            LogWriter.AddErrorLog(error, "StartStreaming");
        }
    }

    public void StopStreaming()
    {
        if (_capture == null)
        {
            LogWriter.AddErrorLog("デバイスが存在しません", "Start Streaming");
            return;
        }

        try
        {
            if(_capture.IsGraphRunning())
            {
                _capture.StopGraph();
                _capture.Dispose();
                LogWriter.AddLog("ストリーミングを終了します", "StopStreaming", false);
            }
        }
        catch (Exception error)
        {
            LogWriter.AddErrorLog(error, "StopStreaming");
        }
    }





    /// <summary>
    /// セルシウス度で設定した 最大・最低温度を Leptpn から送られてくる mK の範囲に変換する
    /// </summary>
    /// <param name="minTemp">最低温度[℃]</param>
    /// <param name="maxTemp">最大温度[℃]</param>
    /// <returns>mK に変換された値</returns>
    public static (ushort, ushort) ConvertCelciusToKelvin(float minTemp, float maxTemp)
    {
        // 変換元の範囲
        double xMin = -10;
        double xMax = 140;

        // 変換先の範囲
        double yMin = 26315;
        double yMax = 41315;

        // 変換の計算
        double tMin = (yMax - yMin) / (xMax - xMin) * (minTemp - xMin) + yMin;
        double tMax = (yMax - yMin) / (xMax - xMin) * (maxTemp - xMin) + yMin;

        return ((ushort)tMin, (ushort)tMax);
    }

    /// <summary>
    /// Lepton から送られてきた 14bit のグレースケール情報を
    /// 設定した最大・最低温度にスケーリングする
    /// </summary>
    /// <param name="value">元データ</param>
    /// <param name="inMin">最低温度</param>
    /// <param name="inMax">最大温度</param>
    /// <returns>スケーリングしたデータ</returns>
    public static ushort[] MapValue16(ushort[] value, float inMin, float inMax)
    {
        int outMin = 0;
        int outMax = 65535;

        List<ushort> result = new List<ushort>();
        for (int i = 0; i < value.Length; i++)
        {
            // 入力範囲を0-1の範囲に正規化
            double normalized = (value[i] - inMin) / (inMax - inMin);
            // 正規化された値を出力範囲にスケーリング
            int scaled = (int)(normalized * (outMax - outMin) + outMin);

            // 算出した値が出力範囲外だった場合は出力範囲内に変更
            if (scaled < outMin)
                scaled = outMin;
            if (scaled > outMax)
                scaled = outMax;

            result.Add((ushort)scaled);
        }

        return result.ToArray();
    }

    /// <summary>
    /// Lepton から得た温度データをセルシウス度に変換する
    /// </summary>
    /// <param name="frame">フレームデータ</param>
    /// <returns>算出した温度[℃]</returns>
    public static double ConvertFrameToTemperature(ushort frame)
    {
        return (frame - 27315.0d) / 100.0d;
    }
    /// <summary>
    /// Lepton から得た温度データをセルシウス度に変換する
    /// </summary>
    /// <param name="frame">フレームデータ</param>
    /// <returns>算出した温度[℃]</returns>
    public static double[] ConvertFrameToTemperature(ushort[] frame)
    {
        return frame.Select(x => (x - 27315) / 100.0).ToArray();
    }
}

public class OnReceivedLeptonFrameEventArgs : EventArgs
{
    public ushort[] Frame { get; }
    public int Width { get; }
    public int Height { get; }

    public OnReceivedLeptonFrameEventArgs(ushort[] frame, int width, int height)
    {
        Frame = frame;
        Width = width;
        Height = height;
    }
}