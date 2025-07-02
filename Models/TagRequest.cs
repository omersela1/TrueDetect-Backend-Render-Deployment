public class TagRequest
{
    public string Line { get; set; }
    public string Tag { get; set; }

    public TagRequest(string line, string tag)
    {
        Line = line;
        Tag = tag;
    }

    public TagRequest() { } // Parameterless constructor for deserialization
}