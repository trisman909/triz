using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Lanternfall.Editor
{
    /// <summary>
    /// Editor-only batch helpers that keep Unity alive until asynchronous
    /// suite execution finishes, then write a small authoritative result file.
    /// </summary>
    public static class Phase17BatchVerificationRunner
    {
        private const string PendingResultKey =
            "Lanternfall.Phase17.PendingTestResult";
        private const string PendingLabelKey =
            "Lanternfall.Phase17.PendingLabel";
        private const string PendingTestNamesKey =
            "Lanternfall.Phase17.PendingTestNames";
        private const string PendingRunGuidKey =
            "Lanternfall.Phase17.PendingRunGuid";
        private const string PendingRunModeKey =
            "Lanternfall.Phase17.PendingRunMode";
        private const string PendingRunStartedKey =
            "Lanternfall.Phase17.PendingRunStartedUtc";
        private static TestRunnerApi s_api;
        private static BatchCallbacks s_callbacks;

        [InitializeOnLoadMethod]
        private static void RestoreCallbacksAfterDomainReload()
        {
            string path = SessionState.GetString(PendingResultKey, string.Empty);
            if (!string.IsNullOrEmpty(path))
            {
                RegisterCallbacks(path);
                HookWatcher();
            }

            if (!string.IsNullOrEmpty(
                    SessionState.GetString(PendingLabelKey, string.Empty)))
            {
                EditorApplication.delayCall += TryStartPendingLaunch;
            }
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (!string.IsNullOrEmpty(
                    SessionState.GetString(PendingLabelKey, string.Empty)))
            {
                EditorApplication.delayCall += TryStartPendingLaunch;
            }
        }

        public static void RunEditModeSuite()
        {
            RunTests("editmode", TestMode.EditMode);
        }

        public static void RunProductionArtEditModeSuite()
        {
            RunTests(
                "editmode_productionart",
                TestMode.EditMode,
                "Lanternfall.Tests.ProductionArtValidationTests");
        }

        public static void RunDodgeTimingEditModeSuite()
        {
            RunTests(
                "editmode_dodgetiming",
                TestMode.EditMode,
                "Lanternfall.Tests.DodgeTimingModelTests");
        }

        public static void RunPlayModeSuite()
        {
            RunTests("playmode", TestMode.PlayMode);
        }

        public static void RunReleaseReadiness()
        {
            const string path = "TestResults/Phase17_readiness_current.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "TestResults");
            try
            {
                ReleaseReadinessValidator.ValidateOrThrow();
                File.WriteAllText(
                    path,
                    $"PASS|{DateTime.UtcNow:O}|Lanternfall release-readiness validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    path,
                    $"FAIL|{DateTime.UtcNow:O}|{exception}");
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void RunTests(
            string label,
            TestMode mode,
            params string[] testNames)
        {
            Directory.CreateDirectory("TestResults");
            string path = $"TestResults/Phase17_{label}_current.txt";
            SessionState.SetString(PendingResultKey, path);
            DateTime startedUtc = DateTime.UtcNow;
            File.WriteAllText(
                path,
                $"QUEUED|{startedUtc:O}|{mode}");
            SessionState.SetString(PendingLabelKey, label);
            SessionState.SetString(PendingRunModeKey, mode.ToString());
            SessionState.SetString(PendingRunStartedKey, startedUtc.ToString("O"));
            SessionState.SetString(
                PendingTestNamesKey,
                string.Join("|", testNames ?? Array.Empty<string>()));
            RegisterCallbacks(path);
            HookWatcher();
            EditorApplication.delayCall += TryStartPendingLaunch;
        }

        private static void TryStartPendingLaunch()
        {
            EditorApplication.delayCall -= TryStartPendingLaunch;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryStartPendingLaunch;
                return;
            }

            string path = SessionState.GetString(PendingResultKey, string.Empty);
            string label = SessionState.GetString(PendingLabelKey, string.Empty);
            string modeText =
                SessionState.GetString(PendingRunModeKey, string.Empty);
            if (string.IsNullOrEmpty(path) ||
                string.IsNullOrEmpty(label) ||
                string.IsNullOrEmpty(modeText) ||
                !Enum.TryParse(modeText, out TestMode mode))
            {
                return;
            }

            string runGuid =
                SessionState.GetString(PendingRunGuidKey, string.Empty);
            if (!string.IsNullOrEmpty(runGuid))
            {
                if (IsRunStillActive(runGuid))
                    return;

                if (File.Exists(path))
                {
                    string status = File.ReadAllText(path);
                    if (status.StartsWith("PASS|", StringComparison.Ordinal) ||
                        status.StartsWith("FAIL|", StringComparison.Ordinal) ||
                        status.StartsWith("STARTED|", StringComparison.Ordinal))
                    {
                        return;
                    }
                }
            }

            RegisterCallbacks(path);
            HookWatcher();
            string[] testNames = ParsePendingTestNames();
            var filter = new Filter
            {
                testMode = mode,
                testNames = testNames
            };
            string newRunGuid = s_api.Execute(new ExecutionSettings(filter));
            SessionState.SetString(PendingRunGuidKey, newRunGuid);
        }

        private static string[] ParsePendingTestNames()
        {
            string raw =
                SessionState.GetString(PendingTestNamesKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
                return Array.Empty<string>();

            string[] names =
                raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            return names.Length == 0 ? Array.Empty<string>() : names;
        }

        private static void RegisterCallbacks(string path)
        {
            s_callbacks = new BatchCallbacks(path);
            s_api = ScriptableObject.CreateInstance<TestRunnerApi>();
            TestRunnerApi.RegisterTestCallback(s_callbacks);
        }

        private static void HookWatcher()
        {
            EditorApplication.update -= WatchPendingRun;
            EditorApplication.update += WatchPendingRun;
        }

        private static void WatchPendingRun()
        {
            string path = SessionState.GetString(PendingResultKey, string.Empty);
            string runGuid =
                SessionState.GetString(PendingRunGuidKey, string.Empty);
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(runGuid))
            {
                EditorApplication.update -= WatchPendingRun;
                return;
            }

            if (IsRunStillActive(runGuid))
                return;

            string xmlPath = GetPersistentTestResultsPath();
            if (!DateTime.TryParse(
                    SessionState.GetString(PendingRunStartedKey, string.Empty),
                    out DateTime startedUtc))
            {
                startedUtc = DateTime.UtcNow.AddMinutes(-5);
            }

            if (!File.Exists(xmlPath))
                return;

            DateTime xmlWriteUtc = File.GetLastWriteTimeUtc(xmlPath);
            if (xmlWriteUtc < startedUtc.AddSeconds(-1))
                return;

            TryWriteResultFromXml(path, xmlPath);
        }

        private static bool IsRunStillActive(string runGuid)
        {
            MethodInfo method = typeof(TestRunnerApi).GetMethod(
                "IsRunning",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
                return false;

            object result = method.Invoke(null, new object[] { runGuid });
            return result is bool active && active;
        }

        private static string GetPersistentTestResultsPath()
        {
            string company = PlayerSettings.companyName;
            string product = PlayerSettings.productName;
            string localLow = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData",
                "LocalLow");
            return Path.Combine(localLow, company, product, "TestResults.xml");
        }

        private static void TryWriteResultFromXml(string summaryPath, string xmlPath)
        {
            try
            {
                XDocument document = XDocument.Load(xmlPath);
                XElement root = document.Root;
                if (root == null)
                    throw new InvalidDataException("Missing test-run root node.");

                string result = root.Attribute("result")?.Value ?? "Failed";
                int total = ParseIntAttribute(root, "total");
                int passed = ParseIntAttribute(root, "passed");
                int failed = ParseIntAttribute(root, "failed");
                int skipped = ParseIntAttribute(root, "skipped");
                int inconclusive = ParseIntAttribute(root, "inconclusive");
                bool success = string.Equals(
                    result,
                    "Passed",
                    StringComparison.OrdinalIgnoreCase) &&
                    failed == 0 &&
                    inconclusive == 0;
                string summary =
                    $"{(success ? "PASS" : "FAIL")}|{DateTime.UtcNow:O}|" +
                    $"total={total};passed={passed};failed={failed};" +
                    $"skipped={skipped};inconclusive={inconclusive}";
                File.WriteAllText(summaryPath, summary);
                ClearPendingRunState();
                EditorApplication.Exit(success ? 0 : 1);
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    summaryPath,
                    $"FAIL|{DateTime.UtcNow:O}|Unable to parse {xmlPath}: {exception}");
                ClearPendingRunState();
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static int ParseIntAttribute(XElement element, string name)
        {
            string value = element.Attribute(name)?.Value;
            return int.TryParse(value, out int parsed) ? parsed : 0;
        }

        private static void ClearPendingRunState()
        {
            EditorApplication.update -= WatchPendingRun;
            SessionState.EraseString(PendingResultKey);
            SessionState.EraseString(PendingLabelKey);
            SessionState.EraseString(PendingTestNamesKey);
            SessionState.EraseString(PendingRunGuidKey);
            SessionState.EraseString(PendingRunModeKey);
            SessionState.EraseString(PendingRunStartedKey);
        }

        private sealed class BatchCallbacks : IErrorCallbacks
        {
            private readonly string _path;
            private readonly StringBuilder _failures = new StringBuilder();

            public BatchCallbacks(string path)
            {
                _path = path;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                File.WriteAllText(
                    _path,
                    $"STARTED|{DateTime.UtcNow:O}|{testsToRun?.TestCaseCount ?? 0}");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                bool passed = result != null &&
                              result.FailCount == 0 &&
                              result.InconclusiveCount == 0;
                string summary =
                    $"{(passed ? "PASS" : "FAIL")}|{DateTime.UtcNow:O}|" +
                    $"total={result?.PassCount + result?.FailCount + result?.SkipCount + result?.InconclusiveCount ?? 0};" +
                    $"passed={result?.PassCount ?? 0};" +
                    $"failed={result?.FailCount ?? 0};" +
                    $"skipped={result?.SkipCount ?? 0};" +
                    $"inconclusive={result?.InconclusiveCount ?? 0}";
                if (_failures.Length > 0)
                    summary += Environment.NewLine + _failures;
                File.WriteAllText(_path, summary);
                ClearPendingRunState();
                EditorApplication.Exit(passed ? 0 : 1);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result == null || result.Test == null || result.FailCount <= 0)
                    return;
                _failures.AppendLine(
                    $"{result.Test.FullName}|{result.Message}".Trim());
            }

            public void OnError(string message)
            {
                File.WriteAllText(
                    _path,
                    $"FAIL|{DateTime.UtcNow:O}|{message}");
                ClearPendingRunState();
                EditorApplication.Exit(1);
            }
        }
    }
}
