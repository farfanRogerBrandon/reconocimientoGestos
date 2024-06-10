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
	public partial class CameraWindow : Window
	{
		FilterInfoCollection cams;
		VideoCaptureDevice currentCam;
		BitmapImage currentImg;

		BitmapImage filteredImage;

		string gesture = "";
		string conta = "";

		MyCurrentFilter MyFilt;
		Bitmap fileToChange;

		bool takeImages = false;

		int nImages = 0;

		System.Windows.Visibility visible = System.Windows.Visibility.Collapsed;
		Gestures GESTURE = Gestures.NOTHING;

		int nroIm = 0;
		bool stopProcessing = false;
		bool applyInvert = false;
		bool applyGrayscale = false;

		public CameraWindow()
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
			Loaded += CameraWindow_Loaded;
		}

		private void CameraWindow_Loaded(object sender, RoutedEventArgs e)
		{
			StartCamera();
		}

		private void StartCamera()
		{
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
			BitmapImage bim = MainWindow.ToBitmapImage(myaux);
			fileToChange = myaux;

			if (takeImages)
			{
				MyDetector();
				takeImages = false;
			}

			Dispatcher.BeginInvoke(new Action(() =>
			{
				if (applyGrayscale)
				{
					ApplyGrayscaleFilter(fileToChange);
				}
				else if (applyInvert)
				{
					ApplyInvertFilter(fileToChange);
				}
				else
				{
					imgVideo1.Source = bim;
				}

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

			if (stopProcessing) return;

			using (HttpClient client = new HttpClient())
			using (var content = new MultipartFormDataContent())
			{
				try
				{
					byte[] imageBytes = MainWindow.ConvertBitmapToBytes(fileToChange);
					var imageContent = new ByteArrayContent(imageBytes);
					content.Add(imageContent, "image", "image.jpg");

					HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/recognize_gesture", content);
					response.EnsureSuccessStatusCode();
					string responseBody = await response.Content.ReadAsStringAsync();

					GESTURE = GetGesture(responseBody);
					MAKEACTION(GESTURE);
				}
				catch (HttpRequestException e)
				{
					MessageBox.Show($"Error al hacer la solicitud HTTP: {e.Message}");
				}
			}
			visible = System.Windows.Visibility.Collapsed;
			if (!stopProcessing) ProcessMyImage();
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
			// Resetear los filtros
			applyGrayscale = false;
			applyInvert = false;

			// Mostrar el mensaje emergente con la acción correspondiente al gesto
			switch (gesture)
			{
				case Gestures.Open:
					MessageBox.Show("You selected: Open", "Gesture");
					break;
				case Gestures.AFinger:
					applyGrayscale = true;
					MessageBox.Show("You selected: A Finger", "Gesture");
					break;
				case Gestures.TwoFinger:
					applyInvert = true;
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

		void ApplyGrayscaleFilter(Bitmap bitmap)
		{
			applyInvert = false;

			if (bitmap != null)
			{
				Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
				Bitmap grayImage = grayscaleFilter.Apply(bitmap);

				filteredImage = MainWindow.ToBitmapImage(grayImage);

				Dispatcher.BeginInvoke(new Action(() =>
				{
					imgVideo1.Source = filteredImage;
				}));
			}
		}

		void ApplyInvertFilter(Bitmap bitmap)
		{
			applyGrayscale = false;

			if (bitmap != null)
			{
				Invert invertFilter = new Invert();
				Bitmap invertedImage = invertFilter.Apply(bitmap);

				filteredImage = MainWindow.ToBitmapImage(invertedImage);

				Dispatcher.BeginInvoke(new Action(() =>
				{
					imgVideo1.Source = filteredImage;
				}));
			}
		}

		private void cmbCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			StartCamera();
		}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			StartCamera();
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			if (currentCam != null)
			{
				currentCam.Stop();
			}
		}

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
	}

}
