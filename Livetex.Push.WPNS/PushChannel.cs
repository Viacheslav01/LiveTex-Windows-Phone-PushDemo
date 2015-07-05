using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Phone.Notification;

namespace Livetex.Push
{
	internal class LtPushChannel
	{
		private const string cPushChanelName = "LiveTexTest-SL";

		private LtPushChannel()
		{
		}

		public static async Task<LtPushChannel> Create()
		{
			var channel = new LtPushChannel();
			await channel.InitializeWpnsChanelAsync();

			return channel;
		}

		public event EventHandler<string> PushReceived;
 		private void OnPushReceived(string @params)
		{
			var handler = Volatile.Read(ref PushReceived);
			if(handler != null)
			{
				handler(this, @params);
			}
		}

		public string Uri
		{
			get { return _notificationChannel.ChannelUri.ToString(); }
		}

		private HttpNotificationChannel _notificationChannel;

		private Task InitializeWpnsChanelAsync()
		{
			_notificationChannel = HttpNotificationChannel.Find(cPushChanelName);

			if (_notificationChannel == null)
			{
				_notificationChannel = new HttpNotificationChannel(cPushChanelName);

				var tcs = new TaskCompletionSource<bool>();

				EventHandler<NotificationChannelUriEventArgs> uriUpdated = null;
				uriUpdated = (o, e) =>
				{
					_notificationChannel.ChannelUriUpdated -= uriUpdated;
					tcs.TrySetResult(true);
				};

				EventHandler<NotificationChannelErrorEventArgs> errorOccured = null;
				errorOccured = (o, e) =>
				{
					_notificationChannel.ErrorOccurred -= errorOccured;
					tcs.SetException(new Exception(e.Message));
				};

				_notificationChannel.ChannelUriUpdated += uriUpdated;
				_notificationChannel.ErrorOccurred += errorOccured;
				_notificationChannel.ShellToastNotificationReceived += PushChannelShellToastNotificationReceived;

				_notificationChannel.Open();
				_notificationChannel.BindToShellToast();

				return tcs.Task;
			}

			_notificationChannel.ShellToastNotificationReceived += PushChannelShellToastNotificationReceived;

			return Task.FromResult(true);
		}

		private void PushChannelShellToastNotificationReceived(object sender, NotificationEventArgs e)
		{
			var @params = e.Collection.Select(kvp => HttpUtility.UrlEncode(kvp.Key) + "=" + HttpUtility.UrlEncode(kvp.Value));
			OnPushReceived(string.Join("&", @params));
		}
	}
}
