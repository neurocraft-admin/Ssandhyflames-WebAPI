namespace WebAPI.Models
{
    public record ProductCategoryDto(int CategoryId, string CategoryName);

    public record ProductSubCategoryDto(int SubCategoryId, int CategoryId, string SubCategoryName);
}
