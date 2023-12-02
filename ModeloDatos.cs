using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Drawing;

namespace Pactometro
{
   

    public class ModeloDatos
    {


        private ObservableCollection<Eleccion> _resultadosElectorales;
        private enum TIPO_ELECCIONES { GENERALES, AUTONÓMICAS };

        public  ObservableCollection<Eleccion> ResultadosElectorales
        {
            get { return _resultadosElectorales; }

            set
            {
                if(_resultadosElectorales != value)
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

                        List< Partido> partidos = new();

                        
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
                List< Partido> partidos = new();
                foreach (var v in partidosInfo)
                    partidos.Add(new Partido(v.Key, v.Value, getRandomColor()));

                Eleccion resultados = new(tipoElecciones.ToString(), fechaElecciones, partidos, nombre);
                ResultadosElectorales.Add(resultados);
            }

        }



        public void CreateNewData(string tipoEleccion, string fechaElectoral, string comunidad, List< Partido> partidos, int nEscaños)
        {


            List< Partido> infoPartidos = new();

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
                nombreelec = "Autonómicas Comunidad de "+ comunidad +" "+ fechaElectoral;
            }
            Eleccion nuevo = new(tipoEleccion, fechaElectoral, infoPartidos,nombreelec,nEscaños);
            ResultadosElectorales.Add(nuevo);
            return;
        }


        public void UpdateData(string tipoEleccion,string fechaElectoral,string comunidad,List<Partido> partidos,int nEscaños,Eleccion eleccionAReemplazar)
        {

            // Buscamos el indice del modelo que tenemos que reemplazar.
            int indexToReplace = ResultadosElectorales.IndexOf(eleccionAReemplazar);

            //Si está en la lista.
            if (indexToReplace != -1)
            {
                List< Partido> infoPartidos = new();

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
    }
}
