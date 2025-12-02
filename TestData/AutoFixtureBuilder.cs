using AutoFixture;

public class AutoFixtureBuilder
{
    private readonly IFixture _fixture;
    public AutoFixtureBuilder()
    {
        _fixture = new Fixture();
    }

    public T Create<T>()
    {
        return _fixture.Create<T>();
    }

    public IFixture GetFixture => _fixture;
}