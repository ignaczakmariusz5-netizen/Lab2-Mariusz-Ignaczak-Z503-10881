using NSubstitute;
using Microsoft.Extensions.Logging;

namespace Lab2.Tests;

public class CarFactoryTests
{
    [Test]
    public async Task BuildCar_ValidModel_ReturnsAssembledCar()
    {
        // 1. Arrange - "Test doubles"
        var engineFactoryStub = Substitute.For<IEngineFactory>();
        var bodyFactoryStub = Substitute.For<IBodyFactory>();
        var loggerDummy = Substitute.For<ILogger<CarFactory>>(); 

        var expectedEngine = new Engine(500, "V8");
        var expectedBody = new Body("Red", "High-Strength");

        engineFactoryStub.Build(Arg.Any<string>()).Returns(expectedEngine);
        bodyFactoryStub.Build(Arg.Any<string>()).Returns(expectedBody);
        
        var carFactory = new CarFactory(engineFactoryStub, bodyFactoryStub, loggerDummy);

        // 2. Act
        var result = carFactory.Build("Mustang");

        // 3. Assert (Styl TUnit)
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Model).IsEqualTo("Mustang");
        await Assert.That(result.Engine).IsEqualTo(expectedEngine);
        await Assert.That(result.Body).IsEqualTo(expectedBody);
        
        // Weryfikacja wywołań (Styl Londyński)
        engineFactoryStub.Received().Build("V8");
        bodyFactoryStub.Received().Build("Red");
    }

    [Test]
    public async Task BuildCar_EmptyModel_ThrowsArgumentException()
    {
        // Arrange
        var engineFactoryDummy = Substitute.For<IEngineFactory>(); 
        var bodyFactoryDummy = Substitute.For<IBodyFactory>();
        var loggerDummy = Substitute.For<ILogger<CarFactory>>();

        var carFactory = new CarFactory(engineFactoryDummy, bodyFactoryDummy, loggerDummy);

        // Act & Assert
        await Assert.That(() => carFactory.Build(""))
            .Throws<ArgumentException>();
    }
}

public class EngineFactoryTests
{
    [Test]
    public async Task BuildEngine_HighPurityAluminium_ReturnsEngine()
    {
        // Arrange
        var aluminiumProviderStub = Substitute.For<IAluminiumProvider>();
        var loggerDummy = Substitute.For<ILogger<EngineFactory>>();

        aluminiumProviderStub.Provide().Returns(new Aluminium(0.95f, "A356"));

        var engineFactory = new EngineFactory(aluminiumProviderStub, loggerDummy);

        // Act
        var result = engineFactory.Build("V8");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Type).IsEqualTo("V8");
    }

    [Test]
    public async Task BuildEngine_LowPurityAluminium_ThrowsInvalidOperationException()
    {
        // Arrange
        var aluminiumProviderStub = Substitute.For<IAluminiumProvider>();
        var loggerDummy = Substitute.For<ILogger<EngineFactory>>();

        aluminiumProviderStub.Provide().Returns(new Aluminium(0.5f, "Bad Grade"));

        var engineFactory = new EngineFactory(aluminiumProviderStub, loggerDummy);

        // Act & Assert
        await Assert.That(() => engineFactory.Build("V8"))
            .Throws<InvalidOperationException>()
            .WithMessage("Aluminium purity is too low for engine production.");
    }
}