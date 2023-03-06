namespace TCP_TCP.Shared.Model;

public class Table
{
    public Table()
    {
        // used for desserialization
        Seats = new List<Seat>();
    }
    
    public Table(string position, int row, int seatCount)
    {
        Position = position;
        Row = row;

        Seats = new List<Seat>();

        for(var s = 0; s < seatCount; s++)
        {
            Seats.Add(new Seat() { Position = $"{position}.{s+1}" } );
        }
    }

    public string Position { get; set; } = default!;

    public int Row { get; set; } = default!;

    public string Note { get; set; } = default!;

    public List<Seat> Seats { get; set; } = default!;
}