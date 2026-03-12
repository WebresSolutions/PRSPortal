using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Components.UIComponents;
using Portal.Shared;
using Portal.Shared.DTO.File;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.Files;

public partial class ViewFiles
{
    /// <summary>
    /// Required. The job id that these files belong to (for adding new files).
    /// </summary>
    [Parameter]
    public required int JobId { get; set; }

    /// <summary>
    /// Required. The files being viewed.
    /// </summary>
    [Parameter]
    public required IEnumerable<FileDto> Files { get; set; }

    /// <summary>
    /// Callback for when a file is created or edited.
    /// </summary>
    [Parameter]
    public EventCallback OnFilesEdited { get; set; }

    private bool _disabled = false;

    /// <summary>
    /// The card reference
    /// </summary>
    private Card? _cardRef;

    public async Task CreateNewFile()
    {
        DialogParameters parameter = new DialogParameters<NewFileDialog> { { "JobId", JobId } };
        DialogOptions options = new() { CloseButton = false, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };
        IDialogReference dialogRef = await _dialog.ShowAsync<NewFileDialog>("", parameter, options);
        DialogResult? result = await dialogRef.Result;

        if (result is { Canceled: false })
        {
            _cardRef?.SetSelectedTab(TabTypeEnum.All);
            await OnFilesEdited.InvokeAsync();
        }
    }

    public async Task EditFile(FileDto file)
    {
        DialogParameters parameter = new DialogParameters<EditFileDialog> { { "File", file } };
        DialogOptions options = new() { CloseButton = false, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };
        IDialogReference dialogRef = await _dialog.ShowAsync<EditFileDialog>("", parameter, options);
        DialogResult? result = await dialogRef.Result;

        if (result is { Canceled: false })
        {
            _cardRef?.SetSelectedTab(TabTypeEnum.All);
            await OnFilesEdited.InvokeAsync();
        }
    }

    public async Task DownloadFile(int fileId)
    {
        _disabled = true;
        Result<FileDto> file = await _apiService.GetFileData(fileId);
        if (!file.IsSuccess)
        {
            _snackbar.Add("File download failed", Severity.Error);
            return;
        }

        // download the file to the browser
        await Helpers.DownloadFileToBrowser(_jsRuntime, file.Value!.Content, file.Value.FileName);
        _disabled = false;
    }
}