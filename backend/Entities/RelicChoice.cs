namespace backend.Entities;

// RelicChoice attributes: 
// Id (int) primary key
// RunId (int) foreign key
// RelicId (string) 
// Picked (boolean)
// Act (int)
// Floor (int)
// Source (string) 

public class RelicChoice
{
    public int Id { get; set; }

    public int RunId { get; set; }

    public string RelicId { get; set; } = string.Empty;

    public bool Picked { get; set; }

    public int Act { get; set; }

    public int Floor { get; set; }

    public string Source { get; set; } = string.Empty;
}