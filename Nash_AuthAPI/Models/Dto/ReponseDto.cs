﻿namespace Nash_AuthAPI.Models.Dto;

public class ReponseDto
{
    public object? Result { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string Message { get; set; } = "";
}
