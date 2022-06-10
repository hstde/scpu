using System;
using System.Collections.Generic;
using System.IO;

namespace Sasm
{
    public class EvaluationContext
    {
        public TextWriter Out { get; }
        public TextWriter Error { get; }
        public TextReader In { get; }

        private Dictionary<string, object> constantStore;

        public EvaluationContext(
            TextWriter outStream,
            TextWriter errorStream,
            TextReader inStream)
        {
            Out = outStream;
            Error = errorStream;
            In = inStream;

            constantStore = new Dictionary<string, object>();
        }

        public object GetConstant(string name)
        {
            return constantStore[name];
        }

        public void SetConstant(string name, object value)
        {
            constantStore[name] = value;
        }

        public static EvaluationContext CreateDefault()
        {
            return new EvaluationContext(Console.Out, Console.Error, Console.In);
        }
    }
}