using System.ComponentModel.DataAnnotations;

namespace CoolNewProject.Domain.Entities;

public class CatalogType {
    public int Id { get; set; }

    [Required] public string Type { get; set; }
}
