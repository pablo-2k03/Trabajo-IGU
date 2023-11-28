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
        private Boolean graficandoPactometro = false;
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
            c.seleccionEliminada += OnDataNotSelectedAnymore;
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
                c.seleccionEliminada += OnDataNotSelectedAnymore;
            }

            c.Show();
        }

        public void OnDataSelected(object? sender, CustomEventArgs e)
        {
            eleccionesAGraficar.Clear();
            this.marcasDibujadas = false;
            this.graficoComparativo = false;
            this.graficandoPactometro = false;

            //Limpiamos el lienzo por si habia datos.
            lienzo.Children.Clear();

            //Añadimos las elecciones a la lista de eleccionesAGraficar
            foreach(var i in e.elecciones)
            {
                eleccionesAGraficar.Add(i);
            }

            //Le ponemos el titulo de la primera eleccion q haya en caso de q haya mas de 1.
            tituloGrafica.Text = e.elecciones[0].Nombre;
            lienzo.Background = Brushes.Beige;

            //Graficamos.       
            GraphBarras(e.elecciones);
            
        }

        private void GraphBarras(List<Eleccion> eleccionesSeleccionadas,Boolean isMax=false)
        {
            double maxHeight = lienzo.ActualHeight - 40; // La altura maxima de las barras es la del lienzo - el margen que le dejamos al nombre.
            double barWidth = 20; // Ancho de las barras
            double xPos = 20;
            double espacioEntreBarras = 0;
            Dictionary<string,Partido> nombresElectorales = new Dictionary<string, Partido>();
            Dictionary<string,double> posiciones = new Dictionary<string,double>();
            Boolean tieneMasElecciones = false;
            int totalElecciones = eleccionesSeleccionadas.Count;
            int i = 0;
            int totalPartidos=0;
            int maxVotes = 0;
            List<string> registro = new ();

            //Añadimos un tooltip por si el usuario no sabe comparar graficas.
            if(!graficoComparativo && eleccionesSeleccionadas.Count>0)
            {
                ToolTip infoComparar = new ToolTip();

                // Content of the ToolTip
                TextBlock titleTextBlock = new TextBlock
                {
                    FontWeight = FontWeights.Bold,
                    Text = "Información adicional"
                };

                TextBlock contentTextBlock = new TextBlock
                {
                    Text = "Para comparar dos gráficas, primero selecciónelas en la ventana secundaria y posteriormente compare en el menú Elecciones --> Comparar gráficas."
                };

                infoComparar.Content = contentTextBlock;

                Grid grid = new Grid();
                Ellipse e = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    Fill = Brushes.AliceBlue,
                    ToolTip = infoComparar,
                };

                TextBlock questionMark = new TextBlock
                {
                    Text = "?",
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                grid.Children.Add(e);
                grid.Children.Add(questionMark);

                Canvas.SetTop(grid, 0);
                Canvas.SetRight(grid, 0);
                lienzo.Children.Add(grid);
                ToolTipService.SetIsEnabled(grid, true);
            }

            //Contamos el numero total de partidos distintos que va a ver en las elecciones.
            foreach(Eleccion ele in eleccionesSeleccionadas)
            {
                foreach ( var ppp in ele.Partidos)
                {
                    if (!registro.Contains(ppp.Value.Nombre))
                    {
                        registro.Add(ppp.Value.Nombre);
                        totalPartidos++;
                    }
                    if(ppp.Value.Votos > maxVotes)
                    {
                        maxVotes = ppp.Value.Votos;
                    }
                }
            }

            espacioEntreBarras = (lienzo.ActualWidth - barWidth) / totalPartidos - 1;
          

            foreach (var eleccion in eleccionesSeleccionadas)
            {
                

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
                    //Comprobamos si el nombre en la segunda iteracion ya está registrado.
                    if(nombresElectorales.ContainsKey(p.Value.Nombre))
                    {

                        //Si lo está, obtenemos el color del registro con su posicion, si lo est.a
                        Partido p1 = nombresElectorales[p.Value.Nombre];
                        partidoColor = System.Windows.Media.Color.FromArgb(p1.Color.A, p1.Color.R, p1.Color.G, p1.Color.B);

                        if (posiciones.ContainsKey(p.Value.Nombre))
                        {
                            xPos = posiciones[p.Value.Nombre]+barWidth;

                            //Si estamos en >2 elecciones, a las nuevas barras se le desplaza a la derecha.
                            if(i > 1)
                            {
                                xPos = posiciones[p.Value.Nombre] + 2*barWidth;
                            }
                        }
                        
                    }
                    else
                    {
                        //Si no está en la lista de partidos, significa que es un partido que solo ha participado en la n-esima elección.
                        if (tieneMasElecciones && i >= 1 )
                        {

                            //Buscamos en el registro de posiciones la mas cercana y le sumamos a xPos el espacio del separador
                            double posicionMasCercana = double.MaxValue;
                            foreach (var posicion in posiciones)
                            {
                                double distancia = Math.Abs(posicion.Value - xPos);
                                if (distancia < Math.Abs(posicionMasCercana - xPos))
                                {
                                    posicionMasCercana = posicion.Value;
                                    break;
                                }
                            }
                            xPos = posicionMasCercana + espacioEntreBarras; 
                            if(posiciones.Values.Contains(xPos))
                            {
                                double masgrande=0;
                                //Buscamos la ultima posicion
                                foreach(var posicion in posiciones)
                                {
                                    if(posicion.Value> masgrande)
                                    {
                                        masgrande = posicion.Value;
                                    }
                                }
                                xPos = masgrande+espacioEntreBarras;
                                posiciones.Add(p.Value.Nombre, xPos);
                            }
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
                            if(i == 1) bar.Opacity = 0.7;
                            if (i == 2) bar.Opacity = 0.4;
                            if (i == 3) bar.Opacity = 0.3;
                            if (i == 4) bar.Opacity = 0.1;
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
                        if (!posiciones.ContainsKey(partyNameLabel.Text))
                        {
                            posiciones.Add(p.Value.Nombre, xPos);
                        }
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
            if (graficoComparativo)
            {
                Grid legend = new Grid();
                //Tantas filas como elecciones haya.
                for(int l = 0; l < totalElecciones; l++)
                {
                    legend.RowDefinitions.Add(new RowDefinition());
                }
                //Dos columnas
                legend.ColumnDefinitions.Add(new ColumnDefinition());
                legend.ColumnDefinitions.Add(new ColumnDefinition());

                legend.Width = lienzo.ActualWidth / 4;
                legend.Height = lienzo.ActualHeight / 3;
                legend.Background = Brushes.AliceBlue;

                Canvas.SetTop(legend, 0);
                Canvas.SetRight(legend, 0);

                lienzo.Children.Add(legend);

                int opacity = 1;
                for (int k = 0; k < totalElecciones; k++)
                {
                    Rectangle r = new Rectangle();
                    r.Fill = Brushes.Gray;
                    r.Opacity = opacity;
                    opacity -= (int)0.3;
                    r.Width = 30;
                    r.Height = 6;
                    r.HorizontalAlignment = HorizontalAlignment.Center;

                    Grid.SetRow(r, k);
                    Grid.SetColumn(r, 0);

                    legend.Children.Add(r);

                    //Fechas
                    TextBlock tb = new TextBlock();
                    tb.Text = eleccionesSeleccionadas[k].FechaElecciones;
                    tb.Foreground = Brushes.Black; 
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.Margin = new Thickness(0,20,0,0);
                    Grid.SetRow(tb, k);
                    Grid.SetColumn(tb, 1);

                    legend.Children.Add(tb);
                    
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
            c.seleccionEliminada -= OnDataNotSelectedAnymore;
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
            else if(this.graficandoPactometro)
            {
                graphPactometro(this.eleccionesAGraficar[0],max);
            }
            else
            {
                if (eleccionesAGraficar != null)
                {
                    this.marcasDibujadas = false;
                    GraphBarras(eleccionesAGraficar, max);
                }

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
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace || 
                     e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
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
                    Y1 = maxHeight - (i / (double)maxVotes * maxHeight)+20,
                    Y2 = maxHeight - (i / (double)maxVotes * maxHeight)+20,
                    Stroke = Brushes.Red
                };

                lienzo.Children.Add(mark);

                TextBlock markLabel = new TextBlock
                {
                    Text = i.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(-25, maxHeight - (i / (double)maxVotes * maxHeight) + 10, 0, 0),
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


        private void OnDataNotSelectedAnymore(object? sender, EventArgs e)
        {
            limpiaLienzo();
        }

        private void genPact_Click(object sender, RoutedEventArgs e)
        {
            if(this.eleccionesAGraficar.Count == 0)
            {
                MessageBox.Show("Por favor, seleccione una elección.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                this.graficandoPactometro = true;
                limpiaLienzo();
                Eleccion seleccion = this.eleccionesAGraficar[0];
                tituloGrafica.Text = seleccion.Nombre;
                lienzo.Background = Brushes.Beige;
                graphPactometro(seleccion,false);
            }
        }

        private void graphPactometro(Eleccion seleccion,Boolean isMax)
        {

            double width = isMax ? lienzo.Width : lienzo.ActualWidth;
            Line line = new()
            {
                X1 = 0,
                X2 = width,
                Y1 = seleccion.Mayoria,
                Y2 = seleccion.Mayoria,
                Stroke = Brushes.Black,  // Añadí Stroke para especificar el color de la línea
                StrokeThickness = 2
            };
            lienzo.Children.Add(line);
        }
    }

}
