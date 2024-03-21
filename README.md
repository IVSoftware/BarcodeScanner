## Barcode Scanner for Maui

So, this is just a perspective, one option. The focus issue can be sidestepped entirely if one chooses to treat a scan as an _activity_. So if there's a barcode field to fill, in this model it would be represented by a label with a tap gesture recognizer. So _cheat to win_ mentality there is no longer a focus issue to begin with. Tapping the label brings up an overlay. 

___
With the option of using the device's camera it might be something like this:

[![scanning with camera][1]][1]

___
If you're using the external bluetooth scanner it might be something like this:

[![scanning with external][2]][2]

___
In either case, the event that occurs when the scan completes causes the result to be copied onto the "box".
___

##### Main Page
```
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
        barcodeScanner.IsEnabled = true;
    }
    new BarcodeScannerBindingContext BindingContext => (BarcodeScannerBindingContext)base.BindingContext;
    private void OnBarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        BindingContext.BarcodeLabelText = $"{e.Results.FirstOrDefault()?.Value}";
        BindingContext.IsDetectingInternal = false;
        Vibration.Vibrate();
    }
}
```
___

##### View Model
```
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
        if(UseExternal)
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
    bool _useExternal = default;


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
    bool _isDetectingInternal = default;

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

    protected virtual void OnPropertyChanged([CallerMemberName]string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public event PropertyChangedEventHandler? PropertyChanged;
}
```
___

##### Xaml
```
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"             
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"             
    xmlns:xzing="clr-namespace:ZXing.Net.Maui.Controls;assembly=ZXing.Net.MAUI.Controls"             
    xmlns:local="clr-namespace:BarcodeScanner"             
    x:Class="BarcodeScanner.MainPage">
    <ContentPage.BindingContext>
        <local:BarcodeScannerBindingContext />
    </ContentPage.BindingContext>
    <ContentPage.Resources>
        <local:InvertBoolConverter x:Key="InvertBoolConverter"/>
    </ContentPage.Resources>
    <Grid>
        <ScrollView
            IsVisible="{Binding IsDetecting, Converter={StaticResource InvertBoolConverter}}">
            <VerticalStackLayout
                Padding="30,0"
                Spacing="25">
                <Image
                    Source="dotnet_bot.png"
                    HeightRequest="185"
                    Aspect="AspectFit"
                    SemanticProperties.Description="dot net bot in a race car number eight" />
                <Label
                    Text="Barcode Scanner"
                    Style="{StaticResource Headline}"
                    SemanticProperties.HeadingLevel="Level1" />
                <Label
                    Text="{Binding BarcodeLabelText}"
                    HorizontalTextAlignment="Center"
                    VerticalTextAlignment="Center"
                    WidthRequest="200"
                    HeightRequest="40"
                    BackgroundColor="Azure"
                    Padding="2,1">
                    <Label.GestureRecognizers>
                        <TapGestureRecognizer Command="{Binding ScanCommand}" />
                    </Label.GestureRecognizers>
                </Label>
                <Grid
                    ColumnDefinitions="*,2*"
                    WidthRequest="200"
                    HorizontalOptions="Center">
                    <CheckBox 
                        IsChecked="{Binding UseExternal}" 
                        HorizontalOptions="End"
                        Grid.Column="0"/>
                    <Label 
                        Grid.Column="1" 
                        VerticalTextAlignment="Center"
                        Margin="10,0,0,0"
                        Text="Use Bluetooth"/>
                </Grid>
            </VerticalStackLayout>
        </ScrollView>
        <Grid
            IsVisible="{Binding IsDetectingInternal}">
            <xzing:CameraBarcodeReaderView
                x:Name="barcodeScanner"
                VerticalOptions="Fill"
                BarcodesDetected="OnBarcodeDetected"
                IsDetecting="{Binding IsDetectingInternal}">
            </xzing:CameraBarcodeReaderView>
            <Grid
                RowDefinitions="*,100,*">
                <Frame
                    Grid.Row="0"
                    BackgroundColor="Black"
                    Opacity="0.75"
                    Margin="0"
                    CornerRadius="0"/>
                <Frame
                    Grid.Row="2"
                    BackgroundColor="Black"
                    CornerRadius="0"
                    Margin="0"
                    Opacity="0.75" />
                <!--Do not move! We need this zorder-->
                <Button
                    Grid.Row="2"
                    Text="Cancel"
                    Margin="0,20"
                    VerticalOptions="End"
                    HeightRequest="40"
                    WidthRequest="200"
                    Command="{Binding CancelCommand}"/>
            </Grid>
        </Grid>
        <Grid
            IsVisible="{Binding IsDetectingExternal}">
            <Frame
                Opacity="0.7"
                BackgroundColor="DarkSlateGray">
            </Frame>
            <ActivityIndicator
                HeightRequest="80"
                VerticalOptions="Start"
                IsRunning="True"
                Margin="0,25"
                Color="White"/>
            <Label
                Grid.Row="2"
                Text="Waiting for Scan..."
                HorizontalTextAlignment="Center"
                Margin="0,20"
                HeightRequest="40"
                WidthRequest="200"
                TextColor="White"
                FontAttributes="Italic"
                FontSize="Medium"/>
            <Button
                Grid.Row="2"
                Text="Cancel"
                Margin="0,20"
                VerticalOptions="End"
                HeightRequest="40"
                WidthRequest="200"
                Command="{Binding CancelCommand}"/>
        </Grid>
    </Grid>
</ContentPage>
```


  [1]: https://i.stack.imgur.com/2PSPM.png
  [2]: https://i.stack.imgur.com/HWjAJ.png