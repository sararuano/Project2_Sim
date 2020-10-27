using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;  //posem el timer d'aquesta manera
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BibliotecaCristal;
using System.IO;
using Microsoft.Win32;

namespace Amplicacion
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Cristal cris;
        int steps;
        DispatcherTimer clock_time;
        List<Parametros> listaParametros;
        Parametros selectedParametros;
        bool CC_temp_constant;     //determinará las condiciones de contorno (true=temperatura constante ; true=contorno reflector)
        StackPanel[,] pan;


        public MainWindow()
        {
            InitializeComponent();

            //definim el primer dia
            steps = 1;
            step_box.Content = Convert.ToString(steps);

            //rejilla
            fillNewListParametros(true, new List<Parametros>());
            CreateDataGridyCristal(Rejilla, 15);
            pan = new StackPanel[Rejilla.RowDefinitions.Count(), Rejilla.RowDefinitions.Count()];
            ListBoxCC.Items.Add("Constant Temperature");
            ListBoxCC.Items.Add("Reflective Boundary");
            CC_temp_constant = false; //determinamos que por defecto la simulación tendrá contorno reflector

            paintInitialT();
            createTempIndicator(100);

            clock_time = new DispatcherTimer();
            clock_time.Tick += new EventHandler(clock_time_Tick);
            clock_time.Interval = new TimeSpan(10000000); //Pongo por defecto que haga un tick cada 1 segundo
            
        }

        //BOTONES

        //    Localiza de la lista de posibles parametros cuál es el clicado, lo selecciona (selectedParametros) 
        //    y llama a la funcion SetTextParametros que los escribe abajo
        private void ListParametros_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedParametro = ListBoxParametros.SelectedItem.ToString();
            foreach (Parametros par in listaParametros)
            {
                if (par.GetName() == selectedParametro)
                {
                    //Select new Par
                    selectedParametros = par;
                    SetTextParametros(par);
                }
            }
        }

        // Abre la ventana que perite seleccionar crear una nueva coleccion de parametros
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewParametrosWindow parametrosVentana = new NewParametrosWindow(listaParametros.Count() + 1);
            parametrosVentana.ShowDialog();
            if (parametrosVentana.GetError() == true)
                MessageBox.Show("The creation of a new set of parameters was cancelled");
            else
            {
                listaParametros.Add(parametrosVentana.GetParmetros());
                ListBoxParametros.Items.Add(parametrosVentana.GetParmetros().GetName());
                MessageBox.Show("The set of parameters " + (listaParametros.Count).ToString() + " was created");
            }
        }

        //Cada vez que se clica en step, pasa un delta_t
        private void Button_Click_Step(object sender, RoutedEventArgs e)
        {
            steps = steps + 1;
            step_box.Content = Convert.ToString(steps);

            double eps = selectedParametros.GetEpsilon();
            double m = selectedParametros.Getm();
            double alpha = selectedParametros.GetAlpha();
            double delta = selectedParametros.GetDelta();
            cris.NextDay(eps, m, alpha, delta);
            paintInitialT();

        }

        //Cuando se clica en AUTO, el timer se enciende, cuando se vuelve a clicar, se para
        private void Auto_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToString(Auto_Button.Content) == "AUTO")
            {
                clock_time.Start();
                Auto_Button.Content = "STOP";
            }
            else
            {
                clock_time.Stop();
                Auto_Button.Content = "AUTO";
            }
        }

        //Para ralentizar
        private void Slow_Button_Click(object sender, RoutedEventArgs e)
        {
            clock_time.Interval = clock_time.Interval + TimeSpan.FromMilliseconds(200);
        }

        //Para acelerar
        private void Fast_Button_Click(object sender, RoutedEventArgs e)
        {
            if (clock_time.Interval.TotalMilliseconds > 1000)
            {
                clock_time.Interval = clock_time.Interval - TimeSpan.FromMilliseconds(500);
            }
            else
            {
                if(clock_time.Interval.TotalMilliseconds > 400)
                {
                    clock_time.Interval = clock_time.Interval - TimeSpan.FromMilliseconds(200);
                }
                else
                {
                    if (clock_time.Interval.TotalMilliseconds > 10)
                    {
                        clock_time.Interval = clock_time.Interval - TimeSpan.FromMilliseconds(10);
                    }
                }
            }
        }
        //Per tornar a començar
        private void Restart_Button_Click(object sender, RoutedEventArgs e)
        {
            //Tornem a començar el load
            steps = 1;
            step_box.Content = Convert.ToString(steps);

            //rejilla
            fillNewListParametros(true, new List<Parametros>());
            CreateDataGridyCristal(Rejilla, 15);
            pan = new StackPanel[Rejilla.RowDefinitions.Count(), Rejilla.RowDefinitions.Count()];
            paintInitialT();
        }

        //Cambia el tamaño de las celdas
        private void Change_Size_Button_Click_(object sender, RoutedEventArgs e)
        {
            if (textGridSize.Text != "")
            {
                int valor = Convert.ToInt16(textGridSize.Text);
                if (valor % 2 == 1)
                {
                    CreateDataGridyCristal(Rejilla, Convert.ToInt16(textGridSize.Text));
                    pan = new StackPanel[Rejilla.RowDefinitions.Count(), Rejilla.RowDefinitions.Count()];
                    paintInitialT();
                }
                else { MessageBox.Show("Para poder asegurar la simetría del cristal las dimensiones tienen que ser impares."); }
                textGridSize.Text = "";
                
            }
        }

        //Solidifica la celda del medio
        private void Put_Grain_Button_Click(object sender, RoutedEventArgs e)
        {
            int dimension = Rejilla.RowDefinitions.Count();
            int ij = dimension / 2;
            cris.Solidificar(ij, ij);
            paintInitialT();
        }

        // Asigna un valor de temperatura a una celda concreta al presionar el boton
        private void Solidify_Button_Click(object sender, RoutedEventArgs e)
        {
            int filas = Rejilla.RowDefinitions.Count - 1;
            int i;
            int j;
            i = Convert.ToInt16(textXS.Text);
            j = Convert.ToInt16(textYS.Text);
            
            if (j < Rejilla.RowDefinitions.Count() && j >= 0 && i < Rejilla.RowDefinitions.Count() && i >= 0)
            {
                
                cris.GetCeldaij(i, j).SetTemperature(0);
                cris.GetCeldaij(i, j).SetPhase(0);
                SetColorTemp(0, i, j);
                
                textXS.Text = "";
                textYS.Text = "";
            }
            else
                MessageBox.Show("Limit values are RowIndex: [0," + filas.ToString() + "], ColumnIndex: [0," + filas.ToString() + "] and T: [-1,0]. Check them!");

        }

        //**VACÍA** Lo que pasaría si abriesemos la consola
        private void OpenConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            Celda una_cela = new Celda();
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "";
            saveFileDialog.Title = "Save text Files";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
                
            //StreamWriter un_archivo = new StreamWriter("Documento");

            double posx = Convert.ToInt32(una_cela.GetX()); //fila
            double posy = Convert.ToInt32(una_cela.GetY()); //columna
            double Temp = una_cela.GetTemperature();

            if (saveFileDialog.ShowDialog() == true)
            {
                text_save.Text = (Convert.ToString(Rejilla.RowDefinitions.Count()) + "\r\n");
                foreach (Parametros para in listaParametros)
                {
                    text_save.Text += (para.GetName() + "\r\n");
                    text_save.Text += (para.GetEpsilon() + "\r\n");
                    text_save.Text += (para.Getm() + "\r\n");
                    text_save.Text += (para.GetDelta() + "\r\n");
                    text_save.Text += (para.GetAlpha() + "\r\n");
                    text_save.Text += (para.GetDeltaSpace() + "\r\n");
                    text_save.Text += (para.GetDeltaTime() + "\r\n");
                }

                //int iiirow = 0;
                //foreach (Celda cel in cris.GetRow(iiirow))
                //{
                //    posy = cel.GetX();
                //    //posy = Convert.ToInt32(cris.GetCeldaij(iiirow, 0).GetX());
                //    text_save.Text += Convert.ToString(posy + "\r\n");
                //    iiirow++;
                //}
                //int iiicol = 0;
                //foreach (Celda cel in cris.GetCol(iiicol))
                //{
                //    posx = cel.GetY(); ;
                //    text_save.Text += Convert.ToString(posx + "\r\n");
                //    iiicol++;
                //}
                int iiirow = 0;
                foreach (RowDefinition row in Rejilla.RowDefinitions)
                {
                    int iiicol = 0;
                    foreach (ColumnDefinition col in Rejilla.ColumnDefinitions)
                    {
                        text_save.Text += Convert.ToString(cris.GetRow(iiicol));
                        iiicol++;
                    }
                    iiirow++;
                }
                    //if (j < Rejilla.RowDefinitions.Count() && j >= 0 && i < Rejilla.RowDefinitions.Count() && i >= 0)
                    //{
                    //    int selected = TempSelection.SelectedIndex;
                    //    if (selected == 0)
                    //    {
                    //        posx = Convert.ToInt32(cris.GetCelda(i, j).GetX());
                    //        posy = Convert.ToInt32(cris.GetCelda(i, j).GetY());

                    //    }
                    //    else if (selected == 1)
                    //    {
                    //        posx = 0;
                    //        posy = Convert.ToInt32(cris.GetCelda(i, j).GetY());
                    //    }
                    //    else
                    //    {
                    //        posx = Convert.ToInt32(cris.GetCelda(i, j).GetX());
                    //        posy = 0;
                    //        i = i + 1;
                    //        j = j + 1;
                    //    }
                    //    text_save.Text += (posx, posy, T);
                    //}


                    File.WriteAllText(saveFileDialog.FileName, text_save.Text);
                //if (j < Rejilla.RowDefinitions.Count() && j >= 0 && i < Rejilla.RowDefinitions.Count() && i >= 0 && T <= 0 && T >= -1)
                //{
                //    T = Convert.ToInt32(cris.GetCelda(i, j).GetTemperature());
                //    if (T != 0 && T != -1)
                //    {
                //        un_archivo.Write(Convert.ToString(T));
                //    }
                //    else
                //    {
                //        j = j + 1;
                //        i = i + 1;
                //    }
                //}
                //foreach (RowDefinition row in Rejilla.RowDefinitions)
                //{
                //    foreach(ColumnDefinition col in Rejilla.ColumnDefinitions)
                //    {
                //        double temp = cris.GetCelda(Convert.ToDouble(row), Convert.ToDouble(col)).GetTemperature();

                //    }
                //}
                //if (j < Rejilla.RowDefinitions.Count() && j >= 0 && i < Rejilla.RowDefinitions.Count() && i >= 0)

                //File.WriteAllText(saveFileDialog.FileName, "hola");
            }
        }

        private void ListCC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedCC = ListBoxCC.SelectedItem.ToString();
            if(selectedCC == "Constant Temperature")
            {
                CC_temp_constant = true;
            }
            else if(selectedCC== "Reflective Boundary")
            {
                CC_temp_constant = false;
            }
           
        }

        private void text_save_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {

        }


        //**************************************************************************************************

        //EVENTOS

        //Cada vez que passa un tick
        public void clock_time_Tick(object sender, EventArgs e)
        {
            steps = steps + 1;
            step_box.Content = Convert.ToString(steps);

            double eps = selectedParametros.GetEpsilon();
            double m = selectedParametros.Getm();
            double alpha = selectedParametros.GetAlpha();
            double delta = selectedParametros.GetDelta();
            cris.NextDay(eps, m, alpha, delta);
            paintInitialT();
        }

        // Escribe los índices de la celda clicada
        private void Rejilla_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double width = Convert.ToDouble(Rejilla.Width);
            int filas = Rejilla.ColumnDefinitions.Count();
            double widthCasilla = width / filas;
            double x = Math.Round(width-Convert.ToDouble(e.GetPosition(Rejilla).Y), 3);
            double y = Math.Round(Convert.ToDouble(e.GetPosition(Rejilla).X), 3);
            double temperature = cris.GetCeldaij(Convert.ToInt32(Math.Round(((x - widthCasilla / 2) / widthCasilla), 0)), Convert.ToInt32(Math.Round(((y - widthCasilla / 2) / widthCasilla), 0))).GetTemperature();
            double phase = cris.GetCeldaij(Convert.ToInt32(Math.Round(((x - widthCasilla / 2) / widthCasilla), 0)), Convert.ToInt32(Math.Round(((y - widthCasilla / 2) / widthCasilla), 0))).GetPhase();

            textX.Text = Math.Round(((x - widthCasilla / 2) / widthCasilla), 0).ToString();
            textY.Text = Math.Round(((y - widthCasilla / 2) / widthCasilla), 0).ToString();
            textTemp.Text = temperature.ToString();
            textPhase.Text = phase.ToString();
        }

        // **INACABADA** Esconde y muestra un panel u otro en funcion de que opcion temp/phase se haya seleccionado en el combobox
        private void TempPhaseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = TempPhaseBox.SelectedIndex;
            if (index == 0)
            {



            }
            else if (index == 1)
            {


            }
            else { }
        }

        //******************************************************************************************************

        //FUNCIONES DE LOS PARÁMETROS

        // Default == true pondra los valores por defecto y listPar no se utilizara ya que estara vacío, 
        // si Default == false, usara los vaalores de listPar para llenar la lista y el listBox
        public void fillNewListParametros(bool Default, List<Parametros> listPar)
        {
            ListBoxParametros.Items.Clear();
            listaParametros = new List<Parametros>();
            if (Default == true)
            {
                listaParametros.Add(new Parametros("Parámetros 1", 0.005, 20, 0.5, 400));
                listaParametros.Add(new Parametros("Parámetros 2", 0.005, 30, 0.7, 300));
                selectedParametros = listaParametros[0];
                SetTextParametros(selectedParametros);
            }
            else
                listaParametros = listPar;

            foreach (Parametros par in listaParametros)
            {
                ListBoxParametros.Items.Add(par.GetName());
            }
            TempPhaseBox.Items.Add("Temperature");
            TempPhaseBox.Items.Add("Phase");


        }

        // Escribe los parametros en los  textbox para la clase de Parametros dada
        private void SetTextParametros(Parametros par)
        {
            textSelectedPar.Text = par.GetName();
            textEpsilon.Text = par.GetEpsilon().ToString();
            textM.Text = par.Getm().ToString();
            textDelta.Text = par.GetDelta().ToString();
            textAlpha.Text = par.GetAlpha().ToString();
            textDeltaSpace.Text = par.GetDeltaSpace().ToString();
            textDeltaTime.Text = par.GetDeltaTime().ToString();
        }

        //******************************************************************************************************

        //FUNCIONES DE LA MALLA

        private Grid CreateDataGridyCristal(Grid Rej, int filas)
        {
            Rej.Children.Clear();
            Rej.ColumnDefinitions.Clear();
            Rej.RowDefinitions.Clear();
            //Define the grid
            int count = 0;
            while (count < filas)
            {
                Rej.ColumnDefinitions.Add(new ColumnDefinition());
                count++;
            }
            int count2 = 0;
            while (count2 < filas)
            {
                Rej.RowDefinitions.Add(new RowDefinition());
                count2++;
            }
            cris = new Cristal(filas);
            return Rej;

        }

        // Añade un stackpanel a cada celda del grid seleccionado y la pinta del color seleccionado
        private Grid CreateGridPanel(Color color)
        {
            int filas = Rejilla.RowDefinitions.Count();

            int irow = 0;
            foreach (RowDefinition row in Rejilla.RowDefinitions)
            {
                int icol = 0;
                foreach (ColumnDefinition col in Rejilla.ColumnDefinitions)
                {
                    StackPanel pan1 = new StackPanel();
                    Brush paint = new SolidColorBrush(color);

                    pan1.Background = paint;

                    pan[irow, icol] = pan1;

                    Grid.SetRow(pan1, irow);
                    Grid.SetColumn(pan1, icol);

                    pan1.Margin = new Thickness(1);
                    Rejilla.Children.Add(pan1);
                    icol++;
                }
                irow++;

            }
            return Rejilla;
        }

        //*********************************************************************************************************

        //FUNCIONES DE LA TEMPERATURA

        // Creara un nuevo grid, en el que conservará los valores anteriores de la rejilla 
        //que estan guardados en la matriz de stackpaneal llamada pan y para la fila y columna 
        //eleccionada creara un stackpanel nuevo con la temperatura deseada
        private void SetColorTemp(double temp, int fila, int columna)
        {
            int filas = Rejilla.RowDefinitions.Count();
            fila = filas - 1 - fila;
            Rejilla.Children.Clear();

            byte R = Convert.ToByte(Math.Round(-1 * temp * 255, 0));
            Color colorset = Color.FromArgb(255, 255, R, 0);
            Brush colorBrush = new SolidColorBrush(colorset);
            int irow = 0;
            foreach (RowDefinition row in Rejilla.RowDefinitions)
            {
                int icol = 0;
                foreach (ColumnDefinition col in Rejilla.ColumnDefinitions)
                {
                    if (columna == icol && fila == irow)
                    {
                        StackPanel panel = new StackPanel();
                        panel.Background = colorBrush;
                        pan[irow, icol] = panel;
                        Grid.SetRow(panel, irow);
                        Grid.SetColumn(panel, icol);
                        Rejilla.Children.Add(panel);
                    }
                    else
                    {
                        StackPanel panel = new StackPanel();
                        Brush brus;
                        if (pan[irow, icol] == null)
                            brus = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                        else
                        {
                            brus = pan[irow, icol].Background;
                        }
                        panel.Background = brus;
                        panel.Visibility = Visibility.Visible;
                        Grid.SetRow(panel, irow);
                        Grid.SetColumn(panel, icol);
                        Rejilla.Children.Add(panel);
                    }
                    icol++;
                }
                irow++;
            }

            // Pendiente de quitar si se pudiese
            int iirow = 0;
            foreach (RowDefinition row in HuecoRejilla.RowDefinitions)
            {
                int iicol = 0;
                foreach (ColumnDefinition col in HuecoRejilla.ColumnDefinitions)
                {
                    if (0 == iicol && 0 == iirow)
                    {
                        HuecoRejilla.Children.Clear();
                        Grid.SetRow(Rejilla, iirow);
                        Grid.SetColumn(Rejilla, iicol);
                        HuecoRejilla.Children.Add(Rejilla);
                    }
                    iicol++;
                }
                iirow++;
            }

            cris.GetCeldaij(filas-1 - fila, columna).SetTemperature(temp);
        }

        //Barre todos los valores de la matriz cristal y pone el color de la temperatura a las celdas 
        private void paintInitialT()
        {
            int filas = Rejilla.RowDefinitions.Count();
            
            Rejilla.Children.Clear();

            
            int irow = 0;
            foreach (RowDefinition row in Rejilla.RowDefinitions)
            {
                int icol = 0;
                foreach (ColumnDefinition col in Rejilla.ColumnDefinitions)
                {
                    double temp = cris.GetCeldaij(irow, icol).GetTemperature();
                    if (temp <-1)
                    {
                        temp = -1;
                    }
                    byte R = Convert.ToByte(Math.Round(-1 * temp * 255, 0));
                    Color colorset = Color.FromArgb(255, 255, R, 0);
                    Brush colorBrush = new SolidColorBrush(colorset);

                    StackPanel panel = new StackPanel();
                    panel.Background = colorBrush;
                    pan[irow, icol] = panel;
                    Grid.SetRow(panel, filas-1-irow);
                    Grid.SetColumn(panel, icol);
                    Rejilla.Children.Add(panel);
                    icol++;
                }
                irow++;
            }

            // Pendiente de quitar si se pudiese
            int iirow = 0;
            foreach (RowDefinition row in HuecoRejilla.RowDefinitions)
            {
                int iicol = 0;
                foreach (ColumnDefinition col in HuecoRejilla.ColumnDefinitions)
                {
                    if (0 == iicol && 0 == iirow)
                    {
                        HuecoRejilla.Children.Clear();
                        Grid.SetRow(Rejilla, iirow);
                        Grid.SetColumn(Rejilla, iicol);
                        HuecoRejilla.Children.Add(Rejilla);
                    }
                    iicol++;
                }
                iirow++;
            }
        }

        //Crea el indicadoor de temperatura de la derecha 
        private void createTempIndicator(int filas)
        {
            int count = 0;
            while (count < filas)
            {
                TempIndicator.RowDefinitions.Add(new RowDefinition());
                count++;
            }
            count = 0;
            double temp = 0;
            foreach (RowDefinition row in TempIndicator.RowDefinitions)
            {
                Double filasD = Convert.ToDouble(filas);
                StackPanel panel = new StackPanel();
                byte R = Convert.ToByte(Math.Round(255 + temp * 255, 0));
                Color colorset = Color.FromArgb(255, 255, R, 0);
                panel.Background = new SolidColorBrush(colorset);
                Grid.SetRow(panel, count);
                TempIndicator.Children.Add(panel);
                count++;
                temp = temp - 1 / filasD;
            }

        }


    }
}
