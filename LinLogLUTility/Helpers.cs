using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace LinLogLUTility
{
    public static class Helpers
    {

        static public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static double findParameter(double pointX, double pointY, Func<double, double, double> transferFunction, out double precision)
        {
            double startValue = 0.000001;
            double theValue = startValue;
            double pointYPrecision = 0.0000001;

            double result = pointY + pointYPrecision + 1; // just setting to something that will not end the loop prematurely, the value set here itself is irrelevant.

            double multiplier = 1;
            Int64 iters = 0;
            Int64 precisionDecreaseThreshold = 1000000;
            Int64 nextPrecisiondecrease = precisionDecreaseThreshold;

            bool? wasSmaller = null;
            do
            {
                if(result < pointY)
                {
                    theValue /= 1*(1+multiplier);
                    if (wasSmaller == false)
                    {
                        multiplier /= 2;
                    }
                    wasSmaller = true;
                }
                else if(result > pointY)
                {
                    theValue *= 1 * (1 + multiplier);
                    if (wasSmaller == true)
                    {
                        multiplier /= 2;
                    }
                    wasSmaller = false;
                } else
                {
                    // nothing to do here
                }
                result = transferFunction(pointX, theValue);
                iters++;
                if (iters > nextPrecisiondecrease)
                {
                    // Avoid infinite loop
                    // TODO notify user of reduced precision
                    pointYPrecision *= 10;
                    nextPrecisiondecrease += precisionDecreaseThreshold;
                }
            } while (Math.Abs(result-pointY)> pointYPrecision);

            precision = pointYPrecision;

            return theValue;
        }
    }
}
