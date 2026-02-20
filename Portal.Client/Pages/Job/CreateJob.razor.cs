using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Job;

public partial class CreateJob : IDisposable
{
    private int _jobNumber = 1;
    private JobTypeEnum _jobType = JobTypeEnum.Construction;
    private ListContactDto? JobContact;
    private JobCreationDto Model = null!;

    #region Map Objects
    /// <summary>
    /// the list of active markers on the map
    /// </summary>
    private Dictionary<int, AdvancedMarkerElement> markers = [];
    /// <summary>
    /// Marker clustering instance
    /// </summary>
    private MarkerClustering? markerClustering;
    private GoogleMap? Map;
    #endregion

    private MapOptions MapOptions = default!;
    private CouncilPartialDto[] Councils = [];
    private bool Submitting;

    /// <summary>
    /// On initialized method for loading data on the first page load.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await base.OnInitializedAsync();
        Model = new()
        {
            JobType = JobTypeEnum.Construction,
            JobNumber = 1,
            Address = new()
            {
                LatLng = new(-37.8136, 144.9631)
            }
        };
        MapOptions = new MapOptions
        {
            Zoom = 10,
            Center = new LatLngLiteral(-37.8136, 144.9631),
            MapTypeId = MapTypeId.Roadmap
        };

        Result<CouncilPartialDto[]>? councilResult = await _apiService.GetCouncils();
        if (councilResult?.IsSuccess == true && councilResult.Value is not null)
            Councils = councilResult.Value;

        IsLoading = false;

        await LoadMap(0);
    }

    private async Task LoadMap(int retryCount)
    {
        if (Map is null || Map.InteropObject is null || Model.Address?.LatLng is null)
        {
            if (retryCount > 4)
            {
                retryCount = retryCount++;
                await Task.Delay(300);
                await LoadMap(retryCount);
            }
            return;
        }

        Console.WriteLine("Success Adding the markers");
        LatLngLiteral latLng = new(Model.Address.LatLng.Latitude, Model.Address.LatLng.Longitude);
        AdvancedMarkerElement marker = await AdvancedMarkerElement.CreateAsync(Map.JsRuntime, new AdvancedMarkerElementOptions()
        {
            Position = latLng,
            Title = "New Coord",
            Content = "dsal;fjsdalkj dsalk j daslkjdsaf dsaf dsaf dsaf dafdsa fdsafdas ",
            GmpClickable = false,
            GmpDraggable = true
        });

        foreach (KeyValuePair<int, AdvancedMarkerElement> kvp in markers)
        {
            await kvp.Value.AddListener<GoogleMapsComponents.Maps.MouseEvent>("dragend", async (e) => await HandleDragEnd(e, kvp));
        }

        if (markerClustering is null)
            markerClustering = await MarkerClustering.CreateAsync(
                Map.JsRuntime,
                Map.InteropObject,
                markers.Values
            );
        else
            await markerClustering.AddMarkers(markers.Values);
    }

    /// <summary>
    /// Helper to unpack the event data and call the main logic.
    /// </summary>
    private async Task HandleDragEnd(GoogleMapsComponents.Maps.MouseEvent _, KeyValuePair<int, AdvancedMarkerElement> marker)
    {
        LatLngAltitudeLiteral newPosition = await marker.Value.GetPosition();
        Model.Address?.LatLng?.Latitude = newPosition.Lat;
        Model.Address?.LatLng?.Longitude = newPosition.Lng;
    }

    private async Task SubmitAsync()
    {
        if (_jobNumber <= 0)
        {
            _snackbar?.Add("Job number must be greater than 0.", Severity.Warning);
            return;
        }

        Submitting = true;
        try
        {
            Result<int> result = await _apiService.CreateJob(Model);
            if (result.IsSuccess && result.Value > 0)
            {
                _snackbar?.Add("Job created successfully.", Severity.Success);
                _navigationManager.NavigateTo($"/jobs/view/{result.Value}");
            }
            else
                _snackbar?.Add(result.ErrorDescription ?? "Failed to create job.", Severity.Error);
        }
        finally
        {
            Submitting = false;
        }
    }

    /// <summary>
    /// Used by the type ahead auto complete for searching contacts
    /// </summary>
    /// <param name="search">The search string</param>
    /// <param name="token">The token</param>
    /// <returns></returns>
    public async Task<IEnumerable<ListContactDto>> SearchContacts(string search, CancellationToken token)
    {
        Result<PagedResponse<ListContactDto>>? contactResult = await _apiService.GetAllContacts(500, 1, search, null, SortDirectionEnum.Asc);

        if (contactResult?.IsSuccess == true && contactResult.Value?.Result is not null)
            return contactResult.Value.Result;
        else
            return [];
    }

    public void Dispose() => Map?.Dispose();
}
