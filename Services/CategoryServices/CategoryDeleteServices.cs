using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Responses;

namespace ControleFinanceiroWeb.Services.CategoryServices
{
    public class CategoryDeleteServices
    {
        private readonly AppDbContext _context;

        public CategoryDeleteServices(AppDbContext context)
        {
            _context = context;
        }

        public SaveResult Delete(int id)
        {
            var validadeData = ValidadeData(id);

            if (!validadeData.Success) return validadeData;

            return ExecuteDelete(id);
        }


        #region "subroutines"

        public SaveResult ValidadeData(int id)
        {
            bool isInUse = _context.Statement.Any(s => s.EntryId == id);

            if (isInUse)
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Não foi possível excluir esta categoria, pois ela está associada a um ou mais lançamentos nos extratos. Para prosseguir com a exclusão, remova primeiro os lançamentos vinculados a esta categoria."
                };
            }

            return new SaveResult { Success = true };
        }

        private SaveResult ExecuteDelete(int id)
        {
            try
            {
                var category = _context.Categories.FirstOrDefault(c => c.Id == id);

                if (category == null) return new SaveResult { Success = false, Message = "Categoria não encontrada." };

                _context.Categories.Remove(category);
                _context.SaveChanges();

                return new SaveResult { Success = true, Message = "Categoria excluída com sucesso." };
            }
            catch (Exception)
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível concluir a exclusão."
                };
            }
        }

        #endregion
    }
}
