namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class CategoryListViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string EntryType { get; set; } = string.Empty;
        public string StatementIdentifiers { get; set; } = string.Empty;
    }
}