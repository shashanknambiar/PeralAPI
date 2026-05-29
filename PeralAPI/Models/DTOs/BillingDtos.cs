namespace PeralAPI.Models.DTOs
{
    // ── Service catalog ───────────────────────────────────────────────────────

    public record ServiceDto(
        string Id,
        string Name,
        decimal Price
    );

    public record CreateServiceDto(
        string Name,
        decimal Price
    );

    public record UpdateServiceDto(
        string Id,
        string Name,
        decimal Price
    );

    // ── Bill service line items ───────────────────────────────────────────────

    public record BillServiceItemDto(
        string ServiceId,
        string ServiceName,
        decimal Price
    );

    public record BillServiceInputDto(
        string ServiceId,
        decimal Price
    );

    // ── Bills ─────────────────────────────────────────────────────────────────

    public record BillDto(
        string Id,
        string PatientName,
        DateTime BillDate,
        string PatientPhoneNumber,
        int Age,
        string Gender,
        string DoctorName,
        List<BillProductItemDto> Products,
        List<BillServiceItemDto> Services,
        double DiscountInPercent,
        double BillTotal
    );

    public record BillProductItemDto(
        string ProductId,
        string ProductName,
        int Quantity,
        decimal PricePerItem
    );

    public record CreateBillDto(
        string PatientName,
        DateTime BillDate,
        string PatientPhoneNumber,
        int Age,
        string Gender,
        string DoctorName,
        List<BillProductInputDto> Products,
        List<BillServiceInputDto>? Services,
        double DiscountInPercent,
        double BillTotal
    );

    public record UpdateBillDto(
        string Id,
        string PatientName,
        DateTime BillDate,
        string PatientPhoneNumber,
        int Age,
        string Gender,
        string DoctorName,
        List<BillProductInputDto> Products,
        List<BillServiceInputDto>? Services,
        double DiscountInPercent,
        double BillTotal
    );

    public record BillProductInputDto(
        string ProductId,
        int Quantity,
        decimal PricePerItem
    );

    public record BillSearchParamsDto(
        string? PatientName = null,
        string? PatientPhoneNumber = null,
        string? DoctorName = null,
        DateTime? FromDate = null,
        DateTime? ToDate = null
    );

    public record BillSearchResultDto(
        int CurrentPage,
        int TotalPages,
        List<BillDto> Bills
    );
}
