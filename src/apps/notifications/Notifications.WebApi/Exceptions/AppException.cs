﻿namespace Genocs.Notifications.WebApi.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message)
        : base(message)
    {
    }
}
