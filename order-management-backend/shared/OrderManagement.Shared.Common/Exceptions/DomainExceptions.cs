namespace OrderManagement.Shared.Common.Exceptions;

/// <summary>
/// Excepción personalizada para errores de dominio
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Excepción para entidades no encontradas
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
    }
}

/// <summary>
/// Excepción para errores de validación
/// </summary>
public class ValidationException : DomainException
{
    public List<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(List<string> errors) : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

/// <summary>
/// Excepción para conflictos de negocio
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
