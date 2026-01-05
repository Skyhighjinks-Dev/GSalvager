using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GSalvager.OCR
{
  internal class WindowsStuff
  {
    // Import necessary Windows API functions
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hwnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    // RECT structure to store window coordinates
    public struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
    }

    public static MemoryStream? CaptureWindow(string windowName, Rectangle captureRegion)
    {
      MemoryStream ms = new MemoryStream();
      // Find the game window by its name
      IntPtr hwnd = FindWindow(null!, windowName);  // Provide the window title

      if (hwnd == IntPtr.Zero)
      {
        Console.WriteLine("Window not found.");
        return ms;
      }

      // Ensure window is visible
      if (!IsWindowVisible(hwnd))
      {
        Console.WriteLine("Window is not visible.");
        return ms;
      }

      // Get the window's dimensions
      RECT windowRect;
      GetWindowRect(hwnd, out windowRect);

      // Calculate the capture region relative to the window's top-left corner
      int regionLeft = captureRegion.Left + windowRect.Left;
      int regionTop = captureRegion.Top + windowRect.Top;
      int regionWidth = captureRegion.Width;
      int regionHeight = captureRegion.Height;

      // Create a Bitmap to hold the captured area
      using (Bitmap bmp = new Bitmap(regionWidth, regionHeight))
      {
        // Create Graphics from the Bitmap
        using (Graphics g = Graphics.FromImage(bmp))
        {
          // Use the window's device context and copy the region into the bitmap
          g.CopyFromScreen(regionLeft, regionTop, 0, 0, new Size(regionWidth, regionHeight));
        }

        // Save the captured screenshot to a file
        bmp.Save(ms, ImageFormat.Png);
        return ms;
      }
    }
  }
}
