using Chat.Infrastructure.Models.Dtos;
using Chat.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Services.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly IVectorStoreService _vectorStore;

    public KnowledgeController(IKnowledgeService knowledgeService, IVectorStoreService vectorStore)
    {
        _knowledgeService = knowledgeService;
        _vectorStore = vectorStore;
    }

    [HttpPost("rebuild")]
    public async Task<ActionResult> RebuildKnowledgeBaseAsync(CancellationToken cancellationToken)
    {
        await _knowledgeService.RebuildKnowledgeBaseAsync(cancellationToken);
        return Ok(new { message = "Knowledge base rebuilt successfully" });
    }

    [HttpPost("index-lawyers")]
    public async Task<ActionResult> IndexLawyersAsync(CancellationToken cancellationToken)
    {
        await _knowledgeService.IndexLawyersDataAsync(cancellationToken);
        return Ok(new { message = "Lawyers data indexed successfully" });
    }

    [HttpPost("index-users")]
    public async Task<ActionResult> IndexUsersAsync(CancellationToken cancellationToken)
    {
        await _knowledgeService.IndexUsersDataAsync(cancellationToken);
        return Ok(new { message = "Users data indexed successfully" });
    }

    [HttpPost("index-appointments")]
    public async Task<ActionResult> IndexAppointmentsAsync(CancellationToken cancellationToken)
    {
        await _knowledgeService.IndexAppointmentsDataAsync(cancellationToken);
        return Ok(new { message = "Appointments data indexed successfully" });
    }

    [HttpPost("index-services-practice-areas")]
    public async Task<ActionResult> IndexServicesAndPracticeAreasAsync(CancellationToken cancellationToken)
    {
        await _knowledgeService.IndexServicesAndPracticeAreasAsync(cancellationToken);
        return Ok(new { message = "Services & practice areas indexed successfully" });
    }

    [HttpPost("index-legal-documents")]
    public async Task<ActionResult> IndexLegalDocumentsAsync(CancellationToken cancellationToken)
    {
        await _knowledgeService.IndexLegalDocumentsAsync(cancellationToken);
        return Ok(new { message = "Legal documents indexed successfully" });
    }

    [HttpPost("index-custom")]
    public async Task<ActionResult> IndexCustomDataAsync([FromBody] CustomDataRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required");
        }

        await _knowledgeService.IndexCustomDataAsync(request.Content, request.Source, request.Metadata, cancellationToken);
        return Ok(new { message = "Custom data indexed successfully" });
    }

    [HttpPost("search")]
    public async Task<ActionResult<SearchResponseDto>> SearchAsync([FromBody] SearchRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest("Query is required");
        }

        var results = await _vectorStore.SearchSimilarAsync(request.Query, request.TopK ?? 5, cancellationToken);
        
        var response = new SearchResponseDto
        {
            Query = request.Query,
            Results = results.Select(r => new SearchResultDto
            {
                Id = r.Id,
                Content = r.Content,
                Score = r.Score,
                Metadata = r.Metadata
            }).ToList()
        };

        return Ok(response);
    }

    [HttpDelete("clear")]
    public async Task<ActionResult> ClearAllAsync(CancellationToken cancellationToken)
    {
        await _vectorStore.ClearAllAsync(cancellationToken);
        return Ok(new { message = "Knowledge base cleared successfully" });
    }
}

public class CustomDataRequest
{
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class SearchRequestDto
{
    public string Query { get; set; } = string.Empty;
    public int? TopK { get; set; }
}

public class SearchResponseDto
{
    public string Query { get; set; } = string.Empty;
    public List<SearchResultDto> Results { get; set; } = new();
}

public class SearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
