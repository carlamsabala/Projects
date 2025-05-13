using System;

namespace Horse.Exception.Interrupted
{
    public class HorseCallbackInterruptedException : Exception
    {
        public HorseCallbackInterruptedException() : base(string.Empty)
        {
        }

        public HorseCallbackInterruptedException(string message) : base(message)
        {
        }
    }
}
