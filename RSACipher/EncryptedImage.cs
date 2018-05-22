using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RSACipher
{
    [Serializable]
    class EncryptedImage
    {
        private PrivateKeyStruct privateKey;
        public Tuple<BigInteger, BigInteger> PrivateKey
        {
            get { return Tuple.Create(privateKey.d,privateKey.n); }
            set { privateKey = new PrivateKeyStruct() {d = value.Item1, n = value.Item2 }; }
        }

        private EncrypedNumberStruct[] encryptedData;

        public List<Tuple<BigInteger,int, int>> EncryptedData
        {
            get { return encryptedData.Select(s => Tuple.Create(s.encryptedNumber,s.originalSign,s.partSize)).ToList(); }
        }


        public EncryptedImage(int size)
        {
            encryptedData = new EncrypedNumberStruct[size];
        }

        public void AddEncryptedNumber(BigInteger number, int sign, int size, int index)
        {
            encryptedData[index] = new EncrypedNumberStruct() { encryptedNumber = number, originalSign = sign, partSize = size };
        }

        [Serializable]
        private struct EncrypedNumberStruct
        {
            public BigInteger encryptedNumber;
            public int originalSign;
            public int partSize;
        }

        [Serializable]
        private struct PrivateKeyStruct
        {
            public BigInteger d;
            public BigInteger n;
        }
    }
}
