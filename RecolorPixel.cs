using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recolor_Guy
{
    public static class RecolorPixel
    {

        public static int Recolor(int color, Spectrum toHue)
        {
            byte[] rgb = BitConverter.GetBytes(color);
            rgb = new byte[] { rgb[2],
                               rgb[1],
                               rgb[0] };
            byte alpha = BitConverter.GetBytes(color)[3];

            byte p = Numbers.LargestOf(rgb);

            byte s = Numbers.MiddleOf(rgb);
            s = !(p > s + 10) ? (byte)(s * .66) : s;

            byte sh = Numbers.SmallestOf(rgb);

            byte ash = (byte)(p - s);
            ash = (byte)(ash/2);
            ash += s;

            //Pieces map
            byte[] colorPieces = { p, s, sh, ash };
            byte[] order       = { 0, 1, 2,  3   };

            switch (toHue)
            {
                case Spectrum.Red:
                    //rgb = new byte[] { p, s, sh };
                    order = new byte[] { 0, 1, 2};
                    break;

                case Spectrum.Orange:
                    //rgb = new byte[] { p, ash, sh};
                    order = new byte[] { 0, 3, 2};
                    break;

                case Spectrum.Yellow:
                    //rgb = new byte[] { p, p, sh };
                    order = new byte[] { 0, 0, 2 };
                    break;

                case Spectrum.Lime:
                    //rgb = new byte[] { ash, p, sh };
                    order = new byte[] { 3, 0, 2 };
                    break;

                case Spectrum.Green:
                    //rgb = new byte[] { s, p, sh };
                    order = new byte[] { 1, 0, 2 };
                    break;

                case Spectrum.SeaGreen:
                    //rgb = new byte[] { sh, p, ash };
                    order = new byte[] { 2, 0, 3 };
                    break;

                case Spectrum.Aqua:
                    //rgb = new byte[] { sh, p, p };
                    order = new byte[] { 2, 0, 0 };
                    break;

                case Spectrum.Denim:
                    //rgb = new byte[] { sh, ash, p };
                    order = new byte[] { 2, 3, 0 };
                    break;

                case Spectrum.Blue:
                    //rgb = new byte[] { sh, s, p };
                    order = new byte[] { 2, 1, 0 };
                    break;

                case Spectrum.Magenta:
                    //rgb = new byte[] { p, sh, p };
                    order = new byte[] { 0, 2, 0 };
                    break;

                case Spectrum.Violet:
                    //rgb = new byte[] { p, sh, ash };
                    order = new byte[] { 0, 2, 3 };
                    break;

                case Spectrum.Rose:
                    //rgb = new byte[] { p, sh, s };
                    order = new byte[] { 0, 2, 1 };
                    break;

                case Spectrum.Null:
                    alpha = 0;
                    break;

                case Spectrum.White:
                    rgb = new byte[] { (byte)(Math.Round((int)rgb[0] * .10) + 230),
                                       (byte)(Math.Round((int)rgb[1] * .10) + 230),
                                       (byte)(Math.Round((int)rgb[2] * .10) + 230)};
                    break;

                case Spectrum.Black:
                    rgb = new byte[] { (byte)(Math.Round((int)rgb[0] * .15)),
                                       (byte)(Math.Round((int)rgb[1] * .15)),
                                       (byte)(Math.Round((int)rgb[2] * .15))};
                    break;

                default:
                    break;
            }

            int newPxl = alpha << 24;
            newPxl |= colorPieces[order[0]] << 16;
            newPxl |= colorPieces[order[1]] << 8;
            newPxl |= colorPieces[order[2]] << 0;
            
            return newPxl;
        }

        public static int GetHue(int color)
        {
            //Thanks on this part to http://www.niwa.nu/2013/05/math-behind-colorspace-conversions-rgb-hsl/

            byte[] rgb = BitConverter.GetBytes(color);
            double R = rgb[2] / 255;
            double G = rgb[1] / 255;
            double B = rgb[0] / 255;

            double max = Numbers.Max(new double[] { R, G, B });
            double min = Numbers.Min(new double[] { R, G, B });

            double lum = (max + min) / 2;
            double sat = min == max ? 0 :
                         (lum > 0.5) ? (max-min)/(max+min) :
                                       (max-min)/(2.0-max-min);
            double hue;
            if (max == R) { hue = (G - B) / (max - min); }
            else if (max == G) { hue = 2.0 + (B - R) / (max - min); }
            else { hue = 4.0 + (R - G) / (max - min); }
            hue = Double.IsNaN(hue) ? 0 : hue;
            hue *= 60;
            hue = hue > 360 ? hue += 360 : hue;

            return (int)hue;
        }
    }
}
