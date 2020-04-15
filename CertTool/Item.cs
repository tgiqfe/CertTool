using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CertTool
{
    class Item
    {
        public static readonly string WORK_DIRECTORY =
            Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "CertTool");
        public static readonly string TOOLS_DIRECTORY =
            Path.Combine(WORK_DIRECTORY, "Tools");

        public static CertTool.OpenSSL.OpensslPath OpenSSLPath = null;
    }
}
