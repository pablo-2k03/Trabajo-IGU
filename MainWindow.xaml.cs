using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        //Buffers para pactometro.
        private List<Partido> partidos;
        private List<Partido> partidosNoCoalicion;

        private List<Rectangle> rectangulosCoalicion = new();
        private List<Rectangle> rectangulosNoCoalicion = new();

        private List<TextBlock> textBlocksCoalicion = new();
        private List<TextBlock> textBlocksNoCoalicion = new();

        private TextBlock tb2Coalicion;
        private TextBlock tb3NoCoalicion;

        //Condiciones que comprueban diferentes casos del programa.
        private Boolean graficoComparativo = false;
        private Boolean graficandoPactometro = false;
        private Boolean marcasDibujadas = false;
        private Boolean lineaDibujada = false;
        private Boolean isMaximized = false;


        private double YMAXCoalicion;
        private double YMAXNoCoalicion;

        //Elección que se está graficando en pactometro.
        private Eleccion eleccionPactometro;



        public MainWindow()
        {
            InitializeComponent();

            //Instanciación de variables.
            eleccionesAGraficar = new List<Eleccion>();
            eleccionesACoomparar = new List<Eleccion>();
            partidos = new List<Partido>();
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
            if (c == null)
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
            this.lineaDibujada = false;

            //Limpiamos el lienzo por si habia datos.
            lienzo.Children.Clear();

            //Añadimos las elecciones a la lista de eleccionesAGraficar
            foreach (var i in e.elecciones)
            {
                eleccionesAGraficar.Add(i);
            }

            //Le ponemos el titulo de la primera eleccion q haya en caso de q haya mas de 1.
            tituloGrafica.Text = "Graficando BARRAS: " + e.elecciones[0].Nombre;
            lienzo.Background = Brushes.Beige;

            //Graficamos.       
            GraphBarras(e.elecciones);

        }
        private void GraphBarras(List<Eleccion> eleccionesSeleccionadas, Boolean isMax = false)
        {
            double maxHeight = lienzo.ActualHeight - 40; // La altura maxima de las barras es la del lienzo - el margen que le dejamos al nombre.
            double barWidth = 20; // Ancho de las barras
            double xPos = 20;
            double espacioEntreBarras = 0;
            Dictionary<string, Partido> nombresElectorales = new Dictionary<string, Partido>();
            Dictionary<string, double> posiciones = new Dictionary<string, double>();
            Boolean tieneMasElecciones = false;
            int totalElecciones = eleccionesSeleccionadas.Count;
            int i = 0;
            int totalPartidos = 0;
            int maxVotes = 0;
            List<string> registro = new();

            //Añadimos un tooltip por si el usuario no sabe comparar graficas.
            if (!graficoComparativo && eleccionesSeleccionadas.Count > 0)
            {
                ToolTip infoComparar = new ToolTip();

                TextBlock contentTextBlock = new TextBlock
                {
                    Text = "Para comparar dos gráficas, primero selecciónelas en la ventana secundaria y posteriormente compare en el menú Elecciones --> Comparar gráficas."
                };

                infoComparar.Content = contentTextBlock;

                //Creamos un grid  donde le vamos a añadir la elipse y el question mark.
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

                //Para que el grid detecte el hover, para cuando se use el tooltip.
                ToolTipService.SetIsEnabled(grid, true);
            }

            //Contamos el numero total de partidos distintos que va a ver en las elecciones.
            foreach (Eleccion ele in eleccionesSeleccionadas)
            {
                foreach (var ppp in ele.Partidos)
                {
                    if (!registro.Contains(ppp.Nombre))
                    {
                        registro.Add(ppp.Nombre);
                        totalPartidos++;
                    }
                    //Pillamos el numero maximo de votos.
                    if (ppp.Votos > maxVotes)
                    {
                        maxVotes = ppp.Votos;
                    }
                }
            }

            espacioEntreBarras = (lienzo.ActualWidth - barWidth) / totalPartidos - 1;


            foreach (var eleccion in eleccionesSeleccionadas)
            {


                List<Partido> partidos = new List<Partido>();
                partidos = eleccion.Partidos;

                //Comprobamos si estamos en pantalla completa o no.
                int intervalo = isMax ? 5 : 20;

                if (!this.marcasDibujadas)
                {
                    dibujarMarcas(maxVotes, maxHeight, intervalo);
                    this.marcasDibujadas = true;
                }


                foreach (var p in partidos)
                {
                    double barHeight = (p.Votos / (double)maxVotes) * maxHeight;
                    Partido partido = p;

                    Color partidoColor;

                    //Comprobamos si la propiedad Color del partido tiene asignado algo o es nulo, si lo tiene se lo asignamos y sino dejamos el azul por defecto.

                    if (partido.Color.ToString() != "Color [Empty]")
                    {
                        partidoColor = System.Windows.Media.Color.FromArgb(partido.Color.A, partido.Color.R, partido.Color.G, partido.Color.B);
                    }
                    //Comprobamos si el nombre en la segunda iteracion ya está registrado.
                    if (nombresElectorales.ContainsKey(p.Nombre))
                    {

                        //Si lo está, obtenemos el color del registro con su posicion, si lo est.a
                        Partido p1 = nombresElectorales[p.Nombre];
                        partidoColor = System.Windows.Media.Color.FromArgb(p1.Color.A, p1.Color.R, p1.Color.G, p1.Color.B);

                        if (posiciones.ContainsKey(p.Nombre))
                        {
                            xPos = posiciones[p.Nombre] + barWidth;

                            //Si estamos en >2 elecciones, a las nuevas barras se le desplaza a la derecha.
                            if (i > 1)
                            {
                                xPos = posiciones[p.Nombre] + 2 * barWidth;
                            }
                        }

                    }
                    else
                    {
                        //Si no está en la lista de partidos, significa que es un partido que solo ha participado en la n-esima elección.
                        if (tieneMasElecciones && i >= 1)
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

                            //Si la xPos que nos tocaria ya está ocupada, buscamos la ultima posicion y le añadimos el espacio.
                            if (posiciones.Values.Contains(xPos))
                            {
                                double masgrande = 0;
                                //Buscamos la ultima posicion
                                foreach (var posicion in posiciones)
                                {
                                    if (posicion.Value > masgrande)
                                    {
                                        masgrande = posicion.Value;
                                    }
                                }
                                xPos = masgrande + espacioEntreBarras;
                                posiciones.Add(p.Nombre, xPos);
                            }
                        }

                    }

                    Rectangle bar = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = new SolidColorBrush(partidoColor)
                    };

                    if (tieneMasElecciones)
                    {

                        if (nombresElectorales.ContainsKey(p.Nombre))
                        {

                            if (i == 1) bar.Opacity = 0.7; //2 elecciones
                            if (i == 2) bar.Opacity = 0.4; // 3 elecciones
                            if (i == 3) bar.Opacity = 0.3; // ...
                            if (i == 4) bar.Opacity = 0.1;
                        }
                    }

                    // Añadimos un toolTip para ver el numero de votos que tiene un partido cuando el raton pasa por encima.
                    bar.ToolTip = new ToolTip { Content = $"{p.Votos} escaños" };

                    //Añadimos el evento al gestor del primer elemento que pasemos como parametro.
                    AddEventHandler(bar, bar, p, eleccion);
                    // Position the bar and add it to the canvas
                    Canvas.SetLeft(bar, xPos);
                    Canvas.SetBottom(bar, 20);
                    lienzo.Children.Add(bar);



                    // Añadimos el nombre del partido en la parte inferior.
                    TextBlock partyNameLabel = new TextBlock
                    {
                        Text = p.Nombre,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    //También se lo añadimos al nombre, por si se da el caso de que la barra sea tan pequeña q casi ni se vea.
                    AddEventHandler(partyNameLabel, bar, p, eleccion);

                    //Calculamos el ancho del texto utilizando la clase FormattedText.
                    FormattedText formattedText = new FormattedText(
                        p.Nombre,
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
                        nombresElectorales.Add(p.Nombre, p);
                        if (!posiciones.ContainsKey(partyNameLabel.Text))
                        {
                            posiciones.Add(p.Nombre, xPos);
                        }
                    }

                    //Añadimos espaciado entre barras.
                    xPos += espacioEntreBarras;
                    if (tieneMasElecciones) { xPos += i * barWidth - 20; }
                }
                //Reestablecemos la xPos y true el booleano de control.
                if (graficoComparativo)
                {
                    xPos = 20 + barWidth;
                    tieneMasElecciones = true;
                    i++;
                }
            }

            //Una vez acabado de graficar, si es comparativo añadimos la leyenda.
            if (graficoComparativo)
            {

                //Creamos un grid para añadirle posteriormente los rectangulos con las fechas y las  distintas opacidades.
                Grid legend = new Grid();

                for (int l = 0; l < totalElecciones; l++)
                {
                    legend.RowDefinitions.Add(new RowDefinition());
                }

                legend.ColumnDefinitions.Add(new ColumnDefinition());
                legend.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // La segunda columna se expandirá

                legend.Width = lienzo.ActualWidth / 4;
                legend.Height = lienzo.ActualHeight / 3;
                legend.Background = Brushes.AliceBlue;

                Canvas.SetTop(legend, 0);
                Canvas.SetRight(legend, 0);

                lienzo.Children.Add(legend);


                for (int k = 0; k < totalElecciones; k++)
                {

                    //Ajustamos la opacidad de acorde al numero de elecciones representadas a graficar.
                    double opacity = 1.0 - k * 0.3;

                    Rectangle r = new Rectangle();
                    r.Fill = Brushes.Gray;
                    r.Opacity = opacity;
                    r.Width = 30;
                    r.VerticalAlignment = VerticalAlignment.Center;
                    r.Height = 6;
                    r.HorizontalAlignment = HorizontalAlignment.Center;

                    Grid.SetRow(r, k);
                    Grid.SetColumn(r, 0);

                    legend.Children.Add(r);

                    TextBlock tb = new TextBlock();
                    tb.Text = eleccionesSeleccionadas[k].FechaElecciones;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.Foreground = Brushes.Black;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(tb, k);
                    Grid.SetColumn(tb, 1);

                    legend.Children.Add(tb);
                }
            }
        }
        private void AddEventHandler(FrameworkElement e, Rectangle barra, Partido p, Eleccion el)
        {
            /*
             Manejadora asociada al doble click del raton sobre una barra o sobre el nombre,
             lo cual le permite al usuario cambiar el color de la barra y guardar dicho cambio.
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
                        foreach (var i in modeloUnico.ResultadosElectorales[indiceEleccion].Partidos)
                        {
                            if (i.Nombre == p.Nombre)
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
            this.eleccionesAGraficar.Clear();
            this.eleccionPactometro = null;
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
                this.isMaximized = true;
                UpdateCanvasSize(isMaximized);
            }
            else
            {
                this.isMaximized = false;
                UpdateCanvasSize();
            }
        }
        private void UpdateCanvasSize(Boolean max = false)
        {
            double margin = 80;
            if (!this.graficandoPactometro)
                lienzo.Width = ActualWidth - 2 * margin;
            lienzo.Children.Clear();


            if (this.graficoComparativo)
            {
                lienzo.Background = Brushes.Beige;
                this.marcasDibujadas = false;
                GraphBarras(eleccionesACoomparar, max);
            }
            else if (this.graficandoPactometro)
            {
                string onlyWinner = checkOneWinner(this.eleccionPactometro);
                this.lineaDibujada = false;
                if (onlyWinner == string.Empty)
                {
                    if (max)
                    {
                        resizePactometro(max);
                    }
                    else
                    {
                        resizePactometro();
                    }
                }
                else
                {
                    graphPactometro(this.eleccionPactometro, onlyWinner, max);
                }
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
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
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
                    this.marcasDibujadas = false;
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
        private void dibujarMarcas(int maxVotes, double maxHeight, int intervalo)
        {
            // Draw marks at the left going from 10 to the max number of votes in increments of 10
            for (int i = 10; i <= maxVotes; i += intervalo)
            {

                Line mark = new Line
                {
                    X1 = 0,
                    X2 = 5,
                    Y1 = maxHeight - (i / (double)maxVotes * maxHeight) + 20,
                    Y2 = maxHeight - (i / (double)maxVotes * maxHeight) + 20,
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
            tituloGrafica.Text = "Graficando COMPARATIVO: " + elecciones[0].Nombre;

            lienzo.Background = Brushes.Beige;
            GraphBarras(elecciones, false);

        }
        private void OnDataSelectedToCompare(object? sender, CustomEventArgs c)
        {

            this.graficoComparativo = true;
            this.graficandoPactometro = false;
            this.eleccionesACoomparar.Clear();
            //Guardamos.
            foreach (Eleccion eleccion in c.elecciones)
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
            if (this.eleccionesAGraficar.Count == 0)
            {
                MessageBox.Show("Por favor, seleccione una elección.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                this.graficandoPactometro = true;
                this.graficoComparativo = false;
                limpiaLienzo();
                eleccionPactometro = this.eleccionesAGraficar[0];
                tituloGrafica.Text = eleccionPactometro.Nombre;
                lienzo.Background = Brushes.Beige;

                string onlyWinner = checkOneWinner(eleccionPactometro);

                graphPactometro(eleccionPactometro, onlyWinner);
            }
        }
        private void graphPactometro(Eleccion seleccion, string ganadorEnSolitario = "", Boolean isMax = false)
        {
            tituloGrafica.Text = "Graficando PACTOMETRO: " + seleccion.Nombre;
            if (ganadorEnSolitario != string.Empty)
            {
                haGanadoEnSolitario(ganadorEnSolitario, isMax);
            }
            else
            {
                Coalicion coal = Utils.CoalicionSingleton.GetInstance(seleccion);
                coal.hayCoalicion += OnCoalicion;
                coal.ShowDialog();
            }

        }
        private string checkOneWinner(Eleccion seleccion)
        {
            foreach (var p in seleccion.Partidos)
            {
                if (p.Votos >= seleccion.Mayoria)
                {
                    return p.Nombre;
                }
            }
            return string.Empty;
        }
        private void OnCoalicion(object? sender, CustomEventArgsPartidos c)
        {
            double width = this.isMaximized ? lienzo.Width : lienzo.ActualWidth;
            partidos = new List<Partido>();

            foreach (var i in c.partidosParaCoalicion)
            {
                partidos.Add(i);
            }

            partidos = partidos.OrderBy(partido => partido.Votos).ToList();


            setPartidosCoalicion(partidos);

            List<string> partidosRestantes = new List<string>();

            /*
             * Con una sentencia LINQ seleccionamos los nombres de los partidos que no están en la lista de partidos
             * 
             * AddRange(toma el rango de partidos que vamos a añadir)
             * La clausula where indica la condición de busqueda, en este caso, 
             * cualquier partido que tenga un nombre distinto a cualquier partido de la coleccion "partidos"
             * 
             * Por ultimo, la clausa select selecciona para guardar el valor deseado, en este caso partido.Value.Nombre.
             */

            partidosRestantes.AddRange(eleccionPactometro.Partidos
            .Where(partido => !partidos.Any(p => p.Nombre == partido.Nombre))
            .Select(partido => partido.Nombre));


            //Ahora buscamos en la lista inicial todos los partidos que estan ahora en restantes.
            partidosNoCoalicion = new();
            foreach (var i in partidosRestantes)
            {
                foreach (var p in eleccionPactometro.Partidos)
                {
                    int index = eleccionPactometro.Partidos.IndexOf(p);
                    if (p.Nombre.Contains(i))
                    {
                        partidosNoCoalicion.Add(eleccionPactometro.Partidos[index]);
                    }
                }
            }
            //Se ordenan de forma creciente.
            partidosNoCoalicion = partidosNoCoalicion.OrderBy(partido => partido.Votos).ToList();
            setPartidosNoCoalicion(partidosNoCoalicion);

            if (!this.lineaDibujada)
            {
                dibujarLineaMayoria(width);
            }

        }
        private void setPartidosCoalicion(List<Partido> partidos)
        {
            int suma = 0;
            double xPos = 20;
            double barWidth = 100;
            int margin = 20;
            int maxVotes = eleccionPactometro.Partidos.Sum(partido => partido.Votos);
            double maxHeight = lienzo.ActualHeight - margin;

            foreach (var partido in partidos)
            {

                double barHeight = (partido.Votos / (double)maxVotes) * maxHeight;

                suma += partido.Votos;
                Rectangle r2 = new()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(partido.Color.A, partido.Color.R, partido.Color.G, partido.Color.B)),
                    Width = barWidth,
                    Height = barHeight,
                    Tag = "coalicion",
                    Name = partido.Nombre
                };


                Canvas.SetLeft(r2, ((lienzo.ActualWidth - (2 * barWidth)) / 3));
                Canvas.SetBottom(r2, xPos);
                lienzo.Children.Add(r2);
                TextBlock nombrePartido = new();
                if (partido.Votos >= 3)
                {
                    nombrePartido.Text = partido.Nombre + "-" + partido.Votos;
                    Canvas.SetBottom(nombrePartido, xPos + barHeight);
                    Canvas.SetLeft(nombrePartido, ((lienzo.ActualWidth - (2 * barWidth)) / 3) + barWidth);
                    lienzo.Children.Add(nombrePartido);
                }
                else
                {
                    nombrePartido.Text = string.Empty;
                }
                AddEventHandlerChangePact(r2, r2, nombrePartido, partido.Votos);
                rectangulosCoalicion.Add(r2);
                textBlocksCoalicion.Add(nombrePartido);
                xPos += barHeight;
            }
            YMAXCoalicion = xPos;
            tb2Coalicion = new()
            {
                Text = suma.ToString(),
            };
            Canvas.SetLeft(tb2Coalicion, ((lienzo.ActualWidth - (2 * barWidth)) / 3));
            Canvas.SetBottom(tb2Coalicion, 0);
            lienzo.Children.Add(tb2Coalicion);
        }
        private void setPartidosNoCoalicion(List<Partido> partidos)
        {
            int suma = 0;
            double xPos = 20;
            double barWidth = 100;
            int margin = 20;
            double maxHeight = lienzo.ActualHeight - margin;
            int maxVotes = eleccionPactometro.Partidos.Sum(partido => partido.Votos);

            foreach (var partido in partidos)
            {
                suma += partido.Votos;

                double barHeight = partido.Votos / (double)maxVotes * maxHeight;

                Rectangle r2 = new()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(partido.Color.A, partido.Color.R, partido.Color.G, partido.Color.B)),
                    Width = barWidth,
                    Height = barHeight,
                    Tag = "NoCoalicion",
                    Name = partido.Nombre
                };


                Canvas.SetLeft(r2, ((lienzo.ActualWidth - (2 * barWidth)) / 3) * 2);
                Canvas.SetBottom(r2, xPos);
                lienzo.Children.Add(r2);
                TextBlock nombrePartido = new();
                if (partido.Votos >= 3)
                {
                    nombrePartido.Text = partido.Nombre + "-" + partido.Votos;
                    Canvas.SetBottom(nombrePartido, xPos + barHeight);
                    Canvas.SetLeft(nombrePartido, ((lienzo.ActualWidth - (2 * barWidth)) / 3 * 2) + barWidth);
                    lienzo.Children.Add(nombrePartido);
                }
                else
                {
                    nombrePartido.Text = string.Empty;
                }
                AddEventHandlerChangePact(r2, r2, nombrePartido,partido.Votos);

                rectangulosNoCoalicion.Add(r2);
                textBlocksNoCoalicion.Add(nombrePartido);

                xPos += barHeight;
            }
            YMAXNoCoalicion = xPos;
            tb3NoCoalicion = new()
            {
                Text = suma.ToString(),
            };
            Canvas.SetLeft(tb3NoCoalicion, ((lienzo.ActualWidth - (2 * barWidth)) / 3) * 2);
            Canvas.SetBottom(tb3NoCoalicion, 0);
            lienzo.Children.Add(tb3NoCoalicion);
        }
        private void resizePactometro(bool isMax = false)
        {
            double width = isMax ? lienzo.Width : lienzo.ActualWidth;


            List<Partido> partidosCoal = new List<Partido>();
            foreach (var i in partidos)
            {
                partidosCoal.Add(i);
            }
            partidosCoal = partidosCoal.OrderBy(partido => partido.Votos).ToList();
            setPartidosCoalicion(partidosCoal);

            List<string> partidosRestantes = new List<string>();

            /*
             * Con una sentencia LINQ seleccionamos los nombres de los partidos que no están en la lista de partidos
             * 
             * AddRange(toma el rango de partidos que vamos a añadir)
             * La clausula where indica la condición de busqueda, en este caso, 
             * cualquier partido que tenga un nombre distinto a cualquier partido de la coleccion "partidos"
             * 
             * Por ultimo, la clausa select selecciona para guardar el valor deseado, en este caso partido.Value.Nombre.
             */

            partidosRestantes.AddRange(eleccionPactometro.Partidos
            .Where(partido => !partidos.Any(p => p.Nombre == partido.Nombre))
            .Select(partido => partido.Nombre));


            //Ahora buscamos en la lista inicial todos los partidos que estan ahora en restantes.
            List<Partido> partidosNoCoalicion = new();
            foreach (var i in partidosRestantes)
            {
                foreach (var p in eleccionPactometro.Partidos)
                {
                    int index = eleccionPactometro.Partidos.IndexOf(p);
                    if (p.Nombre.Contains(i))
                    {
                        partidosNoCoalicion.Add(eleccionPactometro.Partidos[index]);
                    }
                }
            }
            partidosNoCoalicion = partidosNoCoalicion.OrderBy(partido => partido.Votos).ToList();
            setPartidosNoCoalicion(partidosNoCoalicion);

            if (!this.lineaDibujada)
            {
                dibujarLineaMayoria(width);
            }
        }
        private void dibujarLineaMayoria(double width)
        {
            int maxVotes = eleccionPactometro.Partidos.Sum(partido => partido.Votos);

            double majorityPosition = lienzo.ActualHeight - ((eleccionPactometro.Mayoria / (double)maxVotes) * lienzo.ActualHeight);
            Line line = new()
            {
                X1 = 0,
                X2 = width,
                Y1 = majorityPosition,
                Y2 = majorityPosition,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            lienzo.Children.Add(line);
        }
        private void haGanadoEnSolitario(string ganadorEnSolitario, Boolean isMax)
        {
            double width = isMax ? lienzo.Width : lienzo.ActualWidth;
            double barWidth = 60;
            int index = -1, marginInferior = 20;
            int maxVotes = eleccionPactometro.Partidos.Sum(partido => partido.Votos);
            double maxHeight = lienzo.ActualHeight - marginInferior;
            double xPos = 20;
            foreach (var i in eleccionPactometro.Partidos)
            {
                if (i.Nombre.Equals(ganadorEnSolitario))
                {
                    index = eleccionPactometro.Partidos.IndexOf(i);
                    break;
                }
            }
            Partido p = eleccionPactometro.Partidos[index];
            double barHeight = (p.Votos / (double)maxVotes) * maxHeight;
            Rectangle r = new()
            {
                Fill = new SolidColorBrush(Color.FromArgb(p.Color.A, p.Color.R, p.Color.G, p.Color.B)),
                Width = barWidth,
                Height = barHeight,
                
            };

            Canvas.SetLeft(r, (lienzo.ActualWidth - 2 * barWidth) / 3);
            Canvas.SetBottom(r, 20);
            lienzo.Children.Add(r);
            TextBlock tb = new()
            {
                Text = p.Votos.ToString(),
            };
            Canvas.SetLeft(tb, (lienzo.ActualWidth - 2 * barWidth) / 3);
            Canvas.SetBottom(tb, 0);
            lienzo.Children.Add(tb);

            if (p.Votos >= 3)
            {
                TextBlock nombrePartidoGanador = new()
                {
                    Text = ganadorEnSolitario + "-" + p.Votos
                };
                Canvas.SetBottom(nombrePartidoGanador, xPos + barHeight);
                Canvas.SetLeft(nombrePartidoGanador, ((lienzo.ActualWidth - 2 * barWidth) / 3) + barWidth);
                lienzo.Children.Add(nombrePartidoGanador);
            }

            xPos = 20;
            int suma = 0;
            foreach (var partido in eleccionPactometro.Partidos)
            {
                if (partido.Nombre != ganadorEnSolitario)
                {
                    suma += partido.Votos;
                    barHeight = (partido.Votos / (double)maxVotes) * maxHeight;
                    Rectangle r2 = new()
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(partido.Color.A, partido.Color.R, partido.Color.G, partido.Color.B)),
                        Width = barWidth,
                        Height = barHeight
                    };

                    Canvas.SetLeft(r2, ((lienzo.ActualWidth - 2 * barWidth) / 3) * 2);
                    Canvas.SetBottom(r2, xPos);
                    lienzo.Children.Add(r2);

                    if (partido.Votos >= 3)
                    {
                        TextBlock nombrePartido = new()
                        {
                            Text = partido.Nombre + "-" + partido.Votos
                        };
                        Canvas.SetBottom(nombrePartido, xPos + barHeight);
                        Canvas.SetLeft(nombrePartido, ((lienzo.ActualWidth - 2 * barWidth) / 3 * 2) + barWidth);
                        lienzo.Children.Add(nombrePartido);
                    }
                    xPos += barHeight;
                }
            }
            TextBlock tb2 = new()
            {
                Text = suma.ToString(),
            };
            Canvas.SetLeft(tb2, ((lienzo.ActualWidth - 2 * barWidth) / 3) * 2);
            Canvas.SetBottom(tb2, 0);
            lienzo.Children.Add(tb2);
            if (!this.lineaDibujada)
            {
                dibujarLineaMayoria(width);
            }
        }

        private void AddEventHandlerChangePact(FrameworkElement e, Rectangle barra, TextBlock tb,int votos)
        {
            double barWidth = 100;
            e.MouseLeftButtonDown += (sender, x) =>
            {
                //Con doble click, mostramos el modal de elección del color.
                if (x.ClickCount == 1)
                {
                    if(barra.Tag.ToString() == "coalicion")
                    {
                        int index=0;
                        for(int i = 0; i < rectangulosCoalicion.Count; i++)
                        {
                            if(barra == rectangulosCoalicion[i])
                            {
                                index = i;
                                break;
                            }
                        }

                        //Te cargas todos los superiores
                        for(int i = index; i < rectangulosCoalicion.Count; i++)
                        {
                            lienzo.Children.Remove(rectangulosCoalicion[i]);
                            lienzo.Children.Remove(textBlocksCoalicion[i]);
                            YMAXCoalicion -= rectangulosCoalicion[i].Height;
                        }
                        //Volvemos a graficar.
                        for(int i = index +1; i< rectangulosCoalicion.Count; i++)
                        {
                            Canvas.SetLeft(rectangulosCoalicion[i], ((lienzo.ActualWidth - (2 * barWidth)) / 3));
                            Canvas.SetBottom(rectangulosCoalicion[i], YMAXCoalicion);
                            lienzo.Children.Add(rectangulosCoalicion[i]);

                            YMAXCoalicion += rectangulosCoalicion[i].Height;

                            Canvas.SetLeft(textBlocksCoalicion[i], ((lienzo.ActualWidth - (2 * barWidth)) / 3)+barWidth);
                            Canvas.SetBottom(textBlocksCoalicion[i], YMAXCoalicion);
                            lienzo.Children.Add(textBlocksCoalicion[i]);
                        }
                        //Quitamos de una coleccion y lo metemos en la otra.
                        rectangulosCoalicion.Remove(barra);
                        textBlocksCoalicion.Remove(tb);
                        barra.Tag = "noCoalicion";
                        rectangulosNoCoalicion.Add(barra);
                        textBlocksNoCoalicion.Add(tb);

                        int resto = int.Parse(tb2Coalicion.Text) - votos;
                        int suma = int.Parse(tb3NoCoalicion.Text) + votos;

                        //Graficamos en canvas
                        Canvas.SetLeft(barra, ((lienzo.ActualWidth - (2 * barWidth)) / 3)*2);
                        Canvas.SetBottom(barra, YMAXNoCoalicion);
                        lienzo.Children.Add(barra);

                        YMAXNoCoalicion += barra.Height;

                        Canvas.SetLeft(tb, (((lienzo.ActualWidth - (2 * barWidth)) / 3) * 2) + barWidth);
                        Canvas.SetBottom(tb, YMAXNoCoalicion);
                        lienzo.Children.Add(tb);

                        tb2Coalicion.Text = resto.ToString();
                        tb3NoCoalicion.Text = suma.ToString();

                    }
                    else
                    {
                        int index = 0;
                        for (int i = 0; i < rectangulosNoCoalicion.Count; i++)
                        {
                            if (barra == rectangulosNoCoalicion[i])
                            {
                                index = i;
                                break;
                            }
                        }

                        //Te cargas todos los superiores
                        for (int i = index; i < rectangulosNoCoalicion.Count; i++)
                        {
                            lienzo.Children.Remove(rectangulosNoCoalicion[i]);
                            lienzo.Children.Remove(textBlocksNoCoalicion[i]);
                            YMAXNoCoalicion -= rectangulosNoCoalicion[i].Height;
                        }
                        //Volvemos a graficar.
                        for (int i = index + 1; i < rectangulosNoCoalicion.Count; i++)
                        {
                            Canvas.SetLeft(rectangulosNoCoalicion[i], (((lienzo.ActualWidth - (2 * barWidth)) / 3)*2));
                            Canvas.SetBottom(rectangulosNoCoalicion[i], YMAXNoCoalicion);
                            lienzo.Children.Add(rectangulosNoCoalicion[i]);

                            YMAXNoCoalicion += rectangulosNoCoalicion[i].Height;

                            Canvas.SetLeft(textBlocksNoCoalicion[i], (((lienzo.ActualWidth - (2 * barWidth)) / 3)*2) + barWidth);
                            Canvas.SetBottom(textBlocksNoCoalicion[i], YMAXNoCoalicion);
                            lienzo.Children.Add(textBlocksNoCoalicion[i]);
                        }
                        //Quitamos de una coleccion y lo metemos en la otra.
                        rectangulosNoCoalicion.Remove(barra);
                        textBlocksNoCoalicion.Remove(tb);
                        barra.Tag = "coalicion";
                        rectangulosCoalicion.Add(barra);
                        textBlocksCoalicion.Add(tb);

                        int resto = int.Parse(tb3NoCoalicion.Text) - votos;
                        int suma = int.Parse(tb2Coalicion.Text) + votos;
                        

                        //Graficamos en canvas
                        Canvas.SetLeft(barra, ((lienzo.ActualWidth - (2 * barWidth)) / 3));
                        Canvas.SetBottom(barra, YMAXCoalicion);
                        lienzo.Children.Add(barra);

                        YMAXCoalicion += barra.Height;

                        Canvas.SetLeft(tb, ((lienzo.ActualWidth - (2 * barWidth)) / 3) + barWidth) ;
                        Canvas.SetBottom(tb, YMAXCoalicion);
                        lienzo.Children.Add(tb);

                        tb2Coalicion.Text = suma.ToString();
                        tb3NoCoalicion.Text = resto.ToString();
                    }
                }
            };
        }


    }
}