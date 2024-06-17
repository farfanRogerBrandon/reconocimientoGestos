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
using System.Windows.Shapes;
using System.Net.Http;
using System.Threading;

namespace FInalProject_PDI
{
    /// <summary>
    /// Lógica de interacción para ImageFilters.xaml
    /// </summary>
    public partial class ImageFilters : Window
    {
        FilterInfoCollection cams;
        VideoCaptureDevice currentCam;
        #region oculto jeje
        string url = "http://localhost:5000/recognize_gesture";

        #endregion
        string gesture = "";

        bool takeImages = false;

        bool isMessageBoxActive = false;

        const string actionSave = "save";
        const string actionQuit = "quit";

        string actionMessage = "";

        string conta = "";

        string pathSave = "D:\\PDI\\imagesResult\\";

        string respuestaMessageBox = "";

        List<string> imagesToProcess = new List<string>();

        Bitmap fileToChange;
        Bitmap bitmap;

        BitmapImage ImageToProcess = new BitmapImage();
        BitmapImage ImageResult = new BitmapImage();

        QuestionWindow questionWindow;

        Gestures GESTURE = Gestures.NOTHING;

        System.Windows.Visibility visible = System.Windows.Visibility.Collapsed;

        public ImageFilters()
        {
            InitializeComponent();

            cams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in cams)
            {
                cmbCameras.Items.Add(VideoCaptureDevice.Name);
            }
            if (cmbCameras.Items.Count > 0)
            {
                cmbCameras.SelectedIndex = 0;
            }

            if (currentCam != null)
            {
                currentCam.Stop();
            }

            currentCam = new VideoCaptureDevice(cams[cmbCameras.SelectedIndex].MonikerString);
            currentCam.NewFrame += new NewFrameEventHandler(MyNewFrame);

            currentCam.VideoResolution = currentCam.VideoCapabilities[0];
            currentCam.Start();

            takeImages = true;

            LoadImages();

            imgToProcess.Source = new BitmapImage(new Uri(imagesToProcess[0]));
            imgProcessed.Source = new BitmapImage(new Uri(imagesToProcess[0]));

            bitmap = BitmapImage2Bitmap((BitmapImage)imgToProcess.Source);
        }

        void MyNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap myaux = (Bitmap)eventArgs.Frame.Clone();
            BitmapImage bim = ToBitmapImage(myaux);

            fileToChange = myaux;

