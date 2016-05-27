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
            PointRange[0] = pointsource.Min(u => u.X);
            PointRange[1] = pointsource.Max(u => u.X);
            PointRange[2] = pointsource.Min(u => u.Y);
            PointRange[3] = pointsource.Max(u => u.Y);

            int YD = (int)(RenderSize.Height / HeightStep) + 1;
            int XD = (int)(RenderSize.Width / WidthStep) + 1;

            rootgrid.Children.Clear();

            rootgrid.RowDefinitions.Clear();
            for (int i = 0; i < YD; ++i)
            {
                var RowDef = new RowDefinition();
                RowDef.Height = new GridLength(0, GridUnitType.Star);
                rootgrid.RowDefinitions.Add(RowDef);
                if (i % 2 != 0)
                {
                    var rect = new Rectangle();
                    Grid.SetRow(rect, i);
                    rect.Fill = Application.Current.Resources["SystemControlHighlightAltListAccentLowBrush"] as SolidColorBrush;
                    rootgrid.Children.Add(rect);
                }
            }

            rootgrid.ColumnDefinitions.Clear();
            for (int i = 0; i < XD; ++i)
            {
                var ColDef = new ColumnDefinition();
                ColDef.Width = new GridLength(0, GridUnitType.Star);
                rootgrid.ColumnDefinitions.Add(ColDef);
            }

            var line = new Polyline();Grid.SetColumnSpan(line, XD); Grid.SetRowSpan(line, YD);
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.Stroke = new SolidColorBrush(Colors.Blue);
            line.StrokeThickness = 3;
            double YP = RenderSize.Height / (PointRange[3] - PointRange[2]);
            double XP = RenderSize.Width / (PointRange[1] - PointRange[0]);
            pointdraw = new PointCollection();
            foreach(var p in pointsource)
            {
                pointdraw.Add(new Point() { X = XP * (p.X - PointRange[0]), Y = YP * (p.Y - PointRange[2]) });
            }
            line.Points = pointdraw;
            rootgrid.Children.Add(line);
        }
    }
}
