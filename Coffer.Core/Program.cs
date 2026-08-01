using System;
using System.IO;

namespace Coffer.Core
{
  class Program
  {
    static void Main(String[] args)
    {
      Console.WriteLine("Enter Source root.");
      string SourceRoot = Console.ReadLine();
      Console.WriteLine("Enter Destination root.");
      string DestRoot = Console.ReadLine();

      Console.WriteLine($"Source root - {SourceRoot}");
      Console.WriteLine($"Destination root - {DestRoot}");
    }
  }
}
