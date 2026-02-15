using DevBoard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevBoard.Services
{
    public class ProjectService
    {
        private readonly DevBoardContext _context;

        public ProjectService(DevBoardContext context)
        {
            _context = context;
        }

        public List<Project> GetAllProjects()
        {
            return _context.Projects.Include(p => p.Modules).ToList();
        }

        public Project GetProjectById(int id)
        {
            return _context.Projects.Include(p => p.Modules).FirstOrDefault(p => p.Id == id);
        }

        public void CreateProject(Project project)
        {
            _context.Projects.Add(project);
            _context.SaveChanges();
        }

        public void UpdateProject(Project project)
        {
            _context.Entry(project).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteProject(int id)
        {
            var project = _context.Projects.Find(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                _context.SaveChanges();
            }
        }

        public async Task SyncModulesFromGitHubAsync(int projectId)
        {
            var project = _context.Projects.Find(projectId);
            if (project == null || string.IsNullOrEmpty(project.RepoUrl))
                throw new InvalidOperationException("Project not found or RepoUrl is empty");

            try
            {
                // Parse GitHub URL to extract owner and repo
                var uri = new Uri(project.RepoUrl);
                var pathParts = uri.AbsolutePath.Trim('/').Split('/');
                if (pathParts.Length < 2)
                    throw new InvalidOperationException("Invalid GitHub URL format");

                var owner = pathParts[0];
                var repo = pathParts[1];

                // Construct raw GitHub URL
                var rawUrl = string.Format("https://raw.githubusercontent.com/{0}/{1}/main/{2}", owner, repo, project.ConfigPath);

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(rawUrl);
                    response.EnsureSuccessStatusCode();

                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var config = JsonConvert.DeserializeObject<DevBoardConfig>(jsonContent);

                    if (config?.Modules == null)
                        throw new InvalidOperationException("Invalid config format");

                    // Get existing modules for this project
                    var existingModules = _context.Modules.Where(m => m.ProjectId == projectId).ToList();

                    // Upsert modules
                    foreach (var moduleDto in config.Modules)
                    {
                        var existingModule = existingModules.FirstOrDefault(m => m.Name == moduleDto.Name);
                        if (existingModule != null)
                        {
                            existingModule.Path = moduleDto.Path;
                        }
                        else
                        {
                            _context.Modules.Add(new Module
                            {
                                ProjectId = projectId,
                                Name = moduleDto.Name,
                                Path = moduleDto.Path
                            });
                        }
                    }

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Failed to sync modules from GitHub: {0}", ex.Message), ex);
            }
        }
    }

    public class DevBoardConfig
    {
        public string Project { get; set; }
        public List<ModuleDto> Modules { get; set; }
    }

    public class ModuleDto
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
