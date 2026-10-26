namespace backend.Entities;

// CardChoice attributes:
// Id (int) primary key
// RunId (int) foreign key
// CardId (string) 
// Picked (boolean)
// Act (int)
// Floor (int)
// Source (string)

public class CardChoice
{
    public int Id { get; set; }

    public int RunId { get; set; }

    public string CardId { get; set; } = string.Empty;

    public bool Picked { get; set; }

    public int Act { get; set; }

    public int Floor { get; set; }

    public string Source { get; set; } = string.Empty;
}