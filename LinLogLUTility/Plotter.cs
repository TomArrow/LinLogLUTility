using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LinLogLUTility
{
    class HarryPlotter
    {
        public static Bitmap PlotIt(int width, int height, int rangeX, int rangeY, double parameterA,Func<double,double,double> theFunction)
        {
            Bitmap zaBitmap = new Bitmap(width, height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData zeData = zaBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int size = zeData.Stride * height;
            byte[] imageData = new byte[size];
            Marshal.Copy(zeData.Scan0, imageData, 0, size);


            double xRatio = (double)rangeX / (width-1);
            double yRatio = (double)rangeY / (height-1);

            int tmp;

            for(int i = 0; i < width; i++)
            {
                tmp = (int)Math.Round(theFunction(i * xRatio, parameterA)/yRatio);
                if (tmp >= height)
                {
                    continue;
                }
                tmp = height - tmp -1;
                int pixelPosition = zeData.Stride * tmp + i * 3;
                imageData[pixelPosition] = 255; //blue
                imageData[pixelPosition+1] = 255; //green
                imageData[pixelPosition+2] = 255; //red
            }


            Marshal.Copy(imageData, 0, zeData.Scan0,   size);
            zaBitmap.UnlockBits(zeData);
            return zaBitmap;
        }
    }
}
