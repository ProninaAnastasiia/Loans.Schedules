namespace Loans.Schedules.Data.Dto;

public class CalculateContractValues
{
    public Guid ContractId { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public string PaymentType { get; set; }
    public Guid OperationId { get; set; }
}