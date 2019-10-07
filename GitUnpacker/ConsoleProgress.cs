using System;

namespace GitUnpacker
{
    /// <inheritdoc />
    /// <summary>
    /// Inherits from <see cref="IProgress{T}" /> to make progress reporting in console app.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ConsoleProgress<T> : IProgress<T>
    {
        private readonly Action<T> _action;

        internal ConsoleProgress(Action<T> action) =>
            _action = action ?? throw new ArgumentNullException(nameof(action));

        public void Report(T value) => _action(value);
    }
}
