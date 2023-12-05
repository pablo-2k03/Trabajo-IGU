using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.ComponentModel.Design;
using System.Linq;

namespace Pactometro
{


    public class ModeloDatos
    {


        private ObservableCollection<Eleccion> _resultadosElectorales;
        private enum TIPO_ELECCIONES { GENERALES, AUTONÓMICAS };

        public ObservableCollection<Eleccion> ResultadosElectorales
        {
            get { return _resultadosElectorales; }

            set
            {
                if (_resultadosElectorales != value)
                {
                    _resultadosElectorales = value;
                }
            }
        }



        public ModeloDatos()
        {
            _resultadosElectorales = new ObservableCollection<Eleccion>();
        }


        // Load data method
        public void LoadDataTests()
        {
            ModeloDatos modeloUnico = Utils.DataModelSingleton.GetInstance();

            modeloUnico.ResultadosElectorales?.Clear();

            string path = "../../../testCases.txt";
            string[] lines = File.ReadAllLines(path);

            TIPO_ELECCIONES tipoElecciones = TIPO_ELECCIONES.GENERALES;
            string fechaElecciones = string.Empty;
            Dictionary<string, int> partidosInfo = new();
            string nombre = string.Empty;

            foreach (string line in lines)
            {
                if (line.StartsWith("ELECCIONES"))
                {
                    if (fechaElecciones != null && partidosInfo.Count > 0)
                    {
                        //Creamos un nuevo diccionario para guardar los partidos, porque si se lo asignamos directamente le asignamos la referencia
                        //Y luego si lo borramos, se nos borraria todo.

                        List<Partido> partidos = new();


                        foreach (var v in partidosInfo)
                        {


                            partidos.Add(new Partido(v.Key, v.Value, getRandomColor()));
                        }

                        //Creamos una clase de datos y la guardamos a la coleccion.


                        Eleccion resultados = new(tipoElecciones.ToString(), fechaElecciones, partidos, nombre);
                        ResultadosElectorales.Add(resultados);

                        //Limpiamos el diccionario que nos sirve de recopilacion de partidos.
                        partidosInfo.Clear();
                    }

                    // Extraemos el tipo de eleccion, la fecha y formateamos el nombre.
                    string[] parts = line.Split(' ');
                    if (parts.Length >= 3)
                    {
                        tipoElecciones = parts[1].ToUpper() == "GENERALES" ? TIPO_ELECCIONES.GENERALES : TIPO_ELECCIONES.AUTONÓMICAS;
                        if (tipoElecciones != TIPO_ELECCIONES.GENERALES)
                        {
                            fechaElecciones = parts[3];
                            parts[1] = parts[1].ToLower();
                            parts[1] = FormatName(parts[1]);
                            nombre = parts[1] + " Comunidad " + "de " + parts[2] + " " + parts[3];

                        }
                        else
                        {
                            fechaElecciones = parts[2];
                            parts[0] = parts[0].ToLower();
                            parts[0] = FormatName(parts[0]);
                            parts[1] = parts[1].ToLower();
                            parts[1] = FormatName(parts[1]);
                            nombre = parts[0] + " " + parts[1] + " " + parts[2];
                        }


                    }
                }
                else
                {
                    // Parse party information
                    string[] partidoInfo = line.Split(',');
                    if (partidoInfo.Length == 2)
                    {
                        string partido = partidoInfo[0].Trim();
                        string votosStr = partidoInfo[1].Trim();
                        if (int.TryParse(votosStr, out int votos))
                        {
                            partidosInfo.Add(partido, votos);
                        }
                    }
                }
            }

            // Create and store ResultadosElecciones object for the last election
            if (fechaElecciones != null && partidosInfo.Count > 0)
            {
                List<Partido> partidos = new();
                foreach (var v in partidosInfo)
                    partidos.Add(new Partido(v.Key, v.Value, getRandomColor()));

                Eleccion resultados = new(tipoElecciones.ToString(), fechaElecciones, partidos, nombre);
                ResultadosElectorales.Add(resultados);
            }

        }



