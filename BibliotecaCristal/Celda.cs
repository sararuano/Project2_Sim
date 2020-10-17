using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaCristal
{
    public class Celda
    {
        //ATRIBUTES
        double x;           //x-component
        double y;           //y-component
        double T;           //Temperature -1 if liquid and 0 if solid
        double phase;       //phase 1 if liquid and 0 solid

        //CONSTRUCTOR
        public Celda(double x, double y, double T, double phase)
        {
            this.x = x;
            this.y = y;
            this.T = T;
            this.phase = phase;
        }

        //METHODS TO OBTAIN VALUES
        public double GetX()
        {
            return this.x;
        }

        public double GetY()
        {
            return this.y;
        }

        public double GetTemperature()
        {
            return this.T;
        }

        public double GetPhase()
        {
            return this.phase;
        }

        //METHODS TO SET VALUES
        public void SetX(double x)
        {
            this.x = x;
        }
        public void SetY(double y)
        {
            this.y = y;
        }
        public void SetTemperature(double Temperature)
        {
            this.T = Temperature;
        }

        public void SetPhase(double Phase)
        {
            this.phase = Phase;
        }

        //METHODS

        //Compute the difference of phase wrt time
        public double Set_Diff_Phase(double eps, double m, double alpha, double delta, double p_laplatian)
        {
            double phase = this.GetPhase();
            double Set_Diff_Phase = (phase * (1 - phase) * (phase - 0.5 + 30 * eps * alpha * delta *phase* (1 - phase))) / (m * Math.Pow(eps, 2)) + p_laplatian/m;
            return Set_Diff_Phase;
        }

        //Compute the difference of temperature wrt time
        public double Set_Diff_Temperature(double eps, double m, double alpha, double delta, double p_laplatian, double T_laplatian)
        {
            double phase = this.GetPhase();
            double Diff_phase = this.Set_Diff_Phase(eps, m, alpha, delta, p_laplatian);
            double Set_Diff_Temperature = T_laplatian - 1 / delta * (30 * Math.Pow(phase, 2) - 60 * Math.Pow(phase, 3) + 30 * Math.Pow(phase, 4)) * Diff_phase;
            return Set_Diff_Temperature;
        }
    }
}
