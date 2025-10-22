using Emgu.CV;
using Emgu.CV.Structure;

using System.Windows;
using PointCollection = System.Windows.Media.PointCollection;

namespace Framework.Utilities
{
    class DataProvider
    {
        public static Image<Gray, byte> GrayInitialImage { get; set; }
        public static Image<Gray, byte> GrayProcessedImage { get; set; }
        public static Image<Bgr, byte> ColorInitialImage { get; set; }
        public static Image<Bgr, byte> ColorProcessedImage { get; set; }

        public static bool MagnifierOn { get; set; }
        public static bool RowColorLevelsOn { get; set; }
        public static bool ColumnColorLevelsOn { get; set; }
        public static bool InitialHistogramOn { get; set; }
        public static bool ProcessedHistogramOn { get; set; }
        public static bool HistogramOn { get; set; }
        public static bool SliderOn { get; set; }

        public static Point LastMouseClick { get; set; } = new Point(0, 0);
        public static PointCollection MouseClickCollection { get; set; } = new PointCollection();
    }
}