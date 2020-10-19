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
using System.Windows.Shapes;
using BibliotecaCristal;

namespace Amplicacion
{
    /// <summary>
    /// Lógica de interacción para NewParametrosWindow.xaml
    /// </summary>
    public partial class NewParametrosWindow : Window
    {
        int num;
        Parametros par;
        bool error = false;
        public NewParametrosWindow(int num)
        {
            InitializeComponent();
            this.num = num;
            textName.Text = "Parámetros " + num.ToString();

        }
        public Parametros GetParmetros()
        {
            return par;
        }
        public bool GetError()
        {
            return error;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textEpsilon.Text != "" && textM.Text != "" && textDelta.Text != "" && textAlpha.Text != "" && textDelta.Text != "" && textDeltaSpace.Text != "" && textDeltaTime.Text != "")
                {
                    par = new Parametros(textName.Text, Convert.ToDouble(textEpsilon.Text), Convert.ToDouble(textM.Text), Convert.ToDouble(textDelta.Text), Convert.ToDouble(textAlpha.Text), Convert.ToDouble(textDeltaSpace.Text), Convert.ToDouble(textDeltaTime.Text));
                    this.Close();               
                }
                else
                {
                    MessageBox.Show("Cannot create a set of parameters with null values");
                }
            }
            catch
            {
                MessageBox.Show("Parameters has to be setted as numerical values");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            error = true;
            this.Close();
        }
    }
}
