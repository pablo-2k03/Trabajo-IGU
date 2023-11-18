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
        public Partido() { }
        public Partido crearPartido(object sender,CustomEventArgsAddParty p)
        {
            return new Partido(p.name, p.votos, p.Color);
        }
    }
}