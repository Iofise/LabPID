using Algorithms.Sections;
using Algorithms.Tools;
using Algorithms.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using Framework.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Framework.Converters.ImageConverter;
using static Framework.Utilities.DataProvider;
using static Framework.Utilities.DrawingHelper;
using static Framework.Utilities.FileHelper;

namespace Framework.ViewModel
{
    public class MenuCommands : BaseVM
    {
        private readonly MainVM _mainVM;

        public MenuCommands(MainVM mainVM)
        {
            _mainVM = mainVM;
        }

        private ImageSource InitialImage
        {
            get => _mainVM.InitialImage;
            set => _mainVM.InitialImage = value;
        }

        private ImageSource ProcessedImage
        {
            get => _mainVM.ProcessedImage;
            set => _mainVM.ProcessedImage = value;
        }

        private double ScaleValue
        {
            get => _mainVM.ScaleValue;
            set => _mainVM.ScaleValue = value;
        }

        #region File

        #region Load grayscale image
        private RelayCommand _loadGrayscaleImageCommand;
        public RelayCommand LoadGrayscaleImageCommand
        {
            get
            {
                if (_loadGrayscaleImageCommand == null)
                    _loadGrayscaleImageCommand = new RelayCommand(LoadGrayscaleImage);
                return _loadGrayscaleImageCommand;
            }
        }

        private void LoadGrayscaleImage(object parameter)
        {
            Clear(parameter);

            string fileName = LoadFileDialog("Select a grayscale picture");
            if (fileName != null)
            {
                GrayInitialImage = new Image<Gray, byte>(fileName);
                InitialImage = Convert(GrayInitialImage);
            }
        }
        #endregion

        #region Load color image
        private ICommand _loadColorImageCommand;
        public ICommand LoadColorImageCommand
        {
            get
            {
                if (_loadColorImageCommand == null)
                    _loadColorImageCommand = new RelayCommand(LoadColorImage);
                return _loadColorImageCommand;
            }
        }

        private void LoadColorImage(object parameter)
        {
            Clear(parameter);

            string fileName = LoadFileDialog("Select a color picture");
            if (fileName != null)
            {
                ColorInitialImage = new Image<Bgr, byte>(fileName);
                InitialImage = Convert(ColorInitialImage);
            }
        }
        #endregion

        #region Save processed image
        private ICommand _saveProcessedImageCommand;
        public ICommand SaveProcessedImageCommand
        {
            get
            {
                if (_saveProcessedImageCommand == null)
                    _saveProcessedImageCommand = new RelayCommand(SaveProcessedImage);
                return _saveProcessedImageCommand;
            }
        }

        private void SaveProcessedImage(object parameter)
        {
            if (GrayProcessedImage == null && ColorProcessedImage == null)
            {
                MessageBox.Show("If you want to save your processed image, " +
                    "please load and process an image first!");
                return;
            }

            string imagePath = SaveFileDialog("image.jpg");
            if (imagePath != null)
            {
                GrayProcessedImage?.Bitmap.Save(imagePath, GetJpegCodec("image/jpeg"), GetEncoderParameter(Encoder.Quality, 100));
                ColorProcessedImage?.Bitmap.Save(imagePath, GetJpegCodec("image/jpeg"), GetEncoderParameter(Encoder.Quality, 100));
                Process.Start(imagePath);
            }
        }
        #endregion

