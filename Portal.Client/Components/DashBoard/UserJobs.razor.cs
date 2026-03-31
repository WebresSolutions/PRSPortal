using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.DashBoard;

public partial class UserJobs
{

    private UserJobsListDto _jobList = new();

    protected override async Task OnInitializedAsync()
    {
        Result<UserJobsListDto> res = await _apiService.GetUserJobs(0);
        if (res.IsSuccess)
            _jobList = res.Value!;
    }

}