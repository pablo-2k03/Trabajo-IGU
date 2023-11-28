using System;
using System.Windows;

namespace Pactometro
{
    public static class Utils
    {
        public static class DatosGraficasWindowSingleton
        {
            private static DatosGraficas _instance = null;

            public static DatosGraficas GetInstance()
            {
                if (_instance == null)
                {
                    _instance = new DatosGraficas();
                    _instance.Closed += (sender, e) => _instance = null;
                }
                return _instance;
            }

            public static Boolean ExistsInstance() => _instance != null;
        }


        public static class AddDataWindowSingleton
        {
            private static AddData _instance = null;

            public static AddData GetInstance()
            {
                if (_instance == null)
                {
                    _instance = new AddData();
                    _instance.Closed += (sender, e) => _instance = null;
                }
                return _instance;
            }
        }

        public static class DataModelSingleton
        {
            private static ModeloDatos _instance = null;
            public static ModeloDatos GetInstance()
            {
                _instance ??= new ModeloDatos();
                return _instance;
            }
        }

        public static class UpdateDataSingleton
        {
            private static UpdateData _instance = null;

            public static UpdateData GetInstance()
            {
                if(_instance == null)
                {
                    _instance= new UpdateData();
                    _instance.Closed += (sender, e) => _instance = null;

                }
                return _instance;
            }
        }

        public static class CoalicionSingleton
        {
            private static Coalicion _instance = null;

            public static Coalicion GetInstance(Eleccion e )
            {
                if (_instance == null)
                {
                    _instance = new Coalicion(e);
                    _instance.Closed += (sender, e) => _instance = null;

                }
                return _instance;
            }
        }


    }
}
