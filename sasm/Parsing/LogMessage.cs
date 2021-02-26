namespace Sasm.Parsing
{
    using System;
    using System.Collections.Generic;

    public struct LogMessage
    {
        public ErrorLevel Level { get; }
        public string Message { get; }
        public SourceReference Source { get; }

        public LogMessage(ErrorLevel level, string message, SourceReference source)
        {
            Level = level;
            Message = message;
            Source = source;
        }

        public override bool Equals(object obj)
        {
            return obj is LogMessage message &&
                   Level == message.Level &&
                   Message == message.Message &&
                   EqualityComparer<SourceReference>.Default.Equals(Source, message.Source);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Level, Message, Source);
        }
    }
}