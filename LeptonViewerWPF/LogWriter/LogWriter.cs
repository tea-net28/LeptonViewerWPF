/**
 * 指定したパスにログファイルを出力するクラスライブラリ
 * 通常のログとエラーログを分けることができる
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

public static class LogWriter
{
    // ログファイルを出力するパスの設定
    public static string _logDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Logs";

    // キューを保存する変数
    private static ConcurrentQueue<string>? _q_Log;
    public static ConcurrentQueue<string> Q_Log
    {
        get
        {
            if (_q_Log == null)
                _q_Log = new ConcurrentQueue<string>();
            return _q_Log;
        }
    }

    // ログを追記する際に使用するスレッド制限
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);



    /// <summary>
    /// ログを出力する
    /// </summary>
    /// <param name="text">ログ</param>
    /// <param name="addLogFile">ログファイルに追記を行うか</param>
    public static void AddLog(string text, string methodName = "", bool addLogFile = true)
    {
        string dateTime = DateTime.Now.ToString("[MM/dd HH:mm:ss.fff]");
        string logText = String.IsNullOrWhiteSpace(methodName) ? $"{dateTime} {text}" : $"{dateTime} 【{methodName}】 {text}";
        Q_Log.Enqueue(logText);

        // ログファイルに追記する場合は実行
        if (addLogFile)
            AddInfo(String.IsNullOrWhiteSpace(methodName) ? $"{text}" : $"【{methodName}】 {text}", dateTime);
    }

    /// <summary>
    /// 警告ログを出力する
    /// </summary>
    /// <param name="text">ログ</param>
    /// <param name="methodName">発生したメソッド名</param>
    /// <param name="addLogFile">ログファイルに追記するか</param>
    public static void AddWarningLog(string text, string methodName = "", bool addLogFile = true)
    {
        string dateTime = DateTime.Now.ToString("[MM/dd HH:mm:ss.fff]");
        string logText = String.IsNullOrWhiteSpace(methodName) ? $"{dateTime} WARNING: {text}" : $"{dateTime} WARNING 【{methodName}】 {text}";

        Q_Log.Enqueue(logText);

        if (addLogFile)
            AddInfo(String.IsNullOrWhiteSpace(methodName) ? $"WARNING: {text}" : $"WARNING 【{methodName}】 {text}", dateTime);
    }

    /// <summary>
    /// エラーログを出力する
    /// </summary>
    /// <param name="error">例外</param>
    /// <param name="methodName">発生したメソッド名</param>
    public static void AddErrorLog(Exception error, string methodName = "")
    {
        string dateTime = DateTime.Now.ToString("[MM/dd HH:mm:ss.fff]");
        string logText = String.IsNullOrWhiteSpace(methodName) ? $"{dateTime} ERROR: {error}" : $"{dateTime} ERROR 【{methodName}】{error}";
        Q_Log.Enqueue(logText);
        //Q_Log.Enqueue($"{dateTime} ◆◆◆◆◆ ERROR ◆◆◆◆◆ {(methodName == "" ? "" : $"【{methodName}】 :")} {error}");
        AddError(error, methodName, dateTime);
    }

    /// <summary>
    /// エラーログを出力
    /// </summary>
    /// <param name="text">ログ</param>
    /// <param name="methodName">発生したメソッド名</param>
    public static void AddErrorLog(string text, string methodName = "")
    {
        string dateTime = DateTime.Now.ToString("[MM/dd HH:mm:ss.fff]");
        string logText = String.IsNullOrWhiteSpace(methodName) ? $"{dateTime} ERROR: {text}" : $"{dateTime} ERROR 【{methodName}】{text}";
        Q_Log.Enqueue(logText);
        //Q_Log.Enqueue($"{dateTime} ◆◆◆◆◆ ERROR ◆◆◆◆◆ {(methodName == "" ? "" : $"【{methodName}】 :")} {text}");
        AddError(text, methodName, dateTime);
    }

    /// <summary>
    /// 指定した日にちより前のログファイルを削除するメソッド
    /// エラーログについてはデフォルトでは削除しない設定にしている
    /// </summary>
    /// <param name="date">削除する日にち</param>
    /// <param name="deleteErrorLog">エラーログも削除するか</param>
    public static void ClearLog(int date = 180, bool deleteErrorLog = false)
    {
        // ログファイルの一覧を取得
        List<string> files_Log = new List<string>();
        List<string> files_LogError = new List<string>();

        // 各フォルダにあるファイル名を取得
        try
        {
            files_Log = Directory.GetFiles(_logDir, "*", SearchOption.TopDirectoryOnly).ToList();
            files_LogError = Directory.GetFiles(_logDir + "_Error", "*", SearchOption.TopDirectoryOnly).ToList();
        }
        catch (Exception error)
        {
            LogWriter.AddErrorLog(error, "Clear Log - Get Files");
        }

        // 指定した日にちより前のファイルを削除
        if (files_Log.Count > 0)
        {
            DeleteLogs(files_Log, date);
        }
        if (files_LogError.Count > 0 && deleteErrorLog)
        {
            DeleteLogs(files_LogError, date);
        }
    }


    // ============================================================================================


    private static async void AddInfo(string text, string date)
    {
        string _date = date;
        string dir = _logDir;
        string fileName = $"Log_{DateTime.Now.ToString("yyyy_MM_dd")}.txt";
        string fullpath = Path.Combine(dir, fileName);

        // ディレクトリが存在しない場合は作成
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // ロックを取得
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            using (StreamWriter streamWriter = new StreamWriter(fullpath, true, Encoding.UTF8))
            {
                var parentDir = new FileInfo(fullpath).Directory;
                // ディレクトリが存在しない場合は新規作成
                if (parentDir != null && !parentDir.Exists)
                    parentDir.Create();

                // ログを追記
                await streamWriter.WriteLineAsync($"{_date} {text}").ConfigureAwait(false);
            }
        }
        catch (Exception error)
        {
            LogWriter.AddErrorLog(error, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
        finally
        {
            // ロックの開放
            _semaphore.Release();
        }
    }

    private static async void AddError(Exception error, string methodName, string date)
    {
        string _date = date;
        string dir = _logDir + "_Error";
        string fileName = $"ErrorLog_{DateTime.Now.ToString("yyyy_MM_dd")}.txt";
        string fullpath = Path.Combine(dir, fileName);

        // ディレクトリが存在しない場合は作成
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // ロックの取得
        await _semaphore.WaitAsync().ConfigureAwait(false);
        using (StreamWriter streamWriter = new StreamWriter(fullpath, true, Encoding.UTF8))
        {
            try
            {
                var parentDir = new FileInfo(fullpath).Directory;
                // ディレクトリが存在しない場合は新規作成
                if (parentDir != null && !parentDir.Exists)
                    parentDir.Create();

                // ログを追記
                await streamWriter.WriteLineAsync($"{_date} {(String.IsNullOrWhiteSpace(methodName) ? "" : $"@{methodName} ")}: {error}").ConfigureAwait(false);
            }
            finally
            {
                // ロックの開放
                _semaphore.Release();
            }
        }
    }

    private static async void AddError(string text, string methodName, string date)
    {
        string _date = date;
        string dir = _logDir + "_Error";
        string fileName = $"ErrorLog_{DateTime.Now.ToString("yyyy_MM_dd")}.txt";
        string fullpath = Path.Combine(dir, fileName);

        // ディレクトリが存在しない場合は作成
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // ロックの取得
        await _semaphore.WaitAsync().ConfigureAwait(false);
        using (StreamWriter streamWriter = new StreamWriter(fullpath, true, Encoding.UTF8))
        {
            try
            {
                var parentDir = new FileInfo(fullpath).Directory;
                // ディレクトリが存在しない場合は新規作成
                if (parentDir != null && !parentDir.Exists)
                    parentDir.Create();

                // ログを追記
                await streamWriter.WriteLineAsync($"{_date} {(String.IsNullOrWhiteSpace(methodName) ? "" : $"@{methodName} ")}: {text}").ConfigureAwait(false);
            }
            finally
            {
                // ロックの開放
                _semaphore.Release();
            }
        }
    }

    private static void DeleteLogs(List<string> files, int date)
    {
        foreach (var item in files)
        {
            if (File.Exists(item))
            {
                try
                {
                    DateTime lastWriteTime = File.GetLastWriteTime(item);
                    TimeSpan elapsed = DateTime.Now - lastWriteTime;
                    if (elapsed.TotalDays >= date)
                    {
                        File.Delete(item);
                        LogWriter.AddLog($"ログファイル： {Path.GetFileName(item)} を削除しました");
                    }
                }
                catch (Exception error)
                {
                    LogWriter.AddErrorLog(error, "Delete Logs");
                }
            }
        }
    }
}

/* ログを表示する場合は MainWindow.cs 等に以下を追記する
 * 変数
    List<string> logList = new List<string>();
    StringBuilder sbLog = new StringBuilder();
 * Window_Loaded 内
    CompositionTarget.Rendering += CompositionTargetRendering;

 * CompositionTargetRendering メソッド
    string? result = "";
    if (LogWriter.q_Log.Count > 0)
    {
        if (LogWriter.q_Log.TryDequeue(out result))
        {
            logList.Add(result);
            while (logList.Count > 20)
            logList.RemoveAt(0);

            foreach (var log in logList)
            sbLog.AppendLine(log.Replace("\r\n", ""));
            textJson.Text = sbLog.ToString();
            sbLog.Clear();
        }
    }
*/