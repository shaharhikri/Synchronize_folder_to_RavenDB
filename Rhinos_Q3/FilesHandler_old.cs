public class FilesHandler_old
{
    private string _files_folder_path;  //Path of the dir to sync
    private static readonly string FILE_TYPE = "json";
    private string[] _file_names;
    private List<string> _file_ids;
    private string _last_run_date_path;             //Path of the 'last_run_date.txt' (cache) file - that contains the date of the last run.
    private string _last_run_filenames_path;        //Path of the 'last_run_files.txt' (cache) file - that contains the file list from the last run.
    private List<string> _modified_files_names; // Names(paths) of files that were modified/created after last run of the program.

    public FilesHandler_old(string files_folder_path, string last_run_date_path, string last_run_filenames_path)
    {
        if (files_folder_path == null || last_run_date_path == null || last_run_filenames_path == null)
        {
            throw new ArgumentNullException();
        }

        this._files_folder_path = files_folder_path;
        this._last_run_date_path = last_run_date_path;
        this._last_run_filenames_path = last_run_filenames_path;
        this._file_names = Directory.GetFiles(_files_folder_path, "*." + FILE_TYPE);

        InitModifiedFilesList();
        //InitDeletedFilesList();
    }

    public string[] getFileNames()
    {
        return _file_names;
    }

    public List<string> GetModifiedFilesList()
    {
        return _modified_files_names;
    }

    /* Init _modified_files_names (modified/new files names(paths)) */
    private void InitModifiedFilesList()
    {
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
        foreach (var fn in _file_names)
        {
            DateTime f_modied_date = System.IO.File.GetLastWriteTime(fn);
            if (last_run_date < f_modied_date)
            {
                _modified_files_names.Add(fn);
            }
        }
    }



    /*write current datetime to last_time_date.txt,
     * (if last_time_date.txt file doesnt exist create it)*/
    public void UpdateLastDateTimeFile()
    {
        File.WriteAllText(_last_run_date_path, DateTime.Now.ToString()); //update last_run_date.txt file
    }




    //private List<string> _deleted_files_names;  // Names(paths) of files that were deleted after last run of the program.

    /*
    public List<string> GetDeletedFilesList()
    {
        return this._deleted_files_names;
    }
    */

    /* Init _deleted_files_names*/
    /*
    private void InitDeletedFilesList()
    {
        List<string> last_run_files; //if last_run_files.txt is exist and include file names - last_run_files is those file names, else last_run_files is empty list
        try
        {
            last_run_files = File.ReadLines(_last_run_filenames_path).ToList();
        }
        catch (System.IO.FileNotFoundException e)
        {
            last_run_files = new List<string>();
        }

        string[] file_names = Directory.GetFiles(_files_folder_path, "*." + FILE_TYPE);
        _deleted_files_names = new List<string>();
        foreach (var fn in last_run_files)
        {
            if (!file_names.Contains(fn))
            {
                _deleted_files_names.Add(fn);
            }
        }
    }
    */



    /*write current files to last_run_files.txt,
     * (if last_run_files.txt file doesnt exist create it)*/
    /*
    public void UpdateLastDateFileNames()
    {
        string[] file_names = Directory.GetFiles(_files_folder_path, "*." + FILE_TYPE);
        File.WriteAllLines(_last_run_filenames_path, file_names);
    }
    */
}
