## Barcode Scanner for Maui

As I (now) understand it, your 'not' doing low-level HID or bluetooth IO and the reason for the `Entry` is to listen for the virtual "keystrokes" produced by the wireless scanner. The reason that your calls to `keyboardListener.HideSoftInputAsync(default)` aren't working is that they apparently are happening _before_ the soft input has had a chance to open to begin with. Here's how you can fix that, albeit not without seeing a flash when the soft input opens for a split second.

```
/// <summary>
/// Kill the soft keyboard when it (finally) opens. 
/// </summary>
private void OnKeyboardListenerFocused(object sender, FocusEventArgs e)
{
    _ = localInterceptKeyboardShow();

    async Task localInterceptKeyboardShow()
    {
        while(keyboardListener.IsFocused)
        {
            if(keyboardListener.IsSoftInputShowing())
            {
                await keyboardListener.HideSoftInputAsync(default);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }
    }
}
```