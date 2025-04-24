namespace Portfolio.Services.TreasuryDirect.Models;

public class TreasuryDirectSecurityIssuance
{
    public string? Cusip { get; set; }

    public DateTime IssueDate { get; set; }

    public double? OfferingAmount { get; set; }

    public TreasuryDirectSecurityType? SecurityType { get; set; }

    public string? SecurityTerm { get; set; }

    public DateTime? AnnouncementDate { get; set; }

    public DateTime? AuctionDate { get; set; }

    public decimal? HighDiscountRate { get; set; }

    public decimal? HighInvestmentRate { get; set; }

    public decimal? HighPrice { get; set; }

    public DateTime? MaturityDate { get; set; }
}

public enum TreasuryDirectSecurityType
{
    Undefined = 0,
    Bill = 1,
    Note = 2,
    Bond = 3
}
