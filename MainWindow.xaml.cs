using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphicEditor
{
    public partial class MainWindow : Window
    {
        private List<Ellipse> points = new List<Ellipse>(); // Список всех точек
        private Dictionary<Ellipse, List<Line>> pointLines = new Dictionary<Ellipse, List<Line>>(); // Связи точек и линий
        private Ellipse firstPoint = null; // Первая точка для линии
        private Ellipse selectedPoint = null; // Точка для редактирования
        private bool isEditMode = false; // Режим редактирования
        private bool isDragging = false; // Флаг для перетаскивания

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditMode)
            {
                // В режиме редактирования выбираем точку для перетаскивания
                Point clickedPoint = e.GetPosition(DrawCanvas);
                selectedPoint = GetClickedPoint(clickedPoint);

                if (selectedPoint != null)
                {
                    isDragging = true; // Начинаем перетаскивание
                }
            }
            else
            {
                // Добавление новой точки
                Point clickedPoint = e.GetPosition(DrawCanvas);

                Ellipse point = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Black,
                    Tag = clickedPoint // Сохраняем координаты точки в свойстве Tag
                };

                Canvas.SetLeft(point, clickedPoint.X - 5); // Центруем точку
                Canvas.SetTop(point, clickedPoint.Y - 5);

                points.Add(point); // Добавляем точку в список
                pointLines[point] = new List<Line>(); // Создаём пустой список линий для точки
                DrawCanvas.Children.Add(point); // Добавляем точку на Canvas
            }
        }

        private void DrawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isEditMode)
            {
                // Проведение линии между точками
                Point clickedPoint = e.GetPosition(DrawCanvas);

                // Ищем точку, на которой произошёл клик
                Ellipse clickedEllipse = GetClickedPoint(clickedPoint);

                if (clickedEllipse != null)
                {
                    if (firstPoint == null)
                    {
                        // Если первая точка не выбрана, выбираем её
                        firstPoint = clickedEllipse;
                        HighlightPoint(firstPoint, true); // Подсвечиваем точку
                    }
                    else
                    {
                        // Если первая точка выбрана, рисуем линию ко второй
                        Point p1 = (Point)firstPoint.Tag;
                        Point p2 = (Point)clickedEllipse.Tag;
                        if (firstPoint != clickedEllipse)
                        {
                            Line line = new Line
                            {
                                X1 = p1.X,
                                Y1 = p1.Y,
                                X2 = p2.X,
                                Y2 = p2.Y,
                                Stroke = Brushes.Black,
                                StrokeThickness = 2
                            };

                            // Добавляем линию на Canvas
                            DrawCanvas.Children.Add(line);

                            // Связываем линию с обеими точками
                            pointLines[firstPoint].Add(line);
                            pointLines[clickedEllipse].Add(line);

                            HighlightPoint(firstPoint, false); // Убираем подсветку
                            firstPoint = null; // Сбрасываем первую точку
                        }
                        else
                        {
                            HighlightPoint(firstPoint, false);
                            firstPoint = null;
                        }
                    }
                }
            }
        }

        private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedPoint != null)
            {
                // Обновляем положение точки при перетаскивании
                Point newPosition = e.GetPosition(DrawCanvas);
                Canvas.SetLeft(selectedPoint, newPosition.X - 5);
                Canvas.SetTop(selectedPoint, newPosition.Y - 5);
                selectedPoint.Tag = newPosition; // Обновляем координаты

                // Обновляем линии, связанные с точкой
                UpdateLinesForPoint(selectedPoint);
            }
            
            
        }

        private void DrawCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false; // Завершаем перетаскивание
                selectedPoint = null;
            }
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            // Очистка холста
            DrawCanvas.Children.Clear();
            points.Clear();
            pointLines.Clear();
            firstPoint = null;
            selectedPoint = null;
        }

        private void EnableEditMode_Click(object sender, RoutedEventArgs e)
        {
            // Переключение в режим редактирования
            isEditMode = !isEditMode;
            (sender as Button).Content = isEditMode ? "Режим редактирования вкл" : "Режим редактирования выкл";
        }

        private Ellipse GetClickedPoint(Point position)
        {
            // Проверяем, есть ли точка в области клика
            foreach (var point in points)
            {
                double x = Canvas.GetLeft(point) + 5;
                double y = Canvas.GetTop(point) + 5;

                if (IsPointNear(position, new Point(x, y)))
                {
                    return point;
                }
            }

            return null; // Если точка не найдена
        }

        private bool IsPointNear(Point p1, Point p2, double threshold = 10)
        {
            // Проверка, находится ли точка вблизи другой точки
            return (p1 - p2).Length <= threshold;
        }

        private void HighlightPoint(Ellipse point, bool highlight)
        {
            // Подсвечиваем или убираем подсветку у точки
            point.Fill = highlight ? Brushes.Red : Brushes.Black;
        }

        private void UpdateLinesForPoint(Ellipse point)
        {
            // Обновляем координаты всех линий, связанных с точкой
            if (pointLines.ContainsKey(point))
            {
                Point newPosition = (Point)point.Tag;
                foreach (var line in pointLines[point])
                {
                    if (IsPointNear(newPosition, new Point(line.X1, line.Y1)))
                    {
                        line.X1 = newPosition.X;
                        line.Y1 = newPosition.Y;
                    }
                    else
                    {
                        line.X2 = newPosition.X;
                        line.Y2 = newPosition.Y;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.Clear();
            foreach (var pair in pointLines)
            {
                // Выводим свойства эллипса (если есть, например, координаты)
                Console.WriteLine($"Ellipse - X: {Canvas.GetLeft(pair.Key)}, Y: {Canvas.GetTop(pair.Key)}");

                foreach (var line in pair.Value)
                {
                    // Выводим свойства линии
                    Console.WriteLine($"  Line - X1: {line.X1}, Y1: {line.Y1}, X2: {line.X2}, Y2: {line.Y2}");
                }
            }
        }
    }
}