            if (takeImages)
            {
                MyDetector();
                takeImages = false;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                imgVideo.Source = bim;
                hola.Text = GESTURE.ToString();
                myPb.Visibility = visible;
                contador.Text = conta;
            }));
        }

        void MyDetector()
        {
            ThreadStart delegado = new ThreadStart(ProcessMyImage);
            Thread hilo = new Thread(delegado);
            hilo.Start();
        }

        private async void ProcessMyImage()
        {
            visible = System.Windows.Visibility.Visible;
            conta = "Capturando gesto en 3 ";
            Thread.Sleep(1000);
            conta = "Capturando gesto en 2 ";
            Thread.Sleep(1000);
            conta = "Capturando gesto en 1 ";
            Thread.Sleep(1000);
            conta = "0 ";
            gesture = "Procesando Gesto";

            using (HttpClient client = new HttpClient())
            using (var content = new MultipartFormDataContent())
            {
                try
                {
                    byte[] imageBytes = ConvertBitmapToBytes(fileToChange);
                    var imageContent = new ByteArrayContent(imageBytes);
                    content.Add(imageContent, "image", "image.jpg");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    GESTURE = GetGesture(responseBody);

                    if (isMessageBoxActive)
                    {
                        if (actionMessage == actionSave)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                switch (GESTURE)
                                {
                                    case Gestures.Ok:
                                        isMessageBoxActive = false;
                                        questionWindow.Close();




                                        //if (!Directory.Exists(pathSave))
                                        //{
                                        //    Directory.CreateDirectory(pathSave);
                                        //}
                                        //string path = System.IO.Path.Combine(pathSave, name);
                                        //BitmapImage bitmapImage = (BitmapImage)imgProcessed.Source;
                                        //Bitmap bitmap = BitmapImage2Bitmap(bitmapImage);
                                        //bitmap.Save(path, ImageFormat.Jpeg);


                                        break;
                                    case Gestures.NotOk:
                                        isMessageBoxActive = false;
                                        questionWindow.Close();
                                        break;
                                    default:
                                        break;
                                }
                            });
                        }
                        else if (actionMessage == actionQuit)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                switch (GESTURE)
                                {
                                    case Gestures.Ok:
                                        isMessageBoxActive = false;
                                        questionWindow.Close();
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            MainWindow mainWindow = new MainWindow();
                                            mainWindow.Show();
                                            this.Close();
                                        });
                                        break;
                                    case Gestures.NotOk:
                                        isMessageBoxActive = false;
                                        questionWindow.Close();
                                        break;
                                    default:
                                        break;
                                }
                            });
                        }
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            switch (GESTURE)
                            {
                                case Gestures.Open:
                                    ApplyFilter(new AForge.Imaging.Filters.BrightnessCorrection(50));
                                    break;
                                case Gestures.AFinger:
                                    ApplyFilter(new AForge.Imaging.Filters.SaturationCorrection(0.5f));
                                    break;
                                case Gestures.TwoFinger:
                                    ChangeImageToEdit();
                                    bitmap = BitmapImage2Bitmap((BitmapImage)imgToProcess.Source);
                                    break;
                                case Gestures.Rock:
                                    ApplyFilter(new AForge.Imaging.Filters.ContrastCorrection(50));
                                    break;
                                case Gestures.Ok:
                                    actionMessage = actionSave;
                                    isMessageBoxActive = true;
                                    questionWindow = new QuestionWindow($"¿Desea guardar la imagen?");
                                    questionWindow.Show();
                                    break;
                                case Gestures.NotOk:
                                    ApplyFilter(new AForge.Imaging.Filters.Grayscale(0.3, 0.59, 0.11));
                                    break;
                                case Gestures.Close:
                                    actionMessage = actionQuit;
                                    questionWindow = new QuestionWindow("¿Desea salir al menu principal?");
                                    questionWindow.Show();
                                    isMessageBoxActive = true;
                                    break;
                                case Gestures.NONE:
                                    break;
                                case Gestures.NOTHING:
                                    break;
                                default:
                                    break;
                            }
                        });
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"Error al hacer la solicitud HTTP: {e.Message}");
                }
            }
            visible = System.Windows.Visibility.Collapsed;
            ProcessMyImage();
        }

        void ChangeImageToEdit()
        {
            int index = imagesToProcess.IndexOf(((BitmapImage)imgToProcess.Source).UriSource.LocalPath);
            if (index == imagesToProcess.Count - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
            imgToProcess.Source = new BitmapImage(new Uri(imagesToProcess[index]));
            imgProcessed.Source = new BitmapImage(new Uri(imagesToProcess[index]));
        }

        public static byte[] ConvertBitmapToBytes(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
        }

        void ApplyFilter(AForge.Imaging.Filters.IFilter filter)
        {
            Bitmap newImage = filter.Apply(bitmap);
            BitmapImage bitmapImage = ToBitmapImage(newImage);
            imgProcessed.Source = bitmapImage;
        }

        Gestures GetGesture(string gest)
        {
            if (gest.ToLower().Contains("pointing"))
            {
                return Gestures.AFinger;
            }
            if (gest.ToLower().Contains("open"))
            {
                return Gestures.Open;
            }
            if (gest.ToLower().Contains("iloveyou"))
            {
                return Gestures.Rock;
            }
            if (gest.ToLower().Contains("close"))
            {
                return Gestures.Close;
            }
            if (gest.ToLower().Contains("none"))
            {
                return Gestures.NONE;
            }
            if (gest.ToLower().Contains("gesto"))
            {
                return Gestures.NOTHING;
            }
            if (gest.ToLower().Contains("victory"))
            {
                return Gestures.TwoFinger;
            }
            if (gest.ToLower().Contains("up"))
            {
                return Gestures.Ok;
            }
            if (gest.ToLower().Contains("down"))
            {
                return Gestures.NotOk;
            }
            return Gestures.NOTHING;
        }

        public static BitmapImage ToBitmapImage(System.Drawing.Image bitmap)
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

        public static Bitmap BitmapImage2Bitmap(BitmapImage img)
        {
            using (var memory = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(img));
                enc.Save(memory);
                Bitmap btm = new Bitmap(memory);
                return new Bitmap(btm);
            }
        }

        private void LoadImages()
        {
            #region LoadImages
            string projectDirectory = System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
            projectDirectory = projectDirectory.Substring(0, projectDirectory.IndexOf("bin\\Debug"));

            string imagesFolder = System.IO.Path.Combine(projectDirectory, "Images\\forEdit\\");
            string[] files = Directory.GetFiles(imagesFolder);
            foreach (string file in files)
            {
                if (file.EndsWith(".jpg\"") || file.EndsWith(".png\"") || file.EndsWith(".jpeg\""))
                {
                    continue;
                }
                imagesToProcess.Add(file);
            }
            #endregion


            #region atigu
            /*
                         #region LoadImages

            //imagesToProcess.Add("pack://application:,,,/Images/forEdit/cow.jpg");
            //imagesToProcess.Add("pack://application:,,,/Images/forEdit/satured.jpg");
            //imagesToProcess.Add("pack://application:,,,/Images/forEdit/boat.jpg");
            //imagesToProcess.Add("pack://application:,,,/Images/forEdit/frog.jpg");

            string projectDirectory = System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
            projectDirectory = projectDirectory.Substring(0, projectDirectory.IndexOf("bin\\Debug"));

            string imagesFolder = System.IO.Path.Combine(projectDirectory, "Images\\forEdit\\");
            string[] files = Directory.GetFiles(imagesFolder);
            foreach (string file in files)
            {
                //verificar que el archivo sea una imagen
                if (file.EndsWith(".jpg\"") || file.EndsWith(".png\"") || file.EndsWith(".jpeg\""))
                    { continue; }

                imagesToProcess.Add(file);
            }



            //imgToProcess.Source = new BitmapImage(new Uri("pack://application:,,,/Images/forEdit/cow.jpg"));

            //string imagePath = "pack://application:,,,/FinalProject_PDI;component/Images/forEdit/cow.jpg";

            #endregion
             
             
            */
            #endregion
        }
        private void SaveImageEdit()
        {
            #region guardado jiji
            string name = "image" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";


            string projectDirectory = System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
            projectDirectory = projectDirectory.Substring(0, projectDirectory.IndexOf("bin\\Debug"));

            string imagesFolder = System.IO.Path.Combine(projectDirectory, "Images\\result\\");


            bitmap.Save(imagesFolder + name, ImageFormat.Jpeg);
            #endregion
        }
    }
    enum Gestures
    {
        Open,
        AFinger,
        TwoFinger,
        Rock,
        Ok,
        NotOk,
        Close,
        NONE,
        NOTHING
    }
}
