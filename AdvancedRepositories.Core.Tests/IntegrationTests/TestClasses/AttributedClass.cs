using FluentRepositories.Attributes;

namespace AdvancedRepositories.Core.Tests.IntegrationTests.TestClasses;

[DefaultTable("TestTable")]
public class AttributedClass
{
    [DatabaseColumn("Id")]
    public long Id { get; set; }

    [DatabaseColumn("Name")]
    public string? Name { get; set; }
    public long Age { get; set; }
    public DateTime RegistrationDate { get; set; }
}
