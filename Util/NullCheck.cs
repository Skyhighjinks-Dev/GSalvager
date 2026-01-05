namespace GSalvager.Util
{
  internal static class NullCheck
  {
    internal static bool CheckIfNotNull(object obj, bool throwIfNull = false)
    { 
      if(throwIfNull && obj == null)
        throw new NullReferenceException(nameof(obj));

      return obj != null;
    }

    internal static bool CheckIfNotNull<T>(object obj, bool throwIfNull = false) where T : Exception, new()
    { 
      if(throwIfNull && obj == null)
        throw (T)Activator.CreateInstance(typeof(T), nameof(obj))!;

      return obj != null;
    }

    internal static bool CheckIfNotNull(object[] objLst, bool throwIfNull = false)
    { 
      foreach(object _ in objLst)
      { 
        if(!CheckIfNotNull(_, throwIfNull))
          return false;
      }

      return true;
    }


    internal static bool CheckIfNotNull<T>(object[] objLst, bool throwIfNull = false) where T : Exception, new()
    { 
      foreach(object _ in objLst)
      { 
        if(!CheckIfNotNull<T>(_, throwIfNull))
          return false;
      }

      return true;
    }
  }
}
