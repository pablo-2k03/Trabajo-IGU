using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pactometro
{
    


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


    public class CustomEventArgsCompare : EventArgs
    {

        public List<Eleccion> elecciones { get; }

        public CustomEventArgsCompare(List<Eleccion> elecciones)
        {
            this.elecciones = elecciones;
        }
    }


}
