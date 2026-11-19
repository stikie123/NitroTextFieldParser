using BenchmarkDotNet.Running;

namespace NitroTextFieldParserTests.BenchMarks;

public class Program
{
   static void Main(string[] args)
   {
#if DEBUG
     var instance = new SimpleCsvBenchMarks();
     instance.NitroTextFieldParser();
#else
    BenchmarkRunner.Run<SimpleCsvBenchMarks>(args: args);
#endif

  }
}