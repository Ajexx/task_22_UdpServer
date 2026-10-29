using System.Net;
using System.Net.Sockets;
using System.Text;

int port = 5000;

IPAddress address = IPAddress.Parse("239.1.1.1");

using var udpClient = new UdpClient();

udpClient.ExclusiveAddressUse = false;

udpClient.Client.SetSocketOption(
    SocketOptionLevel.Socket,
    SocketOptionName.ReuseAddress,
    true);

udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

udpClient.JoinMulticastGroup(address);

var cts = new CancellationTokenSource();

Task task = Task.Run(() =>
{
    while (Console.ReadLine() != "exit")
    {}
    cts.Cancel();
});

Console.WriteLine("Receiver started. Print exit to stop.");
try
{
    await RecieveResultAsync(udpClient, cts.Token);
}

catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

finally
{
    cts.Dispose();
    udpClient.DropMulticastGroup(address);
}

async Task RecieveResultAsync(UdpClient udpClient, CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        try
        {
            var receiveResult = await udpClient.ReceiveAsync(token);
            var buff = Encoding.UTF8.GetString(receiveResult.Buffer);
            string[] parts = buff.Split('|', 3);

            if (parts.Length == 3 && DateTime.TryParse(parts[0], out DateTime time))
            {
                Console.WriteLine($"[{time:HH:mm:ss}] {parts[1]}: {parts[2]}");
            }
            else
            {
                Console.WriteLine("Message has incorrect format");
            }
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }
}