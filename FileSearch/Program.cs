using FileSearch;

var logger = new Logger();
var capture = await ICapture.LoadAsync(logger);
var skip = await Skip.LoadAsync(logger);
var display = new Display();
var searchedFiles = 0;
var searchedFolders = 0;

var currentFolders = new List<string> { Environment.CurrentDirectory };
while (currentFolders.Count > 0)
{
    var nextFolders = new List<string>();

    foreach (var currentFolder in currentFolders)
    {
        display.Show(currentFolder);

        string[] allFiles;
        try
        {
            allFiles = Directory.GetFiles(currentFolder);
        }
        catch
        {
            allFiles = [];
        }

        var validFiles = allFiles
            .Where(file => !skip.Skipped(file))
            .OrderByDescending(t => t)
            .ToArray();

        foreach (var file in validFiles)
        {
            display.Show(file);

            var fileText = await File.ReadAllTextAsync(file);
            if (capture.Capture(fileText))
            {
                display.Output(file);
                await logger.LogAsync(file);
            }
        }

        searchedFiles += validFiles.Length;

        string[] allFolders;
        try
        {
            allFolders = Directory.GetDirectories(currentFolder);
        }
        catch
        {
            allFolders = [];
        }

        var validFolders = allFolders
            .Where(folder => !skip.Skipped(folder))
            .OrderByDescending(t => t);

        nextFolders.AddRange(validFolders);
    }

    searchedFolders += currentFolders.Count;

    currentFolders = nextFolders;
}

display.Clear();

var end = $"Searched {searchedFiles} files and {searchedFolders} folders.";
Console.WriteLine(end);
await logger.LogAsync(end);