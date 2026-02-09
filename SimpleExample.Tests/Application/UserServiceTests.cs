using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleExample.Application.DTOs;
using SimpleExample.Application.Interfaces;
using SimpleExample.Application.Services;
using SimpleExample.Domain.Entities;
using Xunit;

namespace SimpleExample.Tests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _service = new UserService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        CreateUserDto dto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        // Mock: Email ei ole käytössä
        _mockRepository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        UserDto result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Matti");
        result.LastName.Should().Be("Meikäläinen");
        result.Email.Should().Be("matti@example.com");

        // Varmista että AddAsync kutsuttiin kerran
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        CreateUserDto dto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "existing@example.com"
        };

        User existingUser = new User("Maija", "Virtanen", "existing@example.com");

        // Mock: Email on jo käytössä!
        _mockRepository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*jo olemassa*");

        // Varmista että AddAsync EI kutsuttu
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    // TEHTÄVÄ: Kirjoita itse testit seuraaville:
    // 1. GetByIdAsync - löytyy
    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        User user = new User("Matti", "Meikäläinen", "matti@example.com")
        {
            Id = userId
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _service.GetByIdAsync(userId);

       // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("Matti");
        result.LastName.Should().Be("Meikäläinen");
        result.Email.Should().Be("matti@example.com");

        //Varmistetaan kutsu
        _mockRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
    // 2. GetByIdAsync - ei löydy
    [Fact]
    public async Task GetByIdAsync_WithFalseId_ShouldReturnNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        UserDto? result = await _service.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();

        //Varmistetaan kutsu
        _mockRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
    // 3. GetAllAsync - palauttaa listan
    [Fact]
    public async Task GetAllAsync_WhenUsersExists_ShouldReturnUserList()
    {
        // Arrange
        List<User> users = new List<User>
        {
            new User("Matti", "Meikäläinen", "matti@example.com"),
            new User("Maija", "Virtanen", "maija@example.com"),
            new User("Pekka", "Korhonen", "pekka@example.com")
        };

        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        IEnumerable<UserDto> result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.FirstName == "Matti" && u.LastName == "Meikäläinen" && u.Email == "matti@example.com");
        result.Should().Contain(u =>u.FirstName == "Maija" && u.LastName == "Virtanen" && u.Email == "maija@example.com");

        //Varmistetaan kutsu
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);

    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<User>());
        // Act
        IEnumerable<UserDto> result = await _service.GetAllAsync();
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        //Varmistetaan kutsu
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }
    // 4. UpdateAsync - onnistuu
    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UpdateUserDto updateDto = new UpdateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        User existingUser = new User("Maija", "Virtanen", "maija@example.com");
        existingUser.Id = userId;

        _mockRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        UserDto? result = await _service.UpdateAsync(userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("Matti");
        result.LastName.Should().Be("Meikäläinen");
        result.Email.Should().Be("matti@example.com");

        // Varmistetaan kutsut
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
    // 5. UpdateAsync - käyttäjää ei löydy

    [Fact]
    public async Task UpdateAsync_WithFalseId_ShouldReturnNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UpdateUserDto updateDto = new UpdateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        UserDto? result = await _service.UpdateAsync(userId, updateDto);

        // Assert
        result.Should().BeNull();

        // Varmistetaan kutsut
        _mockRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }


    // 6. DeleteAsync - onnistuu
    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
       
        _mockRepository
            .Setup(x => x.ExistsAsync(userId))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(x => x.DeleteAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.DeleteAsync(userId);

        // Assert
        result.Should().BeTrue();
        
        // Varmistetaan kutsut
        _mockRepository.Verify(x => x.ExistsAsync(userId), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(userId), Times.Once);
    }
   
    // 7. DeleteAsync - käyttäjää ei löydy
    [Fact]
    public async Task DeleteAsync_WithFalseId_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.ExistsAsync(userId))
            .ReturnsAsync(false);
        _mockRepository
            .Setup(x => x.DeleteAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.DeleteAsync(userId);

        // Assert
        result.Should().BeFalse();

        // Varmistetaan kutsut
        _mockRepository.Verify(x => x.ExistsAsync(userId), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(userId), Times.Never);
    }
}