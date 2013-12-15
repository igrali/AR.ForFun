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
        private readonly Dictionary<MediaSampleAttributeKeys, string> _emptyAttributes = new Dictionary<MediaSampleAttributeKeys, string>();

        private MediaStreamDescription _videoStreamDescription = null;
        private MemoryStream _frameStream = null;

        private Size _frameSize = new Size(0, 0);
        private int _frameBufferSize = 0;
        private byte[] _frameBuffer = null;
        private PhotoCaptureDevice photoCaptureDevice;
        private CameraPreviewImageSource cameraPreviewImageSource;
        private ObservableCollection<LiveFilter> _liveFilters;

        private IFilter _activeFilter;
        private int _currentFilterIndex = 0;

        public FrameStreamSource(PhotoCaptureDevice pcd, ObservableCollection<LiveFilter> liveFilters)
        {
            photoCaptureDevice = pcd;
            var smallestPreview = photoCaptureDevice.PreviewResolution;
            _liveFilters = liveFilters;
            if (_liveFilters == null || _liveFilters.Count == 0)
            {
                _activeFilter = null;
            }
            else
            {
                _currentFilterIndex = 0;
                _liveFilters.Insert(0, LiveFilter.None);
                ConvertFilterEnumToIFilter(_liveFilters[_currentFilterIndex]);
            }
            cameraPreviewImageSource = new CameraPreviewImageSource(photoCaptureDevice);
            _frameSize = new Size(smallestPreview.Width / 2, smallestPreview.Height / 2);
            _frameBufferSize = (int)_frameSize.Width * (int)_frameSize.Height * 4; // RGBA
            _frameBuffer = new byte[_frameBufferSize];
            _frameStream = new MemoryStream(_frameBuffer);
        }

        protected override void CloseMedia()
        {
            if (_frameStream != null)
            {
                _frameStream.Close();
                _frameStream = null;
            }

            cameraPreviewImageSource = null;
            photoCaptureDevice = null;
            _frameBufferSize = 0;
            _frameBuffer = null;
            _videoStreamDescription = null;
        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            var task = GetNewFrameAndApplyChosenEffectAsync(_frameBuffer.AsBuffer());

            task.ContinueWith((action) =>
            {
                if (_frameStream != null)
                {
                    _frameStream.Position = 0;

                    var sample = new MediaStreamSample(_videoStreamDescription, _frameStream, 0, _frameBufferSize, 0, _emptyAttributes);

                    ReportGetSampleCompleted(sample);

                }
            });
        }

        private async Task GetNewFrameAndApplyChosenEffectAsync(IBuffer buffer)
        {
            var scanlineByteSize = (uint)_frameSize.Width * 4;
            var bitmap = new Bitmap(_frameSize, ColorMode.Bgra8888, scanlineByteSize, buffer);

            IFilter[] filters;

            if (_activeFilter == null)
            {
                filters = new IFilter[0];
            }
            else
            {
                filters = new IFilter[]
                {
                    _activeFilter
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
            mediaStreamAttributes[MediaStreamAttributeKeys.Width] = ((int)_frameSize.Width).ToString();
            mediaStreamAttributes[MediaStreamAttributeKeys.Height] = ((int)_frameSize.Height).ToString();

            _videoStreamDescription = new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);

            var mediaStreamDescriptions = new List<MediaStreamDescription>();
            mediaStreamDescriptions.Add(_videoStreamDescription);

            var mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromSeconds(0).Ticks.ToString(CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);
        }

        public void NextFilter()
        {
            if (_currentFilterIndex == _liveFilters.Count - 1)
            {
                _currentFilterIndex = 0;
            }
            else
            {
                _currentFilterIndex++;
            }
            ConvertFilterEnumToIFilter(_liveFilters[_currentFilterIndex]);

        }

        private void ConvertFilterEnumToIFilter(LiveFilter liveFilter)
        {
            switch (liveFilter)
            {
                case LiveFilter.None:
                    _activeFilter = null;
                    break;
                case LiveFilter.Antique:
                    _activeFilter = new AntiqueFilter();
                    break;
                case LiveFilter.AutoEnhance:
                    _activeFilter = new AutoEnhanceFilter();
                    break;
                case LiveFilter.AutoLevels:
                    _activeFilter = new AutoLevelsFilter();
                    break;
                case LiveFilter.Cartoon:
                    _activeFilter = new CartoonFilter();
                    break;
                case LiveFilter.ColorBoost:
                    _activeFilter = new ColorBoostFilter();
                    break;
                case LiveFilter.Grayscale:
                    _activeFilter = new GrayscaleFilter();
                    break;
                case LiveFilter.Lomo:
                    _activeFilter = new LomoFilter();
                    break;
                case LiveFilter.MagicPen:
                    _activeFilter = new MagicPenFilter();
                    break;
                case LiveFilter.Negative:
                    _activeFilter = new NegativeFilter();
                    break;
                case LiveFilter.Noise:
                    _activeFilter = new NoiseFilter();
                    break;
                case LiveFilter.Oily:
                    _activeFilter = new OilyFilter();
                    break;
                case LiveFilter.Paint:
                    _activeFilter = new PaintFilter();
                    break;
                case LiveFilter.Sketch:
                    _activeFilter = new SketchFilter();
                    break;
                case LiveFilter.Vignetting:
                    _activeFilter = new VignettingFilter();
                    break;
                case LiveFilter.WhiteboardEnhancement:
                    _activeFilter = new WhiteboardEnhancementFilter();
                    break;
                default:
                    _activeFilter = null;
                    break;
            }
        }
    }
}
