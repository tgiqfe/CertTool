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
    [Cmdlet(VerbsSecurity.Revoke, "ServerCertificate")]
    public class RevokeServerCertificate : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string CrtFile { get; set; } = "server.crt";
        [Parameter(Position = 1)]
        public string CACrtFile { get; set; } = "ca.crt";
        [Parameter(Position = 2)]
        public string CAKeyFile { get; set; } = "ca.key";

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

            using (StreamWriter sw = new StreamWriter(Item.OpenSSLPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  ランダムファイル「.rnd」作成
            OpensslCommand.CreateRnadomFile();

            //  CSRに署名して証明書を発行
            OpensslCommand.RevokeCertificate(CrtFile, CACrtFile, CAKeyFile);
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
