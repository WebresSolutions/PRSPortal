// Set the Blazor environment based on the hostname
(function () {
    let env = "Development";
    if (window.location.hostname.includes("staging")) {
        env = "Staging";
    } else if (!window.location.hostname.includes("localhost")) {
        env = "Production";
    }
    window.BlazorEnvironment = env;
})();
