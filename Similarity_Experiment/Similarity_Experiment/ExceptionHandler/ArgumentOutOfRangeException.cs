using System;

public class ArgumentOutOfRangeException : Exception
{
    /// <summary>
    /// It handles the Custom Exception handler.
    /// </summary>
    /// <param name="message"></param>
    public ArgumentOutOfRangeException(string message) : base(message)
    {

    }

    public ArgumentOutOfRangeException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
