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
    [Cmdlet(VerbsCommon.New, "RootCertificateAuthority")]
    public class NewRootCertificateAuthority : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Subject { get; set; }
        [Parameter]
        public string CACrtFile { get; set; } = "ca.crt";
        [Parameter]
        public string CAKeyFile { get; set; } = "ca.key";
        [Parameter]
        public int ExpireDays { get; set; } = 365;
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
            OpensslFunction.CreateRootCA(CACrtFile, CAKeyFile, Subject, ExpireDays, RsaBits);

            /*
            OpensslConfig config = new OpensslConfig();

            config.Default.RANDFILE = Item.OpenSSLPath.Rnd.Replace("\\", "/");
            
            using (StreamWriter sw = new StreamWriter(Item.OpenSSLPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            //  ランダムファイル「.rnd」作成
            OpensslCommand.CreateRnadomFile();

            //  鍵ファイル作成
            OpensslCommand.CreateKeyFile(CAKeyFile, RsaBits);

            //  証明書ファイルを作成
            OpensslCommand.CreateCACrtFile(ExpireDays, CACrtFile, CAKeyFile, Subject);
            */

        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
