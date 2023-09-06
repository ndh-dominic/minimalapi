/*
 * Setup
 */
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


/*
 * Endpoints
 */
app.MapGet("/api/ping", () => Results.NoContent());
app.MapGet("/{files}", async (HttpContext context) =>
{
    if (!int.TryParse(context.Request.RouteValues["files"] as string, out int numberOfFiles))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid number of files.");
        return;
    }

    // Enable cancellation
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;
    context.RequestAborted.Register(() => cancellationTokenSource.Cancel());

    using (var zipStream = new MemoryStream())
    {
        try
        {
            var fileName = "test.zip";
            context.Response.Headers.Add("Content-Type", "application/octet-stream");
            context.Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");

            await Zip.WriteAsync(zipStream, numberOfFiles, cancellationToken);

            zipStream.Seek(0, SeekOrigin.Begin);
            await zipStream.CopyToAsync(context.Response.Body, cancellationToken); // Stream data to response body
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Request cancelled.");

            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Request cancelled.");
        }
    }
});

app.Run();