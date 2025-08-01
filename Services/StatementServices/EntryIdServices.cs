using ControleFinanceiroWeb.Data;

namespace ControleFinanceiroWeb.Services.StatementServices
{
    public class EntryIdServices
    {
        private readonly AppDbContext _context;
               
        public EntryIdServices(AppDbContext context)
        {
            _context = context;
        }

        public int? GetEntryId(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return null;

            var normalizedDescription = description.ToLowerInvariant();
            var categories = _context.Categories.ToList();
            var identifierList = new List<(string keyword, int categoryId)>();

            foreach (var category in categories)
            {
                if (string.IsNullOrWhiteSpace(category.StatementIdentifiers)) continue;

                var keywords = category.StatementIdentifiers.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim().ToLowerInvariant());

                foreach (var keyword in keywords)
                {
                    if (!identifierList.Any(i => i.keyword == keyword))
                    {
                        identifierList.Add((keyword, category.Id));
                    }
                }
            }

            var match = identifierList.FirstOrDefault(i => normalizedDescription.Contains(i.keyword));

            return match == default ? null : match.categoryId;
        }
    }
}
