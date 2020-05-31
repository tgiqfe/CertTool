using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using CertTool.OpenSSL;

namespace CertTool.Cmdlet
{
    [Cmdlet(VerbsData.Convert, "CertificateFile")]
    public class ConvertCertificateFile : PSCmdlet
    {
        const string MODE_ToNginxCert = "ToNginxCert";

        [Parameter, ValidateSet(MODE_ToNginxCert)]
        public string Mode { get; set; } = MODE_ToNginxCert;

        //  ToNginxCert用のパラメータ
        [Parameter]
        public string RootCert { get; set; }
        [Parameter]
        public string ChainCert { get; set; }
        [Parameter]
        public string ServerCert { get; set; }
        [Parameter]
        public string Output { get; set; }

        [Parameter]
        public SwitchParameter SaveConfig { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            //  可能であれば、絶対パスだった場合とファイル名だけだった場合の分岐も設定したい
            //  ⇒Issue済み
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            if (string.IsNullOrEmpty(RootCert))
            {
                RootCert = Path.Combine(opensslPath.CertDir, Item.DEFAULT_ROOTCA_CRT_NAME);
            }
            if (string.IsNullOrEmpty(ChainCert))
            {
                ChainCert = Path.Combine(opensslPath.CertDir, "chain.crt");
            }
            if (string.IsNullOrEmpty(ServerCert))
            {
                ServerCert = Path.Combine(opensslPath.CertDir, Item.DEFAULT_SERVER_CRT_NAME);
            }

            switch (Mode)
            {
                case MODE_ToNginxCert:
                    //  ToNginxCert以外のパラメータは未実装
                    //  (今後追加する必要ができてから実装予定。多分Java(Tomcat)用を作るかも。
                    StringBuilder joinSB = new StringBuilder();
                    Action<string> AppendingCert = (file) =>
                    {
                        if (File.Exists(file))
                        {
                            using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
                            {
                                joinSB.Append(sr.ReadToEnd());
                            }
                        }
                    };
                    AppendingCert(ServerCert);
                    AppendingCert(ChainCert);
                    AppendingCert(RootCert);

                    if (string.IsNullOrEmpty(Output))
                    {
                        WriteObject(joinSB.ToString());
                    }
                    else
                    {
                        using (StreamWriter sw = new StreamWriter(Output, false, new UTF8Encoding(false)))
                        {
                            sw.Write(joinSB.ToString());
                        }
                    }
                    break;
            }

            if (SaveConfig) { OpensslFunction.BackupConf(); }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
