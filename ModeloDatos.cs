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

        //Definición variables de clase
        private enum TIPO_ELECCIONES { GENERALES, AUTONÓMICAS };
        private string _nombre;
        private string _fechaElecciones;
        private int _mayoria;
        private Dictionary<string, Partido> _partidos;
        private int _numEscaños;

        private static ObservableCollection<ModeloDatos> _resultadosElectorales = new();


        //Definición de propiedades de clase.
        public string Nombre
        {
            get { return _nombre; }
            set
            {
               if (_nombre != value)
                {
                    _nombre = value;
                }
            }
        }

        public int NumEscaños
        {
            get { return _numEscaños; }

            set
            {
                 if (_numEscaños != value)
                {
                    _numEscaños = value;
                }
            }

        }
        
        public string FechaElecciones
        {
            get { return _fechaElecciones; }

        }

        public  ObservableCollection<ModeloDatos> ResultadosElectorales
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


        public int Mayoria
        {
            get { return _mayoria; }

            set
            {
                if (_mayoria != value)
                {
                    _mayoria = value;
                }
            }
        }

        public Dictionary<string, Partido> Partidos
        {
            get { return _partidos; }

            set
            {
                if (_partidos != value)
                {
                    _partidos = value;
                }
            }
        }

        

        public ModeloDatos()
        {
            _fechaElecciones = "";
            _partidos = null;
            _nombre = "";
            _mayoria = 0;
            _numEscaños = 0;
            _resultadosElectorales = new ObservableCollection<ModeloDatos>();
        }


        public ModeloDatos(string tipoElecciones, string fecha, Dictionary<string, Partido> partidos, string nombre,int nEscaños=81)
        {

            switch (tipoElecciones.ToUpper())
            {
                case "GENERALES":
                    _mayoria = 176;
                    _numEscaños = 350;
                    break;
                case "AUTONÓMICAS":
                    _mayoria = nEscaños/2+1;
                    _numEscaños = nEscaños;
                    break;
                default:
                    _mayoria = 0;
                    break;
            }
            _fechaElecciones = fecha;
            _partidos = partidos;
            _nombre = nombre;
        }


        // Load data method
        public void LoadDataTests()
        {
            ModeloDatos modeloUnico = Utils.DataModelSingleton.GetInstance();

            modeloUnico.ResultadosElectorales?.Clear();

            string path = "../../../testCases.txt";
            string[] lines = File.ReadAllLines(path);

            TIPO_ELECCIONES tipoElecciones = TIPO_ELECCIONES.GENERALES;
            string fechaElecciones = null;
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

                        Dictionary<String, Partido> partidos = new();
                        foreach (var v in partidosInfo)
                            partidos.Add(v.Key, new Partido(v.Key,v.Value,default));

                        //Creamos una clase de datos y la guardamos a la coleccion.


                        ModeloDatos resultados = new(tipoElecciones.ToString(), fechaElecciones, partidos, nombre);
                        modeloUnico.ResultadosElectorales.Add(resultados);

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
                Dictionary<String, Partido> partidos = new();
                foreach (var v in partidosInfo)
                    partidos.Add(v.Key, new Partido(v.Key, v.Value, default));

                ModeloDatos resultados = new(tipoElecciones.ToString(), fechaElecciones, partidos, nombre);
                modeloUnico.ResultadosElectorales.Add(resultados);
            }

        }



        public void CreateNewData(object sender, CustomEventArgs c)
        {


            ModeloDatos modeloUnico = Utils.DataModelSingleton.GetInstance();

            string tipo = c.tipoEleccion;
            string fecha = c.fechaElectoral;
            string comunidad = c.comunidad;
            Dictionary<string, Partido> partidos = c.infoPartidos;
            Dictionary<string, Partido> infoPartidos = new();
            int nEscaños = c.nEscaños;
            foreach (var i in partidos)
            {
                infoPartidos.Add(i.Key, i.Value);
            }

            string nombreelec;
            if (tipo == "GENERALES")
            {
                nombreelec = "Elecciones Generales " + fecha; 
            }
            else
            {
                nombreelec = "Autonómicas Comunidad de "+ comunidad +" "+ fecha;
            }
            ModeloDatos nuevo = new(tipo, fecha, infoPartidos,nombreelec,nEscaños);
            modeloUnico.ResultadosElectorales.Add(nuevo);
        }


        public void UpdateData(object sender, CustomEventArgs c)
        {
            ModeloDatos modeloAReemplazar = c.ModeloDatosAReemplazar;

            // Buscamos el indice del modelo que tenemos que reemplazar.
            int indexToReplace = ResultadosElectorales.IndexOf(modeloAReemplazar);

            //Si está en la lista.
            if (indexToReplace != -1)
            {
                string tipo = c.tipoEleccion;
                string fecha = c.fechaElectoral;
                string comunidad = c.comunidad;
                Dictionary<string, Partido> partidos = c.infoPartidos;
                Dictionary<string, Partido> infoPartidos = new();
                int nEscaños = c.nEscaños;

                foreach (var i in partidos)
                {
                    infoPartidos.Add(i.Key, i.Value);
                }

                string nombreelec;
                if (tipo == "GENERALES")
                {
                    nombreelec = "Elecciones Generales " + fecha;
                }
                else
                {
                    nombreelec = "Autonómicas Comunidad de " + comunidad + " " + fecha;
                }

                ModeloDatos nuevo = new ModeloDatos(tipo, fecha, infoPartidos, nombreelec, nEscaños);

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
    }
}
