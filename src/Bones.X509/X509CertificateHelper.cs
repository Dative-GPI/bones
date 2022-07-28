using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Bones.X509
{
    public static class X509CertificateHelper
    {
        public static X509Certificate2 CreateRoot(string commonName, string organisationUnit, string organisation, string locality, string state, string country = "FR", int year = 20)
        {
            X509Certificate2 root;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                CertificateRequest req = new CertificateRequest($"CN={commonName}, OU={organisationUnit}, O={organisation}, L={locality}, S={state}, C={country}",
                    rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                req.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 12, true));

                req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));

                req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

                root = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(year));
            }

            return root;
        }

        public static X509Certificate2 CreateIntermediate(this X509Certificate2 cert, string commonName, string organisationUnit,
            string organisation, string locality, string state, string country = "FR", int year = 10)
        {
            X509Certificate2 intermediate;

            if (!cert.CanCertSign())
                throw new ArgumentException("The certificate should be able to sign certificate in order to create intermediate");

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                CertificateRequest req = new CertificateRequest($"CN={commonName}, OU={organisationUnit}, O={organisation}, L={locality}, S={state}, C={country}",
                    rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

                req.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 12, true));

                req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));

                var issuerSubjectKey = cert.Extensions
                    .Cast<X509Extension>()
                    .Single(e => e is X509SubjectKeyIdentifierExtension)
                    .RawData;

                req.CertificateExtensions.Add(new X509AuthorityKeyIdentifier(issuerSubjectKey, false));

                req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

                var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
                if (notBefore < cert.NotBefore) notBefore = new DateTimeOffset(cert.NotBefore);

                var notAfter = DateTimeOffset.UtcNow.AddYears(year);
                if (notAfter > cert.NotAfter) notAfter = new DateTimeOffset(cert.NotAfter);

                intermediate = req.Create(cert, notBefore, notAfter, GenerateSerialNumber()).CopyWithPrivateKey(rsa);
            }

            return intermediate;
        }

        public static X509Certificate2 CreateLeaf(this X509Certificate2 cert, string commonName, string organisationUnit,
            string organisation, string locality, string state, string country = "FR", int year = 2)
        {
            X509Certificate2 leaf;

            if (!cert.CanCertSign())
                throw new ArgumentException("The certificate should be able to sign certificate in order to create intermediate");

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                CertificateRequest req = new CertificateRequest($"CN={commonName}, OU={organisationUnit}, O={organisation}, L={locality}, S={state}, C={country}",
                    rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

                req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));

                req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));

                var issuerSubjectKey = cert.Extensions
                    .Cast<X509Extension>()
                    .Single(e => e is X509SubjectKeyIdentifierExtension)
                    .RawData;

                req.CertificateExtensions.Add(new X509AuthorityKeyIdentifier(issuerSubjectKey, false));
                req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

                var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
                if (notBefore < cert.NotBefore) notBefore = new DateTimeOffset(cert.NotBefore);

                var notAfter = DateTimeOffset.UtcNow.AddYears(2);
                if (notAfter > cert.NotAfter) notAfter = new DateTimeOffset(cert.NotAfter);

                req.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection {
                            new Oid("1.3.6.1.5.5.7.3.2"), // TLS Client auth
                            new Oid("1.3.6.1.5.5.7.3.1")  // TLS Server auth
                        },
                        false));


                leaf = req.Create(cert, notBefore, notAfter, GenerateSerialNumber()).CopyWithPrivateKey(rsa);
            }

            return leaf;
        }

        public static X509Certificate2 GetPublicCertificate(this X509Certificate2 cert)
        {
            return new X509Certificate2(cert.Export(X509ContentType.Cert));
        }


        public static byte[] ExportCertificateChainToPfx(this X509Certificate2 cert, string password, params X509Certificate2[] chain)
        {
            var certCollection = new X509Certificate2Collection(cert);
            if (chain != default)
            {
                certCollection.AddRange(chain);
            }
            return certCollection.Export(X509ContentType.Pkcs12, password);
        }

        public static bool IsLeaf(this X509Certificate2 leaf, X509Certificate2 root, params X509Certificate2[] intermediates)
        {
            var chain = new X509Chain(false);

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            
            chain.ChainPolicy.CustomTrustStore.Clear();
            chain.ChainPolicy.CustomTrustStore.Add(root);
            chain.ChainPolicy.CustomTrustStore.AddRange(intermediates);

            return chain.Build(leaf);
        }

        public static bool IsSignedBy(this X509Certificate2 leaf, X509Certificate2 parent)
        {
            var chain = new X509Chain(false);

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            
            chain.ChainPolicy.CustomTrustStore.Clear();
            chain.ChainPolicy.CustomTrustStore.Add(parent);

            chain.Build(leaf);

            return chain.ChainElements.Count > 0 && chain.ChainElements[0].Certificate == parent;
        }

        public static bool IsRoot(this X509Certificate2 cert) => cert.Extensions.Cast<object>()
            .Any(e => e is X509BasicConstraintsExtension constraint && constraint.CertificateAuthority);

        public static bool CanCertSign(this X509Certificate2 cert) => cert.Extensions.Cast<object>()
            .Any(e => e is X509KeyUsageExtension usage && (usage.KeyUsages & X509KeyUsageFlags.KeyCertSign) == X509KeyUsageFlags.KeyCertSign);

        public static string GetCommonName(this X509Certificate2 cert) => cert.GetNameInfo(X509NameType.SimpleName, false);

        public static byte[] GenerateSerialNumber()
        {
            ulong random = ((ulong)RandomNumberGenerator.GetInt32(Int32.MaxValue) << 32) | (uint)RandomNumberGenerator.GetInt32(Int32.MaxValue);
            return BitConverter.GetBytes(random);
        }
    }
}
