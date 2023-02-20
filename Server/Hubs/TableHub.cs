using Microsoft.AspNetCore.SignalR;

namespace TCP_TCP.Server.Hubs;

public class TableHub : Hub
{
    public async Task SeatedForShow(string seatPosition)
    {
        await Clients.All.SendAsync("ToggleSeat", seatPosition);
    }

    public async Task StayingForSecondShow(string seatPosition)
    {
        await Clients.All.SendAsync("DoubleActive", seatPosition);
    }

    public async Task ResetSeats()
    {
        await Clients.All.SendAsync("SeatsReset");
    }
}