        #region Exit
        private ICommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new RelayCommand(Exit);
                return _exitCommand;
            }
        }

        private void Exit(object parameter)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #endregion

        #region Edit

        #region Remove drawn shapes from initial canvas
        private ICommand _removeInitialDrawnShapesCommand;
        public ICommand RemoveInitialDrawnShapesCommand
        {
            get
            {
                if (_removeInitialDrawnShapesCommand == null)
                    _removeInitialDrawnShapesCommand = new RelayCommand(RemoveInitialDrawnShapes);
                return _removeInitialDrawnShapesCommand;
            }
        }

        private void RemoveInitialDrawnShapes(object parameter)
        {
            RemoveUiElements(parameter as Canvas);
        }
        #endregion

        #region Remove drawn shapes from processed canvas
        private ICommand _removeProcessedDrawnShapesCommand;
        public ICommand RemoveProcessedDrawnShapesCommand
        {
            get
            {
                if (_removeProcessedDrawnShapesCommand == null)
                    _removeProcessedDrawnShapesCommand = new RelayCommand(RemoveProcessedDrawnShapes);
                return _removeProcessedDrawnShapesCommand;
            }
        }

        private void RemoveProcessedDrawnShapes(object parameter)
        {
            RemoveUiElements(parameter as Canvas);
        }
        #endregion

        #region Remove drawn shapes from both canvases
        private ICommand _removeDrawnShapesCommand;
        public ICommand RemoveDrawnShapesCommand
        {
            get
            {
                if (_removeDrawnShapesCommand == null)
                    _removeDrawnShapesCommand = new RelayCommand(RemoveDrawnShapes);
                return _removeDrawnShapesCommand;
            }
        }

        private void RemoveDrawnShapes(object parameter)
        {
            var canvases = (object[])parameter;
            RemoveUiElements(canvases[0] as Canvas);
            RemoveUiElements(canvases[1] as Canvas);
        }
        #endregion

        #region Clear initial canvas
        private ICommand _clearInitialCanvasCommand;
        public ICommand ClearInitialCanvasCommand
        {
            get
            {
                if (_clearInitialCanvasCommand == null)
                    _clearInitialCanvasCommand = new RelayCommand(ClearInitialCanvas);
                return _clearInitialCanvasCommand;
            }
        }

        private void ClearInitialCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);

            GrayInitialImage = null;
            ColorInitialImage = null;
            InitialImage = null;
        }
        #endregion

        #region Clear processed canvas
        private ICommand _clearProcessedCanvasCommand;
        public ICommand ClearProcessedCanvasCommand
        {
            get
            {
                if (_clearProcessedCanvasCommand == null)
                    _clearProcessedCanvasCommand = new RelayCommand(ClearProcessedCanvas);
                return _clearProcessedCanvasCommand;
            }
        }

        private void ClearProcessedCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);

            GrayProcessedImage = null;
            ColorProcessedImage = null;
            ProcessedImage = null;
        }
        #endregion

        #region Closing all open windows and clear both canvases
        private ICommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                    _clearCommand = new RelayCommand(Clear);
                return _clearCommand;
            }
        }

        private void Clear(object parameter)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window != Application.Current.MainWindow)
                {
                    window.Close();
                }
            }

            ScaleValue = 1;

            var canvases = (object[])parameter;
            ClearInitialCanvas(canvases[0] as Canvas);
            ClearProcessedCanvas(canvases[1] as Canvas);
        }
        #endregion

        #endregion

        #region Tools

        #region Magnifier
        private ICommand _magnifierCommand;
        public ICommand MagnifierCommand
        {
            get
            {
                if (_magnifierCommand == null)
                    _magnifierCommand = new RelayCommand(Magnifier);
                return _magnifierCommand;
            }
        }

        private void Magnifier(object parameter)
        {
            if (MagnifierOn == true) return;
            if (MouseClickCollection.Count == 0)
            {
                MessageBox.Show("Please select an area first!");
                return;
            }

            MagnifierWindow magnifierWindow = new MagnifierWindow();
            magnifierWindow.Show();
        }
        #endregion

        #region Visualize color levels

        #region Row color levels
        private ICommand _rowColorLevelsCommand;
        public ICommand RowColorLevelsCommand
        {
            get
            {
                if (_rowColorLevelsCommand == null)
                    _rowColorLevelsCommand = new RelayCommand(RowColorLevels);
                return _rowColorLevelsCommand;
            }
        }

        private void RowColorLevels(object parameter)
        {
            if (RowColorLevelsOn == true) return;
            if (MouseClickCollection.Count == 0)
            {
                MessageBox.Show("Please select an area first!");
                return;
            }

            ColorLevelsWindow window = new ColorLevelsWindow(_mainVM, CLevelsType.Row);
            window.Show();
        }
        #endregion

        #region Column color levels
        private ICommand _columnColorLevelsCommand;
        public ICommand ColumnColorLevelsCommand
        {
            get
            {
                if (_columnColorLevelsCommand == null)
                    _columnColorLevelsCommand = new RelayCommand(ColumnColorLevels);
                return _columnColorLevelsCommand;
            }
        }

        private void ColumnColorLevels(object parameter)
        {
            if (ColumnColorLevelsOn == true) return;
            if (MouseClickCollection.Count == 0)
            {
                MessageBox.Show("Please select an area first!");
                return;
            }

            ColorLevelsWindow window = new ColorLevelsWindow(_mainVM, CLevelsType.Column);
            window.Show();
        }
        #endregion

        #endregion

        #region Visualize image histogram

        #region Initial image histogram
        private ICommand _histogramInitialImageCommand;
        public ICommand HistogramInitialImageCommand
        {
            get
            {
                if (_histogramInitialImageCommand == null)
                    _histogramInitialImageCommand = new RelayCommand(HistogramInitialImage);
                return _histogramInitialImageCommand;
            }
        }

        private void HistogramInitialImage(object parameter)
        {
            if (InitialHistogramOn == true) return;
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            HistogramWindow window = null;

            if (ColorInitialImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.InitialColor);
            }
            else if (GrayInitialImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.InitialGray);
            }

            window.Show();
        }
        #endregion

        #region Processed image histogram
        private ICommand _histogramProcessedImageCommand;
        public ICommand HistogramProcessedImageCommand
        {
            get
            {
                if (_histogramProcessedImageCommand == null)
                    _histogramProcessedImageCommand = new RelayCommand(HistogramProcessedImage);
                return _histogramProcessedImageCommand;
            }
        }

        private void HistogramProcessedImage(object parameter)
        {
            if (ProcessedHistogramOn == true) return;
            if (ProcessedImage == null)
            {
                MessageBox.Show("Please process an image first!");
                return;
            }

            HistogramWindow window = null;

            if (ColorProcessedImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.ProcessedColor);
            }
            else if (GrayProcessedImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.ProcessedGray);
            }

            window.Show();
        }
        #endregion

        #endregion

        #region Copy image
        private ICommand _copyImageCommand;
        public ICommand CopyImageCommand
        {
            get
            {
                if (_copyImageCommand == null)
                    _copyImageCommand = new RelayCommand(CopyImage);
                return _copyImageCommand;
            }
        }

        private void CopyImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Copy(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
            else if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Copy(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
        }
        #endregion

        #region Invert image
        private ICommand _invertImageCommand;
        public ICommand InvertImageCommand
        {
            get
            {
                if (_invertImageCommand == null)
                    _invertImageCommand = new RelayCommand(InvertImage);
                return _invertImageCommand;
            }
        }

        private void InvertImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Invert(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Invert(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Convert color image to grayscale image
        private ICommand _convertImageToGrayscaleCommand;
        public ICommand ConvertImageToGrayscaleCommand
        {
            get
            {
                if (_convertImageToGrayscaleCommand == null)
                    _convertImageToGrayscaleCommand = new RelayCommand(ConvertImageToGrayscale);
                return _convertImageToGrayscaleCommand;
            }
        }

        private void ConvertImageToGrayscale(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (ColorInitialImage != null)
            {
                GrayProcessedImage = Tools.Convert(ColorInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                MessageBox.Show("It is possible to convert only color images!");
            }
        }
        #endregion

        #region Binary image
        private ICommand _binaryImageCommand;
        public ICommand BinaryImageCommand
        {
            get
            {
                if (_binaryImageCommand == null)
                    _binaryImageCommand = new RelayCommand(BinaryImage);
                return _binaryImageCommand;
            }
        }

        private void BinaryImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            List<string> labels = new List<string>() {"Thresholding: "};
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();

            List<double> values = window.GetValues();
            int T = (int)values[0];

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Binary(GrayInitialImage, T);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                MessageBox.Show("Please add a grayscale image!");
            }
        }
        #endregion

        #region Mirror image
        private ICommand _mirrorImageCommand;
        public ICommand MirrorImageCommand
        {
            get
            {
                if (_mirrorImageCommand == null)
                    _mirrorImageCommand = new RelayCommand(MirrorImage);
                return _mirrorImageCommand;
            }
        }
        private void MirrorImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }
            ClearProcessedCanvas(parameter as Canvas);
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Mirror(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Mirror(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Rotate Clockwise
        private ICommand _rotateClockwiseCommand;
        public ICommand RotateClockwiseCommand
        {
            get
            {
                if (_rotateClockwiseCommand == null)
                    _rotateClockwiseCommand = new RelayCommand(RotateClockwise);
                return _rotateClockwiseCommand;
            }
        }
        private void RotateClockwise(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }
            ClearProcessedCanvas(parameter as Canvas);
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.RotateClockwise(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.RotateClockwise(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Rotate Anti-Clockwise
        private ICommand _rotateAntiClockwiseCommand;
        public ICommand RotateAntiClockwiseCommand
        {
            get
            {
                if (_rotateAntiClockwiseCommand == null)
                    _rotateAntiClockwiseCommand = new RelayCommand(RotateAntiClockwise);
                return _rotateAntiClockwiseCommand;
            }
        }
        private void RotateAntiClockwise(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }
            ClearProcessedCanvas(parameter as Canvas);
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.RotateAntiClockwise(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.RotateAntiClockwise(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #endregion

        #region Pointwise operations

        #region Contrast and Brightness

        private ICommand _contrastAndBrightnessCommand;

        public ICommand ContrastAndBrightnessCommand
        {
            get
            {
                if (_contrastAndBrightnessCommand == null)
                    _contrastAndBrightnessCommand = new RelayCommand(ContrastAndBrightness);
                return _contrastAndBrightnessCommand;
            }
        }

        private void ContrastAndBrightness(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            List<string> labels = new List<string>() { "Value for alpha: ", "Value for beta" };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();

            List<double> values = window.GetValues();
            double alpha = values[0];
            double beta = values[1];

            if (alpha <= 0)
            {
                MessageBox.Show("Please enter a positive value for alpha.");
                return;
            }

            byte[] LUT = PointwiseOperations.ContrastBrightness(alpha, beta);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = PointwiseOperations.ApplyLUT(GrayInitialImage, LUT);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                MessageBox.Show("It is possible to convert only grayscale images!");
            }
        }
        #endregion

        #region Gamma Operator

        private ICommand _gammaOperatorCommand;

        public ICommand GammaOperatorCommand
        {
            get
            {
                if (_gammaOperatorCommand == null)
                    _gammaOperatorCommand = new RelayCommand(GammaOperator);
                return _gammaOperatorCommand;
            }
        }

        private void GammaOperator(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            var labels = new List<string> { "Contrast (a)" };
            var window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();

            var values = window.GetValues();
            var a = values[0];

            if (a <= 0)
            {
                MessageBox.Show("Gamma must be greater than 0!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            var LUT = PointwiseOperations.GammaLUT(a);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = PointwiseOperations.ApplyLUT(GrayInitialImage, LUT);
                ProcessedImage = Convert(GrayProcessedImage);
            }
        }

        #endregion

        #endregion

        #region Thresholding

        private ICommand _intermeansThresholdingCommand;
        public ICommand IntermeansThresholdingCommand
        {
            get
            {
                if (_intermeansThresholdingCommand == null)
                    _intermeansThresholdingCommand = new RelayCommand(IntermeansThresholding);
                return _intermeansThresholdingCommand;
            }
        }

        private void IntermeansThresholding(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            if (GrayInitialImage == null)
            {
                MessageBox.Show("Please use a grayscale image!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            int threshold = Thresholding.CalculIntermeans(GrayInitialImage);
            GrayProcessedImage = Thresholding.Intermeans(GrayInitialImage);
            ProcessedImage = Convert(GrayProcessedImage);

            MessageBox.Show($"Calculated threshold: {threshold}",
                            "Intermeans Thresholding",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        #endregion

        #region Filters
        private ICommand _filter1command;
        public ICommand Filter1Command
        {
            get
            {
                if (_filter1command == null)
                    _filter1command = new RelayCommand(UseFilter1);
                return _filter1command;
            }
        }

        private void UseFilter1(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            double[,] filter = new double[,]
            {
                {0.0, -1.0, 0.0 },
                {-1.0, 5.0, -1.0 },
                {0.0, -1.0, 0.0 }
            };

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Filters.ApplyFilter(GrayInitialImage, filter);
                ProcessedImage = Convert(GrayProcessedImage);
            }

        }

        #endregion

        #region Sobel Diagonal Filter
        private ICommand _sobelDiagonalCommand;
        public ICommand SobelDiagonalCommand
        {
            get
            {
                if (_sobelDiagonalCommand == null)
                    _sobelDiagonalCommand = new RelayCommand(SobelDiagonalFilter);
                return _sobelDiagonalCommand;
            }
        }

        private void SobelDiagonalFilter(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }

            if (GrayInitialImage == null && ColorInitialImage != null)
            {
                GrayInitialImage = Tools.Convert(ColorInitialImage);
            }
            else if (GrayInitialImage == null && ColorInitialImage == null)
            {
                MessageBox.Show("Please load a grayscale image first!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            List<string> labels = new List<string>() { "Threshold (T): ", "Deviation (dev - grade): " };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();

            List<double> values = window.GetValues();

            if (values.Count < 2)
            {
                MessageBox.Show("Please enter values for Threshold and Deviation.");
                return;
            }
            int T = (int)values[0];
            double deviation = values[1];

            GrayProcessedImage = Filters.SobelDiagonal(GrayInitialImage, T, deviation);
            ProcessedImage = Convert(GrayProcessedImage);
        }
        #endregion

        #region Morphological Operations
        private List<double> ShowMorphologyDialog()
        {
            List<string> labels = new List<string>()
            {
                "Inaltime Element Structurant (h): ",
                "Latime Element Structurant (w): ",
                "Prag Binarizare (T): ",
                "Optiune (1=Obiecte Albe, 0=Obiecte Negre): "
            };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();
            return window.GetValues();
        }

        private List<double> ShowGradientDialog()
        {
            List<string> labels = new List<string>()
            {
                "Inaltime Element Structurant (h): ",
                "Latime Element Structurant (w): "
            };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();
            return window.GetValues();
        }

        private ICommand _dilationCommand;
        public ICommand DilationCommand
        {
            get { if (_dilationCommand == null) _dilationCommand = new RelayCommand(DilationFilter); return _dilationCommand; }
        }

        private void DilationFilter(object parameter)
        {
            if (GrayInitialImage == null) { MessageBox.Show("Please load a grayscale image first!"); return; }

            List<double> values = ShowMorphologyDialog();
            if (values == null || values.Count < 4) return;

            int h = (int)values[0];
            int w = (int)values[1];
            int T = (int)values[2];
            int optiune = (int)values[3];

            ClearProcessedCanvas(parameter as Canvas);
            GrayProcessedImage = Morphology.Dilation(GrayInitialImage, h, w, T, optiune);
            ProcessedImage = Convert(GrayProcessedImage);
        }

        private ICommand _erosionCommand;
        public ICommand ErosionCommand
        {
            get { if (_erosionCommand == null) _erosionCommand = new RelayCommand(ErosionFilter); return _erosionCommand; }
        }

        private void ErosionFilter(object parameter)
        {
            if (GrayInitialImage == null) { MessageBox.Show("Please load a grayscale image first!"); return; }

            List<double> values = ShowMorphologyDialog();
            if (values == null || values.Count < 4) return;

            int h = (int)values[0];
            int w = (int)values[1];
            int T = (int)values[2];
            int optiune = (int)values[3];

            ClearProcessedCanvas(parameter as Canvas);
            GrayProcessedImage = Morphology.Erosion(GrayInitialImage, h, w, T, optiune);
            ProcessedImage = Convert(GrayProcessedImage);
        }

        private ICommand _openingCommand;
        public ICommand OpeningCommand
        {
            get { if (_openingCommand == null) _openingCommand = new RelayCommand(OpeningFilter); return _openingCommand; }
        }

        private void OpeningFilter(object parameter)
        {
            if (GrayInitialImage == null) { MessageBox.Show("Please load a grayscale image first!"); return; }

            List<double> values = ShowMorphologyDialog();
            if (values == null || values.Count < 4) return;

            int h = (int)values[0];
            int w = (int)values[1];
            int T = (int)values[2];
            int optiune = (int)values[3];

            ClearProcessedCanvas(parameter as Canvas);
            GrayProcessedImage = Morphology.Opening(GrayInitialImage, h, w, T, optiune);
            ProcessedImage = Convert(GrayProcessedImage);
        }

        private ICommand _closingCommand;
        public ICommand ClosingCommand
        {
            get { if (_closingCommand == null) _closingCommand = new RelayCommand(ClosingFilter); return _closingCommand; }
        }

        private void ClosingFilter(object parameter)
        {
            if (GrayInitialImage == null) { MessageBox.Show("Please load a grayscale image first!"); return; }

            List<double> values = ShowMorphologyDialog();
            if (values == null || values.Count < 4) return;

            int h = (int)values[0];
            int w = (int)values[1];
            int T = (int)values[2];
            int optiune = (int)values[3];

            ClearProcessedCanvas(parameter as Canvas);
            GrayProcessedImage = Morphology.Closing(GrayInitialImage, h, w, T, optiune);
            ProcessedImage = Convert(GrayProcessedImage);
        }

        private ICommand _morphologicalGradientCommand;

        public ICommand MorphologicalGradientCommand
        {
            get { if (_morphologicalGradientCommand == null) _morphologicalGradientCommand = new RelayCommand(MorphologicalGradientFilter); return _morphologicalGradientCommand; }
        }

        private void MorphologicalGradientFilter(object parameter)
        {
            if (GrayInitialImage == null) { MessageBox.Show("Please load a grayscale image first!"); return; }

            List<double> values = ShowGradientDialog();
            if (values == null || values.Count < 2) return;

            int h = (int)values[0];
            int w = (int)values[1];

            ClearProcessedCanvas(parameter as Canvas);

            GrayProcessedImage = Morphology.MorphologicalGradient(GrayInitialImage, h, w);
            ProcessedImage = Convert(GrayProcessedImage);
        }
        #endregion


        #region Geometric transforms
        private List<double> ShowScaleDialog()
        {
            List<string> labels = new List<string>() { "Factor Scalare X (sx): ", "Factor Scalare Y (sy): " };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();
            return window.GetValues();
        }

        private ICommand _scaleBilinearCommand;
        public ICommand ScaleBilinearCommand
        {
            get { if (_scaleBilinearCommand == null) _scaleBilinearCommand = new RelayCommand(ScaleBilinearFilter); return _scaleBilinearCommand; }
        }

        private void ScaleBilinearFilter(object parameter)
        {
            if (GrayInitialImage == null) { MessageBox.Show("Please load a grayscale image first!"); return; }

            List<double> values = ShowScaleDialog();
            if (values.Count < 2 || values[0] <= 0 || values[1] <= 0)
            {
                MessageBox.Show("Please enter valid positive scaling factors (sx, sy).");
                return;
            }
            double sx = values[0];
            double sy = values[1];

            ClearProcessedCanvas(parameter as Canvas);
            try
            {
                GrayProcessedImage = GeometricTransforms.Scale(GrayInitialImage, sx, sy, useBicubic: false);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la scalare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ICommand _scaleBicubicCommand;
        public ICommand ScaleBicubicCommand
        {
            get { if (_scaleBicubicCommand == null) _scaleBicubicCommand = new RelayCommand(ScaleBicubicFilter); return _scaleBicubicCommand; }
        }

        private void ScaleBicubicFilter(object parameter)
        {
            if (GrayInitialImage == null) { MessageBox.Show("Please load a grayscale image first!"); return; }

            List<double> values = ShowScaleDialog();
            if (values.Count < 2 || values[0] <= 0 || values[1] <= 0)
            {
                MessageBox.Show("Please enter valid positive scaling factors (sx, sy).");
                return;
            }
            double sx = values[0];
            double sy = values[1];

            ClearProcessedCanvas(parameter as Canvas);
            try
            {
                GrayProcessedImage = GeometricTransforms.Scale(GrayInitialImage, sx, sy, useBicubic: true);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la scalare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Segmentation
        #endregion

        #region Gauss Separated Filter
        private ICommand _gaussSeparatedFilterCommand;
        public ICommand GaussSeparatedFilterCommand
        {
            get
            {
                if (_gaussSeparatedFilterCommand == null)
                    _gaussSeparatedFilterCommand = new RelayCommand(GaussSeparatedFilter);
                return _gaussSeparatedFilterCommand;
            }
        }

        private void GaussSeparatedFilter(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please load an image first!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            List<string> labels = new List<string>() { "Sigma X (σx): ", "Sigma Y (σy): " };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();

            List<double> values = window.GetValues();

            if (values.Count < 2 || values[0] <= 0 || values[1] <= 0)
            {
                MessageBox.Show("Please enter positive values for Sigma X and Sigma Y.");
                return;
            }
            double sigmaX = values[0];
            double sigmaY = values[1];

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Filters.GaussFilteringSeparated(GrayInitialImage, sigmaX, sigmaY);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Filters.GaussColorFilteringSeparated(ColorInitialImage, sigmaX, sigmaY);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Use processed image as initial image
        private ICommand _useProcessedImageAsInitialImageCommand;
        public ICommand UseProcessedImageAsInitialImageCommand
        {
            get
            {
                if (_useProcessedImageAsInitialImageCommand == null)
                    _useProcessedImageAsInitialImageCommand = new RelayCommand(UseProcessedImageAsInitialImage);
                return _useProcessedImageAsInitialImageCommand;
            }
        }

        private void UseProcessedImageAsInitialImage(object parameter)
        {
            if (ProcessedImage == null)
            {
                MessageBox.Show("Please process an image first!");
                return;
            }

            var canvases = (object[])parameter;

            ClearInitialCanvas(canvases[0] as Canvas);

            if (GrayProcessedImage != null)
            {
                GrayInitialImage = GrayProcessedImage;
                InitialImage = Convert(GrayInitialImage);
            }
            else if (ColorProcessedImage != null)
            {
                ColorInitialImage = ColorProcessedImage;
                InitialImage = Convert(ColorInitialImage);
            }

            ClearProcessedCanvas(canvases[1] as Canvas);
        }
        #endregion
    }
}