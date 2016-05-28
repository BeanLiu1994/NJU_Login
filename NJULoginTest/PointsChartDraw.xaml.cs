using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NJULoginTest
{
    public sealed partial class PointsChartDraw : UserControl
    {
        //可设置
        public Size XYCount = new Size(0, 0);
        public Func<double, string> XStringFormatFunc = (u) => { return u.ToString("#0"); };




        private PointCollection pointdraw;
        private PointCollection pointsource;
        public PointCollection PointsSource { get { return pointsource; } set { pointsource = value; tableRefresh(); } }
        public PointsChartDraw()
        {
            this.InitializeComponent();
        }
        public double[] PointRange = new double[4];
        private const double HeightStep = 50;
        private const double WidthStep = 80;

        private void tableRefresh()
        {
            if (pointsource == null)
                return;
            else if (pointsource.Count == 0)
                return;

            PointRange[0] = pointsource.Min(u => u.X);
            PointRange[1] = pointsource.Max(u => u.X);
            if (PointRange[1] - PointRange[0] == 0) PointRange[1] = PointRange[0] + 1;
            PointRange[2] = pointsource.Min(u => u.Y);
            PointRange[3] = pointsource.Max(u => u.Y);
            if (PointRange[3] - PointRange[2] == 0) PointRange[3] = PointRange[2] + 1;

            double WidthStep_Concrete = WidthStep;
            double HeightStep_Concrete = HeightStep;
            int XD = (int)(RenderSize.Width / WidthStep) + 1;
            int YD = (int)(RenderSize.Height / HeightStep) + 1;

            if (XYCount.Width != 0)
            {
                XD = (int)XYCount.Width;
                WidthStep_Concrete = RenderSize.Width / XD;
            }
            if (XYCount.Height != 0)
            {
                YD = (int)XYCount.Height;
                HeightStep_Concrete = RenderSize.Height / YD;
            }

            rootgrid.Children.Clear();

            var line = new Polyline();
            Grid.SetColumnSpan(line, XD);
            Grid.SetRowSpan(line, YD);
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Bottom;
            //line.Stroke = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
            line.Stroke = new SolidColorBrush(Colors.Orchid);
            line.StrokeThickness = 3;
            double YP = RenderSize.Height / (PointRange[3] - PointRange[2]);
            double XP = RenderSize.Width / (PointRange[1] - PointRange[0]);
            pointdraw = new PointCollection();
            foreach (var p in pointsource)
            {
                pointdraw.Add(new Point() { X = XP * (p.X - PointRange[0]), Y = YP * (p.Y - PointRange[2]) });
            }
            line.Points = pointdraw;
            line.StrokeLineJoin = PenLineJoin.Bevel;
            line.Stretch = Stretch.Fill;
            rootgrid.Children.Add(line);

            rootgrid.RowDefinitions.Clear();
            YLabelGrid.RowDefinitions.Clear();
            YLabelGrid.Children.Clear();
            for (int i = 0; i < YD; ++i)
            {
                var RowDef = new RowDefinition();
                RowDef.Height = new GridLength(1, GridUnitType.Star);
                rootgrid.RowDefinitions.Add(RowDef);
                if (i % 2 != 0)
                {
                    var rect = new Rectangle();
                    Grid.SetRow(rect, i);
                    Grid.SetColumnSpan(rect, XD);
                    rect.Fill = Application.Current.Resources["SystemControlHighlightAltListAccentLowBrush"] as SolidColorBrush;
                    rootgrid.Children.Add(rect);
                }

                var RowDefYLabel = new RowDefinition();
                RowDefYLabel.Height = new GridLength(1, GridUnitType.Star);
                YLabelGrid.RowDefinitions.Add(RowDefYLabel);
                var YLabelText = new TextBlock();
                YLabelText.VerticalAlignment = VerticalAlignment.Bottom;
                var TextValue = (PointRange[2] + HeightStep_Concrete * i / YP);
                if (TextValue > PointRange[3] + HeightStep_Concrete / YP)
                    YLabelText.Text = "";
                else
                    YLabelText.Text = TextValue.ToString("#0.00");
                YLabelText.Foreground = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
                Grid.SetRow(YLabelText, YD - 1 - i);
                YLabelGrid.Children.Add(YLabelText);
            }
            YMax.Text = (PointRange[2] + HeightStep_Concrete * YD / YP).ToString("#0.00");
            YMax.Foreground = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;

            rootgrid.ColumnDefinitions.Clear();
            XLabelGrid.ColumnDefinitions.Clear();
            XLabelGrid.Children.Clear();
            for (int i = 0; i < XD; ++i)
            {
                var ColDef = new ColumnDefinition();
                ColDef.Width = new GridLength(1, GridUnitType.Star);
                rootgrid.ColumnDefinitions.Add(ColDef);
                var ColDefYLabel = new ColumnDefinition();
                ColDefYLabel.Width = new GridLength(1, GridUnitType.Star);
                XLabelGrid.ColumnDefinitions.Add(ColDefYLabel);
                var XLabelText = new TextBlock();
                XLabelText.HorizontalAlignment = HorizontalAlignment.Left;
                var TextValue = (PointRange[0] + WidthStep_Concrete * i / XP);
                if (TextValue > PointRange[1] + WidthStep_Concrete / XP)
                    XLabelText.Text = "";
                else
                    XLabelText.Text = XStringFormatFunc(TextValue);
                XLabelText.Foreground = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
                Grid.SetColumn(XLabelText, i);
                XLabelGrid.Children.Add(XLabelText);

                var bord = new Border();
                Grid.SetColumn(bord, i);
                Grid.SetRowSpan(bord, YD);
                bord.BorderThickness = new Thickness(1,1,1,1);
                bord.BorderBrush = Application.Current.Resources["SystemControlHighlightAltListAccentHighBrush"] as SolidColorBrush;
                rootgrid.Children.Add(bord);
            }
            XMax.Text = XStringFormatFunc(PointRange[0] + WidthStep_Concrete * XD / XP);
            XMax.Foreground = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
        }
        private Size PreviousSize = new Size(0, 0);

        private void SizeChanged_handler(object sender, SizeChangedEventArgs e)
        {
            if (Math.Abs(PreviousSize.Width - e.NewSize.Width) > WidthStep)
            {
                PreviousSize.Width = e.NewSize.Width;
                tableRefresh();
            }
            if (Math.Abs(PreviousSize.Height - e.NewSize.Height) > HeightStep)
            {
                PreviousSize.Height = e.NewSize.Height;
                tableRefresh();
            }
        }
        public void PageRefresh()
        {
            tableRefresh();
        }
    }
}
