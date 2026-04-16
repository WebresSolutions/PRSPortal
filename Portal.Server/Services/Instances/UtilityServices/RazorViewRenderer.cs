using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Portal.Server.Services.Interfaces.UtilityServices;

namespace Portal.Server.Services.Instances.UtilityServices;

public class RazorViewRenderer(
    IRazorViewEngine _viewEngine,
    ITempDataProvider _tempDataProvider,
    IServiceScopeFactory _serviceScopeFactory
    ) : IRazorViewRenderer
{
    ///<inheritdoc  />
    public async Task<string> Render<TModel>(string viewName, TModel model)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        DefaultHttpContext httpContext = new() { RequestServices = scope.ServiceProvider };
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());

        IView view = FindView(actionContext, viewName);

        ViewDataDictionary<TModel> viewData = new(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        TempDataDictionary tempData = new(httpContext, _tempDataProvider);

        using StringWriter output = new();

        ViewContext viewContext = new(
            actionContext,
            view,
            viewData,
            tempData,
            output,
            new HtmlHelperOptions()
        );

        await view.RenderAsync(viewContext);

        return output.ToString();
    }

    /// <summary>
    /// Finds the view based on the view name
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    /// <param name="viewName">The view name</param>
    /// <returns>The view interface.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private IView FindView(ActionContext actionContext, string viewName)
    {
        ViewEngineResult getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        ViewEngineResult findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        IEnumerable<string> searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        string errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations)
        );

        throw new InvalidOperationException(errorMessage);
    }
}