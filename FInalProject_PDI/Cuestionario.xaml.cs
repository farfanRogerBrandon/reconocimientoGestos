using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace FInalProject_PDI
{
    /// <summary>
    /// Lógica de interacción para Cuestionario.xaml
    /// </summary>
    public partial class Cuestionario : Window
    {
        private string conta;
        private VideoCaptureDevice currentCam;
        private int currentQuestionIndex = 0;
        private int score = 0;
        private string[] questions = new string[]
        {
        "¿Está satisfecho con la app?",
        "¿Le parece útil la app?",
        "¿Recomendaría la app a otros?",
        "¿La interfaz es amigable?",
        "¿Volvería a usar la app?"
        };
        public Cuestionario(VideoCaptureDevice camera)
        {
            InitializeComponent();
            currentCam = camera;
            //conta = contador; // Asigna el contador recibido
            StartCamera();
            DisplayNextQuestion();
            //UpdateContador();
        }
        private void StartCamera()
        {
            if (currentCam != null)
            {
                currentCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
                currentCam.Start();
            }
        }

        private void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                imgVideo.Source = ToBitmapImage(bitmap);
            }));
        }

        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (currentCam != null)
            {
                currentCam.SignalToStop();
                currentCam.WaitForStop();
                currentCam = null;
            }
        }

        public void HandleGesture(Gestures gesture)
        {
            if (gesture == Gestures.Ok)
            {
                score++;
                //MessageBox.Show("Satisfecho", "Respuesta");
                DisplayNextQuestion();
            }
            else if (gesture == Gestures.NotOk)
            {
                //MessageBox.Show("Desacuerdo", "Respuesta");
                DisplayNextQuestion();
            }
        }

        private void DisplayNextQuestion()
        {
            if (currentQuestionIndex < questions.Length)
            {
                txb_cuestionario.Text = questions[currentQuestionIndex];
                currentQuestionIndex++;
            }
            else
            {
                MessageBox.Show($"Cuestionario finalizado. Puntuación: {score}/{questions.Length}", "Resultado");
                this.Close();
                Application.Current.MainWindow.Show();
            }
        }


        
    }
}
