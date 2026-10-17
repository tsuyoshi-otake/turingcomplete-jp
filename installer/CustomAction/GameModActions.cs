using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WixToolset.Dtf.WindowsInstaller;

namespace TuringCompleteJpCA
{
    public class GameModActions
    {
        const string BackupMarkerFile = ".jp-mod-installed";
        const string GameExeName = "Turing Complete.exe";
        const string TranslationFile = "Japanese.txt";
        const string TranslationsDir = "translations";

        static void Log(Session session, string msg)
        {
            try { session.Log(msg); } catch { }
            try
            {
                string logPath = Path.Combine(Path.GetTempPath(), "turingcomplete-jp-ca.log");
                File.AppendAllText(logPath, DateTime.Now.ToString("HH:mm:ss") + " " + msg + Environment.NewLine);
            }
            catch { }
        }

        // ── Immediate CA: ゲーム検出 ─────────────────────────────

        [CustomAction]
        public static ActionResult FindGameDirectory(Session session)
        {
            Log(session, "FindGameDirectory: Searching...");

            string gameDir = FindSteamGameDir(session);
            if (gameDir != null)
            {
                session["GAMEDIR"] = gameDir;
                Log(session, "FindGameDirectory: Found at " + gameDir);
            }
            else
            {
                Log(session, "FindGameDirectory: Not found");
            }

            return ActionResult.Success;
        }

        // ── Immediate CA: インストール ───────────────────────────

        [CustomAction]
        public static ActionResult InstallMod(Session session)
        {
            try
            {
                Log(session, "InstallMod: Starting");

                string gameDir = session["GAMEDIR"];
                Log(session, "InstallMod: gameDir = " + gameDir);

                string translationsPath = Path.Combine(gameDir, TranslationsDir);
                string backupDir = Path.Combine(gameDir, ".jp-mod-backup");

                // 埋め込みリソース取得
                Assembly resAssembly = FindResourceAssembly();
                string resourceName = FindTranslationResource(resAssembly);
                Log(session, "InstallMod: Resource = " + (resourceName ?? "null"));

                if (resourceName == null)
                {
                    Log(session, "InstallMod: Translation resource not found");
                    return ActionResult.Failure;
                }

                byte[] modContent;
                using (var stream = resAssembly.GetManifestResourceStream(resourceName))
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    modContent = ms.ToArray();
                }

                // [1] バックアップ（公式ファイルだけを残す。MOD 入りのファイルで上書きしない）
                Directory.CreateDirectory(backupDir);
                string origFile = Path.Combine(translationsPath, TranslationFile);
                string backupFile = Path.Combine(backupDir, TranslationFile);
                if (File.Exists(origFile))
                {
                    BackupDecision decision = DecideBackup(File.ReadAllBytes(origFile), modContent, File.Exists(backupFile));
                    if (decision == BackupDecision.Overwrite)
                    {
                        File.Copy(origFile, backupFile, true);
                        Log(session, "InstallMod: Backed up " + origFile);
                    }
                    else
                    {
                        Log(session, "InstallMod: Kept existing backup (" + decision + ")");
                    }
                }

                // [2] 翻訳ファイルコピー
                File.WriteAllBytes(origFile, modContent);
                Log(session, "InstallMod: Installed " + TranslationFile);

                // [3] 言語設定を Japanese に変更
                UpdateLanguageSetting("Japanese", session);

                // [4] マーカー書き込み
                string marker = string.Format(
                    "{{\n  \"GameDir\": \"{0}\",\n  \"InstalledAt\": \"{1}\"\n}}",
                    gameDir.Replace("\\", "\\\\"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                File.WriteAllText(Path.Combine(backupDir, BackupMarkerFile), marker, Encoding.UTF8);

                Log(session, "InstallMod: Complete");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                Log(session, "InstallMod: FAILED - " + ex);
                return ActionResult.Failure;
            }
        }

        // ── Immediate CA: アンインストール ───────────────────────

        [CustomAction]
        public static ActionResult UninstallMod(Session session)
        {
            try
            {
                Log(session, "UninstallMod: Starting");

                string gameDir;
                try { gameDir = session["GAMEDIR"]; } catch { gameDir = null; }

                // GAMEDIR がない場合はマーカーから探す
                if (string.IsNullOrEmpty(gameDir))
                    gameDir = FindGameDirFromMarker();

                if (string.IsNullOrEmpty(gameDir))
                {
                    Log(session, "UninstallMod: No game dir, skip");
                    return ActionResult.Success;
                }

                string backupDir = Path.Combine(gameDir, ".jp-mod-backup");
                string markerPath = Path.Combine(backupDir, BackupMarkerFile);

                if (!File.Exists(markerPath))
                {
                    Log(session, "UninstallMod: No marker, skip");
                    return ActionResult.Success;
                }

                // バックアップから復元
                string backupFile = Path.Combine(backupDir, TranslationFile);
                string origFile = Path.Combine(gameDir, TranslationsDir, TranslationFile);
                if (File.Exists(backupFile))
                {
                    File.Copy(backupFile, origFile, true);
                    Log(session, "UninstallMod: Restored " + TranslationFile);
                }

                // 言語設定を空文字（デフォルト）に戻す
                UpdateLanguageSetting("", session);

                // バックアップディレクトリ削除
                try { Directory.Delete(backupDir, true); } catch { }

                Log(session, "UninstallMod: Complete");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                Log(session, "UninstallMod: FAILED - " + ex);
                return ActionResult.Failure;
            }
        }

        // ── ヘルパー ─────────────────────────────────────────

        internal enum BackupDecision
        {
            /// <summary>現在のファイルをバックアップする（公式ファイル、またはバックアップがまだない）</summary>
            Overwrite,
            /// <summary>現在のファイルはこの MOD と同一（再インストール／修復）</summary>
            KeepCurrentIsThisMod,
            /// <summary>公式ファイルとは判定できず、既にバックアップがある（旧版 MOD の可能性）</summary>
            KeepUnknownWithExistingBackup,
        }

        /// <summary>
        /// 現在の Japanese.txt をバックアップしてよいかを判定する (#5)。
        /// ゲーム 2.1.334 以降の公式ファイルは "MISSING TRANSLATION" などのブロック見出しを含む。MOD のファイルは含まない。
        /// Steam の更新で公式ファイルに戻っていれば、古いバックアップを新しい公式ファイルで更新する。
        /// </summary>
        internal static BackupDecision DecideBackup(byte[] current, byte[] modContent, bool backupExists)
        {
            if (current.SequenceEqual(modContent))
                return BackupDecision.KeepCurrentIsThisMod;
            if (LooksOfficial(current) || !backupExists)
                return BackupDecision.Overwrite;
            return BackupDecision.KeepUnknownWithExistingBackup;
        }

        static readonly Regex OfficialBlockHeader = new Regex(
            @"^=+ (MISSING|OUTDATED|FINISHED) TRANSLATION =+\r?$", RegexOptions.Multiline);

        static bool LooksOfficial(byte[] content)
        {
            return OfficialBlockHeader.IsMatch(Encoding.UTF8.GetString(content));
        }

        static string FindSteamGameDir(Session session)
        {
            var candidates = new List<string>
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Turing Complete",
                @"C:\Program Files\Steam\steamapps\common\Turing Complete",
                @"D:\SteamLibrary\steamapps\common\Turing Complete",
                @"E:\SteamLibrary\steamapps\common\Turing Complete",
            };

            var vdfPaths = new[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\libraryfolders.vdf",
                @"C:\Program Files\Steam\steamapps\libraryfolders.vdf",
            };

            foreach (string vdf in vdfPaths)
            {
                if (!File.Exists(vdf)) continue;
                try
                {
                    string content = File.ReadAllText(vdf);
                    foreach (Match m in Regex.Matches(content, "\"path\"\\s+\"([^\"]+)\""))
                    {
                        string libPath = m.Groups[1].Value.Replace("\\\\", "\\");
                        string candidate = Path.Combine(libPath, "steamapps", "common", "Turing Complete");
                        if (!candidates.Contains(candidate))
                            candidates.Add(candidate);
                    }
                }
                catch (Exception ex)
                {
                    Log(session, "vdf parse error: " + ex.Message);
                }
            }

            foreach (string dir in candidates)
            {
                if (File.Exists(Path.Combine(dir, GameExeName)))
                    return dir;
            }
            return null;
        }

