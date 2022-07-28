using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Bones.X509.Tests
{
    public class CertificateHelperTests
    {
        [Fact]
        public void TestRootLeaf()
        {
            var root = X509CertificateHelper.CreateRoot("cm", "ou", "o", "l", "s");

            var leaf = root.CreateLeaf("cm2", "ou", "o", "l", "s");

            Assert.True(leaf.IsLeaf(root));
            Assert.True(leaf.IsSignedBy(root));
        } 

        [Fact]
        public void TestRootIntermediateLeaf()
        {
            var root = X509CertificateHelper.CreateRoot("cm", "ou", "o", "l", "s");

            var intermediate = root.CreateIntermediate("cm1", "ou", "o", "l", "s");

            var leaf = intermediate.CreateLeaf("cm2", "ou", "o", "l", "s");

            Assert.True(leaf.IsLeaf(root, intermediate));
            Assert.True(leaf.IsSignedBy(intermediate));

            Assert.False(leaf.IsLeaf(intermediate));
        }


        [Fact]
        public void TestBadRoot()
        {
            var root = X509CertificateHelper.CreateRoot("cm", "ou", "o", "l", "s");

            var root2 = X509CertificateHelper.CreateRoot("cm", "ou", "o", "l", "s");

            var leaf = root2.CreateLeaf("cm2", "ou", "o", "l", "s");

            Assert.False(leaf.IsLeaf(root));
        }

        [Fact]
        public void TestMissingIntermediate()
        {
            var root = X509CertificateHelper.CreateRoot("cm", "ou", "o", "l", "s");

            var intermediate1 = root.CreateIntermediate("cm1", "ou", "o", "l", "s");

            var leaf = intermediate1.CreateLeaf("cm2", "ou", "o", "l", "s");

            Assert.False(leaf.IsLeaf(root));
        }


        [Fact]
        public void TestBadIntermediate()
        {
            var root = X509CertificateHelper.CreateRoot("cm", "ou", "o", "l", "s");

            var intermediate1 = root.CreateIntermediate("cm1", "ou", "o", "l", "s");
            var intermediate2 = root.CreateIntermediate("cm2", "ou", "o", "l", "s");

            var leaf = intermediate2.CreateLeaf("cm2", "ou", "o", "l", "s");

            Assert.False(leaf.IsLeaf(root, intermediate1));
        }
    }
}
