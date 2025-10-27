using Chat.Application.Services;
using Chat.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Chat.Services.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
	private readonly IChatService _chatService;
	private readonly IRetrievalService _retrievalService;

	public ChatController(IChatService chatService, IRetrievalService retrievalService)
	{
		_chatService = chatService;
		_retrievalService = retrievalService;
	}

    [HttpPost]
    [SwaggerOperation(
        Summary = "Kết nối với Chat")]
    public async Task<ActionResult<ChatResponseDto>> PostAsync([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.Message))
		{
			return BadRequest("message is required");
		}

		var knowledge = await _retrievalService.BuildContextAsync(request.UserId, cancellationToken);
		var response = await _chatService.GenerateAnswerAsync(request.Message, knowledge, cancellationToken);
		return Ok(response);
	}
}


