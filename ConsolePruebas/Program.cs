using System;
using BibliotecaCristal;

namespace ConsolePruebas
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            double eps = 0.005;
            double m = 20;
            double delta = 0.5;
            double aplha = 400;

            Cristal cristalpruebas = new Cristal(5);

            cristalpruebas.Solidificar(2, 2);
            cristalpruebas.NextDay(eps, m, aplha, delta);
            Console.WriteLine("Phase (0.005,0): " + cristalpruebas.GetCelda(0, 0).GetPhase());
            Console.WriteLine("Temperature (0.005,0): " + cristalpruebas.GetCelda(0, 0).GetTemperature());
            Console.ReadKey();
        }
    }
}
