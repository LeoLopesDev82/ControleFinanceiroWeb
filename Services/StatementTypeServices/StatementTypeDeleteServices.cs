using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Responses;

namespace ControleFinanceiroWeb.Services.StatementTypeServices
{
    public class StatementTypeDeleteServices
    {
        private readonly AppDbContext _context;

        public StatementTypeDeleteServices(AppDbContext context)
        {
            _context = context;
        }

        public SaveResult DeleteStatementType(int id)
        {
            try
            {
                var validateData = ValidateData(id);

                if (!validateData.Success) return validateData;

                var statementType = _context.StatementTypes.FirstOrDefault(st => st.Id == id);

                if (statementType == null)
                {
                    return new SaveResult
                    {
                        Success = false,
                        Message = "O extrato informado não foi encontrado."
                    };
                }

                _context.StatementTypes.Remove(statementType);
                _context.SaveChanges();

                return new SaveResult
                {
                    Success = true,
                    Message = "Extrato excluído com sucesso."
                };
            }
            catch
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível concluir a exclusão."
                };
            }
        }

        private SaveResult ValidateData(int id)
        {
            bool hasLinkedStatements = _context.Statement.Any(s => s.StatementTypeId == id);

            if (hasLinkedStatements)
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Não é possível excluir este extrato, pois existem registros vinculados a ele. Apague os lançamentos ou transfira-os para outro extrato antes de tentar novamente."
                };
            }

            return new SaveResult { Success = true };
        }
    }
}
