using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recolor_Guy
{
    public static class Numbers
    {
        public static double Max(params double[] numbers)
        {
            return numbers.Max();
        }

        public static double Min(params double[] numbers)
        {
            return numbers.Min();
        }

        public static byte LargestOf(byte[] colorBytes)
        {
            byte result = colorBytes.Max();

            /*
            if (colorBytes[0] > colorBytes[1] && colorBytes[0] > colorBytes[2]) { result = colorBytes[0]; }
            if (colorBytes[1] > colorBytes[0] && colorBytes[1] > colorBytes[2]) { result = colorBytes[1]; }
            if (colorBytes[2] > colorBytes[1] && colorBytes[2] > colorBytes[0]) { result = colorBytes[2]; }
            */
            return result;
        }

        public static byte MiddleOf(byte[] bytes)
        {
  
            byte valR = bytes[2];
            byte valG = bytes[1];
            byte valB = bytes[0];

            if ((valR <= valG && valR >= valB) || (valR <= valB && valR >= valG)) { return valR; }
            if ((valG <= valR && valG >= valB) || (valG <= valB && valG >= valR)) { return valG; }
            if ((valB <= valG && valB >= valR) || (valB <= valR && valB >= valG)) { return valB; }
            return 0;
        }

        public static byte SmallestOf(byte[] bytes)
        {
            return bytes.Min();
            /*
            byte valR = bytes[2];
            byte valG = bytes[1];
            byte valB = bytes[0];

            if (valR <= valG && valR <= valB) { return valR; }
            if (valG <= valR && valG <= valB) { return valG; }
            if (valB <= valG && valB <= valR) { return valB; }
            
            return 0;
            */
        }
    }
}
