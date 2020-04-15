using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace CertTool.OpenSSL
{
    class OpensslCommand
    {
        /// <summary>
        /// OpenSSLコマンドを実行する為のプライベートメソッド
        /// </summary>
        /// <param name="arguments"></param>
        private static void Run(string arguments)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = Item.OpenSSLPath.Exe;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = false;
                proc.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
                proc.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
        }

        /// <summary>
        /// OpenSSLコマンドを実行して、出力内容をStringBuilderに格納する
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="sb"></param>
        private static void Run(string arguments, StringBuilder sb)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = Item.OpenSSLPath.Exe;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = false;
                proc.OutputDataReceived += (sender, e) => { sb.AppendLine(e.Data); };
                proc.ErrorDataReceived += (sender, e) => { sb.AppendLine(e.Data); };
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
        }

        /// <summary>
        /// Opensslのバージョンを表示
        /// </summary>
        public static void GetVersion()
        {
            Run("version");
        }

        /// <summary>
        /// ランダムファイル「.rnd」を作成するコマンドを実行
        /// </summary>
        public static void CreateRnadomFile()
        {
            if (File.Exists(Item.OpenSSLPath.Rnd))
            {
                return;
            }
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            Run(string.Format(
                "rand -out \"{0}\" -rand \"{1}\" 1024",
                Item.OpenSSLPath.Rnd,
                assemblyPath
                ));
        }

        /// <summary>
        /// 鍵ファイル作成
        /// </summary>
        /// <param name="keyFile"></param>
        /// <param name="rsaBits"></param>
        public static void CreateKeyFile(string keyFile, int rsaBits)
        {
            Run(string.Format(
                "genrsa -out \"{0}\" {1}",
                keyFile, rsaBits
                ));
        }

        /// <summary>
        /// ルートCA用証明書ファイル作成
        /// </summary>
        /// <param name="expireDays"></param>
        /// <param name="caCrtFile"></param>
        /// <param name="caKeyFile"></param>
        /// <param name="subject"></param>
        public static void CreateCACrtFile(int expireDays, string caCrtFile, string caKeyFile, string subject)
        {
            Run(string.Format(
                "req -new -x509 -sha256 -config \"{0}\" -days {1} -out \"{2}\" -key \"{3}\" -subj \"{4}\"",
                Item.OpenSSLPath.Cnf,
                expireDays,
                caCrtFile,
                caKeyFile,
                subject
                ));
        }

        /// <summary>
        ///     CSRファイル作成
        /// </summary>
        /// <param name="csrFile"></param>
        /// <param name="keyFIle"></param>
        /// <param name="subject"></param>
        public static void CreateCSRFile(string csrFile, string keyFile, string subject)
        {
            Run(string.Format(
                "req -new -sha256 -config \"{0}\" -out \"{1}\" -key \"{2}\" -subj \"{3}\"",
                Item.OpenSSLPath.Cnf,
                csrFile,
                keyFile,
                subject
                ));
        }

        /// <summary>
        /// 証明書ファイルに署名
        /// </summary>
        /// <param name="expireDays"></param>
        /// <param name="caCrtFile"></param>
        /// <param name="caKeyFile"></param>
        /// <param name="csrFile"></param>
        /// <param name="crtFile"></param>
        public static void SignCrtFile(int expireDays, string caCrtFile, string caKeyFile, string csrFile, string crtFile)
        {
            //  空のデータベースファイル「index.txt」を作成
            if (!File.Exists(Item.OpenSSLPath.Db))
            {
                File.CreateText(Item.OpenSSLPath.Db);
            }

            //  シリアルファイルを作成し、00 を記述。
            if (!File.Exists(Item.OpenSSLPath.Serial))
            {
                using (StreamWriter sw = new StreamWriter(Item.OpenSSLPath.Serial, false, new UTF8Encoding(false)))
                {
                    sw.WriteLine("00");
                }
            }

            Run(string.Format(
                "ca -config \"{0}\" -days {1} -cert \"{2}\" -keyfile \"{3}\" -in \"{4}\" -out \"{5}\" -outdir \"{6}\" -batch",
                Item.OpenSSLPath.Cnf,
                expireDays,
                caCrtFile,
                caKeyFile,
                csrFile,
                crtFile,
                Item.OpenSSLPath.Dir
                ));
        }

        /// <summary>
        /// CSR/証明書/鍵ファイルを開いて中身を表示する。
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="isCsr"></param>
        /// <param name="isCrt"></param>
        /// <param name="isKey"></param>
        public static string ConvertToText(string sourcePath, bool isCsr, bool isCrt, bool isKey)
        {
            StringBuilder sb = new StringBuilder();

            if (!isCsr && !isCrt && !isKey)
            {
                switch (Path.GetExtension(sourcePath).ToLower())
                {
                    case ".csr":
                        isCsr = true;
                        break;
                    case ".crt":
                    case ".cert":
                        isCrt = true;
                        break;
                    case ".key":
                        isKey = true;
                        break;
                }
            }

            if (isCsr)
            {
                Run(string.Format(
                    "req -text -noout -in \"{0}\" -config \"{1}\"",
                    sourcePath,
                    Item.OpenSSLPath.Cnf
                    ), sb);
            }
            if (isCrt)
            {
                Run(string.Format(
                    "x509 -text -noout -in \"{0}\"",
                    sourcePath
                    ), sb);
            }
            if (isKey)
            {
                Run(string.Format(
                    "rsa -text -noout -in \"{0}\"",
                    sourcePath
                    ), sb);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 対象の証明書ファイルの情報を破棄/無効化
        /// </summary>
        /// <param name="crtFile"></param>
        /// <param name="caCrtFile"></param>
        /// <param name="caKeyFile"></param>
        public static void RevokeCertificate(string crtFile, string caCrtFile, string caKeyFile)
        {
            Run(string.Format(
                "ca -revoke \"{0}\" -cert \"{1}\" -keyfile \"{2}\" -config \"{3}\"",
                crtFile,
                caCrtFile,
                caKeyFile,
                Item.OpenSSLPath.Cnf
                ));
        }
    }
}
