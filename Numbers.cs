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
            return colorBytes.Max();
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
        }
    }
}
