using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Controls;

namespace Pactometro
{
    /// <summary>
    /// Lógica de interacción para AddData.xaml
    /// </summary>
    public partial class AddData : Window
    {

        public event EventHandler<EventArgs> DataCreated;

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

        private ComboBox electorComunidad;
        private Dictionary<string, Partido> infoPartidos = new();
        private Partido p = new();
        private int nEscaños=0;
        private TextBox electorEscaños;
        private ModeloDatos modeloUnico = Utils.DataModelSingleton.GetInstance();

        public AddData( )
        {
            InitializeComponent();
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
                if (calendario.SelectedDate.HasValue)
                {
                    string date = calendario.SelectedDate.Value.ToString();

                    //Eliminamos la hora de la fecha, porque el formato original es 02/11/23 00:00:00
                    string[] tokens = date.Split(" ");
                    date = tokens[0];
                    if (registroPartidos.Items.Count > 0)
                    {

                        //Creamos un diccionario y añadimos cargamos los datos
                        Dictionary<String, Partido> Partidos = new();
                        foreach (var partidoEntry in infoPartidos)
                        {
                            string partidoName = partidoEntry.Key;
                            Partido partido = partidoEntry.Value;
                            Partidos.Add(partidoName, partido);
                        }

                        modeloUnico.CreateNewData(electionType, date, comunity, Partidos, nEscaños);

                        nombre.Clear();
                        votos.Clear();
                        registroPartidos.Items.Clear();
                        DataCreated(this,e);
                        MessageBox.Show("Datos añadidos correctamente.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var colorDialog = new System.Windows.Forms.ColorDialog();
                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        System.Drawing.Color selectedColor = colorDialog.Color;

                        string nombre = key;
                        int escaños = int.Parse(value);

                        Partido party = p.crearPartido(nombre,escaños,selectedColor);

                        if (party != null)
                        {
                            infoPartidos[party.Nombre] = party;
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
            // Limpiamos la lista para que no se dupliquen datos.
            registroPartidos.Items.Clear();

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

                // Añadimos al registro.
                registroPartidos.Items.Add(textBlock);

            }
        }

        private void Autonomicas_Selected(object sender, RoutedEventArgs e)
        {



            // Creamos el texblock de "COMUNIDAD".
            TextBlock textBlock = new()
            {
                Text = "Comunidad",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                FontStretch = FontStretches.Condensed,
                Margin = new Thickness(0, 10, 0, 10)
            };

            // Instanciamos la nueva ComboBox y la cargamos con todas las comunidades autonomas de la enumeración.
            electorComunidad = new ComboBox
            {
                Name = "electorComunidad",
                Width = 177,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(30, 0, 0, 0)
            };

            foreach (AutonomousCommunity community in Enum.GetValues(typeof(AutonomousCommunity)))
            {
                electorComunidad.Items.Add(community.ToString());
            }

            //Creamos los selectores de escaños.

            TextBlock tb2 = new()
            {
                Text = "Escaños",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                FontStretch = FontStretches.Condensed,
                Margin = new Thickness(0, 10, 0, 10)
            };

            this.electorEscaños = new()
            {
                Name = "electorEscaños",
                Width = 177,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(30, 0, 0, 0)
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
                //Si son generales no se hace la comprobación del limite de escaños porque por defecto son 350.
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
            registerNewParty.Foreground = System.Windows.Media.Brushes.Red;
        }

        private void reestablecerLimite()
        {
            registerNewParty.IsEnabled = true;
            registerNewParty.Content = "Añadir nuevo partido";
            registerNewParty.Foreground = System.Windows.Media.Brushes.Black;
        }


    }

}

