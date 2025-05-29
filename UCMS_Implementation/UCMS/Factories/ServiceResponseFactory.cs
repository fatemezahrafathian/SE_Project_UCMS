using UCMS.DTOs;

namespace UCMS.Factories;

public class ServiceResponseFactory
{
    public static ServiceResponse<T> Failure<T>(string message) => new() { Success = false, Message = message };
    public static ServiceResponse<T> Failure<T>(T data, string message) => new() { Success = false, Data = data, Message = message };
    public static ServiceResponse<T> Success<T>(string message) => new() { Success = true, Message = message };
    public static ServiceResponse<T> Success<T>(T data, string message) => new() { Success = true, Data = data, Message = message };
}