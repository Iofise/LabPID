using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;

namespace Algorithms.Utilities
{
    public class Utils
    {
        #region Compute histogram
        public static int[] ComputeHistogram(Image<Gray, byte> inputImage)
        {
            int[] histogram = new int[256];

            Parallel.For(0, inputImage.Height, (int y) =>
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    ++histogram[inputImage.Data[y, x, 0]];
                }
            });

            return histogram;
        }
        #endregion

        #region Clip
        public static byte Clip(double value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;

            value += 0.5;
            return (byte)value;
        }
        #endregion

        #region Clamp
        public static int Clamp(int val, int min, int max)
        {
            if (val < min)
            {
                return min;
            }
            if (val > max)
            {
                return max;
            }
            return val;
        }
        #endregion

        
    }
}