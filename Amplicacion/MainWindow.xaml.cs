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
using Syncfusion.UI.Xaml;

namespace Amplicacion
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Cristal cris;
        
        int steps;
        DispatcherTimer clock_time = new DispatcherTimer();
        List<Parametros> listaParametros;
        Parametros selectedParametros;
        bool CC_temp_constant;     //determinará las condiciones de contorno (true=temperatura constante ; true=contorno reflector)
        string show_grid;          //determinará qué malla aparece, temperatura o fase
        StackPanel[,] pan;
        ViewModel chartE;
        List<PruebaChart> listChart;


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
            show_grid = "temperatura";


            
            // Sobre el chart
            listChart = new List<PruebaChart>();
            listChart.Add(new PruebaChart { timeChart = 1, casillasT = 0, casillasP = 0 });



            paintInitialT();

            clock_time = new DispatcherTimer();
            clock_time.Tick += new EventHandler(clock_time_Tick);
            clock_time.Interval = new TimeSpan(10000000); //Pongo por defecto que haga un tick cada 1 segundo

        }

        //BOTONES

        //    Localiza de la lista de posibles parametros cuál es el clicado, lo selecciona (selectedParametros) 
        //    y llama a la funcion SetTextParametros que los escribe abajo

        private void ListBoxParametros_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            cris.NextDay(eps, m, alpha, delta, CC_temp_constant);
            paintInitialT();
            añadirAlChart(cris.CalulateAverageT(), cris.CalulateAverageP());
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
                paintInitialT();

                textXS.Text = "";
                textYS.Text = "";
            }
            else
                MessageBox.Show("Limit values are RowIndex: [0," + filas.ToString() + "], ColumnIndex: [0," + filas.ToString() + "] and T: [-1,0]. Check them!");

        }


        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "";
            saveFileDialog.Title = "Save text Files";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                foreach (PruebaChart unachart in listChart)
                {
                    text_save.Text = (Convert.ToString(unachart.timeChart) + ' ' + Convert.ToString(unachart.casillasT) + ' ' + Convert.ToString(unachart.casillasP) + "\r\n");
                }
                text_save.Text += (Convert.ToString(TempPhaseBox.SelectedIndex) + "\r\n");
                text_save.Text += (Convert.ToString(Rejilla.RowDefinitions.Count()) + "\r\n");
                text_save.Text += (Convert.ToString(steps) + "\r\n");
                foreach (Parametros para in listaParametros)
                {
                    text_save.Text += (para.GetName() + ' ' + para.GetEpsilon() + ' ' + para.Getm() + ' ' + para.GetDelta() + ' ' + para.GetAlpha() + "\r\n");
                }

                int iiirow = 0;
                double p = 0;
                foreach (RowDefinition row in Rejilla.RowDefinitions)
                {
                    int iiicol = 0;
                    foreach (ColumnDefinition col in Rejilla.ColumnDefinitions)
                    {
                        double otrax = Convert.ToDouble(value: cris.GetCeldaij(iiirow, iiicol).GetY());
                        double otray = Convert.ToDouble(value: cris.GetCeldaij(iiirow, iiicol).GetX());
                        double temper = Convert.ToDouble(value: cris.GetCeldaij(iiirow, iiicol).GetTemperature());
                        double phase = Convert.ToDouble(value: cris.GetCeldaij(iiirow, iiicol).GetPhase());

                        text_save.Text += (Convert.ToString(otrax) + ' ' + Convert.ToString(otray) + ' ' + Convert.ToString(temper) + ' ' + Convert.ToString(phase) + "\r\n");

                        iiicol++;
                        p++;
                    }
                    iiirow++;
                }
                File.WriteAllText(saveFileDialog.FileName, text_save.Text);
                MessageBox.Show("S'ha guardat tot correctament");
            }
        }

        private void Load_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            string line;
            if (openFileDialog.ShowDialog() == true)
            {
                var fileStream = openFileDialog.OpenFile();
                StreamReader reader = new StreamReader(fileStream);
                int contador = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] trozos = line.Split(' ');
                    if (contador == 0)
                    {
                        foreach (PruebaChart otrachart in listChart)
                        {
                            double temp = Convert.ToDouble(trozos[0]);
                            double casillasT = Convert.ToDouble(trozos[1]);
                            double casillasP = Convert.ToDouble(trozos[2]);
                            temp = otrachart.timeChart;
                            casillasT = otrachart.casillasT;
                            casillasP = otrachart.casillasP;
                        }
                    }
                    if (contador == 1)
                    {
                        int indeex = Convert.ToInt32(trozos[0]);
                        if (indeex == 0)
                        {
                            show_grid = "temperatura";
                            TempPhaseBox.SelectedItem = "Temperature";
                        }
                        else if (indeex == 1)
                        {
                            show_grid = "fase";
                            TempPhaseBox.SelectedItem = "Phase";
                        }
                        else { }
                    }
                    if (contador == 2)
                    {
                        int rej = Convert.ToInt32(trozos[0]);

                        pan = new StackPanel[rej, rej];
                        CreateDataGridyCristal(Rejilla, rej);
                    }
                    if (contador == 3)
                    {
                        steps = Convert.ToInt32(trozos[0]);
                        step_box.Content = Convert.ToString(steps);
                    }
                    if (contador == 4)
                    {
                        string name_1 = (trozos[0] + ' ' + Convert.ToString(trozos[1]));
                        Parametros par_1 = new Parametros(name_1, Convert.ToDouble(trozos[2]), Convert.ToDouble(trozos[3]), Convert.ToDouble(trozos[4]), Convert.ToDouble(trozos[5]));
                        listaParametros.Add(par_1);
                        SetTextParametros(par_1);
                    }
                    if (contador == 5)
                    {
                        string name_2 = (trozos[0] + ' ' + Convert.ToString(trozos[1]));
                        Parametros par_2 = new Parametros(name_2, Convert.ToDouble(trozos[2]), Convert.ToDouble(trozos[3]), Convert.ToDouble(trozos[4]), Convert.ToDouble(trozos[5]));
                        listaParametros.Add(par_2);
                        SetTextParametros(par_2);
                    }
                    if (contador > 5)
                    {
                        cris.GetCristal();

                        cris.GetCelda(Convert.ToDouble(trozos[1]), Convert.ToDouble(trozos[0])).SetY(Convert.ToDouble(trozos[0]));
                        cris.GetCelda(Convert.ToDouble(trozos[1]), Convert.ToDouble(trozos[0])).SetX(Convert.ToDouble(trozos[1]));
                        cris.GetCelda(Convert.ToDouble(trozos[1]), Convert.ToDouble(trozos[0])).SetTemperature(Convert.ToDouble(trozos[2]));
                        cris.GetCelda(Convert.ToDouble(trozos[1]), Convert.ToDouble(trozos[0])).SetPhase(Convert.ToDouble(trozos[3]));
                    }
                    contador++;
                    paintInitialT();
                }

                if (Convert.ToBoolean(reader.ReadLine()) == true)
                {
                    Auto_Button.Content = "STOP";
                    clock_time.Start();
                }

                else
                {
                    Auto_Button.Content = "AUTO";
                }
            }
        }

        private void ListCC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedCC = ListBoxCC.SelectedItem.ToString();
            if (selectedCC == "Constant Temperature")
            {
                CC_temp_constant = true;
            }
            else if (selectedCC == "Reflective Boundary")
            {
                CC_temp_constant = false;
            }

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
            cris.NextDay(eps, m, alpha, delta, CC_temp_constant);
            paintInitialT();
            añadirAlChart(cris.CalulateAverageT(), cris.CalulateAverageP());
        }

        // Escribe los índices de la celda clicada
        private void Rejilla_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double width = Convert.ToDouble(Rejilla.Width);
            int filas = Rejilla.ColumnDefinitions.Count();
            double widthCasilla = width / filas;
            double x = Math.Round(width - Convert.ToDouble(e.GetPosition(Rejilla).Y), 3);
            double y = Math.Round(Convert.ToDouble(e.GetPosition(Rejilla).X), 3);
            double temperature = cris.GetCeldaij(Convert.ToInt32(Math.Round(((x - widthCasilla / 2) / widthCasilla), 0)), Convert.ToInt32(Math.Round(((y - widthCasilla / 2) / widthCasilla), 0))).GetTemperature();
            double phase = cris.GetCeldaij(Convert.ToInt32(Math.Round(((x - widthCasilla / 2) / widthCasilla), 0)), Convert.ToInt32(Math.Round(((y - widthCasilla / 2) / widthCasilla), 0))).GetPhase();

            textX.Text = Math.Round(((x - widthCasilla / 2) / widthCasilla), 0).ToString();
            textY.Text = Math.Round(((y - widthCasilla / 2) / widthCasilla), 0).ToString();
            textTemp.Text = temperature.ToString();
            textPhase.Text = phase.ToString();
        }

        //Esconde y muestra un panel u otro en funcion de que opcion temp/phase se haya seleccionado en el combobox
        private void TempPhaseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = TempPhaseBox.SelectedIndex;
            if (index == 0)
            {
                show_grid = "temperatura";
                paintInitialT();
                createTempIndicator(100);

            }
            else if (index == 1)
            {
                show_grid = "fase";
                paintInitialT();
                createTempIndicator(100);

            }
            else { }
        }

        //Cambia el valor del periodo entre steps
        private void SliderVelocity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double periodo = SliderVelocity.Value* 10000000;        //Se multiplica este factor porque el intervalo del reloj se mide en cientos de nanosegundos
            
            clock_time.Interval = new TimeSpan((long)periodo);
            
        }

        //**VACÍA**
        private void text_save_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {

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

        //**NO SE USA** Añade un stackpanel a cada celda del grid seleccionado y la pinta del color seleccionado
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

        //FUNCIONES DE LA TEMPERATURA//FASE

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

            cris.GetCeldaij(filas - 1 - fila, columna).SetTemperature(temp);
        }

        //Barre todos los valores de la matriz cristal y pone el color de la temperatura/fase a las celdas 
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
                    if (show_grid == "temperatura")
                    {
                        double temp = cris.GetCeldaij(irow, icol).GetTemperature();
                        if (temp < -1)
                        {
                            temp = -1;
                        }
                        byte R = Convert.ToByte(Math.Round(-1 * temp * 255, 0));
                        Color colorset = Color.FromArgb(255, 255, R, 0);
                        Brush colorBrush = new SolidColorBrush(colorset);

                        StackPanel panel = new StackPanel();
                        panel.Background = colorBrush;
                        pan[irow, icol] = panel;
                        Grid.SetRow(panel, filas - 1 - irow);
                        Grid.SetColumn(panel, icol);
                        Rejilla.Children.Add(panel);
                    }
                    else if (show_grid == "fase")
                    {
                        double phase = cris.GetCeldaij(irow, icol).GetPhase();
                        //if (phase > 1
                        //{
                        //    phase = 1;
                        //}
                        byte R = Convert.ToByte(Math.Round(phase * 255, 0));
                        Color colorset = Color.FromArgb(255, 0, R, 255);
                        Brush colorBrush = new SolidColorBrush(colorset);

                        StackPanel panel = new StackPanel();
                        panel.Background = colorBrush;
                        pan[irow, icol] = panel;
                        Grid.SetRow(panel, filas - 1 - irow);
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
        }

        //Crea el indicadoor de temperatura de la derecha 
        private void createTempIndicator(int filas)
        {
            TempIndicator.Children.Clear();
            int contar=TempIndicator.RowDefinitions.Count();
            int count = 0;
            if (TempIndicator.RowDefinitions.Count() > 0)
            { }
            else
            {
                while (count < filas)
                {
                    TempIndicator.RowDefinitions.Add(new RowDefinition());
                    count++;
                }
            }
            count = 0;
            if (show_grid == "temperatura")
            {
                double temp = 0;
                foreach (RowDefinition row in TempIndicator.RowDefinitions)
                {
                    contar = TempIndicator.RowDefinitions.Count();
                    Double filasD = Convert.ToDouble(filas);
                    StackPanel panel = new StackPanel();
                    byte R = Convert.ToByte(Math.Round(255+temp * 255, 0));
                    Color colorset = Color.FromArgb(255, 255, R, 0);
                    panel.Background = new SolidColorBrush(colorset);
                    Grid.SetRow(panel, count);
                    TempIndicator.Children.Add(panel);
                    count++;
                    temp = temp - 1 / filasD;
                }
                text0.Text = "-1 \nLiquid";
                text1.Text = "0 \nSolid";
            }
            else if (show_grid == "fase")
            {
                double fase = 0;
                foreach (RowDefinition row in TempIndicator.RowDefinitions)
                {
                    contar = TempIndicator.RowDefinitions.Count();
                    Double filasD = Convert.ToDouble(filas);
                    StackPanel panel = new StackPanel();
                    byte R = Convert.ToByte(Math.Round(255-fase * 255, 0));
                    Color colorset = Color.FromArgb(255, 0, R, 255);
                    panel.Background = new SolidColorBrush(colorset);
                    Grid.SetRow(panel, count);
                    TempIndicator.Children.Add(panel);
                    count++;
                    fase = fase + 1/filasD;
                }
                text0.Text = "1\nLiquid";
                text1.Text = "0\nSolid";
            }

        }





        public void añadirAlChart(double T, double P)
        {
            ViewModel chartE = new ViewModel();
            foreach (PruebaChart element in listChart)
            {
                chartE.Data.Add(element);
            }
            double count = listChart.Count();
            PruebaChart newPoint = new PruebaChart { timeChart = count, casillasT = T, casillasP = P };
            chartE.Data.Add(newPoint);
            listChart.Add(newPoint);
            seriesChartP.ItemsSource = chartE.Data;
            seriesChartT.ItemsSource = chartE.Data;
        }

    }
}
