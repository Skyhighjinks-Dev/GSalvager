using GSalvager.OCR;
using GSalvager.Util;

namespace GSalvager
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      StartProcesses();

      await Task.Delay(-1);
    }

    static void StartProcesses()
    { 
      GTEngine.Initialize(GSalvagerConfig.GameName,
        new System.Drawing.Rectangle(
          GSalvagerConfig.InvPosStartX,
          GSalvagerConfig.InvPosStartY,
          GSalvagerConfig.InvPosWidth,
          GSalvagerConfig.InvPosHeight),
        GSalvagerConfig.StartOnLaunch);
    }
  }
}