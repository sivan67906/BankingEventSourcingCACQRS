namespace BankingEventSourcing.Domain.Aggregates;

public abstract class AggregateRoot<TId>
{
    private readonly List<object> _uncommittedEvents = new();

    public TId Id { get; protected set; } = default!;
    public int Version { get; protected set; }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();
    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected void ApplyChange(object @event)
    {
        Apply(@event);
        _uncommittedEvents.Add(@event);
    }

    public void LoadFromHistory(IEnumerable<object> events)
    {
        foreach (var @event in events)
        {
            Apply(@event);
            Version++;
        }
    }

    protected abstract void Apply(object @event);
}
