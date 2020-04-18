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
    [Cmdlet(VerbsData.Convert, "CertificateToText")]
    public class ConvertCertificateToText : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
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
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
            OpensslConfig config = new OpensslConfig();
            using (StreamWriter sw = new StreamWriter(opensslPath.Cnf, false, new UTF8Encoding(false)))
            {
                sw.Write(config.GetIni());
            }
            string text = command.ConvertToText(SourcePath, Csr, Crt, Key);

            WriteObject(text);

            if (SaveConfig) { OpensslFunction.BackupConf(); }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
