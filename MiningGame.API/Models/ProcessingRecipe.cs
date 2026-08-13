namespace MiningGame.API.Models;

public record RecipeInput(MineType Type, int Quantity);
public record RecipeOutput(MaterialType Type, int Quantity);

public class ProcessingRecipe
{
    public MaterialType Id { get; set; }
    public MaterialType OutputType { get; set; }
    public MineType InputType { get; set; }
    public int InputQuantity { get; set; }
    public int OutputQuantity { get; set; }
    public decimal EnergyCost { get; set; }
}
