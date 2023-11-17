using System.IO;

namespace Trader.Services.Repos;
public abstract class Repo {
    protected readonly string folder;
    private static void createIfNotExist(string folder) {
        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }
    }
    protected string[] getFilesInFolder(string folder) => Directory.GetFiles(folder, "*.json");
    public Repo(string folder) {
        this.folder = folder;

        createIfNotExist(this.folder);
    }
}
