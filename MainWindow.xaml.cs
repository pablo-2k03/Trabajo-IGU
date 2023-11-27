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

        //Instancias de objetos
        private DatosGraficas c;
        private ModeloDatos modeloUnico;

        //Lista de elecciones para graficar.
        private List<Eleccion> eleccionesAGraficar;

        //Buffer para guardar las ultimas elecciones seleccionadas para comparar.
        private List<Eleccion> eleccionesACoomparar;

        //Condiciones que comprueban diferentes casos del programa.
        private Boolean graficoComparativo = false;
        private Boolean marcasDibujadas = false;
        public MainWindow()
        {
            InitializeComponent();

            //Instanciación de variables.
            eleccionesAGraficar = new List<Eleccion>();
            eleccionesACoomparar = new List<Eleccion>();

            //Cuando el tamaño de la ventana cambie, el canvas se va a reescalar.
            SizeChanged += MainWindow_SizeChanged;

            modeloUnico = Utils.DataModelSingleton.GetInstance();
            modeloUnico.LoadDataTests();

            //Instanciacion y suscripción de las manejadoras a los eventos de esta.
            c = Utils.DatosGraficasWindowSingleton.GetInstance();
            c.DataSelected += OnDataSelected;
            c.DatosGraficasClosed += OnCloseDatosGraficas;
            c.CompararElecciones += OnDataSelectedToCompare;

            //Nos ponemos a escuchar cambios en el modelo de datos.
            modeloUnico.ResultadosElectorales.CollectionChanged += OnCollectionChanged;


            //Cuando la ventana principal se cierre todas las demas se tienen que cerrar
            Closing += MainWindow_Closing;


        }


        private void SwitchWindow(object sender, RoutedEventArgs e)
        {
            if(c == null)
            {
                c = Utils.DatosGraficasWindowSingleton.GetInstance();
                c.DataSelected += OnDataSelected;
                c.DatosGraficasClosed += OnCloseDatosGraficas;
                c.CompararElecciones += OnDataSelectedToCompare;
            }

            c.Show();
        }

        public void OnDataSelected(object? sender, CustomEventArgs e)
        {
            eleccionesAGraficar.Clear();
            this.marcasDibujadas = false;
            //Limpiamos el lienzo por si habia datos.
            lienzo.Children.Clear();

            //Ponemos un color beige de fondo.
            lienzo.Background = Brushes.Beige;

            foreach(var i in e.elecciones)
            {
                eleccionesAGraficar.Add(i);
            }
            tituloGrafica.Text = e.elecciones[0].Nombre;
            //Graficamos.       
            GraphBarras(e.elecciones);
            
        }

        private void GraphBarras(List<Eleccion> eleccionesSeleccionadas,Boolean isMax=false)
        {
            double maxHeight = lienzo.ActualHeight - 40; // La altura maxima de las barras es la del lienzo - el margen que le dejamos al nombre.
            double barWidth = 20; // Ancho de las barras
            double maxBarWidth = 20 * eleccionesSeleccionadas.Count;
            double xPos = 20;
            double espacioEntreBarras = 0;
            Dictionary<string,Partido> nombresElectorales = new Dictionary<string, Partido>();
            Dictionary<string,double> posiciones = new Dictionary<string,double>();
            Boolean tieneMasElecciones = false;
            int i = 0;
 
            foreach (var eleccion in eleccionesSeleccionadas)
            {
                if (!tieneMasElecciones)
                {
                    espacioEntreBarras = (lienzo.ActualWidth - barWidth) / eleccion.Partidos.Count - 1;
                }

                int maxVotes = eleccion.Partidos.Values.Max(partido => partido.Votos);
                Dictionary<String, Partido> partidos = new Dictionary<String, Partido>();
                partidos = eleccion.Partidos;
                

                if (isMax)
                {
                    if (!this.marcasDibujadas)
                    {
                        dibujarMarcas(maxVotes, maxHeight, 5);
                        this.marcasDibujadas = true;
                    }
                }
                else
                {
                    if (!this.marcasDibujadas)
                    {
                        dibujarMarcas(maxVotes, maxHeight, 20);
                        this.marcasDibujadas = true;
                    }
                }


                foreach (var p in partidos)
                {

                    double barHeight = (p.Value.Votos / (double)maxVotes) * maxHeight;
                    Partido partido = p.Value;

                    Color partidoColor;

                    //Comprobamos si la propiedad Color del partido tiene asignado algo o es nulo, si lo tiene se lo asignamos y sino dejamos el azul por defecto.

                    if (partido.Color.ToString() != "Color [Empty]")
                    {
                        partidoColor = System.Windows.Media.Color.FromArgb(partido.Color.A, partido.Color.R, partido.Color.G, partido.Color.B);
                    }

                    if(nombresElectorales.ContainsKey(p.Value.Nombre))
                    {
                        Partido p1 = nombresElectorales[p.Value.Nombre];
                        partidoColor = System.Windows.Media.Color.FromArgb(p1.Color.A, p1.Color.R, p1.Color.G, p1.Color.B);

                        if (posiciones.ContainsKey(p.Value.Nombre))
                        {
                            xPos = posiciones[p.Value.Nombre]+barWidth;
                            if(i > 1)
                            {
                                xPos = posiciones[p.Value.Nombre] + 2*barWidth;
                            }
                        }
                        else
                        {
                            xPos = 2*espacioEntreBarras+barWidth;
                        }
                    }

                    Rectangle bar = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = new SolidColorBrush(partidoColor)
                    };

                    if(tieneMasElecciones)
                    {

                        if (nombresElectorales.ContainsKey(p.Value.Nombre))
                        {
                            bar.Opacity = 0.5;
                        }
                    }

                    // Añadimos un toolTip para ver el numero de votos que tiene un partido cuando el raton pasa por encima.
                    bar.ToolTip = new ToolTip { Content = $"{p.Value.Votos} escaños" };

                    //Añadimos el evento al gestor del primer elemento que pasemos como parametro.
                    AddEventHandler(bar, bar,p.Value,eleccion);
                    // Position the bar and add it to the canvas
                    Canvas.SetLeft(bar, xPos);
                    Canvas.SetBottom(bar, 20);
                    lienzo.Children.Add(bar);



                    // Añadimos el nombre del partido en la parte inferior.
                    TextBlock partyNameLabel = new TextBlock
                    {
                        Text = p.Key,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    //También se lo añadimos al nombre, por si se da el caso de que la barra sea tan pequeña q casi ni se vea.
                    AddEventHandler(partyNameLabel, bar,p.Value,eleccion);

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

                    //En caso de que nos hayamos dejado el nombre de algún partido, lo ponemos.
                    if (!nombresElectorales.ContainsKey(partyNameLabel.Text))
                    {
                        Canvas.SetBottom(partyNameLabel, 0);
                        lienzo.Children.Add(partyNameLabel);
                        nombresElectorales.Add(p.Value.Nombre,p.Value);
                        posiciones.Add(p.Value.Nombre, xPos);
                    }

                    //Añadimos espaciado entre barras.
                    xPos += espacioEntreBarras;
                    if(tieneMasElecciones) { xPos+= i*barWidth-20; }
                }
                if (graficoComparativo)
                {
                    xPos = 20 + barWidth;
                    tieneMasElecciones = true;
                    i++;
                }
            }   
        }

        private void AddEventHandler(FrameworkElement e,Rectangle barra,Partido p,Eleccion el)
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
                        int indiceEleccion = modeloUnico.ResultadosElectorales.IndexOf(el);

                        //Lo actualizamos en el modelo.
                        foreach( var i in modeloUnico.ResultadosElectorales[indiceEleccion].Partidos.Values)
                        {
                            if( i.Nombre == p.Nombre)
                            {
                                i.Color = colorDialog.Color;
                                break;
                            }
                        }

                    }
                }
            };
        }

        public void OnCloseDatosGraficas(object? sender, EventArgs e)
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
            c.CompararElecciones -= OnDataSelectedToCompare;
            c = null;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cerramos todas las ventanas que estén abiertas.
            Application.Current.Shutdown();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Boolean isMaxizimed = true;
                UpdateCanvasSize(isMaxizimed);
            }
            else
            {
                UpdateCanvasSize();
            }
        }

        private void UpdateCanvasSize(Boolean max=false)
        {
            double margin = 80;
            lienzo.Width = ActualWidth - 2 * margin;
            lienzo.Children.Clear();


            if (this.graficoComparativo)
            {
                lienzo.Background = Brushes.Beige;
                this.marcasDibujadas = false;
                GraphBarras(eleccionesACoomparar, max);
            }
            if(eleccionesAGraficar != null)
            {
                lienzo.Background = Brushes.Beige;
                this.marcasDibujadas=false;
                GraphBarras(eleccionesAGraficar,max);

            }

        }

        public void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Si se ha eliminado un elemento de la colección, limpiamos el lienzo.
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                limpiaLienzo();
            }
            //Si se ha modificado un elemento de la colección, lo graficamos de nuevo.
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {

                if (e.NewItems != null)
                {
                    this.eleccionesAGraficar.Clear();

                    Eleccion newItem = (Eleccion)e.NewItems[0];

                    if (newItem == null) return;

                    limpiaLienzo();
                    lienzo.Background = Brushes.Beige;
                    tituloGrafica.Text = newItem.Nombre;

                    this.eleccionesAGraficar.Add(newItem);
                    GraphBarras(eleccionesAGraficar);
                }
                
            }
            else
            {
                limpiaLienzo();
            }
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
                    Stroke = Brushes.Red
                };

                lienzo.Children.Add(mark);

                TextBlock markLabel = new TextBlock
                {
                    Text = i.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(-25, maxHeight - (i / (double)maxVotes * maxHeight) - 10, 0, 0),
                    Foreground = Brushes.Red
                };

                lienzo.Children.Add(markLabel);
            }
        }

        private void compararGraficos(List<Eleccion> elecciones)
        {
            this.marcasDibujadas = false;
            limpiaLienzo();
            lienzo.Background = Brushes.Beige;
            GraphBarras(elecciones, false);
            
        }

        private void OnDataSelectedToCompare(object? sender,CustomEventArgs c)
        {

            this.graficoComparativo = true;
            this.eleccionesACoomparar.Clear();

            //Guardamos.
            foreach(Eleccion eleccion in c.elecciones)
            {
                this.eleccionesACoomparar.Add(eleccion);
            }
            compararGraficos(eleccionesACoomparar);
        }

    }
}
