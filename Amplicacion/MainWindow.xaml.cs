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
        List<Parametros> listaParametros;

        public MainWindow()
        {
            InitializeComponent();

            
            fillNewListParametros(true, new List<Parametros>());
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
        // Default == true pondra los valores por defecto y listPar no se utilizara ya que estara vacío, 
        // si Default == false, usara los vaalores de listPar para llenar la lista y el listBox
        public void fillNewListParametros(bool Default, List<Parametros> listPar)
        {
            ListParametros.Items.Clear();
            listaParametros = new List<Parametros>();
            if (Default == true)
            {
                listaParametros.Add(new Parametros("Param. 1", 0.005, 20, 0.5, 400));
                listaParametros.Add(new Parametros("Param. 2", 0.005, 30, 0.7, 300));
            }
            else
                listaParametros = listPar;

            foreach (Parametros par in listaParametros)
            {
                ListParametros.Items.Add(par.GetName());
            }
        }
    }
}
