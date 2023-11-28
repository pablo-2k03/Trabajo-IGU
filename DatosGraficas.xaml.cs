using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System;

namespace Pactometro
{
    /// <summary>
    /// Lógica de interacción para DatosGraficas.xaml
    /// </summary>
    /// 

    public partial class DatosGraficas : Window
    {

        //Manejadora del evento y evento para gestionar cuando unos datos han sido seleccionados.
        public event EventHandler<CustomEventArgs> DataSelected; 

        //Manejadora del evento y evento para gestionar cuando esta ventana ha sido cerrada.
        public event EventHandler<EventArgs> DatosGraficasClosed;

        public event EventHandler<CustomEventArgs> CompararElecciones;

        public event EventHandler<EventArgs> seleccionEliminada;

        //Instancia del modelo unico que será el almacen de datos.
        private ModeloDatos modeloUnico;

        //Lista de elecciones que se va a pasar como parametro para graficar el comparativo.
        private List<Eleccion> eleccionesACoomparar;

        //Lista de elecciones que se va a passar como parametro para graficar los datos de una eleccion.
        private List<Eleccion> eleccionesAGraficar;
        public DatosGraficas()
        {
            InitializeComponent();
            //Generar instancia unica del modelo.
            modeloUnico = Utils.DataModelSingleton.GetInstance();
            eleccionesACoomparar = new List<Eleccion>();
            eleccionesAGraficar = new List<Eleccion>();
            resultadosLV.ItemsSource = modeloUnico.ResultadosElectorales;

            //Eventos de la propia ventana
            Closing += DatosGraficas_Closing;

        }

        //Cargar ventana de añadir datos.
        private void AddElectionData(object sender, RoutedEventArgs e)
        {

            AddData add = Utils.AddDataWindowSingleton.GetInstance();
            add.DataCreated += OnDataCreated;
            add.ShowDialog();

        }

        //Cargar datos de prueba.
        private void LoadDataTests(object sender, RoutedEventArgs e)
        {
            this.eleccionesACoomparar.Clear();
            modeloUnico.LoadDataTests();
        }


        //Cuando se haga doble click sobre un elemento de la listview
        // se crea una lista con los valores de los partidos y se le 
        // añade al itemsource de la tabla de la parte inferior.
        private void ResultadosLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(resultadosLV.SelectedItem != null)
            {
                
                if(e.RightButton == MouseButtonState.Pressed)
                {
                    // Creamos un contextMenu para dejarle al usuario de forma más comoda la opción de modificar y eliminar los datos.
                    ContextMenu contextMenu = new ContextMenu();

                    // Creamos el menuItem para añadir el caso.
                    MenuItem modifyMenuItem = new MenuItem();
                    modifyMenuItem.Header = "Modificar datos electorales";
                    modifyMenuItem.Click += (modifySender, modifyEventArgs) =>
                    {
                        //Eleccion a reemplazar cuando se seleccione
                        Eleccion eleccionAReemplazar = (Eleccion)resultadosLV.SelectedItem;

                        UpdateData upd = Utils.UpdateDataSingleton.GetInstance();
                        upd.displayData(eleccionAReemplazar.Nombre, eleccionAReemplazar.FechaElecciones, 
                                        eleccionAReemplazar.Partidos,eleccionAReemplazar, modeloUnico);
                        upd.ShowDialog();
                        
                    };

                    // Para eliminar un elemento, simplemente cogemos nuestro modelo unico y lo eliminamos de la lista de resultadosElectorales
                    // Actualizamos las tablas y disparamos el evento que notifica al mainwindow que hay que borrar la grafica.

                    MenuItem deleteMenuItem = new MenuItem();
                    deleteMenuItem.Header = "Eliminar datos electorales";
                    deleteMenuItem.Click += (deleteSender, deleteEventArgs) =>
                    {
                        modeloUnico.ResultadosElectorales.Remove((Eleccion)resultadosLV.SelectedItem);

                        //La tabla de partidos se vacia.
                        resultadosLV2.ItemsSource = null;

                    };


                    //Vamos a crear otro MenuItem para comparar
                    MenuItem compare = new MenuItem();
                    compare.Header = "Comparar";
                    compare.Click += (compareSender, compareEventArgs) =>
                    {
                        
                        //Si se selecciona para comparar, no se pueden añadir datos.
                        _newData.IsEnabled = false;
                        //TODO: Cambiar UX 
                        Eleccion eleccionAComparar = (Eleccion)resultadosLV.SelectedItem;
                        eleccionesACoomparar.Add(eleccionAComparar);

                        // Cambiarmos el color de fondo solo si se hace clic en "Comparar" en el ContextMenu
                        if (compareEventArgs.OriginalSource is MenuItem)
                        {
                            ListViewItem? selectedItem = resultadosLV.ItemContainerGenerator.ContainerFromItem(resultadosLV.SelectedItem) as ListViewItem;

                            if (selectedItem != null)
                            {
                                selectedItem.Background = System.Windows.Media.Brushes.LightGreen; 
                            }
                        }
                        // Obtener la subcadena "generales" o "autonomicas" del nombre de la elección
                        string tipoEleccionElegida = GetTipoEleccion(eleccionAComparar.Nombre);
                        string comunidadElegida = string.Empty;
                        Boolean sonAutonomicas = false;

                        if (tipoEleccionElegida.Contains("autonomicas"))
                        {
                            comunidadElegida = GetComunidad(eleccionAComparar.Nombre);
                            sonAutonomicas = true;
                        }
                        foreach (Eleccion item in resultadosLV.Items)
                        {
                            if (sonAutonomicas)
                            {
                                if (GetComunidad(item.Nombre) != comunidadElegida)
                                {
                                    // Deshabilitar el elemento y ponerlo en gris
                                    ListViewItem? listViewItem = resultadosLV.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

                                    if (listViewItem != null)
                                    {
                                        listViewItem.IsEnabled = false;
                                        listViewItem.Background = System.Windows.Media.Brushes.LightGray;
                                    }
                                }
                            }
                            else
                            {
                                if (GetTipoEleccion(item.Nombre) != tipoEleccionElegida)
                                {
                                    // Deshabilitar el elemento y ponerlo en gris
                                    ListViewItem? listViewItem = resultadosLV.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

                                    if (listViewItem != null)
                                    {
                                        listViewItem.IsEnabled = false;
                                        listViewItem.Background = System.Windows.Media.Brushes.LightGray;
                                    }
                                }
                            }
                        }

                    };

                    // Añadimos los items del menu al contexto del menu.
                    contextMenu.Items.Add(modifyMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);
                    contextMenu.Items.Add(compare);

                    // Añadimos el contexto a la propiedad listview.Context de los resultados.
                    resultadosLV.ContextMenu = contextMenu;
                }
                Eleccion selectedElection = (Eleccion)resultadosLV.SelectedItem;

                Dictionary<string, Partido> partyData = selectedElection.Partidos;

                // Gracias al metodo Select de los diccionarios podemos realizar un mapeo de los datos de forma muy sencilla.
                // Lo bindeamos con el xaml y asociamos el Key con el nombre del partido y el Value con los escaños generando una lista.
                var partyDataCollection = partyData.Select(pair => new { Key = pair.Key, Value = pair.Value.Votos }).ToList();

                resultadosLV2.ItemsSource = partyDataCollection;

                eleccionesAGraficar.Add(selectedElection);
                //Enviamos los datos al MainWindow para su visualizacion.
                CustomEventArgs args = new(eleccionesAGraficar);
                DataSelected(this, args);
                eleccionesAGraficar.Clear();
            }
        }

