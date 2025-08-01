using ControleFinanceiroWeb.Data;

namespace ControleFinanceiroWeb.Services.StatementServices
{
    public class CategoryReidentificationServices
    {
        private readonly AppDbContext _context;
        private readonly EntryIdServices _entryIdService;

        public CategoryReidentificationServices(AppDbContext context)
        {
            _context = context;
            _entryIdService = new EntryIdServices(context);
        }

        public bool Reidentify(DateTime startDate, DateTime endDate)
        {
            try
            {
                var statementsToUpdate = _context.Statement
                    .Where(s => s.EntryId == null &&
                                s.DueDate >= startDate &&
                                s.DueDate <= endDate)
                    .ToList();

                foreach (var statement in statementsToUpdate)
                {
                    var entryId = _entryIdService.GetEntryId(statement.Description ?? string.Empty);
                    statement.EntryId = entryId;
                }

                _context.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
