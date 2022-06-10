using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using DeliveryManagement.Resources;
using DeliveryManagement.Services.Classes;
using DeliveryManagement.ViewModels;
using DeliveryManagement.Views;
using Shiny.Locations;

namespace DeliveryManagement.Droid.Services
{
    public class GeolocationServiceBinder : Binder
    {
        public GeolocationServiceBinder(GeolocationService service)
        {
            Service = service;
        }

        public GeolocationService Service { get; }

        public bool IsBound { get; set; }
    }

    [Service]
    public class GeolocationService : Service
    {
        IBinder binder;

        public GPsTrackingViewModel ViewModel { get; private set; }

        public override IBinder OnBind(Intent intent)
        {
            binder = new GeolocationServiceBinder(this);
            return binder;
        }
        public override void OnDestroy()
        {
            timer.Dispose();
            StopLocationUpdates();
            base.OnDestroy();
        }
        Timer timer;
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var builder = new NotificationCompat.Builder(this);
            
            var newIntent = new Intent(this, typeof(MainActivity));
            newIntent.PutExtra("tracking", true);
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(this, 0, newIntent, 0);
            var notification = builder.SetContentIntent(pendingIntent)
                .SetSmallIcon(Resource.Drawable.swipeImage)
                .SetAutoCancel(false)
                .SetTicker("")
                .SetChannelId("DefaultChannel")
                .SetContentTitle(AppResource.mrBakerApplication)
                .SetContentText(AppResource.online)
                .Build();

           
            StartForeground((int)NotificationFlags.ForegroundService, notification);
            var startTimeSpan = TimeSpan.Zero;
            ViewModel = new GPsTrackingViewModel();

            var periodTimeSpan = TimeSpan.FromSeconds(10);
             timer = new System.Threading.Timer(async(e) =>
            {
               await StartLocationUpdates().ConfigureAwait(false);
            }, null, startTimeSpan, periodTimeSpan);
        //  StartLocationUpdates();
       
            return StartCommandResult.Sticky;
        }

        public async Task StartLocationUpdates()
        {
            await ViewModel.Connect();
            System.Diagnostics.Debug.WriteLine("Service Started");
            await ViewModel.startTraking();
        }
        public async void StopLocationUpdates()
        {
            try
            {
                await ViewModel.StopTracking();
            }
            catch (Exception exp)
            {
            }
        }
    }

    public class GeolocationServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public GeolocationServiceConnection(GeolocationServiceBinder binder)
        {
            if (binder != null)
            {
                Binder = binder;
            }
        }

        public GeolocationServiceBinder Binder { get; set; }
       

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var serviceBinder = service as GeolocationServiceBinder;

            if (serviceBinder == null)
                return;


            Binder = serviceBinder;
            Binder.IsBound = true;

            // raise the service bound event
            ServiceConnected?.Invoke(this, new ServiceConnectedEventArgs { Binder = service });
            System.Diagnostics.Debug.WriteLine("Service Started from binding");

            // begin updating the location in the Service
            serviceBinder.Service.StartLocationUpdates();
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Binder.IsBound = false;
        }

        public event EventHandler<ServiceConnectedEventArgs> ServiceConnected;
    }

    public class ServiceConnectedEventArgs : EventArgs
    {
        public IBinder Binder { get; set; }
    }
}