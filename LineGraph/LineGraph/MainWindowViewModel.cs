using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPFControls;

namespace LineGraph
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private double _ControlCenter = 1865;
        public double ControlCenter { get { return _ControlCenter; } set { _ControlCenter = value; OnPropertyChanged("ControlCenter"); XAxisCalc(); } }

        private double _ControlSpan = 20;
        public double ControlSpan { get { return _ControlSpan; } set { _ControlSpan = value; OnPropertyChanged("ControlSpan"); XAxisCalc(); } }

        private double _DisplayScale = 10;
        public double DisplayScale { get { return _DisplayScale; } set { _DisplayScale = value; OnPropertyChanged("DisplayScale"); YAxisCalc(); } }

        private double _DisplayReferance = 0;
        public double DisplayReferance { get { return _DisplayReferance; } set { _DisplayReferance = value; OnPropertyChanged("DisplayReferance"); YAxisCalc(); } }

        public double DisplayAverage { get; set; }

        private double _YAxisMin;
        public double YAxisMin { get { return _YAxisMin; } set { _YAxisMin = value; OnPropertyChanged("YAxisMin"); } }

        private double _XAxisMin;
        public double XAxisMin { get { return _XAxisMin; } set { _XAxisMin = value; OnPropertyChanged("XAxisMin"); } }

        private double _XAxisMax;
        public double XAxisMax { get { return _XAxisMax; } set { _XAxisMax = value; OnPropertyChanged("XAxisMax"); } }

        private double _XAxisInterval;
        public double XAxisInterval { get { return _XAxisInterval; } set { _XAxisInterval = value; OnPropertyChanged("XAxisInterval"); } }

        private bool _IsMarkerOn;
        public bool IsMarkerOn
        {
            get { return _IsMarkerOn; }
            set
            {
                _IsMarkerOn = value;
                OnPropertyChanged("IsMarkerOn");
            }
        }

        private bool _IsMarkerDelta;
        public bool IsMarkerDelta
        {
            get { return _IsMarkerDelta; }
            set
            {
                _IsMarkerDelta = value;
                OnPropertyChanged("IsMarkerDelta");
            }
        }
        

        private int _Marker = 1;
        public int Marker
        {
            get { return _Marker; }
            set
            {
                _Marker = value;
                OnPropertyChanged("Marker");
                IsMarkerOn = false;
                IsMarkerDelta = false;
                stpMarker = null;
                foreach (MarkerItem _item in MarkerItems)
                {
                    if (_item.MarkerNum != this.Marker) continue;
                    IsMarkerOn = true;
                    IsMarkerDelta = _item.IsDelta;
                    break;
                }
            }
        }

        private bool _IsSnapShotHold;
        public bool IsSnapShotHold
        {
            get { return _IsSnapShotHold; }
            set
            {
                _IsSnapShotHold = value;
                OnPropertyChanged("IsSnapShotHold");
                if (value == false) SnapShotValueList.Clear();
                else
                {
                    SnapShotValueList.Add(new KeyValuePair<double, double>(1855, -50.5));
                    SnapShotValueList.Add(new KeyValuePair<double, double>(1865, -40.33));
                    SnapShotValueList.Add(new KeyValuePair<double, double>(1875, -50.33));
                }                
            }
        }

        private double _PowerCenter { get; set; }
        public double PowerCenter
        {
            get { return _PowerCenter; }
            set
            {
                _PowerCenter = value;
                OnPropertyChanged("PowerCenter");
            }
        }

        public double _PowerBandwidth;
        public double PowerBandwidth
        {
            get { return _PowerBandwidth; }
            set
            {
                _PowerBandwidth = value;
                OnPropertyChanged("PowerBandwidth");
            }
        }

        public double _PowerLeft;
        public double PowerLeft
        {
            get { return _PowerLeft; }
            set
            {
                _PowerLeft = value;
                OnPropertyChanged("PowerLeft");
            }
        }

        public double _PowerWidth;
        public double PowerWidth
        {
            get { return _PowerWidth; }
            set
            {
                _PowerWidth = value;
                OnPropertyChanged("PowerWidth");
            }
        }


        public ICommand PowerCommand { get; set; }
        public ICommand MarkerOnCommand { get; set; }
        public ICommand MarkerNormalCommand { get; set; }
        public ICommand MarkerPeakCommand { get; set; }

        private StackPanel stpMarker;
        private Canvas lineCanvas;
        private System.Windows.Controls.DataVisualization.Charting.Chart chart;
        private LinearAxis XLinearAxis;
        public ObservableCollection<KeyValuePair<double, double>> valueList { get; set; } = new ObservableCollection<KeyValuePair<double, double>>();

        public ObservableCollection<KeyValuePair<double, double>> SnapShotValueList { get; set; } = new ObservableCollection<KeyValuePair<double, double>>();
        
                
        public ObservableCollection<MarkerItem> MarkerItems { get; set; } = new ObservableCollection<MarkerItem>();        

        Random random = new Random();

        public MainWindowViewModel()
        {
            this.PowerCommand = new DelegateCommand(this.OnPowerCommand);
            this.MarkerOnCommand = new DelegateCommand(this.OnMarkerOnCommand);
            this.MarkerNormalCommand = new DelegateCommand(this.OnMarkerNormalCommand);
            this.MarkerPeakCommand = new DelegateCommand(this.OnMarkerPeakCommand);

            
            YAxisCalc();
        }
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var view = sender as MainWindow;
            this.chart = view.myChart;
            this.XLinearAxis = view.XLinearAxis;
            XAxisCalc();

            valueList.Add(new KeyValuePair<double, double>(1855, -54.5));
            valueList.Add(new KeyValuePair<double, double>(1865, -44.33));
            valueList.Add(new KeyValuePair<double, double>(1875, -40.33));

            PowerLeft = 0;
            PowerWidth = 0;
        }

        public void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.lineCanvas = sender as Canvas;
        }

        /// <summary>
        /// Chart line 움직일 경우 Marke 따라 움직이기
        /// </summary>
        private void MarkerUpdateForChartLineUpdate()
        {
            if (lineCanvas != null)
                this.lineCanvas.Dispatcher.Invoke((ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
            foreach (var item in MarkerItems)
            {
                if (item.MarkerControl != null)
                    MoveMarker(item.MarkerControl, new Point(Canvas.GetLeft(item.MarkerControl), 0));
                if (item.DeltaMarkerControl != null)
                    MoveMarker(item.DeltaMarkerControl, new Point(Canvas.GetLeft(item.DeltaMarkerControl), 0));
            }
        }

        /// <summary>
        /// chart data 입력
        /// </summary>
        private void OnPowerCommand()
        {
            valueList.Clear();
            valueList.Add(new KeyValuePair<double, double>(1855, random.Next(-100, 0)));
            valueList.Add(new KeyValuePair<double, double>(1865, random.Next(-100, 0)));
            valueList.Add(new KeyValuePair<double, double>(1875, random.Next(-100, 0)));
            MarkerUpdateForChartLineUpdate();
            PowerLeft = 100;
            PowerWidth = 150;
        }

        /// <summary>
        /// Marker추가
        /// </summary>
        private void OnMarkerOnCommand()
        {
            MarkerItem markerItem = null;
            foreach(MarkerItem _item in MarkerItems)
            {
                if (_item.MarkerNum != this.Marker) continue;
                markerItem = _item;
                break;
            }
            if (markerItem != null && IsMarkerOn == false)
            {
                IsMarkerDelta = false;
                MarkerItems.Remove(markerItem);
                lineCanvas.Children.Remove(markerItem.MarkerControl);
                lineCanvas.Children.Remove(markerItem.DeltaMarkerControl);
            }
            else if(markerItem == null && IsMarkerOn == true)
            {
                markerItem = new MarkerItem();
                markerItem.MarkerNum = this.Marker;
                markerItem.Freq = ControlCenter;
                markerItem.IsDelta = false;
                MarkerItem.EMarkerColor eMarkerColor = MarkerItem.EMarkerColor.Mark1;
                markerItem.Color = eMarkerColor.FindEnumValue(markerItem.MarkerNum).ToDescription();
                MarkerItems.Add(markerItem);
                markerItem.MarkerControl = MakeMarkControl(markerItem, true);
                markerItem.MarkerControl.MouseDown += StackPanel_MouseDown;
                this.lineCanvas.Dispatcher.Invoke((ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
                MoveMarker(markerItem.MarkerControl, new Point(lineCanvas.ActualWidth / 2, 0));
            }
        }

        /// <summary>
        /// Y축 최댓값 위치로 Marker이동
        /// </summary>
        private void OnMarkerPeakCommand()
        {
            Polyline polyline = null;
            foreach (UIElement element in lineCanvas.Children)
            {
                if (element is Polyline) polyline = element as Polyline;
            }

            MarkerItem markerItem = null;
            foreach (var item in MarkerItems)
            {
                if (item.MarkerNum != this.Marker) return;
                markerItem = item;
                break;
            }

            if (markerItem == null) return;

            PathGeometry pathGeometry = polyline.RenderedGeometry.GetFlattenedPathGeometry();

            List<Tuple<Point, double>> closePoints = new List<Tuple<Point, double>>();
            
            foreach (PathFigure pathFigure in pathGeometry.Figures)
            {
                Point current = pathFigure.StartPoint;
                foreach (PathSegment s in pathFigure.Segments)
                {
                    PolyLineSegment segment = s as PolyLineSegment;
                    LineSegment line = s as LineSegment;
                    Point[] points;
                    if (segment != null)
                    {
                        points = segment.Points.ToArray();
                    }
                    else if (line != null)
                    {
                        points = new[] { line.Point };
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected segment type");
                    }
                    foreach (Point next in points)
                    {
                        double d = (next - current).LengthSquared;
                        closePoints.Add(new Tuple<Point, double>(next, d));
                        current = next;
                    }
                }
            }

            Tuple<Point, double> point = closePoints.OrderBy(p => p.Item1.Y).First();

            MoveMarker(markerItem.MarkerControl, point.Item1);
        }

        /// <summary>
        /// Delta Marker 추가 제거
        /// </summary>
        private void OnMarkerNormalCommand()
        {
            MarkerItem markerItem = null;
            foreach (MarkerItem _item in MarkerItems)
            {
                if (_item.MarkerNum != this.Marker) continue;
                markerItem = _item;
                markerItem.IsDelta = IsMarkerDelta;
                break;
            }

            if(IsMarkerDelta == true)
            {
                markerItem.DeltaMarkerControl = MakeMarkControl(markerItem, false);
                markerItem.DeltaMarkerControl.IsHitTestVisible = false;
                this.lineCanvas.Dispatcher.Invoke((ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
                Canvas.SetLeft(markerItem.DeltaMarkerControl, Canvas.GetLeft(markerItem.MarkerControl));
                Canvas.SetTop(markerItem.DeltaMarkerControl, Canvas.GetTop(markerItem.MarkerControl));
            }
            else
            {
                lineCanvas.Children.Remove(markerItem.DeltaMarkerControl);
            }
        }

        /// <summary>
        /// Marker 동적으로 그리기
        /// </summary>
        /// <param name="markerItem"></param>
        /// <param name="IsNormal"></param>
        /// <returns></returns>
        private StackPanel MakeMarkControl(MarkerItem markerItem, bool IsNormal)
        {
            StackPanel stackPanel = new StackPanel();            
            stackPanel.Width = 15;
            stackPanel.Height = 28;
            TextBlock textBlock = new TextBlock();
            if (IsNormal == true)
                textBlock.Text = markerItem.MarkerNum.ToString();
            else textBlock.Text = markerItem.MarkerNum.ToString() + "R";
            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(markerItem.Color));
            textBlock.TextAlignment = TextAlignment.Center;
            stackPanel.Children.Add(textBlock);

            Polygon polygon = new Polygon();
            polygon.Points.Add(new Point(0, 0));
            polygon.Points.Add(new Point(10, 0));
            polygon.Points.Add(new Point(5, 10));
            polygon.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(markerItem.Color));
            polygon.HorizontalAlignment = HorizontalAlignment.Center;
            stackPanel.Children.Add(polygon);
            lineCanvas.Children.Add(stackPanel);
            return stackPanel;
        }
        
        /// <summary>
        /// Marker 컨트롤 이동 선택 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (stpMarker == null)
            {
                var _stpMarker = sender as StackPanel;
                foreach (var item in MarkerItems)
                {
                    if (item.MarkerControl != _stpMarker && item.DeltaMarkerControl != _stpMarker) continue;
                    if (item.MarkerNum != this.Marker) return;
                    this.stpMarker = _stpMarker;
                    break;
                }                
            }
            else stpMarker = null;
        }
        
        /// <summary>
        /// Control 입력값에 따른 차트 영역 계산
        /// </summary>
        private void XAxisCalc()
        {
            double max = ControlCenter + (ControlSpan / 2);
            double min = ControlCenter - (ControlSpan / 2);
            if(max < XAxisMin)
            {
                XAxisMin = min;
                XAxisMax = max;
            }
            else
            {
                XAxisMax = max;
                XAxisMin = min;
            }
            XAxisInterval = (XAxisMax - XAxisMin) / 8;
            MarkerUpdateForChartLineUpdate();
        }

        /// <summary>
        /// Display 입력값에 따른 차트 영역 계산
        /// </summary>
        private void YAxisCalc()
        {
            YAxisMin = DisplayReferance - (DisplayScale * 10);
            MarkerUpdateForChartLineUpdate();
        }

        /// <summary>
        /// Marker 움직이기
        /// </summary>
        /// <param name="markerControl"></param>
        /// <param name="_point"></param>
        private void MoveMarker(StackPanel markerControl, Point _point)
        {
            if (markerControl == null) return;
            Polyline polyline = null;
            foreach (UIElement element in lineCanvas.Children)
            {
                if (element is Polyline) polyline = element as Polyline;
            }

            Point point = GetClosestPointOnPath(_point, polyline.RenderedGeometry);            
            Canvas.SetLeft(markerControl, point.X - (markerControl.ActualWidth / 2));
            Canvas.SetTop(markerControl, point.Y - markerControl.ActualHeight);

            foreach(var item in MarkerItems)
            {
                if(item.MarkerControl == markerControl)
                {
                    var value = PixelPositionToValue(point);
                    item.Freq = value.X;
                    item.Value = value.Y;
                }
                else if (item.DeltaMarkerControl == markerControl)
                {
                    var value = PixelPositionToValue(point);
                    item.DeltaFreq = value.X;
                    item.DeltaValue = value.Y;
                }
            }
            markerControl.Dispatcher.Invoke((ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// 마우스 위치로 x, y 좌표값 찾기
        /// </summary>
        /// <returns></returns>
        private Point PixelPositionToValue(Point point)
        {
            IRangeAxis xAxisRange = null;
            IRangeAxis yAxisRange = null;
            foreach (var axis in chart.ActualAxes)
            {
                if (axis.Orientation == AxisOrientation.X) xAxisRange = axis as IRangeAxis;
                else if (axis.Orientation == AxisOrientation.Y) yAxisRange = axis as IRangeAxis;
            }

            var mouseXPositionInChartUnits = (double)xAxisRange.GetValueAtPosition(new UnitValue(point.X, Unit.Pixels));
            var mouseYPositionInChartUnits = (double)yAxisRange.GetValueAtPosition(new UnitValue(lineCanvas.ActualHeight - point.Y, Unit.Pixels));

            return new Point(mouseXPositionInChartUnits, mouseYPositionInChartUnits);
        }

        public void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (stpMarker == null) return;
            Point point = e.GetPosition(lineCanvas);
            MoveMarker(stpMarker, point);
        }

        private Point GetClosestPointOnPath(Point p, Geometry geometry)
        {
            PathGeometry pathGeometry = geometry.GetFlattenedPathGeometry();

            var points = pathGeometry.Figures.Select(f => GetClosestPointOnPathFigure(f, p))
                .OrderBy(t => t.Item2).FirstOrDefault();
            return (points == null) ? new Point(0, 0) : points.Item1;
        }

        private Tuple<Point, double> GetClosestPointOnPathFigure(PathFigure figure, Point p)
        {
            List<Tuple<Point, double>> closePoints = new List<Tuple<Point, double>>();
            Point current = figure.StartPoint;
            foreach (PathSegment s in figure.Segments)
            {
                PolyLineSegment segment = s as PolyLineSegment;
                LineSegment line = s as LineSegment;
                Point[] points;
                if (segment != null)
                {
                    points = segment.Points.ToArray();
                }
                else if (line != null)
                {
                    points = new[] { line.Point };
                }
                else
                {
                    throw new InvalidOperationException("Unexpected segment type");
                }
                foreach (Point next in points)
                {
                    Point closestPoint = GetClosestPointOnLine(current, next, p);
                    double d = (closestPoint - p).LengthSquared;
                    closePoints.Add(new Tuple<Point, double>(closestPoint, d));
                    current = next;
                }
            }
            return closePoints.OrderBy(t => t.Item2).First();
        }

        private Point GetClosestPointOnLine(Point start, Point end, Point p)
        {
            Point returnPoint = start;
            double length = (start - end).LengthSquared;
            if (length == 0.0)
            {
                return returnPoint;
            }
            Vector v = end - start;
            double param = (p - start) * v / length;
            if(p.Y== 0)
                returnPoint = (param < 0.0) ? end : (param > 1.0) ? start : (end + param * v);
            else returnPoint = (param < 0.0) ? start : (param > 1.0) ? end : (start + param * v);
            return (param < 0.0) ? start : (param > 1.0) ? end : (start + param * v);
            //double length = (start.X - end.X);
            //if (length == 0.0)
            //{
            //    return start;
            //}
            //Vector v = end - start;
            //double param = (p.X - start.X) * v.X / length;
            //return (param < 0.0) ? end : (param > 1.0) ? start : (end + param * v);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
