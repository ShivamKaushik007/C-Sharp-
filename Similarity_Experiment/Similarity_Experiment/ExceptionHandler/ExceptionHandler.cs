using System;

public class ExceptionHandler : Exception
{
    /// <summary>
    /// It handles the Custom Exception handler.
    /// </summary>
    /// <param name="message"></param>
    public ExceptionHandler(string message) : base(message)
    {

    }

    public ExceptionHandler(string message, Exception innerException) : base(message, innerException)
    {

    }
}

