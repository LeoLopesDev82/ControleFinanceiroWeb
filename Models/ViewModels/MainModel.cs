using ControleFinanceiroWeb.Models.Domain;
using ControleFinanceiroWeb.Models.DTOs;

namespace ControleFinanceiroDesktop.Models.ViewModels
{
    public class MainModel
    {
        public List<ButtonInfoModel> ButtonsInfo { get; set; }
        public List<StatementGridDto> statementDtos { get; set; }
        public SummaryDto SummaryDto { get; set; }
        public List<CategorySummaryDto> CreditSummaries { get; set; }
        public List<CategorySummaryDto> DebitSummaries { get; set; }
        public List<ListItemDto> Categories { get; set; }
        public string Title { get; set; }
    }
}
