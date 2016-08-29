using FileBrowser.Models;
using FileBrowser.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace FileBrowser.Controllers.WebApi
{
    [Route("api/[controller]/[action]")]
    public class FileBrowserController : Controller
    {
        static CancellationTokenSource source = new CancellationTokenSource();

        private readonly ManagingDirectoriesService managingDirectoriesService;

        public FileBrowserController()
        {
            managingDirectoriesService = new ManagingDirectoriesService();
        }

        [HttpGet]
        public DirectoryModel[] GetLogicalDrives()
        {
            return managingDirectoriesService.GetLogicalDrives();
        }

        [HttpPost]
        public DirectoryModel[] GetDirectories([FromBody] string path)
        {
            return managingDirectoriesService.GetDirectories(path);
        }

        [HttpPost]
        public DirectoryModel[] GetFilesInDirectory([FromBody] string path)
        {
            return managingDirectoriesService.GetFilesInDirectory(path);
        }

        [HttpPost]
        public DirectoryModel GoTopDirectory([FromBody] string path)
        {
            return managingDirectoriesService.GoTopDirectory(path);
        }

        [HttpPost]
        public int[] GetAmountOfFiles([FromBody] string path)
        {
            source.Cancel();

            source = new CancellationTokenSource();

            return managingDirectoriesService.GetAmountOfFiles(path, source.Token);
        }
    }
}
