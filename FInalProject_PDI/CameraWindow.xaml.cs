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
		bool applySpeia = false;
		bool applyHsl = false;

		bool makePicture = false;

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

			currentCam = new VideoCaptureDevice(cams[2].MonikerString);
			currentCam.NewFrame += new NewFrameEventHandler(MyNewFrame);
			currentCam.VideoResolution = currentCam.VideoCapabilities[3];
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
				else if (applySpeia)
				{
					ApplySepiaFilter(fileToChange);
				}
				else if (applyHsl)
				{
					ApplyHSLFilter(fileToChange);
				}
				else if (makePicture)
				{
					makePicture = false;
					ShowConfirmationWindow();
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

		void ShowConfirmationWindow()
		{
			GestureCamera confirmationWindow = new GestureCamera();
			confirmationWindow.Owner = this;
			confirmationWindow.ShowDialog();
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
			for (int i = 3; i > 0; i--)
			{
				conta = $"Capturando gesto en {i} ";
				Dispatcher.Invoke(() => contador.Text = conta);
				Thread.Sleep(1000);
			}
			conta = "0 ";
			Dispatcher.Invoke(() => contador.Text = conta);
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

					HttpResponseMessage response = await client.PostAsync(GeneralTools.ApiCpnnection, content);
					response.EnsureSuccessStatusCode();
					string responseBody = await response.Content.ReadAsStringAsync();

					GESTURE = GetGesture(responseBody);
					Dispatcher.Invoke(() => MAKEACTION(GESTURE));
				}
				catch (HttpRequestException e)
				{
					MessageBox.Show($"Error al hacer la solicitud HTTP: {e.Message}");
				}
			}
			visible = System.Windows.Visibility.Collapsed;
			Dispatcher.Invoke(() => myPb.Visibility = visible);
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

			if (gesture == Gestures.NOTHING && gesture == Gestures.NONE)
			{
				return;
			}

			applySpeia = false;
			applyGrayscale = false;
			applyInvert = false;
			applyHsl = false;
			makePicture = false;

			switch (gesture)
			{
				case Gestures.Open:
					applyHsl = true;
					break;
				case Gestures.AFinger:
					applyGrayscale = true;

					break;
				case Gestures.TwoFinger:
					applyInvert = true;

					break;
				case Gestures.Rock:
					applySpeia = true;
					break;
				case Gestures.Ok:
					break;
				case Gestures.NotOk:
					stopProcessing = true;
					MainWindow mainWindow = new MainWindow();
					mainWindow.Show();
					this.Close();
					break;
				case Gestures.Close:
					makePicture = true;
					break;
				case Gestures.NONE:

					break;
				case Gestures.NOTHING:

					break;
				default:

					break;
			}
		}

		void ApplyGrayscaleFilter(Bitmap bitmap)
		{
			applyInvert = false;
			applySpeia = false;
			applyHsl = false;

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
			applySpeia = false;
			applyHsl = false;

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

		void ApplySepiaFilter(Bitmap bitmap)
		{
			applyHsl = false;
			applyGrayscale = false;
			applyInvert = false;

			if (bitmap != null)
			{
				Sepia filtroSepia = new Sepia();
				Bitmap sepiaImage = filtroSepia.Apply(bitmap);

				filteredImage = MainWindow.ToBitmapImage(sepiaImage);

				Dispatcher.BeginInvoke(new Action(() =>
				{
					imgVideo1.Source = filteredImage;
				}));
			}
		}

		void ApplyHSLFilter(Bitmap bitmap)
		{
			applyGrayscale = false;
			applyInvert = false;
			applySpeia = false;

			if (bitmap != null)
			{
				HSLFiltering hSLFiltering = new HSLFiltering();
				hSLFiltering.Hue = new IntRange(340, 20);
				hSLFiltering.UpdateHue = false;
				hSLFiltering.UpdateLuminance = false;
				Bitmap hslImage = hSLFiltering.Apply(bitmap);
				filteredImage = MainWindow.ToBitmapImage(hslImage);

				Dispatcher.BeginInvoke(new Action(() =>
				{
					imgVideo1.Source = filteredImage;
				}));
			}
		}

		public void SavePhoto()
		{
			if (filteredImage != null)
			{
				try
				{
					string folderPath = @"D:\GitHub\reconocimientoGestos\fotitos";
					if (!Directory.Exists(folderPath))
					{
						Directory.CreateDirectory(folderPath);
					}

					string photoPath = System.IO.Path.Combine(folderPath, "foto.jpg");

					BitmapEncoder encoder = new JpegBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(filteredImage));
					using (var fileStream = new FileStream(photoPath, FileMode.Create))
					{
						encoder.Save(fileStream);
					}

					MessageBox.Show($"Foto guardada en: {photoPath}");
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error al guardar la foto: {ex.Message}");
				}
			}
			else
			{
				MessageBox.Show("No hay imagen filtrada para guardar.");
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

		public async Task<Gestures> RecognizeGestureFromCurrentFrame()
		{
			if (fileToChange == null)
				return Gestures.NOTHING;

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

					return GetGesture(responseBody);
				}
				catch (HttpRequestException e)
				{
					MessageBox.Show($"Error al hacer la solicitud HTTP: {e.Message}");
					return Gestures.NOTHING;
				}
			}
		}
	}

	public enum Gestures
	{
		NOTHING,
		AFinger,
		Open,
		Rock,
		Close,
		NONE,
		TwoFinger,
		Ok,
		NotOk
	}

}
