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
        /// <summary>
        /// Opensslのバージョンを取得して返す
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
            return command.GetVersion();
        }

        /// <summary>
        /// ルートCA用証明書/鍵ファイルを作成
        /// </summary>
        /// <param name="caCrtFile"></param>
        /// <param name="caKeyFile"></param>
        /// <param name="subject"></param>
        /// <param name="expireDays"></param>
        /// <param name="rsaBits"></param>
        public static void CreateRootCA(string caCrtFile, string caKeyFile, string subject, int expireDays, int rsaBits)
        {
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
            OpensslConfig config = new OpensslConfig();

            config.Default.RANDFILE = opensslPath.Rnd.Replace("\\", "/");
            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  ランダムファイル「.rnd」作成
            command.CreateRandomFile();

            //  鍵ファイル作成
            command.CreateKeyFile(caKeyFile, rsaBits);

            //  証明書ファイルを作成
            command.CreateCACrtFile(expireDays, caCrtFile, caKeyFile, subject);
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
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
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
        /// <param name="caCrtFile"></param>
        /// <param name="caKeyFile"></param>
        /// <param name="csrFile"></param>
        /// <param name="crtFile"></param>
        /// <param name="expireDays"></param>
        public static void SignCertificate(string caCrtFile, string caKeyFile, string csrFile, string crtFile, int expireDays)
        {
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
            OpensslConfig config = new OpensslConfig();

            config.Default.RANDFILE = opensslPath.Rnd.Replace("\\", "/");
            config.CA_default.dir = opensslPath.Dir.Replace("\\", "/");
            config.CA_default.database = opensslPath.Db.Replace("\\", "/");
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

            //  ランダムファイル「.rnd」作成
            command.CreateRandomFile();

            //  CSRに署名して証明書を発行
            command.SignCrtFile(expireDays, caCrtFile, caKeyFile, csrFile, crtFile);
        }

        /// <summary>
        /// 指定した証明書を破棄
        /// </summary>
        /// <param name="crtFile"></param>
        /// <param name="caCrtFile"></param>
        /// <param name="caKeyFile"></param>
        public static void RevokeCertificate(string crtFile, string caCrtFile, string caKeyFile)
        {
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
            OpensslConfig config = new OpensslConfig();

            config.Default.RANDFILE = opensslPath.Rnd.Replace("\\", "/");
            config.CA_default.dir = opensslPath.Dir.Replace("\\", "/");
            config.CA_default.database = opensslPath.Db.Replace("\\", "/");
            config.CA_default.serial = opensslPath.Serial.Replace("\\", "/");

            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  ランダムファイル「.rnd」作成
            command.CreateRandomFile();

            //  証明書を破棄
            command.RevokeCertificate(crtFile, caCrtFile, caKeyFile);
        }

        /// <summary>
        /// 設定ファイル「openssl.cnf」のバックアップ
        /// </summary>
        public static void BackupConf()
        {
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            File.Copy(
                opensslPath.Cnf,
                Path.Combine(
                    opensslPath.BkDir,
                    Path.GetFileNameWithoutExtension(opensslPath.Cnf) + "_" + 
                        DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(opensslPath.Cnf)), true);
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
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
            OpensslConfig config = new OpensslConfig();
            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            return command.ConvertToText(sourcePath, isCsr, isCrt, isKey);
        }
    }
}
