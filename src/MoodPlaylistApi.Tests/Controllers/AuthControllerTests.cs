using Microsoft.AspNetCore.Mvc;
using Moq;
using MoodPlaylistApi.Controllers;
using MoodPlaylistApi.Dtos.Auth;
using MoodPlaylistApi.Interfaces;
using MoodPlaylistApi.Tests.TestSupport;
using MoodPlaylistApi.Utilities;
using System.Net;

namespace MoodPlaylistApi.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact(DisplayName = "Register returns the repository response and status code")]
    public async Task Register_RepositoryReturnsResponse_ReturnsMatchingStatusAndBody()
    {
        var request = new RegisterDto { Name = "Ada", Email = "ada@example.com", Password = "Password1!" };
        var response = ApiResponse<LoginResponseDto>.Success(
            HttpStatusCode.Created,
            data: new LoginResponseDto { Name = "Ada", Token = "token" });
        var repository = new Mock<IAuthRepository>(MockBehavior.Strict);
        repository.Setup(x => x.RegisterAsync(request)).ReturnsAsync(response);
        var controller = new AuthController(ControllerTestContext.CreateUnitOfWork(authRepository: repository).Object);

        var result = await controller.Register(request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);
        repository.Verify(x => x.RegisterAsync(request), Times.Once);
    }

    [Fact(DisplayName = "Login returns the repository response and status code")]
    public async Task Login_RepositoryReturnsResponse_ReturnsMatchingStatusAndBody()
    {
        var request = new LoginDto { Email = "ada@example.com", Password = "Password1!" };
        var response = ApiResponse<LoginResponseDto>.Error(HttpStatusCode.Unauthorized, "Invalid credentials");
        var repository = new Mock<IAuthRepository>(MockBehavior.Strict);
        repository.Setup(x => x.LoginAsync(request)).ReturnsAsync(response);
        var controller = new AuthController(ControllerTestContext.CreateUnitOfWork(authRepository: repository).Object);

        var result = await controller.Login(request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);
        repository.Verify(x => x.LoginAsync(request), Times.Once);
    }

    [Fact(DisplayName = "Logout passes the authenticated user ID to the repository")]
    public async Task Logout_AuthenticatedUser_PassesUserIdAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var response = ApiResponse<string>.Success(HttpStatusCode.OK, data: "Logged out");
        var repository = new Mock<IAuthRepository>(MockBehavior.Strict);
        repository.Setup(x => x.LogoutAsync(userId)).ReturnsAsync(response);
        var controller = new AuthController(ControllerTestContext.CreateUnitOfWork(authRepository: repository).Object);
        ControllerTestContext.Authenticate(controller, userId);

        var result = await controller.Logout();

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);
        repository.Verify(x => x.LogoutAsync(userId), Times.Once);
    }
}
