using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CertTool.OpenSSL;

namespace CertTool
{
    internal class Item
    {
        /// <summary>
        /// 作業用フォルダー
        /// </summary>
        public static readonly string WORK_DIRECTORY =
            Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "CertTool");

        /// <summary>
        /// ツール展開先フォルダー
        /// </summary>
        public static readonly string TOOLS_DIRECTORY =
            Path.Combine(WORK_DIRECTORY, "Tools");
    }
}
