using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Diagnostics;
using CertTool.OpenSSL;
using System.Security.Cryptography.X509Certificates;
using System.Collections.ObjectModel;

namespace CertTool.Cmdlet
{
    [Cmdlet(VerbsLifecycle.Register, "ServerCertificate")]
    public class RegisterServerCertificate : PSCmdlet, IDynamicParameters
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string CrtFile { get; set; }
        [Parameter(Position = 3)]
        public string FriendlyName { get; set; }

        #region Dynamic Parameter
        private const string PARAM_STORELOCATION = "StoreLocation";
        private const string PARAM_STORENAME = "StoreName";
        private StoreLocation[] _storeLocations = null;
        private StoreName[] _storeNames = null;
        private RuntimeDefinedParameterDictionary _dictionary;

        public object GetDynamicParameters()
        {
            _dictionary = new RuntimeDefinedParameterDictionary();

            //  StoreLocationパラメータ用
            if (_storeLocations == null)
            {
                _storeLocations = new StoreLocation[]
                {
                    StoreLocation.CurrentUser,
                    StoreLocation.LocalMachine,
                };
            }
            Collection<Attribute> locationAttribute = new Collection<Attribute>()
            {
                new ParameterAttribute(){ Mandatory = true, Position = 1 },
                new ValidateSetAttribute(_storeLocations.Select(x => x.ToString()).ToArray()),
            };
            RuntimeDefinedParameter locationRDParam = new RuntimeDefinedParameter(PARAM_STORELOCATION, typeof(string), locationAttribute);
            _dictionary.Add(PARAM_STORELOCATION, locationRDParam);

            //  StoreNameパラメータ用
            if (_storeNames == null)
            {
                _storeNames = new StoreName[]
                {
                    StoreName.Root,
                    StoreName.My,
                    StoreName.AddressBook,
                    StoreName.AuthRoot,
                    StoreName.CertificateAuthority,
                    StoreName.Disallowed,
                    StoreName.TrustedPeople,
                    StoreName.TrustedPublisher,
                };
            }
            Collection<Attribute> nameAttribute = new Collection<Attribute>()
            {
                new ParameterAttribute(){ Mandatory = true, Position = 2 },
                new ValidateSetAttribute(_storeNames.Select(x => x.ToString()).ToArray()),
            };
            RuntimeDefinedParameter nameRDParam = new RuntimeDefinedParameter(PARAM_STORENAME, typeof(string), nameAttribute);
            _dictionary.Add(PARAM_STORENAME, nameRDParam);

            return _dictionary;
        }
        #endregion

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            if (File.Exists(CrtFile))
            {
                X509Certificate2 certificate = new X509Certificate2(CrtFile);
                if (!string.IsNullOrEmpty(FriendlyName))
                {
                    certificate.FriendlyName = FriendlyName;
                }

                StoreName stroeName =
                    Enum.TryParse(_dictionary[PARAM_STORENAME].Value as string, out StoreName tempStoreName) ?
                    tempStoreName : StoreName.My;
                StoreLocation storeLocation =
                    Enum.TryParse(_dictionary[PARAM_STORELOCATION].Value as string, out StoreLocation tempStoreLocation) ?
                    tempStoreLocation : StoreLocation.CurrentUser;

                using (X509Store store = new X509Store(stroeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadWrite);
                    if (store.Certificates.OfType<X509Certificate2>().Any(x => x.Thumbprint == certificate.Thumbprint))
                    {
                        Console.WriteLine("すでに登録済みです。");
                    }
                    else
                    {
                        Console.WriteLine("========================================================");
                        Console.WriteLine("  発行者       : {0}", certificate.Issuer);
                        Console.WriteLine("  発行先       : {0}", certificate.Subject);
                        Console.WriteLine("  フレンドリ名 : {0}", string.IsNullOrEmpty(certificate.FriendlyName) ? "-" : certificate.FriendlyName);
                        Console.WriteLine("  拇印         : {0}", certificate.Thumbprint);
                        Console.WriteLine("========================================================");
                        Console.Write("証明書を登録します。(Y/n)>");
                        string input = Console.ReadLine();
                        if (input != "n" && input != "N")
                        {
                            store.Add(certificate);
                        }
                        else
                        {
                            Console.WriteLine("登録を中断しました。");
                        }

                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;

            //  読み込んだ動的パラメータを消去
            _storeNames = null;
            _storeLocations = null;
        }
    }
}
