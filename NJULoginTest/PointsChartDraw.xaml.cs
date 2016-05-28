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
        public void tableRefresh()
        {
            if (pointsource == null)
                return;
            else if (pointsource.Count == 0)
                return;

            PointRange[0] = pointsource.Min(u => u.X);
            PointRange[1] = pointsource.Max(u => u.X);
            if (PointRange[1] - PointRange[0] == 0) PointRange[1] = PointRange[0] + double.PositiveInfinity;
            PointRange[2] = pointsource.Min(u => u.Y);
            PointRange[3] = pointsource.Max(u => u.Y);
            if (PointRange[3] - PointRange[2] == 0) PointRange[3] = PointRange[2] + double.PositiveInfinity;

            int YD = (int)(RenderSize.Height / HeightStep) + 1;
            int XD = (int)(RenderSize.Width / WidthStep) + 1;

            rootgrid.Children.Clear();

            var line = new Polyline();
            Grid.SetColumnSpan(line, XD);
            Grid.SetRowSpan(line, YD);
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Bottom;
            //line.Stroke = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
            line.Stroke = new SolidColorBrush(Colors.Aqua));
            line.StrokeThickness = 3;
            double YP = RenderSize.Height / (PointRange[3] - PointRange[2]);
            double XP = RenderSize.Width / (PointRange[1] - PointRange[0]);
            pointdraw = new PointCollection();
            foreach (var p in pointsource)
            {
                pointdraw.Add(new Point() { X = XP * (p.X - PointRange[0]), Y = YP * (p.Y - PointRange[2]) });
            }
            line.Points = pointdraw;
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
                var RowDefYLabel = new RowDefinition();
                RowDefYLabel.Height = new GridLength(1, GridUnitType.Star);
                YLabelGrid.RowDefinitions.Add(RowDefYLabel);
                var YLabelText = new TextBlock();
                var TextValue = (PointRange[2] + YP * HeightStep * i);
                if (TextValue > PointRange[3] + YP * HeightStep)
                    YLabelText.Text = "";
                else
                    YLabelText.Text = TextValue.ToString("#0.0");
                YLabelText.Foreground = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
                Grid.SetRow(YLabelText, YD - 1 - i);
                YLabelGrid.Children.Add(YLabelText);
                if (i % 2 != 0)
                {
                    var rect = new Rectangle();
                    Grid.SetRow(rect, i);
                    Grid.SetColumnSpan(rect, XD);
                    rect.Fill = Application.Current.Resources["SystemControlHighlightAltListAccentLowBrush"] as SolidColorBrush;
                    rootgrid.Children.Add(rect);
                }
            }

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
                var TextValue = (PointRange[0] + XP * WidthStep * i);
                if (TextValue > PointRange[1] + XP * WidthStep)
                    XLabelText.Text = "";
                else
                    XLabelText.Text = TextValue.ToString("#0.0");
                XLabelText.Foreground = Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
                Grid.SetColumn(XLabelText, i);
                XLabelGrid.Children.Add(XLabelText);

                var bord = new Border();
                Grid.SetColumn(bord, i);
                Grid.SetRowSpan(bord, YD);
                bord.BorderThickness = new Thickness(1,0,1,0);
                bord.BorderBrush = Application.Current.Resources["SystemControlHighlightAltListAccentHighBrush"] as SolidColorBrush;
                rootgrid.Children.Add(bord);

            }
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
    }
}
