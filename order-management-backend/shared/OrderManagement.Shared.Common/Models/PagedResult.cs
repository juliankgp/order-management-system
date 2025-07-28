namespace OrderManagement.Shared.Common.Models;

/// <summary>
/// Modelo para paginación de resultados
/// </summary>
/// <typeparam name="T">Tipo de elementos en la lista</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Lista de elementos de la página actual
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// Número total de elementos
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Número de página actual (empezando en 1)
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Número total de páginas
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPrevious => CurrentPage > 1;
    
    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNext => CurrentPage < TotalPages;
}

/// <summary>
/// Parámetros para paginación
/// </summary>
public class PaginationParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    
    /// <summary>
    /// Número de página (empezando en 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Tamaño de página (máximo 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    
    /// <summary>
    /// Campo para ordenamiento
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Término de búsqueda
    /// </summary>
    public string? SearchTerm { get; set; }
}
