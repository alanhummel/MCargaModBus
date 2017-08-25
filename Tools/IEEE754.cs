using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCargaModBus.Tools
{
    class IEEE754
    {
        // Obtém um número float e converte em dois inteiros (16bits)
		public static int[] GetBytesSingle( bool isBigEndian, float number )
		{
			int[] result;
			byte[ ] byteArray = BitConverter.GetBytes( number );
			if (isBigEndian)
				Array.Reverse(byteArray); 
			string byteString = BitConverter.ToString (byteArray);
			// Remove os "-"
			byteString = byteString.Replace ("-", "");

			string firstPartString = byteString.Substring (0, 4);
			string secondPartString = byteString.Substring (4, 4);
			result = new int[2];
			result[0] = int.Parse(firstPartString, System.Globalization.NumberStyles.HexNumber);
			result[1] = int.Parse(secondPartString, System.Globalization.NumberStyles.HexNumber);

			return result;
		}

        // Lê dois números de 16 bits e converte para um float
        public static float getNumberFromBytes(bool isBigEndian, int firstValue16bits, int secondValue16bits)
        {
            float result = 0;

            string firstPartString = firstValue16bits.ToString("X");
            string secondPartString = secondValue16bits.ToString("X");

            string byteString = firstPartString + secondPartString;

            byte[] byteArray = StringToByteArray(byteString);
            if (isBigEndian)
            {
                Array.Reverse(byteArray);
            }
            result = BitConverter.ToSingle(byteArray, 0);
            return result;
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static int[] addOneValue(bool isBigEndian, int firstValue16bits, int secondValue16bits)
        {
            int[] result = new int[2];

            string firstPartString = firstValue16bits.ToString("X");
            string secondPartString = secondValue16bits.ToString("X");

            string byteString = firstPartString + secondPartString;
            int intAgain = 0;
            try {
                intAgain = int.Parse(byteString, System.Globalization.NumberStyles.HexNumber);
            }
            catch
            { }
            intAgain++;

            byte[] byteArray = BitConverter.GetBytes(intAgain);

            if (isBigEndian)
                Array.Reverse(byteArray);
            // Remove os "-"
            byteString = BitConverter.ToString(byteArray);
            byteString = byteString.Replace("-", "");

            firstPartString = byteString.Substring(0, 4);
            secondPartString = byteString.Substring(4, 4);
            result = new int[2];
            result[0] = int.Parse(firstPartString, System.Globalization.NumberStyles.HexNumber);
            result[1] = int.Parse(secondPartString, System.Globalization.NumberStyles.HexNumber);


            return result;
        }
    }
}
