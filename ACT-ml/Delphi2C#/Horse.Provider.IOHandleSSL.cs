using System;

namespace Horse.Provider.IOHandleSSL
{
    
    public enum TIdSSLVersion
    {
        TLSv1,
        TLSv1_1,
        TLSv1_2
    }

   
    [Flags]
    public enum TIdSSLVersions
    {
        TLSv1 = 1,
        TLSv1_1 = 2,
        TLSv1_2 = 4
    }

    
    public delegate void TPasswordEvent(object sender, ref string password);

    
    public interface IHorseProviderIOHandleSSL
    {
        
        IHorseProviderIOHandleSSL Active(bool value);
        bool Active { get; }

        IHorseProviderIOHandleSSL CertFile(string value);
        string CertFile { get; }

        IHorseProviderIOHandleSSL RootCertFile(string value);
        string RootCertFile { get; }

        IHorseProviderIOHandleSSL KeyFile(string value);
        string KeyFile { get; }

        IHorseProviderIOHandleSSL Method(TIdSSLVersion value);
        TIdSSLVersion Method { get; }

        IHorseProviderIOHandleSSL SSLVersions(TIdSSLVersions value);
        TIdSSLVersions SSLVersions { get; }

        IHorseProviderIOHandleSSL DHParamsFile(string value);
        string DHParamsFile { get; }

        IHorseProviderIOHandleSSL CipherList(string value);
        string CipherList { get; }

        IHorseProviderIOHandleSSL OnGetPassword(TPasswordEvent value);
        TPasswordEvent OnGetPassword { get; }
    }

    
    public class HorseProviderIOHandleSSL : IHorseProviderIOHandleSSL
    {
        private bool _active;
        private string _keyFile;
        private string _rootCertFile;
        private string _certFile;
        private string _dhParamsFile;
        private string _cipherList;
        private TIdSSLVersion _method;
        private TIdSSLVersions _sslVersions;
        private TPasswordEvent _onGetPassword;

        
        private const TIdSSLVersion DEF_SSLVERSION = TIdSSLVersion.TLSv1_2;
        private const TIdSSLVersions DEF_SSLVERSIONS = TIdSSLVersions.TLSv1_2;

        public HorseProviderIOHandleSSL()
        {
            _active = true;
            _method = DEF_SSLVERSION;
            _sslVersions = DEF_SSLVERSIONS;
        }

        public IHorseProviderIOHandleSSL Active(bool value)
        {
            _active = value;
            return this;
        }

        public bool Active => _active;

        public IHorseProviderIOHandleSSL CertFile(string value)
        {
            _certFile = value;
            return this;
        }

        public string CertFile => _certFile;

        public IHorseProviderIOHandleSSL RootCertFile(string value)
        {
            _rootCertFile = value;
            return this;
        }

        public string RootCertFile => _rootCertFile;

        public IHorseProviderIOHandleSSL KeyFile(string value)
        {
            _keyFile = value;
            return this;
        }

        public string KeyFile => _keyFile;

        public IHorseProviderIOHandleSSL Method(TIdSSLVersion value)
        {
            _method = value;
            return this;
        }

        public TIdSSLVersion Method => _method;

        public IHorseProviderIOHandleSSL SSLVersions(TIdSSLVersions value)
        {
            _sslVersions = value;
            return this;
        }

        public TIdSSLVersions SSLVersions => _sslVersions;

        public IHorseProviderIOHandleSSL DHParamsFile(string value)
        {
            _dhParamsFile = value;
            return this;
        }

        public string DHParamsFile => _dhParamsFile;

        public IHorseProviderIOHandleSSL CipherList(string value)
        {
            _cipherList = value;
            return this;
        }

        public string CipherList => _cipherList;

        public IHorseProviderIOHandleSSL OnGetPassword(TPasswordEvent value)
        {
            _onGetPassword = value;
            return this;
        }

        public TPasswordEvent OnGetPassword => _onGetPassword;

        
        public static IHorseProviderIOHandleSSL New => new HorseProviderIOHandleSSL();
    }
}
