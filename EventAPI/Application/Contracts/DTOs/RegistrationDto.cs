using Domain.Enums;

namespace Application.Contracts.DTOs;


public record RegistrationDto(string Login, string Password, UserRole Role);