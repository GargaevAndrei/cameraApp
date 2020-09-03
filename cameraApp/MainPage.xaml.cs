using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


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
        private StorageFolder storageFolder = null;

        private InMemoryRandomAccessStream videoStream = new InMemoryRandomAccessStream();
        private LowLagMediaRecording _mediaRecording;

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

        private async void MakePhoto_Click(object sender, RoutedEventArgs e)
        {
            await MakePhotoAsync();
        }

        private async Task MakePhotoAsync()
        {
            Debug.WriteLine("Make photo");
            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            storageFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;

            var stream = new InMemoryRandomAccessStream();
            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            try
            {
                var photofile = await storageFolder.CreateFileAsync("CameraPhoto.jpg", CreationCollisionOption.GenerateUniqueName);
                await SavePhotoAsync(stream, photofile);
                Debug.WriteLine("Photo saved in" + photofile.Path);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception while making photo " + ex.Message.ToString());
            }
        }

        private async Task SavePhotoAsync(InMemoryRandomAccessStream stream, StorageFile photofile)
        {

            using (var photoStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(photoStream);
                using(var photoFileStream = await photofile.OpenAsync(FileAccessMode.ReadWrite))
                {                    
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(photoFileStream, decoder);
                  

                    if((decoder.OrientedPixelWidth == 80) && (decoder.OrientedPixelHeight == 60))
                    {
                        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                        encoder.BitmapTransform.ScaledHeight = 360;
                        encoder.BitmapTransform.ScaledWidth = 480;
                       
                    }

                    
                    await encoder.FlushAsync();
                }
            }            


        }

       /* private void CanvasControl_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        CanvasBitmap cbi;
        async Task CreateResourcesAsync(CanvasControl sender)
        {
            try
            {
                cbi = await CanvasBitmap.LoadAsync(sender, @"C:\temp\CameraPhoto.jpg");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void CanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(cbi);
        }*/

       
        private async void StartRecord_Click(object sender, RoutedEventArgs e)
        {
            await StartRecordAsync();

        }


        private async Task StartRecordAsync()
        {
            Debug.WriteLine("StartRecord");

            var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            StorageFile file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga), file);
            await _mediaRecording.StartAsync();

        }

        private async void StopRecord_Click(object sender, RoutedEventArgs e)
        {
            await _mediaRecording.FinishAsync();
        }



    }
}
