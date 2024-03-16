#nullable disable

using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;

string checkIP = Directory.GetCurrentDirectory().ToString() == "C:\\Users\\asus\\source\\repos\\NetworkScreenSharing\\ServerSide\\bin\\Debug\\net6.0" ? "192.168.56.1" : "127.0.0.1";

var ip = IPAddress.Parse(checkIP);
var port = 27001;

var listenerEP = new IPEndPoint(ip, port);
var listener = new UdpClient(listenerEP);


IPEndPoint? remoteEP = null;
while (true)
{
    var result = await listener.ReceiveAsync();
    if (result != null)
    {
        Console.WriteLine($"{result.RemoteEndPoint} Connected To Server.");
        _ = Task.Run(async () =>
        {
            IPEndPoint? remoteEP = result.RemoteEndPoint;

            while (true)
            {
                var screenImage = await TakeScreenShotAsync();
                var imgBytes = await ImageToByteAsync(screenImage);
                var chunks = imgBytes?.Chunk(ushort.MaxValue - 29);
                foreach (var chunk in chunks!)
                    await listener.SendAsync(chunk, chunk.Length, remoteEP);
            }
        });
    }
}

async Task<Image?> TakeScreenShotAsync()
{
    var width = Screen.PrimaryScreen.Bounds.Width;
    var height = Screen.PrimaryScreen.Bounds.Height;
    Bitmap? bitmap = new Bitmap(width, height);
    using Graphics graphics = Graphics.FromImage(bitmap);
    graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

    return bitmap;
}

async Task<byte[]?> ImageToByteAsync(Image? image)
{
    using MemoryStream ms = new MemoryStream();
    image?.Save(ms, ImageFormat.Jpeg);

    return ms.ToArray();
}