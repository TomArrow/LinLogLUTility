using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinLogLUTility
{
    class TransferFunctionV1
    {


        //y = log[a+1,a*x+1]
        public static double LinToLog(double input, double parameterA)
        {
            return Math.Log(parameterA*input+1, parameterA + 1);
        }

        // x = ((a + 1)^y - 1)/a
        public static double LogToLin(double input, double parameterA)
        {
            return (Math.Pow(parameterA+1,input)-1)/parameterA;
        }
    }
}
