using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RSACipher
{
    class PrimeGenerator
    {
        private int iterNum;
        private Random random;

        public PrimeGenerator(int precision)
        {
            iterNum = precision;
            random = new Random();
        }

        public BigInteger Get1024BitPrime()
        {
            byte[] bytes = new byte[128];
            BigInteger output;
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                do
                {
                    rng.GetBytes(bytes);
                    output = new BigInteger(bytes);
                }
                while (!CheckPrime(output));
            }
            return output;
        }

        private bool CheckPrime(BigInteger number)
        {
            if (number < 2)
                return false;
            if (number > 0 && number % 2 == 0)
                return false;

            BigInteger s = number - 1;
            while (s % 2 == 0)
                s /= 2;

            bool loopResult = true;

            Parallel.For(0, iterNum, (fl, loopState) => {
                BigInteger a = (random.Next() % (number - 1)) + 1;
                BigInteger temp = s;
                BigInteger mod = BigInteger.ModPow(a, temp, number);
                while (temp != number - 1 && mod != 1 && mod != number - 1)
                {
                    mod = (mod * mod) % number;
                    temp *= 2;
                }
                if (mod != number - 1 && temp % 2 == 0)
                {
                    loopResult = false;
                    loopState.Stop();
                }
            });
            return loopResult;
        }
    }
}
