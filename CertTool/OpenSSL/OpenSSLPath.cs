﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CertTool.OpenSSL
{
    public class OpensslPath
    {
        private string _Base = null;
        private string _Zip = null;
        private string _Dir = null;
        private string _Exe = null;
        private string _Work = null;
        private string _Cnf = null;
        private string _Rnd = null;
        private string _OpenlslDB = null;
        private string _Serial = null;
        private string _BkDir = null;
        private string _CertDir = null;

        public string Base { get { return this._Base; } }
        public string Zip
        {
            get
            {
                if (_Zip == null)
                {

                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._Zip = Path.Combine(_Base, "openssl.zip");
                    }
                }
                return this._Zip;
            }
        }
        public string Dir
        {
            get
            {
                if (_Dir == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._Dir = Path.Combine(_Base, "openssl");
                    }
                }
                return this._Dir;
            }
        }
        public string Exe
        {
            get
            {
                if (_Exe == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._Exe = Directory.GetFiles(_Base, "*.exe", SearchOption.AllDirectories).
                            FirstOrDefault(x => Path.GetFileName(x).Equals("openssl.exe"));
                    }
                }
                return this._Exe;
            }
        }
        public string Work
        {
            get
            {
                if (_Work == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._Work = Function.RelatedToAbsolutePath(_Base, "..\\CA\\openssl");
                    }
                    if (!Directory.Exists(_Work))
                    {
                        Directory.CreateDirectory(_Work);
                    }
                }
                return this._Work;
            }
        }
        public string Cnf
        {
            get
            {
                if (_Cnf == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._Cnf = Path.Combine(Work, "openssl.cnf");
                    }
                }
                return this._Cnf;
            }
        }
        public string Rnd
        {
            get
            {
                if (_Rnd == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._Rnd = Path.Combine(Work, ".rnd");
                    }
                }
                return this._Rnd;
            }
        }
        public string OpensslDB
        {
            get
            {
                if (_OpenlslDB == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._OpenlslDB = Path.Combine(Work, "index.txt");
                    }
                }
                return this._OpenlslDB;
            }
        }
        public string Serial
        {
            get
            {
                if (_Serial == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._Serial = Path.Combine(Work, "serial");
                    }
                }
                return this._Serial;
            }
        }
        public string BkDir
        {
            get
            {
                if (_BkDir == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._BkDir = Function.RelatedToAbsolutePath(_Base, "..\\bk");
                    }
                    if (!Directory.Exists(_BkDir))
                    {
                        Directory.CreateDirectory(_BkDir);
                    }
                }
                return this._BkDir;
            }
        }
        public string CertDir
        {
            get
            {
                if (_CertDir == null)
                {
                    if (!string.IsNullOrEmpty(_Base))
                    {
                        this._CertDir = Function.RelatedToAbsolutePath(_Base, "..\\CA\\cert");
                    }
                    if (!Directory.Exists(_CertDir))
                    {
                        Directory.CreateDirectory(_CertDir);
                    }
                }
                return this._CertDir;
            }
        }

        public OpensslPath() { }
        public OpensslPath(string baseDir)
        {
            this._Base = baseDir;
        }
    }
}
