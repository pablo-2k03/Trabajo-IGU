using System.Drawing;



namespace Pactometro
{
    public class Partido
    {

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
    }
}