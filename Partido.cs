using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;

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

        public override string ToString()
        {      
            return $"{Nombre}, {Votos}";
        }

        public TextBlock formatPartyData(KeyValuePair<string,Partido> partidoEntry)
        {
            // Obtenemos la info de los partidos para mostrarla.
            string partidoName = partidoEntry.Key;
            int partidoVotes = partidoEntry.Value.Votos;
            System.Drawing.Color partidoColor = partidoEntry.Value.Color;

            //Separamos en tokens para obtener solo el color 
            string[] tokens = partidoColor.ToString().Split(" ");
            string color = tokens[1];

            TextBlock textBlock = new()
            {
                Text = $"{partidoName}, {partidoVotes}, {color}"
            };

            return textBlock;
        }

        public List<Partido> GetPartidosA(Dictionary<string,Partido> infoPartidos)
        {
            //Creamos un diccionario y añadimos cargamos los datos
            List<Partido> Partidos = new();
            foreach (var partidoEntry in infoPartidos)
            {
                Partido partido = partidoEntry.Value;
                Partidos.Add(partido);
            }
            return Partidos;
        }
        public List<Partido> GetPartidos(Dictionary<string, Partido> infoPartidos, List<Partido> Partidos)
        {
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
            return Partidos;
        }
    }
}