using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using LiveTex.SDK;
using LiveTex.SDK.Client;
using Microsoft.Phone.Controls;

namespace Livetex.Push
{
	public partial class MainPage
		: PhoneApplicationPage
	{
		private LtPushChannel _pushChannel;

		public MainPage()
		{
			InitializeComponent();
		}

		private async void ButtonClick(object sender, RoutedEventArgs e)
		{
			viewBusyMessage.Text = "подключение к LiveTex";
			viewBusyPanel.Visibility = Visibility.Visible;

			try
			{
				if (!Validate())
				{
					return;
				}

				Storage.LiveTexApplication = viewApp.Text;
				Storage.LiveTexKey = viewKey.Text;
				Storage.LiveTexServer = viewUri.Text;

				var factory = new LiveTexClientFactory(viewKey.Text, viewApp.Text, new Uri(viewUri.Text, UriKind.Absolute));
				var client = await factory.CreateAsync(_pushChannel.Uri, null, Capabilities.Chat);

				viewSubscribeButton.IsEnabled = false;
				viewApp.IsEnabled = false;
				viewKey.IsEnabled = false;
				viewUri.IsEnabled = false;

				MessageBox.Show("Подключение установлено: token " + client.GetToken());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				viewBusyPanel.Visibility = Visibility.Collapsed;
			}
		}

		private bool Validate()
		{
			string error = null;

			if (string.IsNullOrWhiteSpace(viewApp.Text))
			{
				error += "Не указан Application" + Environment.NewLine;
			}

			if (string.IsNullOrWhiteSpace(viewKey.Text))
			{
				error += "Не указан Key" + Environment.NewLine;
			}

			if (string.IsNullOrWhiteSpace(viewUri.Text))
			{
				error += "Не указан URI" + Environment.NewLine;
			}

			if (error != null)
			{
				MessageBox.Show(error, "Ошибка", MessageBoxButton.OK);
			}

			return error == null;
		}

		private bool _firstStart = true;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if(_firstStart)
			{
				viewApp.Text = Storage.LiveTexApplication ?? "";
				viewKey.Text = Storage.LiveTexKey ?? "";
				viewUri.Text = Storage.LiveTexServer ?? "";

				_firstStart = false;
			}

			InitializePushChannel();

			// Обработка навигации инициированной push сообщением

			if (e.NavigationMode != NavigationMode.New
				&& e.NavigationMode != NavigationMode.Refresh)
			{
				return;
			}

			if (!NavigationContext.QueryString.ContainsKey("LiveTex"))
			{
				return;
			}

			var @params = NavigationContext.QueryString.Select(kvp => HttpUtility.UrlEncode(kvp.Key) + "=" + HttpUtility.UrlEncode(kvp.Value));
			var uri = "/PushPage.xaml?" + string.Join("&", @params);

			NavigationService.Navigate(new Uri(uri, UriKind.Relative));
		}

		private async void InitializePushChannel()
		{
			if(_pushChannel != null)
			{
				return;
			}

			viewSubscribeButton.IsEnabled = false;

			viewBusyMessage.Text = "подключение к WNS";
			viewBusyPanel.Visibility = Visibility.Visible;
			
			try
			{
				_pushChannel = await LtPushChannel.Create();
				_pushChannel.PushReceived += OnPushReceived;

				viewSubscribeButton.IsEnabled = true;
				viewPushChanel.Text = _pushChannel.Uri;

				Debug.WriteLine("New Push chanel registered: {0}", _pushChannel.Uri);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				viewBusyPanel.Visibility = Visibility.Collapsed;
			}
		}

		private void OnPushReceived(object sender, string @params)
		{
			Dispatcher.BeginInvoke(() =>
			{
				var result = MessageBox.Show("Получено уведомление показать детали?", "push", MessageBoxButton.OKCancel);
				if (result != MessageBoxResult.OK)
				{
					return;
				}

				var uri = "/PushPage.xaml?" + @params;
				NavigationService.Navigate(new Uri(uri, UriKind.Relative));
			});
		}
	}
}