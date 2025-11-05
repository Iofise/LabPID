using Algorithms.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace Algorithms.Sections
{
    public class Filters
    {

        public static Image<Gray, byte> ApplyFilter(Image<Gray, byte> inputImage, double[,] filter)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            int w = filter.GetLength(1);
            int h = filter.GetLength(0);
            int h_half = h / 2;
            int w_half = w / 2;

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    double sumPond = 0.0;
                    for (int i = -h_half; i <= h_half; i++)
                    {
                        for (int j = -w_half; j <= w_half; j++)
                        {
                            int y_clamped = Utils.Clamp(y + i, 0, inputImage.Height - 1);
                            int x_clamped = Utils.Clamp(x + j, 0, inputImage.Width - 1);

                            sumPond += filter[i + h_half, j + w_half] * inputImage.Data[y_clamped, x_clamped, 0];
                        }
                    }
                    result.Data[y, x, 0] = Utils.Clip(sumPond);
                }
            }

            return result;
        }
        public static double[,] GaussMask1D(double sigma)
        {
            int l = (int)Math.Ceiling(4 * sigma);
            if (l % 2 == 0)
            {
                l++;
            }

            double[,] mask = new double[1, l];
            double sum = 0.0;

            double constant = 1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma);
            double sigma2 = 2 * sigma * sigma;

            for (int i = 0; i < l; i++)
            {
                double z = i - l / 2;

                double exponent = -(z * z) / sigma2;
                double value = constant * Math.Exp(exponent);

                mask[0, i] = value;
                sum += value;
            }

            for (int i = 0; i < l; i++)
            {
                mask[0, i] /= sum;
            }

            return mask;
        }

        public static Image<Gray, byte> GaussFilteringSeparated(Image<Gray, byte> initialImage, double sigmaX, double sigmaY)
        {
            double[,] maskX = GaussMask1D(sigmaX);
            Image<Gray, byte> intermediateImage = ApplyFilter(initialImage, maskX);

            double[,] maskY1D = GaussMask1D(sigmaY);

            int lY = maskY1D.GetLength(1);
            double[,] maskY = new double[lY, 1];
            for (int i = 0; i < lY; i++)
            {
                maskY[i, 0] = maskY1D[0, i];
            }

            Image<Gray, byte> resultImage = ApplyFilter(intermediateImage, maskY);

            return resultImage;
        }

        public static Image<Bgr, byte> GaussColorFilteringSeparated(Image<Bgr, byte> initialImage, double sigmaX, double sigmaY)
        {
            Image<Gray, byte>[] channels = initialImage.Split();

            Image<Gray, byte> filteredB = GaussFilteringSeparated(channels[0], sigmaX, sigmaY);
            Image<Gray, byte> filteredG = GaussFilteringSeparated(channels[1], sigmaX, sigmaY);
            Image<Gray, byte> filteredR = GaussFilteringSeparated(channels[2], sigmaX, sigmaY);

            Image<Bgr, byte> resultImage = new Image<Bgr, byte>(new Image<Gray, byte>[] { filteredB, filteredG, filteredR });

            return resultImage;
        }
    }
}