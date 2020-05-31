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
    [Cmdlet(VerbsCommon.Show, "CertificateFile")]
    public class ShowCertificateFile : PSCmdlet
    {
        //  ToText用のパラメータ
        [Parameter(Position = 0), Alias("Path")]
        public string SourcePath { get; set; }
        [Parameter]
        public SwitchParameter Csr { get; set; }
        [Parameter]
        public SwitchParameter Crt { get; set; }
        [Parameter]
        public SwitchParameter Key { get; set; }

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
            if (File.Exists(SourcePath))
            {
                string text = OpensslFunction.ConvertToText(SourcePath, Csr, Crt, Key);
                WriteObject(text);

                if (SaveConfig) { OpensslFunction.BackupConf(); }
            }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
