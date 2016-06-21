namespace UrlRewrite.Interfaces.Utilities
{
    public interface IPropertyBag
    {
        string this[string name] { get; set; }
        IPropertyBag CreateChild();

        T Get<T>(string name = null);
        void Set<T>(T value, string name = null);
    }
}