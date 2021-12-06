//demo.ravendb.net
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions.Database;
using Raven.Client.Exceptions.Documents;
using Raven.Client.Http;
using Sparrow.Json;

public class Program
{
    public static readonly string LAST_RUN_DATE_PATH = "../../../last_run_date.txt";
    public static readonly string LAST_RUN_IDS_PATH = "../../../last_run_ids.txt";
    public enum CommandType { PUT, DELETE };

    public static void Main(string[] args)
    {
        //Main args
        string serverURL = "http://live-test.ravendb.net";
        string databaseName = "ShaharHikriDB";
        string folder_path = "C:/Users/Shahar/Desktop/";

        FilesHandler f_handler = new FilesHandler(folder_path, LAST_RUN_DATE_PATH, LAST_RUN_IDS_PATH);

        Console.WriteLine("");
        if (updatesFilesInDB(serverURL, databaseName, f_handler.GetModifiedFilesNames(), f_handler.GetDeletedIDs()))
        {
            f_handler.UpdateCacheFiles();
            Console.WriteLine("\'" + databaseName + "\' DB Update succeded , OR There's no update to do in DB...");
        }
        else
        {
            Console.WriteLine("Cannot connect to DB \'" + databaseName + "\'...");
        }
    }

    public static IDocumentStore createDocumentStore(string serverURL, string databaseName)
    {
        IDocumentStore documentStore = new DocumentStore
        {
            Urls = new[] { serverURL },
            Database = databaseName
        };

        documentStore.Initialize();
        return documentStore;
    }

    public static bool updatesFilesInDB(string serverURL, string databaseName, List<string> modified_files_names, List<string> deleted_ids)
    {
        try
        {
            using (IDocumentStore documentStore = createDocumentStore(serverURL, databaseName))
            {
                using (IDocumentSession session = documentStore.OpenSession(databaseName))
                {
                    updateDocsInDB(session, modified_files_names);
                    deleteDocsFromDB(session, deleted_ids);
                    session.SaveChanges();
                }
            }
            return true;
        }
        catch (AggregateException e)
        {
            return false;
        }
        catch (DatabaseDoesNotExistException e)
        {
            return false;
        }
    }

    public static void deleteDocsFromDB(IDocumentSession session, List<string> deleted_ids)
    {
        foreach (string id in deleted_ids)
        {
            var command = new DeleteDocumentCommand(id, null);
            session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
            Console.WriteLine("Document - id:\'" + id + "\' deleted.\n");
        }
    }

    public static void updateDocsInDB(IDocumentSession session, List<string> jsons_files_names)
    {
        string json;
        JObject jo;
        JToken id_token;
        string id;
        foreach (string modified_file_path in jsons_files_names)
        {
            json = File.ReadAllText(modified_file_path, System.Text.Encoding.UTF8);

            try
            {
                jo = JObject.Parse(json); // JsonReaderException
                id_token = jo["Id"];
                if (id_token != null)
                {
                    id = jo["Id"].ToString();
                    //Console.WriteLine(id);
                    var blittableJson = ParseJson(session.Advanced.Context, json);
                    var command = new PutDocumentCommand(id, null, blittableJson);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context); //DocumentCollectionMismatchException
                    Console.WriteLine("Json \'" + modified_file_path + "\' created/updated.\n");
                }
                else
                {
                    Console.WriteLine("Json \'" + modified_file_path + "\' has no \'Id\',\n");
                }

            }
            catch (JsonReaderException)
            {
                Console.WriteLine("Json " + modified_file_path+" format is corrupted ,and cannot sync (UPDATE/CREATE) with DB.");
            }
            catch (DocumentCollectionMismatchException)
            {
                Console.WriteLine("Json has no \'@metadata\' attribute  ,and cannot sync (UPDATE/CREATE) with DB.");
            }
        }
    }

    public static BlittableJsonReaderObject ParseJson(JsonOperationContext context, string json)
    {
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
        {
            ValueTask<BlittableJsonReaderObject> b = context.ReadForMemoryAsync(stream, "json");
            return b.Result;
        }
    }
}



