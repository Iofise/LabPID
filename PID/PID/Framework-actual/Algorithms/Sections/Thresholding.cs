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

            double T_vechi = 0;
            double t = mu;

            do
            {
                T_vechi = t;

                double P1 = 0;
                double P2 = 0;
                double suma_k_p1 = 0;
                double suma_k_p2 = 0;

                int T_int = (int)Math.Round(T_vechi);

                if (T_int < 0) T_int = 0;
                if (T_int > 255) T_int = 255;

                for (int k = 0; k <= T_int; k++)
                {
                    P1 = P1 + p[k];
                    suma_k_p1 = suma_k_p1 + k * p[k];
                }

                for (int k = T_int + 1; k < 256; k++)
                {
                    P2 = P2 + p[k];
                    suma_k_p2 = suma_k_p2 + k * p[k];
                }

                double mu1 = 0;
                double mu2 = 0;

                if (P1 > 0)
                    mu1 = suma_k_p1 / P1;
                else
                    mu1 = 0;

                if (P2 > 0)
                    mu2 = suma_k_p2 / P2;
                else
                    mu2 = 255;

                t = (mu1 + mu2) / 2.0;

            } while (Math.Abs(T_vechi - t) >= 0.5);

            return (int)Math.Round(t);
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
