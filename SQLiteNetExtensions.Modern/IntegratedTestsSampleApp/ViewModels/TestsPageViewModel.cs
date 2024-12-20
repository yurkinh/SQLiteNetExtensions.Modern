using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntegratedTestsSampleApp.Tests;

namespace IntegratedTestsSampleApp.ViewModels;

public partial class TestsPageViewModel : ObservableObject
{
    [ObservableProperty]
    bool isLoading;

    [ObservableProperty]
    string? deleteTestsResult = "Press to run test";

    [ObservableProperty]
    string? manyToManyResult = "Press to run test";

    [ObservableProperty]
    string? manyToOneResult = "Press to run test";

    [ObservableProperty]
    string? oneToManyResult = "Press to run test";

    [ObservableProperty]
    string? oneToOneTestsResult = "Press to run test";

    [ObservableProperty]
    string? recursiveReadResult = "Press to run test";

    [ObservableProperty]
    string? recursiveWriteResult = "Press to run test";

    [ObservableProperty]
    string? asyncRecursiveReadResult = "Press to run test";

    [ObservableProperty]
    string? asyncRecursiveWriteResult = "Press to run test";

    [RelayCommand]
    private async Task RunAllTestsAsync()
    {
        try
        {
            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));

            await Task.Run(() =>
            {
                Delete();
                ManyToMany();
                ManyToOne();
                OneToMany();
                OneToOne();
                RecursiveRead();
                RecursiveWrite();
            });
        }
        catch (Exception ex)
        {
            DeleteTestsResult = $"Error occurred: {ex.Message}";
            ManyToManyResult = $"Error occurred: {ex.Message}";
            OneToManyResult = $"Error occurred: {ex.Message}";
            OneToOneTestsResult = $"Error occurred: {ex.Message}";
            RecursiveReadResult = $"Error occurred: {ex.Message}";
            RecursiveWriteResult = $"Error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    [RelayCommand]
    private async Task RunAllAsyncTestsAsync()
    {
        try
        {
            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));
            
            await DeleteAsync();
                ManyToMany();
                ManyToOne();
                OneToMany();
                OneToOne();
                RecursiveRead();
                RecursiveWrite();
            
            await AsyncRecursiveRead();
            //await AsyncRecursiveWrite();
        }
        catch (Exception ex)
        {
            AsyncRecursiveReadResult = $"Error occurred: {ex.Message}";
            AsyncRecursiveWriteResult = $"Error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    [RelayCommand]
    private void ManyToMany()
    {
        try
        {
            var testResults = new[]
            {
                ManyToManyTests.TestGetManyToManyList(),
                ManyToManyTests.TestGetManyToManyArray(),
                ManyToManyTests.TestUpdateSetManyToManyList(),
                ManyToManyTests.TestUpdateUnsetManyToManyList(),
                ManyToManyTests.TestGetManyToManyGuidIdentifier(),
                ManyToManyTests.TestManyToManyCircular(),
                ManyToManyTests.TestManyToManyCircularReadOnly(),
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                ManyToManyResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                ManyToManyResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            ManyToManyResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ManyToOne()
    {
        try
        {
            var testResults = new[]
            {
                ManyToOneTests.TestGetManyToOne(),
                ManyToOneTests.TestUpdateSetManyToOne(),
                ManyToOneTests.TestUpdateUnsetManyToOne()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                ManyToOneResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                ManyToOneResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            ManyToOneResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OneToMany()
    {
        try
        {
            var testResults = new[]
            {
                OneToManyTests.TestGetOneToManyList(),
                OneToManyTests.TestGetOneToManyListWithInverse(),
                OneToManyTests.TestGetOneToManyArray(),
                OneToManyTests.TestGetOneToManyArray(),
                OneToManyTests.TestUpdateSetOneToManyList(),
                OneToManyTests.TestUpdateUnsetOneToManyEmptyList(),
                OneToManyTests.TestUpdateUnsetOneToManyNullList(),
                OneToManyTests.TestUpdateSetOneToManyArray(),
                OneToManyTests.TestUpdateSetOneToManyListWithInverse(),
                OneToManyTests.TestGetOneToManyListWithInverseGuidId(),
                OneToManyTests.TestUpdateSetOneToManyListWithInverseGuidId(),

                // Tests the recursive inverse relationship automatic discovery
                // Issue #17: https://bitbucket.org/twincoders/sqlite-net-extensions/issue/17
                //OneToManyTests.TestRecursiveInverseRelationship()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                OneToManyResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                OneToManyResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            OneToManyResult = $"Error occurred: {ex.Message}";
        }
    }

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
    private void Delete()
    {
        try
        {
            var testResults = new[]
            {
                DeleteTests.TestDeleteAllGuidPK(),
                DeleteTests.TestDeleteAllIntPK(),
                DeleteTests.TestDeleteAllThousandObjects(),
                DeleteTests.TestDeleteAllIdsGuidPK(),
                DeleteTests.TestDeleteAllIdsIntPK()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                DeleteTestsResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                DeleteTestsResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            DeleteTestsResult = $"Error occurred: {ex.Message}";
        }
    }

     [RelayCommand]
    private Task DeleteAsync()
    {
        try
        {
            var testResults = new[]
            {
                DeleteTests.TestDeleteAllGuidPK(),
                DeleteTests.TestDeleteAllIntPK(),
                DeleteTests.TestDeleteAllThousandObjects(),
                DeleteTests.TestDeleteAllIdsGuidPK(),
                DeleteTests.TestDeleteAllIdsIntPK()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                DeleteTestsResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                DeleteTestsResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            DeleteTestsResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RecursiveRead()
    {
        try
        {
            var testResults = new[]
            {
                RecursiveReadTests.TestOneToOneCascadeWithInverse(),
                RecursiveReadTests.TestOneToOneCascadeWithInverseReversed(),
                RecursiveReadTests.TestOneToOneCascadeWithInverseDoubleForeignKey(),
                RecursiveReadTests.TestOneToOneCascadeWithInverseDoubleForeignKeyReversed(),
                RecursiveReadTests.TestOneToManyCascadeWithInverse(),
                RecursiveReadTests.TestManyToOneCascadeWithInverse(),
                RecursiveReadTests.TestManyToManyCascadeWithSameClassRelationship(),
                RecursiveReadTests.TestInsertTextBlobPropertiesRecursive()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                RecursiveReadResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                RecursiveReadResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            RecursiveReadResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RecursiveWrite()
    {
        try
        {
            var testResults = new[]
            {
                RecursiveWriteTests.TestOneToOneRecursiveInsert(),
                RecursiveWriteTests.TestOneToOneRecursiveInsertOrReplace(),
                RecursiveWriteTests.TestOneToOneRecursiveInsertGuid(),
                RecursiveWriteTests.TestOneToOneRecursiveInsertOrReplaceGuid(),
                RecursiveWriteTests.TestOneToManyRecursiveInsert(),
                RecursiveWriteTests.TestOneToManyRecursiveInsertOrReplace(),
                RecursiveWriteTests.TestOneToManyRecursiveInsertGuid(),
                RecursiveWriteTests.TestOneToManyRecursiveInsertOrReplaceGuid(),
                RecursiveWriteTests.TestManyToOneRecursiveInsert(),
                RecursiveWriteTests.TestManyToOneRecursiveInsertOrReplace(),
                RecursiveWriteTests.TestManyToOneRecursiveInsertGuid(),
                RecursiveWriteTests.TestManyToOneRecursiveInsertOrReplaceGuid(),
                RecursiveWriteTests.TestManyToManyRecursiveInsertWithSameClassRelationship(),
                RecursiveWriteTests.TestManyToManyRecursiveDeleteWithSameClassRelationship(),
                RecursiveWriteTests.TestInsertTextBlobPropertiesRecursive()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                RecursiveWriteResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                RecursiveWriteResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            RecursiveWriteResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AsyncRecursiveRead()
    {
        try
        {
            var testResults = await Task.WhenAll(
            [
                //RecursiveReadAsyncTests.TestOneToOneCascadeWithInverseAsync(),
                //RecursiveReadAsyncTests.TestOneToOneCascadeWithInverseReversedAsync(),
                RecursiveReadAsyncTests.TestOneToOneCascadeWithInverseDoubleForeignKeyAsync(),
                RecursiveReadAsyncTests.TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync(),
                RecursiveReadAsyncTests.TestOneToManyCascadeWithInverseAsync(),
                RecursiveReadAsyncTests.TestManyToOneCascadeWithInverseAsync(),
                //RecursiveReadAsyncTests.TestManyToManyCascadeWithSameClassRelationshipAsync(),
                //RecursiveReadAsyncTests.TestInsertTextBlobPropertiesRecursiveAsync()
            ]);

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count != 0)
            {
                AsyncRecursiveReadResult = $"Tests failed: {failedTests.Count}.\nFailed tests:\n{string.Join(",\n", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                AsyncRecursiveReadResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            AsyncRecursiveReadResult = $"Error occurred: {ex.Message}";
        }
    }


    [RelayCommand]
    private async Task AsyncRecursiveWrite()
    {
        try
        {
            var testResults = await Task.WhenAll(
            [
                RecursiveWriteAsyncTests.TestOneToOneRecursiveInsertAsync(),
                RecursiveWriteAsyncTests.TestOneToOneRecursiveInsertOrReplaceAsync(),
                RecursiveWriteAsyncTests.TestOneToOneRecursiveInsertGuidAsync(),
                RecursiveWriteAsyncTests.TestOneToOneRecursiveInsertOrReplaceGuidAsync(),
                RecursiveWriteAsyncTests.TestOneToManyRecursiveInsertAsync(),
                RecursiveWriteAsyncTests.TestOneToManyRecursiveInsertOrReplaceAsync(),
                RecursiveWriteAsyncTests.TestOneToManyRecursiveInsertGuidAsync(),
                RecursiveWriteAsyncTests.TestOneToManyRecursiveInsertOrReplaceGuidAsync(),
                RecursiveWriteAsyncTests.TestManyToOneRecursiveInsertAsync(),
                RecursiveWriteAsyncTests.TestManyToOneRecursiveInsertOrReplaceAsync(),
                RecursiveWriteAsyncTests.TestManyToOneRecursiveInsertGuidAsync(),
                RecursiveWriteAsyncTests.TestManyToOneRecursiveInsertOrReplaceGuidAsync(),
                RecursiveWriteAsyncTests.TestManyToManyRecursiveInsertWithSameClassRelationshipAsync(),
                RecursiveWriteAsyncTests.TestManyToManyRecursiveDeleteWithSameClassRelationshipAsync(),
                RecursiveWriteAsyncTests.TestInsertTextBlobPropertiesRecursiveAsync(),
            ]);

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count != 0)
            {
                AsyncRecursiveWriteResult = $"Tests failed: {failedTests.Count}.\nFailed tests:\n{string.Join(",\n", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                AsyncRecursiveWriteResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            AsyncRecursiveWriteResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ResetTestResults()
    {
        DeleteTestsResult = "Press to run test";
        ManyToManyResult = "Press to run test";
        ManyToOneResult = "Press to run test";
        OneToManyResult = "Press to run test";
        OneToOneTestsResult = "Press to run test";
        RecursiveReadResult = "Press to run test";
        RecursiveWriteResult = "Press to run test";
        AsyncRecursiveReadResult = "Press to run test";
        AsyncRecursiveWriteResult = "Press to run test";
    }

}