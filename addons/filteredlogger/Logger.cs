using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Godot;
using FileAccess = System.IO.FileAccess;

namespace FilteredLogger.Addons.FilteredLogger;

public class Logger
{
    
    //Categories for filtering
    public List<string> Categories { get; private set; } = [];
    
    private static readonly LogWriter Writer = new LogWriter();
    private bool WriteFile { set;get; }
    private bool WriteGd { set; get; }
    private bool WriteConsole {  set;get; }

    public bool IsNew = true;

    private static readonly Logger Instance = new Logger();

    #region public API

    

   
    public static void Log(string message, string category = "")
    {
        //Default to System
        if (category == "")
        {
            category = (string)ProjectSettings.GetSetting("FilteredLogger/Settings/DefaultCategory");
        }
        
        Instance.Write(message, category,GetDateTime());
    }

    //returns the full text log
    public static string GetText()
    {
        return Writer.ReadText();
    }
    
    /// <summary>
    /// Returns the full binary log
    /// </summary>
    /// <returns>returns a byte array. the array will be formated as 3 sets of strings: a category, a timestamp, and a message in that order</returns>
    public static byte[] GetBinary()
    {
        return Logger.Writer.ReadBinary();
    }
    public static List<string> GetCategories()
    {
        return Instance.Categories;
    }

    /// <summary>
    /// Preformats all events in the log
    /// </summary>
    /// <returns></returns>
    public static List<LoggedEvent> GetEvents()
    {
        List<LoggedEvent> events = new List<LoggedEvent>();
        byte[] bytes = GetBinary();
        MemoryStream stream = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(stream);
        while (stream.Position < stream.Length)
        {
            string category = reader.ReadString();
            string timestamp = reader.ReadString();
            string message = reader.ReadString();
            events.Add(new LoggedEvent(category, timestamp, message));
        }
        
        return events;
    }
    /// <summary>
    /// /// Preformats all events in the log
    /// </summary>
    /// <param name="categories">will only return events under specified categories</param>
    /// <returns></returns>
    public static List<LoggedEvent> GetEvents(string[] categories)
    {
        List<LoggedEvent> events = new List<LoggedEvent>();
        byte[] bytes = GetBinary();
        MemoryStream stream = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(stream);
        while (stream.Position < stream.Length)
        {
            string category = reader.ReadString();
            string timestamp = reader.ReadString();
            string message = reader.ReadString();
            if(categories.Contains(category))
                events.Add(new LoggedEvent(category, timestamp, message));
        }
        
        return events;
    }
    
    #endregion

    #region Utility

    private static string FormatMessage(string message, string category, string timestamp = "")
    {
        if (timestamp == "")
        {
            timestamp = GetDateTime();
        }
        return $"[{category}][{timestamp}] {message}";
    }
   
    public static StringName GetPath()
    {
        string path = (string)ProjectSettings.GetSetting("FilteredLogger/Settings/LogDirectory");
        string abspath = ProjectSettings.GlobalizePath(path);
        return abspath;
    }

    private static StringName GetFile()
    {
        string file = (string)ProjectSettings.GetSetting("FilteredLogger/Settings/LogName");
        return $"{GetPath()}{file}";
    }
    public static StringName GetTextFile()
    {
        
        return $"{GetFile()}.log";
    }
    public static StringName GetBinaryFile()
    {
        
        return $"{GetFile()}.blog";
    }

    private static string GetDateTimeFormat()
    {
        return "h:mm:ss:ffff";
    }

    private static string GetDateTime()
    {
        return DateTime.Now.ToString(GetDateTimeFormat());
    }
    
    #endregion
    private Logger()
    {
       
        Writer.Open();
        
        WriteConsole = ProjectSettings.HasSetting("FilteredLogger/Settings/WriteToC#Console");
        WriteGd = ProjectSettings.HasSetting("FilteredLogger/Settings/WriteToGodotConsole");
        WriteFile = ProjectSettings.HasSetting("FilteredLogger/Settings/WriteToFile");
        
    }

