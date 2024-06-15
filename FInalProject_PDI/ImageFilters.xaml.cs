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
        //direccion de las imagenes
        //C:\Users\rf924\OneDrive\Imágenes\pdi

        FilterInfoCollection cams;
        VideoCaptureDevice currentCam;

        string url = "http://localhost:5000/recognize_gesture";
        string gesture = "";

        bool takeImages = false;

        bool isMessageBoxActive = false;


        

        string conta = "";



        string pathSave = "D:\\PDI\\imagesResult\\";

        string respuestaMessageBox = "";


        List<string> imagesToProcess = new List<string>();
        //agregar las imagenes a la lista

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


            imagesToProcess.Add("C:\\Users\\rf924\\OneDrive\\Imágenes\\pdi\\cow.jpg");
            imagesToProcess.Add("C:\\Users\\rf924\\OneDrive\\Imágenes\\pdi\\satured.jpg");
            imagesToProcess.Add("C:\\Users\\rf924\\OneDrive\\Imágenes\\pdi\\boat.jpg");
            imagesToProcess.Add("C:\\Users\\rf924\\OneDrive\\Imágenes\\pdi\\frog.jpg");
            imgToProcess.Source = new BitmapImage(new Uri(imagesToProcess[0]));

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
                // imgVideo2.Source = bim;
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
            //conta = "Capturando gesto en 6 ";

            //Thread.Sleep(1000);
            //conta = "Capturando gesto en 5 ";
            //Thread.Sleep(1000);
            //conta = "Capturando gesto en 4 ";
            //Thread.Sleep(1000);

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
                    // MessageBox.Show(  responseBody);

                    

                    GESTURE = GetGesture(responseBody);
                    //Bitmap bitmap = BitmapImage2Bitmap((BitmapImage)imgToProcess.Source);
                    //hacer un switch para mostrar el mensaje de acuerdo al gesto
                    if (isMessageBoxActive)
                    {
                        this.Dispatcher.Invoke(() =>
                        {





                            switch (GESTURE)
                            {
                                case Gestures.Ok:

                                    isMessageBoxActive = false;
                                    questionWindow.Close();
                                    //Mostrar un messagebox para confirmar si se desea guardar la imagen

                                    //Guardar la imagen en la carpeta de imagenes D:\PDI\imagesResult
                                    string name = "image" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                                    bitmap.Save(pathSave + name, ImageFormat.Jpeg);
                                    //MessageBox.Show("Imagen guardada en: " + path + name, "Imagen guardada");


                                    break;
                                case Gestures.NotOk:
                                    //MessageBox.Show("You selected: Not Ok", "Gesture");
                                    isMessageBoxActive = false;
                                    questionWindow.Close();
                                    break;
                                default:
                                    break;

                            }
                        });

                    }
                    else
                    {

                        this.Dispatcher.Invoke(() =>
                        {
                            switch (GESTURE)
                            {
                                case Gestures.Open:
                                    //aplicar un filtro con afore a la imagen

                                    AForge.Imaging.Filters.BrightnessCorrection filter = new AForge.Imaging.Filters.BrightnessCorrection(50);

                                    //aplicar el filtro

                                    Bitmap newImage = filter.Apply(bitmap);
                                    //convertir la imagen a BitmapImage
                                    BitmapImage bitmapImage = ToBitmapImage(newImage);
                                    //asignar la imagen a imgToProcess.Source
                                    //arreglar el error: System.InvalidOperationException: 'El subproceso que realiza la llamada no puede obtener acceso a este objeto porque el propietario es otro subproceso.'
                                    imgProcessed.Source = bitmapImage;


                                    
                                    break;
                                case Gestures.AFinger:
                                    //MessageBox.Show("You selected: A Finger", "Gesture");
                                    //aplicar un filtro con afore a la imagen
                                    AForge.Imaging.Filters.SaturationCorrection filter2 = new AForge.Imaging.Filters.SaturationCorrection(0.5f);
                                    //aplicar el filtro
                                    Bitmap newImage2 = filter2.Apply(bitmap);
                                    //convertir la imagen a BitmapImage
                                    BitmapImage bitmapImage2 = ToBitmapImage(newImage2);
                                    //asignar la imagen a imgToProcess.Source
                                    imgProcessed.Source = bitmapImage2;


                                    break;
                                case Gestures.TwoFinger:

                                    ChangeImageToEdit();
                                    bitmap = BitmapImage2Bitmap((BitmapImage)imgToProcess.Source);

                                    break;
                                case Gestures.Rock:

                                    AForge.Imaging.Filters.ContrastCorrection filter3 = new AForge.Imaging.Filters.ContrastCorrection(50);
                                    Bitmap newImage3 = filter3.Apply(bitmap);
                                    BitmapImage bitmapImage3 = ToBitmapImage(newImage3);
                                    imgProcessed.Source = bitmapImage3;

                                    break;
                                case Gestures.Ok:

                                    isMessageBoxActive = true;
                                    questionWindow = new QuestionWindow($"Desea guardar la imagen? en: {pathSave}");
                                    questionWindow.Show();


                                    break;
                                case Gestures.NotOk:
                                    //Aplicar un filtro de escala de grises a la imagen
                                    AForge.Imaging.Filters.Grayscale filter4 = new AForge.Imaging.Filters.Grayscale(0.3, 0.59, 0.11);
                                    //aplicar el filtro
                                    Bitmap newImage4 = filter4.Apply(bitmap);
                                    //convertir la imagen a BitmapImage
                                    BitmapImage bitmapImage4 = ToBitmapImage(newImage4);

                                    //asignar la imagen a imgToProcess.Source
                                    imgProcessed.Source = bitmapImage4;


                                    break;
                                case Gestures.Close:
                                    //salir de esta ventana

                                    //questionWindow = new QuestionWindow("Desea salir de esta ventana?");
                                    //questionWindow.Show();
                                    //isMessageBoxActive = true;
                                    break;
                                case Gestures.NONE:
                                    //MessageBox.Show("You selected: NONE", "Gesture");
                                    break;
                                case Gestures.NOTHING:
                                    //MessageBox.Show("You selected: NOTHING", "Gesture");
                                    break;
                                default:
                                    //MessageBox.Show("Unknown gesture", "Gesture");
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
            //alternar en la lista de imagenes, si ya no hay mas imagenes, regresar a la primera
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
            Bitmap aux = new Bitmap(bitmap);
            using (MemoryStream stream = new MemoryStream())
            {
                aux.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
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


        void MAKEACTION(Gestures gesture)
        {
            switch (gesture)
            {
                case Gestures.Open:
                    MessageBox.Show("You selected: Open", "Gesture");
                    break;
                case Gestures.AFinger:
                    MessageBox.Show("You selected: A Finger", "Gesture");
                    break;
                case Gestures.TwoFinger:
                    MessageBox.Show("You selected: Two Finger", "Gesture");
                    break;
                case Gestures.Rock:
                    MessageBox.Show("You selected: Rock", "Gesture");
                    break;
                case Gestures.Ok:
                    MessageBox.Show("You selected: Ok", "Gesture");
                    break;
                case Gestures.NotOk:
                    MessageBox.Show("You selected: Not Ok", "Gesture");
                    break;
                case Gestures.Close:
                    MessageBox.Show("You selected: Close", "Gesture");
                    break;
                case Gestures.NONE:
                    MessageBox.Show("You selected: NONE", "Gesture");
                    break;
                case Gestures.NOTHING:
                    MessageBox.Show("You selected: NOTHING", "Gesture");
                    break;
                default:
                    MessageBox.Show("Unknown gesture", "Gesture");
                    break;
            }
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
