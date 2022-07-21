using System;
using System.Security.Cryptography;
using System.Xml.Linq;
using static System.Convert;

namespace SimplePM.WebAPI.Library
{
    public static class RsaExtensions
    {
        /// <summary>
        /// Exports RSA parameters from RSA inherit class instanse and inserts them to XML string.
        /// </summary>
        /// 
        /// <returns>XML formated string constains RSA parameters (public or private key).</returns>
        /// 
        /// <exception cref="ArgumentNullException">One of RSA private key parameters is null.</exception>
        public static string ToXmlStringExt(this System.Security.Cryptography.RSA rsa, bool includePrivateParameters)
        {
            var p = rsa.ExportParameters(includePrivateParameters);
            XElement xml;
            if (includePrivateParameters)
            {
                xml = new XElement("RSAKeyValue",
                    new XElement("Modulus", ToBase64String(p.Modulus)),
                    new XElement("Exponent", ToBase64String(p.Exponent)),
                    new XElement("D", ToBase64String(p.D)),
                    new XElement("P", ToBase64String(p.P)),
                    new XElement("Q", ToBase64String(p.Q)),
                    new XElement("DP", ToBase64String(p.DP)),
                    new XElement("DQ", ToBase64String(p.DQ)),
                    new XElement("InverseQ", ToBase64String(p.InverseQ))
                );
            }
            else
            {
                xml = new XElement("RSAKeyValue",
                    new XElement("Modulus", ToBase64String(p.Modulus)),
                    new XElement("Exponent", ToBase64String(p.Exponent)));
            }
            return xml?.ToString();
        }

        /// <summary>
        /// Extracts RSA parameters from XML string and imports them to RSA inherit class instanse.
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException">One of RSA key parameters is null.</exception>
        /// 
        /// <exception cref="FormatException">The length of one of RSA key parameters,
        ///     ignoring white-space characters, is not zero or a multiple 4.
        ///     -or- The format of parameter is invalid. Parameter contains a non-base-64 character, more
        ///     than two padding characters, or a non-white space-character among the padding characters.</exception>
        public static void FromXmlStringExt(this System.Security.Cryptography.RSA rsa, string parametersAsXml)
        {
            var xml = XDocument.Parse(parametersAsXml);
            var root = xml.Element("RSAKeyValue");
            var p = new RSAParameters
            {
                Modulus = FromBase64String(root.Element("Modulus").Value),
                Exponent = FromBase64String(root.Element("Exponent").Value)
            };
            if (root.Element("P") != null)
            {
                p.D = FromBase64String(root.Element("D").Value);
                p.P = FromBase64String(root.Element("P").Value);
                p.Q = FromBase64String(root.Element("Q").Value);
                p.DP = FromBase64String(root.Element("DP").Value);
                p.DQ = FromBase64String(root.Element("DQ").Value);
                p.InverseQ = FromBase64String(
                root.Element("InverseQ").Value);
            }
            rsa.ImportParameters(p);
        }
    }
}
