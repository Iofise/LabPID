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

            for (int y = h / 2; y < inputImage.Height - h / 2; ++y)
            {
                for (int x = w / 2; x < inputImage.Width - w / 2; ++x)
                {
                    double sumPond = 0.0;
                    for (int i = -h / 2; i <= h / 2; i++)
                    {
                        for (int j = -w / 2; j <= w / 2; j++)
                        {
                            sumPond += filter[i + h / 2, j + w / 2] * inputImage.Data[y + i, x + j, 0];
                        }
                    }
                    result.Data[y, x, 0] = Utils.Clip(sumPond);
                }
            }


            return result;
        }

    }
}