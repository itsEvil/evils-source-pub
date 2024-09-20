using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Shared;
public class RSA
{
    private readonly RSACryptoServiceProvider _csp = new(2048);
    private readonly RSAParameters _privKey;
    private readonly RSAParameters _pubKey;
    public RSA()
    {
        _privKey = _csp.ExportParameters(true);
        _pubKey = _csp.ExportParameters(false);
    }

    public string GetPublicKey()
    {
        var sw = new StringWriter();
        var xmlSerializer = new XmlSerializer(typeof(RSAParameters));
        xmlSerializer.Serialize(sw, _pubKey);
        return sw.ToString();
    }

    public string Encrypt(string toEncrypt)
    {
        _csp.ImportParameters(_pubKey);
        return Convert.ToBase64String(_csp.Encrypt(Encoding.Unicode.GetBytes(toEncrypt), false));
    }

    public string Decrypt(string encrypted)
    {
        _csp.ImportParameters(_privKey);
        var data = Convert.FromBase64String(encrypted);
        return Encoding.Unicode.GetString(_csp.Decrypt(data, false));
    }
}

