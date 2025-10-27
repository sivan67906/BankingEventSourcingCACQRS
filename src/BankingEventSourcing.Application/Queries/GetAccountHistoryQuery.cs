using BankingEventSourcing.Application.DTOs;
using MediatR;

namespace BankingEventSourcing.Application.Queries;

public record GetAccountHistoryQuery(Guid AccountId) : IRequest<List<EventDto>>;
