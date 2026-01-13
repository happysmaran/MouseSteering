namespace MouseSteering;

static class Program
{
    [STAThread]
    static void Main()
    {
        // This part is for handling different DPIs.
        // Without this, GetCursorPos and GetWindowRect will return wrong numbers
        ApplicationConfiguration.Initialize(); 
        
        Application.Run(new Form1());
    }
}