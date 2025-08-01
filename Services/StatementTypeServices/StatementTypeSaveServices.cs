using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Responses;
using System.Net;
using System.Text;

public class StatementTypeSaveServices
{
    private readonly AppDbContext _context;

    public StatementTypeSaveServices(AppDbContext context)
    {
        _context = context;
    }

    public SaveResult SaveStatementType(string description, int id)
    {
        var validate = ValidateData(description, id);

        if (!validate.Success)
        {
            validate.StatusCode = HttpStatusCode.BadRequest;

            return validate;
        }

        return id == 0 ? Insert(description) : Update(description, id);
    }

    private SaveResult ValidateData(string description, int id)
    {
        StringBuilder sbError = new StringBuilder();

        bool exists = _context.StatementTypes.Any(st => st.Description == description && st.Id != id);

        if (string.IsNullOrEmpty(description))
            sbError.AppendLine("• A descrição não pode ser nula!");
        else if (description.Length > 255)
            sbError.AppendLine("• A descrição não pode conter mais do que 255 caracteres");

        if (exists)
            sbError.AppendLine("• Já existe outro registro com a mesma descrição.");

        return new SaveResult
        {
            Success = sbError.Length == 0,
            Message = sbError.ToString(),
            StatusCode = sbError.Length == 0 ? HttpStatusCode.OK : HttpStatusCode.BadRequest
        };
    }

    private SaveResult Insert(string description)
    {
        try
        {
            var newStatement = new StatementTypes { Description = description };

            _context.StatementTypes.Add(newStatement);
            _context.SaveChanges();

            return new SaveResult
            {
                Success = true,
                Message = "Nova opção criada com sucesso!",
                Id = newStatement.Id,
                StatusCode = HttpStatusCode.OK
            };
        }
        catch
        {
            return new SaveResult
            {
                Success = false,
                Message = "Ocorreu um erro inesperado e não foi possível criar a nova opção.",
                Id = null,
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }

    private SaveResult Update(string description, int id)
    {
        try
        {
            var existing = _context.StatementTypes.FirstOrDefault(st => st.Id == id);

            if (existing == null)
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Registro não encontrado para atualização.",
                    Id = null,
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            existing.Description = description;

            _context.SaveChanges();

            return new SaveResult
            {
                Success = true,
                Message = "Opção atualizada com sucesso!",
                Id = existing.Id,
                StatusCode = HttpStatusCode.OK
            };
        }
        catch
        {
            return new SaveResult
            {
                Success = false,
                Message = "Ocorreu um erro inesperado e não foi possível atualizar a opção.",
                Id = null,
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }
}
