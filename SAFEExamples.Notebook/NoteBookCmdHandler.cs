using SAFE.CQRS;
using SAFE.SystemUtils;
using System;
using System.Threading.Tasks;
using Context = SAFE.CQRS.Context<SAFE.TestCQRSApp.AddNote, SAFE.TestCQRSApp.NoteBook>;

namespace SAFE.TestCQRSApp
{
    public class NoteBookCmdHandler
    {
        Repository _repo;

        public NoteBookCmdHandler(Repository repo)
        {
            _repo = repo;
        }

        public async Task<Result<bool>> Handle(AddNote cmd)
        {
            try
            {
                var ctx = new Context(cmd, _repo);

                var changed = await ctx.ExecuteAsync((c, ar) =>
                {
                    if (ar.Version == -1)
                        ar.Init(cmd.TargetId).GetAwaiter().GetResult();
                    return ar.AddNote(c.Note);
                });

                if (!changed)
                    return Result.OK(false);

                var savedChanges = await ctx.CommitAsync();

                if (!savedChanges)
                    throw new InvalidOperationException("Could not save changes");

                return Result.OK(true);
            }
            catch (InvalidOperationException ex)
            {
                // logging
                return Result.Fail<bool>(ex.Message);
            }
            catch (Exception ex)
            {
                // logging
                return Result.Fail<bool>(ex.Message);
            }
        }
    }

    // A cmd type can only ever have one recipient type. (ExampleCmd is only handled by ExampleAggregate).
    public class AddNote : Cmd
    {
        public AddNote(Guid targetId, int expectedVersion, string note)
            : base(targetId, expectedVersion)
        {
            Note = note;
        }

        public string Note { get; private set; }
    }
}