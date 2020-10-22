using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            CreateDataGridyCristal(15);
            SetColorTemp(-1, 1, 1);

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
        private void CreateDataGridyCristal(int filas)
        {
            //Define the grid
            int count = 0;
            while (count < filas)
            {
                Rejilla.ColumnDefinitions.Add(new ColumnDefinition());
                RejillaT.ColumnDefinitions.Add(new ColumnDefinition());
                RejillaP.ColumnDefinitions.Add(new ColumnDefinition());
                RejillaVacia.ColumnDefinitions.Add(new ColumnDefinition());
                count++;

            }
            int count2 = 0;
            while (count2 < filas)
            {
                Rejilla.RowDefinitions.Add(new RowDefinition());
                RejillaT.RowDefinitions.Add(new RowDefinition());
                RejillaP.RowDefinitions.Add(new RowDefinition());
                RejillaVacia.RowDefinitions.Add(new RowDefinition());
                count2++;
            }

            cris = new Cristal(filas);
            Color color = Color.FromArgb(100, 50, 50, 50);
            CreateGridPanel(RejillaT, color);
            CreateGridPanel(RejillaP, color);


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
        private void CreateGridPanel(Grid RejillaGrid, Color color)
        {
            int filas = Rejilla.RowDefinitions.Count();
            pan = new StackPanel[filas, filas];
            int irow = 0;
            foreach (RowDefinition row in RejillaGrid.RowDefinitions)
            {
                int icol = 0;
                foreach (ColumnDefinition col in RejillaGrid.ColumnDefinitions)
                {
                    StackPanel pan1 = new StackPanel();
                    pan1.Background = new SolidColorBrush(color);
                    pan[irow, icol]=pan1;
                    Grid.SetRow(pan1, irow);
                    Grid.SetColumn(pan1, icol);
                    pan1.Margin = new Thickness(1);
                    RejillaGrid.Children.Add(pan1);
                    icol++;
                }
                irow++;
            }
        }
        // Econde y muestra un panel u otro en funcion de que opcion temp/phase se haya seleccionado en el combobox
        private void TempPhaseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = TempPhaseBox.SelectedIndex;
            if (index == 0)
            {
                
                Rejilla = RejillaT;
                RejillaT.Visibility = Visibility.Hidden;
                Rejilla.Visibility = Visibility.Visible;

            }
            else if (index==1)
            {
                
                Rejilla = RejillaP;
                RejillaP.Visibility = Visibility.Hidden;
                Rejilla.Visibility = Visibility.Visible;
            }
            else { }
        }
        private void SetColorTemp(double temp, int fila, int columna)
        {
            int filas = Rejilla.RowDefinitions.Count();
            StackPanel[,] pan1 = new StackPanel[filas,filas];
            byte R = Convert.ToByte(Math.Round(255 + temp * 255,0));
            Color color = Color.FromArgb(100,255, R, 0);
            int irow = 0;
            Grid grid = RejillaVacia;
            foreach (RowDefinition row in grid.RowDefinitions)
            {
                int icol = 0;
                foreach (ColumnDefinition col in grid.ColumnDefinitions)
                {
                    if (columna == icol && fila == irow)
                    {
                        StackPanel pan2 = new StackPanel();
                        pan2.Background = new SolidColorBrush(color);
                        pan1[irow, icol] = pan2;
                        Grid.SetRow(pan2, irow);
                        Grid.SetColumn(pan2, icol);
                        grid.Children.Add(pan2);
                    }
                    else
                    {

                        StackPanel pan3 = new StackPanel();
                        pan3.Background=pan[irow, icol].Background;
                        pan1[irow, icol] = pan[irow, icol];
                        Grid.SetRow(pan3, irow);
                        Grid.SetColumn(pan3, icol);
                        grid.Children.Add(pan3);

                    }
                    icol++;
                }
                irow++; 
            }
            pan = pan1;
            Rejilla = grid;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SetColorTemp(-1, 1, 1);
        }
    }
}
