namespace DirectoryService.Contracts.Departments;

public record DepartmentShortDto  
{  
    public Guid Id { get; init; }  
  
    public string Name { get; init; } = string.Empty;  
  
    public string Identifier { get; init; } = string.Empty;  
  
    public string Path { get; init; } = string.Empty;  
  
    public bool IsActive { get; init; }  
  
    public DateTime CreatedAt { get; init; }  
  
    public DateTime UpdatedAt { get; init; }  
}