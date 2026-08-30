using System.Collections.Generic;

namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class SidebarViewModel
    {
        public bool IsSummaryActive { get; set; }

        public bool IsExtratoActive { get; set; }

        public List<SidebarStatementViewModel> Statements { get; set; } = new();
    }

    public class SidebarStatementViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
