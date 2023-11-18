using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pactometro
{
    public class CustomEventArgs : EventArgs
    {
        public string tipoEleccion { get; }
        public string comunidad { get; }

        public string fechaElectoral { get; }

        public Dictionary<string, Partido> infoPartidos { get; }

        public Color color { get; }

        public int nEscaños { get; }

        public ModeloDatos ModeloDatosAReemplazar { get; set; }


        public CustomEventArgs(string tipoEleccion, string comunidad, string fechaElectoral, Dictionary<string, Partido> infoPartidos,int nEscaños=0,ModeloDatos m = null)
        {
            this.tipoEleccion = tipoEleccion;
            this.comunidad = comunidad;
            this.fechaElectoral = fechaElectoral;
            this.infoPartidos = infoPartidos;
            this.nEscaños = nEscaños;
            this.ModeloDatosAReemplazar = m;
        }
    }


    public class CustomEventArgsMain : EventArgs
    {

        public Dictionary<String, Partido> infoPartidos { get; }
        public string name { get; }

        public CustomEventArgsMain(Dictionary<string, Partido> infoPartidos, string name)
        {
            this.infoPartidos = infoPartidos;
            this.name = name;
        }
    }

    public class CustomEventArgsAddParty : EventArgs
    {
        public string name { get;  }
        public int votos {  get; }
        public Color Color { get; }

        public CustomEventArgsAddParty(string name, int votos, Color color)
        {
            this.name = name;
            this.votos = votos;
            Color = color;
        }
    }


}
