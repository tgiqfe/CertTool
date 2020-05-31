using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CertTool.OpenSSL
{
    public class OpensslFunction
    {
        private static OpensslPath opensslPath = null;
        private static OpensslCommand command = null;

        /// <summary>
        /// Opensslのバージョンを取得して返す
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            if (opensslPath == null) { opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY); }
            if (command == null) { command = new OpensslCommand(opensslPath); }

            return command.GetVersion();
        }

        /// <summary>
        /// ルートCA用証明書/鍵ファイルを作成
        /// </summary>
        /// <param name="rootCACrtFile"></param>
        /// <param name="rootCAKeyFile"></param>
        /// <param name="subject"></param>
        /// <param name="expireDays"></param>
        /// <param name="rsaBits"></param>
        public static void CreateRootCA(string rootCACrtFile, string rootCAKeyFile, string subject, int expireDays, int rsaBits)
        {
            if (opensslPath == null) { opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY); }
            if (command == null) { command = new OpensslCommand(opensslPath); }

            OpensslConfig config = new OpensslConfig();
            config.Default.RANDFILE = opensslPath.Rnd.Replace("\\", "/");
            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  未指定の場合にデフォルトパスにセット
            if (string.IsNullOrEmpty(rootCACrtFile)) { rootCACrtFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_ROOTCA_CRT_NAME); }
            if (string.IsNullOrEmpty(rootCAKeyFile)) { rootCAKeyFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_ROOTCA_KEY_NAME); }

            //  ランダムファイル「.rnd」作成
            command.CreateRandomFile();

            //  鍵ファイル作成
            command.CreateKeyFile(rootCAKeyFile, rsaBits);

            //  証明書ファイルを作成
            command.CreateCACrtFile(expireDays, rootCACrtFile, rootCAKeyFile, subject);
        }

        /// <summary>
        /// サーバ証明書の為のCSRを作成
        /// </summary>
        /// <param name="csrFile"></param>
        /// <param name="keyFile"></param>
        /// <param name="subject"></param>
        /// <param name="alternateNames"></param>
        /// <param name="rsaBits"></param>
        public static void CreateCSR(string csrFile, string keyFile, string subject, string[] alternateNames, int rsaBits)
        {
            if (opensslPath == null) { opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY); }
            if (command == null) { command = new OpensslCommand(opensslPath); }

            OpensslConfig config = new OpensslConfig();
            config.Default.RANDFILE = opensslPath.Rnd.Replace("\\", "/");

            //  マルチドメイン用設定 (引数から読み取り)
            //  https://rms.ne.jp/sslserver/csr/openssl.html/
            //  ver1.1.1から、-addextオプションでマルチドメイン設定ができるっぽい
            //  次バージョンあたりで検討よろしく
            //  https://kaede.jp/2018/06/10191854.html
            config.req.req_extensions = "v3_req";
            config.usr_cert.subjectAltName = "@alt_names";
            config.v3_req.basicConstraints = "CA:FALSE";
            config.v3_req.keyUsage = "nonRepudiation, digitalSignature, keyEncipherment";
            config.v3_req.subjectAltName = "@alt_names";

            if (alternateNames == null || alternateNames.Length == 0)
            {
                config.alt_names.DNS_altnames = new List<string>() { subject.Substring(subject.IndexOf("/CN=") + 4) };
            }
            else
            {
                Regex reg_ip = new Regex(@"^((\d\d?|1\d\d|2[0-4]\d|25[0-5])\.){3}(\d\d?|1\d\d|2[0-4]\d|25[0-5])$");
                config.alt_names.IP_altnames = new List<string>();
                config.alt_names.DNS_altnames = new List<string>();
                foreach (string altname in alternateNames)
                {
                    if (reg_ip.IsMatch(altname))
                    {
                        config.alt_names.IP_altnames.Add(altname);
                    }
                    else
                    {
                        config.alt_names.DNS_altnames.Add(altname);
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  未指定の場合にデフォルトパスにセット
            if (string.IsNullOrEmpty(csrFile)) { csrFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_SERVER_CSR_NAME); }
            if (string.IsNullOrEmpty(keyFile)) { keyFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_SERVER_KEY_NAME); }

            //  ランダムファイル「.rnd」作成
            command.CreateRandomFile();

            //  鍵ファイル作成
            command.CreateKeyFile(keyFile, rsaBits);

            //  CSRファイルを作成
            command.CreateCSRFile(csrFile, keyFile, subject);
        }

        /// <summary>
        /// CSRに署名して証明書を発行
        /// </summary>
        /// <param name="rootCACrtFile"></param>
        /// <param name="rootCAKeyFile"></param>
        /// <param name="csrFile"></param>
        /// <param name="crtFile"></param>
        /// <param name="expireDays"></param>
        public static void SignCertificate(string rootCACrtFile, string rootCAKeyFile, string csrFile, string crtFile, int expireDays)
        {
            if (opensslPath == null) { opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY); }
            if (command == null) { command = new OpensslCommand(opensslPath); }

            OpensslConfig config = new OpensslConfig();
            config.Default.RANDFILE = opensslPath.Rnd.Replace("\\", "/");
            config.CA_default.dir = opensslPath.Dir.Replace("\\", "/");
            config.CA_default.database = opensslPath.OpensslDB.Replace("\\", "/");
            config.CA_default.serial = opensslPath.Serial.Replace("\\", "/");

            //  マルチドメイン用設定 (CSRから読み取り)
            config.req.req_extensions = "v3_req";
            config.usr_cert.subjectAltName = "@alt_names";
            config.v3_req.basicConstraints = "CA:FALSE";
            config.v3_req.keyUsage = "nonRepudiation, digitalSignature, keyEncipherment";
            config.v3_req.subjectAltName = "@alt_names";

            string tempCsrText = command.ConvertToText(csrFile, true, false, false);
            using (StringReader sr = new StringReader(tempCsrText))
            {
                string readLine = "";
                while ((readLine = sr.ReadLine()) != null)
                {
                    if (readLine.Trim().EndsWith("Subject Alternative Name:"))
                    {
                        List<string> dnsList = new List<string>();
                        List<string> ipList = new List<string>();
                        foreach (string field in sr.ReadLine().Split(','))
                        {
                            string fieldStr = field.Trim();
                            if (fieldStr.StartsWith("DNS:"))
                            {
                                dnsList.Add(fieldStr.Substring(4));
                            }
                            else if (fieldStr.StartsWith("IP Address:"))
                            {
                                ipList.Add(fieldStr.Substring(11));
                            }
                        }
                        config.alt_names.DNS_altnames = dnsList;
                        config.alt_names.IP_altnames = ipList;
                        break;
                    }
                }
            }
            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  未指定の場合にデフォルトパスにセット
            if (string.IsNullOrEmpty(rootCACrtFile)) { rootCACrtFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_ROOTCA_CRT_NAME); }
            if (string.IsNullOrEmpty(rootCAKeyFile)) { rootCAKeyFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_ROOTCA_KEY_NAME); }
            if (string.IsNullOrEmpty(csrFile)) { csrFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_SERVER_CSR_NAME); }
            if (string.IsNullOrEmpty(crtFile)) { crtFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_SERVER_CRT_NAME); }

            //  ランダムファイル「.rnd」作成
            command.CreateRandomFile();

            //  CSRに署名して証明書を発行
            command.SignCrtFile(expireDays, rootCACrtFile, rootCAKeyFile, csrFile, crtFile);
        }

        /// <summary>
        /// 指定した証明書を破棄
        /// </summary>
        /// <param name="crtFile"></param>
        /// <param name="rootCACrtFile"></param>
        /// <param name="rootCAKeyFile"></param>
        public static void RevokeCertificate(string crtFile, string rootCACrtFile, string rootCAKeyFile)
        {
            if (opensslPath == null) { opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY); }
            if (command == null) { command = new OpensslCommand(opensslPath); }

            OpensslConfig config = new OpensslConfig();

            config.Default.RANDFILE = opensslPath.Rnd.Replace("\\", "/");
            config.CA_default.dir = opensslPath.Dir.Replace("\\", "/");
            config.CA_default.database = opensslPath.OpensslDB.Replace("\\", "/");
            config.CA_default.serial = opensslPath.Serial.Replace("\\", "/");

            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  未指定の場合にデフォルトパスにセット
            if (string.IsNullOrEmpty(crtFile)) { crtFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_SERVER_CSR_NAME); }
            if (string.IsNullOrEmpty(rootCACrtFile)) { rootCACrtFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_ROOTCA_CRT_NAME); }
            if (string.IsNullOrEmpty(rootCAKeyFile)) { rootCAKeyFile = Path.Combine(opensslPath.CertDir, Item.DEFAULT_ROOTCA_KEY_NAME); }

            //  ランダムファイル「.rnd」作成
            command.CreateRandomFile();

            //  証明書を破棄
            command.RevokeCertificate(crtFile, rootCACrtFile, rootCAKeyFile);
        }

        /// <summary>
        /// 設定ファイル「openssl.cnf」のバックアップ
        /// </summary>
        public static void BackupConf()
        {
            //OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            if (opensslPath == null) { opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY); }

            string bkFile = Path.Combine(
                opensslPath.BkDir,
                Path.GetFileNameWithoutExtension(opensslPath.Cnf) + "_" +
                    DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(opensslPath.Cnf));
            File.Copy(opensslPath.Cnf, bkFile, true);
        }

        /// <summary>
        /// CSR/証明書/鍵ファイルの中身を確認
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="isCsr"></param>
        /// <param name="isCrt"></param>
        /// <param name="isKey"></param>
        /// <returns></returns>
        public static string ConvertToText(string sourcePath, bool isCsr, bool isCrt, bool isKey)
        {
            if (opensslPath == null) { opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY); }
            if (command == null) { command = new OpensslCommand(opensslPath); }

            OpensslConfig config = new OpensslConfig();
            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            return command.ConvertToText(sourcePath, isCsr, isCrt, isKey);
        }

        /// <summary>
        /// CSRからサブジェクト代替名を取得
        /// </summary>
        /// <param name="csrFile"></param>
        /// <returns></returns>
        public static string[] GetAlternateNamesFromCsr(string csrFile)
        {
            if (opensslPath == null) { opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY); }
            if (command == null) { command = new OpensslCommand(opensslPath); }

            HashSet<string> altnameSets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string tempCsrText = command.ConvertToText(csrFile, true, false, false);
            using (StringReader sr = new StringReader(tempCsrText))
            {
                string subjectStr = "Subject:";
                string subjectAltNameStr = "Subject Alternative Name:";
                string dnsStr = "DNS:";
                string ipStr = "IP Address:";

                string readLine = "";
                while ((readLine = sr.ReadLine()) != null)
                {
                    string tempLine = readLine.Trim();

                    //  CN名を取得
                    if (tempLine.StartsWith(subjectStr))
                    {
                        foreach (string field in tempLine.Substring(subjectStr.Length).Split(','))
                        {
                            if (field.Trim().StartsWith("CN"))
                            {
                                altnameSets.Add(field.Substring(field.IndexOf("=") + 1).Trim());
                            }
                        }
                    }

                    //  サブジェクト代替名を取得
                    if (tempLine.EndsWith(subjectAltNameStr))
                    {
                        foreach (string field in sr.ReadLine().Split(','))
                        {
                            string fieldStr = field.Trim();
                            if (fieldStr.StartsWith(dnsStr))
                            {
                                altnameSets.Add(fieldStr.Substring(dnsStr.Length));
                            }
                            else if (fieldStr.StartsWith(ipStr))
                            {
                                altnameSets.Add(fieldStr.Substring(ipStr.Length));
                            }
                        }
                        break;
                    }
                }
            }

            return altnameSets.ToArray();
        }
    }
}
