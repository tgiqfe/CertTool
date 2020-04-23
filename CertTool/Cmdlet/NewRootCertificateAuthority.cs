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
        public string CACrtFile { get; set; }
        [Parameter]
        public string CAKeyFile { get; set; }
        [Parameter]
        public int ExpireDays { get; set; } = 365;
        [Parameter]
        public int RsaBits { get; set; } = 4096;
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
            OpensslFunction.CreateRootCA(CACrtFile, CAKeyFile, Subject, ExpireDays, RsaBits);

            if (SaveConfig) { OpensslFunction.BackupConf(); }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
