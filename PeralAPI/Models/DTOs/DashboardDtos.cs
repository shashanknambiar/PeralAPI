namespace PeralAPI.Models.DTOs
{
    public record DashboardDto(
        int PendingOrdersCount,
        int LowStockItemsCount,
        int TotalInventoryItems,
        int PendingPaymentsCount,
        double TotalPayableCredit,
        int VendorsWithAdvanceCount,
        double TotalAdvanceCredit
    );
}
