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
        public string CrtFile { get; set; }
        [Parameter(Position = 1)]
        public string CACrtFile { get; set; }
        [Parameter(Position = 2)]
        public string CAKeyFile { get; set; }
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
            OpensslFunction.RevokeCertificate(CrtFile, CACrtFile, CAKeyFile);

            if (SaveConfig) { OpensslFunction.BackupConf(); }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
