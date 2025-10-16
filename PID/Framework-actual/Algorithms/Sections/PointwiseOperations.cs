using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Diagnostics.Contracts;
using System.Windows.Forms;

namespace Algorithms.Sections
{
    public class PointwiseOperations
    {
        private static byte Clip(double value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;

            value += 0.5; // rounding
            return (byte)value;
        }

        public static Image<Gray, byte> ApplyLUT(Image<Gray, byte> inputImage, byte[] LUT)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[y, x, 0] = LUT[inputImage.Data[y, x, 0]];
                }
            }
            return result;
        }

        public static byte[] ContrastBrightness(double a, double b)
        {
            byte[] LUT = new byte[256];

            for (int r = 0; r < 256; ++r)
            {
                double result = a * r + b;

                LUT[r] = Clip(result);
            }

            return LUT;
        }

        #region Gamma Correction
        public static byte[] GammaLUT(double gamma)
        {
            var LUT = new byte[256];

            var c = 255.0 / Math.Pow(255.0, gamma);

            for (var i = 0; i < 256; ++i)
                LUT[i] = Clip(c * Math.Pow(i, gamma));

            return LUT;
        }

        #endregion

    }
}