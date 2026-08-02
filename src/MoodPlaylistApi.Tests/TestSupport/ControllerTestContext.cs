using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MoodPlaylistApi.Interfaces;
using System.Security.Claims;

namespace MoodPlaylistApi.Tests.TestSupport;

internal static class ControllerTestContext
{
    public static Mock<IUnitOfWork> CreateUnitOfWork(
        Mock<IAuthRepository>? authRepository = null,
        Mock<ILibraryRepository>? libraryRepository = null)
    {
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

        if (authRepository is not null)
            unitOfWork.SetupGet(x => x.AuthRepository).Returns(authRepository.Object);

        if (libraryRepository is not null)
            unitOfWork.SetupGet(x => x.LibraryRepository).Returns(libraryRepository.Object);

        return unitOfWork;
    }

    public static void Authenticate(ControllerBase controller, Guid userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
