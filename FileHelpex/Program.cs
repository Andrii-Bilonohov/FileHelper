using System.Text.RegularExpressions;

class Program
{
    static private string filePathStatistics = "C:\\System programming\\Homework\\FileHelpex\\FileHelpex\\Statistics";


    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        MainAsync().GetAwaiter().GetResult();
    }


    static async Task MainAsync()
    {
        Console.Title = "📂 File Assistant";
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n═══════════════════════════════════════════════════");
        Console.WriteLine("               📂 Welcome to File Assistant!");
        Console.WriteLine("═══════════════════════════════════════════════════\n");
        Console.ResetColor();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("════════════ 📜 Menu 📜 ════════════");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" [1] 🔍 Search for a word");
            Console.WriteLine(" [2] ✏️  Replace a word and copy files");
            Console.WriteLine(" [3] 📄 Find classes and interfaces in .cs files");
            Console.WriteLine(" [0] 🚪 Exit");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\nPress Backspace at any time to ⛔ cancel the operation.");
            Console.ResetColor();

            string choice = await GetStringValidAsync("\n🎛️  Your choice: ");

            var cts = new CancellationTokenSource();

            Task cancelTask = Task.Run(() =>
            {
                while (true)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Backspace)
                    {
                        cts.Cancel();
                        break;
                    }
                }
            });

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    await SearchWordAsync(cts.Token);
                    break;
                case "2":
                    Console.Clear();
                    await CopyAndReplaceWordAsync(cts.Token);
                    break;
                case "3":
                    Console.Clear();
                    await FindClassesAndInterfacesAsync(cts.Token);
                    Console.ResetColor();
                    break;
                case "0":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n🚪 Exiting application...");
                    Console.ResetColor();
                    Environment.Exit(0);
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n❌ Invalid option. Please try again.");
                    Console.ResetColor();
                    ReturnToMenu();
                    break;
            }
        }
    }


    static async Task SearchWordAsync(CancellationToken token)
    {
        string folderPath = await GetFolderValidAsync("Enter the folder path to search in: ");
        string word = await GetStringValidAsync("Enter the word to search for: ");

        string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
        int countFiles = files.Length;
        int countFilesWithWord = 0;
        int processedFiles = 0;
        int countWords = 0;

        var results = new List<string>();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n📁 Found {countFiles} files. Starting search for word: '{word}'...\n");
        Console.ResetColor();

        var progressTask = ShowProgressAsync(() => (processedFiles, countFiles), "SearchWord", token);

        try
        {
            foreach (string file in files)
            {
                string content;
                try
                {
                    content = await File.ReadAllTextAsync(file, token);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error reading file: {file}\n{ex.Message}");
                    Console.ResetColor();
                    processedFiles++;
                    continue;
                }

                var regex = GetRegexForPattern("word", word);
                int count = regex.Matches(content).Count;
                countWords += count;
                if (count > 0) countFilesWithWord++;

                results.Add($"File: {file}");
                results.Add($"Word count: {count}");
                results.Add(new string('-', 40));

                processedFiles++;
            }

            await progressTask;

            string statsFolder = filePathStatistics;
            Directory.CreateDirectory(statsFolder);
            string statsFile = Path.Combine(statsFolder, $"SearchStats_{DateTime.Now:yyyy_MM_dd_HH_mm}.txt");

            await File.WriteAllLinesAsync(statsFile, results);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\n========== 📊 Search Results ==========");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Total words found: {countWords}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"📄 Files containing '{word}': {countFilesWithWord}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"📊 Statistics saved to: {statsFile}");
            Console.ResetColor();
        }
        catch (OperationCanceledException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⛔ Operation canceled.");
            Console.ResetColor();
        }
        finally
        {
            ReturnToMenu();
        }
    }


    static async Task CopyAndReplaceWordAsync(CancellationToken token)
    {
        string sourceFolder = await GetFolderValidAsync("📁 Enter the source folder path: ");
        string wordToReplace = await GetStringValidAsync("✏️ Enter the word to replace: ");
        string replacement = await GetStringValidAsync("📝 Enter the replacement word: ");
        string destFolder = Path.Combine(sourceFolder, "CopyResults");
        Directory.CreateDirectory(destFolder);

        string[] files = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
        int totalFiles = files.Length;
        int processedFiles = 0;
        int replacedFiles = 0;
        int totalReplacements = 0;

        var results = new List<string>();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n📁 Found {totalFiles} files. Starting replacement of '{wordToReplace}' with '{replacement}'...\n");
        Console.ResetColor();

        var progressTask = ShowProgressAsync(() => (processedFiles, totalFiles), "Copy&Replace", token);

        try
        {
            foreach (var file in files)
            {
                string content;
                try
                {
                    content = await File.ReadAllTextAsync(file, token);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error reading file: {file}\n{ex.Message}");
                    Console.ResetColor();
                    processedFiles++;
                    continue;
                }

                var regex = GetRegexForPattern("word", wordToReplace);
                int count = regex.Matches(content).Count;

                if (count > 0)
                {
                    string updatedContent = regex.Replace(content, replacement);
                    string destPath = Path.Combine(destFolder, Path.GetFileName(file));
                    await File.WriteAllTextAsync(destPath, updatedContent, token);

                    results.Add($"File: {file}");
                    results.Add($"🔄 Replacements: {count}");
                    results.Add(new string('-', 50));

                    replacedFiles++;
                    totalReplacements += count;
                }

                processedFiles++;
            }

            await progressTask;

            string statsFile = Path.Combine(filePathStatistics, $"ReplaceStats_{DateTime.Now:yyyy_MM_dd_HH_mm}.txt");
            await File.WriteAllLinesAsync(statsFile, results);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\n========== 📊 Replacement Results ==========");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Files with replacements: {replacedFiles}");
            Console.WriteLine($"🔄 Total replacements: {totalReplacements}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"📊 Statistics saved to: {statsFile}");
            Console.ResetColor();
        }
        catch (OperationCanceledException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⛔ Operation canceled.");
            Console.ResetColor();
        }
        finally
        {
            ReturnToMenu();
        }
    }


    static async Task FindClassesAndInterfacesAsync(CancellationToken token)
    {
        string folderPath = await GetFolderValidAsync("Enter the folder path to search in: ");
        string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

        int totalFiles = files.Length;
        int processedFiles = 0;
        int totalMatches = 0;

        var results = new List<string>();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n📁 Found {totalFiles} .cs files. Starting class/interface search...\n");
        Console.ResetColor();

        var progressTask = ShowProgressAsync(() => (processedFiles, totalFiles), "FindClassesAndInterfaces", token);

        try
        {
            foreach (string file in files)
            {
                string content;
                try
                {
                    content = await File.ReadAllTextAsync(file, token);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error reading file: {file}\n{ex.Message}");
                    Console.ResetColor();
                    processedFiles++;
                    continue;
                }

                string cleanedContent = CleanCodeFromCommentsAndStrings(content);

                Regex regex = new Regex(@"\b(class|interface)\s+([A-Z]\w*)\b(?=\s*[:{])");

                var matches = regex.Matches(cleanedContent);

                if (matches.Count > 0)
                {
                    results.Add($"📄 File: {file}");
                    foreach (Match match in matches)
                    {
                        string type = match.Groups[1].Value;
                        string name = match.Groups[2].Value;
                        results.Add($"   ➝ {type}: {name}");
                        totalMatches++;
                    }
                    results.Add(new string('-', 50));
                }

                processedFiles++;
            }

            await progressTask;

            string statsFolder = filePathStatistics;
            Directory.CreateDirectory(statsFolder);
            string statsFile = Path.Combine(statsFolder, $"ClassesInterfacesStats_{DateTime.Now:yyyy_MM_dd_HH_mm}.txt");

            await File.WriteAllLinesAsync(statsFile, results);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\n========== 📊 Class & Interface Search Results ==========");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Total classes/interfaces found: {totalMatches}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"📄 Processed files: {totalFiles}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"📊 Statistics saved to: {statsFile}");
            Console.ResetColor();
        }
        catch (OperationCanceledException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⛔ Operation canceled.");
            Console.ResetColor();
        }
        finally
        {
            ReturnToMenu();
        }
    }


    static string CleanCodeFromCommentsAndStrings(string content)
    {
        string noStrings = Regex.Replace(content, @"""([^""\\]|\\.)*""", "");

        string noMultiLineComments = Regex.Replace(noStrings, @"/\*.*?\*/", "", RegexOptions.Singleline);

        string cleanContent = Regex.Replace(noMultiLineComments, @"//.*", "");

        return cleanContent;
    }


    static async Task ShowProgressAsync(Func<(int processed, int total)> getProgress, string operationName, CancellationToken token)
    {
        var random = new Random();
        double progress = 0;

        while (!token.IsCancellationRequested)
        {
            progress += random.Next(1, 51);

            if (progress > 100)
                progress = 100;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\r⏳ [{operationName}] Progress: {progress:F2}%   ");
            Console.ResetColor();

            if (progress >= 100)
                break;

            await Task.Delay(500, token);
        }
    }


    static async Task<string> GetStringValidAsync(string message)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(message);
            Console.ResetColor();

            string? result = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Invalid input. Please try again.");
            Console.ResetColor();
            await Task.Delay(500);
        }
    }


    static Regex GetRegexForPattern(string patternName, string word = "")
    {
        return patternName.ToLower() switch
        {
            "word" when !string.IsNullOrWhiteSpace(word)
                => new Regex($@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase),

            "classInterface" => new Regex(@"\b(class|interface)\s+([A-Z]\w*)\b(?=\s*[:{])", RegexOptions.IgnoreCase),

            _ => throw new ArgumentException("Unknown pattern name or missing word.", nameof(patternName))
        };
    }


    static async Task<string> GetFolderValidAsync(string message)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(message);
            Console.ResetColor();

            string? result = Console.ReadLine();
            if (Directory.Exists(result))
            {
                return result;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Invalid folder path. Please try again.");
            Console.ResetColor();
            await Task.Delay(500);
        }
    }


    static void ReturnToMenu()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\nPress any key to return to the main menu...");
        Console.ResetColor();
        Console.ReadKey();
        Console.Clear();
    }
}
