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
       
        public MainWindow()
        {
            InitializeComponent();
            fillNewListParametros(true, new List<Parametros>());
            CreateDataGridyCristal(15);
            
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
            textSelectedPar.Text=par.GetName();
            textEpsilon.Text=par.GetEpsilon().ToString();
            textM.Text = par.Getm().ToString();
            textDelta.Text=par.GetDelta().ToString();
            textAlpha.Text=par.GetAlpha().ToString();
            textDeltaSpace.Text=par.GetDeltaSpace().ToString();
            textDeltaTime.Text=par.GetDeltaTime().ToString();
        }

        // Abre la ventana que perite seleccionar crear una nueva coleccion de poarametros
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewParametrosWindow parametrosVentana = new NewParametrosWindow(listaParametros.Count()+1);
            parametrosVentana.ShowDialog();
            if (parametrosVentana.GetError() == true)
                MessageBox.Show("The creation of a new set of parameters was cancelled");
            else
            {
                listaParametros.Add(parametrosVentana.GetParmetros());
                ListBoxParametros.Items.Add(parametrosVentana.GetParmetros().GetName());
                MessageBox.Show("The set of parameters "+(listaParametros.Count).ToString()+" was created");
            }
        }
        private void CreateDataGridyCristal(int filas)
        {
            //Define the grid
            int count = 0;
            while (count < filas)
            {
                Rejilla.ColumnDefinitions.Add(new ColumnDefinition());
                count++;

            }
            int count2 = 0;
            while (count2 < filas)
            {
                Rejilla.RowDefinitions.Add(new RowDefinition());
                count2++;
            }
            cris = new Cristal(filas);
            Rejilla.Visibility = Visibility.Visible;
            SetTempRejilla(5,6);        }

        // Escribe los índices de la celda clicada
        private void Rejilla_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double width = Convert.ToDouble(Rejilla.Width);
            int filas = Rejilla.ColumnDefinitions.Count();
            double widthCasilla = width / filas;
            double x = Math.Round(Convert.ToDouble(e.GetPosition(Rejilla).X),3);
            double y = Math.Round(width-Convert.ToDouble(e.GetPosition(Rejilla).Y),3);
            
            textX.Text =Math.Round(((x-widthCasilla/2)/widthCasilla),0).ToString();
            textY.Text = Math.Round(((y - widthCasilla / 2) / widthCasilla), 0).ToString();
        }
        private void SetTempRejilla(int fila, int columna)
        {
            double width = Convert.ToDouble(Rejilla.Width);
            int filas = Rejilla.ColumnDefinitions.Count();
            Rectangle RecGrid = new Rectangle();
            
            double GridWidth=width / filas;
            RecGrid.Width = GridWidth;
            RecGrid.Height = RecGrid.Width;
            RecGrid.Visibility = Visibility.Visible;

            Point p = new Point(fila * GridWidth, width - columna * GridWidth);
            RecGrid.TranslatePoint(p,Rejilla);

        }
    }
}
