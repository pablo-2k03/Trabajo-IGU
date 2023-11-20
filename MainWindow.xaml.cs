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
        private DatosGraficas c;
        public MainWindow()
        {
            InitializeComponent();
            //Le añadimos al delegado el metodo de cambiar de ventana.
            //Cuando el tamaño de la ventana cambie, el canvas se va a reescalar.
            SizeChanged += MainWindow_SizeChanged;
            c = Utils.DatosGraficasWindowSingleton.GetInstance();
            _v += c.NewWindow;
            c.DataSelected += OnDataSelected;
            c.DatosGraficasClosed += OnCloseDatosGraficas;
            c.removeData += removeData;

            //Cuando la ventana principal se cierre todas las demas se tienen que cerrar
            Closing += MainWindow_Closing;

            //Cuando la ventana principal se cargue, establecemos la instancia del singleton para evitarnos problemas.
            Loaded += setInstance;

            StateChanged += stateChanged;
        }


        private void SwitchWindow(object sender, RoutedEventArgs e)
        {
            if(c == null)
            {
                c = Utils.DatosGraficasWindowSingleton.GetInstance();
                c.DataSelected += OnDataSelected;
                c.DatosGraficasClosed += OnCloseDatosGraficas;
                c.removeData += removeData;
            }

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

        private void GraphBarras(Dictionary<String, Partido> infoPartidos,Boolean isMax=false)
        {
            double maxHeight = lienzo.ActualHeight; // Maximum height of bars (adjust as needed)
            double barWidth = 20; // Width of each bar
            double xPos = 20; // X-coordinate to start drawing bars
            int maxVotes = infoPartidos.Values.Max(partido => partido.Votos); // Maximum number of votes

            if(isMax)
            {

                dibujarMarcas(maxVotes, maxHeight,5);
            }
            else
            {
                dibujarMarcas(maxVotes, maxHeight, 20);
            }


            foreach (var p in infoPartidos)
            {
                double barHeight = (p.Value.Votos / (double)maxVotes) * maxHeight;

                Partido partido = p.Value;

                Color partidoColor;

                //Comprobamos si la propiedad Color del partido tiene asignado algo o es nulo, si lo tiene se lo asignamos y sino dejamos el azul por defecto.

                if (partido.Color.ToString() != "Color [Empty]")
                {
                    partidoColor = System.Windows.Media.Color.FromArgb(partido.Color.A, partido.Color.R, partido.Color.G, partido.Color.B);
                }
                else
                {
                    partidoColor = System.Windows.Media.Colors.Blue;
                }

                Rectangle bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new SolidColorBrush(partidoColor)
                };

                // Añadimos un toolTip para ver el numero de votos que tiene un partido cuando el raton pasa por encima.
                bar.ToolTip = new ToolTip { Content = $"{p.Value.Votos} votos" };

                //Añadimos el evento al gestor del primer elemento que pasemos como parametro.
                AddEventHandler(bar, bar);


                // Position the bar and add it to the canvas
                Canvas.SetLeft(bar, xPos);
                Canvas.SetBottom(bar, 0); // Align bars to the bottom of the canvas
                lienzo.Children.Add(bar);



                // Añadimos el nombre del partido en la parte inferior.
                TextBlock partyNameLabel = new TextBlock
                {
                    Text = p.Key,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top
                };

                //También se lo añadimos al nombre, por si se da el caso de que la barra sea tan pequeña q casi ni se vea.
                AddEventHandler(partyNameLabel, bar);

                //Calculamos el ancho del texto utilizando la clase FormattedText.
                FormattedText formattedText = new FormattedText(
                    p.Key,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(partyNameLabel.FontFamily, partyNameLabel.FontStyle, partyNameLabel.FontWeight, partyNameLabel.FontStretch),
                    partyNameLabel.FontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    1);

                double textWidth = formattedText.Width;

                //Ajustamos el nombre del partido de acuerdo a su ancho y al ancho de la barra.
                partyNameLabel.Margin = new Thickness(xPos + (barWidth / 2) - (textWidth / 2), 0, 0, 0);

                Canvas.SetBottom(partyNameLabel, -20);
                lienzo.Children.Add(partyNameLabel);

                //Añadimos espaciado entre barras.
                xPos += barWidth + 32;

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
            c.DataSelected -= OnDataSelected;
            c.DatosGraficasClosed -= OnCloseDatosGraficas;
            c.removeData -= removeData;
            c = null;
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

        private void UpdateCanvasSize(Boolean max=false)
        {
            double margin = 80;
            lienzo.Width = ActualWidth - 2 * margin;
            lienzo.Height = ActualHeight - 2 * margin;
            lienzo.Children.Clear();
            if(bufPartidos != null)
            {
                lienzo.Background = Brushes.Beige;
                GraphBarras(bufPartidos,max);

            }

        }

        private void stateChanged(object? sender,EventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                Boolean isMaxizimed = true;
                UpdateCanvasSize(isMaxizimed);
            }
            else 
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

        private void dibujarMarcas(int maxVotes,double maxHeight,int intervalo)
        {
            // Draw marks at the left going from 10 to the max number of votes in increments of 10
            for (int i = 10; i <= maxVotes; i += intervalo)
            {


                Line mark = new Line
                {
                    X1 = 0,
                    X2 = 5,
                    Y1 = maxHeight - (i / (double)maxVotes * maxHeight),
                    Y2 = maxHeight - (i / (double)maxVotes * maxHeight),
                    Stroke = Brushes.Black
                };

                lienzo.Children.Add(mark);

                TextBlock markLabel = new TextBlock
                {
                    Text = i.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(-25, maxHeight - (i / (double)maxVotes * maxHeight) - 10, 0, 0)
                };

                lienzo.Children.Add(markLabel);
            }
        }



    }
}
