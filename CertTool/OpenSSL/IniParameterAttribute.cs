using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertTool.OpenSSL
{
    class IniParameterAttribute : Attribute
    {
        public string AltName { get; set; }
        public bool IsCount { get; set; }

        public IniParameterAttribute() { }
        public IniParameterAttribute(string altName)
        {
            this.AltName = altName;
            this.IsCount = altName.Contains("*");
        }
    }
}
