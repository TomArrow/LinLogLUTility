using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinLogLUTility
{
    public static class Helpers
    {

        public static double findParameter(double pointX, double pointY, Func<double, double, double> transferFunction)
        {
            double startValue = 0.000001;
            double theValue = startValue;
            double pointYPrecision = 0.00001;

            double result = pointY + pointYPrecision + 1; // just setting to something that will not end the loop prematurely, the value set here itself is irrelevant.

            double multiplier = 1;

            do
            {
                if(result < pointY)
                {
                    theValue /= 1*(1+multiplier);
                }
                else if(result > pointY)
                {
                    theValue *= 5 * (1 + multiplier);
                    multiplier /= 1.1;
                } else
                {
                    // nothing to do here
                }
                result = transferFunction(pointX, theValue);
            } while (Math.Abs(result-pointY)> pointYPrecision);

            return theValue;
        }
    }
}
