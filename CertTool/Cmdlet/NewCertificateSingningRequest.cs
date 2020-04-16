using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Diagnostics;
using CertTool.OpenSSL;
using System.Text.RegularExpressions;

namespace CertTool.Cmdlet
{
    [Cmdlet(VerbsCommon.New, "CertificateSingningRequest")]
    public class NewCertificateSingningRequest : PSCmdlet
    {
        [Parameter]
        public string CsrFile { get; set; } = "server.csr";
        [Parameter]
        public string KeyFile { get; set; } = "server.key";
        [Parameter]
        public string Subject { get; set; }
        [Parameter]
        public string[] AlternateNames { get; set; }
        [Parameter]
        public int RsaBits { get; set; } = 4096;

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;

            /*
            Item.OpenSSLPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            Function.ExpandEmbeddedResource(Item.OpenSSLPath.Base);
            if (!Directory.Exists(Item.OpenSSLPath.Dir))
            {
                Function.ExtractZipFile(Item.OpenSSLPath.Zip, Item.OpenSSLPath.Dir);
            }
            */
        }

        protected override void ProcessRecord()
        {
            OpensslFunction.CreateCSR(CsrFile, KeyFile, Subject, AlternateNames, RsaBits);

            /*
            OpensslConfig config = new OpensslConfig();

            config.Default.RANDFILE = Item.OpenSSLPath.Rnd.Replace("\\", "/");

            //  マルチドメイン用設定 (引数から読み取り)
            //  https://rms.ne.jp/sslserver/csr/openssl.html/
            config.req.req_extensions = "v3_req";
            config.usr_cert.subjectAltName = "@alt_names";
            config.v3_req.basicConstraints = "CA:FALSE";
            config.v3_req.keyUsage = "nonRepudiation, digitalSignature, keyEncipherment";
            config.v3_req.subjectAltName = "@alt_names";

            if (AlternateNames == null || AlternateNames.Length == 0)
            {
                config.alt_names.DNS_altnames = new List<string>() { Subject.Substring(Subject.IndexOf("/CN=") + 4) };
            }
            else
            {
                Regex reg_ip = new Regex(@"^((\d\d?|1\d\d|2[0-4]\d|25[0-5])\.){3}(\d\d?|1\d\d|2[0-4]\d|25[0-5])$");
                List<string> ipList = new List<string>();
                List<string> dnsList = new List<string>();
                foreach (string altname in AlternateNames)
                {
                    if (reg_ip.IsMatch(altname))
                    {
                        ipList.Add(altname);
                    }
                    else
                    {
                        dnsList.Add(altname);
                    }
                }
                config.alt_names.IP_altnames = ipList;
                config.alt_names.DNS_altnames = dnsList;
            }

            //  ver1.1.1から、-addextオプションでマルチドメイン設定ができるっぽい
            //  次バージョンあたりで検討よろしく
            //  https://kaede.jp/2018/06/10191854.html

            using (StreamWriter sw = new StreamWriter(Item.OpenSSLPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  ランダムファイル「.rnd」作成
            OpensslCommand.CreateRnadomFile();

            //  鍵ファイル作成
            OpensslCommand.CreateKeyFile(KeyFile, RsaBits);

            //  CSRファイルを作成
            OpensslCommand.CreateCSRFile(CsrFile, KeyFile, Subject);
            */
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
