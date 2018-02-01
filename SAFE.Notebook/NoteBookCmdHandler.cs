using SAFE.CQRS;

namespace SAFE.TestCQRSApp
{
    public class NoteBookCmdHandler : CmdHandler
    {
        public NoteBookCmdHandler(Repository repo)
            :base (repo)
        { }

        IContext Handle(AddNote cmd)
        {
            var ctx = new Context<AddNote, NoteBook>(cmd, _repo);

            ctx.SetAction((c, ar) =>
            {
                if (ar.Version == -1)
                    ar.Init(cmd.TargetId).GetAwaiter().GetResult();
                return ar.AddNote(c.Note);
            });

            return ctx;
        }
    }

    // A cmd type can only ever have one recipient type. (ExampleCmd is only handled by ExampleAggregate).
    public class AddNote : Cmd
    {
        public AddNote(long targetId, int expectedVersion, string note)
            : base(targetId, expectedVersion)
        {
            Note = note;
        }

        public string Note { get; private set; }
    }
}