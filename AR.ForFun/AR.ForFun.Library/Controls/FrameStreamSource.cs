using GART.Data;
using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Media;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Windows.Storage.Streams;


namespace AR.ForFun.Library.Controls
{
    public class FrameStreamSource : MediaStreamSource
    {
        private readonly Dictionary<MediaSampleAttributeKeys, string> emptyAttributes = new Dictionary<MediaSampleAttributeKeys, string>();

        private MediaStreamDescription videoStreamDescription = null;
        private MemoryStream frameStream = null;

        private Size frameSize = new Size(0, 0);
        private int frameBufferSize = 0;
        private byte[] frameBuffer = null;
        private PhotoCaptureDevice photoCaptureDevice;
        private CameraPreviewImageSource cameraPreviewImageSource;
        private ObservableCollection<LiveFilter> liveFilters;

        private IFilter activeFilter;
        private int currentFilterIndex = 0;

        public FrameStreamSource(PhotoCaptureDevice pcd, ObservableCollection<LiveFilter> liveFilters)
        {
            this.photoCaptureDevice = pcd;
            var smallestPreview = this.photoCaptureDevice.PreviewResolution;
            this.liveFilters = liveFilters;
            if (this.liveFilters == null || this.liveFilters.Count == 0)
            {
                this.liveFilters = new ObservableCollection<LiveFilter>();
            }

            this.liveFilters.Insert(0, LiveFilter.None);
            currentFilterIndex = 0;
            ConvertFilterEnumToIFilter(this.liveFilters[currentFilterIndex]);
            
            cameraPreviewImageSource = new CameraPreviewImageSource(photoCaptureDevice);
            frameSize = new Size(smallestPreview.Width / 2, smallestPreview.Height / 2);
            frameBufferSize = (int)frameSize.Width * (int)frameSize.Height * 4; // RGBA
            frameBuffer = new byte[frameBufferSize];
            frameStream = new MemoryStream(frameBuffer);
        }

        protected override void CloseMedia()
        {
            if (frameStream != null)
            {
                frameStream.Close();
                frameStream = null;
            }

            cameraPreviewImageSource = null;
            photoCaptureDevice = null;
            frameBufferSize = 0;
            frameBuffer = null;
            videoStreamDescription = null;
        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            var task = GetNewFrameAndApplyChosenEffectAsync(frameBuffer.AsBuffer());

            task.ContinueWith((action) =>
            {
                if (frameStream != null)
                {
                    frameStream.Position = 0;

                    var sample = new MediaStreamSample(videoStreamDescription, frameStream, 0, frameBufferSize, 0, emptyAttributes);

                    ReportGetSampleCompleted(sample);

                }
            });
        }

        private async Task GetNewFrameAndApplyChosenEffectAsync(IBuffer buffer)
        {
            var lineSize = (uint)frameSize.Width * 4;
            var bitmap = new Bitmap(frameSize, ColorMode.Bgra8888, lineSize, buffer);

            IFilter[] filters;

            if (activeFilter == null)
            {
                filters = new IFilter[0];
            }
            else
            {
                filters = new IFilter[]
                {
                    activeFilter
                };
            }

            using (FilterEffect fe = new FilterEffect(cameraPreviewImageSource)
            {
                Filters = filters
            })
            using (BitmapRenderer renderer = new BitmapRenderer(fe, bitmap))
            {
                await renderer.RenderAsync();
            }
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void SeekAsync(long seekToTime)
        {
            ReportSeekCompleted(seekToTime);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

        protected override void OpenMediaAsync()
        {
            var mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();

            mediaStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
            mediaStreamAttributes[MediaStreamAttributeKeys.Width] = ((int)frameSize.Width).ToString();
            mediaStreamAttributes[MediaStreamAttributeKeys.Height] = ((int)frameSize.Height).ToString();

            videoStreamDescription = new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);

            var mediaStreamDescriptions = new List<MediaStreamDescription>();
            mediaStreamDescriptions.Add(videoStreamDescription);

            var mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromSeconds(0).Ticks.ToString(CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);
        }

        public void NextFilter()
        {
            if (currentFilterIndex == liveFilters.Count - 1)
            {
                currentFilterIndex = 0;
            }
            else
            {
                currentFilterIndex++;
            }
            ConvertFilterEnumToIFilter(liveFilters[currentFilterIndex]);

        }

        private void ConvertFilterEnumToIFilter(LiveFilter liveFilter)
        {
            switch (liveFilter)
            {
                case LiveFilter.None:
                    activeFilter = null;
                    break;
                case LiveFilter.Antique:
                    activeFilter = new AntiqueFilter();
                    break;
                case LiveFilter.AutoEnhance:
                    activeFilter = new AutoEnhanceFilter();
                    break;
                case LiveFilter.AutoLevels:
                    activeFilter = new AutoLevelsFilter();
                    break;
                case LiveFilter.Cartoon:
                    activeFilter = new CartoonFilter();
                    break;
                case LiveFilter.ColorBoost:
                    activeFilter = new ColorBoostFilter();
                    break;
                case LiveFilter.Grayscale:
                    activeFilter = new GrayscaleFilter();
                    break;
                case LiveFilter.Lomo:
                    activeFilter = new LomoFilter();
                    break;
                case LiveFilter.MagicPen:
                    activeFilter = new MagicPenFilter();
                    break;
                case LiveFilter.Negative:
                    activeFilter = new NegativeFilter();
                    break;
                case LiveFilter.Noise:
                    activeFilter = new NoiseFilter();
                    break;
                case LiveFilter.Oily:
                    activeFilter = new OilyFilter();
                    break;
                case LiveFilter.Paint:
                    activeFilter = new PaintFilter();
                    break;
                case LiveFilter.Sketch:
                    activeFilter = new SketchFilter();
                    break;
                case LiveFilter.Vignetting:
                    activeFilter = new VignettingFilter();
                    break;
                case LiveFilter.WhiteboardEnhancement:
                    activeFilter = new WhiteboardEnhancementFilter();
                    break;
                default:
                    activeFilter = null;
                    break;
            }
        }
    }
}
