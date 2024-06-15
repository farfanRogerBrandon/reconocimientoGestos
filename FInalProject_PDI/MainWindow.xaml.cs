using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Drawing;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using AForge.Video;
using AForge.Imaging.Filters;
using AForge;

namespace FInalProject_PDI
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public delegate BitmapImage MyCurrentFilter(Bitmap img);
    public partial class MainWindow : Window
    {
        string conta = "";
        FilterInfoCollection cams;

        VideoCaptureDevice currentCam;

        BitmapImage currentImg;


        string gesture = "";

        MyCurrentFilter MyFilt;


        Bitmap fileToChange;




        bool takeImages = false;

        int nImages = 0;

        System.Windows.Visibility visible = System.Windows.Visibility.Collapsed;



        Gestures GESTURE = Gestures.NOTHING;

        int nroIm = 0;



        public MainWindow()
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
            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            /*
            _graph = new Graph();
            _graph.Import(File.ReadAllBytes("E:/1-2024/PDI/hand_gesture_model.pb"));
            _session = new Session(_graph);*/

            //_graph = new TFGraph();
            //var modelBuffer = File.ReadAllBytes("E:/1-2024/PDI/saved_model.pb");
            //MessageBox.Show("Llego");
            ////     graph.Import(modelBuffer);


            //_graph.Import(modelBuffer);
            //_session = new TFSession(_graph);

            //string baseRoute = "E:/1-2024/PDI/GESTOS/";
            //for (int i = 30; i <= 32; i++)
            //{
            //    Bitmap openimg = new Bitmap(baseRoute + $"OpenHand/{i}.jpg");
            //    Bitmap normalImg = new Bitmap(baseRoute + $"Normal/{i}.jpg");
            //    Bitmap closeImg = new Bitmap(baseRoute + $"CloseHand/{i}.jpg");
            //    Bitmap f1img = new Bitmap(baseRoute + $"1Finger/{i}.jpg");
            //    Bitmap f2img = new Bitmap(baseRoute + $"2Finger/{i}.jpg");
            //    Bitmap f3img = new Bitmap(baseRoute + $"3Finger/{i}.jpg");
            //    Bitmap f4img = new Bitmap(baseRoute + $"4Finger/{i}.jpg");



            //    NormalImages.Add(normalImg);

            //    Open.Add(openimg);
            //    Close.Add(closeImg);
            //    F1.Add(f1img);
            //    F2.Add(f2img);
            //    F3.Add(f3img);
            //    F4.Add(f4img);



            //}


        }


        private void cmbCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {


            //SetNewFilter(cmbFilters.Text);
            if (currentCam != null)
            {
                currentCam.Stop();
            }

            currentCam = new VideoCaptureDevice(cams[cmbCameras.SelectedIndex].MonikerString);
            currentCam.NewFrame += new NewFrameEventHandler(MyNewFrame);


            currentCam.VideoResolution = currentCam.VideoCapabilities[5];
            currentCam.Start();


            takeImages = true;

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



        //private int DetectFingers(List<IntPoint> defects, AForge.Point center)
        //{
        //    int fingerCount = 0;
        //    foreach (IntPoint defect in defects)
        //    {
        //        double angle = Math.Atan2(defect.Y - center.Y, defect.X - center.X) * 180.0 / Math.PI;
        //        if (angle > 0) 
        //        {
        //            fingerCount++;
        //        }
        //    }
        //    return fingerCount;
        //}





      

        //void SetNewFilter(string text)
        //{
        //    switch (text)
        //    {
        //        case "Sepia":
        //            MyFilt = SepiaFilter;
        //            takeImages = true;
        //            break;
        //        case "Hue Modifier":
        //            MyFilt = HueModifier;
        //            break;

        //        case "Sal y Pimienta":

        //            MyFilt = SaltAndPepper;
        //            break;
        //        case "Sobel Edge Color":
        //            takeImages = true;

        //            MyFilt = SobelEdgeColor;
        //            break;
        //        case "Jitter":

        //            MyFilt = Jitter;
        //            break;
        //        case "Resaltar Bordes":

        //            MyFilt = kernel;
        //            break;
        //        default:
        //            MyFilt = ToBitmapImage;
        //            break;
        //    }
        //}









        // Método auxiliar para obtener el centroide de un blob
      

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // currentCam.Stop();
                ProcessMyImage();
            }
            catch (Exception ex)
            {

            }
        }








        #region Filtros
        public BitmapImage SobelEdgeColor(Bitmap img)
        {
            try
            {

                ColorFiltering filter = new ColorFiltering();

                filter.Red = new IntRange(45, 133);
                filter.Green = new IntRange(25, 106);
                filter.Blue = new IntRange(45, 130);

                filter.ApplyInPlace(img);
                //img = toGrayScaleAforge(img);

                //Threshold threshold = new Threshold(10);
                //img = threshold.Apply(img);



                //AForge.Imaging.Filters.SobelEdgeDetector filter2 = new AForge.Imaging.Filters.SobelEdgeDetector();
                //img = filter2.Apply(img);

                return ToBitmapImage(img);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("No se puede apllicar el filtro +" + ex.Message);
                return null;
            }
        }

        public BitmapImage SepiaFilter(Bitmap imagee)
        {
            try
            {
                // Bitmap grayImage = toGrayScaleAforge(imagee);
                // imagee = BitmapImage2Bitmap(imagee);



                ColorFiltering filter = new ColorFiltering();

                filter.Red = new IntRange(45, 123);
                filter.Green = new IntRange(25, 96);
                filter.Blue = new IntRange(45, 120);

                filter.ApplyInPlace(imagee);


                AForge.Imaging.Filters.Grayscale filter2 = new AForge.Imaging.Filters.Grayscale(0.3, 0.6, 0.1);
                System.Drawing.Bitmap newImage2 = filter2.Apply(imagee);

                Threshold threshold = new Threshold(10);

                newImage2 = threshold.Apply(newImage2);

                //newImage2 = new Invert().Apply(newImage2);
                //AForge.Imaging.Filters.Invert filter2 = new AForge.Imaging.Filters.Invert();
                //System.Drawing.Bitmap newImage2 = filter2.Apply(grayImage);






                return ToBitmapImage(newImage2);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("No se puede apllicar el filtro +" + ex.Message);
                return null;
            }
        }
        public BitmapImage SaltAndPepper(Bitmap imagee)
        {
            try
            {
                //Bitmap grayImage = toGrayScaleAforge(imagee);
                // Bitmap grayImage = BitmapImage2Bitmap(imagee);

                AForge.Imaging.Filters.SaltAndPepperNoise filter2 = new AForge.Imaging.Filters.SaltAndPepperNoise(23);
                System.Drawing.Bitmap newImage2 = filter2.Apply(imagee);

                return ToBitmapImage(newImage2);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("No se puede apllicar el filtro +" + ex.Message);
                return null;
            }
        }

        public BitmapImage HueModifier(Bitmap imagee)
        {
            try
            {
                //Bitmap grayImage = toGrayScaleAforge(imagee);
                //   Bitmap grayImage = BitmapImage2Bitmap(imagee);

                AForge.Imaging.Filters.HueModifier filter2 = new AForge.Imaging.Filters.HueModifier(150);
                System.Drawing.Bitmap newImage2 = filter2.Apply(imagee);

                return ToBitmapImage(newImage2);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("No se puede apllicar el filtro +" + ex.Message);
                return null;
            }
        }

        public BitmapImage kernel(Bitmap imagee)
        {
            try
            {
                //Bitmap grayImage = toGrayScaleAforge(imagee);
                // Bitmap grayImage = BitmapImage2Bitmap(imagee);

                int[,] kernel = {
            { -2, -1,  0 },
            { -1,  1,  1 },
            {  0,  1,  2 } };
                Convolution filter = new Convolution(kernel);
                System.Drawing.Bitmap newImage2 = filter.Apply(imagee);



                return ToBitmapImage(newImage2);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("No se puede apllicar el filtro +" + ex.Message);
                return null;
            }
        }


        public BitmapImage Jitter(Bitmap imagee)
        {
            try
            {
                //Bitmap grayImage = toGrayScaleAforge(imagee);
                // Bitmap grayImage = BitmapImage2Bitmap(imagee);

                AForge.Imaging.Filters.Jitter filter2 = new AForge.Imaging.Filters.Jitter(4);
                System.Drawing.Bitmap newImage2 = filter2.Apply(imagee);

                return ToBitmapImage(newImage2);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("No se puede apllicar el filtro +" + ex.Message);
                return null;
            }
        }


        Bitmap toGrayScaleAforge(Bitmap image)
        {
            // Bitmap image = BitmapImage2Bitmap(bitmapIm);
            AForge.Imaging.Filters.Grayscale filter = new AForge.Imaging.Filters.Grayscale(0.30, 0.59, 0.11);
            System.Drawing.Bitmap newImage = filter.Apply(image);


            return newImage;
        }

        #endregion

        /*  private void btnAddFilter_Click(object sender, RoutedEventArgs e)
          {
              SetNewFilter(cmbFilters.Text);
              if (currentCam == null) { return; }
              currentCam.Stop();
              currentCam.Start();
          }*/



        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                currentCam.Stop();
            }
            catch (Exception)
            {


            }
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
            conta = "Capturando gesto en 6 ";

            Thread.Sleep(1000);
            conta = "Capturando gesto en 5 ";
            Thread.Sleep(1000);
            conta = "Capturando gesto en 4 ";
            Thread.Sleep(1000);

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

                    HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/recognize_gesture", content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // MessageBox.Show(  responseBody);

                    GESTURE = GetGesture(responseBody);

                    MAKEACTION(GESTURE);
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"Error al hacer la solicitud HTTP: {e.Message}");
                }
            }
            visible = System.Windows.Visibility.Collapsed;


            ProcessMyImage();
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

        #region NOSI<rve
        //VectorOfInt hullIndices;
        //Mat defects;
        //private void ExtractContourAndHull(Mat skin)
        //{
        //    // Utiliza UMat para un mejor rendimiento si es posible
        //    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
        //    {
        //        // Encuentra contornos
        //        CvInvoke.FindContours(skin, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

        //        if (contours.Size > 0)
        //        {
        //            // Encuentra el contorno más grande
        //            double maxArea = 0;
        //            int maxContourIdx = 0;
        //            for (int i = 0; i < contours.Size; i++)
        //            {
        //                double contourArea = CvInvoke.ContourArea(contours[i]);
        //                if (contourArea > maxArea)
        //                {
        //                    maxArea = contourArea;
        //                    maxContourIdx = i;
        //                }
        //            }

        //            // Aproxima el contorno
        //            using (VectorOfPoint approxContour = new VectorOfPoint())
        //            {
        //                CvInvoke.ApproxPolyDP(contours[maxContourIdx], approxContour, CvInvoke.ArcLength(contours[maxContourIdx], true) * 0.0025, true);
        //                hull = new VectorOfPoint();
        //                CvInvoke.ConvexHull(approxContour, hull, true);

        //                // Dibuja el casco convexo
        //                currentFrame.DrawPolyline(hull.ToArray(), true, new Bgr(200, 125, 75), 2);

        //                // Obtiene el rectángulo del área mínima y dibuja el centro
        //                box = CvInvoke.MinAreaRect(approxContour);
        //                currentFrame.Draw(new CircleF(box.Center, 3), new Bgr(200, 125, 75), 2);

        //                // Filtra el casco convexo
        //                filteredHull = new VectorOfPoint();
        //                System.Drawing.Point[] hullArray = hull.ToArray();
        //                for (int i = 0; i < hullArray.Length - 1; i++)
        //                {
        //                    double distance = Math.Sqrt(
        //                     (hullArray[i].X - hullArray[i + 1].X) * (hullArray[i].X - hullArray[i + 1].X) +
        //                     (hullArray[i].Y - hullArray[i + 1].Y) * (hullArray[i].Y - hullArray[i + 1].Y));

        //                    if (distance > box.Size.Width / 10)
        //                    {
        //                        filteredHull.Push(new[] { hullArray[i] });
        //                    }
        //                }

        //                // Obtiene los defectos de convexidad
        //                hullIndices = new VectorOfInt();
        //                CvInvoke.ConvexHull(approxContour, hullIndices, false, false);
        //                defects = new Mat();
        //                CvInvoke.ConvexityDefects(approxContour, hullIndices, defects);

        //                // Ahora puedes acceder a los defectos de convexidad desde 'defects'
        //                // La estructura de datos puede ser diferente, así que tendrás que ajustar tu código
        //            }
        //        }
        //    }
        //}

        //private void DrawAndComputeFingersNum(Mat skin, VectorOfPoint filteredHull, Mat defects, RotatedRect box)
        //{
        //    // Asumiendo que 'currentFrame' es un Image<Bgr, Byte> que ya has definido en tu clase
        //    int fingerNum = 0;

        //    if (defects != null && defects.Rows > 0)
        //    {
        //        // Convertir Mat a array de defectos de convexidad
        //        // Nota: Asegúrate de obtener los índices de los puntos del contorno y luego los puntos reales
        //        for (int i = 0; i < defects.Rows; i++)
        //        {
        //            // Obtén los índices de los puntos del contorno
        //            int startIdx = defects.GetData().GetValue;
        //            int endIdx = defects.GetData(i)[1];
        //            int depthIdx = defects.GetData(i)[2];

        //            // Usa los índices para obtener los puntos reales del contorno
        //           System.Drawing. Point startPoint = filteredHull[startIdx];
        //            System.Drawing.Point endPoint = filteredHull[endIdx];
        //            System.Drawing.Point depthPoint = filteredHull[depthIdx];

        //            // Dibuja las líneas de los defectos de convexidad
        //            LineSegment2D startDepthLine = new LineSegment2D(startPoint, depthPoint);
        //            LineSegment2D depthEndLine = new LineSegment2D(depthPoint, endPoint);

        //            CircleF startCircle = new CircleF(startPoint, 5f);
        //            CircleF depthCircle = new CircleF(depthPoint, 5f);
        //            CircleF endCircle = new CircleF(endPoint, 5f);

        //            // Heurística personalizada basada en algunos experimentos, verifica antes de usar
        //            if ((startCircle.Center.Y < box.Center.Y || depthCircle.Center.Y < box.Center.Y) &&
        //                (startCircle.Center.Y < depthCircle.Center.Y) &&
        //                (CvInvoke.Norm(startPoint, depthPoint) > box.Size.Height / 6.5))
        //            {
        //                fingerNum++;
        //                currentFrame.Draw(startDepthLine, new Bgr(System.Drawing.Color.Green), 2);
        //            }

        //            // Dibuja los círculos de los defectos de convexidad
        //            currentFrame.Draw(startCircle, new Bgr(System.Drawing.Color.Red), 2);
        //            currentFrame.Draw(depthCircle, new Bgr(System.Drawing.Color.Yellow), 5);
        //        }
        //    }


        //}

        #endregion


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

        private void btnFilters_Click(object sender, RoutedEventArgs e)
        {
            ImageFilters img = new ImageFilters();
            img.Show();
            //cerrar la ventana actual
            this.Close();
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
