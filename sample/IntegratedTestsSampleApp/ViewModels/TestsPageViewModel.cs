using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntegratedTestsSampleApp.Tests;

namespace IntegratedTestsSampleApp.ViewModels;

public partial class TestsPageViewModel : ObservableObject
{
    [ObservableProperty]
    bool isLoading;

    [ObservableProperty]
    bool isAsyncLoading;

    [ObservableProperty]
    string? manyToManyResult = "Press to run test";

    [ObservableProperty]
    string? manyToOneResult = "Press to run test";

    [ObservableProperty]
    string? oneToManyResult = "Press to run test";

    [ObservableProperty]
    string? oneToOneTestsResult = "Press to run test";

    [ObservableProperty]
    string? deleteTestsResult = "Press to run test";

    [ObservableProperty]
    string? recursiveReadResult = "Press to run test";

    [ObservableProperty]
    string? recursiveWriteResult = "Press to run test";


    [ObservableProperty]
    string? manyToManyAsyncResult = "Press to run test";

    [ObservableProperty]
    string? manyToOneAsyncResult = "Press to run test";

    [ObservableProperty]
    string? oneToManyAsyncResult = "Press to run test";

    [ObservableProperty]
    string? oneToOneTestsAsyncResult = "Press to run test";

    [ObservableProperty]
    string? deleteTestsAsyncResult = "Press to run test";

    [ObservableProperty]
    string? asyncRecursiveReadResult = "Press to run test";

    [ObservableProperty]
    string? asyncRecursiveWriteResult = "Press to run test";

    [RelayCommand]
    private async Task RunAllSyncTestsAsync()
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
            IsAsyncLoading = true;
            OnPropertyChanged(nameof(IsLoading));

