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
            if (_tipoElecciones.SelectedItem == null) {
                nombre.IsEnabled = false;
                votos.IsEnabled = false;
            }
        }


        private void RegisterNewData(object sender, RoutedEventArgs e)
        {
            string electionType;
            ComboBoxItem item = (ComboBoxItem)_tipoElecciones.SelectedItem;

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
                if (_calendario.SelectedDate.HasValue)
                {
                    string date = _calendario.SelectedDate.Value.ToString();

                    date = this.modeloUnico.getDateFormatted(date);

                    if (registroPartidos_.Items.Count > 0)
                    {

                        List<Partido> Partidos = this.p.GetPartidosA(infoPartidos);

                        modeloUnico.CreateNewData(electionType, date, comunity, Partidos, nEscaños);

                        nombre.Clear();
                        votos.Clear();
                        registroPartidos_.Items.Clear();

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
            int numMaxEscaños = 0;
            ComboBoxItem item = (ComboBoxItem)_tipoElecciones.SelectedItem;

            if (item != null)
            {
                string electionType = item.Content.ToString().ToUpper();

                numMaxEscaños = this.modeloUnico.getEscaños(electionType.ToUpper(),this.nEscaños);
                
            }

            infoPartidos = this.modeloUnico.validateData(key, value, numMaxEscaños, nEscaños, infoPartidos,int.Parse(votos.Text));

            if(infoPartidos != null)
            {
                UpdateDataListBox();
            }

            // Limpiamos los campos de entrada.
            LimpiaCampos();
        }

        private void UpdateDataListBox()
        {
            // Limpiamos la lista para que no se dupliquen datos.
            registroPartidos_.Items.Clear();


            
            // Añadimos la info de los partidos a la lista.
            foreach (var partidoEntry in infoPartidos)
            {
   
                TextBlock tb = this.p.formatPartyData(partidoEntry);                

                // Añadimos al registro.
                registroPartidos_.Items.Add(tb);

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
            if (_tipoElecciones.SelectedItem == generales)
            {
                comunidad.Children.Clear();
                nEscaños = 350;    
            }
            else
            {
                nEscaños = 0;
            }
            nombre.IsEnabled = true;
            votos.IsEnabled = true;

            //Reestablecemos el boton de añadir partidos porque el numero de escaños puede haber variado.
            reestablecerLimite();

            //Limpiamos los partidos.
            infoPartidos.Clear();

            registroPartidos_.Items.Clear();
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
                if (_tipoElecciones.Text.ToUpper() == "GENERALES")
                {
                    nEscaños = 350;
                    int res = this.modeloUnico.checkVotes(votos.Text, nEscaños);
                    if (res == -1) { MessageBox.Show("Introduce un numero de escaños valido (un numero entero).", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                    if (!this.modeloUnico.Comprobar_Limite(nEscaños, infoPartidos,res))
                    {
                        establecerLimite();
                    }
                    else
                    {
                        reestablecerLimite();
                    }

                    return;
                }

                nEscaños = this.modeloUnico.checkVotes(electorEscaños.Text,nEscaños);
                if (nEscaños == -1) { MessageBox.Show("Introduce un numero de escaños limite valido (un numero entero).", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

                int res2 = this.modeloUnico.checkVotes(votos.Text, nEscaños);
                if (res2 == -1) { MessageBox.Show("Introduce un numero de escaños valido (un numero entero).", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

                if (!this.modeloUnico.Comprobar_Limite(nEscaños, infoPartidos,res2))
                {
                    establecerLimite();
                }
                else
                {
                    reestablecerLimite();
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

