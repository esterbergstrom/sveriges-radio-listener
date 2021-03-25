using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SverigesRadioListener.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SverigesRadioListener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://api.sr.se/api/v2/")
            };
            int categoryId = 133;
            IEnumerable<dynamic> programs = new dynamic[0];
            IEnumerable<dynamic> podFiles = new dynamic[0];
            HomeIndexViewModel viewModel = new HomeIndexViewModel()
            {
                Channels = new HomeIndexViewModel.Channel[0]
            };

            // Get category ID
            {
                dynamic response = GetResponse("programcategories/index?format=json");
                if (response != null)
                {
                    IEnumerable<dynamic> categories = (IEnumerable<dynamic>)response.programcategories;
                    dynamic category = categories.FirstOrDefault(x => x.name == "Humor" && x.id != null);
                    if (category != null)
                    {
                        categoryId = category.id;
                    }
                }
            }

            // Get programs
            {
                dynamic response = GetResponse($"programs/index?programcategoryid={categoryId}&isarchived=false&haspod=true&format=json");
                if (response != null)
                {
                    programs = (IEnumerable<dynamic>)response.programs;
                }
            }

            // Get pod files
            {
                List<dynamic> dynamics = new List<dynamic>();
                foreach (dynamic program in programs)
                {
                    dynamic response = GetResponse($"podfiles?programid={program.id}&size=3&format=json");
                    if (response != null)
                    {
                        dynamics.AddRange((IEnumerable<dynamic>)response.podfiles);
                    }
                }
                podFiles = dynamics;
            }

            // Set models
            {
                IEnumerable<string> channelNames = programs.Select(x => (string)x.channel.name).Distinct();
                viewModel.Channels = channelNames.Select(channelName => new HomeIndexViewModel.Channel()
                {
                    Title = channelName,
                    Programs = programs.Where(program => program.channel.name == channelName && podFiles.Any(podFile => podFile.program.id == program.id)).Select(program => new HomeIndexViewModel.Program()
                    {
                        Title = program.name,
                        ImageUrl = program.programimage,
                        Description = program.description,
                        PodFiles = podFiles.Where(podFile => podFile.program.id == program.id).Select(podFile => new HomeIndexViewModel.PodFile()
                        {
                            Title = podFile.title,
                            Date = TimeZoneInfo.ConvertTimeFromUtc((DateTime)podFile.publishdateutc, TimeZoneInfo.Local).ToString("yyyy-MM-dd"),
                            Length = GetLength(podFile),
                            Url = podFile.url
                        })
                    }).OrderBy(x => x.Title)
                }).OrderBy(x => x.Title);
                viewModel.HasPrograms = viewModel.Channels.Where(x => x.Programs.Any()).Any();
            }

            return View(viewModel);

            dynamic GetResponse(string requestUri)
            {
                Task<HttpResponseMessage> messageTask = httpClient.GetAsync(requestUri);
                messageTask.Wait();
                if (messageTask.Result.IsSuccessStatusCode)
                {
                    Task<string> readTask = messageTask.Result.Content.ReadAsStringAsync();
                    readTask.Wait();
                    return JsonConvert.DeserializeObject(readTask.Result);
                }
                else
                {
                    return null;
                }
            }

            string GetLength(dynamic podFile)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds((double)podFile.duration);
                if (timeSpan.Hours > 0)
                {
                    return $"{timeSpan.Hours}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
                }
                else
                {
                    return $"{timeSpan.Minutes}:{timeSpan.Seconds:00}";
                }
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
