namespace ControleFinanceiroWeb.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public char EntryType { get; set; }
        public string StatementIdentifiers { get; set; } 
    }
}
