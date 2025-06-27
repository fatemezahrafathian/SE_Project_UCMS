using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using UCMS.DTOs;
using UCMS.DTOs.ProjectDto;
using UCMS.Models;

namespace UCMS.IntegrationTests;

public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProject_ReturnsCreatedAndProject_WhenDataIsValid()
    {
        // Arrange
        var formContent = new MultipartFormDataContent();

        formContent.Add(new StringContent("پروژه تست"), "Title");
        formContent.Add(new StringContent("توضیحات پروژه"), "Description");
        formContent.Add(new StringContent("100"), "TotalScore");
        formContent.Add(new StringContent("1"), "ProjectType");
        formContent.Add(new StringContent("5"), "GroupSize");
        formContent.Add(new StringContent("2025-07-01T00:00:00"), "StartDate");
        formContent.Add(new StringContent("2025-07-10T00:00:00"), "EndDate");

        // اگر فایل لازم است می‌توان با ByteArrayContent اضافه کرد، اینجا صرفا بدون فایل است

        int classId = 1; // فرضی، اگر نیاز باشد در DB بساز قبل تست

        // Act
        var response = await _client.PostAsync($"/api/project?classId={classId}", formContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var jsonString = await response.Content.ReadAsStringAsync();
        var serviceResponse = JsonConvert.DeserializeObject<ServiceResponse<GetProjectForInstructorDto>>(jsonString);

        serviceResponse.Should().NotBeNull();
        serviceResponse.Success.Should().BeTrue();
        serviceResponse.Data.Should().NotBeNull();
        serviceResponse.Data.Title.Should().Be("پروژه تست");
        serviceResponse.Data.TotalScore.Should().Be(100);
        serviceResponse.Data.ProjectType.Should().Be((ProjectType)1); // فرض enum مشابه مقدار 1

        // می‌توان Assert های بیشتری بر اساس خواص پروژه اضافه کرد
    }

    [Fact]
    public async Task CreateProject_ReturnsBadRequest_WhenTitleIsMissing()
    {
        // Arrange
        var formContent = new MultipartFormDataContent();

        // عنوان را حذف کردیم تا خطا بگیریم
        formContent.Add(new StringContent(""), "Title");
        formContent.Add(new StringContent("توضیحات پروژه"), "Description");
        formContent.Add(new StringContent("100"), "TotalScore");
        formContent.Add(new StringContent("1"), "ProjectType");
        formContent.Add(new StringContent("5"), "GroupSize");
        formContent.Add(new StringContent("2025-07-01T00:00:00"), "StartDate");
        formContent.Add(new StringContent("2025-07-10T00:00:00"), "EndDate");

        int classId = 1;

        // Act
        var response = await _client.PostAsync($"/api/project?classId={classId}", formContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var jsonString = await response.Content.ReadAsStringAsync();
        jsonString.Should().Contain("message"); // پیام خطا باید برگردد
    }
}