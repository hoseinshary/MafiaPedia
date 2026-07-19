namespace MafiaPedia.Api.IServices.Phase2;

public interface IFinanceAuditService
{
    Task<List<object>> GetDeletedRecordsAsync(int clubId, string type);
}
