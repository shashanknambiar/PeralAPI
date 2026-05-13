namespace PeralAPI.Models.DTOs
{
    public record StockCardDto(
        string ProductId,
        string ProductName,
        string? Identifier,
        int MinQuantity,
        string? ImageUrl,
        double TotalQuantity,
        int FillPercentage,
        DateTime? LastCheckInOn,
        string? PlacedOrderId = null
    );

    public record ConfirmStockAdjustmentDto(
        DateTime CheckInDate,
        List<StockAdjustmentItemDto> Adjustments
    );

    public record StockAdjustmentItemDto(
        string ProductId,
        int NewFillPercentage,
        int BoxesOpened
    );

    public record ConfirmStockAdjustmentResultDto(
        int ProductsAdjusted,
        double TotalBoxesConsumed,
        DateTime CheckInDate
    );
}
