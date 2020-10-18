using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaCristal
{
    class Parametros
    {
        //ATRIBUTES
        double eps;
        double m;
        double delta;
        double alpha;
        // En principio no cambian
        double delta_space;
        double delta_time;

        //METHODS

        // Si no se introduce delta_time ni delta_spice se dan por defecto
        public Parametros (double eps, double m,double delta, double alpha)
        {
            this.eps = eps;
            this.m = m;
            this.delta = delta;
            this.alpha = alpha;
            this.delta_space = 0.005;
            this.delta_time = 5 * Math.Pow(10, -6);
        }

        // Si se introduce delta_time ni delta_spice se les da un valor
        public Parametros(double eps, double m, double delta, double alpha, double delta_space, double delta_time)
        {
            this.eps = eps;
            this.m = m;
            this.delta = delta;
            this.alpha = alpha;
            this.delta_space = delta_space;
            this.delta_time = delta_time;
        }
    }
}
