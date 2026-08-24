using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameCompanyTests
{
    [Fact]
    public void Constructor_ShouldCreateGameCompanyWithProvenance()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var game = new Game("The Legend of Zelda: Ocarina of Time 3D");
        var company = new Company("Grezzo");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "game-1",
            game.Id);

        var externalCompanyRecord = CreateLinkedExternalCompanyRecord(
            dataSourceId,
            "company-1",
            company.Id);

        // Act
        var gameCompany = new GameCompany(
            externalGameRecord,
            externalCompanyRecord,
            GameCompanyRole.Developer);

        // Assert
        gameCompany.GameId.ShouldBe(game.Id);
        gameCompany.CompanyId.ShouldBe(company.Id);
        gameCompany.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        gameCompany.ExternalCompanyRecordId.ShouldBe(externalCompanyRecord.Id);
        gameCompany.Role.ShouldBe(GameCompanyRole.Developer);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordIsNull()
    {
        // Arrange
        var company = new Company("Grezzo");

        var externalCompanyRecord = CreateLinkedExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            company.Id);

        // Act
        var action = () => new GameCompany(
            null!,
            externalCompanyRecord,
            GameCompanyRole.Developer);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalCompanyRecordIsNull()
    {
        // Arrange
        var game = new Game("The Legend of Zelda: Ocarina of Time 3D");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            Guid.NewGuid(),
            "game-1",
            game.Id);

        // Act
        var action = () => new GameCompany(
            externalGameRecord,
            null!,
            GameCompanyRole.Developer);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordIsNotLinked()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var company = new Company("Grezzo");

        var externalGameRecord = CreateExternalGameRecord(
            dataSourceId,
            "game-1");

        var externalCompanyRecord = CreateLinkedExternalCompanyRecord(
            dataSourceId,
            "company-1",
            company.Id);

        // Act
        var action = () => new GameCompany(
            externalGameRecord,
            externalCompanyRecord,
            GameCompanyRole.Developer);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalCompanyRecordIsNotLinked()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var game = new Game("The Legend of Zelda: Ocarina of Time 3D");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "game-1",
            game.Id);

        var externalCompanyRecord = CreateExternalCompanyRecord(
            dataSourceId,
            "company-1");

        // Act
        var action = () => new GameCompany(
            externalGameRecord,
            externalCompanyRecord,
            GameCompanyRole.Developer);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalRecordsBelongToDifferentDataSources()
    {
        // Arrange
        var game = new Game("The Legend of Zelda: Ocarina of Time 3D");
        var company = new Company("Grezzo");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            Guid.NewGuid(),
            "game-1",
            game.Id);

        var externalCompanyRecord = CreateLinkedExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            company.Id);

        // Act
        var action = () => new GameCompany(
            externalGameRecord,
            externalCompanyRecord,
            GameCompanyRole.Developer);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid dataSourceId, string externalId, Guid gameId)
    {
        var record = CreateExternalGameRecord(dataSourceId, externalId);
        record.LinkToGame(gameId);

        return record;
    }

    private static ExternalGameRecord CreateExternalGameRecord(Guid dataSourceId, string externalId) =>
        new(
            dataSourceId,
            externalId,
            DateTimeOffset.UtcNow);

    private static ExternalCompanyRecord CreateLinkedExternalCompanyRecord(Guid dataSourceId, string externalId, Guid companyId)
    {
        var record = CreateExternalCompanyRecord(dataSourceId, externalId);
        record.LinkToCompany(companyId);

        return record;
    }

    private static ExternalCompanyRecord CreateExternalCompanyRecord(Guid dataSourceId, string externalId) =>
        new(
            dataSourceId,
            externalId,
            DateTimeOffset.UtcNow);
}