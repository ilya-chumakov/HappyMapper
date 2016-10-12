using System;

namespace HappyMapper.Text
{
    public class Block : IDisposable
    {
        protected Recorder Recorder { get; set; }

        public Block(Recorder recorder, string statement, string condition = null)
        {
            Recorder = recorder;

            Recorder.Append($"{statement} ");
            if (condition != null) Recorder.AppendLine($"({condition})");
            Recorder.AppendLine("{");
        }

        public void Dispose()
        {
            Recorder.AppendLine("}");
        }
    }
}