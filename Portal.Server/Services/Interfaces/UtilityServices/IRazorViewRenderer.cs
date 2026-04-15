namespace Portal.Server.Services.Interfaces.UtilityServices;


public interface IRazorViewRenderer
{
    /// <summary>
    /// Renders the view as a string based on the viewname and the supplied model.
    /// </summary>
    /// <typeparam name="TModel">The Model Type</typeparam>
    /// <param name="viewName">The Name of the view</param>
    /// <param name="model">The Model Object used for the razor file</param>
    /// <returns>The rendered HTML string</returns>
    Task<string> Render<TModel>(string viewName, TModel model);
}
