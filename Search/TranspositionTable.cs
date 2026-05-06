namespace ChessBot
{
  public sealed class TranspositionTable
  {

    private readonly TranspositionEntry[] entries;
    private readonly ulong indexMask;

    public TranspositionTable(int entryCount)
    {
      if (entryCount <= 0)
        throw new ArgumentOutOfRangeException(nameof(entryCount));

      if (!IsPowerOfTwo(entryCount))
        throw new ArgumentException("Entry count must be a power of two.");

      entries = new TranspositionEntry[entryCount];
      indexMask = (ulong)(entryCount - 1);
    }

    public void Clear()
    {
      Array.Clear(entries, 0, entries.Length);
    }

    public bool TryGet(ulong key, out TranspositionEntry entry)
    {
      int index = GetIndex(key);

      entry = entries[index];

      return entry.isValid && entry.key == key;
    }

    public void Store(TranspositionEntry entry)
    {
      int index = GetIndex(entry.key);

      TranspositionEntry existingEntry = entries[index];

      if (ShouldReplace(existingEntry, entry))
      {
        entries[index] = entry;
      }
    }

    private static bool ShouldReplace(TranspositionEntry existingEntry, TranspositionEntry newEntry)
    {
      if (!existingEntry.isValid)
        return true;

      return newEntry.depth >= existingEntry.depth;
    }

    private bool IsPowerOfTwo(int value)
    {
      return value > 0 && (value & (value - 1)) == 0;
    }

    private int GetIndex(ulong key)
    {
      return (int)(key & indexMask);
    }

  }
}

