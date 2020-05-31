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

        public static readonly string DEFAULT_SERVER_CSR_NAME = "server.csr";
        public static readonly string DEFAULT_SERVER_CRT_NAME = "server.crt";
        public static readonly string DEFAULT_SERVER_KEY_NAME = "server.key";
        public static readonly string DEFAULT_ROOTCA_CRT_NAME = "ca.crt";
        public static readonly string DEFAULT_ROOTCA_KEY_NAME = "ca.key";
    }
}
