using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Responses;

namespace ControleFinanceiroWeb.Services.StatementServices
{
	public class StatementDeleteServices
	{
		private readonly AppDbContext _context;

		public StatementDeleteServices(AppDbContext context)
		{
			_context = context;
		}

        public SaveResult Delete(int id)
        {
            try
            {
                var statement = _context.Statement.Find(id);

                if (statement == null)
                {
                    return new SaveResult
                    {
                        Success = false,
                        Message = "Registro não encontrado.",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }

                _context.Statement.Remove(statement);
                _context.SaveChanges();

                return new SaveResult
                {
                    Success = true,
                    Message = "Registro excluído com sucesso.",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception)
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível concluir a exclusão.",
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

    }
}
