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
        const string MODE_ToText = "ToText";
        const string MODE_ToNginxCert = "ToNginxCert";

        [Parameter, ValidateSet(MODE_ToText, MODE_ToNginxCert)]
        public string Mode { get; set; } = MODE_ToText;

        //  ToText用のパラメータ
        [Parameter(Position = 0), Alias("Path")]
        public string SourcePath { get; set; }
        [Parameter]
        public SwitchParameter Csr { get; set; }
        [Parameter]
        public SwitchParameter Crt { get; set; }
        [Parameter]
        public SwitchParameter Key { get; set; }

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
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
            OpensslConfig config = new OpensslConfig();
            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }

            switch (Mode)
            {
                case MODE_ToText:
                    string text = command.ConvertToText(SourcePath, Csr, Crt, Key);
                    WriteObject(text);
                    break;
                case MODE_ToNginxCert:
                    string joinedCert = command.JoinCertificates(ServerCert, ChainCert, RootCert);
                    if (string.IsNullOrEmpty(Output))
                    {
                        WriteObject(joinedCert);
                    }
                    else
                    {
                        using(StreamWriter sw = new StreamWriter(Output, false, new UTF8Encoding(false)))
                        {
                            sw.Write(joinedCert);
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
