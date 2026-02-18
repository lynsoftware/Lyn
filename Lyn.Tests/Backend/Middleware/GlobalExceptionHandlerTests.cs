using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Lyn.Backend.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyn.Tests.Backend.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _sut;
    private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;

    public GlobalExceptionHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _sut = new GlobalExceptionHandler(_mockLogger.Object);
    }
    
    // ================= Arrange - Setter opp HttpContext og metode for å deserializere ProblemDetails =================
    
    /// <summary>
    /// Vi oppretter en HttpContext (en http-forespørsel)
    /// </summary>
    /// <returns></returns>
    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        // Vi lagerer resultatet fra GlobalExceptionHandleren, som da sendes i body, med MemoryStream
        context.Response.Body = new MemoryStream();
        // Test-endepunkt
        context.Request.Path = "/test";
        return context;
    }
    
    /// <summary>
    /// Spoler tilbake Response.Body som er et MemoryStream-objekt og deserialiserer dette til et C#-objekt
    /// </summary>
    /// <param name="context">Http-forespørselen med svaret fra GlobalExceptionHandleren</param>
    /// <returns>Et problem-details objekt</returns>
    private static async Task<ProblemDetails?> ReadProblemDetails(HttpContext context)
    {
        // Spoler MemoryStream til starten, som en video-kasett
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        // Deserialiserer alle egenskapene til et C#-objekt
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);
    }
    
    
    // ==================== Tester at GlobalExceptionHandler returnerer korrekt øsnket oppførsel ====================
    
    /// <summary>
    /// Tester at GlobalExceptionHandler alltid håndterer feilen
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_AlwaysReturnsTrue()
    {
        var context = CreateHttpContext();

        var result = await _sut.TryHandleAsync(context, new Exception(), CancellationToken.None);

        Assert.True(result);
    }
    
    /// <summary>
    /// Tester at TraceId er med 
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_IncludesTraceId()
    {
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-123"; // Lager en TraceIdentifier

        await _sut.TryHandleAsync(context, new Exception(), CancellationToken.None);

        var problem = await ReadProblemDetails(context);
        Assert.Equal("test-trace-123", problem!.Extensions["traceId"]!.ToString());
        
    }
    
    /// <summary>
    /// Tester at endepunktet som er kalt er med og korrekt (altså Request.Path)
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_IncludesRequestPath()
    {
        var context = CreateHttpContext();
        context.Request.Path = "/api/password"; // Setter endepunktet sin Path

        await _sut.TryHandleAsync(context, new Exception(), CancellationToken.None);

        var problem = await ReadProblemDetails(context);
        Assert.Equal("/api/password", problem!.Instance);
    }
    
    /// <summary>
    /// Tester at ProblemTypeUri settes korrekt
    /// </summary>
    /// <param name="exceptionType">Feilen vi kaster</param>
    /// <param name="expectedUri">Forventet URI</param>
    [Theory]
    [InlineData(typeof(ArgumentException), "https://tools.ietf.org/html/rfc9110#section-15.5.1")]
    [InlineData(typeof(KeyNotFoundException), "https://tools.ietf.org/html/rfc9110#section-15.5.5")]
    [InlineData(typeof(Exception), "https://tools.ietf.org/html/rfc9110#section-15.6.1")]
    public async Task TryHandleAsync_SetsCorrectProblemTypeUri(Type exceptionType, string expectedUri)
    {
        var context = CreateHttpContext();
        // Oppretter en exception av ønsket type i sanntid for InlineData kan ikke ta en exception
        var exception = (Exception)Activator.CreateInstance(exceptionType)!; 
        
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        var problem = await ReadProblemDetails(context);
        Assert.Equal(expectedUri, problem!.Type);
    }
    
     // ==================== Tester exceptionsene med GLobalExceptionHandler ====================
    
    /// <summary>
    /// Tester ArgumentException fra GlobalExceptionHandler
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_ArgumentException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext(); // Oppretter Http-forespørselen
        var exception = new ArgumentException("Invalid input"); // Ønsket Exception å teste
        
        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);
        
        // Assert
        Assert.True(result); // Bekrefter at GlboalExceptionHandler har håndtert feilen
        Assert.Equal(400, context.Response.StatusCode); // Bekrefter 400 statuskode
        var problem = await ReadProblemDetails(context);
        Assert.Equal("Bad Request", problem!.Title); // Sjekker at Title er Bad Request
        Assert.Equal("Invalid input", problem.Detail); // Sjekker at strengen stemmer med exception-strengen fra Arrange
    }
    
    /// <summary>
    /// Tester KeyNotFoundException fra GlobalExceptionHandler
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404()
    {
        // Arrange
        var context = CreateHttpContext(); // Oppretter Http-forespørselen
        var exception = new KeyNotFoundException("Resource not found"); // Ønsket Exception å teste
        
        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);
        
        // Assert
        Assert.True(result); // Bekrefter at GlboalExceptionHandler har håndtert feilen
        Assert.Equal(404, context.Response.StatusCode); // Bekrefter 404 statuskode
        var problem = await ReadProblemDetails(context);
        Assert.Equal("Not Found", problem!.Title); // Sjekker at Title er Bad Request
        Assert.Equal("Resource not found", problem.Detail); // Sjekker at Details stemmer
    }
    
    /// <summary>
    /// Tester UnauthorizedAccessException fra GlobalExceptionHandler
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns403()
    {
        // Arrange
        var context = CreateHttpContext(); // Oppretter Http-forespørselen
        var exception = new UnauthorizedAccessException("Access denied");
    
        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);
    
        // Assert
        Assert.True(result); // Bekrefter at GlobalExceptionHandler har håndtert feilen
        Assert.Equal(403, context.Response.StatusCode); // Bekrefter 403 statuskode
        var problem = await ReadProblemDetails(context);
        Assert.Equal("Forbidden", problem!.Title); // Sjekker at Title er Forbidden
        Assert.Equal("Access denied", problem.Detail); // Sjekker at Details stemmer
    }
    
    /// <summary>
    /// Tester ValidationException, fra DataAnnotations, fra GlobalExceptionHandler
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext(); // Oppretter Http-forespørselen
        var exception = new ValidationException("Validation failed");
    
        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);
    
        // Assert
        Assert.True(result); // Bekrefter at GlobalExceptionHandler har håndtert feilen
        Assert.Equal(400, context.Response.StatusCode); // Bekrefter 400 statuskode
        var problem = await ReadProblemDetails(context);
        Assert.Equal("Validation Error", problem!.Title); // Sjekker at Title er Validation Error
        Assert.Equal("Validation failed", problem.Detail); // Sjekker at Details stemmer
    }

    /// <summary>
    /// Tester DbUpdateException, fra EFCore, fra GlobalExceptionHandler
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_DbUpdateException_Returns409()
    {
        // Arrange
        var context = CreateHttpContext(); // Oppretter Http-forespørselen
        var exception = new DbUpdateException("Database error"); 
    
        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);
    
        // Assert
        Assert.True(result); // Bekrefter at GlobalExceptionHandler har håndtert feilen
        Assert.Equal(409, context.Response.StatusCode); // Bekrefter 409 statuskode
        var problem = await ReadProblemDetails(context);
        Assert.Equal("Database Conflict", problem!.Title); // Sjekker at Title er Database Conflict
        Assert.Equal("A conflict occurred while saving data. The resource may have been modified or deleted.", 
            problem.Detail); // Sjekker at hardkodet melding brukes
    }
    
    /// <summary>
    /// Tester default ( _ i switchen), med InvalidOperationException, fra GlobalExceptionHandler
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_UnexpectedException_Returns500()
    {
        // Arrange
        var context = CreateHttpContext(); // Oppretter Http-forespørselen
        var exception = new InvalidOperationException("Something broke"); // Ønsket Exception å teste
        
        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);
        
        // Assert
        Assert.True(result); // Bekrefter at GlboalExceptionHandler har håndtert feilen
        Assert.Equal(500, context.Response.StatusCode); // Bekrefter 500 statuskode
        var problem = await ReadProblemDetails(context);
        Assert.Equal("Server Error", problem!.Title);
        Assert.Equal("An unexpected error occurred.", problem.Detail);
    }
    
    /// <summary>
    /// Tester default ( _ i switchen), med default Exception, fra GlobalExceptionHandler
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500()
    {
        // Arrange
        var context = CreateHttpContext(); // Oppretter Http-forespørselen
        var exception = new Exception("Something broke"); // Ønsket Exception å teste
        
        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);
        
        // Assert
        Assert.True(result); // Bekrefter at GlboalExceptionHandler har håndtert feilen
        Assert.Equal(500, context.Response.StatusCode); // Bekrefter 500 statuskode
        var problem = await ReadProblemDetails(context);
        Assert.Equal("Server Error", problem!.Title);
        Assert.Equal("An unexpected error occurred.", problem.Detail);
    }
}