using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CertTool
{
    class Function
    {
        /// <summary>
        /// ログ出力設定
        /// </summary>
        /// <param name="preName"></param>
        /// <returns></returns>
        public static Logger SetLogger(string logDir, string preName, bool debugMode)
        {
            if (!Directory.Exists(logDir)) { Directory.CreateDirectory(logDir); }

            string logPath = System.IO.Path.Combine(
                logDir,
                string.Format("{0}_{1}.log", preName, DateTime.Now.ToString("yyyyMMdd")));

            //  ファイル出力先設定
            FileTarget file = new FileTarget("File");
            file.Encoding = Encoding.GetEncoding("Shift_JIS");
            file.Layout = "[${longdate}][${windows-identity}][${uppercase:${level}}] ${message}";
            //file.Layout = "[${longdate}][${uppercase:${level}}] ${message}";
            file.FileName = logPath;

            //  コンソール出力設定
            ConsoleTarget console = new ConsoleTarget("Console");
            //console.Layout = "[${longdate}][${windows-identity}][${uppercase:${level}}] ${message}";
            console.Layout = "[${longdate}][${uppercase:${level}}] ${message}";

            LoggingConfiguration conf = new LoggingConfiguration();
            conf.AddTarget(file);
            conf.AddTarget(console);
            if (debugMode)
            {
                conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, file));
                conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, console));
            }
            else
            {
                conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, file));
            }
            LogManager.Configuration = conf;
            Logger logger = LogManager.GetCurrentClassLogger();

            return logger;
        }

        /// <summary>
        /// 埋め込みリソースを展開する。
        /// アセンブリのバージョンが同じならばスキップ。
        /// </summary>
        /// <param name="outputDir">展開先フォルダー</param>
        public static void ExpandEmbeddedResource(string outputDir)
        {
            //  現バージョン以外で展開済みの場合、フォルダーごと削除
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;

            string versionFile = Path.Combine(outputDir, string.Format("{0}_{1}_{2}_{3}.txt",
                ver.Major, ver.Minor, ver.Build, ver.Revision));
            if (!File.Exists(versionFile))
            {
                if (Directory.Exists(outputDir)) { Directory.Delete(outputDir, true); }
                Directory.CreateDirectory(outputDir);

                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                int excludeLength = (executingAssembly.GetName().Name + ".Embedded.").Length;
                foreach (string resourcePath in executingAssembly.GetManifestResourceNames())
                {
                    string outputFile = Path.Combine(outputDir, resourcePath.Substring(excludeLength));
                    using (Stream stream = executingAssembly.GetManifestResourceStream(resourcePath))
                    using (BinaryReader reader = new BinaryReader(stream))
                    using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(outputFile)))
                    {
                        writer.Write(reader.ReadBytes((int)stream.Length), 0, (int)stream.Length);
                    }
                }
                File.Create(versionFile).Close();
            }
        }

        /// <summary>
        /// baseDirからtargetPathを相対パスで辿った結果を絶対パスで返す
        /// targetPathが絶対パスの場合は、targetPathを返す
        /// targetPathがnull/emptyの場合、baseDirを返す
        /// </summary>
        /// <param name="baseDir"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static string RelatedToAbsolutePath(string baseDir, string targetPath)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                return baseDir;
            }
            else
            {
                int len = targetPath.Length;
                if ((2 <= len &&
                    (targetPath[0] == Path.DirectorySeparatorChar || targetPath[0] == Path.AltDirectorySeparatorChar) &&
                    (targetPath[1] == Path.DirectorySeparatorChar || targetPath[1] == Path.AltDirectorySeparatorChar)) ||
                    (3 <= len &&
                    targetPath[1] == Path.VolumeSeparatorChar && (targetPath[2] == Path.DirectorySeparatorChar || targetPath[2] == Path.AltDirectorySeparatorChar)))
                {
                    return targetPath;
                }
                else
                {
                    return new Uri(
                        new Uri(baseDir.EndsWith("\\") ? baseDir : $"{baseDir}\\"), targetPath).LocalPath;
                }
            }
        }
    }
}

