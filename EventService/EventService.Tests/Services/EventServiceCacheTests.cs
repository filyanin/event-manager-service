using EventService.Application.DTOs;
using EventService.Application.Interfaces;
using EventService.Application.Services;
using EventService.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Contracts.Configuration;
using Xunit;

namespace EventService.Tests.Services;

public class EventServiceCacheTests
{
    private readonly Mock<IEventRepository> _repositoryMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly RedisSettings _redisSettings = new()
    {
        EventTtlSeconds = 300,
        TopEventsTtlSeconds = 60,
        EventKeyPrefix = "event:",
        TopEventsKey = "events:top10"
    };

    private EventService.Application.Services.EventService CreateSut()
    {
        return new EventService.Application.Services.EventService(
            _repositoryMock.Object,
            _cacheServiceMock.Object,
            Options.Create(_redisSettings),
            NullLogger<EventService.Application.Services.EventService>.Instance);
    }

    private static DomainEvent CreateDomainEvent(Guid id, int totalSeats = 100, int availableSeats = 50)
    {
        return DomainEvent.Create(id, "Test Event Title", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), totalSeats, Guid.NewGuid(), availableSeats: availableSeats);
    }

    [Fact]
    public async Task GetEventByIdAsync_CacheHit_DoesNotCallRepository()
    {
        var id = Guid.NewGuid();
        var cachedDto = new OutputEventDTO(CreateDomainEvent(id));

        _cacheServiceMock
            .Setup(c => c.GetAsync<OutputEventDTO>($"event:{id}"))
            .ReturnsAsync(cachedDto);

        var sut = CreateSut();

        var result = await sut.GetEventByIdAsync(id);

        Assert.Equal(cachedDto.Id, result.Id);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<OutputEventDTO>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task GetEventByIdAsync_CacheMiss_FetchesFromRepositoryAndCachesResult()
    {
        var id = Guid.NewGuid();
        var domainEvent = CreateDomainEvent(id);

        _cacheServiceMock
            .Setup(c => c.GetAsync<OutputEventDTO>($"event:{id}"))
            .ReturnsAsync((OutputEventDTO?)null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(domainEvent);

        var sut = CreateSut();

        var result = await sut.GetEventByIdAsync(id);

        Assert.Equal(id, result.Id);
        _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _cacheServiceMock.Verify(
            c => c.SetAsync($"event:{id}", It.Is<OutputEventDTO>(dto => dto.Id == id), TimeSpan.FromSeconds(_redisSettings.EventTtlSeconds)),
            Times.Once);
    }

    [Fact]
    public async Task GetTopEventsAsync_CacheHit_DoesNotCallRepository()
    {
        var cached = new List<OutputEventDTO> { new(CreateDomainEvent(Guid.NewGuid())) };

        _cacheServiceMock
            .Setup(c => c.GetAsync<IList<OutputEventDTO>>("events:top10"))
            .ReturnsAsync(cached);

        var sut = CreateSut();

        var result = await sut.GetTopEventsAsync();

        Assert.Single(result);
        _repositoryMock.Verify(r => r.GetTopEventsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetTopEventsAsync_CacheMiss_FetchesFromRepositoryAndCachesResult()
    {
        var events = new List<DomainEvent> { CreateDomainEvent(Guid.NewGuid()) };

        _cacheServiceMock
            .Setup(c => c.GetAsync<IList<OutputEventDTO>>("events:top10"))
            .ReturnsAsync((IList<OutputEventDTO>?)null);

        _repositoryMock
            .Setup(r => r.GetTopEventsAsync(10))
            .ReturnsAsync(events);

        var sut = CreateSut();

        var result = await sut.GetTopEventsAsync();

        Assert.Single(result);
        _repositoryMock.Verify(r => r.GetTopEventsAsync(10), Times.Once);
        _cacheServiceMock.Verify(
            c => c.SetAsync("events:top10", It.IsAny<IList<OutputEventDTO>>(), TimeSpan.FromSeconds(_redisSettings.TopEventsTtlSeconds)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateEventAsync_InvalidatesEventCache_AfterSavingToRepository()
    {
        var id = Guid.NewGuid();
        var existingEvent = CreateDomainEvent(id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingEvent);

        var callOrder = new List<string>();
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<DomainEvent>()))
            .Callback(() => callOrder.Add("db"))
            .Returns(Task.CompletedTask);
        _cacheServiceMock
            .Setup(c => c.RemoveAsync($"event:{id}"))
            .Callback(() => callOrder.Add("cache"))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var inputDto = new InputEventDTO
        {
            Title = "Updated Title",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeat = 100
        };

        await sut.UpdateEventAsync(id, inputDto);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DomainEvent>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"event:{id}"), Times.Once);
        Assert.Equal(new[] { "db", "cache" }, callOrder);
    }

    [Fact]
    public async Task DeleteEventAsync_InvalidatesEventCache_AfterDeletingFromRepository()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);

        var callOrder = new List<string>();
        _repositoryMock
            .Setup(r => r.DeleteAsync(id))
            .Callback(() => callOrder.Add("db"))
            .Returns(Task.CompletedTask);
        _cacheServiceMock
            .Setup(c => c.RemoveAsync($"event:{id}"))
            .Callback(() => callOrder.Add("cache"))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.DeleteEventAsync(id);

        _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"event:{id}"), Times.Once);
        Assert.Equal(new[] { "db", "cache" }, callOrder);
    }
}
