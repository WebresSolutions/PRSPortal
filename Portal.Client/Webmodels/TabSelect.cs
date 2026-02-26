namespace Portal.Client.Webmodels;

public class TabSelect
{
    public TabSelect(string name)
    {
        TabId = Guid.NewGuid();
        Name = name;
    }

    public string Name { get; set; } = "";
    public Guid TabId { get; set; }
}
