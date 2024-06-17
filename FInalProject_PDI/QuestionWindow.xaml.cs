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

namespace FInalProject_PDI
{
    /// <summary>
    /// Lógica de interacción para QuestionWindow.xaml
    /// </summary>
    public partial class QuestionWindow : Window
    {
        public QuestionWindow()
        {
            InitializeComponent();
        }
        //agregar un constructor que reciba el texto de la pregunta
        public QuestionWindow(string question)
        {
            InitializeComponent();
            lbl_info.Content = question;
            //hacer que el texto este en negrita y centrado
            lbl_info.FontWeight = FontWeights.Bold;
            lbl_info.HorizontalContentAlignment = HorizontalAlignment.Center;
            lbl_info.FontSize = 20;
        }
    }
}
