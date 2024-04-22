using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize]
public class CoursesController(HttpClient httpClient) : Controller
{

    private readonly HttpClient _httpClient = httpClient;

    public async Task<IActionResult> Index()
    {
        var viewModel = new CourseIndexViewModel();

        //hämtar våra kurser
        var response = await _httpClient.GetAsync("https://localhost:7276/api/Courses");
        if (response.IsSuccessStatusCode)
        {
            var courses = JsonConvert.DeserializeObject<IEnumerable<CourseViewModel>>(await response.Content.ReadAsStringAsync());
            if (courses != null && courses.Any())
                viewModel.Courses = courses;

        }

        return View(viewModel);


    }
}
