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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BibliotecaCristal;


namespace Amplicacion
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Cristal cris;
        List<Parametros> listaParametros;
        Parametros selectedParametros;
        StackPanel[,] pan;

        public MainWindow()
        {
            InitializeComponent();
            fillNewListParametros(true, new List<Parametros>());
            CreateDataGridyCristal(Rejilla, 15);
            pan = new StackPanel[Rejilla.RowDefinitions.Count(), Rejilla.RowDefinitions.Count()];
            paintInitialT();
            createTempIndicator(200);
        }

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

        // Abre la ventana que perite seleccionar crear una nueva coleccion de poarametros
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

        private void Button_Click_Step(object sender, RoutedEventArgs e)
        {
            double eps = selectedParametros.GetEpsilon();
            double m = selectedParametros.Getm();
            double alpha = selectedParametros.GetAlpha();
            double delta = selectedParametros.GetDelta();
            cris.NextDay(eps, m, alpha, delta);
            paintInitialT();
        }



        private Grid CreateDataGridyCristal(Grid Rej, int filas)
        {
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

        // Escribe los índices de la celda clicada
        private void Rejilla_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double width = Convert.ToDouble(Rejilla.Width);
            int filas = Rejilla.ColumnDefinitions.Count();
            double widthCasilla = width / filas;
            double x = Math.Round(Convert.ToDouble(e.GetPosition(Rejilla).X), 3);
            double y = Math.Round(width - Convert.ToDouble(e.GetPosition(Rejilla).Y), 3);

            textX.Text = Math.Round(((x - widthCasilla / 2) / widthCasilla), 0).ToString();
            textY.Text = Math.Round(((y - widthCasilla / 2) / widthCasilla), 0).ToString();
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
        // Econde y muestra un panel u otro en funcion de que opcion temp/phase se haya seleccionado en el combobox
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
        // Creara un nuevo grid, en el que conservará los valores anteriores de la rejilla 
        //que estan guardados en la matriz de stackpaneal llamada pan y para la fila y columna 
        //eleccionada creara un stackpanel nuevo con la temperatura deseada
        private void SetColorTemp(double temp, int fila, int columna)
        {
            int filas = Rejilla.RowDefinitions.Count();
            fila = filas - 1 - fila;
            Rejilla.Children.Clear();

            byte R = Convert.ToByte(Math.Round(-1 * temp * 255, 0));
            Color colorset = Color.FromArgb(100, 255, R, 0);
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

            cris.GetCeldaij(14 - fila, columna).SetTemperature(temp);
        }
        // Asigna un valor de temperatura a una celda concreta al presionar el boton
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int i = Convert.ToInt16(textXS.Text);
            int j = Convert.ToInt16(textYS.Text);
            // Es lo como he conseguido que te converta a doule un string que tiene un menos
            double T = 0;
            string Tstr = (textTS.Text);
            char[] Tchar = Tstr.ToCharArray();
            int count = 0;
            double neg = 1;
            foreach (char pos in Tchar)
            {

                if (Tchar[0] == '-' && neg == 1 && count == 0)
                {
                    count = count - 1;
                    neg = -1;
                }
                else { }
                if (count == 0)
                {
                    string posstr = pos.ToString();
                    T = T + Convert.ToDouble(posstr);
                }
                else if (count == 1 && Tchar[count - 1] == '0')
                { }
                else if (count > 1)
                {
                    string posstr = pos.ToString();
                    T = T + Convert.ToDouble(posstr) / (10 ^ (count - 1));
                }
                else if (Tstr == "0")
                {
                    T = 0;
                    break;
                }
                else if (Tstr == "-1")
                {
                    T = -1;
                    neg = 1;
                    break;
                }
                else { }
                count++;

            }
            T = T * neg;
            // 
            if (T != 0 && T != -1)
                T = Math.Round(T, count - 2);
            int filas = Rejilla.RowDefinitions.Count - 1;
            if (j < Rejilla.RowDefinitions.Count() && j >= 0 && i < Rejilla.RowDefinitions.Count() && i >= 0 && T <= 0 && T >= -1)
            {
                SetColorTemp(T, i, j);
                textXS.Text = "";
                textYS.Text = "";
                textTS.Text = "";
            }
            else
                MessageBox.Show("Limit values are RowIndex: [0," + filas.ToString() + "], ColumnIndex: [0," + filas.ToString() + "] and T: [-1,0]. Check them!");

        }
        //Barre todos los valores de la matriz cristal y pone el color de la temperatura a las celdas 
        private void paintInitialT()
        {
            int i = 0;
            while (i < Rejilla.RowDefinitions.Count())
            {
                Celda[] fila = cris.GetRow(i);
                int j = 0;
                foreach (Celda cell in fila)
                {
                    SetColorTemp(cell.GetTemperature(), i, j);
                    j++;
                }
                i++;
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
                Color colorset = Color.FromArgb(100, 255, R, 0);
                panel.Background = new SolidColorBrush(colorset);
                Grid.SetRow(panel, count);
                TempIndicator.Children.Add(panel);
                count++;
                temp = temp - 1 / filasD;
            }

        }

        private void OpenConsoleButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
        
}
