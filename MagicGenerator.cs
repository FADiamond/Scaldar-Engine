using System;

namespace chessBot
{
  public static class MagicGenerator
  {

    public static void generateMagicNumbers()
    {
      rookMagicNumbers();
      bishopMagicNumbers();
    }

    public static void rookMagicNumbers()
    {
      Console.WriteLine("----- ROOK MAGIC NUMBERS -----");
      Random random = new();
      Attacks.initRookMask();
      for (byte squareNbr = 0; squareNbr < 64; squareNbr++)
      {
        ulong mask = Attacks.RookMask[squareNbr];
        int bitsInMask = Constants.RookRelevantBits[squareNbr];

        int relevantBits = BitboardHelper.NumberOfSetBits(mask);
        if (bitsInMask != relevantBits)
        {
          Console.WriteLine($"Mismatch: bitsInMask={bitsInMask}, relevantBits={relevantBits} (square {squareNbr})");
          return;
        }
        int occupanciesVariationAmounts = 1 << Constants.RookRelevantBits[squareNbr];

        ulong[] occupancies = new ulong[occupanciesVariationAmounts];
        ulong[] attacks = new ulong[occupanciesVariationAmounts];

        for (int variationIndex = 0; variationIndex < occupanciesVariationAmounts; variationIndex++)
        {
          ulong occupancy = Attacks.getMaskOccupancyBitboard(variationIndex, bitsInMask, mask);
          occupancies[variationIndex] = occupancy;

          attacks[variationIndex] = Attacks.generateRookPossibleMoves(squareNbr, occupancy);

        }
        while (true)
        {
          ulong magicNumber = (ulong)(random.NextInt64() & random.NextInt64() & random.NextInt64());

          ulong[] usedAttacks = new ulong[occupanciesVariationAmounts];
          bool[] filled = new bool[occupanciesVariationAmounts];

          bool fail = false;

          for (int variationIndex = 0; variationIndex < occupanciesVariationAmounts; variationIndex++)
          {
            ulong occupancy = occupancies[variationIndex];

            int magicIndex = (int)((occupancy * magicNumber) >> (64 - relevantBits));

            ulong attack = attacks[variationIndex];

            if (!filled[magicIndex])
            {
              filled[magicIndex] = true;
              usedAttacks[magicIndex] = attack;
            }
            else if (usedAttacks[magicIndex] != attack)
            {
              fail = true;
              break;
            }
          }

          if (!fail)
          {
            Console.WriteLine("0x" + magicNumber.ToString("X16") + "UL,");
            break;
          }
        }
      }

      Console.WriteLine("------------------------------");
    }


    public static void bishopMagicNumbers()
    {
      Console.WriteLine("----- BISHOP MAGIC NUMBERS -----");
      Random random = new();
      Attacks.initBishopMask();
      for (byte squareNbr = 0; squareNbr < 64; squareNbr++)
      {
        ulong mask = Attacks.BishopMask[squareNbr];
        int bitsInMask = Constants.BishopRelevantBits[squareNbr];

        int relevantBits = BitboardHelper.NumberOfSetBits(mask);
        if (bitsInMask != relevantBits)
        {
          Console.WriteLine($"Mismatch: bitsInMask={bitsInMask}, relevantBits={relevantBits} (square {squareNbr})");
          return;
        }
        int occupanciesVariationAmounts = 1 << Constants.BishopRelevantBits[squareNbr];

        ulong[] occupancies = new ulong[occupanciesVariationAmounts];
        ulong[] attacks = new ulong[occupanciesVariationAmounts];

        for (int variationIndex = 0; variationIndex < occupanciesVariationAmounts; variationIndex++)
        {
          ulong occupancy = Attacks.getMaskOccupancyBitboard(variationIndex, bitsInMask, mask);
          occupancies[variationIndex] = occupancy;

          attacks[variationIndex] = Attacks.generateBishopPossibleMoves(squareNbr, occupancy);

        }
        while (true)
        {
          ulong magicNumber = (ulong)(random.NextInt64() & random.NextInt64() & random.NextInt64());

          ulong[] usedAttacks = new ulong[occupanciesVariationAmounts];
          bool[] filled = new bool[occupanciesVariationAmounts];

          bool fail = false;

          for (int variationIndex = 0; variationIndex < occupanciesVariationAmounts; variationIndex++)
          {
            ulong occupancy = occupancies[variationIndex];

            int magicIndex = (int)((occupancy * magicNumber) >> (64 - relevantBits));

            ulong attack = attacks[variationIndex];

            if (!filled[magicIndex])
            {
              filled[magicIndex] = true;
              usedAttacks[magicIndex] = attack;
            }
            else if (usedAttacks[magicIndex] != attack)
            {
              fail = true;
              break;
            }
          }

          if (!fail)
          {
            Console.WriteLine("0x" + magicNumber.ToString("X16") + "UL,");
            break;
          }
        }
      }
      Console.WriteLine("------------------------------");
    }
  }
}

