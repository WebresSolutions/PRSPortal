using Portal.Shared;

namespace Portal.Client.Webmodels;

public class TabSelect
{
    public TabSelect(TabTypeEnum tabType)
    {
        TabId = Guid.NewGuid();
        TabType = tabType;
    }

    public TabTypeEnum TabType { get; set; }
    public Guid TabId { get; set; }
}
