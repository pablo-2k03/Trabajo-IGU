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
        public event EventHandler<CustomEventArgsMain> DataSelected; 

        //Delegado encargado de eliminar los datos (el parametro eventArgs acepta valores nulos)
        public event EventHandler<EventArgs> removeData;

        //Manejadora del evento y evento para gestionar cuando esta ventana ha sido cerrada.
        public event EventHandler<EventArgs> DatosGraficasClosed;


        //Instancia del modelo unico que será el almacen de datos.
        private ModeloDatos modeloUnico;

        
        public DatosGraficas()
        {
            InitializeComponent();
            //Generar instancia unica del modelo.
            modeloUnico = Utils.DataModelSingleton.GetInstance();

            //Eventos de la propia ventana
            Closing += DatosGraficas_Closing;

        }

        //Cargar ventana de añadir datos.
        private void AddElectionData(object sender, RoutedEventArgs e)
        {

            AddData add = Utils.AddDataWindowSingleton.GetInstance(modeloUnico);
            add.DataCreated += OnDataCreated;
            add.ShowDialog();

        }

        //Cargar datos de prueba.
        private void LoadDataTests(object sender, RoutedEventArgs e)
        {
            modeloUnico.LoadDataTests();

            resultadosLV.ItemsSource = modeloUnico.ResultadosElectorales;    
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

                        UpdateData upd = Utils.UpdateDataSingleton.GetInstance(this);
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

                        //Lanzamos el evento removeData para que la MainWindow limpie el lienzo.
                        removeData(this,e);
                    };

                    // Añadimos los items del menu al contexto del menu.
                    contextMenu.Items.Add(modifyMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    // Añadimos el contexto a la propiedad listview.Context de los resultados.
                    resultadosLV.ContextMenu = contextMenu;
                }
                Eleccion selectedElection = (Eleccion)resultadosLV.SelectedItem;

                Dictionary<string, Partido> partyData = selectedElection.Partidos;

                // Gracias al metodo Select de los diccionarios podemos realizar un mapeo de los datos de forma muy sencilla.
                // Lo bindeamos con el xaml y asociamos el Key con el nombre del partido y el Value con los escaños generando una lista.
                var partyDataCollection = partyData.Select(pair => new { Key = pair.Key, Value = pair.Value.Votos }).ToList();

                resultadosLV2.ItemsSource = partyDataCollection;

                //Enviamos los datos al MainWindow para su visualizacion.
                CustomEventArgsMain args = new(selectedElection.Partidos,selectedElection.Nombre); 
                DataSelected(this, args);

            }
        }

        //Evento para controlar cuando se está cerrando la ventana secundaria.
        private void DatosGraficas_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            DatosGraficasClosed(this, EventArgs.Empty);

            modeloUnico.ResultadosElectorales.Clear();
        }

        private void OnDataCreated(object? sender,EventArgs e)
        {
            resultadosLV.ItemsSource = modeloUnico.ResultadosElectorales;
        }

    }
        
}