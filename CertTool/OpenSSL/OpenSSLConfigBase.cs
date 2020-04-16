using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CertTool.OpenSSL
{
    public class OpensslConfigBase
    {
        /// <summary>
        /// 対象クラス(セクション単位)の名前(セクション名)
        /// </summary>
        public virtual string Name
        {
            get
            {
                return "[ " + this.GetType().Name.Replace("Section_", "") + " ]";
            }
        }
    }
}
