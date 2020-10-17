using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BibliotecaCristal
{
    public class Cristal
    {
        //ATRIBUTES
        Celda[,] cristal;
        double delta_space = 0.005;
        double delta_time = 5 * Math.Pow(10, -6);


        //CONSTRUCTORS

        //Constructor in case we create a new cristal, all cells are liquid. Number of cells must be specified
        //_______________________________LA FILA DEBE SER IMPAR!!!!!
        public Cristal(int filas)
        {
            cristal = new Celda[filas, filas];
            int i = 0;
            while (i < filas)
            {
                int j = 0;
                while (j < filas)
                {
                    double x = Math.Round((filas - 1) * (-delta_space) / 2 + delta_space * i, 5);
                    double y = Math.Round((filas - 1) * (-delta_space) / 2 + delta_space * j, 5);
                    double temp = -1.0;
                    double phase = 1.0;
                    Celda cell = new Celda(x, y, temp, phase);
                    this.cristal[i, j] = cell;
                    j++;
                }
                i++;
            }
        }

        //Constructor in case we want to add a predefined cristal
        public Cristal(Celda[,] cristal)
        {
            this.cristal = cristal;
        }

        //METHODS

        //Obtain cell in cristal
        public Celda GetCelda(double x, double y)
        {
            int i = 0;
            while (i < cristal.GetLength(0))
            {
                foreach (Celda cell in this.GetRow(i))
                {
                    if (cell.GetX() == x && cell.GetY() == y)
                    {
                        return cell;
                    }
                }
                i++;
            }
            return null;
        }
        public Celda[] GetRow(int row)
        {

            Celda[] fila= new Celda[cristal.GetLength(0)];
            int i = 0;
            while (i < cristal.GetLength(0))
            {
                if (i == row)
                {
                    int j = 0;
                    while (j < cristal.GetLength(0))
                    {
                        fila[j] = cristal[i, j];
                        j++;
                    }
                    return fila;
                }
                i++;
            }
            return null;
        }
        public Celda[,] GetCristal()
        {
            return cristal;
        }
        //Function to solidify directly a specific cell, indexes must be specified
        public void Solidificar(int i, int j)
        {
            this.cristal[i, j].SetPhase(0);
            this.cristal[i, j].SetTemperature(0);
        }

        //What happens each time-step
        public void NextDay(double eps, double m, double alpha, double delta)
        {
            Celda[,] futurecristall = new Celda[this.cristal.GetLength(0), this.cristal.GetLength(0)];
            int i = 0;
            //int count = 0;
            while (i < futurecristall.GetLength(0))
            {
                int j = 0;
                while (j < futurecristall.GetLength(0))
                {
                    double x = this.cristal[i,j].GetX();
                    double y = this.cristal[i,j].GetY();

                    //double y = Math.Round((futurecristall.GetLength(0) - 1) / 2 * (-delta_space) + delta_space * j, 5);
                    double lapT = this.T_Laplatian(i, j);
                    double lapPh = this.Ph_Laplatian(i, j);
                    double deltap_t = this.cristal[i, j].Set_Diff_Phase(eps, m, alpha, delta, lapPh);
                    double deltaT_t = this.cristal[i, j].Set_Diff_Temperature(eps, m, alpha, delta, lapPh, lapT);
                    double newphase = this.cristal[i, j].GetPhase() + deltap_t * delta_time;
                    double newTemperature = this.cristal[i, j].GetTemperature() + deltaT_t * delta_time;
                    Celda cell = new Celda(x, y, Math.Round(newTemperature,10), Math.Round(newphase,10));
                    futurecristall[i, j] = cell;
                    //count++;
                    //Console.WriteLine(count.ToString()+ " i:"+i.ToString()+" j:"+j.ToString());
                    //Lo usé para encontar een que paso hay fallo, lo eliminamos al final por si se necesita
                    j++;
                }
                i++;
            }
            this.cristal = futurecristall;

        }

        //MATH METHODS

        //Function to obtain the laplatian of the Temperature in a given cell
        public double T_Laplatian(int i, int j)
        {
            double Txx;     //Second derivative wrt x
            double Tyy;     //Second derivative wrt y
            //BC i=0 with edges
            if (i == 0 && j == 0)
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            else if (i == 0 && j != 0 && j != (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            else if (i == 0 && j == (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            //BC i = N-1 with edges
            else if (i == (this.cristal.GetLength(0) - 1) && j == 0)
            {
                Txx = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            else if (i == (this.cristal.GetLength(0) - 1) && j != 0 && j != (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            else if (i == (this.cristal.GetLength(0) - 1) && j == (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            //BC j=0 in the middle
            else if (j == 0 && i != 0 && i != (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            //BC j=N-1 in the middle
            else if (j == (this.cristal.GetLength(0)-1) && i != 0 && i != (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            else
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);
            }
            double lap = Txx + Tyy;
            return lap;
            
        }


        //Function to obtain the laplatian of the Phase in a given cell
        public double Ph_Laplatian(int i, int j)
        {
            double Pxx;  //Second derivative wrt x
            double Pyy;  //Second derivative wrt y
            //BC i=0 with edges
            if (i == 0 && j == 0)
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);
            }
            else if (i == 0 && j != 0 && j != (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2);
            }
            else if (i == 0 && j == (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2);
            }
            //BC i = N-1 with edges
            else if (i == (this.cristal.GetLength(0) - 1) && j == 0)
            {
                Pxx = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);
            }
            else if (i == (this.cristal.GetLength(0) - 1) && j != 0 && j != (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2);
            }
            else if (i == (this.cristal.GetLength(0) - 1) && j == (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2);
            }
            //BC j=0 in the middle
            else if (j == 0 && i != 0 && i != (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);
            }
            //BC j=N-1 in the middle
            else if (j == (this.cristal.GetLength(0)-1) && i != 0 && i != (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2);
            }
            else
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2);
            }
            double lap = Pxx + Pyy;
            return lap;
        }

        //Function to obtain the space differential of a phase
        public double Set_Phase_Diff_X(int i, int j)
        {
            double dx = (this.cristal[i + 1, j].GetPhase() - this.cristal[i - 1, j].GetPhase()) / (2 * delta_space);
            return dx;
        }

        //Function to obtain the space differential of a Temperature
        public double Set_Temperature_Diff_X(int i, int j)
        {
            double dx = (this.cristal[i + 1, j].GetTemperature() - this.cristal[i - 1, j].GetTemperature()) / (2 * delta_space);
            return dx;
        }
    }
}
