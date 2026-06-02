using Cogfather.HQ.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Cogfather.HQ.Tests;

internal sealed class FakeScopeFactory : IServiceScopeFactory
{
    private readonly INodeRepository _repository;

    public FakeScopeFactory(INodeRepository repository) => _repository = repository;

    public IServiceScope CreateScope() => new FakeScope(_repository);

    private sealed class FakeScope : IServiceScope
    {
        public FakeScope(INodeRepository repository) =>
            ServiceProvider = new FakeServiceProvider(repository);

        public IServiceProvider ServiceProvider { get; }
        public void Dispose() { }
    }

    private sealed class FakeServiceProvider : IServiceProvider
    {
        private readonly INodeRepository _repository;

        public FakeServiceProvider(INodeRepository repository) => _repository = repository;

        public object? GetService(Type serviceType) =>
            serviceType == typeof(INodeRepository) ? _repository : null;
    }
}
