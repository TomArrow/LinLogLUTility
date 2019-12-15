using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinLogLUTility
{
    class StopStepTester
    {

        Func<double, double, double> functionToTest;

        public StopStepTester(Func<double,double,double> argFunctionToTest)
        {
            functionToTest = argFunctionToTest;
        }

        public double CallThis(double input,double parameterA)
        {
            double lower = Math.Pow(2,(Math.Log(input, 2)-0.5));
            double upper = Math.Pow(2,(Math.Log(input, 2)+0.5));
            return functionToTest(upper, parameterA) - functionToTest(lower, parameterA);
        }
    }
}
