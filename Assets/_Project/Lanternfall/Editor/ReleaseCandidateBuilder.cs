using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lanternfall.Editor
{
    public static class ReleaseCandidateBuilder
    {
        public const string Version = "1.0.0-rc.2";
        private const string OutputRoot = "Builds/WindowsRC";
        private const string ExecutablePath = OutputRoot + "/Lanternfall.exe";

        [Serializable]
        private sealed class ReleaseManifest
        {
            public string product;
            public string version;
            public string unityVersion;
            public string gitCommit;
            public string builtUtc;
            public string platform;
            public long reportedBuildBytes;
            public List<ReleaseFile> files = new List<ReleaseFile>();
        }

        [Serializable]
        private sealed class ReleaseFile
        {
            public string path;
            public long bytes;
            public string sha256;
        }

        [MenuItem("Lanternfall/Build Windows Release Candidate")]
        public static void BuildWindowsReleaseCandidate()
        {
            ConfigureProductionSettings();
            ReleaseReadinessValidator.ValidateOrThrow();
            if (Directory.Exists(OutputRoot))
                Directory.Delete(OutputRoot, true);
            Directory.CreateDirectory(OutputRoot);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EnabledScenes(),
                locationPathName = ExecutablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.StrictMode |
                    BuildOptions.CleanBuildCache
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException(
                    $"Release candidate failed: {report.summary.result}");

            ReleaseManifest manifest = CreateManifest(report.summary.totalSize);
            File.WriteAllText(
                Path.Combine(OutputRoot, "release-manifest.json"),
                JsonUtility.ToJson(manifest, true));
            Debug.Log(
                $"Lanternfall {Version} release candidate succeeded: " +
                $"{report.summary.totalSize} bytes, {manifest.files.Count} checksums");
        }

        private static void ConfigureProductionSettings()
        {
            PlayerSettings.companyName = "Lanternfall Studio";
            PlayerSettings.productName = "Lanternfall";
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.usePlayerLog = true;
            PlayerSettings.SetApplicationIdentifier(
                BuildTargetGroup.Standalone,
                "com.lanternfallstudio.lanternfall");
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
        }

        private static string[] EnabledScenes()
        {
            var paths = new List<string>(4);
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled) paths.Add(scene.path);
            return paths.ToArray();
        }

        private static ReleaseManifest CreateManifest(ulong buildBytes)
        {
            var manifest = new ReleaseManifest
            {
                product = PlayerSettings.productName,
                version = Version,
                unityVersion = Application.unityVersion,
                gitCommit = ResolveGitCommit(),
                builtUtc = DateTime.UtcNow.ToString("O"),
                platform = "Windows x86_64",
                reportedBuildBytes = checked((long)buildBytes)
            };
            string[] importantFiles =
            {
                "Lanternfall.exe",
                "UnityPlayer.dll",
                "UnityCrashHandler64.exe",
                "Lanternfall_Data/globalgamemanagers",
                "Lanternfall_Data/resources.assets"
            };
            foreach (string relative in importantFiles)
            {
                string path = Path.Combine(OutputRoot, relative);
                if (!File.Exists(path)) continue;
                var info = new FileInfo(path);
                manifest.files.Add(new ReleaseFile
                {
                    path = relative.Replace('\\', '/'),
                    bytes = info.Length,
                    sha256 = Sha256(path)
                });
            }
            if (manifest.files.Count < 3)
                throw new BuildFailedException(
                    "Release manifest could not locate core player files.");
            return manifest;
        }

        private static string ResolveGitCommit()
        {
            try
            {
                string head = File.ReadAllText(".git/HEAD").Trim();
                if (!head.StartsWith("ref: ", StringComparison.Ordinal))
                    return head;
                string reference = head.Substring(5);
                string looseRef = Path.Combine(".git", reference);
                if (File.Exists(looseRef)) return File.ReadAllText(looseRef).Trim();
                foreach (string line in File.ReadLines(".git/packed-refs"))
                    if (!line.StartsWith("#", StringComparison.Ordinal) &&
                        line.EndsWith(reference, StringComparison.Ordinal))
                        return line.Split(' ')[0];
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not resolve Git commit: {exception.Message}");
            }
            return "unknown";
        }

        private static string Sha256(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 algorithm = SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(stream))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
        }
    }
}
