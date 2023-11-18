using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace Pactometro
{

    public partial class MainWindow : Window
    {

        //Delegado encargado de gestionar cambios de ventana.
        public delegate void delegadoCambioV(object sender, RoutedEventArgs a);


        private delegadoCambioV _v;
        private Dictionary<string, Partido> bufPartidos;
        public MainWindow()
        {
            InitializeComponent();
            //Le añadimos al delegado el metodo de cambiar de ventana.
            _v += DatosGraficas.NewWindow;

            //Cuando el tamaño de la ventana cambie, el canvas se va a reescalar.
            SizeChanged += MainWindow_SizeChanged;

            //Cuando la ventana principal se cierre todas las demas se tienen que cerrar
            Closing += MainWindow_Closing;

            //Cuando la ventana principal se cargue, establecemos la instancia del singleton para evitarnos problemas.
            Loaded += setInstance;

            StateChanged += stateChanged;
        }


        private void SwitchWindow(object sender, RoutedEventArgs e)
        {
            _v?.Invoke(sender, e);
        }

        public void OnDataSelected(object sender, CustomEventArgsMain e)
        {

            //Limpiamos el lienzo por si habia datos.
            lienzo.Children.Clear();

            //Ponemos un color beige de fondo.
            lienzo.Background = Brushes.Beige;

            // Actualizar el textblock con el nombre de la elección.
            tituloGrafica.Text = e.name;

            /*
            name = e.name;

            //Guardamos en un array de elecciones los datos de los partidos que participan en ellas.
            for (int i = 0; i < this.elecciones.Length; i++)
            {
                if (this.elecciones[i] == null)
                {
                    Dictionary<string,Partido> keyValuePairs = new Dictionary<string,Partido>();
                    foreach(var keyValue in bufPartidos)
                    {
                        keyValuePairs.Add(keyValue.Key,keyValue.Value);
                    }
                    this.elecciones[i] = keyValuePairs;
                    break;
                }
                
            }
            */

            //Guardamos en el buffer el ultimo dato seleccionado.
            
            bufPartidos = new Dictionary<string, Partido>();
            foreach (var partido in e.infoPartidos)
            {
                bufPartidos.Add(partido.Key, partido.Value);
            }
            
            //Graficamos.
            GraphBarras(e.infoPartidos);
            
        }

        private void GraphBarras(Dictionary<string, Partido> infoPartidos, double maxHeightScale = 0.8)
        {

            double maxHeight = lienzo.ActualHeight; // Obtener la altura maxima del lienzo
            double barWidth = 15; // Ancho de la barra
            int maxVotes = infoPartidos.Values.Max(partido => partido.Votos); // Obtener el numero maximo de votos.

            //Creamos los ejes de coordenadas.
            Line yAxis = new Line
            {
                X1 = 0, 
                Y1 = 0,
                X2 = 0, 
                Y2 = lienzo.ActualHeight, 
                Stroke = Brushes.Black, 
                StrokeThickness = 2 
            };

            Line xAxis = new Line
            {
                X1 = 0,
                Y1 = lienzo.ActualHeight,
                X2 = lienzo.ActualWidth,
                Y2 = lienzo.ActualHeight,
                Stroke = Brushes.Black, 
                StrokeThickness = 2 
            };
            

            // Añadimos los ejes al canvas
            lienzo.Children.Add(yAxis);
            lienzo.Children.Add(xAxis);

            int xPos = 10;
            foreach(var partido in infoPartidos)
            {
                string nombre = partido.Key;
                int escaños = partido.Value.Votos;
                System.Drawing.Color color = partido.Value.Color;
                Color partidoColor;
                double barHeight = (partido.Value.Votos / (double)maxVotes) * maxHeight;

                if (color.ToString() != "Color [Empty]")
                {
                    partidoColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                }
                else
                {
                    partidoColor = System.Windows.Media.Colors.Blue;
                }

                Rectangle bar = new()
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new SolidColorBrush(partidoColor),
                    Margin = new Thickness(0, 0, 0, 0)
                };

                bar.ToolTip = new ToolTip { Content = $"{partido.Value.Votos} votos" };

                //Añadimos el evento al gestor del primer elemento que pasemos como parametro.
                AddEventHandler(bar, bar);

                Canvas.SetLeft(bar, xPos);
                Canvas.SetBottom(bar, 0);
                lienzo.Children.Add(bar);
                xPos += 50;
            }
        }







        private static void AddEventHandler(FrameworkElement e,Rectangle barra)
        {
            /*
             Manejadora asociada al doble click del raton sobre una barra o sobre el nombre,
             lo cual le permite al usuario cambiar el color solo en la vista.
             */

            e.MouseLeftButtonDown += (sender, e) =>
            {
                //Con doble click, mostramos el modal de elección del color.
                if (e.ClickCount == 2)
                {
                    var colorDialog = new System.Windows.Forms.ColorDialog();
                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // Añadimos el color que haya seleccionado al usuario a la barra.
                        SolidColorBrush brush = new(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                        barra.Fill = brush;
                    }
                }
            };
        }

        public void OnCloseDatosGraficas(object sender, EventArgs e)
        {
            /*
             * 
             Cuando se haya cerrado la ventana secundaria, la cual es la encargada 
             de determinar que elecciones se grafican y cuales no, debemos limpiar
             el lienzo, eliminar el titulo de la eleccion, volver transparente el
             fondo y eliminar de los buffers elecciones y partidos los datos restantes.

             */

            limpiaLienzo();
            /*
            this.bufPartidos?.Clear();
            for(int i = 0;i< this.elecciones.Length;i++)
            {
                if (this.elecciones[i] != null)
                {
                    this.elecciones[i] = null;

                }
                else
                {
                    //El bucle finalizará cuando encuentre el primer indice que valga null.

                    break;
                }
            }
            */
        }

        private void GraphCompare(object sender, RoutedEventArgs e)
        {
            
        }
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cerramos todas las ventanas que estén abiertas.
            foreach (Window window in Application.Current.Windows)
            {
                if (window != this)
                {
                    window.Close();
                }
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Call a method to update the canvas size when the window size changes
            UpdateCanvasSize();
        }

        private void UpdateCanvasSize()
        {
            double margin = 50;
            lienzo.Width = ActualWidth - 2 * margin;
            lienzo.Height = ActualHeight - 2 * margin;
            lienzo.Children.Clear();
            if(bufPartidos != null)
            {
                lienzo.Background = Brushes.Beige;
                GraphBarras(bufPartidos);

            }

        }

        private void stateChanged(object sender,EventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                UpdateCanvasSize();
            }
        }

        private void setInstance(object sender,RoutedEventArgs e)
        {
            Utils.MainWindowSingleton.setInstance(this);
        }

        public void removeData(object sender,EventArgs? e)
        {
            limpiaLienzo();
        }


        private void limpiaLienzo()
        {
            lienzo.Children.Clear();
            tituloGrafica.Text = string.Empty;
            lienzo.Background = Brushes.Transparent;
        }
    }
}
