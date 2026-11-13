using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("=======================================");
Console.WriteLine("Student: Mariusz Ignaczak, Album: [10881]");
Console.WriteLine("=======================================\n");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddTransient<IAluminiumProvider, AluminiumProvider>();
        services.AddTransient<ISteelProvider, SteelProvider>();
        services.AddTransient<IEngineFactory, EngineFactory>();
        services.AddTransient<IBodyFactory, BodyFactory>();
        services.AddTransient<ICarFactory, CarFactory>();
    })
    .Build();

var carFactory = host.Services.GetRequiredService<ICarFactory>();

try
{
    var myCar = carFactory.Build("Mustang");
    Console.WriteLine($"\n[SUKCES] Wyprodukowano samochód: {myCar.Model}, Silnik: {myCar.Engine.HorsePower}KM, Kolor: {myCar.Body.Color}");
}
catch (Exception ex)
{
    Console.WriteLine($"\n[BŁĄD] {ex.Message}");
}

//REKORDY
public record Car(string Model, Engine Engine, Body Body);
public record Engine(int HorsePower, string Type);
public record Body(string Color, string Material);
public record Aluminium(float Purity, string Grade);
public record Steel(float Strength, string Type);

//INTERFEJSY (Styl Londyński)
public interface IAluminiumProvider { Aluminium Provide(); }
public interface ISteelProvider { Steel Provide(); }
public interface IEngineFactory { Engine Build(string type); }
public interface IBodyFactory { Body Build(string color); }
public interface ICarFactory { Car Build(string model); }

//IMPLEMENTACJE
public class AluminiumProvider(ILogger<AluminiumProvider> logger) : IAluminiumProvider
{
    public Aluminium Provide()
    {
        logger.LogInformation("Wydobywanie aluminium z kopalni...");
        return new Aluminium(0.95f, "A356");
    }
}

public class SteelProvider(ILogger<SteelProvider> logger) : ISteelProvider
{
    public Steel Provide()
    {
        logger.LogInformation("Pozyskiwanie stali z huty...");
        return new Steel(500f, "High-Strength");
    }
}

public class EngineFactory(IAluminiumProvider aluminiumProvider, ILogger<EngineFactory> logger) : IEngineFactory
{
    public Engine Build(string type)
    {
        logger.LogInformation("Rozpoczęto produkcję silnika typu: {Type}", type);
        var aluminium = aluminiumProvider.Provide();

        if (aluminium.Purity < 0.9f)
        {
            logger.LogError("Zbyt niska czystość aluminium!");
            throw new InvalidOperationException("Aluminium purity is too low for engine production.");
        }

        logger.LogInformation("Zakończono produkcję silnika.");
        return new Engine(450, type);
    }
}

public class BodyFactory(ISteelProvider steelProvider, ILogger<BodyFactory> logger) : IBodyFactory
{
    public Body Build(string color)
    {
        logger.LogInformation("Rozpoczęto produkcję karoserii w kolorze: {Color}", color);
        var steel = steelProvider.Provide();

        if (steel.Strength < 400f)
        {
            logger.LogError("Stal jest zbyt słaba!");
            throw new InvalidOperationException("Steel is not strong enough.");
        }

        logger.LogInformation("Zakończono produkcję karoserii.");
        return new Body(color, steel.Type);
    }
}

public class CarFactory(IEngineFactory engineFactory, IBodyFactory bodyFactory, ILogger<CarFactory> logger) : ICarFactory
{
    public Car Build(string model)
    {
        logger.LogInformation("--- Zlecenie produkcji samochodu: {Model} ---", model);

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model name cannot be empty");

        var engine = engineFactory.Build("V8");
        var body = bodyFactory.Build("Red");

        logger.LogInformation("--- Samochód {Model} zmontowany pomyślnie ---", model);
        return new Car(model, engine, body);
    }
}