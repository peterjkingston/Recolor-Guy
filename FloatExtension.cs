using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recolor_Guy
{
    public static class FloatExtension
    {
        /// <summary>
        /// Tests equality with a certain amount of precision. Default to smallest possible double
        /// </summary>

        ///first value ///second value ///optional, smallest possible double value ///
        public static bool AlmostEquals(this float a, float b, double precision = float.Epsilon)
        {
            return Math.Abs(a - b) <= precision;
        }
    }
}