        public void CreateNewData(string tipoEleccion, string fechaElectoral, string comunidad, List<Partido> partidos, int nEscaños)
        {


            List<Partido> infoPartidos = new();

            foreach (var i in partidos)
            {
                infoPartidos.Add(i);
            }

            string nombreelec;
            if (tipoEleccion == "GENERALES")
            {
                nombreelec = "Elecciones Generales " + fechaElectoral;
            }
            else
            {
                nombreelec = "Autonómicas Comunidad de " + comunidad + " " + fechaElectoral;
            }
            Eleccion nuevo = new(tipoEleccion, fechaElectoral, infoPartidos, nombreelec, nEscaños);
            ResultadosElectorales.Add(nuevo);
            return;
        }


        public void UpdateData(string tipoEleccion, string fechaElectoral, string comunidad, List<Partido> partidos, int nEscaños, Eleccion eleccionAReemplazar)
        {

            // Buscamos el indice del modelo que tenemos que reemplazar.
            int indexToReplace = ResultadosElectorales.IndexOf(eleccionAReemplazar);

            //Si está en la lista.
            if (indexToReplace != -1)
            {
                List<Partido> infoPartidos = new();

                foreach (var i in partidos)
                {
                    infoPartidos.Add(i);
                }

                string nombreelec;
                if (tipoEleccion == "GENERALES")
                {
                    nombreelec = "Elecciones Generales " + fechaElectoral;
                }
                else
                {
                    nombreelec = "Autonómicas Comunidad de " + comunidad + " " + fechaElectoral;
                }

                Eleccion nuevo = new Eleccion(tipoEleccion, fechaElectoral, infoPartidos, nombreelec, nEscaños);

                // Reemplazamos el modelo antiguo por el nuevo
                ResultadosElectorales[indexToReplace] = nuevo;
            }
        }



        public static string FormatName(string input)
        {
            CultureInfo cultureInfo = new("es-ES");
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(input);
        }


        private Color getRandomColor()
        {
            Random random = new Random();

            byte red = (byte)random.Next(256);
            byte green = (byte)random.Next(256);
            byte blue = (byte)random.Next(256);

            return Color.FromArgb(255, red, green, blue);

        }


        public void exportAll()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
            saveFileDialog.Title = "Guardar información de todas las elecciones";

            // Muestra el cuadro de diálogo
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string rutaArchivo = saveFileDialog.FileName;

