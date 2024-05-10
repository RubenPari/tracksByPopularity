using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

DotEnv.Load();

app.MapGet("/", () => "Hello World!");

app.Run();
