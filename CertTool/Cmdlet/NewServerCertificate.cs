using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Diagnostics;
using CertTool.OpenSSL;

namespace CertTool.Cmdlet
{
    [Cmdlet(VerbsCommon.New, "ServerCertificate")]
    public class NewServerCertificate : PSCmdlet
    {
        [Parameter]
        public string CACrtFile { get; set; } = "ca.crt";
        [Parameter]
        public string CAKeyFile { get; set; } = "ca.key";
        [Parameter]
        public string CsrFile { get; set; } = "server.csr";
        [Parameter]
        public string CrtFile { get; set; } = "server.crt";
        [Parameter]
        public int ExpireDays { get; set; } = 365;
        
        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;

            Item.OpenSSLPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            Function.ExpandEmbeddedResource(Item.OpenSSLPath.Base);
            if (!Directory.Exists(Item.OpenSSLPath.Dir))
            {
                Function.ExtractZipFile(Item.OpenSSLPath.Zip, Item.OpenSSLPath.Dir);
            }
        }

        protected override void ProcessRecord()
        {
            OpensslConfig config = new OpensslConfig();

            config.Default.RANDFILE = Item.OpenSSLPath.Rnd.Replace("\\", "/");
            config.CA_default.dir = Item.OpenSSLPath.Dir.Replace("\\", "/");
            config.CA_default.database = Item.OpenSSLPath.Db.Replace("\\", "/");
            config.CA_default.serial = Item.OpenSSLPath.Serial.Replace("\\", "/");

            //  マルチドメイン用設定 (CSRから読み取り)
            config.req.req_extensions = "v3_req";
            config.usr_cert.subjectAltName = "@alt_names";
            config.v3_req.basicConstraints = "CA:FALSE";
            config.v3_req.keyUsage = "nonRepudiation, digitalSignature, keyEncipherment";
            config.v3_req.subjectAltName = "@alt_names";

            string tempCsrText = OpensslCommand.ConvertToText(CsrFile, true, false, false);
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

            using (StreamWriter sw = new StreamWriter(Item.OpenSSLPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  ランダムファイル「.rnd」作成
            OpensslCommand.CreateRnadomFile();

            //  CSRに署名して証明書を発行
            OpensslCommand.SignCrtFile(ExpireDays, CACrtFile, CAKeyFile, CsrFile, CrtFile);
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
