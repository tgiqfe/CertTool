using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace CertTool.OpenSSL
{
    public class OpensslConfig
    {
        public class Section_Default : OpensslConfigBase
        {
            public override string Name { get { return null; } }

            public string HOME { get; set; } = ".";
            public string RANDFILE { get; set; } = "$ENV::HOME/.rnd";
            public string oid_file { get; set; }
            public string oid_section { get; set; } = "new_oids";
        }
        public class Section_new_oids : OpensslConfigBase
        {
            public string tsa_policy1 { get; set; } = "1.2.3.4.1";
            public string tsa_policy2 { get; set; } = "1.2.3.4.5.6";
            public string tsa_policy3 { get; set; } = "1.2.3.4.5.7";
        }
        public class Section_ca : OpensslConfigBase
        {
            public string default_ca { get; set; } = "CA_default";
        }
        public class Section_CA_default : OpensslConfigBase
        {
            public string dir { get; set; } = ".";
            public string certs { get; set; } = "$dir/certs";
            public string crl_dir { get; set; } = "$dir/crl";
            public string database { get; set; } = "$dir/index.txt";
            public string unique_subject { get; set; }
            public string new_certs_dir { get; set; } = "$dir";
            public string certificate { get; set; } = "$dir/cacert.pem";
            public string serial { get; set; } = "$dir/serial";
            public string crlnumber { get; set; } = "$dir/crlnumber";
            public string crl { get; set; } = "$dir/crl.pem";
            public string private_key { get; set; } = "$dir/private/cakey.pem";
            public string RANDFILE { get; set; } = "$dir/private/.rnd";
            public string x509_extensions { get; set; } = "usr_cert";
            public string name_opt { get; set; } = "ca_default";
            public string cert_opt { get; set; } = "ca_default";
            public int default_days { get; set; } = 365;
            public int default_crl_days { get; set; } = 30;
            public string default_md { get; set; } = "default";
            public string preserve { get; set; } = "no";
            public string policy { get; set; } = "policy_match";
        }
        public class Section_policy_match : OpensslConfigBase
        {
            public string countryName { get; set; } = "match";
            public string stateOrProvinceName { get; set; } = "optional";
            public string organizationName { get; set; } = "optional";
            public string organizationalUnitName { get; set; } = "optional";
            public string commonName { get; set; } = "supplied";
            public string emailAddress { get; set; } = "optional";
        }
        public class Section_policy_anything : OpensslConfigBase
        {
            public string countryName { get; set; } = "optional";
            public string stateOrProvinceName { get; set; } = "optional";
            public string localityName { get; set; } = " optional";
            public string organizationName { get; set; } = "optional";
            public string organizationalUnitName { get; set; } = "optional";
            public string commonName { get; set; } = "supplied";
            public string emailAddress { get; set; } = "optional";
        }
        public class Section_req : OpensslConfigBase
        {
            public int default_bits { get; set; } = 2048;
            public string default_keyfile { get; set; } = "privkey.pem";
            public string distinguished_name { get; set; } = "req_distinguished_name";
            public string attributes { get; set; } = "req_attributes";
            public string x509_extensions { get; set; } = "v3_ca";
            public string string_mask { get; set; } = "utf8only";
            public string req_extensions { get; set; }
        }
        public class Section_req_distinguished_name : OpensslConfigBase
        {
            public string countryName { get; set; } = "Country Name(2 letter code)";
            public string countryName_default { get; set; } = "JP";
            public int countryName_min = 2;
            public int countryName_max = 2;
            public string stateOrProvinceName { get; set; } = "State or Province Name(full name)";
            public string stateOrProvinceName_default { get; set; } = "Some-State";
            public string localityName { get; set; } = "Locality Name (eg, city)";
            [IniParameterName("0.organizationName")]
            public string _0_organizationName { get; set; } = "Organization Name (eg, company)";
            [IniParameterName("0.organizationName_default")]
            public string _0_organizationName_default { get; set; } = "Internet Widgits Pty Ltd";
            [IniParameterName("1.organizationName")]
            public string _1_organizationName { get; set; }
            [IniParameterName("1.organizationName_default")]
            public string _1_organizationName_default { get; set; }
            public string organizationalUnitName { get; set; } = "Organizational Unit Name (eg, section)";
            public string organizationalUnitName_default { get; set; }
            public string commonName { get; set; } = "Common Name (e.g. server FQDN or YOUR name)";
            public int commonName_max { get; set; } = 64;
            public string emailAddress { get; set; } = "Email Address";
            public int emailAddress_max { get; set; } = 64;
            [IniParameterName("SET-ex3")]
            public string SET_ex3 { get; set; }
        }
        public class Section_req_attributes : OpensslConfigBase
        {
            public string challengePassword { get; set; } = "A challenge password";
            public int challengePassword_min { get; set; } = 4;
            public int challengePassword_max { get; set; } = 20;
            public string unstructuredName { get; set; } = "An optional company name";
        }
        public class Section_usr_cert : OpensslConfigBase
        {
            public string basicConstraints { get; set; } = "CA:FALSE";
            public string nsCertType { get; set; }
            public string keyUsage { get; set; }
            public string nsComment { get; set; } = "OpenSSL Generated Certificate";
            public string subjectKeyIdentifier { get; set; } = "hash";
            public string authorityKeyIdentifier { get; set; } = "keyid,issuer";
            public string subjectAltName { get; set; }
            public string issuerAltName { get; set; }
            public string nsCaRevocationUrl { get; set; }
            public string nsBaseUrl { get; set; }
            public string nsRevocationUrl { get; set; }
            public string nsRenewalUrl { get; set; }
            public string nsCaPolicyUrl { get; set; }
            public string nsSslServerName { get; set; }
            public string extendedKeyUsage { get; set; }
        }
        public class Section_v3_req : OpensslConfigBase
        {
            public override string Name
            {
                get
                {
                    return (basicConstraints == null && keyUsage == null && subjectAltName == null) ?
                        null : base.Name;
                    //null : "[ v3_req ]";
                }
            }
            public string basicConstraints { get; set; }
            public string keyUsage { get; set; }
            public string subjectAltName { get; set; }
        }
        public class Section_alt_names : OpensslConfigBase
        {
            public override string Name
            {
                get
                {
                    return (DNS_altnames == null || DNS_altnames.Count == 0) &&
                           (IP_altnames == null || IP_altnames.Count == 0) ?
                        null : base.Name;
                    //null : "[ alt_names ]";
                }
            }

            [IniParameterName("DNS.*")]
            public List<string> DNS_altnames { get; set; }
            [IniParameterName("IP.*")]
            public List<string> IP_altnames { get; set; }
        }
        public class Section_v3_ca : OpensslConfigBase
        {
            public string subjectKeyIdentifier { get; set; } = "hash";
            public string authorityKeyIdentifier { get; set; } = "keyid:always,issuer";
            public string basicConstraints { get; set; } = "critical,CA:true";
            public string keyUsage { get; set; }
            public string nsCertType { get; set; }
            public string subjectAltName { get; set; }
            public string issuerAltName { get; set; }
            public string obj { get; set; }
        }
        public class Section_crl_ext : OpensslConfigBase
        {
            public string issuerAltName { get; set; }
            public string authorityKeyIdentifier { get; set; } = "keyid:always";
        }
        public class Section_proxy_cert_ext : OpensslConfigBase
        {
            public string basicConstraints { get; set; } = "CA:FALSE";
            public string nsCertType { get; set; }
            public string keyUsage { get; set; }
            public string nsComment { get; set; } = "OpenSSL Generated Certificate";
            public string subjectKeyIdentifier { get; set; } = "hash";
            public string authorityKeyIdentifier { get; set; } = "keyid,issuer";
            public string subjectAltName { get; set; }
            public string issuerAltName { get; set; }
            public string nsCaRevocationUrl { get; set; }
            public string nsBaseUrl { get; set; }
            public string nsRevocationUrl { get; set; }
            public string nsRenewalUrl { get; set; }
            public string nsCaPolicyUrl { get; set; }
            public string nsSslServerName { get; set; }
            public string proxyCertInfo { get; set; } = "critical,language:id-ppl-anyLanguage,pathlen:3,policy:foo";
        }
        public class Section_tsa : OpensslConfigBase
        {
            public string default_tsa { get; set; } = "tsa_config1";
        }
        public class Section_tsa_config1 : OpensslConfigBase
        {
            public string dir { get; set; } = "./demoCA";
            public string serial { get; set; } = "$dir/tsaserial";
            public string crypto_device { get; set; } = "builtin";
            public string signer_cert { get; set; } = "$dir/tsacert.pem";
            public string certs { get; set; } = "$dir/cacert.pem";
            public string signer_key { get; set; } = "$dir/private/tsakey.pem";
            public string signer_digest { get; set; } = "sha256";
            public string default_policy { get; set; } = "tsa_policy1";
            public string other_policies { get; set; } = "tsa_policy2, tsa_policy3";
            public string digests { get; set; } = "sha1, sha256, sha384, sha512";
            public string accuracy { get; set; } = "secs:1, millisecs:500, microsecs:100";
            public string clock_precision_digits { get; set; } = "0";
            public string ordering { get; set; } = "yes";
            public string tsa_name { get; set; } = "yes";
            public string ess_cert_id_chain { get; set; } = "no";
        }

        public Section_Default Default { get; set; } = new Section_Default();
        public Section_new_oids new_oids { get; set; } = new Section_new_oids();
        public Section_ca ca { get; set; } = new Section_ca();
        public Section_CA_default CA_default { get; set; } = new Section_CA_default();
        public Section_policy_match policy_match { get; set; } = new Section_policy_match();
        public Section_policy_anything policy_anything { get; set; } = new Section_policy_anything();
        public Section_req req { get; set; } = new Section_req();
        public Section_req_distinguished_name req_distinguished_name { get; set; } = new Section_req_distinguished_name();
        public Section_req_attributes req_attributes { get; set; } = new Section_req_attributes();
        public Section_usr_cert usr_cert { get; set; } = new Section_usr_cert();
        public Section_v3_req v3_req { get; set; } = new Section_v3_req();
        public Section_alt_names alt_names { get; set; } = new Section_alt_names();
        public Section_v3_ca v3_ca { get; set; } = new Section_v3_ca();
        public Section_crl_ext crl_ext { get; set; } = new Section_crl_ext();
        public Section_proxy_cert_ext proxy_cert_ext { get; set; } = new Section_proxy_cert_ext();
        public Section_tsa tsa { get; set; } = new Section_tsa();
        public Section_tsa_config1 tsa_config1 { get; set; } = new Section_tsa_config1();

        public List<OpensslConfigBase> configList = null;

        public OpensslConfig() { }

        public string GetIni()
        {
            StringBuilder sb = new StringBuilder();
            foreach (PropertyInfo pi in
                this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                OpensslConfigBase configBase = pi.GetValue(this) as OpensslConfigBase;
                if (configBase.Name != null)
                {
                    sb.Append(configBase.Name + "\n");
                }

                foreach (PropertyInfo pii in
                    configBase.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (pii.Name == "Name") { continue; }

                    object paramValue = pii.GetValue(configBase);
                    if (paramValue != null)
                    {
                        IniParameterNameAttribute paramNameAttr =
                            Attribute.GetCustomAttribute(pii, typeof(IniParameterNameAttribute)) as IniParameterNameAttribute;
                        if (paramNameAttr == null)
                        {
                            //  IniParameterName属性無しのプロパティ
                            sb.Append(string.Format("{0} = {1}\n", pii.Name, paramValue));
                        }
                        else
                        {
                            if (paramNameAttr.IsCount)
                            {
                                //  カウント指定有りのプロパティ
                                string tempName = paramNameAttr.AltName.Replace("*", "{0}") + " = {1}\n";
                                IEnumerable<string> list = paramValue as IEnumerable<string>;
                                if (list != null)
                                {
                                    int count = 1;
                                    foreach (string item in list)
                                    {
                                        sb.Append(string.Format(tempName, count++, item));
                                    }
                                }
                            }
                            else
                            {
                                //  IniParameterName属性有り、別名設定
                                sb.Append(string.Format("{0} = {1}\n", paramNameAttr.AltName, paramValue));
                            }
                        }
                    }
                }

                sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}
