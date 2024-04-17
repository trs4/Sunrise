namespace Sunrise.Model;

public interface IProgress
{
    void Show(string title);

    void Next(double progress, string text);

    void Hide();
}
