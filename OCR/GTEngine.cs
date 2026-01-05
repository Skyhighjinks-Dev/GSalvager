using GSalvager.Util;
using System.Diagnostics;
using System.Drawing;
using Tesseract;

namespace GSalvager.OCR
{
  /// <summary>
  /// Tess OCR Engine
  /// </summary>
  internal static class GTEngine
  {
    private static bool _Running = false;
    private static InventorySlotValue _values = new(0, 0);
    private static object _lock = new();

    private static string GameName;
    private static Rectangle InventorySlotIndicatorArea;

    private static bool _isInitialized = false;

    private static bool Running
    { 
      get
      { 
        return _Running;  
      }
      set
      { 
        if(value == _Running) // Dont want to mess up process if attempted to do the same action as currently happening
          return;

        if(!_isInitialized)
          throw new InvalidOperationException($"{nameof(GTEngine)} must be initialized before running the process!");

        _Running = value;
        if (_Running) 
          StartTaskAsync();
      }
    }

    public static InventorySlotValue GetSlotValues()
    { 
      lock(_lock)
      { 
        return (InventorySlotValue)_values.Clone();
      }
    }

    public static void Initialize(string gameName, Rectangle invSlotRec, bool start = false)
    { 
      if(_isInitialized)
        throw new InvalidOperationException($"{nameof(GTEngine)} has already been initialized!");

      GameName = gameName;
      InventorySlotIndicatorArea = invSlotRec;
      _isInitialized = true;
      if(start)
        StartProcess();
    }

    public static void StartProcess() => Running = true;
    public static void StopProcess() => Running = false;
    public static void StartStopProcess() => Running = !Running; // toggle

    private static async Task StartTaskAsync()
    { 
      Console.WriteLine("Started main process!");
      while(Running)
      { 
        Stopwatch sw = Stopwatch.StartNew();
        await GetInvSlotsValuesAsync();

        Console.WriteLine($"Getting values took: {sw.ElapsedMilliseconds}");

        /* Operation takes roughly 70-80ms after first iteration at runtime (~150ms during debug on first run) 
           Shouldn't eat up all the resources, as other OCR will be needed too */
        int elapsedMs = (int)sw.ElapsedMilliseconds;
        if (250-elapsedMs>0) 
          await Task.Delay(250-elapsedMs);
      }
    }

    private static async Task GetInvSlotsValuesAsync()
    { 
      try
      { 
        using var eng = new TesseractEngine("./Tessadata", "eng", EngineMode.Default);
        using MemoryStream mem = WindowsStuff.CaptureWindow(GameName, InventorySlotIndicatorArea)!;

        NullCheck.CheckIfNotNull((eng, mem), true);

        using var pix = Pix.LoadFromMemory(mem.ToArray());
        NullCheck.CheckIfNotNull(pix, true);

        var result = eng.Process(pix);
        NullCheck.CheckIfNotNull(result, true);

        var values = ExtractInventorySlotsData(result);
        if(values == null)
          return;

        _values.UpdateValue(values.UsedSlots, values.MaxSlots);

        Console.WriteLine((_values == null ? "Null" : $"{_values.UsedSlots}/{_values.MaxSlots}"));
      }
      catch(Exception ex)
      { 
        Console.WriteLine(ex);  
      }
    }

    private static InventorySlotValue ExtractInventorySlotsData(Page page)
    { 
      string content = page.GetText();
      if(string.IsNullOrEmpty(content))
        return null!;

      string[] contentSplit = content.Replace(" ", "").Split('/');
      if (contentSplit.Length != 2 || 
          !int.TryParse(contentSplit[0], out int usedSlots) || 
          !int.TryParse(contentSplit[1], out int maxSlots))
        return null!;

      return new InventorySlotValue(usedSlots, maxSlots);
    }
  }

  internal class InventorySlotValue : ICloneable
  { 
    public int UsedSlots { get; private set; }
    public int MaxSlots { get; private set; }

    private object _lock = new object();

    public InventorySlotValue(int usedSlots, int maxSlots)
    { 
      this.UsedSlots = usedSlots;
      this.MaxSlots = maxSlots;
    }

    public void UpdateValue(int usedSlots, int maxSlots)
    { 
      lock(_lock)
      { 
        this.UsedSlots = usedSlots;
        this.MaxSlots = maxSlots;
      }
    }

    public object Clone()
    { 
      lock(_lock)
      { 
        return new InventorySlotValue(this.UsedSlots, this.MaxSlots);
      }
    }
  }
}
