using AdvancedRepositories.Core.Repositories;
using AdvancedRepositories.Core.Tests.IntegrationTests.TestClasses;
using FluentAssertions;

namespace AdvancedRepositories.Core.Tests.IntegrationTests;

public class AdvancedRepositoryTests
{
    private TestRepository TestRepository;

    public AdvancedRepositoryTests()
    {
        TestRepository = new TestRepository(DbContext.GetBaseDbConfig());
    }

    [Fact]
    public void GetList_ByColumnAttributes_Success()
    {
        // Given
        List<AttributedClass> expected = new()
        {
            new(){ Id = 1, Name = "Victor", Age = 0, RegistrationDate = new DateTime() },
            new(){ Id = 2, Name = "Alejandro", Age = 0, RegistrationDate = new DateTime() },
            new(){ Id = 3, Name = null, Age = 0, RegistrationDate = new DateTime() },
            new(){ Id = 4, Name = "Asuaje", Age = 0, RegistrationDate = new DateTime() },
        };

        // When 
        DbResult<List<AttributedClass>> result = TestRepository.GetList_ByColumnAttributes();

        // Then
        result.ExceptionContent.Should().Be(null);
        result.IsSuccess.Should().Be(true);
        result.Value.Should().HaveCount(expected.Count);
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetList_ByPropertyNames_Success()
    {
        // Given
        List<AttributedClass> expected = new()
        {
            new(){ Id = 1, Name = "Victor", Age = 26, RegistrationDate = Convert.ToDateTime("2023-01-01 00:00:00") },
            new(){ Id = 2, Name = "Alejandro", Age = 27, RegistrationDate = Convert.ToDateTime("2023-01-02 00:00:00") },
            new(){ Id = 3, Name = null, Age = 28, RegistrationDate = Convert.ToDateTime("2023-01-03 00:00:00") },
            new(){ Id = 4, Name = "Asuaje", Age = 29, RegistrationDate = Convert.ToDateTime("2023-01-04 00:00:00") },
        };

        // When 
        DbResult<List<AttributedClass>> result = TestRepository.GetList_ByPropertyNames();

        // Then
        result.ExceptionContent.Should().Be(null);
        result.IsSuccess.Should().Be(true);
        result.Value.Should().HaveCount(expected.Count);
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetList_ByMappingFields_Success()
    {
        // Given
        List<AttributedClass> expected = new()
        {
            new(){ Id = 1, Name = "Victor", Age = 0, RegistrationDate = Convert.ToDateTime("2023-01-01 00:00:00") },
            new(){ Id = 2, Name = "Alejandro", Age = 0, RegistrationDate = Convert.ToDateTime("2023-01-02 00:00:00") },
            new(){ Id = 3, Name = null, Age = 0, RegistrationDate = Convert.ToDateTime("2023-01-03 00:00:00") },
            new(){ Id = 4, Name = "Asuaje", Age = 0, RegistrationDate = Convert.ToDateTime("2023-01-04 00:00:00") },
        };

        // When 
        DbResult<List<AttributedClass>> result = TestRepository.GetList_ByMappingFields();

        // Then
        result.ExceptionContent.Should().Be(null);
        result.IsSuccess.Should().Be(true);
        result.Value.Should().HaveCount(expected.Count);
        result.Value.Should().BeEquivalentTo(expected);
    }
}
