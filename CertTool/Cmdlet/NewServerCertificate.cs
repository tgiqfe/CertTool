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
        public string CACrtFile { get; set; }
        [Parameter]
        public string CAKeyFile { get; set; }
        [Parameter]
        public string CsrFile { get; set; }
        [Parameter]
        public string CrtFile { get; set; }
        [Parameter]
        public int ExpireDays { get; set; } = 365;
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
            OpensslFunction.SignCertificate(CACrtFile, CAKeyFile, CsrFile, CrtFile, ExpireDays);

            if (SaveConfig) { OpensslFunction.BackupConf(); }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