            await AsyncDelete();
            await AsyncManyToMany();
            await AsyncManyToOne();
            await AsyncOneToMany();
            await AsyncOneToOne();
            await AsyncRecursiveRead();
            await AsyncRecursiveWrite();
        }
        catch (Exception ex)
        {
            AsyncRecursiveReadResult = $"Error occurred: {ex.Message}";
            AsyncRecursiveWriteResult = $"Error occurred: {ex.Message}";
        }
        finally
        {
            IsAsyncLoading = false;
            OnPropertyChanged(nameof(IsAsyncLoading));
        }
    }
    #region Sync
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
    #endregion

    #region Async

    [RelayCommand]
    private async Task AsyncManyToMany()
    {
        try
        {
            var testResults = new List<Tuple<bool, string>>
            {
                await ManyToManyTestsAsync.TestGetManyToManyListAsync(),
                await ManyToManyTestsAsync.TestGetManyToManyArrayAsync(),
                await ManyToManyTestsAsync.TestUpdateSetManyToManyListAsync(),
                await ManyToManyTestsAsync.TestUpdateUnsetManyToManyListAsync(),
                await ManyToManyTestsAsync.TestGetManyToManyGuidIdentifierAsync(),
                await ManyToManyTestsAsync.TestManyToManyCircularAsync(),
                await ManyToManyTestsAsync.TestManyToManyCircularReadOnlyAsync(),
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                ManyToManyAsyncResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                ManyToManyAsyncResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            ManyToManyAsyncResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AsyncManyToOne()
    {
        try
        {
            var testResults = new List<Tuple<bool, string>>
            {
                await ManyToOneTestsAsync.TestGetManyToOneAsync(),
                await ManyToOneTestsAsync.TestUpdateSetManyToOneAsync(),
                await ManyToOneTestsAsync.TestUpdateUnsetManyToOneAsync()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                ManyToOneAsyncResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                ManyToOneAsyncResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            ManyToOneAsyncResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AsyncOneToMany()
    {
        try
        {
            var testResults = new List<Tuple<bool, string>>
            {
                await OneToManyTestsAsync.TestGetOneToManyListAsync(),
                await OneToManyTestsAsync.TestGetOneToManyListWithInverseAsync(),
                await OneToManyTestsAsync.TestGetOneToManyArrayAsync(),
                await OneToManyTestsAsync.TestGetOneToManyArrayAsync(),
                await OneToManyTestsAsync.TestUpdateSetOneToManyListAsync(),
                await OneToManyTestsAsync.TestUpdateUnsetOneToManyEmptyListAsync(),
                await OneToManyTestsAsync.TestUpdateUnsetOneToManyNullListAsync(),
                await OneToManyTestsAsync.TestUpdateSetOneToManyArrayAsync(),
                await OneToManyTestsAsync.TestUpdateSetOneToManyListWithInverseAsync(),
                await OneToManyTestsAsync.TestGetOneToManyListWithInverseGuidIdAsync(),
                await OneToManyTestsAsync.TestUpdateSetOneToManyListWithInverseGuidIdAsync(),

                // Tests the recursive inverse relationship automatic discovery
                // Issue #17: https://bitbucket.org/twincoders/sqlite-net-extensions/issue/17
                //await OneToManyTestsAsync.TestRecursiveInverseRelationshipAsync()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                OneToManyAsyncResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                OneToManyAsyncResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            OneToManyAsyncResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AsyncOneToOne()
    {
        try
        {
            var testResults = new List<Tuple<bool, string>>
            {
                await OneToOneTestsAsync.TestGetOneToOneDirectAsync(),
                await OneToOneTestsAsync.TestGetOneToOneInverseForeignKeyAsync(),
                await OneToOneTestsAsync.TestGetOneToOneWithInverseRelationshipAsync(),
                await OneToOneTestsAsync.TestGetInverseOneToOneRelationshipWithExplicitKeyAsync(),
                await OneToOneTestsAsync.TestUpdateSetOneToOneRelationshipAsync(),
                await OneToOneTestsAsync.TestUpdateUnsetOneToOneRelationshipAsync(),
                await OneToOneTestsAsync.TestUpdateSetOneToOneRelationshipWithInverseAsync(),
                await OneToOneTestsAsync.TestUpdateSetOneToOneRelationshipWithInverseForeignKeyAsync(),
                await OneToOneTestsAsync.TestUpdateUnsetOneToOneRelationshipWithInverseForeignKeyAsync(),
                await OneToOneTestsAsync.TestGetAllNoFilterAsync(),
                await OneToOneTestsAsync.TestGetAllFilterAsync()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                OneToOneTestsAsyncResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                OneToOneTestsAsyncResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            OneToOneTestsAsyncResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AsyncDelete()
    {
        try
        {
            var testResults = new List<Tuple<bool, string>>
            {
                await DeleteTestsAsunc.TestDeleteAllGuidPKAsync(),
                await DeleteTestsAsunc.TestDeleteAllIntPKAsync(),
                await DeleteTestsAsunc.TestDeleteAllThousandObjectsAsync(),
                await DeleteTestsAsunc.TestDeleteAllIdsGuidPKAsync(),
                await DeleteTestsAsunc.TestDeleteAllIdsIntPKAsync()
            };

            var failedTests = testResults.Where(result => !result.Item1).ToList();
            if (failedTests.Count() != 0)
            {
                DeleteTestsAsyncResult = $"Tests failed: {failedTests.Count}. \n Failed tests: \n {string.Join(",\n ", failedTests.Select(x => x.Item2))}.";
            }
            else
            {
                DeleteTestsAsyncResult = "All tests passed successfully!";
            }
        }
        catch (Exception ex)
        {
            DeleteTestsAsyncResult = $"Error occurred: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AsyncRecursiveRead()
    {
        try
        {
            var testResults = new List<Tuple<bool, string>>
            {
               await RecursiveReadAsyncTests.TestOneToOneCascadeWithInverseAsync(),
               await RecursiveReadAsyncTests.TestOneToOneCascadeWithInverseReversedAsync(),
               await RecursiveReadAsyncTests.TestOneToOneCascadeWithInverseDoubleForeignKeyAsync(),
               await RecursiveReadAsyncTests.TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync(),
               await RecursiveReadAsyncTests.TestOneToManyCascadeWithInverseAsync(),
               await RecursiveReadAsyncTests.TestManyToOneCascadeWithInverseAsync(),
               await RecursiveReadAsyncTests.TestManyToManyCascadeWithSameClassRelationshipAsync(),
               await RecursiveReadAsyncTests.TestInsertTextBlobPropertiesRecursiveAsync()
            };

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
            var testResults = new List<Tuple<bool, string>>
            {
               await RecursiveWriteAsyncTests.TestOneToOneRecursiveInsertAsync(),
               await RecursiveWriteAsyncTests.TestOneToOneRecursiveInsertOrReplaceAsync(),
               await RecursiveWriteAsyncTests.TestOneToOneRecursiveInsertGuidAsync(),
               await RecursiveWriteAsyncTests.TestOneToOneRecursiveInsertOrReplaceGuidAsync(),
               await RecursiveWriteAsyncTests.TestOneToManyRecursiveInsertAsync(),
               await RecursiveWriteAsyncTests.TestOneToManyRecursiveInsertOrReplaceAsync(),
               await RecursiveWriteAsyncTests.TestOneToManyRecursiveInsertGuidAsync(),
               await RecursiveWriteAsyncTests.TestOneToManyRecursiveInsertOrReplaceGuidAsync(),
               await RecursiveWriteAsyncTests.TestManyToOneRecursiveInsertAsync(),
               await RecursiveWriteAsyncTests.TestManyToOneRecursiveInsertOrReplaceAsync(),
               await RecursiveWriteAsyncTests.TestManyToOneRecursiveInsertGuidAsync(),
               await RecursiveWriteAsyncTests.TestManyToOneRecursiveInsertOrReplaceGuidAsync(),
               await RecursiveWriteAsyncTests.TestManyToManyRecursiveInsertWithSameClassRelationshipAsync(),
               await RecursiveWriteAsyncTests.TestManyToManyRecursiveDeleteWithSameClassRelationshipAsync(),
               await RecursiveWriteAsyncTests.TestInsertTextBlobPropertiesRecursiveAsync(),
            };

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
    #endregion

    [RelayCommand]
    private void ResetTestResults()
    {
        ManyToManyResult = "Press to run test";
        ManyToOneResult = "Press to run test";
        OneToManyResult = "Press to run test";
        OneToOneTestsResult = "Press to run test";
        DeleteTestsResult = "Press to run test";
        RecursiveReadResult = "Press to run test";
        RecursiveWriteResult = "Press to run test";
    }

    [RelayCommand]
    private void ResetAsyncTestResults()
    {
        ManyToManyAsyncResult = "Press to run test";
        ManyToOneAsyncResult = "Press to run test";
        OneToManyAsyncResult = "Press to run test";
        OneToOneTestsAsyncResult = "Press to run test";
        DeleteTestsAsyncResult = "Press to run test";
        AsyncRecursiveReadResult = "Press to run test";
        AsyncRecursiveWriteResult = "Press to run test";
    }
}