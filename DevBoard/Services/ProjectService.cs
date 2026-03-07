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
    public partial class ProjectService
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

                if (repo.EndsWith(".git"))
                {
                    repo = repo.Substring(0, repo.Length - 4);
                }

                // Construct raw GitHub URL
                var rawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/main/{project.ConfigPath}";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(rawUrl);
                    response.EnsureSuccessStatusCode();

                    var jsonContent = await response.Content.ReadAsStringAsync();
                    
                    List<ModuleDto> parsedModules = null;
                    try 
                    {
                        // Try parsing as array first (new format)
                        parsedModules = JsonConvert.DeserializeObject<List<ModuleDto>>(jsonContent);
                    }
                    catch
                    {
                        // Fallback to object format (old format)
                        var config = JsonConvert.DeserializeObject<DevBoardConfig>(jsonContent);
                        parsedModules = config?.Modules;
                    }

                    if (parsedModules == null)
                        throw new InvalidOperationException("Invalid config format");

                    // Get existing modules for this project
                    var existingModules = _context.Modules.Include(m => m.Categories).Where(m => m.ProjectId == projectId).ToList();

                    // Upsert modules
                    foreach (var moduleDto in parsedModules)
                    {
                        var existingModule = existingModules.FirstOrDefault(m => (!string.IsNullOrEmpty(moduleDto.Id) && m.ExtId == moduleDto.Id) || m.Name == moduleDto.Name);
                        if (existingModule != null)
                        {
                            existingModule.Name = moduleDto.Name;
                            existingModule.ExtId = moduleDto.Id;
                            existingModule.Description = moduleDto.Description;
                            existingModule.Path = moduleDto.Path ?? existingModule.Path;
                            existingModule.IsCritical = moduleDto.IsCritical;
                        }
                        else
                        {
                            existingModule = new Module
                            {
                                ProjectId = projectId,
                                ExtId = moduleDto.Id,
                                Name = moduleDto.Name,
                                Description = moduleDto.Description,
                                Path = moduleDto.Path,
                                IsCritical = moduleDto.IsCritical
                            };
                            _context.Modules.Add(existingModule);
                        }

                        // Upsert Categories
                        if (moduleDto.Categories != null)
                        {
                            foreach (var catDto in moduleDto.Categories)
                            {
                                var existingCat = existingModule.Categories?.FirstOrDefault(c => c.ExtId == catDto.Id || c.Name == catDto.Name);
                                if (existingCat != null)
                                {
                                    existingCat.Name = catDto.Name;
                                    existingCat.ExtId = catDto.Id;
                                    existingCat.SeverityMultiplier = catDto.SeverityMultiplier;
                                    existingCat.BaseScore = catDto.BaseScore;
                                }
                                else
                                {
                                    var newCat = new Category
                                    {
                                        ExtId = catDto.Id,
                                        Name = catDto.Name,
                                        SeverityMultiplier = catDto.SeverityMultiplier,
                                        BaseScore = catDto.BaseScore,
                                        StressScore = 0.0m,
                                        Module = existingModule
                                    };
                                    if (existingModule.Categories == null) existingModule.Categories = new List<Category>();
                                    existingModule.Categories.Add(newCat);
                                }
                            }
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
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("is_critical")]
        public bool IsCritical { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("categories")]
        public List<CategoryDto> Categories { get; set; }
    }

    public class CategoryDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("severity_multiplier")]
        public decimal SeverityMultiplier { get; set; }
        [JsonProperty("base_score")]
        public decimal BaseScore { get; set; }
    }

    public class GitHubIssueDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("labels")]
        public List<string> Labels { get; set; }
    }

    public partial class ProjectService
    {
        public async Task<string> CreateGitHubIssueAsync(int projectId, Ticket ticket)
        {
            var project = _context.Projects.Find(projectId);
            if (project == null || string.IsNullOrEmpty(project.RepoUrl))
                throw new InvalidOperationException("Project not found or RepoUrl is empty");

            var token = System.Web.Configuration.WebConfigurationManager.AppSettings["GitHubPersonalAccessToken"];
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("GitHub Personal Access Token is not configured.");

            try
            {
                var uri = new Uri(project.RepoUrl);
                var pathParts = uri.AbsolutePath.Trim('/').Split('/');
                if (pathParts.Length < 2) throw new InvalidOperationException("Invalid GitHub URL");

                var owner = pathParts[0];
                var repo = pathParts[1].EndsWith(".git") ? pathParts[1].Substring(0, pathParts[1].Length - 4) : pathParts[1];

                var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/issues";

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("DevBoard", "1.0"));
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var issue = new GitHubIssueDto
                    {
                        Title = ticket.Title,
                        Body = $"{ticket.Description}\n\n**Type:** {ticket.Type}\n**Priority:** {ticket.Priority}\n**Created By:** {ticket.CreatedById}\n\n*Created via DevBoard*",
                        Labels = new List<string> { "DevBoard", ticket.Type.ToString(), ticket.Priority.ToString() }
                    };

                    var json = JsonConvert.SerializeObject(issue);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(apiUrl, content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"GitHub API Error: {response.StatusCode} - {responseBody}");
                    }

                    // dynamic result = JsonConvert.DeserializeObject(responseBody);
                    // return result.html_url;
                    return "Issue created successfully"; 
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create GitHub issue: {ex.Message}", ex);
            }
        }
    }
}
