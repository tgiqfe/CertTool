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
        public string CsrFile { get; set; } = "server.csr";
        [Parameter]
        public string KeyFile { get; set; } = "server.key";
        [Parameter]
        public string Subject { get; set; }
        [Parameter]
        public string[] AlternateNames { get; set; }
        [Parameter]
        public int RsaBits { get; set; } = 4096;

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
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
