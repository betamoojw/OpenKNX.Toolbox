using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenKNX.Toolkit.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    #region Properties
    public ReleaseContentModel ReleaseContent { get; set;}
    public ObservableCollection<Repository> Repos { get; set; } = new ();
    public ObservableCollection<ReleaseContentModel> LocalReleases { get; set; } = new ();

    public bool CanSelectRepo
    {
        get { return Repos.Count > 0 && !_isDownloading && !_isUpdating; }
    }

    private Repository? _selectedRepository;
    public Repository? SelectedRepository
    {
        get { return _selectedRepository; }
        set { 
            _selectedRepository = value;
            NotifyPropertyChanged("SelectedRepository");
            NotifyPropertyChanged("CanSelectRelease");
        }
    }

    private Release? _selectedRelease;
    public Release? SelectedRelease
    {
        get { return _selectedRelease; }
        set { 
            _selectedRelease = value;
            NotifyPropertyChanged("SelectedRelease");
            NotifyPropertyChanged("CanDownloadRelease");
        }
    }

    public bool CanSelectRelease
    {
        get { return _selectedRepository != null && !_isDownloading && !_isUpdating; }
    }

    public bool CanDownloadRelease
    {
        get { return _selectedRelease != null && !_isDownloading && !_isUpdating; }
    }

    public bool CanUpdateRepos
    {
        get { return !_isUpdating && !_isDownloading; }
    }

    private bool _isDownloading = false;
    public bool IsDownloading
    {
        get { return _isDownloading; }
        set {
            _isDownloading = value;
            NotifyPropertyChanged("CanSelectRepo");
            NotifyPropertyChanged("CanSelectRelease");
            NotifyPropertyChanged("CanUpdateRepos");
            NotifyPropertyChanged("CanDownloadRelease");
        }
    }

    private bool _isUpdating = false;
    public bool IsUpdating
    {
        get { return _isUpdating; }
        set {
            _isUpdating = value;
            NotifyPropertyChanged("CanSelectRepo");
            NotifyPropertyChanged("CanSelectRelease");
            NotifyPropertyChanged("CanUpdateRepos");
            NotifyPropertyChanged("CanDownloadRelease");
        }
    }

    private bool _canStep2 = false;
    public bool CanStep2
    {
        get { return _canStep2; }
        set {
            _canStep2 = value;
            NotifyPropertyChanged("CanStep2");
        }
    }

    private bool _showPrereleases = false;
    public bool ShowPrereleases
    {
        get { return _showPrereleases; }
        set {
            _showPrereleases = value;
            NotifyPropertyChanged("ShowPrereleases");
        }
    }

    private Product? _selectedProduct { get; set; }
    public Product? SelectedProduct
    {
        get { return _selectedProduct; }
        set { 
            if(value == null) return;
            _selectedProduct = value;
            NotifyPropertyChanged("SelectedProduct");
            Console.WriteLine("changed");
            CanStep2 = true;
        }
    }

    #endregion

    public MainWindowViewModel()
    {
        string cache = Path.Combine(GetStoragePath(), "cache.json");
        if(File.Exists(cache))
        {
            cache = File.ReadAllText(cache);
            var repos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Repository>>(cache);
            Repos.Clear();
            foreach(Repository repo in repos)
                Repos.Add(repo);
            NotifyPropertyChanged("CanSelectRepo");
        }

        if(Directory.Exists(GetStoragePath()))
        {
            foreach(string folder in Directory.GetDirectories(GetStoragePath()))
            {
                if(!File.Exists(Path.Combine(folder, "cache.json")))
                    continue;
                cache = File.ReadAllText(Path.Combine(folder, "cache.json"));
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<ReleaseContentModel>(cache);
                foreach(Product prod in model.Products)
                    prod.ReleaseContent = model;
                LocalReleases.Add(model);
                //LocalReleases.Sort((a, b) => a.RepositoryName.ComareTo(b.RepositoryName));
            }
        }
    }

    public async Task DownloadRelease()
    {
        System.Console.WriteLine("Downloading Release: " + SelectedRelease?.Name);
        IsDownloading = true;
        string targetPath = "";
        string targetFolder = "";

        try {
            string current = GetStoragePath();
            if(!Directory.Exists(current))
                Directory.CreateDirectory(current);

            targetPath = Path.Combine(current, "download.zip");
            if(File.Exists(targetPath))
                File.Delete(targetPath);
            Console.WriteLine("Save Release in " + targetPath);
            
            await GitHubAccess.DownloadRepo(SelectedRelease.Url, targetPath);
            
            targetFolder = Path.Combine(current, SelectedRelease.Name.Substring(0, SelectedRelease.Name.LastIndexOf('.')));
            if(Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder, true);
                Directory.CreateDirectory(targetFolder);
            }
            System.IO.Compression.ZipFile.ExtractToDirectory(targetPath, targetFolder);

            if(File.Exists(targetPath))
                File.Delete(targetPath);

            ReleaseContentModel content = ReleaseContentHelper.GetReleaseContent(Path.Combine(targetFolder, "data"));
            content.RepositoryName = SelectedRepository.Name;
            content.ReleaseName = SelectedRelease.Name;
            content.IsPrerelease = SelectedRelease.IsPrerelease;
            content.Published = SelectedRelease.Published;
            content.Version = $"v{SelectedRelease.Major}.{SelectedRelease.Minor}.{SelectedRelease.Build}";
            
            File.WriteAllText(Path.Combine(targetFolder, "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(content));
            LocalReleases.Add(content);
            //LocalReleases.Sort((a, b) => a.RepositoryName.ComareTo(b.RepositoryName));
        } catch(Exception ex)
        {
            // TODO handle exception and notify user
        }

        IsDownloading = false;
    }

    public async Task UpdateRepos()
    {
        System.Console.WriteLine("Updating Repos");
        IsUpdating = true;
        try {
            var x = await GitHubAccess.GetOpenKnxRepositoriesAsync(ShowPrereleases);
            Repos.Clear();
            foreach(Repository repo in x)
                Repos.Add(repo);
            NotifyPropertyChanged("CanSelectRepo");
            File.WriteAllText(Path.Combine(GetStoragePath(), "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(Repos));
        } catch(Exception ex)
        {
            System.Console.WriteLine("Failed to update Repos: " + ex.Message);
        }
        IsUpdating = false;
    }

    public async Task DeleteRelease(ReleaseContentModel model)
    {
        System.Console.WriteLine($"Delete Release {model.RepositoryName} {model.ReleaseName}");

        string name = model.ReleaseName;
        name = name.Substring(0, name.LastIndexOf('.'));
        string path = Path.Combine(GetStoragePath(), name);
        Directory.Delete(path, true);
        LocalReleases.Remove(model);
    }

    public async Task CreateKnxProd()
    {
        Console.WriteLine("creating knxprod");

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        string defaultName = SelectedProduct.ReleaseContent.ReleaseName;
        defaultName = defaultName.Substring(0, defaultName.LastIndexOf('.'));

        var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Speichere KnxProd",
            SuggestedFileName = defaultName,
            FileTypeChoices = new[] { new FilePickerFileType("Knx Produkt Datenbank")
            {
                Patterns = new[] { "*.knxprod" }
            }}
        });

        if (file is not null)
        {
            string outpuFolder = Path.Combine(GetStoragePath(), "Temp");
            string xmlFile = SelectedProduct.ReleaseContent.XmlFile;
            string outFile = file.Path.AbsolutePath;
            string workingDir = GetAbsWorkingDir(xmlFile);
            Toolbox.Sign.SignHelper.ExportKnxprod(workingDir, outFile, xmlFile, "", false, false);
        }
    }

    public async Task ImportZip()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Öffne Release Datei",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            // Open reading stream from the first file.
            string outputPath = files[0].Path.AbsolutePath;
            string fileName = Path.GetFileName(outputPath);
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            outputPath = Path.Combine(GetStoragePath(), fileName);
            if(!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            System.IO.Compression.ZipFile.ExtractToDirectory(files[0].Path.AbsolutePath, outputPath);

            Regex regex = new Regex("([0-9]+).([0-9]+).([0-9]+)");
            Match m = regex.Match(fileName);
            int major = 0, minor = 0, build = 0;
            if(m.Success) 
            {
                major = int.Parse(m.Groups[1].Value);
                minor = int.Parse(m.Groups[2].Value);
                build = int.Parse(m.Groups[3].Value);
            } else 
            {
                regex = new Regex("([0-9]+).([0-9]+)");
                m = regex.Match(fileName);
                if(m.Success)
                {
                    major = int.Parse(m.Groups[1].Value);
                    minor = int.Parse(m.Groups[2].Value);
                } else {

                }
            }

            ReleaseContentModel content = ReleaseContentHelper.GetReleaseContent(Path.Combine(outputPath, "data"));
            content.RepositoryName = fileName.Substring(0, fileName.LastIndexOf('-'));;
            content.ReleaseName = Path.GetFileName(files[0].Path.AbsolutePath);
            // content.IsPrerelease = SelectedRelease.IsPrerelease;
            // content.Published = SelectedRelease.Published;
            content.Version = $"v{major}.{minor}.{build}";
            
            File.WriteAllText(Path.Combine(outputPath, "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(content));
            LocalReleases.Add(content);
        }
    }
    
    private static string GetAbsWorkingDir(string iFilename)
    {
        string lResult = Path.GetFullPath(iFilename);
        lResult = Path.GetDirectoryName(lResult);
        return lResult;
    }


    private string GetStoragePath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "Storage");
    }

    public event PropertyChangedEventHandler? PropertyChanged; 
    private void NotifyPropertyChanged(string propertyName = "")  
    {  
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }  
}
