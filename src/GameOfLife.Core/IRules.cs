namespace GameOfLife.Core;

public interface IRules<TState>
{
    TState DefaultState { get; }
    TState GetNextState(TState current, IEnumerable<TState> neighborStates);
}
