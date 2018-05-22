using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RSACipher
{
    class RSACore
    {
        private BigInteger n;
        private BigInteger d;
        private BigInteger e;

        public long initTime = 0;

        public RSACore(int precision)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            PrimeGenerator primeGenerator = new PrimeGenerator(precision);
            
            //generate p & q
            BigInteger p = primeGenerator.Get1024BitPrime();
            BigInteger q;
            while ((q = primeGenerator.Get1024BitPrime()) == p) { }
            
            //calculate n & fi
            n = p * q;
            BigInteger fi = (p - 1) * (q - 1);

            //find e
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] temp = new byte[256];
                do
                {
                    rng.GetBytes(temp);
                    e = new BigInteger(temp);
                }
                while (e < 1 || e > fi - 1 || BigInteger.GreatestCommonDivisor(e, fi) != 1);
            }
            //calculate d
            d = ModInverse(e, fi);

            s.Stop();
            initTime = s.ElapsedMilliseconds;
        }

        private BigInteger ModInverse(BigInteger a, BigInteger b)
        {
            BigInteger u = 1;
            BigInteger w = a;
            BigInteger x = 0;
            BigInteger z = b;
            BigInteger q;
            while (w != 0)
            {
                if (w < z)
                {
                    BigInteger temp;
                    temp = u;
                    u = x;
                    x = temp;
                    temp = w;
                    w = z;
                    z = temp;
                }
                q = w / z;
                u = u - (q * x);
                w = w - (q * z);
            }
            if (z != 1)
                throw new FormatException("No possible inverse modulo");
            if (x < 0)
                x = x + b;
            return x;
        }

        public EncryptedImage Encrypt(byte[] inputImage)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            int partsNum = GetPartsNumber(inputImage);            
            int partSize = inputImage.Length / partsNum;

            byte[][] imageParts = inputImage.Split(partSize).Select(sl => sl.ToArray()).ToArray();
            partsNum = imageParts.Length;

            EncryptedImage output = new EncryptedImage(partsNum);

            Parallel.For(0, partsNum, i => {
                BigInteger tempNumber = new BigInteger(imageParts[i]);
                int sign = tempNumber.Sign;
                if (sign == -1)
                    tempNumber *= -1;
                tempNumber = BigInteger.ModPow(tempNumber, e, n);
                output.AddEncryptedNumber(tempNumber, sign, imageParts[i].Length,i);
            });

            output.PrivateKey = Tuple.Create(d, n);

            s.Stop();
            initTime = s.ElapsedMilliseconds;
            return output;
        }

        public byte[] Decrypt(EncryptedImage image)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            byte[][] output = new byte[image.EncryptedData.Count][];

            Parallel.For(0, image.EncryptedData.Count, i => {
                BigInteger tempNumber = image.EncryptedData[i].Item1;
                tempNumber = BigInteger.ModPow(tempNumber, image.PrivateKey.Item1, image.PrivateKey.Item2);
                if (image.EncryptedData[i].Item2 == -1)
                    tempNumber *= -1;
                var tempArray = tempNumber.ToByteArray().ToList();

                if (tempArray.Count != image.EncryptedData[i].Item3)
                    tempArray.Add((image.EncryptedData[i].Item2 == -1) ? (byte)0xFF : (byte)0x00);
                output[i] = tempArray.ToArray();
            });

            s.Stop();
            initTime = s.ElapsedMilliseconds;
            return output.SelectMany(sm => sm).ToArray();
        }

        private int GetPartsNumber(byte[] inputImage)
        {
            int partsNum = 1;
            while (true)
            {
                bool numFound = true;
                int partsSize = (inputImage.Length / partsNum);
                var parts = inputImage.Split(partsSize).Select(s => s);

                foreach (var part in parts)
                {
                    BigInteger tempNumber = new BigInteger(part.ToArray());
                    if (tempNumber.Sign == -1)
                        tempNumber *= -1;
                    if (tempNumber > n)
                    {
                        partsNum++;
                        numFound = false;
                        break;
                    }
                }
                if (numFound)
                    break;
            }
            return partsNum;
        }
    }
}
