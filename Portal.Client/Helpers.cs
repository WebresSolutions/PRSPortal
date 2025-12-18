using Portal.Shared;

namespace Portal.Client;

public static class Helpers
{
    public static string GetJobColor(JobTypeEnum type)
        => type == JobTypeEnum.Construction ? "var(--colour-construction)" : "var(--colour-survey)";

}
