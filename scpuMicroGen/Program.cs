namespace ScpuMicroGen
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System;

    class Program
    {
        const int NumberMicroOpsPerOpcode = 16;
        const string FetchRomName = "Fetch{0}.hex";
        const string ExecuteRomName = "Execute{0}.hex";
        const string RomHeader = "v2.0 raw\n";
        const string HighSpecifier = "High";
        const string LowSpecifier = "Low";
        const string ExtendedSpecifier = "Ext";
        const string DefaultOutputDirectory = "./";
        // ExtInst, IntEn, Carry, Sign, Zero
        const int NumberFlagCombinations = 2 * 2 * 2 * 2 * 2;
        const int NumberOpcodes = 256;

        static Dictionary<string, (StreamWriter writer, Task lastTask)> writeBuffer;

        static void Main(string[] args)
        {
            ParseArgs(args, out var outputDirectory);

            Console.WriteLine($"will write to {outputDirectory}");

            writeBuffer = new Dictionary<string, (StreamWriter, Task)>();

            int maxNumberOfMicroOpsInRom =
                NumberFlagCombinations
                * NumberOpcodes
                * NumberMicroOpsPerOpcode;

            Console.WriteLine($"Build time: {DateTime.Now}");

            var fetchRomPath = Path.Combine(outputDirectory, FetchRomName);
            int fetchOps = GenerateMicroCode(fetchRomPath, FetchGenerator.Generate);
            double percentage = fetchOps * 1.0 / maxNumberOfMicroOpsInRom;
            Console.WriteLine($"{fetchOps} ({percentage:P2}) micro ops in fetch rom");

            var executeRomPath = Path.Combine(outputDirectory, ExecuteRomName);
            int execOps = GenerateMicroCode(executeRomPath, ExecuteGenerator.Generate);
            percentage = execOps * 1.0 / maxNumberOfMicroOpsInRom;
            Console.WriteLine($"{execOps} ({percentage:P2}) micro ops in execute rom");

            CloseStreams();
        }

        private static void ParseArgs(string[] args, out string outputDirectory)
        {
            outputDirectory = null;
            foreach (var argument in args)
            {
                if (argument.StartsWith("-"))
                {
                    // command
                    var commandSplit = argument.Split("=");
                    switch (commandSplit[0])
                    {
                        case "--output":
                        case "-o":
                            if (outputDirectory is null)
                                outputDirectory = commandSplit[1];
                            else
                                throw new ArgumentException("Double assignment of output directory");
                            break;
                    }
                }
                else
                {
                    // interprete this as the output directory
                    if (outputDirectory is null)
                        outputDirectory = argument;
                    else
                        throw new ArgumentException("Double assignment of output directory");
                }
            }

            // default the entries not set
            outputDirectory ??= DefaultOutputDirectory;
        }

        static void CloseStreams()
        {
            Task.WaitAll(writeBuffer.Select(e => e.Value.lastTask).ToArray());
            foreach (var entry in writeBuffer)
            {
                entry.Value.writer.Close();
            }
        }

        static int GenerateMicroCode(string romName, Func<Flags, byte, (ControlLines[], ControlLines2[])> generator)
        {
            int mcOpsCounter = 0;
            for (int f = 0; f < NumberFlagCombinations; f++)
                for (int o = 0; o < NumberOpcodes; o++)
                {
                    var flags = (Flags)f;
                    var primaryOp = (Opcodes)o;
                    var extendedOp = (ExtOpcodes)o;
                    var lines = generator(flags, (byte)o);
                    if (lines.Item1.Length != lines.Item2.Length)
                    {
                        throw new Exception($"{flags} {o}/{primaryOp}/{extendedOp} mismatching length of lines 1 & 2");
                    }
                    mcOpsCounter += lines.Item1.Length;
                    WriteLines(romName, lines);
                }

            return mcOpsCounter;
        }

        static void WriteLines(string romName, (ControlLines[], ControlLines2[]) lines)
        {
            var fileNameHigh = string.Format(romName, HighSpecifier);
            var fileNameLow = string.Format(romName, LowSpecifier);
            var fileNameExt = string.Format(romName, ExtendedSpecifier);

            var padded1 = PadControlLineArray(lines.Item1, NumberMicroOpsPerOpcode);

            WriteHighLines(fileNameHigh, padded1);
            WriteLowLines(fileNameLow, padded1);

            var padded2 = PadControlLineArray(lines.Item2, NumberMicroOpsPerOpcode);

            WriteLowLines(fileNameExt, padded2);
        }

        static void GetOrCreateWriter(string name, Func<StreamWriter, Task> writeFunc)
        {
            (StreamWriter writer, Task lastTask) buffer;
            if (!writeBuffer.TryGetValue(name, out buffer))
            {
                string path = Path.GetDirectoryName(name);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var writer = new StreamWriter(name, false);
                writer.WriteLine(RomHeader);
                var task = writeFunc(writer);
                writeBuffer[name] = (writer, task);
                return;
            }

            Task.WaitAll(buffer.lastTask);
            writeBuffer[name] = (buffer.writer, writeFunc(buffer.writer));
        }

        static void WriteHighLines<T>(string romName, T[] lines)
        where T : struct
        {
            var converted = string.Join(' ', lines.Select(e => ((uint)(Convert.ToUInt64(e) >> 32)).ToString("X")));
            GetOrCreateWriter(romName, w => w.WriteLineAsync(converted));
        }

        static void WriteLowLines<T>(string romName, T[] lines)
        where T : struct
        {
            var converted = string.Join(' ', lines.Select(e => ((uint)Convert.ToUInt64(e)).ToString("X")));
            GetOrCreateWriter(romName, w => w.WriteLineAsync(converted));
        }

        static T[] PadControlLineArray<T>(T[] lines, int padTo)
        {
            if (lines.Length > padTo)
            {
                throw new ArgumentException("Lines was longer than max padding :O");
            }
            if (lines.Length < padTo)
            {
                var padded = new T[padTo];
                Array.Copy(lines, 0, padded, 0, lines.Length);
                return padded;
            }

            return lines;
        }
    }
}