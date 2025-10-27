using BankingEventSourcing.Application.DTOs;
using MediatR;

namespace BankingEventSourcing.Application.Queries;

public record GetAllAccountsQuery() : IRequest<List<AccountDto>>;
