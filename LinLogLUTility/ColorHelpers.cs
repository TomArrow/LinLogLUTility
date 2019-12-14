using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinLogLUTility
{
    public static class ColorHelpers
    {

        public static double applySRGBTransferFunction(double input) {
            return input > 0.0031308 ? 1.055 * Math.Pow(input, 1 / 2.4) - 0.055 : 12.92 * input;
        }

        public static double applyInverseSRGBTransferFunction(double input)
        {
            return input > 0.04045 ? Math.Pow((input + 0.055) / 1.055, 2.4) : input / 12.92;
        }
    }
}
