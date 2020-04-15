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
        protected override void BeginProcessing()
        {
            Item.OpenSSLPath = new OpensslPath(Item.TOOLS_DIRECTORY);
            Function.ExpandEmbeddedResource(Item.OpenSSLPath.Base);
            if (!Directory.Exists(Item.OpenSSLPath.Dir))
            {
                Function.ExtractZipFile(Item.OpenSSLPath.Zip, Item.OpenSSLPath.Dir);
            }
        }

        protected override void ProcessRecord()
        {
            OpensslCommand.GetVersion();
        }
    }
}
