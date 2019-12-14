using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinLogLUTility
{
    class CubeLUT1D
    {
        public static string ArrayToLUT1DString(double[] values,string singleLineComment="")
        {
            string theLUT = "";
            if(singleLineComment != "")
            {
                theLUT = "#" + singleLineComment + "\n";
            }
            theLUT += "LUT_1D_SIZE "+values.Count().ToString();
            theLUT += "\n\n";
            string tmp;
            foreach(double value in values)
            {
                tmp = value.ToString();
                theLUT += tmp + " " + tmp + " " + tmp+"\n";
            }

            return theLUT;
        }
    }
}
