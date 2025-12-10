using Algorithms.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace Algorithms.Sections
{
    public static class GeometricTransforms
    {
        private static double LinearInterpolation(double x_fractional, double f0, double f1)
        {
            return x_fractional * f1 + (1.0 - x_fractional) * f0;
        }
        private static double BilinearInterpolation(double xc, double yc, double[] f)
        {
            double x_frac = xc - Math.Floor(xc);
            double y_frac = yc - Math.Floor(yc);

            double p0 = LinearInterpolation(x_frac, f[0], f[1]);

            double p1 = LinearInterpolation(x_frac, f[2], f[3]);

            return LinearInterpolation(y_frac, p0, p1);
        }

        private static double CubicInterpolation(double x_frac, double fm1, double f0, double f1, double f2)
        {
            double x3 = x_frac * x_frac * x_frac;
            double x2 = x_frac * x_frac;

            double result =
                (-0.5 * ((-x3 + 2.0 * x2 - x_frac) * fm1 +
                         (3.0 * x3 - 5.0 * x2 + 2.0) * f0 +
                         (-3.0 * x3 + 4.0 * x2 + x_frac) * f1 +
                         (x3 - x2) * f2));

            return result;
        }

        private static double BicubicInterpolation(double xc, double yc, double[,] f)
        {
            double x_frac = xc - Math.Floor(xc);
            double y_frac = yc - Math.Floor(yc);

            double[] P = new double[4];

            for (int i = 0; i < 4; i++)
            {
                P[i] = CubicInterpolation(x_frac, f[i, 0], f[i, 1], f[i, 2], f[i, 3]);
            }

            return CubicInterpolation(y_frac, P[0], P[1], P[2], P[3]);
        }

        public static Image<Gray, byte> Scale(Image<Gray, byte> inputImage, double sx, double sy, bool useBicubic)
        {
            if (sx <= 0 || sy <= 0)
            {
                throw new ArgumentException("Factorii de scalare (sx, sy) trebuie să fie pozitivi.");
            }

            int resultHeight = (int)Math.Round(sy * inputImage.Height);
            int resultWidth = (int)Math.Round(sx * inputImage.Width);
            Image<Gray, byte> result = new Image<Gray, byte>(resultWidth, resultHeight);

            double inv_sx = 1.0 / sx;
            double inv_sy = 1.0 / sy;

            for (int y_prime = 0; y_prime < resultHeight; y_prime++)
            {
                for (int x_prime = 0; x_prime < resultWidth; x_prime++)
                {
                    double xc = x_prime * inv_sx;
                    double yc = y_prime * inv_sy;

                    int x0 = (int)Math.Floor(xc);
                    int y0 = (int)Math.Floor(yc);

                    double interpolated_value = 0;

                    if (!useBicubic)
                    {
                        if (x0 >= 0 && x0 < inputImage.Width - 1 &&
                            y0 >= 0 && y0 < inputImage.Height - 1)
                        {
                            double[] f = new double[4];

                            for (int i = 0; i < 2; i++)
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    f[2 * i + j] = inputImage.Data[y0 + i, x0 + j, 0];
                                }
                            }

                            interpolated_value = BilinearInterpolation(xc, yc, f);
                        }

                        byte final_pixel = (byte)(interpolated_value + 0.5);
                        result.Data[y_prime, x_prime, 0] = final_pixel;

                    }
                    else
                    {
                        if (x0 >= 1 && x0 < inputImage.Width - 2 &&
                            y0 >= 1 && y0 < inputImage.Height - 2)
                        {
                            double[,] f = new double[4, 4];

                            for (int i = -1; i <= 2; i++)
                            {
                                for (int j = -1; j <= 2; j++)
                                {
                                    f[i + 1, j + 1] = inputImage.Data[y0 + i, x0 + j, 0];
                                }
                            }

                            interpolated_value = BicubicInterpolation(xc, yc, f);

                            byte final_pixel = (byte)(Math.Max(0, Math.Min(255, interpolated_value)) + 0.5);
                            result.Data[y_prime, x_prime, 0] = final_pixel;
                        }
                    }

                }
            }
            return result;
        }
    }

    public static class Morphology
    {
        private static Image<Gray, byte> ApplyMorphology(Image<Gray, byte> binaryImage, int h, int w, int optiune, bool isDilation)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(binaryImage.Size);

            int height = binaryImage.Height;
            int width = binaryImage.Width;
            int h_half = h / 2;
            int w_half = w / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool conditionMet = !isDilation;

                    for (int i = -h_half; i <= h_half; i++)
                    {
                        for (int j = -w_half; j <= w_half; j++)
                        {
                            int y_clamped = Utils.Clamp(y + i, 0, height - 1);
                            int x_clamped = Utils.Clamp(x + j, 0, width - 1);

                            byte neighborValue = binaryImage.Data[y_clamped, x_clamped, 0];

                            if (isDilation)
                            {
                                byte targetValue = (optiune == 1) ? (byte)255 : (byte)0;

                                if (neighborValue == targetValue)
                                {
                                    conditionMet = true;
                                    goto SetPixel;
                                }
                            }
                            else
                            {
                                byte oppositeValue = (optiune == 1) ? (byte)0 : (byte)255;

                                if (neighborValue == oppositeValue)
                                {
                                    conditionMet = true;
                                    goto SetPixel;
                                }
                            }
                        }
                    }

                SetPixel:
                    if (isDilation)
                    {
                        byte finalColor = (optiune == 1) ? (byte)255 : (byte)0;
                        result.Data[y, x, 0] = conditionMet ? finalColor : (byte)(255 - finalColor);
                    }
                    else
                    {
                        byte finalColor = (optiune == 1) ? (byte)0 : (byte)255;
                        result.Data[y, x, 0] = conditionMet ? finalColor : (byte)(255 - finalColor);
                    }
                }
            }
            return result;
        }

        private static Image<Gray, byte> ApplyGradient(Image<Gray, byte> dilatedImage, Image<Gray, byte> erotedImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(dilatedImage.Size);

            int height = dilatedImage.Height;
            int width = dilatedImage.Width;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dilatedValue = dilatedImage.Data[y, x, 0];
                    int erodedValue = erotedImage.Data[y, x, 0];

                    int gradientValue = dilatedValue - erodedValue;

                    result.Data[y, x, 0] = Utils.Clip(gradientValue);
                }
            }
            return result;
        }

        public static Image<Gray, byte> Dilation(Image<Gray, byte> inputImage, int h, int w, int T, int optiune)
        {
            Image<Gray, byte> binaryImage = Thresholding.Binary(inputImage, T);
            return ApplyMorphology(binaryImage, h, w, optiune, isDilation: true);
        }

        public static Image<Gray, byte> Erosion(Image<Gray, byte> inputImage, int h, int w, int T, int optiune)
        {
            Image<Gray, byte> binaryImage = Thresholding.Binary(inputImage, T);
            return ApplyMorphology(binaryImage, h, w, optiune, isDilation: false);
        }

        public static Image<Gray, byte> Opening(Image<Gray, byte> inputImage, int h, int w, int T, int optiune)
        {
            Image<Gray, byte> erodedImage = Erosion(inputImage, h, w, T, optiune);
            return ApplyMorphology(erodedImage, h, w, optiune, isDilation: true);
        }

        public static Image<Gray, byte> Closing(Image<Gray, byte> inputImage, int h, int w, int T, int optiune)
        {
            Image<Gray, byte> dilatedImage = Dilation(inputImage, h, w, T, optiune);
            return ApplyMorphology(dilatedImage, h, w, optiune, isDilation: false);
        }

        private static Image<Gray, byte> GrayscaleDilation(Image<Gray, byte> inputImage, int h, int w)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            int height = inputImage.Height;
            int width = inputImage.Width;
            int h_half = h / 2;
            int w_half = w / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte maxValue = 0;
                    for (int i = -h_half; i <= h_half; i++)
                    {
                        for (int j = -w_half; j <= w_half; j++)
                        {
                            int y_clamped = Utils.Clamp(y + i, 0, height - 1);
                            int x_clamped = Utils.Clamp(x + j, 0, width - 1);
                            byte neighborValue = inputImage.Data[y_clamped, x_clamped, 0];

                            if (neighborValue > maxValue)
                            {
                                maxValue = neighborValue;
                            }
                        }
                    }
                    result.Data[y, x, 0] = maxValue;
                }
            }
            return result;
        }
        private static Image<Gray, byte> GrayscaleErosion(Image<Gray, byte> inputImage, int h, int w)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            int height = inputImage.Height;
            int width = inputImage.Width;
            int h_half = h / 2;
            int w_half = w / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte minValue = 255;

                    for (int i = -h_half; i <= h_half; i++)
                    {
                        for (int j = -w_half; j <= w_half; j++)
                        {
                            int y_clamped = Utils.Clamp(y + i, 0, height - 1);
                            int x_clamped = Utils.Clamp(x + j, 0, width - 1);

                            byte neighborValue = inputImage.Data[y_clamped, x_clamped, 0];

                            if (neighborValue < minValue)
                            {
                                minValue = neighborValue;
                            }
                        }
                    }
                    result.Data[y, x, 0] = minValue;
                }
            }
            return result;
        }

        public static Image<Gray, byte> MorphologicalGradient(Image<Gray, byte> inputImage, int h, int w)
        {
            Image<Gray, byte> dilatedImage = GrayscaleDilation(inputImage, h, w);
            Image<Gray, byte> erodedImage = GrayscaleErosion(inputImage, h, w);

            return ApplyGradient(dilatedImage, erodedImage);
        }
    }

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

        public static Image<Gray, byte> SobelDiagonal(Image<Gray, byte> inputImage, double T, double deviation)
        {
            double[,] Sx = new double[,]
            {
                { -1.0, 0.0, 1.0 },
                { -2.0, 0.0, 2.0 },
                { -1.0, 0.0, 1.0 }
            };

            double[,] Sy = new double[,]
            {
                { -1.0, -2.0, -1.0 },
                { 0.0, 0.0, 0.0 },
                { 1.0, 2.0, 1.0 }
            };

            Image<Gray, byte> resultImage = new Image<Gray, byte>(inputImage.Size);

            int w = inputImage.Width;
            int h = inputImage.Height;

            double devRad = deviation * Math.PI / 180.0;

            double angle45 = 45.0 * Math.PI / 180.0;
            ///double angle135 = 135.0 * Math.PI / 180.0;
            ///double angleMinus45 = -45.0 * Math.PI / 180.0;
            ///double angleMinus135 = -135.0 * Math.PI / 180.0;

            Image<Gray, float> fxImage = new Image<Gray, float>(w, h);
            Image<Gray, float> fyImage = new Image<Gray, float>(w, h);

            int half_size = 1;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double fx = 0.0;
                    double fy = 0.0;

                    for (int i = -half_size; i <= half_size; i++)
                    {
                        for (int j = -half_size; j <= half_size; j++)
                        {
                            int y_clamped = Utils.Clamp(y + i, 0, h - 1);
                            int x_clamped = Utils.Clamp(x + j, 0, w - 1);

                            double p = inputImage.Data[y_clamped, x_clamped, 0];

                            fx += p * Sx[i + half_size, j + half_size];
                            fy += p * Sy[i + half_size, j + half_size];
                        }
                    }

                    fxImage.Data[y, x, 0] = (float)fx;
                    fyImage.Data[y, x, 0] = (float)fy;
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double fx = fxImage.Data[y, x, 0];
                    double fy = fyImage.Data[y, x, 0];

                    double n_grad = Math.Sqrt(fx * fx + fy * fy);

                    if (n_grad > T)
                    {
                        double theta = Math.Atan2(fy, fx);

                        bool is45 = false;

                        /*
                        if (Math.Abs(theta - angle45) < devRad ||
                            Math.Abs(theta - angle135) < devRad ||
                            Math.Abs(theta - angleMinus135) < devRad ||
                            Math.Abs(theta - angleMinus45) < devRad)
                        {
                            is45 = true;
                        }
                        */

                        if (Math.Abs(theta - angle45) < devRad)
                        {
                            is45 = true;
                        }

                        if (is45)
                        {
                            resultImage.Data[y, x, 0] = 255;
                        }
                        else
                        {
                            resultImage.Data[y, x, 0] = 0;
                        }
                    }
                    else
                    {
                        resultImage.Data[y, x, 0] = 0;
                    }
                }
            }

            return resultImage;
        }

    }
}