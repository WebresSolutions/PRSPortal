using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Webmodels;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.ScheduleComponents;

public partial class ViewScheduleTrack
{
    [Parameter]
    public required ScheduleTrackDtoWithCalendar Track { get; set; }

    [Parameter]
    public required UserDto[] Users { get; set; }

    [Parameter]
    public required int JobTypeId { get; set; }

    [Parameter]
    public required EventCallback OnUpdate { get; set; }

    private bool IsEditing = false;

    private List<UserDto> _selectedUsers = [];

    protected override async Task OnParametersSetAsync()
    {
        _selectedUsers = Track.AssignedUsers;
    }

    /// <summary>
    /// Adds a new schedule entry for the specified track
    /// </summary>
    /// <param name="trackId">The schedule track identifier</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task AddNewSchedule()
    {
        DialogParameters parameter = new DialogParameters<int> { { "TrackId", Track.TrackId } };
        DialogOptions options = new() { CloseButton = false, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };

        IDialogReference res = await _dialog.ShowAsync<AddSchedule>("", parameter, options);
        int? returnVal = await res.GetReturnValueAsync<int?>();
        if (returnVal is not null)
        {
            await OnUpdate.InvokeAsync();
        }
    }

    private void OnSelectedUsersChanged(IEnumerable<UserDto> values)
    {
        _selectedUsers = values?.ToList() ?? [];
    }

    private async Task SaveAssignedUsers()
    {
        Track.AssignedUsers = _selectedUsers;
        UpdateScheduleTrackDto updateDto = new()
        {
            Date = Track.Day,
            AssignedUsers = [.. _selectedUsers.Select(x => x.userId ?? 0).Distinct()],
            JobTypeEnum = (JobTypeEnum)JobTypeId,
            ScheduleTrackId = Track.TrackId
        };
        Result<ScheduleTrackDto> res = await _apiService.UpdateScheduleTrack(updateDto);
        if (res.IsSuccess)
            IsEditing = false;
        else
            _snackbar.Add("Failed to SaveScheduleTrack", Severity.Error);
    }

    private async Task OpenDialogAsync(CustomCalendarItem cal)
    {
        if (cal is null)
        {
            Console.WriteLine("Calendar item is null!");
            return;
        }

        DialogParameters parameter = new DialogParameters<int> { { "ScheduleId", cal.ScheduleItemId } };
        DialogOptions options = new() { CloseButton = false, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };

        IDialogReference res = await _dialog.ShowAsync<ViewEditSchedule>("", parameter, options);
        int? returnVal = await res.GetReturnValueAsync<int?>();
        if (returnVal is not null)
        {
            await OnUpdate.InvokeAsync();
        }
    }

    private async Task Delete(int id)
    {
        Result<int> res = await _apiService.DeleteScheduleTrack(id);
        if (!res.IsSuccess)
            _snackbar.Add("Failed to delete the schedule track", Severity.Error);

        await OnUpdate.InvokeAsync();
    }
}