using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FrameWork.Manager
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public class LogManager : Singleton<LogManager>
    {
        // 日志级别枚举
        public enum LogLevel
        {
            None = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            All = 4
        }

        private LogLevel currentLogLevel = LogLevel.All;
        private bool enableLog = true;
        private bool writeToFile = false;
        private string logFilePath;

        public void Initialize()
        {
#if UNITY_EDITOR
            enableLog = true;
            currentLogLevel = LogLevel.All;
#else
            enableLog = Debug.isDebugBuild;
            currentLogLevel = Debug.isDebugBuild ? LogLevel.All : LogLevel.Error;
#endif

            logFilePath = $"{Application.persistentDataPath}/game_log.txt";
            Log("LogManager initialized.");
        }

        #region 公共日志接口

        public static void Log(string message)
        {
            if (!Instance.enableLog || Instance.currentLogLevel < LogLevel.Info) return;
            Debug.Log($"[INFO] {message}");
            Instance.WriteToFileIfNeeded("[INFO]", message);
        }

        public static void Log(string message, object context)
        {
            if (!Instance.enableLog || Instance.currentLogLevel < LogLevel.Info) return;
            Debug.Log($"[INFO] {message}", context as UnityEngine.Object);
            Instance.WriteToFileIfNeeded("[INFO]", message);
        }

        public static void LogWarning(string message)
        {
            if (!Instance.enableLog || Instance.currentLogLevel < LogLevel.Warning) return;
            Debug.LogWarning($"[WARNING] {message}");
            Instance.WriteToFileIfNeeded("[WARNING]", message);
        }

        public static void LogWarning(string message, object context)
        {
            if (!Instance.enableLog || Instance.currentLogLevel < LogLevel.Warning) return;
            Debug.LogWarning($"[WARNING] {message}", context as UnityEngine.Object);
            Instance.WriteToFileIfNeeded("[WARNING]", message);
        }

        public static void LogError(string message)
        {
            if (!Instance.enableLog || Instance.currentLogLevel < LogLevel.Error) return;
            Debug.LogError($"[ERROR] {message}");
            Instance.WriteToFileIfNeeded("[ERROR]", message);
        }

        public static void LogError(string message, object context)
        {
            if (!Instance.enableLog || Instance.currentLogLevel < LogLevel.Error) return;
            Debug.LogError($"[ERROR] {message}", context as UnityEngine.Object);
            Instance.WriteToFileIfNeeded("[ERROR]", message);
        }

        public static void LogException(Exception exception)
        {
            if (!Instance.enableLog || Instance.currentLogLevel < LogLevel.Error) return;
            Debug.LogException(exception);
            Instance.WriteToFileIfNeeded("[EXCEPTION]", exception.ToString());
        }

        #endregion

        #region 日志控制接口

        public void SetLogLevel(LogLevel level)
        {
            currentLogLevel = level;
            Log($"Log level changed to: {level}");
        }

        public void EnableLog(bool enable)
        {
            enableLog = enable;
            Log($"Logging {(enable ? "enabled" : "disabled")}");
        }

        public void EnableFileLogging(bool enable)
        {
            writeToFile = enable;
            Log($"File logging {(enable ? "enabled" : "disabled")}");
        }

        public void SetLogFilePath(string path)
        {
            logFilePath = path;
            Log($"Log file path changed to: {path}");
        }

        #endregion

        #region 私有方法

        private void WriteToFileIfNeeded(string level, string message)
        {
            if (!writeToFile) return;

            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{level} {message}\n";
                System.IO.File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to log file: {e.Message}");
            }
        }

        #endregion
    }
}