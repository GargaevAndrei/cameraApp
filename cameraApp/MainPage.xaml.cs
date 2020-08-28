using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace cameraApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private MediaCapture mediaCapture;
        bool isInitialized = false;
        private readonly DisplayRequest displayRequest = new DisplayRequest();


        public MainPage()
        {
            this.InitializeComponent();
        }

        private static async  Task<DeviceInformationCollection> FindCameraDeviceAsync()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return allVideoDevices;
        }

        private async Task initializeCameraAsync()
        {
            Debug.WriteLine("initializeCametaAsync");

            if(mediaCapture == null)
            {
                var cameraDeviceList = await FindCameraDeviceAsync();
            
                if(cameraDeviceList.Count == 0)
                {
                    Debug.WriteLine("No camera device found");
                    return;
                }

                DeviceInformation cameraDevice;
                cameraDevice = cameraDeviceList[0];
                mediaCapture = new MediaCapture();
                //mediaCapture.Failed += MediaCaptureFiled;

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                try
                {
                    await mediaCapture.InitializeAsync(settings);
                    isInitialized = true;
                }
                catch(UnauthorizedAccessException)
                {
                    Debug.WriteLine("camera denided");
                }

                if(isInitialized)
                {
                    await StartPreviewAsync();
                }

            }
            
        }

        private async Task StartPreviewAsync()
        {
            displayRequest.RequestActive();
            PreviewControl.Source = mediaCapture;
            await mediaCapture.StartPreviewAsync();
        }

        private async void SwitchCam_Click(object sender, RoutedEventArgs e)
        {
            await initializeCameraAsync();
        }


    }
}
