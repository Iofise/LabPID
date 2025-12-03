using Algorithms.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace Algorithms.Sections
{

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

        private static Image<Gray, byte> ApplyGradient(Image<Gray, byte> dilatedImage, Image<Gray, byte> erotedImage, int h, int w, int dilatation, int erosion)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(dilatedImage.Size);

            int height = dilatedImage.Height;
            int width = dilatedImage.Width;
            int h_half = h / 2;
            int w_half = w / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int i = -h_half; i <= h_half; i++)
                    {
                        for (int j = -w_half; j <= w_half; j++)
                        {
                            int y_clamped = Utils.Clamp(y + i, 0, height - 1);
                            int x_clamped = Utils.Clamp(x + j, 0, width - 1);

                            byte neighborValueD = dilatedImage.Data[y_clamped, x_clamped, 0];
                            byte neighborValueE = erotedImage.Data[y_clamped, x_clamped, 0];
                        }
                    }

                SetPixel:
                        byte finalColor = (dilatation == 1) ? (byte)255 : (byte)0;
                        result.Data[y, x, 0] = (byte)(255 - finalColor);
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

        public static Image<Gray, byte> MorphologicalGradient(Image<Gray, byte> inputImage, int h, int w, int T,int optiune, int dilatation, int erosion)
        {
            Image<Gray, byte> dilatedImage = Dilation(inputImage, h, w, T, optiune);
            Image<Gray, byte> erodedImage = Erosion(inputImage, h, w, T, optiune);
            return ApplyGradient(dilatedImage, erodedImage, h, w, erosion, dilatation);
        }

        public static Image<Gray, byte> MorphologicalGradient(int h, int w, int optiune, int dilatation, int erosion)
        {
            throw new NotImplementedException();
        }

        public static Image<Gray, byte> MorphologicalGradient(Image<Gray, byte> grayInitialImage, int h, int w, int optiune, int dilatation, int erosion)
        {
            throw new NotImplementedException();
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