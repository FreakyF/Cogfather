using System.Security.Cryptography;
using System.Text;
using Cogfather.Node.Application.Interfaces;
using Cogfather.Node.Domain.ValueObjects;

namespace Cogfather.Node.Application.Services;

public class ManifestHashService : IManifestHashService
{
    public string GenerateHash(ProductionManifest manifest)
    {
        var canonicalString = $"{manifest.CorrelationId}:{manifest.ComponentId}:{manifest.Amount}";

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalString));

        var builder = new StringBuilder();
        foreach (var b in bytes) builder.Append(b.ToString("x2"));

        return builder.ToString();
    }
}