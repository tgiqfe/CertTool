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
        public string CsrFile { get; set; }
        [Parameter]
        public string KeyFile { get; set; }
        [Parameter]
        public string Subject { get; set; }
        [Parameter]
        public string[] AlternateNames { get; set; }
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
            OpensslFunction.CreateCSR(CsrFile, KeyFile, Subject, AlternateNames, RsaBits);

            if (SaveConfig) { OpensslFunction.BackupConf(); }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
