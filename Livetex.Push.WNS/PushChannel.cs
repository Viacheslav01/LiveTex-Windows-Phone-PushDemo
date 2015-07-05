using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Microsoft.Phone.Notification;

namespace Livetex.Push
{
	internal class LtPushChannel
	{
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
			get { return _notificationChannel.Uri; }
		}

		private PushNotificationChannel _notificationChannel;

		private async Task InitializeWpnsChanelAsync()
		{
			_notificationChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
			_notificationChannel.PushNotificationReceived += PushNotificationReceived;
		}

		private void PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
		{
			OnPushReceived(args.ToastNotification.Content.ToString());
		}
	}
}
