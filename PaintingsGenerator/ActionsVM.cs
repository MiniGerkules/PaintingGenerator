using System;
using System.Collections.Immutable;

namespace PaintingsGenerator {
    public record class ProcessAction(string Description,
                                      Action ToDo);

    public class ActionsVM : NotifierOfPropertyChange {
        public ImmutableList<ProcessAction> Actions { get; }

        public ActionsVM(ImmutableList<ProcessAction> actions) {
            Actions = actions;
        }
    }
}
