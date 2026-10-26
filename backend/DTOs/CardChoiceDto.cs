namespace backend.DTOs;

public class CardChoiceDto
{
    public int Id { get; set; }

    public string CardId { get; set; } = string.Empty;

    public bool Picked { get; set; }

    public int Act { get; set; }

    public int Floor { get; set; }

    public string Source { get; set; } = string.Empty;
}