    /// <summary>
    /// This is where the actual Writing logic is
    /// </summary>
    private void Write(string message, string category, string timestamp)
    {

        string formattedMessage = $"[{category}][{DateTime.Now}] {message}";
        if (WriteConsole)
        {
            Console.WriteLine(formattedMessage);
        }

        if (WriteGd)
        {
            GD.Print(formattedMessage);

        }

        if (WriteFile)
        {
            //write puts the message, category and timestamp in as seperate strings, we do not want to send
            //it the formatted message
            Writer.Write(message, category, timestamp);
            
        }
    }
    public static void ClearLogs()
    {
        Writer.Close();
        File.Delete(GetTextFile());
        File.Delete(GetBinaryFile());
        Writer.Open();
    }

    #region OnLogEvent

    //Called when the logger does its thing

   
    public static event EventHandler<LogEventargs> OnLog;
    
    //Logger invocation
    private static void InvokeOnLog(LogEventargs e)
    {
        OnLog?.Invoke(Instance, e);
    }
    #endregion
    //Used to handle writing to the file
    private class LogWriter
    {
        
        private FileStream _binfileStream = null;
        private BinaryWriter _binaryWriter = null;
        private FileStream _textfileStream = null;
        private StreamWriter _textWriter = null;
        
        private bool _isOpen = false;
        
        public void  Open()
        {
            
            string path = GetPath();
            if (!Directory.Exists(path))
            {
                if (path != null) Directory.CreateDirectory(path);
            }
            _binfileStream = File.Open(GetBinaryFile(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _binaryWriter = new BinaryWriter(_binfileStream, Encoding.UTF8, true);
            
            //_textfileStream = File.Open(GetTextFile(), FileMode.OpenOrCreate);
            string file = GetTextFile();
            _textfileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.ReadWrite);
            _textWriter = new StreamWriter(_textfileStream, Encoding.UTF8, 1024, true);
          
            
            
            _isOpen = true;

        }
        public void Write(string message, string category, string timestamp)
        {
            if (_isOpen == false)
            {
                return;
            }
            _binaryWriter.Write(category);
            _binaryWriter.Write(timestamp);
            _binaryWriter.Write(message);
            _binaryWriter.Flush();


            string formattedMessage = Logger.FormatMessage(message, category);

            _textWriter.WriteLine(formattedMessage);
            _textWriter.Flush();
            
            bool isNew = Instance.Categories.Contains(category);
            LogEventargs arg = new LogEventargs(isNew , category, timestamp, formattedMessage);
            InvokeOnLog(arg);
        }

        public  string ReadText()
        {
            long pos = _textfileStream.Position;
            _textfileStream.Position = 0;
            TextReader r = new StreamReader(_textfileStream, Encoding.UTF8, true);
            string result= r.ReadToEnd();
            _textfileStream.Position = pos;
            return result;
        }

        
        public byte[] ReadBinary()
        {
            long pos = _binfileStream.Position;
            _binfileStream.Position = 0;
            BinaryReader r = new BinaryReader(_binfileStream, Encoding.UTF8, true);
            var bytes = r.ReadBytes((int)_binfileStream.Length);
            _textfileStream.Position = pos;
            return bytes;
        }
        public void Close()
        {
            if (!_isOpen)
            {
                return;
            }
            _isOpen = false;
            _binaryWriter?.Dispose();
            _binfileStream?.Dispose();
            _textfileStream?.Dispose();
            _binaryWriter = null;
            // _textWriter = null;
            _binfileStream = null;
        }
    }
}

public struct LoggedEvent(string category,  string message, string timestamp)
{
    public string Timestamp = timestamp;
    public string Category = category;
    public string Message = message;
}


public class LogEventargs(bool isNew, string category, string message, string timestamp)
    : EventArgs
{
    public string Message { get; } = message;
    public string Category { get; } = category;
    public string Timestamp { get; } = timestamp;
    public bool IsNew { get; } = isNew;
}