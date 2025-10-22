using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace Algorithms.Sections
{
    public class Thresholding
    {
        #region Intermeans
        public static int CalculIntermeans(Image<Gray, byte> inputImage)
        {
            int width = inputImage.Width;
            int height = inputImage.Height;
            int n = width * height;
            int[] historgram = new int[256];
            for (int i = 0; i < 256; i++)
                historgram[i] = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    historgram[inputImage.Data[y, x, 0]]++;
                }
            }

            double[] p = new double[256];
            double mu = 0;

            for (int k = 0; k < 256; k++)
            {
                p[k] = (double)historgram[k] / n;
                mu += k * p[k];
            }

            double T = 0;
            double t = mu;

            do
            {
                T = (int)t;
                double P1 = 0;
                double P2 = 0;
                double mu1 = 0;
                double mu2 = 0;

                for (int k = 0; k <= T; k++)
                {
                    P1 = P1 + p[k];
                    mu1 = mu1 + k * p[k];
                }

                for (int k = (int)T + 1; k < 256; k++)
                {
                    P2 = P2 + p[k];
                    mu2 = mu2 + k * p[k];
                }

                if (P1 > 0)
                    mu1 = mu1 / P1;
                if (P2 > 0)
                    mu2 = mu2 / P2;

                t = (mu1 + mu2) / 2;

            } while (Math.Abs(T - t) >= 0.5);

            return (int)T;
        }
        public static Image<Gray, byte> Binary(Image<Gray, byte> inputImage, int threshold)
        {
            return inputImage.ThresholdBinary(new Gray(threshold), new Gray(255));
        }

        public static Image<Gray, byte> Intermeans(Image<Gray, byte> inputImage)
        {
            int threshold = CalculIntermeans(inputImage);
            return Binary(inputImage, threshold);
        }
        #endregion
    }
}
