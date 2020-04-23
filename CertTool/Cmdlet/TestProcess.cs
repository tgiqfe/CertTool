using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using CertTool.OpenSSL;

namespace CertTool.Cmdlet
{
    [Cmdlet(VerbsDiagnostic.Test, "Process")]
    public class TestProcess : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            string[] array = OpensslFunction.GetAlternateNamesFromCsr(@"C:\Users\tq\AppData\Local\Temp\CertTool\db\cert\server.csr");
            
            foreach(string ar in array)
            {
                Console.WriteLine(ar);
            }


        }
    }
}
