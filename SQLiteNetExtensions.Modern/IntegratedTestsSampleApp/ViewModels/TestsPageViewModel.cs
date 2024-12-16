using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntegratedTestsSampleApp.Tests;

namespace IntegratedTestsSampleApp.ViewModels;

public partial class TestsPageViewModel : ObservableObject
{

    [ObservableProperty]
    string? oneToOneTestsResult;

    [RelayCommand]
    private void DeleteTests() => ExecuteAction("DeleteTests.cs");

    [RelayCommand]
    private void ManyToManyTests() => ExecuteAction("ManyToManyTests.cs");

    [RelayCommand]
    private void ManyToOneTests() => ExecuteAction("ManyToOneTests.cs");

    [RelayCommand]
    private void OneToManyTests() => ExecuteAction("OneToManyTests.cs");

    [RelayCommand]
    private void OneToOne()
    {
        try
        {
            var testResults = new[]
            {
                OneToOneTests.TestGetOneToOneDirect(),
                OneToOneTests.TestGetOneToOneInverseForeignKey(),
                OneToOneTests.TestGetOneToOneWithInverseRelationship(),
                OneToOneTests.TestGetInverseOneToOneRelationshipWithExplicitKey(),
                OneToOneTests.TestUpdateSetOneToOneRelationship(),
                OneToOneTests.TestUpdateUnsetOneToOneRelationship(),
                OneToOneTests.TestUpdateSetOneToOneRelationshipWithInverse(),
                OneToOneTests.TestUpdateSetOneToOneRelationshipWithInverseForeignKey(),
                OneToOneTests.TestUpdateUnsetOneToOneRelationshipWithInverseForeignKey(),
                OneToOneTests.TestGetAllNoFilter(),
                OneToOneTests.TestGetAllFilter()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                OneToOneTestsResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                OneToOneTestsResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            OneToOneTestsResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RecursiveReadTests() => ExecuteAction("RecursiveReadTests.cs");

    [RelayCommand]
    private void RecursiveWriteTests() => ExecuteAction("RecursiveWriteTests.cs");

    private void ExecuteAction(string fileName)
    {
        Console.WriteLine($"Executing: {fileName}");
    }
}