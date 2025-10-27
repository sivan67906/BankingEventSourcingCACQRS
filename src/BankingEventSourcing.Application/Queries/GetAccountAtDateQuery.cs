using BankingEventSourcing.Application.DTOs;
using MediatR;

namespace BankingEventSourcing.Application.Queries;

public record GetAccountAtDateQuery(Guid AccountId, DateTime AsOfDate) : IRequest<AccountDto>;
