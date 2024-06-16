﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FInalProject_PDI
{
	public partial class GestureCamera : Window
	{
		public GestureCamera()
		{
			InitializeComponent();
			Loaded += GestureCamera_Loaded;
		}

		private void GestureCamera_Loaded(object sender, RoutedEventArgs e)
		{
			StartGestureRecognition();
		}

		private async void StartGestureRecognition()
		{
			Gestures recognizedGesture = await RecognizeGestureAsync();
			if (recognizedGesture == Gestures.Ok)
			{
				((CameraWindow)Owner).SavePhoto();
			}
			else if (recognizedGesture == Gestures.NotOk)
			{
				MessageBox.Show("Foto descartada");
			}
			else
			{

			}
			Close();
		}

		private async Task<Gestures> RecognizeGestureAsync()
		{
			await Task.Delay(3000);
			var cameraWindow = (CameraWindow)Owner;
			var gesture = await cameraWindow.RecognizeGestureFromCurrentFrame();
			return gesture;
		}
	}
}