                try
                {
                    // Aquí puedes agregar la lógica para escribir los datos en el archivo
                    // Por ejemplo, si tienes una lista de datos llamada "datos", podrías hacer algo como:

                    // List<string> datos = ObtenerDatos(); // Obtén tus datos desde alguna fuente
                    StringBuilder sb = new StringBuilder();
                    foreach (var i in ResultadosElectorales)
                    {
                        sb.Append(i.ToString());
                    }

                    // Para este ejemplo simple, escribiremos un mensaje en el archivo
                    File.WriteAllText(rutaArchivo, sb.ToString());

                    System.Windows.MessageBox.Show("Datos exportados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error al exportar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void importFromFile(Boolean isOne)
        {
            try
            {

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                openFileDialog.Title = "Seleccionar archivo de datos";

                // Si el usuario ha aceptado el modal, borramos los datos que tenia y cargamos los nuevos.
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!isOne)
                    {
                        this.ResultadosElectorales.Clear();
                    }
                    // Get the selected file path
                    string filePath = openFileDialog.FileName;

                    // Read all lines from the file into an array
                    string[] lines = File.ReadAllLines(filePath);

                    // Initialize variables to store data
                    Eleccion eleccion = null;
                    List<Partido> partidos = null;
                    List<Eleccion> elecciones = new();
                    // Loop through each line in the file
                    foreach (string line in lines)
                    {

                        if (line == ".")
                        {
                            if (partidos != null && partidos.Count > 0)
                            {
                                eleccion.Partidos = partidos;
                                elecciones.Add(eleccion);
                                eleccion = null;
                                partidos = null;
                            }
                        }

                        // Split each line into parts based on the colon ':' separator
                        string[] parts = line.Split(':', StringSplitOptions.TrimEntries);
                        if (parts.Length == 2)
                        {
                            switch (parts[0])
                            {
                                case "Nombre":
                                    eleccion = new Eleccion((parts[1].Split(" "))[1], "", null, "");
                                    eleccion.Nombre = parts[1].TrimEnd(',');
                                    partidos = new List<Partido>();
                                    break;
                                case "Fecha":
                                    // Parse the "Fecha" line and set the property of the Eleccion object
                                    if (eleccion != null)
                                        eleccion.FechaElecciones = parts[1].TrimEnd(',');
                                    break;
                                case "Mayoria":
                                    if (eleccion != null)
                                        eleccion.Mayoria = int.Parse(parts[1].TrimEnd(','));
                                    break;
                                case "Escaños":
                                    if (eleccion != null)
                                        eleccion.NumEscaños = int.Parse(Regex.Match(parts[1], @"\d+").Value);
                                    break;
                            }
                        }
                        else if (parts.Length == 1 && parts[0].StartsWith("Partidos,"))
                        {
                            partidos = new List<Partido>();

                        }
                        else if (parts.Length == 1 && !parts[0].StartsWith("Partidos,"))
                        {


                            string[] partidoInfo = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (partidoInfo.Length >= 3)
                            {
                                string partidoNombre = partidoInfo[0];
                                int partidoEscaños = int.Parse(partidoInfo[1]);
                                byte a = 0, r = 0, g = 0, b = 0;
                                for (int i = 2; i < partidoInfo.Length; i++)
                                {
                                    if (partidoInfo[i].TrimEnd(',').StartsWith('['))
                                    {
                                        byte.TryParse(partidoInfo[i].Split("=")[1].TrimEnd(','), out a);
                                    }
                                    else if (partidoInfo[i].TrimEnd(',').EndsWith(']'))
                                    {
                                        byte.TryParse(partidoInfo[i].Split("=")[1].TrimEnd(',').Split("]")[0], out b);
                                    }
                                    else
                                    {
                                        if (partidoInfo[i].TrimEnd(',').StartsWith('R'))
                                        {
                                            byte.TryParse(partidoInfo[i].Split("=")[1].TrimEnd(','), out r);
                                        }
                                        else
                                        {
                                            byte.TryParse(partidoInfo[i].Split("=")[1].TrimEnd(','), out g);
                                        }
                                    }
                                }

                                Partido partido = new Partido
                                {
                                    Nombre = partidoNombre,
                                    Votos = partidoEscaños,
                                    Color = Color.FromArgb(a, r, g, b)
                                };

                                if (partidos != null)
                                    partidos.Add(partido);
                            }
                        }

                    }
                    foreach (var i in elecciones)
                    {
                        ResultadosElectorales.Add(i);
                    }
                    System.Windows.MessageBox.Show("Datos importados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al importar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void exportElection(List<Eleccion> eleccionAExportar)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
            saveFileDialog.Title = "Guardar información de todas las elecciones";

            // Muestra el cuadro de diálogo
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string rutaArchivo = saveFileDialog.FileName;

                try
                {
                    if (eleccionAExportar.Count > 1)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (Eleccion i in eleccionAExportar)
                        {
                            sb.Append(i.ToString());
                        }
                        File.WriteAllText(rutaArchivo, sb.ToString());
                        System.Windows.MessageBox.Show("Datos exportados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    else
                    {
                        // Para este ejemplo simple, escribiremos un mensaje en el archivo
                        File.WriteAllText(rutaArchivo, eleccionAExportar[0].ToString());

                        System.Windows.MessageBox.Show("Datos exportados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error al exportar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public Dictionary<string,Partido> validateData(string key,string value,int numMaxEscaños,int nEscaños,Dictionary<string,Partido>infoPartidos,int votos)
        {
            // Primera validación: Que hayan introducido un nombre, aunque posteriormente se va a revalidar por si introducen una string no valida (numero)
            if (!string.IsNullOrWhiteSpace(key))
            {

                try
                {
                    if (votos > numMaxEscaños)
                    {
                        throw new Exception();
                    }

                    if (!Comprobar_Limite(nEscaños, infoPartidos,votos))
                    {
                        return null;
                    }


                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("El numero de escaños no es valido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
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
                        Partido party = new Partido(nombre, escaños, selectedColor);

                        if (party != null)
                        {
                            infoPartidos[party.Nombre] = party;
                            return infoPartidos;
                        }

                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("El nombre del partido no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

            }
            else
            {
                System.Windows.MessageBox.Show("No ha introducido un nombre para el partido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return null;
        }

        public Boolean Comprobar_Limite(int nEscaños, Dictionary<string, Partido> infoPartidos,int votos)
        {
            int suma = 0;
            foreach (var partido in infoPartidos)
            {
                suma += partido.Value.Votos;
            }

            suma += votos;

            try
            {
                if (suma > nEscaños)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("El numero de escaños totales no puede superar el máximo establecido: " + nEscaños, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
            return true;
        }
        public int getEscaños(string electionType,int nEscaños)
        {
            if (electionType == "GENERALES")
            {
                return 350;
            }
            else
            {
                return nEscaños;
            }
        }

        public int checkVotes(string votos,int numMaxEscaños)
        {
            try
            {
                int votes = int.Parse(votos);

                if (votes == 0) { System.Windows.MessageBox.Show("El numero de escaños no puede ser 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return -1; }

                if (votes > numMaxEscaños)
                {
                    throw new Exception();
                }

                return int.Parse(votos);
            }
            catch(Exception)
            {
                System.Windows.MessageBox.Show("El numero de escaños es erroneo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        public string getDateFormatted(string date)
        {
            //Eliminamos la hora de la fecha, porque el formato original es 02/11/23 00:00:00
            string[] tokens = date.Split(" ");
            return tokens[0];
        }
        public bool checkSumaVotosActualizar(List<Partido> partyDataCollection,string value,string key,int votes,int numMaxEscaños)
        {
            int j = 0;
            int votosPAnterior = 0;
            try
            {
                foreach (var i in partyDataCollection)
                {
                    if (partyDataCollection[j].Nombre.ToUpper().Equals(key.ToUpper()))
                    {
                        votosPAnterior = partyDataCollection[j].Votos;
                        break;
                    }
                    j++;
                }
                //Si está registrado tenemos q ver si los escaños nuevo no se sobrepasan.
                if (votosPAnterior != 0)
                {
                    int sumaAntigua = getTotalEscañosPartidosActuales(partyDataCollection) - votosPAnterior;
                    int sNueva = sumaAntigua + votes;
                    if (sNueva > numMaxEscaños)
                    {
                        throw new Exception();
                    }
                    return true;
                }
                //Si no está registrado tenemos q ver q no se sobrepase.
                else
                {
                    int suma = getTotalEscañosPartidosActuales(partyDataCollection) + votes;
                    if (suma > numMaxEscaños) { throw new Exception(); }
                    return true;
                }
            }
            catch(Exception)
            {
                System.Windows.MessageBox.Show("El numero de escaños es erroneo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private int getTotalEscañosPartidosActuales(List<Partido> partyDataCollection)
        {
            return partyDataCollection.Sum(partido => partido.Votos);
        }

        public Dictionary<string,Partido> validateUpdateData(string key,string value,Dictionary<string,Partido> infoPartidos,List<Partido> partyDataCollection)
        {
            //Segunda validación: Comprobamos que el nombre sea una string valida y no un numero
            if (!int.TryParse(key, out int res) && !double.TryParse(key, out double res2))
            {
                var colorDialog = new System.Windows.Forms.ColorDialog();
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.Drawing.Color selectedColor = colorDialog.Color;

                    string nombre = key;
                    int escaños = int.Parse(value);
                    Partido party = new Partido(nombre, escaños, selectedColor);

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

                        return infoPartidos;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("El nombre del partido no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

    }
}