        //Evento para controlar cuando se está cerrando la ventana secundaria.
        private void DatosGraficas_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            DatosGraficasClosed(this, EventArgs.Empty);

        }

        private void OnDataCreated(object? sender,EventArgs e)
        {
            if (modeloUnico.ResultadosElectorales.Count > 0)
            {
                resultadosLV.SelectedItem = modeloUnico.ResultadosElectorales.Last();
            }
            else
            {
                resultadosLV.SelectedItem = modeloUnico.ResultadosElectorales.First();
            }
        }

        private void RestoreBackgroundColors(ListView listView)
        {
            // Restaurar los colores de fondo de todos los elementos en el ListView
            foreach (var item in listView.Items)
            {
                ListViewItem? listViewItem = listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (listViewItem != null)
                {
                    listViewItem.Background = System.Windows.Media.Brushes.Transparent;
                    listViewItem.IsEnabled = true;
                }
            }
        }

        private string GetTipoEleccion(string nombreEleccion)
        {
            if (nombreEleccion.ToLower().Contains("generales"))
            {
                return "generales";
            }
            else if (nombreEleccion.ToLower().Contains("autonomicas"))
            {
                return "autonomicas";
            }
            else
            {
                return "otro";
            }
        }

        private void _compare_Click(object sender, RoutedEventArgs e)
        {
            if(eleccionesACoomparar.Count < 2)
            {
                MessageBox.Show("Seleccione al menos dos elecciones para comparar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                CustomEventArgs c = new(this.eleccionesACoomparar);
                CompararElecciones(this,c);
                RestoreBackgroundColors(resultadosLV);
                _newData.IsEnabled = true;
                this.eleccionesACoomparar.Clear();
            }
        }


        private string GetComunidad(string nombreEleccion)
        {
            string[] tokens = nombreEleccion.Split(" ");
            return tokens[tokens.Length-2];
        }

        private void _deleteSelection_Click(object sender, RoutedEventArgs e)
        {
            eleccionesACoomparar?.Clear();
            RestoreBackgroundColors(resultadosLV);
            _newData.IsEnabled = true;
            seleccionEliminada(this, e);
        }
    }
        
}