        static string FindGameDirFromMarker()
        {
            // 既知のパスからマーカーを探す
            var candidates = new[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Turing Complete",
                @"C:\Program Files\Steam\steamapps\common\Turing Complete",
                @"D:\SteamLibrary\steamapps\common\Turing Complete",
                @"E:\SteamLibrary\steamapps\common\Turing Complete",
            };

            foreach (string dir in candidates)
            {
                string markerPath = Path.Combine(dir, ".jp-mod-backup", BackupMarkerFile);
                if (File.Exists(markerPath))
                    return dir;
            }
            return null;
        }

        static Assembly FindResourceAssembly()
        {
            var asm = Assembly.GetExecutingAssembly();
            if (asm.GetManifestResourceNames().Any(n => n.Contains(".mod.")))
                return asm;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.GetManifestResourceNames().Any(n => n.Contains(".mod.")))
                    return a;
            }
            return asm;
        }

        static string FindTranslationResource(Assembly assembly)
        {
            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith(".Japanese.txt") || name.EndsWith(".translations.Japanese.txt"))
                    return name;
            }
            return null;
        }

        static void UpdateLanguageSetting(string language, Session session)
        {
            try
            {
                int rows = GameLanguageSetting.Write(language);
                Log(session, "UpdateLanguageSetting: Set to '" + language + "' (" + rows + " rows)");
            }
            catch (FileNotFoundException)
            {
                Log(session, "UpdateLanguageSetting: progress.dat not found at " + GameLanguageSetting.DbPath);
            }
            catch (Exception ex)
            {
                Log(session, "UpdateLanguageSetting: FAILED - " + ex.Message);
            }
        }
    }
}
