using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ClassesCristal;

namespace ClassesCristal
{
    class Cristal
    {
        //ATRIBUTES
        Celda[,] cristal;
        double delta_space = 0.005;
        double delta_time = 1 * Math.Pow(10, -5);


        //CONSTRUCTORS

        //Constructor in case we create a new cristal, all cells are liquid. Number of cells must be specified
        public Cristal(int filas)
        {
            this.cristal = new Celda[filas, filas];
            int i = 0;
            while (i < filas)
            {
                int j = 0;
                while (j < filas)
                {
                    this.cristal[i, j].SetX((filas - 1) / 2 * (-delta_space) + delta_space * i);
                    this.cristal[i, j].SetY((filas - 1) / 2 * (-delta_space) + delta_space * j);
                    this.cristal[i, j].SetTemperature(-1);
                    this.cristal[i, j].SetPhase(1);
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
        public Celda GetCelda(int x, int y)
        {
            foreach (Celda cell in cristal)
            {
                if(cell.GetX() == x && cell.GetY() == y)
                {
                    return cell;
                }
            }
            return null;
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
            int i = 0;
            while (i < this.cristal.GetLength(0))
            {
                int j = 0;
                while (j < this.cristal.GetLength(0))
                {
                    double lapT = T_Laplatian(i, j);
                    double lapPh = Ph_Laplatian(i, j);
                    double deltap_t = this.cristal[i, j].Set_Diff_Phase(eps, m, alpha, delta, lapPh);
                    double deltaT_t = this.cristal[i, j].Set_Diff_Temperature(eps, m, alpha, delta, lapPh, lapT);
                    double newphase = this.cristal[i, j].GetPhase() + deltap_t * delta_time;
                    double newTemperature = this.cristal[i, j].GetTemperature() + deltaT_t * delta_time;
                    this.cristal[i, j].SetPhase(newphase);
                    this.cristal[i, j].SetTemperature(newTemperature);

                    j++;
                }
                i++;
            }

        }
         
        //MATH METHODS

        //Function to obtain the laplatian of the Temperature in a given cell
        public double T_Laplatian(int i, int j)
        {
            double Txx;     //Second derivative wrt x
            double Tyy;     //Second derivative wrt y
            //BC i=0 with edges
            if (i==0 && j==0)
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);  
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);  
            }
            if (i == 0 && j != 0 && j!=(this.cristal.GetLength(0)-1))
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2); 
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2); 
            }
            if (i == 0 && j == (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);  
                Tyy = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);  
            }
            //BC i = N-1 with edges
            if (i == (this.cristal.GetLength(0) - 1) && j == 0)
            {
                Txx = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2); 
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);  
            }
            if(i == (this.cristal.GetLength(0) - 1) && j != 0 && j != (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);  
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);  
            }
            if(i == (this.cristal.GetLength(0) - 1) && j == (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);
                Tyy = (this.cristal[i, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j - 1].GetTemperature()) / Math.Pow(delta_space, 2);  
            }
            //BC j=0 in the middle
            if (j == 0 && i != 0 && i != (this.cristal.GetLength(0) - 1))
            {
                Txx = (this.cristal[i + 1, j].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i - 1, j].GetTemperature()) / Math.Pow(delta_space, 2);  
                Tyy = (this.cristal[i, j + 1].GetTemperature() - 2 * this.cristal[i, j].GetTemperature() + this.cristal[i, j].GetTemperature()) / Math.Pow(delta_space, 2);  
            }
            //BC j=N-1 in the middle
            if(j == this.cristal.GetLength(0) && i != 0 && i != (this.cristal.GetLength(0) - 1))
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
            if (i == 0 && j != 0 && j != (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);  
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2); 
            }
            if (i == 0 && j == (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2); 
                Pyy = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2); 
            }
            //BC i = N-1 with edges
            if (i == (this.cristal.GetLength(0) - 1) && j == 0)
            {
                Pxx = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2); 
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);  
            }
            if (i == (this.cristal.GetLength(0) - 1) && j != 0 && j != (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);  
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2);  
            }
            if (i == (this.cristal.GetLength(0) - 1) && j == (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);  
                Pyy = (this.cristal[i, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j - 1].GetPhase()) / Math.Pow(delta_space, 2);  
            }
            //BC j=0 in the middle
            if (j == 0 && i != 0 && i != (this.cristal.GetLength(0) - 1))
            {
                Pxx = (this.cristal[i + 1, j].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i - 1, j].GetPhase()) / Math.Pow(delta_space, 2);
                Pyy = (this.cristal[i, j + 1].GetPhase() - 2 * this.cristal[i, j].GetPhase() + this.cristal[i, j].GetPhase()) / Math.Pow(delta_space, 2);  
            }
            //BC j=N-1 in the middle
            if (j == this.cristal.GetLength(0) && i != 0 && i != (this.cristal.GetLength(0) - 1))
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
