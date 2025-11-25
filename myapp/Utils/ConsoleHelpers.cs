namespace myapp.Utils;

public class ConsoleHelpers : IConsoleHelpers
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        Console.WriteLine(format, args);
    }

    public void WriteLine()
    {
        Console.WriteLine();
    }
  
    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public string? ReadLine(string prompt)
    {
        Write(prompt);
        return Console.ReadLine();
    }

    public void Write(string message)
    {
        Console.Write(message);
    }

    public void Write(string format, params object[] args)
    {
        Console.Write(format, args);
    }
}

public interface IConsoleHelpers
{
    void WriteLine(string message);
    void WriteLine(string format, params object[] args);
    void WriteLine();
    void Write(string message);
    void Write(string format, params object[] args);
    string? ReadLine();
    string? ReadLine(string prompt);
}