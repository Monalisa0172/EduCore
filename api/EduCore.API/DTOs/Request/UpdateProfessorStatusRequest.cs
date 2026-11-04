using EduCore.API.Enums;

namespace EduCore.API.DTOs.Request;

public class UpdateProfessorStatusRequest
{
    public StatusFuncionarioEnum Status { get; set; }
}