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

            int count = 0;
            int ciclos = 4; 
            Cristal cristalpruebas = new Cristal(5);
            cristalpruebas.Solidificar(2, 2);
            while (count <= ciclos)
            {
                if (count == 0)
                { }
                else
                    cristalpruebas.NextDay(eps, m, aplha, delta);

                Console.WriteLine("Ciclo " + (count).ToString() + " / 0,0_______ Phase: " + cristalpruebas.GetCelda(0, 0).GetPhase() + " Tempe: " + cristalpruebas.GetCelda(0, 0).GetTemperature());
                Console.WriteLine("       " + " / 1,0_______ Phase: " + cristalpruebas.GetCelda(-0.005, 0).GetPhase() + " Tempe: " + cristalpruebas.GetCelda(0.005, 0).GetTemperature());
                Console.WriteLine("       "+ " / 2,0_______ Phase: " + cristalpruebas.GetCelda(-0.005 * 2, 0).GetPhase() + " Tempe: " + cristalpruebas.GetCelda(0.005 * 2, 0).GetTemperature());
                Console.WriteLine();
                count++;
            }
        }
    }
}
