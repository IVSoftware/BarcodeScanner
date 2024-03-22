
// https://youtu.be/XdsFhlazyhk?si=GitLHcozV3nqy-Id

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BarcodeScanner
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            barcodeScanner.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats =
                    ZXing.Net.Maui.BarcodeFormat.UpcA |
                    ZXing.Net.Maui.BarcodeFormat.UpcE |
                    ZXing.Net.Maui.BarcodeFormat.Code128 |
                    ZXing.Net.Maui.BarcodeFormat.QrCode,
                TryHarder = true,
            };
            _ = CheckCameraPermission();
            BindingContext.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(BindingContext.IsDetectingExternal):
                        keyboardListener.Focus();
                        break;
                    default:
                        break;
                }
            };
        }
        async Task CheckCameraPermission()
        {
            // I notice that, especially on a "first run" after installing
            // the demo app, the ZXing scanner might not want to run. And I 
            // 'think' this is a sequencing issue with obtaining the CAMERA
            // permission. This is an attempted workaround.
            barcodeScanner.IsEnabled = false;
            while (true)
            {
                if (PermissionStatus.Granted == await Permissions.CheckStatusAsync<Permissions.Camera>())
                {
                    barcodeScanner.IsEnabled = true;
                    break;
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
        new BarcodeScannerBindingContext BindingContext => (BarcodeScannerBindingContext)base.BindingContext;
        private void OnBarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            BindingContext.BarcodeLabelText = $"{e.Results.FirstOrDefault()?.Value}";
            BindingContext.IsDetectingInternal = false;
            Vibration.Vibrate();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(sender is Editor editor)
            {
                if (editor.Text.Contains("\r"))
                {
                    BindingContext.BarcodeLabelText = editor.Text.Trim();
                    BindingContext.IsDetectingExternal = false;
#if !WINDOWS
                    Vibration.Vibrate();
#endif
                }
            }
        }
    }
    class BarcodeScannerBindingContext : INotifyPropertyChanged
    {
        public BarcodeScannerBindingContext()
        {
            ScanCommand = new Command(OnScan);
            CancelCommand = new Command(OnCancel);
        }
        public ICommand ScanCommand { get; private set; }
        private void OnScan(object o)
        {
            if (UseExternal)
            {
                IsDetectingExternal = true;
            }
            else
            {
                IsDetectingInternal = true;
            }
        }
        public ICommand CancelCommand { get; private set; }
        private void OnCancel(object o)
        {
            IsDetectingInternal = IsDetectingExternal = false;
        }
        public bool IsDetecting
        {
            get => _isDetecting;
            set
            {
                if (!Equals(_isDetecting, value))
                {
                    _isDetecting = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isDetecting = false;

        public bool UseExternal
        {
            get => _useExternal;
            set
            {
                if (!Equals(_useExternal, value))
                {
                    _useExternal = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _useExternal = false;


        public bool IsDetectingInternal
        {
            get => _isDetectingInternal;
            set
            {
                if (!Equals(_isDetectingInternal, value))
                {
                    _isDetectingInternal = value;
                    OnPropertyChanged();
                }
                IsDetecting = value;
            }
        }
        bool _isDetectingInternal = false;

        public bool IsDetectingExternal
        {
            get => _isDetectingExternal;
            set
            {
                if (!Equals(_isDetectingExternal, value))
                {
                    _isDetectingExternal = value;
                    OnPropertyChanged();
                }
                IsDetecting = value;
            }
        }
        bool _isDetectingExternal = default;

        public string BarcodeLabelText
        {
            get => _barcodeLabelText;
            set
            {
                if (!Equals(_barcodeLabelText, value))
                {
                    _barcodeLabelText = value;
                    OnPropertyChanged();
                }
            }
        }
        string _barcodeLabelText = "|";

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
