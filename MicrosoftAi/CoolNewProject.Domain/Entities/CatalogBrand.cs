using System.ComponentModel.DataAnnotations;

namespace CoolNewProject.Domain.Entities;

public class CatalogBrand {
    public int Id { get; set; }

    [Required] public string Brand { get; set; }
}
