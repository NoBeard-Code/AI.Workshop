using Microsoft.Extensions.AI;
using System.Threading.RateLimiting;

namespace AI.Workshop.ConsoleChat.OpenAI;

/// <summary>
/// The DelegatingEmbeddingGenerator<TInput,TEmbedding> class is an implementation of the 
/// IEmbeddingGenerator<TInput, TEmbedding> interface that serves as a base class for creating embedding generators 
/// that delegate their operations to another IEmbeddingGenerator<TInput, TEmbedding> instance. 
/// It allows for chaining multiple generators in any order, passing calls through to an underlying generator. 
/// The class provides default implementations for methods such as GenerateAsync and Dispose, 
/// which forward the calls to the inner generator instance, enabling flexible and modular embedding generation.
/// </summary>
/// <param name="innerGenerator"></param>
/// <param name="rateLimiter"></param>
internal class RateLimitingEmbeddingGenerator(
    IEmbeddingGenerator<string, Embedding<float>> innerGenerator, RateLimiter rateLimiter)
        : DelegatingEmbeddingGenerator<string, Embedding<float>>(innerGenerator)
{
    public override async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var lease = await rateLimiter.AcquireAsync(permitCount: 1, cancellationToken)
            .ConfigureAwait(false);

        if (!lease.IsAcquired)
        {
            throw new InvalidOperationException("Unable to acquire lease.");
        }

        return await base.GenerateAsync(values, options, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            rateLimiter.Dispose();
        }

        base.Dispose(disposing);
    }
}
