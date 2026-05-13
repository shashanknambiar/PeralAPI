namespace PeralAPI.Models.DTOs
{
    public record BillDto(
        string Id,
        string PatientName,
        DateTime BillDate,
        string PatientPhoneNumber,
        int Age,
        string Gender,
        string DoctorName,
        List<BillProductItemDto> Products,
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
