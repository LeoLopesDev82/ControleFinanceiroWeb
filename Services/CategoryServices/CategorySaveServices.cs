using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Responses;
using System.Text;

namespace ControleFinanceiroWeb.Services.CategoryServices
{
    public class CategorySaveServices
    {
        private readonly AppDbContext _context;

        public CategorySaveServices(AppDbContext context)
        {
            _context = context;
        }

        public SaveResult Save(int? id, string? description, string? selectedRadio, string? identifiers)
        {
            var validation = ValidateData(id, description, selectedRadio, identifiers);

            if (!validation.Success) return validation;

            return id == null || id == 0 ? Insert(description!, selectedRadio!, identifiers) : Update(id.Value, description!, selectedRadio!, identifiers);
        }

        #region "private"

        private SaveResult ValidateData(int? id, string? description, string? selectedRadio, string? identifiers)
        {
            StringBuilder sbError = new StringBuilder();

            bool exists = _context.Categories.Any(c =>
                c.Description == description && (id == null || c.Id != id));

            if (string.IsNullOrWhiteSpace(description))
                sbError.AppendLine("• A descrição não pode ser nula ou vazia.");
            else if (description.Length > 255)
                sbError.AppendLine("• A descrição não pode conter mais do que 255 caracteres.");

            if (string.IsNullOrWhiteSpace(selectedRadio))
                sbError.AppendLine("• O tipo de lançamento não pode ser nulo.");

            if (!string.IsNullOrWhiteSpace(identifiers) && identifiers.Length > 5000)
                sbError.AppendLine("• O campo identificadores não pode ter mais de 5000 caracteres");

            if (exists)
                sbError.AppendLine("• Já existe outro registro com a mesma descrição.");

            return new SaveResult
            {
                Success = sbError.Length == 0,
                Message = $"Não é possível salvar:\n\n{sbError.ToString().Trim()}"
            };
        }

        private SaveResult Insert(string description, string selectedRadio, string? identifiers)
        {
            try
            {
                var newCategory = new Category
                {
                    Description = description,
                    EntryType = ParseEntryType(selectedRadio),
                    StatementIdentifiers = identifiers ?? string.Empty
                };

                _context.Categories.Add(newCategory);
                _context.SaveChanges();

                return new SaveResult
                {
                    Success = true,
                    Message = "Categoria criada com sucesso!",
                    Id = newCategory.Id
                };
            }
            catch
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível criar a categoria.",
                    Id = null
                };
            }
        }

        private SaveResult Update(int id, string description, string selectedRadio, string? identifiers)
        {
            try
            {
                var existing = _context.Categories.FirstOrDefault(c => c.Id == id);

                if (existing == null)
                {
                    return new SaveResult
                    {
                        Success = false,
                        Message = "Categoria não encontrada para atualização.",
                        Id = null
                    };
                }

                existing.Description = description;
                existing.EntryType = ParseEntryType(selectedRadio);
                existing.StatementIdentifiers = identifiers ?? string.Empty;

                _context.SaveChanges();

                return new SaveResult
                {
                    Success = true,
                    Message = "Categoria atualizada com sucesso!",
                    Id = existing.Id
                };
            }
            catch
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível atualizar a categoria.",
                    Id = null
                };
            }
        }

        private char ParseEntryType(string? entryType)
        {
            if (string.IsNullOrEmpty(entryType)) return 'V';

            if (entryType.Equals("rdbFixed", StringComparison.OrdinalIgnoreCase) ||
                entryType.Equals("F", StringComparison.OrdinalIgnoreCase))
                return 'F';

            return 'V';
        }

        #endregion
    }
}
