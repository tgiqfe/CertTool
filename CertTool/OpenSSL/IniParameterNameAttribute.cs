using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertTool.OpenSSL
{
    class IniParameterNameAttribute : Attribute
    {
        public string AltName { get; set; }
        public bool IsCount { get; set; }

        public IniParameterNameAttribute() { }
        public IniParameterNameAttribute(string altName)
        {
            this.AltName = altName;
            this.IsCount = altName.Contains("*");
        }
    }
}
