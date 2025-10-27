using BankingEventSourcing.Application.DTOs;
using MediatR;

namespace BankingEventSourcing.Application.Queries;

public record GetAccountQuery(Guid AccountId) : IRequest<AccountDto>;
