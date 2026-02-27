namespace mym.Services;

public interface IUserApprovalSettings
{
    bool AutoApproveNewUsers { get; set; }
}

public class UserApprovalSettings : IUserApprovalSettings
{
    public bool AutoApproveNewUsers { get; set; }
}
