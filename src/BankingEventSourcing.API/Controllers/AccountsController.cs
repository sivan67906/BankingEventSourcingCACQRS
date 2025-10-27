using BankingEventSourcing.Application.Commands;
using BankingEventSourcing.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankingEventSourcing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> OpenAccount([FromBody] OpenAccountCommand command)
    {
        var accountId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), new { id = accountId }, new { accountId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var account = await _mediator.Send(new GetAccountQuery(id));
        return Ok(account);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        var accounts = await _mediator.Send(new GetAllAccountsQuery());
        return Ok(accounts);
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] DepositRequest request)
    {
        await _mediator.Send(new DepositMoneyCommand(id, request.Amount, request.Description));
        return Ok();
    }

    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] WithdrawRequest request)
    {
        await _mediator.Send(new WithdrawMoneyCommand(id, request.Amount, request.Description));
        return Ok();
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseAccount(Guid id, [FromBody] CloseRequest request)
    {
        await _mediator.Send(new CloseAccountCommand(id, request.Reason));
        return Ok();
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var history = await _mediator.Send(new GetAccountHistoryQuery(id));
        return Ok(history);
    }

    [HttpGet("{id}/at-date")]
    public async Task<IActionResult> GetAccountAtDate(Guid id, [FromQuery] DateTime date)
    {
        var account = await _mediator.Send(new GetAccountAtDateQuery(id, date));
        return Ok(account);
    }

    public record DepositRequest(decimal Amount, string Description);
    public record WithdrawRequest(decimal Amount, string Description);
    public record CloseRequest(string Reason);
}
