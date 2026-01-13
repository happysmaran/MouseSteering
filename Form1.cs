namespace MouseSteering;

using vJoyInterfaceWrap;
public partial class Form1 : Form
{
    private ComboBox windowComboBox;
    private Button refreshBtn;
    private System.Windows.Forms.Timer mainTimer;
    private Label statusLabel;
    private vJoy joystick;
    private uint deviceId = 1;

    public Form1()
    {
        windowComboBox = new ComboBox { Location = new Point(10, 10), Width = 250 };
        refreshBtn = new Button { Location = new Point(270, 10), Text = "Refresh" };
        statusLabel = new Label { Location = new Point(10, 50), Text = "Steering: 0%", Width = 200 };
        
        mainTimer = new System.Windows.Forms.Timer();
        mainTimer.Interval = 10; // 10ms polling for smooth steering
        mainTimer.Tick += MainTimer_Tick;

        refreshBtn.Click += (s, e) => RefreshWindowList();

        this.Controls.Add(windowComboBox);
        this.Controls.Add(refreshBtn);
        this.Controls.Add(statusLabel);
        this.Text = "Mouse Steering Feeder";

        joystick = new vJoy();
        if (!joystick.vJoyEnabled()) {
            MessageBox.Show("vJoy driver not found or disabled!");
        }
        joystick.AcquireVJD(deviceId);

        // Initial scan
        RefreshWindowList();
        mainTimer.Start();
    }

    private void RefreshWindowList() 
    {
        windowComboBox.Items.Clear();
        
        WinApi.EnumWindows((hWnd, lParam) => {
            if (WinApi.IsWindowVisible(hWnd)) {
                var sb = new System.Text.StringBuilder(256);
                WinApi.GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                if (!string.IsNullOrWhiteSpace(title) && title != this.Text) {
                    windowComboBox.Items.Add(new WindowItem { Title = title, Handle = hWnd });
                }
            }
            return true;
        }, 0);
    }

    private void MainTimer_Tick(object? sender, EventArgs e) 
    {
        if (windowComboBox.SelectedItem is WindowItem selected) {
            if (WinApi.GetWindowRect(selected.Handle, out var rect)) {
                WinApi.GetCursorPos(out var mouse);

                float width = rect.Right - rect.Left;
                
                // Calculate raw ratio
                float rawRatio = (float)(mouse.X - rect.Left) / width;

                // Clamp between 0.0 and 1.0
                float clampedRatio = Math.Clamp(rawRatio, 0f, 1f);

                // Add a tiny deadzone to compensate for human error
                if (Math.Abs(clampedRatio - 0.5f) < 0.02f) {
                    clampedRatio = 0.5f;
                }

                statusLabel.Text = $"Steering: {clampedRatio:P0}";
                
                // 1. Map 0.0-1.0 to 1-32767 (vJoy doesn't like 0 for some reason)
                int vJoyValue = (int)(clampedRatio * 32767);
                if (vJoyValue < 1) vJoyValue = 1;

                // 2. Set the X Axis (HID_USAGE_X is the standard for Steering)
                // The parameters are: (Value, DeviceID, Usage)
                joystick.SetAxis(vJoyValue, deviceId, HID_USAGES.HID_USAGE_X);
            }
        }
    }
}