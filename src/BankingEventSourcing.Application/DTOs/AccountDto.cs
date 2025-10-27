namespace BankingEventSourcing.Application.DTOs;

public class AccountDto
{
    public Guid AccountId { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int Version { get; set; }
}
