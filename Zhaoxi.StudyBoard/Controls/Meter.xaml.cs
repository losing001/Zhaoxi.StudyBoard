using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zhaoxi.StudyBoard.Controls
{
    /// <summary>
    /// Meter.xaml 的交互逻辑
    /// </summary>
    public partial class Meter : UserControl
    {
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), 
                typeof(Meter), new PropertyMetadata(0.0));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), 
                typeof(Meter), new PropertyMetadata(100.0));

        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(Meter), new PropertyMetadata(10));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), 
                typeof(Meter), new PropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged)));
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Meter).UpdatePointer();
        }
        private void UpdatePointer()
        {
            double step = 270 / (this.Maximum - this.Minimum);
            double angle = (this.Value - this.Minimum) * step + 135;

            DoubleAnimation da = new DoubleAnimation(angle, new TimeSpan(0, 0, 0, 0, 200));
            //this.rtPointer.Angle = angle;
            this.rtPointer.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(Meter), new PropertyMetadata(""));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(Meter), new PropertyMetadata(""));


        public Meter()
        {
            InitializeComponent();
            this.SizeChanged += Meter_SizeChanged;
        }

        private void Meter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.back_border.Width = Math.Min(this.RenderSize.Width, this.RenderSize.Height);
            this.back_border.Height = Math.Min(this.RenderSize.Width, this.RenderSize.Height);
            this.back_border.CornerRadius = new CornerRadius(this.back_border.Width / 2);

            //半径
            double radius = this.back_border.Width / 2;
            if (radius <= 0) return;

            this.canvasPlate.Children.Clear();
            string borderPathData = $"M4,{radius}A{radius - 4} {radius - 4} 0 1 1 {radius} {this.back_border.Height - 4}";
            this.pathBorder.Data = PathGeometry.Parse(borderPathData);


            double label = this.Minimum;
            double step = 270 / (this.Maximum - this.Minimum);
            for (int i = 0; i <= (this.Maximum - this.Minimum); i++)  
            {
                int offset = 8;
                if (i % 10 == 0)
                {
                    offset = 12;

                    //加文本块
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = label.ToString();
                    textBlock.Width = 34;
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.Foreground = Brushes.White;
                    textBlock.FontSize = 10;
                    Canvas.SetLeft(textBlock, radius + (radius - 30) * Math.Cos((step * i + 135) * Math.PI / 180) - 17);
                    Canvas.SetTop(textBlock, radius + (radius - 30) * Math.Sin((step * i + 135) * Math.PI / 180) - 8);
                    this.canvasPlate.Children.Add(textBlock);

                    label += 10;
                }
                else if (i % 5 == 0) offset = 10;

                Line line = new Line();
                line.X1 = radius + (radius - 5) * Math.Cos((step * i + 135) * Math.PI / 180);
                line.Y1 = radius + (radius - 5) * Math.Sin((step * i + 135) * Math.PI / 180);
                line.X2 = radius + (radius - offset) * Math.Cos((step * i + 135) * Math.PI / 180);
                line.Y2 = radius + (radius - offset) * Math.Sin((step * i + 135) * Math.PI / 180);

                line.Stroke = Brushes.White;
                line.StrokeThickness = 1;

                this.canvasPlate.Children.Add(line);
            }

            string pointerData = $"M{radius} {radius + 2} {this.back_border.Width * 0.95} {radius} {radius} {radius - 2}";
            this.pointer.Data = PathGeometry.Parse(pointerData);
        }
    }
}
