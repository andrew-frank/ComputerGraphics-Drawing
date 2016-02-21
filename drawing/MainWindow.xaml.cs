using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace drawing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region public 

        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region handles

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Down");
            this.firstPoint = e.GetPosition(this);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Up");
            this.secondPoint = e.GetPosition(this);

            if (this.scaleCheckBox.IsChecked == true) {
                this.drawLineScaled();
                return;
            }

            if (this.circleRadioBtn.IsChecked == true) {
                this.drawCircle();
            } else { //line
                this.drawLine();
            }
        }

        #endregion


        //////////////////////////////////////////////////////////////////////////////


        #region Private

        private int scale = 2;
        private Canvas scaledCanvas;

        private Point firstPoint = new Point();
        private Point secondPoint = new Point();
        private int _thickness = 1;

        public int thickness {
            get {
                int t = this.parseThickness();
                if (t <= 0) {
                    this.thicknessTextBox.Text = "1";
                    t = 1;
                } else if(t % 2 == 0) {
                    t += 1;
                    this.thicknessTextBox.Text = String.Format("{0}", t);
                }
                _thickness = t;
                return _thickness;
            }
            set { _thickness = value; }
        }

        private int parseThickness()
        {
            int t = -1;
            Int32.TryParse(this.thicknessTextBox.Text, out t);
            return t;
        }


        private void drawCircle()
        {
            //Debug.WriteLine("Draw Circle. \nFirst point = {0} \nSecond point = {1}", this.firstPoint, this.secondPoint);
            //Debug.WriteLine("Thickness = {0}", this.thickness);
            int R = (int)distanceBetweenPoints(this.firstPoint, this.secondPoint);
            //Debug.WriteLine("Radius = {0}", R);
            this.midpointCircle((int)this.firstPoint.X, (int)this.firstPoint.Y, R, this.thickness, this.canvas);
        }


        private void drawLine()
        {
            //Debug.WriteLine("Draw line. \nFirst point = {0} \nSecond point = {1}", this.firstPoint, this.secondPoint);
            //Debug.WriteLine("Thickness = {0}", this.thickness);
            this.bresenhamsLine((int)this.firstPoint.X, (int)this.firstPoint.Y, (int)this.secondPoint.X, (int)this.secondPoint.Y, 
                this.thickness, this.canvas);
        }


        private void drawLineScaled()
        {
            this.scaledCanvasArr = new int[(int)this.canvas.ActualWidth, (int)this.canvas.ActualHeight];

            this.scaledCanvas = new Canvas();
            this.scaledCanvas.Width = this.canvas.Width * this.scale;
            this.scaledCanvas.Height = this.canvas.Height * this.scale;

            Point scaledFirst = new Point(this.firstPoint.X * this.scale, this.firstPoint.Y * this.scale);
            Point scaledSecond = new Point(this.secondPoint.X * this.scale, this.secondPoint.Y * this.scale);

            this.bresenhamsLine((int)scaledFirst.X, (int)scaledFirst.Y, (int)scaledSecond.X, (int)scaledSecond.Y, 
                this.thickness*this.scale+1, this.scaledCanvas);

            this.copyScaledPixelsToCanvas();
        }

        private void copyScaledPixelsToCanvas()
        {
            for (int i = 0; i < this.scaledCanvas.ActualWidth; i+=2 ) {
                for (int j = 0; j < this.scaledCanvas.ActualHeight; j+=2 ) {
                    int counter = this.scaledCanvasArr[i, j] + this.scaledCanvasArr[i + 1, j] 
                        + this.scaledCanvasArr[i, j + 1] + this.scaledCanvasArr[i + 1, j + 1];

                }
            }
        }

        #endregion


        //////////////////////////////////////////////////////////////////////////////


        #region drawing

        private int[,] scaledCanvasArr;


        private void drawPixel(Point point, Brush stroke, Canvas canvas)
        {
            Rectangle rect = new Rectangle();
            rect.Width = 1;
            rect.Stroke = stroke;

            Canvas.SetTop(rect, point.Y);
            Canvas.SetLeft(rect, point.X);
            this.canvas.Children.Add(rect);
            if (canvas == this.scaledCanvas) {
                this.scaledCanvasArr[(int)point.X, (int)point.Y] = 1;
            }
        }


        private void putPixel(int x, int y, Canvas canvas)
        {
            Point point = new Point(x, y);
            drawPixel(point, Brushes.Black, canvas);
        }

        #endregion


        //////////////////////////////////////////////////////////////////////////////


        #region algorithms

        public void bresenhamsLine(int x0, int y0, int x1, int y1, int width, Canvas canvas)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) {
                swapInts(ref x0, ref y0);
                swapInts(ref x1, ref y1);
            }

            if (x0 > x1) {
                swapInts(ref x0, ref x1);
                swapInts(ref y0, ref y1);
            }

            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;

            for (int x = x0; x <= x1; x++) {
                int start = y - (width / 2);
                int end = y + (width / 2);
                if (steep) {
                    for (int i = start ; i <= end ; i++)
                        this.drawPixel(new Point(i, x), Brushes.Black, canvas);
                } else {
                    for (int i = start; i <= end; i++)
                        this.drawPixel(new Point(x, i), Brushes.Black, canvas);
                }

                error = error - dy;
                if (error < 0) {
                    y += ystep;
                    error += dx;
                }
            }
        }


        /*
         if(tempDebug == true) {
              Debug.WriteLine("steep");
              tempDebug = false;
         }
        */

        /*
        void midpointLine(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int d = 2 * dy - dx; // initial value of d
            int dE = 2 * dy; // increment used when moving to E
            int dNE = 2 * (dy - dx); // increment used when movint to NE
            int x = x1, y = y1;
            putPixel(x, y);
            while (x < x2) {
                if (d < 0) { // move to E
                    d += dE;
                    x++;
                } else { // move to NE 
                    d += dNE;
                    ++x;
                    ++y;
                }

                putPixel(x, y);
            }
        }
        */

        public void midpointCircle(int x0, int y0, int radius, int width, Canvas canvas)
        {
            int x = radius;
            int y = 0;
            int radiusError = 1 - x;

            while (x >= y) {
                int start = y - (width / 2);
                int end = y + (width / 2);
                for (int i = start; i <= end; i++) {
                    putPixel(x + x0, i + y0, canvas);
                    putPixel(i + x0, x + y0, canvas);
                    putPixel(-x + x0, i + y0, canvas);
                    putPixel(-i + x0, x + y0, canvas);
                    putPixel(-x + x0, -i + y0, canvas);
                    putPixel(-i + x0, -x + y0, canvas);
                    putPixel(x + x0, -i + y0, canvas);
                    putPixel(i + x0, -x + y0, canvas);
                }

                y++;
                if (radiusError < 0) {
                    radiusError += 2 * y + 1;
                } else {
                    x--;
                    radiusError += 2 * (y - x) + 1;
                }
            }
        }

        /*
        void MidpointCircle(int x, int y, int R)
        {
            int dE = 3;
            int dSE = 5 - 2 * R;
            int d = 1 - R;
            y += R;
            putPixel(x, y);
            while (y > x) {
                if (d < 0) { //move to E
                    d += dE;
                    dE += 2;
                    dSE += 2;
                } else { //move to SE
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;
                putPixel(x, y);
            }
        }
        */

        #endregion


        //////////////////////////////////////////////////////////////////////////////

        #region utils
        public double distanceBetweenPoints(Point p, Point q)
        {
            double a = p.X - q.X;
            double b = p.Y - q.Y;
            double distance = Math.Sqrt(a * a + b * b);
            return distance;
        }

        private void swapInts(ref int x, ref int y)
        {
            int tempswap = x;
            x = y;
            y = tempswap;
        }

        #endregion

    }
}
