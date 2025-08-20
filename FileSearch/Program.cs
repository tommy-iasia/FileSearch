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

        var files = Directory
            .GetFiles(currentFolder)
            .Where(file => !skip.Skipped(file))
            .OrderByDescending(t => t)
            .ToArray();

        foreach (var file in files)
        {
            display.Show(file);

            var fileText = await File.ReadAllTextAsync(file);
            if (capture.Capture(fileText))
            {
                display.Output(file);
                await logger.LogAsync(file);
            }
        }

        searchedFiles += files.Length;

        var folders = Directory
            .GetDirectories(currentFolder)
            .Where(folder => !skip.Skipped(folder))
            .OrderByDescending(t => t);

        nextFolders.AddRange(folders);
    }

    searchedFolders += currentFolders.Count;

    currentFolders = nextFolders;
}

display.Clear();

var end = $"Searched {searchedFiles} files and {searchedFolders} folders.";
Console.WriteLine(end);
await logger.LogAsync(end);