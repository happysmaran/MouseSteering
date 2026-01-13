namespace MouseSteering;

public class WindowItem 
{
    public string Title { get; set; } = "";
    public IntPtr Handle { get; set; }
    
    public override string ToString() => Title; 
}