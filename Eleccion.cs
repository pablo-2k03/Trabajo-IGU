using System.Collections.Generic;
using System.ComponentModel;


namespace Pactometro
{
    public class Eleccion : INotifyPropertyChanged
    {


        //Definición variables de clase
        private enum TIPO_ELECCIONES { GENERALES, AUTONÓMICAS };
        private string _nombre;
        private string _fechaElecciones;
        private int _mayoria;
        private Dictionary<string, Partido> _partidos;
        private int _numEscaños;

        public event PropertyChangedEventHandler? PropertyChanged;

        //Definición de propiedades de clase.
        public string Nombre
        {
            get { return _nombre; }
            set
            {
                if (_nombre != value)
                {
                    _nombre = value;
                    if(PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Nombre"));
                    }

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
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("NumEscaños"));
                    }
                }
            }

        }

        public string FechaElecciones
        {
            get { return _fechaElecciones; }

            set
            {
                if (_fechaElecciones != value)
                {
                    _fechaElecciones = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FechaElecciones"));
                    }
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
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Mayoria"));
                    }
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
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Partidos"));
                    }
                }
            }
        }


        public Eleccion(string tipoElecciones, string fecha, Dictionary<string, Partido> partidos, string nombre, int nEscaños = 81)
        {

            switch (tipoElecciones.ToUpper())
            {
                case "GENERALES":
                    _mayoria = 176;
                    _numEscaños = 350;
                    break;
                case "AUTONÓMICAS":
                    _mayoria = nEscaños / 2 + 1;
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
    }
}
