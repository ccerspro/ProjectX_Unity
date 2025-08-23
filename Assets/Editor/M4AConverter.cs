#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace ZhouSoftware.Editor
{
    public static class M4AConverter
    {
        private enum TargetFormat { Ogg, Wav, Mp3 }

        [MenuItem("Tools/Audio/Convert .m4a to Audio…")]
        private static void ConvertM4A()
        {
            // 1) 选择根目录
            string folder = EditorUtility.OpenFolderPanel("Select folder that contains .m4a", Application.dataPath, "");
            if (string.IsNullOrEmpty(folder)) return;

            // 2) 选择目标格式
            int choice = EditorUtility.DisplayDialogComplex(
                "Choose Target Format",
                "Convert .m4a files to which format?",
                "OGG (Vorbis)",            // 0
                "WAV (PCM/ADPCM)",         // 1
                "MP3 (LAME)"               // 2
            );
            if (choice < 0) return;

            TargetFormat format = choice == 0 ? TargetFormat.Ogg : (choice == 1 ? TargetFormat.Wav : TargetFormat.Mp3);

            // 3) 定位 ffmpeg
            string ffmpegPath = FindFfmpegOnPath();
            if (string.IsNullOrEmpty(ffmpegPath))
            {
                ffmpegPath = EditorUtility.OpenFilePanel("Locate ffmpeg executable", "", "");
                if (string.IsNullOrEmpty(ffmpegPath))
                {
                    EditorUtility.DisplayDialog("ffmpeg not found", "You must install ffmpeg and select its executable.", "OK");
                    return;
                }
            }

            // 4) 扫描 m4a
            string[] m4aFiles;
            try
            {
                m4aFiles = Directory.GetFiles(folder, "*.m4a", SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", "Scan failed:\n" + e.Message, "OK");
                return;
            }

            if (m4aFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("No .m4a found", "This folder has no .m4a files.", "OK");
                return;
            }

            int ok = 0, fail = 0;
            List<string> importedAssetPaths = new List<string>();

            try
            {
                EditorUtility.DisplayProgressBar("Converting", "Preparing…", 0f);

                for (int i = 0; i < m4aFiles.Length; i++)
                {
                    string inPath = m4aFiles[i];
                    string outPath = ReplaceExtension(inPath, format);

                    EditorUtility.DisplayProgressBar(
                        "Converting",
                        $"{Path.GetFileName(inPath)} → {Path.GetFileName(outPath)}",
                        (float)i / m4aFiles.Length
                    );

                    Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                    string args = BuildFfmpegArgs(inPath, outPath, format);

                    bool success = RunProcess(ffmpegPath, args, out string stdout, out string stderr);
                    if (success && File.Exists(outPath))
                    {
                        ok++;

                        // 如果输出在 Assets 目录树内，则记录以便设置导入参数
                        string unityRelative = ToUnityAssetPath(outPath);
                        if (!string.IsNullOrEmpty(unityRelative))
                            importedAssetPaths.Add(unityRelative);
                    }
                    else
                    {
                        fail++;
                        UnityEngine.Debug.LogError($"FFmpeg failed for {inPath}\n{stderr}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // 5) 刷新资源
            AssetDatabase.Refresh();

            // 6) 为新资源设置导入参数（基于路径启发式）
            foreach (var assetPath in importedAssetPaths)
            {
                TrySetupImporter(assetPath, format);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Done", $"Converted: {ok}\nFailed: {fail}", "OK");
        }

        private static string BuildFfmpegArgs(string inPath, string outPath, TargetFormat fmt)
        {
            // 注意：一定要用双引号包裹路径，避免空格问题
            switch (fmt)
            {
                case TargetFormat.Ogg:
                    // qscale 0~10，4~5 通常够用
                    return $"-y -i \"{inPath}\" -vn -c:a libvorbis -qscale:a 4 \"{outPath}\"";
                case TargetFormat.Wav:
                    // 16-bit PCM；若要体积更小可改 ADPCM：-c:a adpcm_ima_wav
                    return $"-y -i \"{inPath}\" -vn -c:a pcm_s16le \"{outPath}\"";
                case TargetFormat.Mp3:
                    // LAME VBR，高品质约 V2
                    return $"-y -i \"{inPath}\" -vn -c:a libmp3lame -q:a 2 \"{outPath}\"";
                default:
                    throw new ArgumentOutOfRangeException(nameof(fmt));
            }
        }

        private static string ReplaceExtension(string inPath, TargetFormat fmt)
        {
            string ext = fmt == TargetFormat.Ogg ? ".ogg" : (fmt == TargetFormat.Wav ? ".wav" : ".mp3");
            return Path.ChangeExtension(inPath, ext);
        }

        private static bool RunProcess(string file, string args, out string stdout, out string stderr)
        {
            var psi = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            try
            {
                using (var p = Process.Start(psi))
                {
                    stdout = p.StandardOutput.ReadToEnd();
                    stderr = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch (Exception e)
            {
                stdout = "";
                stderr = e.Message;
                return false;
            }
        }

        private static string FindFfmpegOnPath()
        {
            // 尝试直接调用 "ffmpeg -version"
            if (RunProcess("ffmpeg", "-version", out var _, out var _))
                return "ffmpeg";

            // 常见安装路径的简单探测（可按需补充）
#if UNITY_EDITOR_WIN
            string[] candidates =
            {
                @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
                @"C:\ProgramData\chocolatey\bin\ffmpeg.exe",
            };
#else
            string[] candidates =
            {
                "/usr/local/bin/ffmpeg",
                "/opt/homebrew/bin/ffmpeg",
                "/usr/bin/ffmpeg"
            };
#endif
            foreach (var c in candidates)
                if (File.Exists(c)) return c;

            return null;
        }

        private static string ToUnityAssetPath(string absolutePath)
        {
            // 仅当文件位于项目 Assets 下时，返回 "Assets/..." 形式
            string projectPath = Path.GetFullPath(Application.dataPath + "/..").Replace('\\', '/');
            string abs = Path.GetFullPath(absolutePath).Replace('\\', '/');
            if (abs.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                string rel = abs.Substring(projectPath.Length + 1);
                return rel.Replace('\\', '/');
            }
            return null;
        }

        private static void TrySetupImporter(string assetPath, TargetFormat fmt)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (importer == null) return;

            bool isSFX = assetPath.IndexOf("/SFX/", StringComparison.OrdinalIgnoreCase) >= 0;

            // 读取默认 SampleSettings
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;

            // ——常规建议——
            // 短音效（SFX）：走 DecompressOnLoad，预加载 = true（零延迟，耗内存）
            // 长/大文件（BGM/环境）：走 Streaming 或 CompressedInMemory，预加载 = false
            if (isSFX)
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = (fmt == TargetFormat.Wav)
                    ? AudioCompressionFormat.PCM
                    : AudioCompressionFormat.ADPCM;
                settings.quality = 1f; // 对 ADPCM 无效，但保留
                settings.preloadAudioData = true;     // ← 改到这里
                importer.forceToMono = true;
            }
            else
            {
                settings.loadType = AudioClipLoadType.CompressedInMemory;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.5f;
                settings.preloadAudioData = false;    // ← 改到这里
                importer.forceToMono = false;
            }

            // 大文件（> 5MB）用 Streaming，并确保不预加载
            try
            {
                string full = System.IO.Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                if (File.Exists(full) && new FileInfo(full).Length > 5 * 1024 * 1024)
                {
                    settings.loadType = AudioClipLoadType.Streaming;
                    settings.preloadAudioData = false;
                }
            }
            catch { /* ignore */ }

            importer.defaultSampleSettings = settings;   // 应用默认设置

            // （可选）按平台单独覆盖示例：
            // var ios = settings; ios.preloadAudioData = false;
            // importer.SetOverrideSampleSettings("iOS", ios);
            // var android = settings; android.preloadAudioData = false;
            // importer.SetOverrideSampleSettings("Android", android);

            importer.SaveAndReimport();
        }
    }
}
#endif