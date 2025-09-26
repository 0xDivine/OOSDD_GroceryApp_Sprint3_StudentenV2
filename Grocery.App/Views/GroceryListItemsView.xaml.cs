using Grocery.App.ViewModels;
using Microsoft.Maui.Controls;

namespace Grocery.App.Views;

public partial class GroceryListItemsView : ContentPage
{
    public GroceryListItemsView(GroceryListItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Voor platforms waar SearchButtonPressed nodig is: trigger SearchCommand handmatig
    private void OnSearchButtonPressed(object sender, EventArgs e)
    {
        if (BindingContext is GroceryListItemsViewModel vm)
        {
            if (vm.SearchCommand.CanExecute(vm.SearchText))
                vm.SearchCommand.Execute(vm.SearchText);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GroceryListItemsViewModel vm)
        {
            vm.OnAppearing();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is GroceryListItemsViewModel vm)
        {
            vm.OnDisappearing();
        }
    }
}