using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace tracksByPopularity.controllers;

/// <summary>
/// Health check controller for monitoring application status.
/// Provides endpoints for Docker health checks and monitoring systems.
/// </summary>
[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for recording health check activities.</param>
    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint.
    /// Returns 200 OK if the application is running.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> with status 200 OK and a simple health status message.
    /// </returns>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogDebug("Health check endpoint called");
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

