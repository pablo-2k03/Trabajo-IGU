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

        //Añadimos el delegado que gestiona el cambio de ventana.
        public delegate void delegadoCambioV(object sender, RoutedEventArgs a);

        //Manejadora del evento y evento para gestionar cuando unos datos han sido seleccionados.
        public delegate void DataSelectedEventHandler(object sender, CustomEventArgsMain e); 
        public event DataSelectedEventHandler DataSelected; 

        //Delegado encargado de eliminar los datos (el parametro eventArgs acepta valores nulos)
        public delegate void DataRemove(object sender, EventArgs? e);

        //Manejadora del evento y evento para gestionar cuando esta ventana ha sido cerrada.
        public delegate void DatosGraficasClosedEventHandler(object sender, EventArgs e);
        public event DatosGraficasClosedEventHandler DatosGraficasClosed;

        //Delegado encargado de cargar los datos de prueba iniciales.
        public delegate void cargaDatos();

        //Delegado encargado de registrar nuevos datos introducidos desde AddData por el usuario.
        public delegate void createNewData(object s, CustomEventArgs c);

        //Delegado encargado de actualizar los datos introducidos por teclado desde UpdateData.
        public delegate void updateData(object sender, CustomEventArgs e);

        private delegadoCambioV _v;


        //Delegado encargado de cargar datos automaticamente.
        private cargaDatos _delegadoCargaDatos;

        //Delegados encargados de añadir datos del usuario (AddData.xaml.cs ) y de modificarlos (UpdateData.xaml.cs)
        private createNewData _addData;
        private createNewData _actualizarDatos;

        //Delegado encargado de mostrar en la nueva ventana nada mas lanzarse los datos necesarios para que el usuario sepa cual son los actuales.
        private updateData _updateData;
        
        //Delegado encargado de eliminar los datos.
        private DataRemove _removeData;

        //Instancias de los modelos que nos van a servir para añadir a los eventos y delegados los gestores.
        private ModeloDatos md;
        private MainWindow _mainWindow;
        private UpdateData upd;

        //Modelo de datos a reemplazar cuando este seleccionado
        private ModeloDatos m;
        
        public DatosGraficas()
        {
            InitializeComponent();
            
            //Instancias de las clases.
            _mainWindow = Utils.MainWindowSingleton.GetInstance();
            md = Utils.DataModelSingleton.GetInstance();
            upd = Utils.UpdateDataSingleton.GetInstance();

            //Eventos de la propia ventana
            DataSelected += _mainWindow.OnDataSelected;
            DatosGraficasClosed += _mainWindow.OnCloseDatosGraficas;
            Closing += DatosGraficas_Closing;

            //Delegados y manejadoras.
            _removeData += _mainWindow.removeData;
            _v += AddData.NewWindow;
            _delegadoCargaDatos += md.LoadDataTests;
            _addData += md.CreateNewData;
            _updateData = upd.displayData;
            upd.Closing  += UpdateData_Closing;
            upd.DataAdded += actualizarDatos;
            _actualizarDatos += md.UpdateData;
        }

        public static void NewWindow(object sender, RoutedEventArgs e)
        {
            DatosGraficas c = Utils.DatosGraficasWindowSingleton.GetInstance();
            c.Show();
        }

        //Cargar ventana de añadir datos.
        private void AddElectionData(object sender, RoutedEventArgs e)
        {

            _v?.Invoke(sender, e);
            
        }

        //Cargar datos de prueba.
        private void LoadDataTests(object sender, RoutedEventArgs e)
        {
            _delegadoCargaDatos?.Invoke();
            if(md.ResultadosElectorales == null)
            {
                return;
            }
            resultadosLV.ItemsSource = md.ResultadosElectorales;    
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
                        this.m = (ModeloDatos)resultadosLV.SelectedItem;
                        CustomEventArgs c = new(this.m.Nombre, "", this.m.FechaElecciones, this.m.Partidos);
                        upd = Utils.UpdateDataSingleton.GetInstance();

                        //Le asignamos el método displayData.
                        _updateData += upd.displayData;
                        upd.DataAdded += actualizarDatos;

                        _updateData?.Invoke(this, c);
                        upd.newWindow();
                        
                        
                    };

                    // Para eliminar un elemento, simplemente cogemos nuestro modelo unico y lo eliminamos de la lista de resultadosElectorales
                    // Actualizamos las tablas y invocamos a un delegado para que notifique al mainwindow que hay que borrar la grafica.

                    MenuItem deleteMenuItem = new MenuItem();
                    deleteMenuItem.Header = "Eliminar datos electorales";
                    deleteMenuItem.Click += (deleteSender, deleteEventArgs) =>
                    {
                        md.ResultadosElectorales.Remove((ModeloDatos)resultadosLV.SelectedItem);
                        resultadosLV.Items.Refresh();

                        //La tabla de partidos se vacia.
                        resultadosLV2.ItemsSource = null;
                        resultadosLV2.Items.Refresh();

                        _removeData?.Invoke(this, null);
                    };

                    // Añadimos los items del menu al contexto del menu.
                    contextMenu.Items.Add(modifyMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    // Añadimos el contexto a la propiedad listview.Context de los resultados.
                    resultadosLV.ContextMenu = contextMenu;
                }
                var selectedElection = (ModeloDatos)resultadosLV.SelectedItem;

                Dictionary<string, Partido> partyData = selectedElection.Partidos;

                // Gracias al metodo Select de los diccionarios podemos crear una nueva estructura de datos de forma muy sencilla.
                // Lo bindeamos con el xaml y asociamos el Key con el nombre del partido y el Value con los escaños generando una lista.
                var partyDataCollection = partyData.Select(pair => new { Key = pair.Key, Value = pair.Value.Votos }).ToList();

                resultadosLV2.ItemsSource = partyDataCollection;

                //Enviamos los datos al MainWindow para su visualizacion.
                CustomEventArgsMain args = new(selectedElection.Partidos,selectedElection.Nombre); 
                DataSelected?.Invoke(this, args);

            }
        }
        //Función para añadir también los datos nuevos de la ventana AddData
        public void cargarDatos(object? sender, CustomEventArgs c)
        {

            //Añadimos a la coleccion observable de nuestro modelo  la información que nos han pasado por AddData.
            _addData?.Invoke(this, c);
            
            resultadosLV.ItemsSource = md.ResultadosElectorales;
            resultadosLV.Items.Refresh();

        }
        //Evento para controlar cuando se está cerrando la ventana secundaria.
        private void DatosGraficas_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            DatosGraficasClosed?.Invoke(this, EventArgs.Empty);
            md.ResultadosElectorales?.Clear();
        }

        private void UpdateData_Closing(object? sender,EventArgs e){

            // Cuando la ventana de actualizar datos se cierre, eliminamos el delegado para que no haya problemas
            // de referencia a una instancia nula.

            _updateData -= upd.displayData;
            upd.DataAdded -= actualizarDatos;

        }

        public void actualizarDatos(object? sender, CustomEventArgs c)
        {

            //Añadimos a la coleccion observable de nuestro modelo  la información que nos han pasado por AddData.
            c.ModeloDatosAReemplazar = this.m;
            _actualizarDatos?.Invoke(this, c);

            resultadosLV.ItemsSource = md.ResultadosElectorales;
            resultadosLV.Items.Refresh();
            

        }
    }
        
}
