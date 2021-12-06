using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FilesHandler
{
    private string _files_folder_path;
    private string _last_run_date_path;
    private string _last_run_ids_path;
    private static readonly string FILE_TYPE = "json";

    private List<string> _modified_files_names;
    private List<string> _deleted_ids;

    private List<string> _file_ids;

    public FilesHandler(string files_folder_path, string last_run_date_path, string last_run_ids_path)
    {
        if (files_folder_path == null || last_run_date_path == null || last_run_ids_path == null)
        {
            throw new ArgumentNullException();
        }

        this._files_folder_path = files_folder_path;
        this._last_run_date_path = last_run_date_path;
        this._last_run_ids_path = last_run_ids_path;

        Init();

        Console.WriteLine("deleted ids:");
        foreach(string id in _deleted_ids)
        {
            Console.WriteLine(id);
        }

        Console.WriteLine("modified files:");
        foreach (string m in _modified_files_names)
        {
            Console.WriteLine(m);
        }
    }

    public List<string> GetModifiedFilesNames()
    {
        return _modified_files_names;
    }

    public List<string> GetDeletedIDs()
    {
        return _deleted_ids;
    }

    private void Init()
    {
        List<string> file_names;
        List<string> last_file_ids;

        try
        {
            file_names = Directory.GetFiles(_files_folder_path, "*." + FILE_TYPE).ToList();
        }
        catch (System.IO.FileNotFoundException)
        {
            file_names = new List<string>();
        }

        try
        {
            last_file_ids = File.ReadLines(_last_run_ids_path).ToList();
        }
        catch (System.IO.FileNotFoundException)
        {
            last_file_ids = new List<string>();
        }

        List<string> file_names_clone = new List<string>(file_names);
        _file_ids = new List<string>();
        foreach (string path in file_names_clone)
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);

            try
            {
                JObject jo = JObject.Parse(json); // JsonReaderException
                JToken id_token = jo["Id"];
                if (id_token != null)
                {
                    string id = jo["Id"].ToString();
                    _file_ids.Add(id);
                }
                else
                {
                    file_names.Remove(path);
                }

            }
            catch (JsonReaderException)
            {
                continue;
            }
        }
        

        //Init _deleted_ids
        _deleted_ids = new List<string>();
        foreach (string id in last_file_ids)
        {
            if (!_file_ids.Contains(id))
            {
                _deleted_ids.Add(id);
            }
        }

        //Init _modified_files_names
        DateTime last_run_date; //if last_time_date.txt is exist and include Datetime - last_run_date equals this Datetime, else last_run_date equals the oldest Datetime
        try
        {
            last_run_date = DateTime.Parse(File.ReadAllText(_last_run_date_path));
        }
        catch (System.IO.FileNotFoundException)
        {
            last_run_date = DateTime.MinValue; //oldest date
        }
        catch (FormatException)
        {
            last_run_date = DateTime.MinValue; //oldest date
        }
        _modified_files_names = new List<string>();
        foreach (var fn in file_names)
        {
            DateTime f_modied_date = System.IO.File.GetLastWriteTime(fn);
            if (last_run_date < f_modied_date)
            {
                _modified_files_names.Add(fn);
            }
        }
    }

    public void UpdateCacheFiles()
    {
        File.WriteAllText(_last_run_date_path, DateTime.Now.ToString()); //update last_run_date.txt file
        File.WriteAllLines(_last_run_ids_path, _file_ids);
    }
}
