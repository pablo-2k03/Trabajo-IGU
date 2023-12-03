using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pactometro
{
    /// <summary>
    /// Lógica de interacción para UpdateData.xaml
    /// </summary>
    public partial class UpdateData : Window
    {


        private ComboBox electorComunidad;
        private Dictionary<string, Partido> infoPartidos = new();
        private Partido p = new();
        private int nEscaños = 0;
        private TextBox electorEscaños;

        private ModeloDatos modeloUnico;
        private Eleccion eleccionAReemplazar;
        private List<Partido> partyDataCollection;
        public UpdateData()
        {
            InitializeComponent();
            StateChanged += changedState;
        }

        public enum AutonomousCommunity
        {
            Andalucia,
            Aragon,
            Asturias,
            Cantabria,
            CastillayLeon,
            CastillaLaMancha,
            Cataluña,
            Ceuta,
            Melilla,
            Madrid,
            Navarra,
            ComunidadValenciana,
            Extremadura,
            Galicia,
            IslasBaleares,
            IslasCanarias,
            LaRioja,
            PaisVasco,
            Murcia
        }

        public void displayData(string nombre, string fecha, List<Partido> partidos, Eleccion eleccionAReemplazar, ModeloDatos modeloUnico)
        {

            this.modeloUnico = modeloUnico;
            this.eleccionAReemplazar = eleccionAReemplazar;

            string tipoEleccion = string.Empty;
            string comunidad = string.Empty;

            string[] tokens = nombre.Split(" ");

            if (tokens.Length > 3)
            {
                tipoEleccion = tokens[0].ToUpper();
                comunidad = tokens[3].ToUpper();
            }
            else
            {
                tipoEleccion = tokens[1].ToUpper();
            }

            elecActual.Text = tipoEleccion;
            fechaActual.Text = fecha;
            partyDataCollection = new List<Partido>();

            foreach (var partido in partidos)
            {
                partyDataCollection.Add(partido);
            }

            partidosAct.ItemsSource = partyDataCollection;
            registroPartidos.ItemsSource = partyDataCollection;

            if (tipoEleccion == "GENERALES")
            {
                tipoElecciones.SelectedItem = tipoElecciones.Items[0];
            }
            else
            {
                tipoElecciones.SelectedItem = tipoElecciones.Items[1];
                electorEscaños.Text = eleccionAReemplazar.NumEscaños.ToString();
                electorComunidad.SelectedItem = electorComunidad.Items[GetComunidad(eleccionAReemplazar.Nombre)];
            }
            fechaNueva.SelectedDate = DateTime.ParseExact(fechaActual.Text, "dd/M/yyyy", CultureInfo.InvariantCulture);
            fechaNueva.DisplayDate = (DateTime)fechaNueva.SelectedDate;

            return;

        }
        private void changedState(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                elecActual.Width = 1790;
                fechaActual.Width = 1790;
            }
        }

        private void RegisterNewData(object sender, RoutedEventArgs e)
        {
            string electionType;
            ComboBoxItem item = (ComboBoxItem)tipoElecciones.SelectedItem;

            if (item != null)
            {
                electionType = item.Content.ToString().ToUpper();

                string comunity = "";
                if (electionType == "AUTONÓMICAS")
                {
                    if (electorComunidad.SelectedItem != null)
                    {
                        comunity = electorComunidad.SelectedItem.ToString();
                    }
                    else
                    {
                        MessageBox.Show("No ha seleccionado ninguna comunidad.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                }
                if (fechaNueva.SelectedDate.HasValue)
                {
                    string date = fechaNueva.SelectedDate.Value.ToString();
                    //Eliminamos la hora de la fecha, porque el formato original es 02/11/23 00:00:00
                    string[] tokens = date.Split(" ");
                    date = tokens[0];
                    if (registroPartidos.Items.Count > 0)
                    {

                        //Creamos un diccionario y añadimos cargamos los datos
                        List<Partido> Partidos = new();

                        // Agregar todos los partidos actuales
                        Partidos.AddRange(partyDataCollection);

                        foreach (var i in infoPartidos)
                        {
                            // Buscar el partido en Partidos por nombre
                            var partidoExistente = Partidos.FirstOrDefault(p => p.Nombre.Equals(i.Value.Nombre));

                            if (partidoExistente != null)
                            {
                                // Actualizar los votos si el partido ya existe
                                partidoExistente.Votos = i.Value.Votos;
                            }
                        }

                        modeloUnico.UpdateData(electionType, date, comunity, Partidos, nEscaños, eleccionAReemplazar);

                        nombre.Clear();
                        votos.Clear();

                        registroPartidos.ItemsSource = null;
                        MessageBox.Show("Datos actualizados correctamente.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                        return;
                    }
                    else
                    {
                        MessageBox.Show("No ha introducido la información de los partidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("No ha seleccionado una fecha del calendario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("No has seleccionado ningún tipo de elección.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void RegisterNewParty_Click(object sender, RoutedEventArgs e)
        {
            //Cuando den click para registrar la info del partido, el focus se lo ponemos al nombre de un nuevo partido para optimizar UX.
            nombre.Focus();

            string key = nombre.Text;
            string value = votos.Text;
            int votes;
            int numMaxEscaños = 0;
            ComboBoxItem item = (ComboBoxItem)tipoElecciones.SelectedItem;

            if (item != null)
            {
                string electionType = item.Content.ToString().ToUpper();

                if (electionType.ToUpper() == "GENERALES")
                {
                    numMaxEscaños = 350;
                }
                else
                {
                    numMaxEscaños = this.nEscaños;
                }
            }
            // Primera validación: Que hayan introducido un nombre, aunque posteriormente se va a revalidar por si introducen una string no valida (numero)
            if (!string.IsNullOrWhiteSpace(key))
            {

                try
                {
                    votes = int.Parse(value);

                    if (votes == 0) { MessageBox.Show("El numero de escaños no puede ser 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

                    if (votes > numMaxEscaños)
                    {
                        throw new Exception();
                    }

                    if (!Comprobar_Limite(nEscaños, infoPartidos))
                    {
                        return;
                    }


                }
                catch (Exception)
                {
                    votos.Clear();
                    MessageBox.Show("El numero de escaños no es valido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //Segunda validación: Comprobamos que el nombre sea una string valida y no un numero
                if (!int.TryParse(key, out int res) && !double.TryParse(key, out double res2))
                {

                    //Dejamos al usuario la elección libre del color.
                    var colorDialog = new System.Windows.Forms.ColorDialog();
                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        System.Drawing.Color selectedColor = colorDialog.Color;

                        //Creamos un partido.
                        string nombre = key;
                        int escaños = int.Parse(value);

                        Partido party = p.crearPartido(nombre, escaños, selectedColor);

                        if (party != null)
                        {
                            infoPartidos[party.Nombre] = party;

                            var existingParty = partyDataCollection.FirstOrDefault(p => p.Nombre.ToUpper().Equals(nombre.ToUpper()));

                            if (existingParty == null)
                            {
                                partyDataCollection.Add(party);
                            }
                            else
                            {
                                existingParty.Votos = escaños;
                            }

                            UpdateDataListBox();
                        }

                    }
                }
                else
                {
                    MessageBox.Show("El nombre del partido no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else
            {
                MessageBox.Show("No ha introducido un nombre para el partido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Limpiamos los campos de entrada.
            LimpiaCampos();
        }

        private void UpdateDataListBox()
        {

            // Añadimos la info de los partidos a la lista.
            foreach (var partidoEntry in infoPartidos)
            {

                // Obtenemos la info de los partidos para mostrarla.
                string partidoName = partidoEntry.Key;
                int partidoVotes = partidoEntry.Value.Votos;
                System.Drawing.Color partidoColor = partidoEntry.Value.Color;

                //Separamos en tokens para obtener solo el color 
                string[] tokens = partidoColor.ToString().Split(" ");
                string color = tokens[1];

                // Creamos el bloque de texto y lo formateamos.
                TextBlock textBlock = new()
                {
                    Text = $"{partidoName}, {partidoVotes}, {color}"
                };

                foreach(Partido p in registroPartidos.Items)
                {
                    if (p.Nombre.Equals(partidoName.ToUpper())) { 
                        p.Votos = partidoVotes;
                        break;
                    }
                }
                registroPartidos.Items.Refresh();
            }
        }

        private void Autonomicas_Selected(object sender, RoutedEventArgs e)
        {



            // Creamos el texblock de "COMUNIDAD".
            TextBlock textBlock = new()
            {
                Text = "Comunidad",
                HorizontalAlignment = HorizontalAlignment.Left,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Margin = new Thickness(0, 10, 0, 10)
            };

            // Instanciamos la nueva ComboBox y la cargamos con todas las comunidades autonomas de la enumeración.
            electorComunidad = new ComboBox
            {
                Name = "electorComunidad",
                Width = 177,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            foreach (AutonomousCommunity community in Enum.GetValues(typeof(AutonomousCommunity)))
            {
                electorComunidad.Items.Add(community.ToString());
            }

            //Creamos los selectores de escaños.

            TextBlock tb2 = new()
            {
                Text = "Escaños",
                HorizontalAlignment = HorizontalAlignment.Left,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Margin = new Thickness(0, 10, 0, 10)
            };

            this.electorEscaños = new()
            {
                Name = "electorEscaños",
                Width = 177,
                HorizontalAlignment = HorizontalAlignment.Left
            };



            //Limpiamos el stackpanel en caso de q hubiera datos y añadimos el texto de COMUNIDAD y el combobox.
            comunidad.Children.Clear(); // Clear previous content
            comunidad.Children.Add(textBlock);
            comunidad.Children.Add(electorComunidad);
            comunidad.Children.Add(tb2);
            comunidad.Children.Add(this.electorEscaños);
        }

        private void TipoElecciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Si seleccionan elecciones generales, eliminamos el combobox de selector de comunidad y el textblock que añadimos anteriormente.
            if (tipoElecciones.SelectedItem == generales)
            {
                comunidad.Children.Clear();
                nEscaños = 350;
            }

            //Si la pantalla está cargada y el usuario cambia el tipo de elecciones, los partidos se borran.
            if(IsLoaded)
            {
                partyDataCollection.Clear();
            }

            //La listbox se recarga.
            registroPartidos.Items.Refresh();

            //Reestablecemos el boton de añadir partidos porque el numero de escaños puede haber variado.
            reestablecerLimite();

            //Limpiamos los partidos.
            infoPartidos.Clear();
        }

        private void LimpiaCampos()
        {
            nombre.Clear();
            votos.Clear();
        }


        private void Votos_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(votos.Text))
            {
                votos.Text = "Introduzca numero votos";
            }
            else
            {
                if (tipoElecciones.Text.ToUpper() == "GENERALES")
                {
                    nEscaños = 350;
                    if (!Comprobar_Limite(nEscaños, infoPartidos))
                    {
                        establecerLimite();
                    }
                    else
                    {
                        reestablecerLimite();
                    }

                    return;
                }

                try
                {
                    nEscaños = int.Parse(electorEscaños.Text);

                    if (!Comprobar_Limite(nEscaños, infoPartidos))
                    {
                        establecerLimite();
                    }
                    else
                    {
                        reestablecerLimite();
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("Introduce un numero de escaños limite valido (un numero entero).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FocusNombrePartido(object sender, RoutedEventArgs e)
        {
            if (nombre.Text.StartsWith("Introduzca"))
            {
                nombre.Clear();
            }
        }

        private void SinFocusNombrePartido(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(nombre.Text))
            {
                nombre.Text = "Introduzca nombre partido";
            }
        }

        private void Votos_GotFocus(object sender, RoutedEventArgs e)
        {
            if (votos.Text.StartsWith("Introduzca"))
            {
                votos.Clear();
            }
        }

        private Boolean Comprobar_Limite(int nEscaños, Dictionary<string, Partido> infoPartidos)
        {
            int suma = 0;
            foreach (var partido in infoPartidos)
            {
                suma += partido.Value.Votos;
            }

            suma += int.Parse(votos.Text);

            try
            {
                if (suma > nEscaños)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("El numero de escaños totales no puede superar el máximo establecido: " + nEscaños, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
            return true;
        }

        private void establecerLimite()
        {
            registerNewParty.IsEnabled = false;
            registerNewParty.Content = "LIMITE ALCANZADO";
            registerNewParty.Foreground = Brushes.Red;
        }

        private void reestablecerLimite()
        {
            registerNewParty.IsEnabled = true;
            registerNewParty.Content = "Añadir nuevo partido";
            registerNewParty.Foreground = Brushes.Black;
        }


        private int GetComunidad(string Nombre)
        {
            string[] tokens = Nombre.Split(" ");
            string comunidad = tokens[3];
            if (comunidad.Equals("CyL")) { return 4; }
            // Assuming the enumeration values are named exactly as the communities in the string
            if (Enum.TryParse<AutonomousCommunity>(comunidad, out var communityEnum))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = communityEnum; // Set the content to the enum value
                int index = 0;
                foreach(AutonomousCommunity c in Enum.GetValues(typeof(AutonomousCommunity)))
                {
                    if (c.ToString().Equals(comunidad))
                    {
                        return index;
                    }
                    index++;
                }
                return 0;
            }
            else
            {
                MessageBox.Show("Error en la seleccion de comunidad: " + nEscaños, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }

        }

    }



}