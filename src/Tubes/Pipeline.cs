using System;
using System.Collections.Generic;

namespace Tubes
{
    public interface IPipeline<TMessage>
    {
        Pipeline<TMessage> Register(Action<TMessage> filter);
        void Execute(TMessage message);
    }

    public sealed class Pipeline<TMessage> : IPipeline<TMessage>
    {
        private readonly List<Action<TMessage>> _filters;

        public Pipeline()
        {
            _filters = new List<Action<TMessage>>();
        }

        internal Pipeline(List<Action<TMessage>> filters)
        {
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        }

        public Pipeline<TMessage> Register(Action<TMessage> filter)
        {
            _filters.Add(filter ?? throw new ArgumentNullException(nameof(filter)));
            return this;
        }

        public void Execute(TMessage message)
        {
            if(message == null)
                throw new ArgumentNullException(nameof(message));

            _filters.ForEach(f =>
            {
                if (message is IStopProcessing stopProcessing && stopProcessing.Stop)
                    return;

                f(message);
            });
        }
    }
}