using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;
using System;
using System.Threading;
using Microsoft.Maui.ApplicationModel;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        private readonly IFileSaverService _fileSaverService;

        // Correct geïnitialiseerde collecties
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = new ObservableCollection<GroceryListItem>();
        public ObservableCollection<Product> AvailableProducts { get; set; } = new ObservableCollection<Product>();

        // Gefilterde collectie die aan de tweede CollectionView gebonden wordt
        public ObservableCollection<Product> FilteredProducts { get; set; } = new ObservableCollection<Product>();

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        [ObservableProperty]
        string myMessage;

        // SearchText property (CommunityToolkit genereert setter/getter en roept partial OnSearchTextChanged aan)
        [ObservableProperty]
        private string searchText;

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService, IFileSaverService fileSaverService)
        {
            Title = "Boodschappenlijst (items)";
            _groceryListItemsService = groceryListItemsService;
            _product_service_guard(productService);
            _productService = productService;
            _fileSaverService = fileSaverService;

            // Niet automatisch Load(groceryList.Id) hier omdat GroceryList via QueryProperty gezet wordt.
            // Wanneer GroceryList wordt gezet, wordt partial OnGroceryListChanged aangeroepen.
        }

        // Helper om null-injectie in sommige test-situaties te fangen (optioneel)
        private void _product_service_guard(IProductService productService)
        {
            if (productService == null)
                throw new ArgumentNullException(nameof(productService));
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id))
                MyGroceryListItems.Add(item);

            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();
            foreach (Product p in _productService.GetAll())
            {
                if (MyGroceryListItems.FirstOrDefault(g => g.ProductId == p.Id) == null && p.Stock > 0)
                    AvailableProducts.Add(p);
            }

            // Initialiseer FilteredProducts op basis van huidige zoektekst (null-safe)
            FilterProducts(SearchText);
        }

        // Wanneer QueryProperty GroceryList wordt gezet, roept toolkit deze partial aan.
        partial void OnGroceryListChanged(GroceryList value)
        {
            if (value != null)
            {
                // Load op basis van het nieuwe grocerylist id
                Load(value.Id);
            }
        }

        // Partial methode die door source-generator wordt verwacht wanneer SearchText property verandert.
        partial void OnSearchTextChanged(string value)
        {
            // Live filteren while typing; als je enkel wilt filteren op SearchCommand, verwijder deze regel.
            FilterProducts(value);
        }

        // Dit is het SearchCommand dat SearchBar SearchCommand aanroept.
        // Door deze naam genereert CommunityToolkit een ICommand genaamd SearchCommand.
        [RelayCommand]
        private void Search(string parameter)
        {
            var term = parameter ?? SearchText;
            FilterProducts(term);
        }

        // Core filtering: zoekt in AvailableProducts, case-insensitive op Name (uitbreidbaar)
        private void FilterProducts(string term)
        {
            term = (term ?? string.Empty).Trim();

            var results = string.IsNullOrWhiteSpace(term)
                ? AvailableProducts.ToList()
                : AvailableProducts.Where(p =>
                    !string.IsNullOrEmpty(p.Name) &&
                    p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();

            // Update FilteredProducts (UI-bound). We clear + add zodat UI updates correct plaatsvinden.
            FilteredProducts.Clear();
            foreach (var p in results) FilteredProducts.Add(p);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }

        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null) return;
            GroceryListItem item = new(0, GroceryList.Id, product.Id, 1);
            _groceryListItemsService.Add(item);
            product.Stock--;
            _productService.Update(product);

            // Verwijder uit beschikbare lijst en refresh filter (zodat FilteredProducts ook up-to-date is)
            AvailableProducts.Remove(product);
            FilterProducts(SearchText);

            // Herlaad de boodschappenlijst items
            Load(GroceryList.Id);
        }

        [RelayCommand]
        public async Task ShareGroceryList(CancellationToken cancellationToken)
        {
            if (GroceryList == null || MyGroceryListItems == null) return;
            string jsonString = JsonSerializer.Serialize(MyGroceryListItems);
            try
            {
                await _fileSaverService.SaveFileAsync("Boodschappen.json", jsonString, cancellationToken);
                await Toast.Make("Boodschappenlijst is opgeslagen.").Show(cancellationToken);
            }
            catch (Exception ex)
            {
                await Toast.Make($"Opslaan mislukt: {ex.Message}").Show(cancellationToken);
            }
        }
    }
}