namespace System.Security.Cryptography.X509Certificates
{
    public class X509AuthorityKeyIdentifier
    {
        const string _oid = "2.5.29.35";
        byte[] _authorityKeyIdentifier;
        bool _critical;

        public X509AuthorityKeyIdentifier(byte[] X509AuthorityKeyIdentifier, bool critical)
        {
            _authorityKeyIdentifier = X509AuthorityKeyIdentifier;
            _critical = critical;
        }

        public static implicit operator X509Extension(X509AuthorityKeyIdentifier ex)
        {
            var segment = new ArraySegment<byte>(ex._authorityKeyIdentifier, 2, ex._authorityKeyIdentifier.Length - 2);
            var authorityKeyIdentifer = new byte[segment.Count + 4];
                authorityKeyIdentifer[0] = 0x30;
                authorityKeyIdentifer[1] = 0x16;
                authorityKeyIdentifer[2] = 0x80;
                authorityKeyIdentifer[3] = 0x14;
            segment.CopyTo(authorityKeyIdentifer, 4);
            return new X509Extension(_oid, authorityKeyIdentifer, ex._critical);
        }
    }
}
