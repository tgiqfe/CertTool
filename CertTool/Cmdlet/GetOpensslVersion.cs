using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using CertTool.OpenSSL;
using System.IO;

namespace CertTool.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "OpensslVersion")]
    public class GetOpensslVersion : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            OpensslPath opensslPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            OpensslCommand command = new OpensslCommand(opensslPath);
            command.GetVersion();
        }
    }
}
