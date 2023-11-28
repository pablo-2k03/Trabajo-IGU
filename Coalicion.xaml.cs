
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Pactometro
{
    /// <summary>
    /// Lógica de interacción para Coalicion.xaml
    /// </summary>
    public partial class Coalicion : Window
    {

        public event EventHandler<CustomEventArgsPartidos> hayCoalicion;


        private Eleccion eleccion;
        private bool isDragging = false;
        private Point startPoint;
        private ObservableCollection<Partido> partidos;

        public Coalicion(Eleccion seleccion)
        {
            InitializeComponent();
            eleccion = seleccion;
            partidos = new ObservableCollection<Partido>(eleccion.Partidos.Values);
            infoPartidos.ItemsSource = partidos;
        }

        private void infoPartidos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void infoPartidos_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(null);
                if (Math.Abs(currentPosition.X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    isDragging = true;
                    Partido selectedItem = (Partido)infoPartidos.SelectedItem;
                    DragDrop.DoDragDrop(infoPartidos, selectedItem, DragDropEffects.Move);
                }
            }
        }

        private void partidosCoalicion_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Partido)))
            {
                Partido itemToAdd = (Partido)e.Data.GetData(typeof(Partido));

                if (itemToAdd != null)
                {
                    partidos.Remove(itemToAdd);
                    partidosCoalicion.Items.Add(itemToAdd);
                }
            }
        }

        private void formar_gobierno(object sender, RoutedEventArgs e)
        {
            var listaPartidosCoalicion = partidosCoalicion.Items;
            List<Partido> list = new();
            int total = 0;
            foreach (var i in  listaPartidosCoalicion)
            {
                Partido p = (Partido)i;
                list.Add(p);
            }
            foreach(var p in list)
            {
                total += p.Votos;
            }
            if(total < eleccion.Mayoria)
            {
                MessageBox.Show("Los partidos seleccionados no suman los votos necesarios para alcanzar la mayoria.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                CustomEventArgsPartidos c = new(list);
                hayCoalicion(this, c);
                partidos.Clear();
                Close();
            }
           
        }
    }

}
