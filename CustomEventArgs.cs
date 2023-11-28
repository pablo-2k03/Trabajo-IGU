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

        public List<Eleccion> elecciones { get; }

        public CustomEventArgs(List<Eleccion> elecciones)
        {
            this.elecciones = elecciones;
        }
    }

    public class CustomEventArgsPartidos: EventArgs
    {
        public List<Partido> partidosParaCoalicion { get; }

        public CustomEventArgsPartidos (List<Partido> partidosParaCoalicion)
        {
            this.partidosParaCoalicion = partidosParaCoalicion;
        }
    }


}
