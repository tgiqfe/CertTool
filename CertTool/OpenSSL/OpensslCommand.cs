using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.IO.Compression;

namespace CertTool.OpenSSL
{
    public class OpensslCommand
    {
        private OpensslPath _opensslPath = null;

        public OpensslCommand() { }
        public OpensslCommand(OpensslPath opensslPath)
        {
            this._opensslPath = opensslPath;

            //  埋め込みリソースを展開
            Function.ExpandEmbeddedResource(_opensslPath.Base);
            if (!Directory.Exists(_opensslPath.Dir))
            {
                ZipFile.ExtractToDirectory(_opensslPath.Zip, opensslPath.Dir);
            }
        }

        /// <summary>
        /// OpenSSLコマンドを実行する為のプライベートメソッド
        /// </summary>
        /// <param name="arguments"></param>
        private void Run(string arguments)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = _opensslPath.Exe;
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
        private void Run(string arguments, StringBuilder sb)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = _opensslPath.Exe;
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
        public string GetVersion()
        {
            StringBuilder sb = new StringBuilder();
            Run("version", sb);

            return sb.ToString();
        }

        /// <summary>
        /// ランダムファイル「.rnd」を作成するコマンドを実行
        /// </summary>
        public void CreateRandomFile()
        {
            if (File.Exists(_opensslPath.Rnd))
            {
                return;
            }
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            Run(string.Format(
                "rand -out \"{0}\" -rand \"{1}\" 1024",
                _opensslPath.Rnd,
                assemblyPath
                ));
        }

        /// <summary>
        /// 鍵ファイル作成
        /// </summary>
        /// <param name="keyFile"></param>
        /// <param name="rsaBits"></param>
        public void CreateKeyFile(string keyFile, int rsaBits)
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
        public void CreateCACrtFile(int expireDays, string caCrtFile, string caKeyFile, string subject)
        {
            Run(string.Format(
                "req -new -x509 -sha256 -config \"{0}\" -days {1} -out \"{2}\" -key \"{3}\" -subj \"{4}\"",
                _opensslPath.Cnf,
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
        public void CreateCSRFile(string csrFile, string keyFile, string subject)
        {
            Run(string.Format(
                "req -new -sha256 -config \"{0}\" -out \"{1}\" -key \"{2}\" -subj \"{3}\"",
                _opensslPath.Cnf,
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
        public void SignCrtFile(int expireDays, string caCrtFile, string caKeyFile, string csrFile, string crtFile)
        {
            //  空のデータベースファイル「index.txt」を作成
            if (!File.Exists(_opensslPath.Db))
            {
                File.CreateText(_opensslPath.Db);
            }

            //  シリアルファイルを作成し、00 を記述。
            if (!File.Exists(_opensslPath.Serial))
            {
                using (StreamWriter sw = new StreamWriter(_opensslPath.Serial, false, new UTF8Encoding(false)))
                {
                    sw.WriteLine("00");
                }
            }

            Run(string.Format(
                "ca -config \"{0}\" -days {1} -cert \"{2}\" -keyfile \"{3}\" -in \"{4}\" -out \"{5}\" -outdir \"{6}\" -batch",
                _opensslPath.Cnf,
                expireDays,
                caCrtFile,
                caKeyFile,
                csrFile,
                crtFile,
                _opensslPath.Dir
                ));
        }

        /// <summary>
        /// CSR/証明書/鍵ファイルを開いて中身を表示する。
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="isCsr"></param>
        /// <param name="isCrt"></param>
        /// <param name="isKey"></param>
        public string ConvertToText(string sourcePath, bool isCsr, bool isCrt, bool isKey)
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
                    _opensslPath.Cnf
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
        /// サーバー証明書 + 中間証明書 + ルート証明書を結合
        /// </summary>
        /// <param name="serverCert"></param>
        /// <param name="chainCert"></param>
        /// <param name="rootCert"></param>
        public string JoinCertificates(string serverCert, string chainCert, string rootCert)
        {
            StringBuilder sb = new StringBuilder();
            Action<string> AppendingCert = (file) =>
            {
                if (File.Exists(file))
                {
                    using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
                    {
                        sb.Append(sr.ReadToEnd());
                    }
                }
            };
            AppendingCert(serverCert);
            AppendingCert(chainCert);
            AppendingCert(rootCert);

            return sb.ToString();
        }

        /// <summary>
        /// 対象の証明書ファイルの情報を破棄/無効化
        /// </summary>
        /// <param name="crtFile"></param>
        /// <param name="caCrtFile"></param>
        /// <param name="caKeyFile"></param>
        public void RevokeCertificate(string crtFile, string caCrtFile, string caKeyFile)
        {
            Run(string.Format(
                "ca -revoke \"{0}\" -cert \"{1}\" -keyfile \"{2}\" -config \"{3}\"",
                crtFile,
                caCrtFile,
                caKeyFile,
                _opensslPath.Cnf
                ));
        }
    }
}
