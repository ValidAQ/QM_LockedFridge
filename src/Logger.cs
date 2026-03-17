using System;
using System.Reflection;
using UnityEngine;

namespace QM_LockedFridge
{
    /// <summary>
    /// A Unity Debug logger that automatically prefixes every message with the mod's assembly name.
    /// Makes it easy to find this mod's output in the log.
    /// </summary>
    public class Logger
    {
        public string LogPrefix { get; set; }

        public Logger(string logPrefix = "")
        {
            if (string.IsNullOrEmpty(logPrefix))
                logPrefix = Assembly.GetExecutingAssembly().GetName().Name;

            LogPrefix = logPrefix;
        }

        public void Log(string message) => Debug.Log($"[{LogPrefix}] {message}");

        public void LogWarning(string message) => Debug.LogWarning($"[{LogPrefix}] {message}");

        public void LogError(string message) => Debug.LogError($"[{LogPrefix}] {message}");

        public void LogException(Exception ex)
        {
            Debug.LogError($"[{LogPrefix}] Exception:");
            Debug.LogException(ex);
        }
    }
}
