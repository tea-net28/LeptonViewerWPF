using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace LeptonViewerWPF
{
    public partial class App : Application
    {
        private System.Threading.Mutex _mutex = new System.Threading.Mutex(false, $"%@{Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location)}@%");

        private string _appVersion = "";
        private MainWindow? _mainWindow;
        private MainViewModel? _mainViewModel;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Check Mutex
            if (!_mutex.WaitOne(0, false))
            {
                // 既に起動しているため終了させる
                MessageBox.Show("アプリケーションは既に起動しています。", "二重起動は出来ません", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                _mutex.Close();
                this.Shutdown();
            }
            else
            {
                // バージョンの取得 ＝＞ ログに出力
                string version = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("[ yyyy.MM.dd HH:mm:ss ]", CultureInfo.CurrentCulture);
                _appVersion = $"Version: {version}";
                LogWriter.AddLog($"===== Start Application: {_appVersion} =====");

                // MainWindow クラスの作成 ＞ ウインドウの表示
                _mainWindow = new MainWindow();
                _mainViewModel = new MainViewModel();
                _mainWindow.DataContext = _mainViewModel;
                // バージョン情報を保存
                _mainViewModel.Version = _appVersion;

                _mainWindow.Show();

                // 毎フレーム実行するメソッドを設定
                CompositionTarget.Rendering += CompositionTargetRendering;
            }
        }

        private List<string> logList = new List<string>();
        private StringBuilder sbLog = new StringBuilder();
        private int maxLogCount = 20;
        private void CompositionTargetRendering(object? sender, EventArgs e)
        {
            if (_mainViewModel != null)
            {
                // 現在の時間を表示
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.CurrentCulture));
                _mainViewModel.Time = sb.ToString();
                sb.Clear();

                // キューに貯まっているログを処理
                string? result = "";
                if (!LogWriter.Q_Log.IsEmpty)
                {
                    if (LogWriter.Q_Log.TryDequeue(out result))
                    {
                        logList.Add(result);
                        while (logList.Count > maxLogCount)
                            logList.RemoveAt(0);

                        foreach (var log in logList)
                            sbLog.AppendLine(log.Replace("\r\n", ""));
                        _mainViewModel.Logs = sbLog.ToString();
                        sbLog.Clear();
                    }
                }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Lepton のストリーミングの終了
            _mainViewModel?.LeptonClass?.StopStreaming();

            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
            }
            LogWriter.AddLog("===== Exit Application =====");
        }
    }
}
