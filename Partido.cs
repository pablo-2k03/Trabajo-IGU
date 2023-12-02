using System.ComponentModel;
using System.Drawing;



namespace Pactometro
{
    public class Partido : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public string Nombre { get; set; }
        public int Votos { get; set; }
        public Color Color { get; set; }

        public Partido(string name, int votes, System.Drawing.Color color)
        {
            Nombre = name;
            Votos = votes;
            Color = color;
        }
        public Partido() { this.Nombre = string.Empty; }
        public Partido crearPartido(string nombre,int votos,System.Drawing.Color color)
        {
            return new Partido(nombre, votos, color);
        }

        public override string ToString()
        {


            // Return a string representation containing relevant information.
            return $"{Nombre}, {Votos}";
        }
    }
}